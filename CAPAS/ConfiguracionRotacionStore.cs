using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CAPAS
{
    // Persistencia de la configuración de rotación (N03) en un archivo XML serializado con
    // XmlSerializer. Vive en %LocalAppData%\ClubDeVinos (igual que el log de integridad):
    // la carpeta de la aplicación (Program Files) no es escribible para un usuario común.
    internal static class ConfiguracionRotacionStore
    {
        private const string NombreArchivo = "configuracion_rotacion.xml";

        internal static string Carpeta
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ClubDeVinos");
            }
        }

        internal static string Ruta
        {
            get { return Path.Combine(Carpeta, NombreArchivo); }
        }

        // Carga la configuración. Si el archivo no existe lo crea con los valores por defecto;
        // si está corrupto o es incoherente devuelve los valores por defecto sin lanzar
        // (archivoInvalido = true) y no pisa el archivo hasta que el usuario guarde.
        internal static BE.ConfiguracionRotacion Cargar(out bool archivoInvalido)
        {
            archivoInvalido = false;
            try
            {
                if (!File.Exists(Ruta))
                {
                    BE.ConfiguracionRotacion nueva = BE.ConfiguracionRotacion.PorDefecto();
                    Guardar(nueva);
                    return nueva;
                }

                XmlSerializer serializador = new XmlSerializer(typeof(BE.ConfiguracionRotacion));
                XmlDocument documento = new XmlDocument();
                using (FileStream fs = new FileStream(Ruta, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    documento.Load(fs);
                }

                BE.ConfiguracionRotacion config;
                using (XmlNodeReader lector = new XmlNodeReader(documento))
                {
                    config = serializador.Deserialize(lector) as BE.ConfiguracionRotacion;
                }

                // Archivo anterior sin MesesCoberturaObjetivo: se deriva de los umbrales guardados
                // (antes de validar) para no descartar una configuración personalizada que era válida.
                if (config != null && documento.SelectSingleNode("/ConfiguracionRotacion/MesesCoberturaObjetivo") == null)
                    config.MesesCoberturaObjetivo = DerivarCoberturaObjetivo(config);

                BLL.RotacionBLL.Validar(config);
                return config;
            }
            catch (Exception)
            {
                // Archivo corrupto, valores incoherentes o sin permisos: se sigue con los defaults.
                archivoInvalido = File.Exists(Ruta);
                return BE.ConfiguracionRotacion.PorDefecto();
            }
        }

        // Objetivo = min(Sobrestock, max(3, Reposición + 1)): queda mayor a Reposición y no
        // supera Sobrestock. Si Sobrestock <= Reposición el archivo es inválido de verdad y se
        // deja el valor tal cual para que Validar lo rechace.
        private static decimal DerivarCoberturaObjetivo(BE.ConfiguracionRotacion c)
        {
            if (c.MesesCoberturaSobrestock <= c.MesesCoberturaReposicion)
                return c.MesesCoberturaObjetivo;
            return Math.Min(c.MesesCoberturaSobrestock, Math.Max(3m, c.MesesCoberturaReposicion + 1m));
        }

        // Guardado atómico: serializa a un archivo temporal y luego lo reemplaza de una sola vez
        // (File.Replace), así el XML nunca queda truncado ni ausente si algo falla a mitad.
        // Propaga IOException/UnauthorizedAccessException: el caller decide cómo informarlo.
        internal static void Guardar(BE.ConfiguracionRotacion config)
        {
            Directory.CreateDirectory(Carpeta);
            string temporal = Ruta + "." + Guid.NewGuid().ToString("N") + ".tmp";

            try
            {
                XmlSerializer serializador = new XmlSerializer(typeof(BE.ConfiguracionRotacion));
                XmlWriterSettings ajustes = new XmlWriterSettings
                {
                    Indent = true,
                    Encoding = new UTF8Encoding(false)
                };
                using (XmlWriter escritor = XmlWriter.Create(temporal, ajustes))
                {
                    serializador.Serialize(escritor, config);
                }

                if (File.Exists(Ruta))
                    File.Replace(temporal, Ruta, null);
                else
                    File.Move(temporal, Ruta);
            }
            finally
            {
                // Si el reemplazo falló, no se deja basura en la carpeta.
                try
                {
                    if (File.Exists(temporal))
                        File.Delete(temporal);
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }
    }
}
