using BE;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace BLL
{
    public class VinoBLL
    {
        private readonly DAL.VinoDAL _dal = new DAL.VinoDAL();
        private readonly DAL.BodegaDAL _bodegaDal = new DAL.BodegaDAL();
        private readonly DAL.MovimientoStockDAL _movimientoDal = new DAL.MovimientoStockDAL();
        private readonly BitacoraBLL _bitacora = new BitacoraBLL();

        public List<Bodega> ListarBodegas()
        {
            return _bodegaDal.ListarHabilitadas();
        }

        public void ProponerAlta(Vino v, USUARIO sesion)
        {
            if (string.IsNullOrWhiteSpace(v.Codigo))
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

            if (_dal.ObtenerPorCodigo(v.Codigo) != null)
                throw new InvalidOperationException("Ya existe un vino con el SKU indicado.");

            v.CreadoPor = sesion.Id;
            v.Estado = EstadoVino.Activo;
            v.AutorizadoPor = null;

            try
            {
                _dal.Insertar(v);
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                // Violación de FK_VINO_BODEGA: bypass de UI con un BodegaId inexistente.
                throw new InvalidOperationException("Debe seleccionar una bodega válida.");
            }

            _bitacora.RegistrarAccion(sesion.Usuario, "ALTA_VINO:" + v.Codigo);
        }

        public List<Vino> ListarPendientes()
        {
            return _dal.ListarPendientes();
        }

        public List<Vino> ListarAutorizados()
        {
            return _dal.ListarAutorizados();
        }

        public void Autorizar(int vinoId, USUARIO sesion)
        {
            Vino v = _dal.ObtenerPorId(vinoId);
            if (v == null)
                throw new InvalidOperationException("El vino indicado no existe.");
            if (v.AutorizadoPor.HasValue)
                throw new InvalidOperationException("El vino ya fue autorizado.");
            // RN-01, capa BLL: guard en memoria, ANTES de llamar al DAL.
            if (v.CreadoPor == sesion.Id)
                throw new InvalidOperationException("No puede autorizar un vino que usted mismo dio de alta.");

            // RN-01, backstop SP: WHERE CREADO_POR <> @admin_id. 0 filas = rechazado por la BD.
            int filas = _dal.Autorizar(vinoId, sesion.Id);
            if (filas == 0)
                throw new InvalidOperationException("No puede autorizar un vino que usted mismo dio de alta.");

            _bitacora.RegistrarAccion(sesion.Usuario, "AUTORIZACION_VINO:" + v.Codigo);
        }

        // CU-24: solicita la baja de un vino autorizado y activo. No cambia el ESTADO:
        // solo registra quién y cuándo la pidió, a la espera del Administrador (CU-25).
        public void SolicitarBaja(int vinoId, USUARIO sesion)
        {
            Vino v = _dal.ObtenerPorId(vinoId);
            if (v == null)
                throw new InvalidOperationException("El vino indicado no existe.");
            if (!v.AutorizadoPor.HasValue)
                throw new InvalidOperationException("Solo se puede solicitar la baja de un vino autorizado.");
            if (v.Estado == EstadoVino.Descontinuado)
                throw new InvalidOperationException("El vino ya está descontinuado.");
            if (v.BajaSolicitadaPor.HasValue)
                throw new InvalidOperationException("El vino ya tiene una baja solicitada, pendiente de autorización.");

            // RN-15: no se descontinúa un vino con botellas en depósito; el stock remanente se
            // liquida antes con un ajuste de inventario (CU-27). Se valida al solicitar para no
            // generar pedidos que no podrían autorizarse.
            ValidarSinStock(vinoId);

            // 0 filas = el estado o el stock cambiaron entre la lectura y la escritura.
            int filas = _dal.SolicitarBaja(vinoId, sesion.Id);
            if (filas == 0)
                throw new InvalidOperationException("No se pudo solicitar la baja: el estado o el stock del vino cambiaron. Actualice e intente nuevamente.");

            _bitacora.RegistrarAccion(sesion.Usuario, "BAJA_SOLICITADA_VINO:" + v.Codigo);
        }

        public List<Vino> ListarPendientesDeBaja()
        {
            return _dal.ListarPendientesDeBaja();
        }

        // CU-25: RN-04 (baja lógica, el SP cambia ESTADO y nunca borra) y RN-01 (separación de funciones).
        public void AutorizarBaja(int vinoId, USUARIO sesion)
        {
            Vino v = ObtenerConBajaPendiente(vinoId);
            // RN-01, capa BLL: guard en memoria, ANTES de llamar al DAL.
            if (v.BajaSolicitadaPor == sesion.Id)
                throw new InvalidOperationException("No puede autorizar una descontinuación que usted mismo solicitó.");

            // RN-15 se revalida: entre la solicitud y la autorización pudo ingresar mercadería (CU-23).
            ValidarSinStock(vinoId);

            // Backstop SP de RN-01 y RN-15: 0 filas = rechazado por la BD (otro usuario cambió el estado o el stock).
            int filas = _dal.AutorizarBaja(vinoId, sesion.Id);
            if (filas == 0)
                throw new InvalidOperationException(
                    "No se pudo autorizar la baja: el vino cambió de estado o de stock. Actualice e intente nuevamente.");

            _bitacora.RegistrarAccion(sesion.Usuario, "AUTORIZACION_BAJA_VINO:" + v.Codigo);
        }

        // CU-25 (rechazo): limpia la solicitud y el vino sigue Activo.
        public void RechazarBaja(int vinoId, USUARIO sesion)
        {
            Vino v = ObtenerConBajaPendiente(vinoId);

            int filas = _dal.RechazarBaja(vinoId);
            if (filas == 0)
                throw new InvalidOperationException("No se pudo rechazar la baja: el estado del vino cambió. Actualice e intente nuevamente.");

            _bitacora.RegistrarAccion(sesion.Usuario, "RECHAZO_BAJA_VINO:" + v.Codigo);
        }

        private void ValidarSinStock(int vinoId)
        {
            int stock = _movimientoDal.ObtenerStockActual(vinoId);
            if (stock != 0)
                throw new InvalidOperationException(
                    "No se puede descontinuar un vino con stock (" + stock + " botellas). " +
                    "Registre un ajuste de inventario para dejarlo en 0 y luego solicite la baja.");
        }

        private Vino ObtenerConBajaPendiente(int vinoId)
        {
            Vino v = _dal.ObtenerPorId(vinoId);
            if (v == null)
                throw new InvalidOperationException("El vino indicado no existe.");
            if (v.Estado != EstadoVino.Activo || !v.BajaSolicitadaPor.HasValue)
                throw new InvalidOperationException("El vino no tiene una baja solicitada pendiente.");
            return v;
        }

        // CU-26: consulta pasiva, sin efectos secundarios ni bitácora.
        public List<AlertaStockMinimo> ListarAlertaStockMinimo()
        {
            return _dal.ListarBajoStockMinimo();
        }
    }
}
