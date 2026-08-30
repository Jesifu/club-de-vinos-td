using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class IntegridadDAL
    {
        private readonly Acceso _acceso = new Acceso();

        public DataTable ListarUsuariosParaIntegridad()
        {
            try
            {
                _acceso.Abrir();
                return _acceso.Leer("USUARIO_LISTAR_PARA_INTEGRIDAD");
            }
            finally
            {
                _acceso.Cerrar();
            }
        }

        public void ActualizarDVHUsuario(int id, int dvh)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@id",  id),
                _acceso.CrearParametro("@dvh", dvh)
            };
            try
            {
                _acceso.Abrir();
                _acceso.Escribir("USUARIO_ACTUALIZAR_DVH", parametros);
            }
            finally
            {
                _acceso.Cerrar();
            }
        }

        public Dictionary<string, int> ObtenerDVV(string tabla)
        {
            Dictionary<string, int> resultado = new Dictionary<string, int>();
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@tabla", tabla)
            };
            try
            {
                _acceso.Abrir();
                DataTable dt = _acceso.Leer("DVV_LISTAR", parametros);
                foreach (DataRow fila in dt.Rows)
                    resultado[fila["COLUMNA"].ToString()] = Convert.ToInt32(fila["DVV"]);
            }
            finally
            {
                _acceso.Cerrar();
            }
            return resultado;
        }

        public void ActualizarDVV(string tabla, string columna, int dvv)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@tabla",   tabla),
                _acceso.CrearParametro("@columna", columna),
                _acceso.CrearParametro("@dvv",     dvv)
            };
            try
            {
                _acceso.Abrir();
                _acceso.Escribir("DVV_ACTUALIZAR", parametros);
            }
            finally
            {
                _acceso.Cerrar();
            }
        }
    }
}
