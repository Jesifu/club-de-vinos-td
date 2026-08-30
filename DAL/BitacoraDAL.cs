using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class BitacoraDAL
    {
        private readonly Acceso _acceso = new Acceso();

        public void Registrar(string usuario, string accion)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@usuario", usuario),
                _acceso.CrearParametro("@accion",  accion)
            };
            try
            {
                _acceso.Abrir();
                _acceso.Escribir("BITACORA_INSERTAR", parametros);
            }
            finally
            {
                _acceso.Cerrar();
            }
        }

        public BE.PaginaResultado<BE.BITACORA> ListarPaginado(string usuario, string accion,
            DateTime? fechaDesde, DateTime? fechaHasta, int pagina, int tamanio)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                ParametroONull("@usuario", usuario),
                ParametroONull("@accion", accion),
                ParametroFechaONull("@fecha_desde", fechaDesde),
                ParametroFechaONull("@fecha_hasta", fechaHasta),
                _acceso.CrearParametro("@pagina", pagina),
                _acceso.CrearParametro("@tamanio", tamanio)
            };

            BE.PaginaResultado<BE.BITACORA> resultado = new BE.PaginaResultado<BE.BITACORA>
            {
                Pagina = pagina,
                Tamanio = tamanio
            };

            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("BITACORA_LISTAR_PAGINADO", parametros);
                foreach (DataRow fila in tabla.Rows)
                {
                    resultado.Items.Add(new BE.BITACORA
                    {
                        Id = int.Parse(fila["ID"].ToString()),
                        Usuario = fila["USUARIO"].ToString(),
                        Accion = fila["ACCION"].ToString(),
                        Fecha = System.DateTime.Parse(fila["FECHA"].ToString())
                    });
                }
                resultado.TotalFilas = tabla.Rows.Count > 0
                    ? Convert.ToInt32(tabla.Rows[0]["TOTAL_FILAS"])
                    : 0;
            }
            finally
            {
                _acceso.Cerrar();
            }
            return resultado;
        }

        public List<string> ListarUsuariosDistinct()
        {
            List<string> lista = new List<string>();
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("BITACORA_LISTAR_USUARIOS_DISTINCT");
                foreach (DataRow fila in tabla.Rows)
                    lista.Add(fila["USUARIO"].ToString());
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }

        public List<string> ListarAccionesDistinct()
        {
            List<string> lista = new List<string>();
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("BITACORA_LISTAR_ACCIONES_DISTINCT");
                foreach (DataRow fila in tabla.Rows)
                    lista.Add(fila["ACCION"].ToString());
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }

        private SqlParameter ParametroONull(string nombre, string valor)
        {
            SqlParameter p = new SqlParameter(nombre, DbType.String);
            p.Value = string.IsNullOrEmpty(valor) ? (object)DBNull.Value : valor;
            return p;
        }

        private SqlParameter ParametroFechaONull(string nombre, DateTime? valor)
        {
            SqlParameter p = new SqlParameter(nombre, DbType.DateTime);
            p.Value = valor.HasValue ? (object)valor.Value : DBNull.Value;
            return p;
        }
    }
}