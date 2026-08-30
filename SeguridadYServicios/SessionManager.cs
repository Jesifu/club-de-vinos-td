using BE;
using System.Collections.Generic;

namespace SeguridadYServicios
{
    public sealed class SessionManager
    {
        private static SessionManager _instance = null;
        private static readonly object _cerrojo = new object();
        private USUARIO _usuario;
        private List<string> _permisos = new List<string>();

        private SessionManager() { }

        public static SessionManager getInstance()
        {
            if (_instance == null)
            {
                lock (_cerrojo)
                {
                    if (_instance == null)
                        _instance = new SessionManager();
                }
            }
            return _instance;
        }

        public USUARIO getUsuario()
        {
            return _usuario;
        }

        public void setUsuario(USUARIO usuario)
        {
            _usuario = usuario;
        }

        public void setPermisos(List<string> permisos)
        {
            _permisos = permisos ?? new List<string>();
        }

        public bool TienePermiso(string nombrePermiso)
        {
            return _permisos.Contains(nombrePermiso);
        }

        public void cerrarSesion()
        {
            _usuario = null;
            _permisos = new List<string>();
            _instance = null;
        }

        public bool EsAdmin()
        {
            return _usuario != null && _usuario.Rol == "admin";
        }
    }
}
