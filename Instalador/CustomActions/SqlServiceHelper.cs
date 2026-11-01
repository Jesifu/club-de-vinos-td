using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.ServiceProcess;

namespace ClubDeVinos.Installer.CustomActions
{
    public enum ServiceStartResult
    {
        AlreadyRunning,
        Started,
        NotFound,
        AccessDenied,
        Timeout,
        Failed
    }

    /// <summary>
    /// Acceso al Administrador de servicios de Windows y prueba de conexión a SQL Server.
    /// </summary>
    public static class SqlServiceHelper
    {
        public const int StartTimeoutSeconds = 20;

        /// <summary>Intentos y espera de la prueba de conexión (peor caso: 2 x 5 s + 3 s de pausa = 13 s).</summary>
        public const int ConnectAttempts = 2;
        public const int ConnectTimeoutSeconds = 5;

        /// <summary>Nombres de servicio que empiezan con MSSQL (se filtran luego con el mapper). Vacía si no se pudo leer.</summary>
        public static IList<string> ListSqlServiceNames()
        {
            IList<string> names;
            TryListSqlServiceNames(out names);
            return names;
        }

        /// <summary>
        /// Igual que ListSqlServiceNames pero distingue "no se pudo leer el SCM" (false) de
        /// "se leyó y no hay servicios" (true con lista vacía).
        /// </summary>
        public static bool TryListSqlServiceNames(out IList<string> names)
        {
            var found = new List<string>();
            names = found;
            try
            {
                foreach (ServiceController sc in ServiceController.GetServices())
                {
                    using (sc)
                    {
                        if (sc.ServiceName.StartsWith("MSSQL", StringComparison.OrdinalIgnoreCase))
                        {
                            found.Add(sc.ServiceName);
                        }
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Exists(string serviceName)
        {
            foreach (string name in ListSqlServiceNames())
            {
                if (string.Equals(name, serviceName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Garantiza que el servicio esté en ejecución: lo inicia si está detenido y espera hasta el tiempo indicado.
        /// Nunca lanza excepciones; el resultado describe qué pasó.
        /// </summary>
        public static ServiceStartResult EnsureRunning(string serviceName, TimeSpan timeout, out string detail)
        {
            detail = "";
            try
            {
                using (var sc = new ServiceController(serviceName))
                {
                    ServiceControllerStatus status;
                    try
                    {
                        status = sc.Status;
                    }
                    catch (InvalidOperationException ex)
                    {
                        detail = ex.Message;
                        return IsAccessDenied(ex) ? ServiceStartResult.AccessDenied : ServiceStartResult.NotFound;
                    }

                    if (status == ServiceControllerStatus.Running)
                    {
                        return ServiceStartResult.AlreadyRunning;
                    }

                    if (status == ServiceControllerStatus.Stopped)
                    {
                        sc.Start();
                    }
                    else if (status == ServiceControllerStatus.Paused)
                    {
                        sc.Continue();
                    }

                    // StartPending / ContinuePending: solo se espera.
                    sc.WaitForStatus(ServiceControllerStatus.Running, timeout);
                    return ServiceStartResult.Started;
                }
            }
            catch (System.ServiceProcess.TimeoutException ex)
            {
                detail = ex.Message;
                return ServiceStartResult.Timeout;
            }
            catch (InvalidOperationException ex)
            {
                detail = ex.Message;
                return IsAccessDenied(ex) ? ServiceStartResult.AccessDenied : ServiceStartResult.Failed;
            }
            catch (Exception ex)
            {
                detail = ex.Message;
                return ServiceStartResult.Failed;
            }
        }

        /// <summary>Prueba un inicio de sesión con autenticación de Windows contra master. Nunca lanza.</summary>
        public static bool TryConnect(string server, int attempts, out string error)
        {
            error = "";
            for (int i = 0; i < Math.Max(1, attempts); i++)
            {
                try
                {
                    var csb = new SqlConnectionStringBuilder
                    {
                        DataSource = server,
                        InitialCatalog = "master",
                        IntegratedSecurity = true,
                        ConnectTimeout = ConnectTimeoutSeconds
                    };
                    using (var conn = new SqlConnection(csb.ConnectionString))
                    {
                        conn.Open();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    error = Clean(ex.Message);
                    if (i + 1 < attempts)
                    {
                        System.Threading.Thread.Sleep(3000);
                    }
                }
            }

            return false;
        }

        private static bool IsAccessDenied(InvalidOperationException ex)
        {
            var win = ex.InnerException as Win32Exception;
            return win != null && win.NativeErrorCode == 5;
        }

        private const int MaxDetailLength = 260;

        /// <summary>
        /// Quita corchetes y saltos de línea (rompen el formato de Windows Installer), descarta el sufijo técnico
        /// "(provider: ..., error: ...)" y acota el largo cortando en el último punto completo (nunca a mitad de
        /// frase). El resultado siempre termina en punto, así que las plantillas no deben agregar otro.
        /// </summary>
        public static string Clean(string text)
        {
            if (text == null)
            {
                return "";
            }

            text = text.Replace('[', '(').Replace(']', ')').Replace("\r", " ").Replace("\n", " ").Trim();
            while (text.Contains("  "))
            {
                text = text.Replace("  ", " ");
            }

            int provider = text.IndexOf("(provider:", StringComparison.OrdinalIgnoreCase);
            if (provider > 0)
            {
                text = text.Substring(0, provider).Trim();
            }

            if (text.Length > MaxDetailLength)
            {
                string head = text.Substring(0, MaxDetailLength);
                int stop = head.LastIndexOf(". ", StringComparison.Ordinal);
                if (stop > 40)
                {
                    text = head.Substring(0, stop + 1);
                }
                else
                {
                    int space = head.LastIndexOf(' ');
                    text = (space > 40 ? head.Substring(0, space) : head).TrimEnd(' ', ',', ';', ':') + "...";
                }
            }

            if (text.Length > 0 && !text.EndsWith(".", StringComparison.Ordinal))
            {
                text += ".";
            }

            return text;
        }
    }
}
