using System;
using System.Collections.Generic;
using System.Linq;

namespace ClubDeVinos.Installer.CustomActions
{
    /// <summary>
    /// Lógica pura (sin dependencias de Windows Installer ni del registro) que convierte
    /// los nombres de instancia de SQL Server en filas para el ComboBox del instalador.
    /// </summary>
    public static class SqlInstanceMapper
    {
        public const string DefaultInstanceName = "MSSQLSERVER";
        public const string DefaultInstanceValue = ".";

        /// <summary>
        /// Convierte un nombre de instancia en el par (valor, texto).
        /// MSSQLSERVER es la instancia predeterminada y se conecta como ".".
        /// Cualquier otro nombre N se conecta como ".\N".
        /// </summary>
        public static KeyValuePair<string, string> Map(string instanceName)
        {
            if (string.Equals(instanceName, DefaultInstanceName, StringComparison.OrdinalIgnoreCase))
            {
                return new KeyValuePair<string, string>(
                    DefaultInstanceValue, "(local) - instancia predeterminada (" + DefaultInstanceName + ")");
            }

            string value = @".\" + instanceName;
            return new KeyValuePair<string, string>(value, value);
        }

        /// <summary>
        /// Elimina vacíos y duplicados (sin distinguir mayúsculas) y devuelve las filas ordenadas:
        /// primero la instancia predeterminada y luego las nombradas en orden alfabético.
        /// </summary>
        public static IList<KeyValuePair<string, string>> BuildItems(IEnumerable<string> instanceNames)
        {
            if (instanceNames == null)
            {
                return new List<KeyValuePair<string, string>>();
            }

            return instanceNames
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => string.Equals(n, DefaultInstanceName, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(n => n, StringComparer.OrdinalIgnoreCase)
                .Select(Map)
                .ToList();
        }

        /// <summary>
        /// Devuelve el nombre del servicio de Windows de una instancia LOCAL ("." -> MSSQLSERVER,
        /// ".\N" -> MSSQL$N). Devuelve null si el servidor es remoto o lleva puerto
        /// (no hay un servicio local que controlar).
        /// </summary>
        public static string ServiceNameFor(string server)
        {
            if (string.IsNullOrWhiteSpace(server))
            {
                return null;
            }

            string text = server.Trim();
            if (text.IndexOf(',') >= 0)
            {
                return null;
            }

            int slash = text.IndexOf('\\');
            string host = slash < 0 ? text : text.Substring(0, slash);
            string instance = slash < 0 ? DefaultInstanceName : text.Substring(slash + 1).Trim();

            bool local = host == "." ||
                string.Equals(host, "(local)", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(host, Environment.MachineName, StringComparison.OrdinalIgnoreCase);
            if (!local || instance.Length == 0)
            {
                return null;
            }

            return string.Equals(instance, DefaultInstanceName, StringComparison.OrdinalIgnoreCase)
                ? DefaultInstanceName
                : "MSSQL$" + instance;
        }

        /// <summary>
        /// Extrae nombres de instancia a partir de nombres de servicio de Windows
        /// (MSSQLSERVER -> MSSQLSERVER, MSSQL$X -> X). Ignora otros servicios y la base interna de Windows (WID).
        /// </summary>
        public static IList<string> InstanceNamesFromServices(IEnumerable<string> serviceNames)
        {
            var result = new List<string>();
            if (serviceNames == null)
            {
                return result;
            }

            foreach (string name in serviceNames)
            {
                if (name == null)
                {
                    continue;
                }

                if (string.Equals(name, DefaultInstanceName, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(DefaultInstanceName);
                }
                else if (name.StartsWith("MSSQL$", StringComparison.OrdinalIgnoreCase) &&
                         name.Length > 6 && name.IndexOf("##", StringComparison.Ordinal) < 0)
                {
                    result.Add(name.Substring(6));
                }
            }

            return result;
        }
    }
}
