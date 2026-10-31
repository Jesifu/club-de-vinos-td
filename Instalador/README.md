# Club de Vinos installer (A01)

Simple MSI installer built with WiX Toolset 7. Output: `bin\x64\Release\es-ES\ClubDeVinos.msi` (installer UI in Spanish, es-ES).

## What it installs

- `CAPAS.exe` and its runtime files to `C:\Program Files\Club de Vinos` (per-machine, 64-bit), with Start Menu and desktop shortcuts.
- Database `BDCAPAS` on the SQL Server instance chosen in the installer (Windows authentication), created if missing, then `DAL\script.sql` is executed.

## Prerequisites on the target machine

- Windows 10 or 11.
- .NET Framework 4.8.
- A local SQL Server instance, with the installing user allowed to create databases (Windows authentication). The installer lets the user pick the instance and stores it so the app connects to it (see "How the app finds the instance").

## How the app finds the instance

The installer writes the chosen instance to `HKLM\SOFTWARE\Club de Vinos`, value `SqlInstance` (REG_SZ, 64-bit view, component `SqlInstanceSetting` in the `Main` feature). It is written even with "Instalar solo la aplicación", because the script will be run later against that instance. It is removed on uninstall; the database is never dropped.

`DAL\Acceso.cs` resolves the server once (lazy static, never throws) in this order:

1. Environment variable `CLUBVINOS_SQL` (support override, e.g. `setx CLUBVINOS_SQL .\SQLEXPRESS`; restart the app).
2. Registry `HKLM\SOFTWARE\Club de Vinos\SqlInstance` (64-bit view, then 32-bit view).
3. `.` (default instance).

The connection keeps Windows authentication and the database name `BDCAPAS`. Upgrade/repair keeps the stored instance: `SQLINSTANCE` has no static default; a `RegistrySearch` (64-bit view) loads the stored value into `SQLINSTANCE_PREV`, and `SetProperty` actions (UI and execute sequences, right after `AppSearch`, before `DetectSqlInstances`) set `SQLINSTANCE` only while it is empty: first from `SQLINSTANCE_PREV`, otherwise `.`. Precedence: command line (`SQLINSTANCE=...`) or value typed in the dialog > stored registry value > `.`. So `msiexec /i ... /qn`, `/fa` or a major upgrade without the dialog no longer resets the value to `.`.

## SQL Server instance selection

