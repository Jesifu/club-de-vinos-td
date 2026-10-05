using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class SolicitudAutorizacionDAL
    {
        private readonly Acceso _acceso = new Acceso();

        public List<BE.SolicitudAutorizacion> ListarPendientes(BE.TipoSolicitud tipo)
        {
            List<BE.SolicitudAutorizacion> lista = new List<BE.SolicitudAutorizacion>();
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@tipo", TipoADb(tipo))
            };
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("SOLICITUD_LISTAR_PENDIENTES", parametros);
                foreach (DataRow fila in tabla.Rows)
                    lista.Add(MapearFila(fila));
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }

        public BE.SolicitudAutorizacion ObtenerPorId(int id)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@id", id)
            };
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("SOLICITUD_OBTENER_POR_ID", parametros);
                return tabla.Rows.Count > 0 ? MapearFila(tabla.Rows[0]) : null;
            }
            finally { _acceso.Cerrar(); }
        }

        // La solicitud Pendiente de un tipo para un vino (a lo sumo una, por el índice único de la base).
        public BE.SolicitudAutorizacion ObtenerPendiente(int vinoId, BE.TipoSolicitud tipo)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@vino_id", vinoId),
                _acceso.CrearParametro("@tipo", TipoADb(tipo))
            };
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("SOLICITUD_OBTENER_PENDIENTE", parametros);
                return tabla.Rows.Count > 0 ? MapearFila(tabla.Rows[0]) : null;
            }
            finally { _acceso.Cerrar(); }
        }

        // Las cuatro operaciones de resolución devuelven FILAS (0 = rechazado por la base: RN-01,
        // solicitud ya resuelta, motivo vacío o RN-15 en la baja).
        public int AprobarAlta(int solicitudId, int resolutorId)
        {
            return Resolver("SOLICITUD_APROBAR_ALTA", solicitudId, resolutorId, null, false);
        }

        public int RechazarAlta(int solicitudId, int resolutorId, string motivo)
        {
            return Resolver("SOLICITUD_RECHAZAR_ALTA", solicitudId, resolutorId, motivo, true);
        }

        public int AprobarBaja(int solicitudId, int resolutorId)
        {
            return Resolver("SOLICITUD_APROBAR_BAJA", solicitudId, resolutorId, null, false);
        }

        public int RechazarBaja(int solicitudId, int resolutorId, string motivo)
        {
            return Resolver("SOLICITUD_RECHAZAR_BAJA", solicitudId, resolutorId, motivo, true);
        }

        private int Resolver(string sp, int solicitudId, int resolutorId, string motivo, bool conMotivo)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@solicitud_id", solicitudId),
                _acceso.CrearParametro("@resolutor_id", resolutorId)
            };
            if (conMotivo)
            {
                // VarChar(200) manual: Acceso.CrearParametro no controla longitud ni NULL.
                parametros.Add(new SqlParameter("@motivo", SqlDbType.VarChar, 200)
                {
                    Value = (object)motivo ?? DBNull.Value
                });
            }
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer(sp, parametros);
                return Convert.ToInt32(tabla.Rows[0]["FILAS"]);
            }
            finally { _acceso.Cerrar(); }
        }

        private static string TipoADb(BE.TipoSolicitud tipo)
        {
            return tipo == BE.TipoSolicitud.Alta ? "ALTA" : "BAJA";
        }

        internal static BE.TipoSolicitud TipoDesdeDb(string valor)
        {
            // Estricto: un valor desconocido (CK_SOLAUT_TIPO lo impide) no se interpreta como Baja.
            switch (valor)
            {
                case "ALTA": return BE.TipoSolicitud.Alta;
                case "BAJA": return BE.TipoSolicitud.Baja;
                default:
                    throw new InvalidOperationException("Tipo de solicitud desconocido: '" + valor + "'.");
            }
        }

        private BE.SolicitudAutorizacion MapearFila(DataRow fila)
        {
            BE.SolicitudAutorizacion s = new BE.SolicitudAutorizacion
            {
                Id = Convert.ToInt32(fila["ID"]),
                Tipo = TipoDesdeDb(fila["TIPO"].ToString()),
                VinoId = Convert.ToInt32(fila["VINO_ID"]),
                VinoCodigo = fila["CODIGO"].ToString(),
                VinoNombre = fila["NOMBRE"].ToString(),
                BodegaNombre = fila["BODEGA_NOMBRE"].ToString(),
                Estado = (BE.EstadoSolicitud)Enum.Parse(typeof(BE.EstadoSolicitud), fila["ESTADO"].ToString()),
                EmisorId = Convert.ToInt32(fila["EMISOR_ID"]),
                EmisorLogin = fila["EMISOR_LOGIN"].ToString(),
                FechaEmision = Convert.ToDateTime(fila["FECHA_EMISION"]),
                ResolutorId = fila["RESOLUTOR_ID"] == DBNull.Value ? (int?)null : Convert.ToInt32(fila["RESOLUTOR_ID"]),
                ResolutorLogin = fila["RESOLUTOR_LOGIN"] == DBNull.Value ? null : fila["RESOLUTOR_LOGIN"].ToString(),
                FechaResolucion = fila["FECHA_RESOLUCION"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(fila["FECHA_RESOLUCION"]),
                MotivoRechazo = fila["MOTIVO_RECHAZO"] == DBNull.Value ? null : fila["MOTIVO_RECHAZO"].ToString(),
                StockActual = Convert.ToInt32(fila["STOCK_ACTUAL"])
            };

            s.Vino = new BE.Vino
            {
                Id = s.VinoId,
                Codigo = s.VinoCodigo,
                Nombre = s.VinoNombre,
                BodegaId = Convert.ToInt32(fila["BODEGA_ID"]),
                BodegaNombre = s.BodegaNombre,
                Varietal = fila["VARIETAL"].ToString(),
                Aniada = Convert.ToInt32(fila["ANIADA"]),
                Precio = Convert.ToDecimal(fila["PRECIO"]),
                StockMinimo = Convert.ToInt32(fila["STOCK_MINIMO"]),
                Estado = (BE.EstadoVino)Enum.Parse(typeof(BE.EstadoVino), fila["VINO_ESTADO"].ToString()),
                Maridaje = fila["MARIDAJE"] == DBNull.Value ? null : fila["MARIDAJE"].ToString(),
                Puntaje = fila["PUNTAJE"] == DBNull.Value ? (int?)null : Convert.ToInt32(fila["PUNTAJE"]),
                CreadoPor = Convert.ToInt32(fila["CREADO_POR"]),
                FechaAlta = Convert.ToDateTime(fila["FECHA_ALTA"])
            };
            return s;
        }
    }
}
