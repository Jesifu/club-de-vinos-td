using BE;
using SeguridadYServicios;

namespace BLL
{
    public class UsuarioBLL
    {
        private readonly DAL.UsuarioDAL _dal = new DAL.UsuarioDAL();
        private readonly BitacoraBLL _bitacora = new BitacoraBLL();
        private readonly IntegridadBLL _integridad = new IntegridadBLL();
        private readonly UsuarioHistorialBLL _historial = new UsuarioHistorialBLL();

        public BE.USUARIO ObtenerPorId(int id)
        {
            return _dal.ObtenerPorId(id);
        }

        public bool VerificarContrasena(string usuario, string contrasena)
        {
            string hash = Hasher.Hashear(contrasena);
            BE.USUARIO u = _dal.ObtenerPorCredenciales(usuario, hash);
            return u != null;
        }

        public bool CambiarContrasena(string usuario, string nuevaContrasena)
        {
            string hash = Hasher.Hashear(nuevaContrasena);
            bool resultado = _dal.CambiarContrasena(usuario, hash);

            if (resultado)
            {
                _bitacora.RegistrarAccion(usuario, "CAMBIO_CONTRASENA");
                _integridad.RecalcularIntegridadUsuarios();
                int? id = _dal.ObtenerIdPorNombre(usuario);
                if (id.HasValue) _historial.RegistrarCambio(id.Value, "CAMBIO_CLAVE", usuario);
            }

            return resultado;
        }

        public BE.PaginaResultado<BE.USUARIO> ListarPaginado(string busqueda, int pagina, int tamanio)
        {
            return _dal.ListarPaginado(busqueda, pagina, tamanio);
        }

        public void Eliminar(int id)
        {
            string admin = SessionManager.getInstance().getUsuario().Usuario;
            _historial.RegistrarCambio(id, "BAJA", admin);
            _dal.Eliminar(id);
            _integridad.RecalcularIntegridadUsuarios();
        }

        public bool Crear(string usuario, string contrasena, int rolId, string nombre, string apellido,
                           string telefono = null, string email = null)
        {
            string hash = Hasher.Hashear(contrasena);
            bool resultado = _dal.Crear(usuario, hash, rolId, nombre, apellido, telefono, email);
            if (resultado)
            {
                _integridad.RecalcularIntegridadUsuarios();
                string admin = SessionManager.getInstance().getUsuario()?.Usuario ?? "sistema";
                int? id = _dal.ObtenerIdPorNombre(usuario);
                if (id.HasValue) _historial.RegistrarCambio(id.Value, "ALTA", admin);
            }
            return resultado;
        }

        public void ActualizarDatos(int id, string nombre, string apellido, string telefono = null, string email = null)
        {
            _dal.ActualizarDatos(id, nombre, apellido, telefono, email);
            string admin = SessionManager.getInstance().getUsuario().Usuario;
            _historial.RegistrarCambio(id, "EDICION_DATOS", admin);
        }

        public void ActualizarIdioma(int id, int idiomaId) =>
            _dal.ActualizarIdioma(id, idiomaId);

        public void Desbloquear(string usuario)
        {
            _dal.Desbloquear(usuario);
            string admin = SessionManager.getInstance().getUsuario().Usuario;
            _bitacora.RegistrarAccion(admin, "DESBLOQUEO_USUARIO:" + usuario);
            _integridad.RecalcularIntegridadUsuarios();
            int? id = _dal.ObtenerIdPorNombre(usuario);
            if (id.HasValue) _historial.RegistrarCambio(id.Value, "DESBLOQUEO", admin);
        }
    }
}
