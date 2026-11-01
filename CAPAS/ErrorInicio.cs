using System;
using System.Threading;
using System.Windows.Forms;

namespace CAPAS
{
    /// <summary>
    /// Mensajes de arranque y manejo global de excepciones. Los textos están fijos en español
    /// a propósito: las traducciones viven en la base de datos, que puede no estar disponible.
    /// </summary>
    internal static class ErrorInicio
    {
        private const string Titulo = "Club de Vinos";
        private static bool _mostrando;

        internal static void Instalar()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => Manejar(e.Exception, false);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => Manejar(e.ExceptionObject as Exception, true);
        }

        /// <summary>
        /// Prueba la conexión a la base de datos. Si falla, informa y deja elegir Reintentar o Cancelar.
        /// Devuelve true si se puede continuar.
        /// </summary>
        internal static bool VerificarConexion()
        {
            var negocio = new BLL.ConexionBLL();
            while (true)
            {
                BE.ResultadoConexion r = negocio.ProbarConexion();
                if (r.Exitosa) return true;

                DialogResult d = MessageBox.Show(
                    ArmarMensajeConexion(r.Servidor, r.Motivo),
                    Titulo, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                if (d != DialogResult.Retry) return false;
            }
        }

        private static string ArmarMensajeConexion(string servidor, string motivo)
        {
            return "No se pudo conectar con la base de datos de Club de Vinos." + Environment.NewLine + Environment.NewLine
                 + "Servidor o instancia probado: " + servidor + Environment.NewLine
                 + "Motivo: " + motivo + Environment.NewLine + Environment.NewLine
                 + "Qué puede hacer:" + Environment.NewLine
                 + "1. Verifique que SQL Server esté instalado y que su servicio esté iniciado (services.msc)." + Environment.NewLine
                 + "2. Verifique que la base de datos BDCAPAS esté creada: ejecute el script de la carpeta BaseDeDatos "
                 + "dentro de la carpeta de instalación (consulte el archivo README del instalador o la guía de operación)." + Environment.NewLine
                 + "3. Si usa otra instancia, defínala en la variable de entorno CLUBVINOS_SQL (servidor\\instancia)." + Environment.NewLine + Environment.NewLine
                 + "Presione Reintentar para volver a comprobar o Cancelar para salir.";
        }

        private static void Manejar(Exception ex, bool terminando)
        {
            if (_mostrando) return;
            _mostrando = true;
            try
            {
                if (ex != null && BLL.ConexionBLL.EsErrorDeConexion(ex))
                {
                    var r = new BLL.ConexionBLL().ProbarConexion();
                    string servidor = r.Servidor;
                    string motivo = r.Exitosa ? "se perdió la conexión con la base de datos." : r.Motivo;
                    MessageBox.Show(ArmarMensajeConexion(servidor, motivo).Replace(
                            "Presione Reintentar para volver a comprobar o Cancelar para salir.",
                            terminando ? "La aplicación se cerrará." : "Corrija el problema y repita la operación."),
                        Titulo, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(
                        "Ocurrió un error inesperado." + Environment.NewLine
                        + (terminando ? "La aplicación se cerrará." : "Puede intentar la operación nuevamente; si el problema continúa, reinicie la aplicación y avise al administrador.")
                        + Environment.NewLine + Environment.NewLine
                        + "Detalle: " + (ex == null ? "desconocido" : ex.Message),
                        Titulo, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch { }
            finally
            {
                _mostrando = false;
            }

            if (terminando) Environment.Exit(1);
        }
    }
}
