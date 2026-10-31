using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class RotacionDAL
    {
        private readonly Acceso _acceso = new Acceso();

        // Datos crudos del período [desde, hasta] (ambos días inclusive) para cada vino Activo.
        // Los campos derivados (consumo, cobertura, clasificación) los completa la BLL.
        public List<BE.ResultadoRotacion> Analizar(DateTime desde, DateTime hasta)
        {
            List<BE.ResultadoRotacion> lista = new List<BE.ResultadoRotacion>();
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                new SqlParameter("@desde", SqlDbType.DateTime) { Value = desde.Date },
                new SqlParameter("@hasta", SqlDbType.DateTime) { Value = hasta.Date }
            };

            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("ROTACION_ANALIZAR", parametros);
                foreach (DataRow fila in tabla.Rows)
                {
                    lista.Add(new BE.ResultadoRotacion
                    {
                        VinoId = Convert.ToInt32(fila["ID"]),
                        Codigo = fila["CODIGO"].ToString(),
                        Nombre = fila["NOMBRE"].ToString(),
                        BodegaNombre = fila["BODEGA_NOMBRE"].ToString(),
                        StockMinimo = Convert.ToInt32(fila["STOCK_MINIMO"]),
                        StockActual = Convert.ToInt32(fila["STOCK_ACTUAL"]),
                        UnidadesVendidas = Convert.ToInt32(fila["UNIDADES_VENDIDAS"]),
                        UnidadesPerdidas = Convert.ToInt32(fila["UNIDADES_PERDIDAS"]),
                        SociosAtendidos = Convert.ToInt32(fila["SOCIOS_ATENDIDOS"])
                    });
                }
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }
    }
}
