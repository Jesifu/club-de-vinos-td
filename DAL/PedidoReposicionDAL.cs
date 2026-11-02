using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    // N03 — Pedido de Reposición. Los SP de escritura son transaccionales y devuelven un único
    // result set (FILAS, o ID en INSERTAR) porque Acceso.Leer() solo lee el primero.
    public class PedidoReposicionDAL
    {
        private readonly Acceso _acceso = new Acceso();

        // PEDIDO_REPOSICION_INSERTAR: cabecera + líneas en una transacción dentro del SP.
        // Devuelve el id creado; 0 = rechazado por RN-31/RN-32 (backstop de la BLL).
        public int Insertar(int bodegaId, int creadoPor, string observacion, IEnumerable<PedidoReposicionLinea> lineas)
        {
            DataTable tvp = new DataTable();
            tvp.Columns.Add("VINO_ID", typeof(int));
            tvp.Columns.Add("CANTIDAD_SUGERIDA", typeof(int));
            tvp.Columns.Add("CANTIDAD_PEDIDA", typeof(int));
            foreach (PedidoReposicionLinea l in lineas)
                tvp.Rows.Add(l.VinoId, l.CantidadSugerida, l.CantidadPedida);

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@bodega_id",  bodegaId),
                _acceso.CrearParametro("@creado_por", creadoPor),
                new SqlParameter("@observacion", SqlDbType.VarChar, 200) { Value = (object)observacion ?? DBNull.Value },
                new SqlParameter("@lineas", SqlDbType.Structured) { TypeName = "dbo.PEDIDO_LINEA_TT", Value = tvp }
            };
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("PEDIDO_REPOSICION_INSERTAR", parametros);
                return Convert.ToInt32(tabla.Rows[0]["ID"]);
            }
            finally { _acceso.Cerrar(); }
        }

        // Nº del pedido abierto (Pendiente o Aprobado) que incluye al vino; 0 = ninguno (RN-15 extendida).
        public int ObtenerPedidoAbiertoDeVino(int vinoId)
        {
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("PEDIDO_REPOSICION_ABIERTO_POR_VINO",
                    new List<SqlParameter> { _acceso.CrearParametro("@vino_id", vinoId) });
                return Convert.ToInt32(tabla.Rows[0]["ID"]);
            }
            finally { _acceso.Cerrar(); }
        }

        // estado null = todos.
        public List<PedidoReposicion> Listar(EstadoPedidoReposicion? estado)
        {
            List<PedidoReposicion> lista = new List<PedidoReposicion>();
            SqlParameter pEstado = new SqlParameter("@estado", SqlDbType.VarChar, 10)
            {
                Value = estado.HasValue ? (object)estado.Value.ToString() : DBNull.Value
            };
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("PEDIDO_REPOSICION_LISTAR", new List<SqlParameter> { pEstado });
                foreach (DataRow fila in tabla.Rows)
                    lista.Add(MapearCabecera(fila));
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }

        public PedidoReposicion ObtenerPorId(int id)
        {
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("PEDIDO_REPOSICION_OBTENER_POR_ID",
                    new List<SqlParameter> { _acceso.CrearParametro("@id", id) });
                return tabla.Rows.Count > 0 ? MapearCabecera(tabla.Rows[0]) : null;
            }
            finally { _acceso.Cerrar(); }
        }

        public List<PedidoReposicionLinea> ObtenerLineas(int pedidoId)
        {
            List<PedidoReposicionLinea> lista = new List<PedidoReposicionLinea>();
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("PEDIDO_REPOSICION_OBTENER_LINEAS",
                    new List<SqlParameter> { _acceso.CrearParametro("@pedido_id", pedidoId) });
                foreach (DataRow fila in tabla.Rows)
                {
                    lista.Add(new PedidoReposicionLinea
                    {
                        Id = Convert.ToInt32(fila["ID"]),
                        PedidoId = Convert.ToInt32(fila["PEDIDO_ID"]),
                        VinoId = Convert.ToInt32(fila["VINO_ID"]),
                        VinoCodigo = fila["VINO_CODIGO"].ToString(),
                        VinoNombre = fila["VINO_NOMBRE"].ToString(),
                        CantidadSugerida = Convert.ToInt32(fila["CANTIDAD_SUGERIDA"]),
                        CantidadPedida = Convert.ToInt32(fila["CANTIDAD_PEDIDA"]),
                        CantidadRecibida = fila["CANTIDAD_RECIBIDA"] == DBNull.Value
                            ? (int?)null : Convert.ToInt32(fila["CANTIDAD_RECIBIDA"])
                    });
                }
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }

        // Filas afectadas; 0 = rechazado (estado distinto de Pendiente o RN-33).
        public int Autorizar(int pedidoId, int resolutorId)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@pedido_id",    pedidoId),
                _acceso.CrearParametro("@resolutor_id", resolutorId)
            };
            return EjecutarFilas("PEDIDO_REPOSICION_AUTORIZAR", parametros);
        }

        public int Rechazar(int pedidoId, int resolutorId, string motivo)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@pedido_id",    pedidoId),
                _acceso.CrearParametro("@resolutor_id", resolutorId),
                _acceso.CrearParametro("@motivo",       motivo)
            };
            return EjecutarFilas("PEDIDO_REPOSICION_RECHAZAR", parametros);
        }

        // cantidadesPorLinea: id de línea -> cantidad recibida. La transacción vive en el SP.
        public int Recibir(int pedidoId, int recibidoPor, string responsableLogin, IDictionary<int, int> cantidadesPorLinea)
        {
            DataTable tvp = new DataTable();
            tvp.Columns.Add("LINEA_ID", typeof(int));
            tvp.Columns.Add("CANTIDAD", typeof(int));
            foreach (KeyValuePair<int, int> kv in cantidadesPorLinea)
                tvp.Rows.Add(kv.Key, kv.Value);

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@pedido_id",         pedidoId),
                _acceso.CrearParametro("@recibido_por",      recibidoPor),
                _acceso.CrearParametro("@responsable_login", responsableLogin),
                new SqlParameter("@recepcion", SqlDbType.Structured) { TypeName = "dbo.PEDIDO_RECEPCION_TT", Value = tvp }
            };
            return EjecutarFilas("PEDIDO_REPOSICION_RECIBIR", parametros);
        }

        private int EjecutarFilas(string sp, List<SqlParameter> parametros)
        {
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer(sp, parametros);
                return Convert.ToInt32(tabla.Rows[0]["FILAS"]);
            }
            finally { _acceso.Cerrar(); }
        }

        private PedidoReposicion MapearCabecera(DataRow fila)
        {
            return new PedidoReposicion
            {
                Id = Convert.ToInt32(fila["ID"]),
                BodegaId = Convert.ToInt32(fila["BODEGA_ID"]),
                BodegaNombre = fila["BODEGA_NOMBRE"].ToString(),
                Estado = (EstadoPedidoReposicion)Enum.Parse(typeof(EstadoPedidoReposicion), fila["ESTADO"].ToString()),
                FechaCreacion = Convert.ToDateTime(fila["FECHA_CREACION"]),
                CreadoPor = Convert.ToInt32(fila["CREADO_POR"]),
                CreadoPorLogin = fila["CREADO_POR_LOGIN"].ToString(),
                ResolutorId = fila["RESOLUTOR_ID"] == DBNull.Value ? (int?)null : Convert.ToInt32(fila["RESOLUTOR_ID"]),
                ResolutorLogin = fila["RESOLUTOR_LOGIN"] == DBNull.Value ? null : fila["RESOLUTOR_LOGIN"].ToString(),
                FechaResolucion = fila["FECHA_RESOLUCION"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(fila["FECHA_RESOLUCION"]),
                MotivoRechazo = fila["MOTIVO_RECHAZO"] == DBNull.Value ? null : fila["MOTIVO_RECHAZO"].ToString(),
                RecibidoPor = fila["RECIBIDO_POR"] == DBNull.Value ? (int?)null : Convert.ToInt32(fila["RECIBIDO_POR"]),
                RecibidoPorLogin = fila["RECIBIDO_POR_LOGIN"] == DBNull.Value ? null : fila["RECIBIDO_POR_LOGIN"].ToString(),
                FechaRecepcion = fila["FECHA_RECEPCION"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(fila["FECHA_RECEPCION"]),
                Observacion = fila["OBSERVACION"] == DBNull.Value ? null : fila["OBSERVACION"].ToString(),
                CantidadLineas = Convert.ToInt32(fila["CANTIDAD_LINEAS"]),
                // La suma llega como BIGINT; se acota para que la propiedad int nunca desborde.
                UnidadesPedidas = (int)Math.Min(int.MaxValue, Convert.ToInt64(fila["UNIDADES_PEDIDAS"]))
            };
        }
    }
}
