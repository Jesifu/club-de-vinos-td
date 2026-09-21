using BE;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace BLL
{
    public class CajaMensualBLL
    {
        private readonly DAL.CajaMensualDAL _dal = new DAL.CajaMensualDAL();
        private readonly DAL.VinoDAL _vinoDal = new DAL.VinoDAL();
        private readonly DAL.SocioDAL _socioDal = new DAL.SocioDAL();
        private readonly DAL.SustitucionDAL _sustitucionDal = new DAL.SustitucionDAL();
        private readonly BitacoraBLL _bitacora = new BitacoraBLL();

        // RN-06 filtra en la SP (WHERE), RN-05.2 ordena en la SP (ORDER BY PREFERIDO DESC) — la BLL no repite ninguna de las dos.
        public List<VinoCandidato> ListarCandidatos(int socioId)
        {
            return _vinoDal.ListarCandidatos(socioId);
        }

        // Decision 7: Σ Cantidad × PrecioSnapshot.
        public decimal CalcularTotal(IEnumerable<CajaVino> lineas)
        {
            return lineas.Sum(l => l.Cantidad * l.PrecioSnapshot);
        }

        public List<CajaMensual> ListarArmadas()
        {
            return _dal.ListarPorEstado("Armada");
        }

        // CAJA_LISTAR_POR_ESTADO ordena ORDER BY FECHA_ARMADO DESC — para un
        // historial de despachos se re-ordena en memoria por FechaDespacho.
        public List<CajaMensual> ListarDespachadas()
        {
            return _dal.ListarPorEstado("Despachada")
                       .OrderByDescending(c => c.FechaDespacho)
                       .ToList();
        }

        public int Armar(CajaMensual caja, USUARIO sesion)
        {
            if (caja.Lineas == null || caja.Lineas.Count == 0)
                throw new InvalidOperationException("La caja debe tener al menos un vino.");

            Socio socio = _socioDal.ObtenerPorId(caja.SocioId);
            if (socio == null)
                throw new InvalidOperationException("El socio no existe.");

            // RN-06: revalidación por línea contra los candidatos vigentes — defensa en
            // profundidad. La UI ya filtra con ListarCandidatos, pero la BLL no confía
            // ciegamente en lo que llega desde la capa de presentación.
            List<VinoCandidato> candidatos = ListarCandidatos(caja.SocioId);
            foreach (CajaVino linea in caja.Lineas)
            {
                VinoCandidato candidato = candidatos.FirstOrDefault(c => c.Id == linea.VinoId);
                if (candidato == null)
                    throw new InvalidOperationException("Uno de los vinos seleccionados ya no está disponible para este socio.");

                // RN-04: la caja congela nombre y precio al momento del armado.
                linea.NombreSnapshot = candidato.Nombre;
                linea.PrecioSnapshot = candidato.Precio;
            }

            // RN-05.1: el total no puede superar el presupuesto mensual del socio.
            decimal total = CalcularTotal(caja.Lineas);
            if (total > socio.PresupuestoMensual)
                throw new InvalidOperationException("El total de la caja supera el presupuesto mensual del socio.");

            caja.PresupuestoSnapshot = socio.PresupuestoMensual;
            caja.Estado = EstadoCaja.Armada;
            caja.ArmadoPor = sesion.Id;

            try
            {
                int cajaId = _dal.Armar(caja); // tx en el DAL (C2)
                _bitacora.RegistrarAccion(sesion.Usuario, "ARMADO_CAJA:" + caja.Periodo);
                return cajaId;
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                // El índice filtrado UQ_CAJA_SOCIO_PERIODO es la autoridad (Decision 8).
                throw new InvalidOperationException("El socio ya tiene una caja armada para ese período.");
            }
        }

        // Header + composición efectiva (RN-07) en un solo objeto para las grillas de
        // picking/despacho. La resolución de la cadena de sustituciones vive en la SP
        // (CAJA_VINO_LISTAR_EFECTIVO), acá solo se combinan los dos resultados.
        public CajaMensual ObtenerConComposicionEfectiva(int cajaId)
        {
            CajaMensual caja = _dal.ObtenerPorId(cajaId);
            if (caja == null)
                throw new InvalidOperationException("La caja no existe.");

            caja.Lineas = _dal.ListarComposicionEfectiva(cajaId);
            return caja;
        }

        // Traza completa de sustituciones de una caja (dgvTrazaSustituciones en
        // frmPickingCaja) — wrapper delgado sobre SustitucionDAL, mismo criterio que
        // ListarCandidatos sobre VinoDAL. No está en la tabla de firmas del design
        // porque esa tabla no enumeró la grilla de traza; se agrega acá como
        // consecuencia directa de la tabla de controles de frmPickingCaja (Deviation).
        public List<Sustitucion> ListarTrazaSustituciones(int cajaId)
        {
            return _sustitucionDal.ListarPorCaja(cajaId);
        }

        // RN-10 caja.ArmadoPor == sesion.Id -> throw (ANTES del DAL); RN-08 caja.Estado
        // != Armada -> throw; RN-06 sobre el reemplazo; RN-05 recheck del total efectivo
        // contra el PresupuestoSnapshot congelado al armado.
        //
        // Deviation: se agrega @cajaId como parámetro (el design solo listaba
        // cajaVinoId/vinoReemplazoId/motivo/sesion). Es necesario para resolver caja.Estado
        // y caja.ArmadoPor sin agregar una 19ª SP (el contrato de 18 SPs quedó fijado y
        // verificado en Slice 1, Reconciliation note 1) — el dato ya está disponible en la
        // UI porque CajaVino.CajaId viaja en cada fila de ObtenerConComposicionEfectiva.
        // SUSTITUCION_INSERTAR conserva su propio backstop RN-08/RN-10 en el WHERE,
        // independiente de este chequeo en memoria (triple guard).
        public void RegistrarSustitucion(int cajaId, int cajaVinoId, int vinoReemplazoId, string motivo, USUARIO sesion)
        {
            CajaMensual caja = _dal.ObtenerPorId(cajaId);
            if (caja == null)
                throw new InvalidOperationException("La caja no existe.");

            if (caja.ArmadoPor == sesion.Id)
                throw new InvalidOperationException("Quien armó la caja no puede registrar sustituciones sobre ella.");

            if (caja.Estado != EstadoCaja.Armada)
                throw new InvalidOperationException("Solo se pueden registrar sustituciones sobre una caja Armada.");

            List<CajaVino> efectiva = _dal.ListarComposicionEfectiva(cajaId);
            CajaVino linea = efectiva.FirstOrDefault(l => l.Id == cajaVinoId);
            if (linea == null)
                throw new InvalidOperationException("La línea seleccionada no pertenece a esta caja.");

            // RN-06: el reemplazo debe ser un candidato vigente para el socio.
            List<VinoCandidato> candidatos = ListarCandidatos(caja.SocioId);
            VinoCandidato reemplazo = candidatos.FirstOrDefault(c => c.Id == vinoReemplazoId);
            if (reemplazo == null)
                throw new InvalidOperationException("El vino de reemplazo no está disponible para este socio.");

            // RN-05: recheck del total efectivo (con el reemplazo aplicado) contra el
            // presupuesto congelado al armado — no el presupuesto actual del socio.
            decimal totalConReemplazo = efectiva
                .Where(l => l.Id != cajaVinoId)
                .Sum(l => l.Cantidad * l.PrecioEfectivo)
                + linea.Cantidad * reemplazo.Precio;
            if (totalConReemplazo > caja.PresupuestoSnapshot)
                throw new InvalidOperationException("El reemplazo supera el presupuesto de la caja.");

            Sustitucion s = new Sustitucion
            {
                CajaVinoId = cajaVinoId,
                VinoOriginalId = linea.VinoEfectivoId, // último eslabón de la cadena (RN-07), no necesariamente el original de CAJA_VINO
                VinoReemplazoId = vinoReemplazoId,
                NombreSnapshot = reemplazo.Nombre,
                PrecioSnapshot = reemplazo.Precio,
                Motivo = motivo,
                ResponsableId = sesion.Id
            };

            int filas = _sustitucionDal.Insertar(s); // SUSTITUCION_INSERTAR: backstop RN-08/RN-10 propio (C4)
            if (filas == 0)
                throw new InvalidOperationException("No se pudo registrar la sustitución (la caja pudo haber cambiado de estado).");

            _bitacora.RegistrarAccion(sesion.Usuario, "SUSTITUCION_CAJA:" + cajaId);
        }

        // RN-08/RN-10 en memoria; RN-03 pre-check amable por línea antes de tocar la SP
        // (que igual lo re-verifica de forma transaccional y atómica, C5).
        public void Despachar(int cajaId, USUARIO sesion)
        {
            CajaMensual caja = _dal.ObtenerPorId(cajaId);
            if (caja == null)
                throw new InvalidOperationException("La caja no existe.");

            if (caja.Estado != EstadoCaja.Armada)
                throw new InvalidOperationException("Solo se puede despachar una caja Armada.");

            if (caja.ArmadoPor == sesion.Id)
                throw new InvalidOperationException("Quien armó la caja no puede despacharla.");

            List<CajaVino> efectiva = _dal.ListarComposicionEfectiva(cajaId);
            CajaVino sinStock = efectiva.FirstOrDefault(l => l.StockDisponible < l.Cantidad);
            if (sinStock != null)
                throw new InvalidOperationException("No hay stock suficiente de \"" + sinStock.NombreEfectivo + "\" para despachar la caja.");

            int filas = _dal.Despachar(cajaId, sesion.Id, sesion.Usuario); // CAJA_DESPACHAR: tx en la SP (C5)
            if (filas == 0)
                throw new InvalidOperationException("No se pudo despachar la caja (verificar estado o permisos).");

            _bitacora.RegistrarAccion(sesion.Usuario, "DESPACHO_CAJA:" + cajaId);
        }

        // RN-08 terminal: solo una caja Armada puede cancelarse. Sin form dedicado
        // (decisión de producto, design) — se llama por código o desde una consola de
        // soporte. Completa Reconciliation note 5 del artefacto de tasks: este método no
        // existía hasta esta slice.
        public void Cancelar(int cajaId, string motivo, USUARIO sesion)
        {
            CajaMensual caja = _dal.ObtenerPorId(cajaId);
            if (caja == null)
                throw new InvalidOperationException("La caja no existe.");

            if (caja.Estado != EstadoCaja.Armada)
                throw new InvalidOperationException("Solo se puede cancelar una caja Armada.");

            int filas = _dal.Cancelar(cajaId, motivo, sesion.Id);
            if (filas == 0)
                throw new InvalidOperationException("No se pudo cancelar la caja.");

            _bitacora.RegistrarAccion(sesion.Usuario, "CANCELACION_CAJA:" + cajaId);
        }
    }
}
