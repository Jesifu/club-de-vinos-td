using System;
using System.Collections.Generic;
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

            var skin = MaterialSkinManager.Instance;
            skin.Theme = MaterialSkinManager.Themes.DARK;
            // Barra de título y controles Material con la misma paleta bordó de AppTheme.
            skin.ColorScheme = new MaterialColorScheme(
                AppTheme.FondoHeader,
                AppTheme.FondoForm,
                AppTheme.Borde,
                AppTheme.Acento,
                MaterialTextShade.WHITE
            );

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
