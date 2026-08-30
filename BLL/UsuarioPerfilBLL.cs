using System.Collections.Generic;
using SeguridadYServicios;

namespace BLL
{
    public class UsuarioPerfilBLL
    {
        private readonly DAL.UsuarioPerfilDAL _dal = new DAL.UsuarioPerfilDAL();
        private readonly DAL.UsuarioDAL _usuarioDal = new DAL.UsuarioDAL();
        private readonly IntegridadBLL _integridad = new IntegridadBLL();
        private readonly UsuarioHistorialBLL _historial = new UsuarioHistorialBLL();

        public List<string> ObtenerPermisos(int usuarioId)
        {
            return _dal.ListarPermisosDeUsuario(usuarioId);
        }

        public List<int> ObtenerPerfilesAsignados(int usuarioId)
        {
            return _dal.ListarPerfilesPorUsuario(usuarioId);
        }

        public void GuardarAsignaciones(int usuarioId, List<int> perfilIds)
        {
            _dal.BorrarTodos(usuarioId);
            foreach (int id in perfilIds)
                _dal.Asignar(usuarioId, id);
            _integridad.RecalcularIntegridadUsuarios();
            string admin = SessionManager.getInstance().getUsuario()?.Usuario ?? "sistema";
            _historial.RegistrarCambio(usuarioId, "ASIGNACION_PERFIL", admin);

            // Si se reasignaron perfiles al usuario de la sesión activa
            // (el propio admin logueado), refrescamos su USUARIO y sus
            // permisos en caliente para que no haga falta desloguearse.
            BE.USUARIO usuarioSesion = SessionManager.getInstance().getUsuario();
            if (usuarioSesion != null && usuarioSesion.Id == usuarioId)
            {
                BE.USUARIO usuarioActualizado = _usuarioDal.ObtenerPorId(usuarioId);
                SessionManager.getInstance().setUsuario(usuarioActualizado);

                List<string> permisosActualizados = _dal.ListarPermisosDeUsuario(usuarioId);
                SessionManager.getInstance().setPermisos(permisosActualizados);
            }
        }
    }
}