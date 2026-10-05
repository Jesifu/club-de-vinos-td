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
    }
}
