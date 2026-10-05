using System;
using System.Collections.Generic;
using Microsoft.Win32;
using WixToolset.Dtf.WindowsInstaller;

namespace ClubDeVinos.Installer.CustomActions
{
    public static class DetectSqlInstancesAction
    {
        private const string InstanceNamesKey = @"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL";
        private const string ComboProperty = "SQLINSTANCE";

        /// <summary>
        /// CA inmediata de la secuencia de UI: detecta las instancias locales de SQL Server
        /// y las publica como filas temporales de la tabla ComboBox para la propiedad SQLINSTANCE.
        /// </summary>
        [CustomAction]
        public static ActionResult DetectSqlInstances(Session session)
        {
            session.Log("DetectSqlInstances: begin");
            try
            {
                var names = new List<string>();
                // La vista se indica de forma explícita: el host de la CA puede ser de 32 bits
                // y sin esto leería WOW6432Node en lugar de la vista nativa de 64 bits.
                names.AddRange(ReadInstanceNames(RegistryView.Registry64));
                names.AddRange(ReadInstanceNames(RegistryView.Registry32));

                IList<KeyValuePair<string, string>> items = SqlInstanceMapper.BuildItems(names);
                session.Log("DetectSqlInstances: " + items.Count + " instance(s) found");

                if (items.Count == 0)
                {
                    session["SQLINSTANCEFOUND"] = "";
                    return ActionResult.Success;
                }

                InsertComboRows(session, items);

                // Se prefiere la instancia predeterminada (valor "."); si no existe, la primera.
                string selected = items[0].Key;
                foreach (var item in items)
                {
                    if (item.Key == SqlInstanceMapper.DefaultInstanceValue)
                    {
                        selected = item.Key;
                        break;
                    }
                }

                session[ComboProperty] = selected;
                session["SQLINSTANCEFOUND"] = "1";
                session.Log("DetectSqlInstances: selected '" + selected + "'");
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                // Ante cualquier fallo se deja SQLINSTANCEFOUND vacío: el diálogo muestra
                // el mensaje de "no se encontró SQL Server" en lugar de abortar la instalación.
                session.Log("DetectSqlInstances: error " + ex);
                session["SQLINSTANCEFOUND"] = "";
                return ActionResult.Success;
            }
        }

        private static IEnumerable<string> ReadInstanceNames(RegistryView view)
        {
            try
            {
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                using (RegistryKey key = baseKey.OpenSubKey(InstanceNamesKey))
                {
                    if (key != null)
                    {
                        return key.GetValueNames();
                    }
                }
            }
            catch (Exception)
            {
                // Una vista inaccesible equivale a "sin instancias" en esa vista.
            }

            return new string[0];
        }

        private static void InsertComboRows(Session session, IList<KeyValuePair<string, string>> items)
        {
            using (View view = session.Database.OpenView(
                "SELECT `Property`, `Order`, `Value`, `Text` FROM `ComboBox`"))
            {
                view.Execute();
                int order = 1;
                foreach (var item in items)
                {
                    using (var record = new Record(4))
                    {
                        record[1] = ComboProperty;
                        record[2] = order++;
                        record[3] = item.Key;
                        record[4] = item.Value;
                        view.Modify(ViewModifyMode.InsertTemporary, record);
                    }
                }
            }
        }
    }
}
