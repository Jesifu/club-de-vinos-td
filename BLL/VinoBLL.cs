using BE;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace BLL
{
    public class VinoBLL
    {
        private const int MotivoMaxLongitud = 200;

        private readonly DAL.VinoDAL _dal = new DAL.VinoDAL();
        private readonly DAL.SolicitudAutorizacionDAL _solicitudDal = new DAL.SolicitudAutorizacionDAL();
        private readonly DAL.BodegaDAL _bodegaDal = new DAL.BodegaDAL();
        private readonly DAL.MovimientoStockDAL _movimientoDal = new DAL.MovimientoStockDAL();
        private readonly BitacoraBLL _bitacora = new BitacoraBLL();

        public List<Bodega> ListarBodegas()
        {
            return _bodegaDal.ListarHabilitadas();
        }

        // Validaciones de datos compartidas por la propuesta de alta y la re-propuesta (el SKU solo
        // se exige en la alta: al reproponer se conserva el del vino rechazado).
        private void ValidarDatos(Vino v, bool exigirCodigo)
        {
            if (exigirCodigo && string.IsNullOrWhiteSpace(v.Codigo))
                throw new InvalidOperationException("El SKU es obligatorio.");
            if (string.IsNullOrWhiteSpace(v.Nombre))
                throw new InvalidOperationException("El nombre es obligatorio.");
            if (v.BodegaId <= 0)
                throw new InvalidOperationException("Debe seleccionar una bodega válida.");
            if (string.IsNullOrWhiteSpace(v.Varietal))
                throw new InvalidOperationException("El varietal es obligatorio.");
            if (v.Aniada < 1900 || v.Aniada > DateTime.Now.Year + 1)
                throw new InvalidOperationException("El año de añada no es válido.");
            if (v.Precio <= 0)
                throw new InvalidOperationException("El precio debe ser mayor a cero.");
            if (v.StockMinimo < 0)
                throw new InvalidOperationException("El stock mínimo no puede ser negativo.");
            if (v.Puntaje.HasValue && (v.Puntaje.Value < 0 || v.Puntaje.Value > 100))
                throw new InvalidOperationException("El puntaje debe estar entre 0 y 100.");
        }

        public void ProponerAlta(Vino v, USUARIO sesion)
        {
            ValidarDatos(v, true);

            Vino existente = _dal.ObtenerPorCodigo(v.Codigo);
            if (existente != null)
            {
                // REQ-ALTA-06: un SKU rechazado no se reutiliza con un alta nueva, se repropone.
                if (existente.Estado == EstadoVino.Rechazado)
                    throw new InvalidOperationException(
                        "El SKU indicado corresponde a un vino rechazado. Seleccione ese vino en el catálogo y use 'Corregir y reproponer'.");
                throw new InvalidOperationException("Ya existe un vino con el SKU indicado.");
            }

            v.CreadoPor = sesion.Id;
            // El vino nace Pendiente; la SP crea en la misma transacción su solicitud ALTA Pendiente.
            v.Estado = EstadoVino.Pendiente;

            try
            {
                _dal.Insertar(v);
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                // Violación de FK_VINO_BODEGA: bypass de UI con un BodegaId inexistente.
                throw new InvalidOperationException("Debe seleccionar una bodega válida.");
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                // Carrera contra otro alta con el mismo SKU (UQ_VINO_CODIGO).
                throw new InvalidOperationException("Ya existe un vino con el SKU indicado.");
            }

            _bitacora.RegistrarAccion(sesion.Usuario, "ALTA_VINO:" + v.Codigo);
        }

        // R2: corrige y repropone un vino Rechazado con el mismo SKU. El vino vuelve a Pendiente y se
        // emite una solicitud ALTA nueva (la anterior queda Rechazada). El emisor es quien repropone (RN-01).
        public void Reproponer(Vino v, USUARIO sesion)
        {
            ValidarDatos(v, false);

            Vino actual = _dal.ObtenerPorId(v.Id);
            if (actual == null)
                throw new InvalidOperationException("El vino indicado no existe.");
            if (actual.Estado != EstadoVino.Rechazado)
                throw new InvalidOperationException("Solo se puede reproponer un vino rechazado.");
            if (_solicitudDal.ObtenerPendiente(actual.Id, TipoSolicitud.Alta) != null)
                throw new InvalidOperationException("El vino ya tiene una solicitud de alta pendiente de autorización.");

            int filas;
            try
            {
                filas = _dal.Reproponer(v, sesion.Id);
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                // Violación de FK_VINO_BODEGA: bypass de UI con un BodegaId inexistente.
                throw new InvalidOperationException("Debe seleccionar una bodega válida.");
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                // Carrera contra otra re-propuesta: lo frena el índice único UX_SOLAUT_PENDIENTE.
                throw new InvalidOperationException("El vino ya tiene una solicitud de alta pendiente de autorización.");
            }
            if (filas == 0)
                throw new InvalidOperationException("No se pudo reproponer: el vino ya no está rechazado. Actualice e intente nuevamente.");

            _bitacora.RegistrarAccion(sesion.Usuario, "REPROPUESTA_ALTA_VINO:" + actual.Codigo);
        }

        // Catálogo de Compras: activos más los propios pendientes o rechazados, con su última solicitud (R7).
        public List<Vino> ListarCatalogo(USUARIO sesion)
        {
            return _dal.ListarCatalogo(sesion.Id);
        }

        public List<Vino> ListarAutorizados()
        {
            return _dal.ListarAutorizados();
        }

        public List<SolicitudAutorizacion> ListarSolicitudesPendientes(TipoSolicitud tipo)
        {
            return _solicitudDal.ListarPendientes(tipo);
        }

        // CU-22: se resuelve la solicitud exacta que leyó el administrador, no "el vino".
        public void AutorizarAlta(int solicitudId, USUARIO sesion)
        {
            SolicitudAutorizacion s = ObtenerSolicitudPendiente(solicitudId, TipoSolicitud.Alta);
            // RN-01, capa BLL: guard en memoria, ANTES de llamar al DAL.
            ValidarSeparacionDeFunciones(s, sesion, "autorizar");

            // RN-01, backstop SP: WHERE EMISOR_ID <> @resolutor. 0 filas = rechazado por la BD.
            int filas = _solicitudDal.AprobarAlta(s.Id, sesion.Id);
            if (filas == 0)
                throw new InvalidOperationException(
                    "No se pudo autorizar el alta: la solicitud cambió de estado o usted la emitió. Actualice e intente nuevamente.");

            _bitacora.RegistrarAccion(sesion.Usuario, "AUTORIZACION_VINO:" + s.VinoCodigo);
        }

        // R6: el rechazo exige motivo (UI, BLL y SP). El vino queda Rechazado.
        public void RechazarAlta(int solicitudId, string motivo, USUARIO sesion)
        {
            string motivoLimpio = ValidarMotivo(motivo);
            SolicitudAutorizacion s = ObtenerSolicitudPendiente(solicitudId, TipoSolicitud.Alta);
            ValidarSeparacionDeFunciones(s, sesion, "rechazar");

            int filas = _solicitudDal.RechazarAlta(s.Id, sesion.Id, motivoLimpio);
            if (filas == 0)
                throw new InvalidOperationException(
                    "No se pudo rechazar el alta: la solicitud cambió de estado o usted la emitió. Actualice e intente nuevamente.");

            _bitacora.RegistrarAccion(sesion.Usuario, "RECHAZO_ALTA_VINO:" + s.VinoCodigo);
        }

        // CU-24: solicita la baja de un vino activo. El vino sigue Activo (R1): la baja queda como
        // solicitud Pendiente, a la espera del Administrador (CU-25).
        public void SolicitarBaja(int vinoId, USUARIO sesion)
        {
            Vino v = _dal.ObtenerPorId(vinoId);
            if (v == null)
                throw new InvalidOperationException("El vino indicado no existe.");
            if (v.Estado == EstadoVino.Descontinuado)
                throw new InvalidOperationException("El vino ya está descontinuado.");
            if (v.Estado != EstadoVino.Activo)
                throw new InvalidOperationException("Solo se puede solicitar la baja de un vino activo.");
            if (_solicitudDal.ObtenerPendiente(vinoId, TipoSolicitud.Baja) != null)
                throw new InvalidOperationException(MensajeBajaYaPendiente);

            // RN-15: no se descontinúa un vino con botellas en depósito; el stock remanente se
            // liquida antes con un ajuste de inventario (CU-27). Se valida al solicitar para no
            // generar pedidos que no podrían autorizarse.
            ValidarSinStock(vinoId);

            // 0 filas = el estado, el stock o una baja pendiente cambiaron entre la lectura y la escritura.
            int filas;
            try
            {
                filas = _dal.SolicitarBaja(vinoId, sesion.Id);
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                // Carrera contra otra solicitud de baja: lo frena el índice único UX_SOLAUT_PENDIENTE.
                throw new InvalidOperationException(MensajeBajaYaPendiente);
            }
            if (filas == 0)
                throw new InvalidOperationException("No se pudo solicitar la baja: el estado o el stock del vino cambiaron. Actualice e intente nuevamente.");

            _bitacora.RegistrarAccion(sesion.Usuario, "BAJA_SOLICITADA_VINO:" + v.Codigo);
        }

        private const string MensajeBajaYaPendiente =
            "El vino ya tiene una baja solicitada, pendiente de autorización.";

        // CU-25: RN-04 (baja lógica, el SP cambia ESTADO y nunca borra) y RN-01 (separación de funciones).
        public void AutorizarBaja(int solicitudId, USUARIO sesion)
        {
            SolicitudAutorizacion s = ObtenerSolicitudPendiente(solicitudId, TipoSolicitud.Baja);
            // RN-01, capa BLL: guard en memoria, ANTES de llamar al DAL.
            ValidarSeparacionDeFunciones(s, sesion, "autorizar");

            // RN-15 se revalida: entre la solicitud y la autorización pudo ingresar mercadería (CU-23).
            ValidarSinStock(s.VinoId);

            // Backstop SP de RN-01 y RN-15: 0 filas = rechazado por la BD (otro usuario cambió el estado o el stock).
            int filas = _solicitudDal.AprobarBaja(s.Id, sesion.Id);
            if (filas == 0)
                throw new InvalidOperationException(
                    "No se pudo autorizar la baja: el vino cambió de estado o de stock. Actualice e intente nuevamente.");

            _bitacora.RegistrarAccion(sesion.Usuario, "AUTORIZACION_BAJA_VINO:" + s.VinoCodigo);
        }

        // CU-25 (rechazo): R3 el vino sigue Activo; R6 el motivo es obligatorio. RN-01 también aplica al rechazo.
        public void RechazarBaja(int solicitudId, string motivo, USUARIO sesion)
        {
            string motivoLimpio = ValidarMotivo(motivo);
            SolicitudAutorizacion s = ObtenerSolicitudPendiente(solicitudId, TipoSolicitud.Baja);
            ValidarSeparacionDeFunciones(s, sesion, "rechazar");

            int filas = _solicitudDal.RechazarBaja(s.Id, sesion.Id, motivoLimpio);
            if (filas == 0)
                throw new InvalidOperationException("No se pudo rechazar la baja: la solicitud cambió de estado o usted la emitió. Actualice e intente nuevamente.");

            _bitacora.RegistrarAccion(sesion.Usuario, "RECHAZO_BAJA_VINO:" + s.VinoCodigo);
        }

        private SolicitudAutorizacion ObtenerSolicitudPendiente(int solicitudId, TipoSolicitud tipo)
        {
            SolicitudAutorizacion s = _solicitudDal.ObtenerPorId(solicitudId);
            if (s == null)
                throw new InvalidOperationException("La solicitud indicada no existe.");
            if (s.Estado != EstadoSolicitud.Pendiente)
                throw new InvalidOperationException("La solicitud ya fue resuelta.");
            if (s.Tipo != tipo)
                throw new InvalidOperationException("La solicitud no corresponde a este trámite.");
            return s;
        }

        // RN-01 (capa BLL): quien emitió la solicitud no puede resolverla, ni aprobándola ni rechazándola.
        private void ValidarSeparacionDeFunciones(SolicitudAutorizacion s, USUARIO sesion, string accion)
        {
            if (s.EmisorId == sesion.Id)
                throw new InvalidOperationException("No puede " + accion + " una solicitud que usted mismo emitió.");
        }

        // R6: el motivo es obligatorio; se recorta y se limita a la longitud de la columna.
        private string ValidarMotivo(string motivo)
        {
            string limpio = motivo == null ? string.Empty : motivo.Trim();
            if (limpio.Length == 0)
                throw new InvalidOperationException("Debe indicar el motivo del rechazo.");
            if (limpio.Length > MotivoMaxLongitud)
                throw new InvalidOperationException("El motivo del rechazo no puede superar los " + MotivoMaxLongitud + " caracteres.");
            return limpio;
        }

        private void ValidarSinStock(int vinoId)
        {
            int stock = _movimientoDal.ObtenerStockActual(vinoId);
            if (stock != 0)
                throw new InvalidOperationException(
                    "No se puede descontinuar un vino con stock (" + stock + " botellas). " +
                    "Registre un ajuste de inventario para dejarlo en 0 y luego solicite la baja.");
        }

        // CU-26: consulta pasiva, sin efectos secundarios ni bitácora.
        public List<AlertaStockMinimo> ListarAlertaStockMinimo()
        {
            return _dal.ListarBajoStockMinimo();
        }
    }
}
