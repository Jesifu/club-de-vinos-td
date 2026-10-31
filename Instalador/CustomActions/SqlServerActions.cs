using System;
using WixToolset.Dtf.WindowsInstaller;

namespace ClubDeVinos.Installer.CustomActions
{
    /// <summary>
    /// Validaciones de SQL Server antes de instalar. Los textos al usuario viven en Package.es-es.wxl:
    /// Package.wxs los expone como propiedades SQLMSG_* con marcadores {0}/{1} que se completan aquí.
    /// Ninguna acción lanza excepciones: cualquier fallo se convierte en un mensaje legible.
    /// </summary>
    public static class SqlServerActions
    {
        /// <summary>
        /// CA inmediata de la UI (botón Siguiente de SqlInstanceDlg). Deja SQLVALID="1" si se puede continuar,
        /// o SQLERRORMSG con el motivo en español. Pasos:
        ///   1) "solo aplicación" (SQLSKIPDB) -> continúa sin validar.
        ///   2) instancia vacía -> error.
        ///   3) instancia local con servicio detenido -> intenta iniciarlo (20 s). Si no es administrador
        ///      (acceso denegado) se difiere: la CA elevada EnsureSqlService lo inicia y además prueba la conexión
        ///      (con Reintentar/Cancelar) antes de copiar archivos.
        ///   4) prueba de conexión con autenticación de Windows (2 intentos de 5 s) -> error legible si falla.
        /// Peor caso de espera: unos 35 s (20 s de inicio del servicio + 13 s de conexión).
        /// </summary>
        [CustomAction]
        public static ActionResult ValidateSqlInstance(Session session)
        {
            session["SQLVALID"] = "";
            session["SQLERRORMSG"] = "";
            try
            {
                if (session["SQLSKIPDB"] == "1")
                {
                    session.Log("ValidateSqlInstance: database skipped by user");
                    session["SQLVALID"] = "1";
                    return ActionResult.Success;
                }

                string server = (session["SQLINSTANCE"] ?? "").Trim();
                if (server.Length == 0)
                {
                    return Fail(session, "SQLMSG_EMPTY");
                }

                bool justStarted = false;
                ReportStatus(session, "Validando la instancia de SQL Server...");
                string service = SqlInstanceMapper.ServiceNameFor(server);
                if (service != null && SqlServiceHelper.Exists(service))
                {
                    string detail;
                    ServiceStartResult r = SqlServiceHelper.EnsureRunning(
                        service, TimeSpan.FromSeconds(SqlServiceHelper.StartTimeoutSeconds), out detail);
                    session.Log("ValidateSqlInstance: service " + service + " -> " + r + " " + detail);

                    switch (r)
                    {
                        case ServiceStartResult.Started:
                            justStarted = true;
                            break;
                        case ServiceStartResult.AccessDenied:
                            // Sin privilegios en la fase de UI: la CA diferida (elevada) iniciará el servicio.
                            session.Log("ValidateSqlInstance: start deferred to elevated phase");
                            session["SQLVALID"] = "1";
                            return ActionResult.Success;
                        case ServiceStartResult.Timeout:
                        case ServiceStartResult.Failed:
                        case ServiceStartResult.NotFound:
                            return Fail(session, "SQLMSG_STARTFAIL", service, SqlServiceHelper.StartTimeoutSeconds.ToString());
                    }
                }

                string error;
                ReportStatus(session, "Probando la conexión a " + server + "...");
                if (!SqlServiceHelper.TryConnect(server, justStarted ? SqlServiceHelper.ConnectAttempts : 1, out error))
                {
                    session.Log("ValidateSqlInstance: connect failed: " + error);
                    return Fail(session, "SQLMSG_CONNFAIL", server, error);
                }

                session["SQLVALID"] = "1";
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log("ValidateSqlInstance: unexpected error " + ex);
                return Fail(session, "SQLMSG_CONNFAIL", session["SQLINSTANCE"], SqlServiceHelper.Clean(ex.Message));
            }
        }

        /// <summary>
        /// CA inmediata de la secuencia de ejecución: calcula el nombre del servicio local y empaqueta los datos
        /// (incluidos los mensajes localizados) para la CA diferida EnsureSqlService.
        /// </summary>
        [CustomAction]
        public static ActionResult PrepareSqlService(Session session)
        {
            try
            {
                var data = new CustomActionData();
                if (session["SQLSKIPDB"] != "1")
                {
                    string server = (session["SQLINSTANCE"] ?? "").Trim();
                    string service = SqlInstanceMapper.ServiceNameFor(server);
                    data["server"] = server;
                    if (service != null)
                    {
                        data["svc"] = service;
                    }

                    data["msgFail"] = session["SQLMSG_STARTFAILRETRY"];
                    data["msgMissing"] = session["SQLMSG_NOSERVICE"];
                    data["msgConn"] = session["SQLMSG_CONNFAILRETRY"];
                }

                session["EnsureSqlService"] = data.ToString();
                session.Log("PrepareSqlService: server=" + (data.ContainsKey("server") ? data["server"] : "(none)"));
            }
            catch (Exception ex)
            {
                session.Log("PrepareSqlService: error " + ex);
                session["EnsureSqlService"] = "";
            }

            return ActionResult.Success;
        }

