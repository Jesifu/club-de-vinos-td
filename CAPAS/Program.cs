using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ReaLTaiizor.Colors;
using ReaLTaiizor.Manager;
using ReaLTaiizor.Util;

namespace CAPAS
{
    static class Program
    {
        internal static BLL.ResultadoIntegridad ResultadoIntegridad { get; private set; }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Esquema neutro con los colores del sistema: la barra de título Material usa el color de
            // selección de Windows y el resto de la interfaz queda con los controles por defecto.
            var skin = MaterialSkinManager.Instance;
            skin.Theme = MaterialSkinManager.Themes.LIGHT;
            skin.ColorScheme = new MaterialColorScheme(
                SystemColors.Highlight,
                SystemColors.HotTrack,
                SystemColors.ControlLight,
                SystemColors.Highlight,
                MaterialTextShade.WHITE
            );

            ErrorInicio.Instalar();
            // Sin base de datos alcanzable no tiene sentido seguir: se informa y se sale (o se reintenta).
            if (!ErrorInicio.VerificarConexion())
                return;

            InicializarIntegridad();
            Application.Run(new LogIn());
        }

        private static void InicializarIntegridad()
        {
            try
            {
                BLL.IntegridadBLL integridadBLL = new BLL.IntegridadBLL();
                if (!integridadBLL.EstaInicializado())
                    integridadBLL.RecalcularIntegridad();
                ResultadoIntegridad = integridadBLL.VerificarIntegridad();
            }
            catch (Exception ex)
            {
                ResultadoIntegridad = new BLL.ResultadoIntegridad
                {
                    EsValido = false,
                    Errores  = new List<string> { ex.ToString() }
                };
            }
        }

        internal static void GuardarLogIntegridad(List<string> errores)
        {
            try
            {
                // %LocalAppData%\ClubDeVinos: la carpeta de la aplicación (Program Files) no es escribible.
                string carpeta = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ClubDeVinos");
                Directory.CreateDirectory(carpeta);
                string ruta = Path.Combine(carpeta, "integridad_error.log");
                using (StreamWriter sw = new StreamWriter(ruta, append: true))
                {
                    sw.WriteLine($"=== {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
                    foreach (string error in errores)
                        sw.WriteLine(error);
                    sw.WriteLine();
                }
            }
            catch { }
        }
    }
}
