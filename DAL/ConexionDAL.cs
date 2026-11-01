using System;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Prueba liviana de conexión a BDCAPAS. Usa el mismo servidor que el resto de la DAL
    /// (CLUBVINOS_SQL > registro del instalador > ".") pero con un tiempo de espera corto,
    /// para que un servidor inexistente falle rápido. No muestra nada al usuario.
    /// </summary>
    public class ConexionDAL
    {
        private const int TiempoEsperaSegundos = 5;

        public BE.ResultadoConexion Probar()
        {
            string servidor = Acceso.Servidor;
            try
            {
                var csb = new SqlConnectionStringBuilder(Acceso.CadenaConexionPublica)
                {
                    ConnectTimeout = TiempoEsperaSegundos
                };
                using (var conn = new SqlConnection(csb.ConnectionString))
                {
                    conn.Open();
                }
                return new BE.ResultadoConexion { Exitosa = true, Servidor = servidor, Motivo = "" };
            }
            catch (Exception ex)
            {
                return new BE.ResultadoConexion { Exitosa = false, Servidor = servidor, Motivo = MotivoBreve(ex) };
            }
        }

        /// <summary>True si la excepción (o alguna interna) indica que no hay acceso a la base de datos.</summary>
        public static bool EsErrorDeConexion(Exception ex)
        {
            for (int i = 0; ex != null && i < 10; i++, ex = ex.InnerException)
            {
                var sql = ex as SqlException;
                if (sql == null) continue;
                if (sql.Class >= 20) return true;
                switch (sql.Number)
                {
                    case -2: case -1: case 2: case 53: case 64: case 26: case 40: case 233: case 258:
                    case 4060: case 18456: case 18452: case 10053: case 10054: case 10060: case 10061:
                    case 11001:
                        return true;
                }
            }
            return false;
        }

        private static string MotivoBreve(Exception ex)
        {
            SqlException sql = null;
            for (Exception e = ex; e != null && sql == null; e = e.InnerException)
                sql = e as SqlException;

            if (sql != null)
            {
                switch (sql.Number)
                {
                    case 4060:
                        return "la base de datos BDCAPAS no existe o su usuario de Windows no tiene acceso a ella.";
                    case 18456:
                    case 18452:
                        return "SQL Server rechazó el inicio de sesión de su usuario de Windows.";
                    case -2:
                        return "se agotó el tiempo de espera de la conexión.";
                    case -1:
                    case 2:
                    case 53:
                    case 26:
                    case 40:
                    case 258:
                    case 10060:
                    case 10061:
                    case 11001:
                        return "no se encontró el servidor o el servicio de SQL Server no está iniciado.";
                }
            }

            string msg = (ex.Message ?? "").Replace("\r", " ").Replace("\n", " ").Trim();
            int punto = msg.IndexOf(". ", StringComparison.Ordinal);
            if (punto > 0) msg = msg.Substring(0, punto + 1);
            if (msg.Length > 200) msg = msg.Substring(0, 200) + "...";
            return msg.Length == 0 ? "error desconocido de conexión." : msg;
        }
    }
}
