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
                using (FileStream fs = new FileStream(Ruta, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BE.ConfiguracionRotacion config = serializador.Deserialize(fs) as BE.ConfiguracionRotacion;
                    BLL.RotacionBLL.Validar(config);
                    return config;
                }
            }
            catch (Exception)
            {
                // Archivo corrupto, valores incoherentes o sin permisos: se sigue con los defaults.
                archivoInvalido = File.Exists(Ruta);
                return BE.ConfiguracionRotacion.PorDefecto();
            }
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
