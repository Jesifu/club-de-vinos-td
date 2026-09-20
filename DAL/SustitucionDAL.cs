using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class SustitucionDAL
    {
        private readonly Acceso _acceso = new Acceso();

        // SUSTITUCION_INSERTAR es una única sentencia INSERT...SELECT...WHERE (RN-08+RN-10
        // backstop en el WHERE) -> Escribir() devuelve un 0/1 honesto (C4).
        public int Insertar(Sustitucion s)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@caja_vino_id",      s.CajaVinoId),
                _acceso.CrearParametro("@vino_original_id",  s.VinoOriginalId),
                _acceso.CrearParametro("@vino_reemplazo_id", s.VinoReemplazoId),
                _acceso.CrearParametro("@nombre_snapshot",   s.NombreSnapshot),
                _acceso.CrearParametro("@motivo",            s.Motivo),
                _acceso.CrearParametro("@responsable_id",    s.ResponsableId)
            };
            parametros.Add(new SqlParameter("@precio_snapshot", SqlDbType.Decimal)
            {
                Precision = 10,
                Scale = 2,
                Value = s.PrecioSnapshot
            });

            try
            {
                _acceso.Abrir();
                return _acceso.Escribir("SUSTITUCION_INSERTAR", parametros);
            }
            finally { _acceso.Cerrar(); }
        }

        public List<Sustitucion> ListarPorCaja(int cajaId)
        {
            List<Sustitucion> lista = new List<Sustitucion>();
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("SUSTITUCION_LISTAR_POR_CAJA",
                    new List<SqlParameter> { _acceso.CrearParametro("@caja_id", cajaId) });
                foreach (DataRow fila in tabla.Rows)
                    lista.Add(MapearFila(fila));
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }

        private Sustitucion MapearFila(DataRow fila)
        {
            return new Sustitucion
            {
                Id = Convert.ToInt32(fila["ID"]),
                CajaVinoId = Convert.ToInt32(fila["CAJA_VINO_ID"]),
                VinoOriginalId = Convert.ToInt32(fila["VINO_ORIGINAL_ID"]),
                VinoReemplazoId = Convert.ToInt32(fila["VINO_REEMPLAZO_ID"]),
                NombreSnapshot = fila["NOMBRE_SNAPSHOT"].ToString(),
                PrecioSnapshot = Convert.ToDecimal(fila["PRECIO_SNAPSHOT"]),
                Motivo = fila["MOTIVO"].ToString(),
                ResponsableId = Convert.ToInt32(fila["RESPONSABLE_ID"]),
                ResponsableLogin = fila["RESPONSABLE_LOGIN"].ToString(),
                Fecha = Convert.ToDateTime(fila["FECHA"])
            };
        }
    }
}