        /// <summary>
        /// CA diferida (elevada, programada antes de InstallFiles): inicia el servicio SQL local si está detenido y
        /// prueba el inicio de sesión (autenticación de Windows) contra el servidor elegido. Corre con Impersonate="no",
        /// es decir con la misma cuenta (SYSTEM) con la que luego actuará SqlDatabase, así que la prueba es fiel.
        /// Si algo falla muestra un cuadro Reintentar/Cancelar en español; Cancelar devuelve error y Windows Installer
        /// revierte todo (todavía no se copió ningún archivo, así que no queda una instalación a medias).
        /// No tiene acceso a las propiedades de la sesión: todo llega por CustomActionData.
        /// </summary>
        [CustomAction]
        public static ActionResult EnsureSqlService(Session session)
        {
            try
            {
                var data = session.CustomActionData;
                string server = data.ContainsKey("server") ? (data["server"] ?? "").Trim() : "";
                string service = data.ContainsKey("svc") ? data["svc"] : "";
                if (string.IsNullOrEmpty(service) && server.Length == 0)
                {
                    return ActionResult.Success;
                }

                while (true)
                {
                    string text = null;
                    bool serviceOk = true;

                    if (!string.IsNullOrEmpty(service))
                    {
                        string detail;
                        ServiceStartResult r = SqlServiceHelper.EnsureRunning(
                            service, TimeSpan.FromSeconds(SqlServiceHelper.StartTimeoutSeconds), out detail);
                        session.Log("EnsureSqlService: " + service + " -> " + r + " " + detail);

                        if (r == ServiceStartResult.Started)
                        {
                            // Margen para que el motor termine de recuperar las bases antes de conectar.
                            System.Threading.Thread.Sleep(5000);
                        }
                        else if (r != ServiceStartResult.AlreadyRunning)
                        {
                            serviceOk = false;
                            string template = r == ServiceStartResult.NotFound ? "msgMissing" : "msgFail";
                            text = Format(data.ContainsKey(template) ? data[template] : "", service,
                                SqlServiceHelper.StartTimeoutSeconds.ToString());
                            if (text.Length == 0)
                            {
                                text = "SQL Server service " + service + " could not be started: " + detail;
                            }
                        }
                    }

                    if (serviceOk && server.Length > 0)
                    {
                        string error;
                        if (SqlServiceHelper.TryConnect(server, SqlServiceHelper.ConnectAttempts, out error))
                        {
                            session.Log("EnsureSqlService: login to " + server + " OK");
                            return ActionResult.Success;
                        }

                        session.Log("EnsureSqlService: login to " + server + " failed: " + error);
                        text = Format(data.ContainsKey("msgConn") ? data["msgConn"] : "", server, error);
                        if (text.Length == 0)
                        {
                            text = "Could not connect to SQL Server " + server + ": " + error;
                        }
                    }
                    else if (serviceOk)
                    {
                        return ActionResult.Success;
                    }

                    using (var record = new Record(1))
                    {
                        record.FormatString = "[1]";
                        record[1] = text;
                        // MB_RETRYCANCEL (5) | MB_ICONERROR (0x10)
                        MessageResult result = session.Message(
                            (InstallMessage)((int)InstallMessage.Error | 5 | 0x10), record);
                        if (result != MessageResult.Retry)
                        {
                            return ActionResult.Failure;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                session.Log("EnsureSqlService: unexpected error " + ex);
                return ActionResult.Failure;
            }
        }

        /// <summary>Texto de estado en el diálogo (evento ActionText). Es opcional: cualquier fallo se ignora.</summary>
        private static void ReportStatus(Session session, string text)
        {
            try
            {
                using (var record = new Record(3))
                {
                    record[1] = "ValidateSqlInstance";
                    record[2] = text;
                    record[3] = "";
                    session.Message(InstallMessage.ActionStart, record);
                }
            }
            catch (Exception)
            {
            }
        }

        private static ActionResult Fail(Session session, string messageProperty, params string[] args)
        {
            session["SQLERRORMSG"] = Format(session[messageProperty], args);
            session["SQLVALID"] = "";
            return ActionResult.Success;
        }

        private static string Format(string template, params string[] args)
        {
            if (string.IsNullOrEmpty(template))
            {
                return "";
            }

            for (int i = 0; i < args.Length; i++)
            {
                template = template.Replace("{" + i + "}", args[i] ?? "");
            }

            return template;
        }
    }
}
