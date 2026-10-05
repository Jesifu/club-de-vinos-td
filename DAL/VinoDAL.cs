using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class VinoDAL
    {
        private readonly Acceso _acceso = new Acceso();

        public void Insertar(BE.Vino v)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@codigo",       v.Codigo),
                _acceso.CrearParametro("@nombre",       v.Nombre),
                _acceso.CrearParametro("@bodega_id",    v.BodegaId),
                _acceso.CrearParametro("@varietal",     v.Varietal),
                _acceso.CrearParametro("@aniada",       v.Aniada),
                _acceso.CrearParametro("@stock_minimo", v.StockMinimo),
                _acceso.CrearParametro("@creado_por",   v.CreadoPor)
            };

            // Acceso.CrearParametro no tiene overload para decimal.
            SqlParameter pPrecio = new SqlParameter("@precio", SqlDbType.Decimal)
            {
                Precision = 10,
                Scale = 2,
                Value = v.Precio
            };
            parametros.Add(pPrecio);

            SqlParameter pMaridaje = new SqlParameter("@maridaje", SqlDbType.VarChar, 200)
            {
                Value = (object)v.Maridaje ?? DBNull.Value
            };
            parametros.Add(pMaridaje);

            // Nullable int: construcción manual, per convención documentada en CLAUDE.md.
            SqlParameter pPuntaje = new SqlParameter("@puntaje", SqlDbType.Int)
            {
                Value = (object)v.Puntaje ?? DBNull.Value
            };
            parametros.Add(pPuntaje);

            try
            {
                _acceso.Abrir();
                _acceso.Escribir("VINO_INSERTAR", parametros);
            }
            finally { _acceso.Cerrar(); }
        }

        // R2: devuelve FILAS (0 = el vino ya no está Rechazado). El SKU no se modifica.
        // Si dos re-propuestas compiten, el índice único de la base lanza SqlException 2601/2627.
        public int Reproponer(BE.Vino v, int emisorId)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@id",           v.Id),
                _acceso.CrearParametro("@nombre",       v.Nombre),
                _acceso.CrearParametro("@bodega_id",    v.BodegaId),
                _acceso.CrearParametro("@varietal",     v.Varietal),
                _acceso.CrearParametro("@aniada",       v.Aniada),
                _acceso.CrearParametro("@stock_minimo", v.StockMinimo),
                _acceso.CrearParametro("@emisor_id",    emisorId)
            };

            parametros.Add(new SqlParameter("@precio", SqlDbType.Decimal)
            {
                Precision = 10,
                Scale = 2,
                Value = v.Precio
            });
            parametros.Add(new SqlParameter("@maridaje", SqlDbType.VarChar, 200)
            {
                Value = (object)v.Maridaje ?? DBNull.Value
            });
            parametros.Add(new SqlParameter("@puntaje", SqlDbType.Int)
            {
                Value = (object)v.Puntaje ?? DBNull.Value
            });

            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("VINO_REPROPONER", parametros);
                return Convert.ToInt32(tabla.Rows[0]["FILAS"]);
            }
            finally { _acceso.Cerrar(); }
        }

        // Catálogo de Compras: vinos activos más los propios pendientes o rechazados, con la última solicitud.
        public List<BE.Vino> ListarCatalogo(int usuarioId)
        {
            List<BE.Vino> lista = new List<BE.Vino>();
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@usuario_id", usuarioId)
            };
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("VINO_LISTAR_CATALOGO", parametros);
                foreach (DataRow fila in tabla.Rows)
                    lista.Add(MapearFila(fila));
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }

        public BE.Vino ObtenerPorCodigo(string codigo)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@codigo", codigo)
            };
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("VINO_OBTENER_POR_CODIGO", parametros);
                return tabla.Rows.Count > 0 ? MapearFila(tabla.Rows[0]) : null;
            }
            finally { _acceso.Cerrar(); }
        }

        public BE.Vino ObtenerPorId(int id)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@id", id)
            };
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("VINO_OBTENER_POR_ID", parametros);
                return tabla.Rows.Count > 0 ? MapearFila(tabla.Rows[0]) : null;
            }
            finally { _acceso.Cerrar(); }
        }

        public List<BE.Vino> ListarAutorizados()
        {
            List<BE.Vino> lista = new List<BE.Vino>();
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("VINO_LISTAR_AUTORIZADOS");
                foreach (DataRow fila in tabla.Rows)
                    lista.Add(MapearFila(fila));
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }

        // CU-24: devuelve FILAS (0 = el estado, el stock o una baja pendiente impiden la solicitud).
        // Si dos solicitudes compiten, el índice único de la base lanza SqlException 2601/2627.
        public int SolicitarBaja(int vinoId, int solicitanteId)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@id",             vinoId),
                _acceso.CrearParametro("@solicitante_id", solicitanteId)
            };
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("VINO_SOLICITAR_BAJA", parametros);
                return Convert.ToInt32(tabla.Rows[0]["FILAS"]);
            }
            finally { _acceso.Cerrar(); }
        }

        // CU-26: vinos activos con stock derivado < STOCK_MINIMO.
        public List<BE.AlertaStockMinimo> ListarBajoStockMinimo()
        {
            List<BE.AlertaStockMinimo> lista = new List<BE.AlertaStockMinimo>();
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("VINO_LISTAR_STOCK_BAJO");
                foreach (DataRow fila in tabla.Rows)
                {
                    lista.Add(new BE.AlertaStockMinimo
                    {
                        VinoId = Convert.ToInt32(fila["ID"]),
                        Codigo = fila["CODIGO"].ToString(),
                        Nombre = fila["NOMBRE"].ToString(),
                        BodegaNombre = fila["BODEGA_NOMBRE"].ToString(),
                        StockMinimo = Convert.ToInt32(fila["STOCK_MINIMO"]),
                        StockActual = Convert.ToInt32(fila["STOCK_ACTUAL"]),
                        Faltante = Convert.ToInt32(fila["FALTANTE"])
                    });
                }
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }

        // RN-06 (filtra en el WHERE de la SP) + RN-05.2 (ordena, nunca filtra).
        public List<BE.VinoCandidato> ListarCandidatos(int socioId)
        {
            List<BE.VinoCandidato> lista = new List<BE.VinoCandidato>();
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@socio_id", socioId)
            };
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("VINO_LISTAR_CANDIDATOS", parametros);
                foreach (DataRow fila in tabla.Rows)
                    lista.Add(MapearCandidato(fila));
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }

        private BE.VinoCandidato MapearCandidato(DataRow fila)
        {
            return new BE.VinoCandidato
            {
                Id = Convert.ToInt32(fila["ID"]),
                Codigo = fila["CODIGO"].ToString(),
                Nombre = fila["NOMBRE"].ToString(),
                BodegaNombre = fila["BODEGA_NOMBRE"].ToString(),
                Varietal = fila["VARIETAL"].ToString(),
                Aniada = Convert.ToInt32(fila["ANIADA"]),
                Precio = Convert.ToDecimal(fila["PRECIO"]),
                Stock = Convert.ToInt32(fila["STOCK"]),
                Preferido = Convert.ToBoolean(fila["PREFERIDO"])
            };
        }

        private BE.Vino MapearFila(DataRow fila)
        {
            return new BE.Vino
            {
                Id = Convert.ToInt32(fila["ID"]),
                Codigo = fila["CODIGO"].ToString(),
                Nombre = fila["NOMBRE"].ToString(),
                BodegaId = Convert.ToInt32(fila["BODEGA_ID"]),
                BodegaNombre = fila["BODEGA_NOMBRE"].ToString(),
                Varietal = fila["VARIETAL"].ToString(),
                Aniada = Convert.ToInt32(fila["ANIADA"]),
                Precio = Convert.ToDecimal(fila["PRECIO"]),
                StockMinimo = Convert.ToInt32(fila["STOCK_MINIMO"]),
                Estado = (BE.EstadoVino)Enum.Parse(typeof(BE.EstadoVino), fila["ESTADO"].ToString()),
                Maridaje = fila["MARIDAJE"] == DBNull.Value ? null : fila["MARIDAJE"].ToString(),
                Puntaje = fila["PUNTAJE"] == DBNull.Value ? (int?)null : Convert.ToInt32(fila["PUNTAJE"]),
                CreadoPor = Convert.ToInt32(fila["CREADO_POR"]),
                CreadoPorLogin = fila["CREADO_POR_LOGIN"].ToString(),
                FechaAlta = Convert.ToDateTime(fila["FECHA_ALTA"]),
                // Solo VINO_LISTAR_CATALOGO devuelve estas columnas.
                UltimaSolicitudTipo = TextoOpcional(fila, "ULT_SOLICITUD_TIPO") == null
                    ? (BE.TipoSolicitud?)null
                    : SolicitudAutorizacionDAL.TipoDesdeDb(TextoOpcional(fila, "ULT_SOLICITUD_TIPO")),
                UltimaSolicitudEstado = TextoOpcional(fila, "ULT_SOLICITUD_ESTADO") == null
                    ? (BE.EstadoSolicitud?)null
                    : (BE.EstadoSolicitud)Enum.Parse(typeof(BE.EstadoSolicitud), TextoOpcional(fila, "ULT_SOLICITUD_ESTADO")),
                UltimoMotivoRechazo = TextoOpcional(fila, "ULT_MOTIVO_RECHAZO")
            };
        }

        private static string TextoOpcional(DataRow fila, string columna)
        {
            if (!fila.Table.Columns.Contains(columna) || fila[columna] == DBNull.Value)
                return null;
            return fila[columna].ToString();
        }
    }
}
