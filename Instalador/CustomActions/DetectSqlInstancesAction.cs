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
        /// CA inmediata de la secuencia de UI (se puede volver a ejecutar con "Reintentar detección").
        /// Detecta instancias locales por registro (vistas 64 y 32 bits) y por servicios de Windows
        /// (MSSQLSERVER, MSSQL$*), y las publica como filas temporales de la tabla ComboBox.
        ///   SQLINSTANCEFOUND = "1" si hay al menos una instancia (lista no vacía).
        ///   SQLENGINEMISSING = "1" solo si registro y servicios se leyeron sin error y no hay rastro del motor.
        /// Si alguna lectura falla (o hay una excepción), ambas quedan vacías: el diálogo ofrece
        /// escribir el servidor\instancia a mano en lugar de abortar la instalación.
        /// </summary>
        [CustomAction]
        public static ActionResult DetectSqlInstances(Session session)
        {
            session.Log("DetectSqlInstances: begin");
            session["SQLINSTANCEFOUND"] = "";
            session["SQLENGINEMISSING"] = "";
            try
            {
                var names = new List<string>();
                // La vista se indica de forma explícita: el host de la CA puede ser de 32 bits
                // y sin esto leería WOW6432Node en lugar de la vista nativa de 64 bits.
                bool regOk = ReadInstanceNames(RegistryView.Registry64, names);
                regOk &= ReadInstanceNames(RegistryView.Registry32, names);
                IList<string> serviceNames;
                bool svcOk = SqlServiceHelper.TryListSqlServiceNames(out serviceNames);
                names.AddRange(SqlInstanceMapper.InstanceNamesFromServices(serviceNames));

                IList<KeyValuePair<string, string>> items = SqlInstanceMapper.BuildItems(names);
                session.Log("DetectSqlInstances: " + items.Count + " instance(s) found");

                ClearComboRows(session);

                if (items.Count == 0)
                {
                    // Solo se afirma "motor no instalado" si registro Y servicios se leyeron bien y están vacíos.
                    // Si alguna lectura falló, ambas propiedades quedan vacías y el diálogo muestra "no se pudo
                    // obtener la lista" (se puede escribir la instancia a mano).
                    session.Log("DetectSqlInstances: empty result, registryRead=" + regOk + " servicesRead=" + svcOk);
                    if (regOk && svcOk)
                    {
                        session["SQLENGINEMISSING"] = "1";
                    }

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

                // En "Reintentar detección" no se pisa lo que escribió el usuario: solo se reemplaza el valor
                // si está vacío o si la lista nueva no lo contiene.
                string current = (session[ComboProperty] ?? "").Trim();
                bool listed = false;
                foreach (var item in items)
                {
                    if (string.Equals(item.Key, current, StringComparison.OrdinalIgnoreCase))
                    {
                        listed = true;
                        break;
                    }
                }

                if (current.Length == 0 || !listed)
                {
                    session[ComboProperty] = selected;
                }

                session["SQLINSTANCEFOUND"] = "1";
                session.Log("DetectSqlInstances: selected '" + session[ComboProperty] + "'");
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log("DetectSqlInstances: error " + ex);
                return ActionResult.Success;
            }
        }

        /// <summary>Agrega los nombres de la vista indicada. Devuelve false si el registro no se pudo leer (clave ausente = true).</summary>
        private static bool ReadInstanceNames(RegistryView view, List<string> into)
        {
            try
            {
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                using (RegistryKey key = baseKey.OpenSubKey(InstanceNamesKey))
                {
                    if (key != null)
                    {
                        into.AddRange(key.GetValueNames());
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>Elimina las filas temporales previas (permite re-ejecutar la detección sin violar la clave primaria).</summary>
        private static void ClearComboRows(Session session)
        {
            using (View view = session.Database.OpenView(
                "SELECT `Property`, `Order`, `Value`, `Text` FROM `ComboBox` WHERE `Property` = '" + ComboProperty + "'"))
            {
                view.Execute();
                Record record;
                while ((record = view.Fetch()) != null)
                {
                    using (record)
                    {
                        view.Modify(ViewModifyMode.Delete, record);
                    }
                }
            }
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
