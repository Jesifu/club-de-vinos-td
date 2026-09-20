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
    }
}
