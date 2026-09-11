using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class MovimientoStockDAL
    {
        private readonly Acceso _acceso = new Acceso();

        public void Insertar(BE.MovimientoStock m)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@vino_id",     m.VinoId),
                _acceso.CrearParametro("@tipo",        m.Tipo.ToString()),
                _acceso.CrearParametro("@cantidad",    m.Cantidad),
                _acceso.CrearParametro("@motivo",      m.Motivo),
                _acceso.CrearParametro("@responsable", m.Responsable)
            };

            SqlParameter pReferenciaTipo = new SqlParameter("@referencia_tipo", SqlDbType.VarChar, 20)
            {
                Value = (object)m.ReferenciaTipo ?? DBNull.Value
            };
            parametros.Add(pReferenciaTipo);

            // Nullable int: construcción manual, per convención documentada en CLAUDE.md.
            SqlParameter pReferenciaId = new SqlParameter("@referencia_id", SqlDbType.Int)
            {
                Value = (object)m.ReferenciaId ?? DBNull.Value
            };
            parametros.Add(pReferenciaId);

            try
            {
                _acceso.Abrir();
                _acceso.Escribir("MOVIMIENTO_STOCK_INSERTAR", parametros);
            }
            finally { _acceso.Cerrar(); }
        }

        public List<BE.MovimientoStock> ListarPorVino(int vinoId)
        {
            List<BE.MovimientoStock> lista = new List<BE.MovimientoStock>();
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@vino_id", vinoId)
            };
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("MOVIMIENTO_STOCK_LISTAR_POR_VINO", parametros);
                foreach (DataRow fila in tabla.Rows)
                    lista.Add(MapearFila(fila));
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }

        public int ObtenerStockActual(int vinoId)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@vino_id", vinoId)
            };
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("STOCK_ACTUAL_OBTENER", parametros);
                return tabla.Rows.Count > 0 ? Convert.ToInt32(tabla.Rows[0]["STOCK"]) : 0;
            }
            finally { _acceso.Cerrar(); }
        }

        private BE.MovimientoStock MapearFila(DataRow fila)
        {
            return new BE.MovimientoStock
            {
                Id = Convert.ToInt32(fila["ID"]),
                VinoId = Convert.ToInt32(fila["VINO_ID"]),
                VinoNombre = fila["VINO_NOMBRE"].ToString(),
                Fecha = Convert.ToDateTime(fila["FECHA"]),
                Tipo = (BE.TipoMovimiento)Enum.Parse(typeof(BE.TipoMovimiento), fila["TIPO"].ToString()),
                Cantidad = Convert.ToInt32(fila["CANTIDAD"]),
                Motivo = fila["MOTIVO"].ToString(),
                Responsable = fila["RESPONSABLE"].ToString(),
                ReferenciaTipo = fila["REFERENCIA_TIPO"] == DBNull.Value ? null : fila["REFERENCIA_TIPO"].ToString(),
                ReferenciaId = fila["REFERENCIA_ID"] == DBNull.Value ? (int?)null : Convert.ToInt32(fila["REFERENCIA_ID"])
            };
        }
    }
}
