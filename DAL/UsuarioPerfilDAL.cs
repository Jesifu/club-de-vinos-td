using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class UsuarioPerfilDAL
    {
        private readonly Acceso _acceso = new Acceso();

        public List<string> ListarPermisosDeUsuario(int usuarioId)
        {
            List<string> permisos = new List<string>();
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@usuario_id", usuarioId)
            };
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("USUARIO_PERMISOS_LISTAR", parametros);
                foreach (DataRow fila in tabla.Rows)
                    permisos.Add(fila["NOMBRE"].ToString());
            }
            finally
            {
                _acceso.Cerrar();
            }
            return permisos;
        }

        public List<int> ListarPerfilesPorUsuario(int usuarioId)
        {
            List<int> ids = new List<int>();
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@usuario_id", usuarioId)
            };
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("USUARIO_PERFIL_LISTAR", parametros);
                foreach (DataRow fila in tabla.Rows)
                    ids.Add(Convert.ToInt32(fila["PERFIL_ID"]));
            }
            finally
            {
                _acceso.Cerrar();
            }
            return ids;
        }

        public void BorrarTodos(int usuarioId)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@usuario_id", usuarioId)
            };
            try
            {
                _acceso.Abrir();
                _acceso.Escribir("USUARIO_PERFIL_BORRAR_TODOS", parametros);
            }
            finally
            {
                _acceso.Cerrar();
            }
        }

        public void Asignar(int usuarioId, int perfilId)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@usuario_id", usuarioId),
                _acceso.CrearParametro("@perfil_id",  perfilId)
            };
            try
            {
                _acceso.Abrir();
                _acceso.Escribir("USUARIO_PERFIL_ASIGNAR", parametros);
            }
            finally
            {
                _acceso.Cerrar();
            }
        }
    }
}