A `SqlInstanceDlg` dialog sits between the install folder page and the final confirmation page (Back and Next are wired both ways; the override of the folder page's Next repeats the valid-path condition of `WixUI_InstallDir`, so an invalid folder still blocks the wizard).

- `Instalador\CustomActions` is a net472 class library using `WixToolset.Dtf.CustomAction` 7.0.0. It builds `CustomActions.CA.dll`, which `Instalador.wixproj` references and `Package.wxs` embeds as a `<Binary>`.
- The immediate custom action `DetectSqlInstances` runs in `InstallUISequence` (only on a fresh install). It merges the value names under `HKLM\SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL` (64-bit and 32-bit registry views) with the Windows services `MSSQLSERVER` / `MSSQL$*`, removes duplicates and inserts temporary rows into the `ComboBox` table for the `SQLINSTANCE` property.
- `MSSQLSERVER` maps to `.` (shown as "(local) - instancia predeterminada (MSSQLSERVER)"); any other name `N` maps to `.\N`. `SQLINSTANCE` is pre-filled with the stored value from a previous install (if any); otherwise it defaults to `.` when the default instance exists, otherwise to the first instance found. "Reintentar detección" never overwrites what the user typed: the value is only reset when it is empty or the new list is non-empty and does not contain it.
- `SqlDatabase` uses `Server="[SQLINSTANCE]"`, so `.\SQLEXPRESS` works without a separate `Instance`.
- The combo is editable: detected instances are listed, but any `server\instance` can be typed.
- Next runs `ValidateSqlInstance` (service state, then a Windows-auth login to `master`). Only a valid result advances; otherwise `SqlErrorDlg` (enlarged to fit ~500 characters) shows the Spanish reason and the user can fix it and press Next again.
- Worst-case wait on Next: about 35 s (service start up to 20 s, then 2 connection attempts of 5 s timeout with a 3 s pause = 13 s). The dialog UI is blocked meanwhile; a status text ("Validando..." / "Probando la conexión...") is sent to the `ValidationStatus` control through `ActionText` (best effort, may not repaint while blocked).
- If the UI phase is not elevated and the service is stopped (access denied), validation passes and the elevated `EnsureSqlService` (before `InstallFiles`) starts the service **and** tests the login with the same account `SqlDatabase` will use (SYSTEM, `Impersonate="no"`; on a remote server, the machine account). On failure it shows a Spanish Retry/Cancel box; Cancel rolls back before any file is copied.
- Version 1.5.0.0: the three special cases below (A01 final deliverable).

## Special cases (A01)

| # | Case | How it is detected | What the user sees | What the installer does | How to test (in a VM, never on the dev PC) |
|---|------|--------------------|--------------------|-------------------------|--------------------------------------------|
| 1 | Instance list could not be read, or only a remote server is used | Registry and/or services could not be read (exception or Service Control Manager access error), so the result is not trustworthy: `SQLINSTANCEFOUND` and `SQLENGINEMISSING` both empty. A genuinely empty but readable result is case 2, not this one | Message "No se pudo obtener la lista de instancias...", editable combo for `server\instance`, "Reintentar detección" button | Never aborts. The typed value is validated on Next (connection test); the retry button re-runs detection and rebuilds the dialog without overwriting the typed value | Hard to force in a VM: with SQL Express installed but the list unreadable (e.g. deny read on the `Instance Names\SQL` key and stop the Service Control Manager query via ACL), or simply use a remote server: type `HOST\INSTANCIA` (reachable) or a wrong name (`.\NOEXISTE`) and press Next to see the readable connection error. The unreadable-list path is otherwise covered by code only |
| 2 | SQL Server engine not installed | Registry (both views) AND services were read successfully and no instance name / no service `MSSQLSERVER` / `MSSQL$*` exists (`SQLENGINEMISSING=1`) | Message explaining that the database engine is required, with the SQL Server Express download guidance; checkbox "Instalar solo la aplicación"; "Reintentar detección" | Next is validated, so with an empty/invalid server the installation is blocked (Cancel leaves nothing installed). If the box is checked: `Database` feature goes to Level 0 (no `SqlDatabase`/`SqlScript`), app files plus `BaseDeDatos\script.sql` and `EjecutarScript.cmd` are installed | Clean Windows VM without SQL Server: run the MSI, confirm the message; type `.\NOEXISTE`, Next, read the error; check the box and finish, then verify no `BDCAPAS` and that `C:\Program Files\Club de Vinos\BaseDeDatos\` exists. Later install SQL Express and run `EjecutarScript.cmd .\SQLEXPRESS` (log in `%TEMP%\EjecutarScript_BDCAPAS.log`). Retry: install Express with the installer still open and press "Reintentar detección" |
| 3 | SQL service installed but stopped | Service state via `ServiceController` (`ValidateSqlInstance` on Next, and `EnsureSqlService` during execution) | If it starts: nothing, install continues. If not: "El servicio de SQL Server ... está detenido y no se pudo iniciar" with `services.msc` / `net start` steps (UI), or a Reintentar/Cancelar box (execution phase), which also appears when the login fails after the service is up | Starts the service and waits up to 20 s. Non-elevated UI (access denied) defers the start to the elevated deferred action `EnsureSqlService` (runs before `InstallFiles`), which also tests the login. Cancel there rolls back before any file is copied | VM with SQL Express: `net stop "MSSQL$SQLEXPRESS"` (as admin, inside the VM), run the MSI. (a) Elevated prompt: the UI starts it and continues. (b) Normal double-click: UAC prompt, then the service starts before files are copied. (c) Failure box: set the service startup to Disabled (`sc config "MSSQL$SQLEXPRESS" start= disabled`), run the MSI, read the message, fix with `sc config ... start= demand`, press Reintentar. (d) Login failure box: with the service running, remove the SYSTEM login (or its create-database right), run non-elevated or silent, read the message, restore it and press Reintentar |

General failure handling:

- Every custom action catches its exceptions and turns them into a Spanish message; nothing fails silently.
- A DB login problem is caught in the wizard (UI phase) or, when the UI could not test it, in `EnsureSqlService` before any file is copied, always with the SQL error text.
- Silent installs (`/qn`) do not run the UI sequence, but `EnsureSqlService` still runs (`SQLINSTANCE` = value passed on the command line, else the one stored by a previous install, else `.`). With no SQL Server installed, a silent install fails there (service not found / login failure) unless `SQLSKIPDB=1` is passed. Note that a Retry/Cancel box in a silent install is auto-answered by Windows Installer (the install fails and rolls back).
- If `SqlDatabase` / `SqlScript` still fail during execution (script error), Windows Installer rolls back the whole install and the WiX SQL extension raises its own message box (English text from the extension). Check `msiexec /i ClubDeVinos.msi /l*v install.log` for the details.
- LocalDB is not detected; it can be typed (`(localdb)\MSSQLLocalDB`) but it is not validated for the WiX SQL actions.
- `EjecutarScript.cmd` writes the `sqlcmd` output to `%TEMP%\EjecutarScript_BDCAPAS.log`, ignores the known harmless "already exists" errors (SQL error numbers 2714, 1913, 1779, 1781), prints a warning pointing to the log when other errors appear (exit code 3) and reports a missing `sqlcmd` explicitly (exit code 9).

## Installer language (es-ES)

The installer UI is in Spanish (es-ES, LCID 3082, codepage 1252).

- `Instalador.wixproj` sets `<Cultures>es-ES</Cultures>`: the SDK takes the Spanish WixUI strings from the extension, and the MSI is emitted in `bin\x64\Release\es-ES\`.
- Our own strings (`SqlInstanceDlg`, downgrade message) live in `Package.es-es.wxl` and are referenced with `!(loc.Id)` from `Package.wxs`. To add a language, add another `.wxl` with its `Culture` and list it in `<Cultures>`.
- The instance combo text comes from `SqlInstanceMapper` (Spanish); the values (`.`, `.\NAME`) are unchanged.

## Build

Requirements: Visual Studio (MSBuild), .NET SDK, and the WiX CLI (`dotnet tool install --global wix --version 7.0.0`). WiX 7 requires accepting the OSMF EULA once per machine: `wix eula accept wix7`.

```powershell
.\Instalador\build.ps1
```

The script builds `TP_IS.sln` in Release with MSBuild (found through `vswhere`) and then builds `Instalador.wixproj`.

## Notes

- `Instalador.wixproj` and `Instalador\CustomActions` are intentionally NOT part of `TP_IS.sln`, so the solution still opens without the WiX extension for Visual Studio.
- `DAL\script.sql` is UTF-8 with BOM, and the WiX SQL custom action does not decode UTF-8. The build converts it to UTF-16 LE (`Instalador\obj\script.utf16.sql`); the source file is never modified.
- Uninstall keeps the `BDCAPAS` database on purpose (it is never dropped).
- The script is re-run on every install or upgrade; it is idempotent.
- At runtime the app writes remito PDFs to `Documents\ClubDeVinos\Remitos` and `integridad_error.log` to `%LocalAppData%\ClubDeVinos`, because Program Files is not writable by regular users.
