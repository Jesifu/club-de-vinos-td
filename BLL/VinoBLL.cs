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
    }
}
