# Club de Vinos installer (A01)

Simple MSI installer built with WiX Toolset 7. Output: `bin\x64\Release\es-ES\ClubDeVinos.msi` (installer UI in Spanish, es-ES).

## What it installs

- `CAPAS.exe` and its runtime files to `C:\Program Files\Club de Vinos` (per-machine, 64-bit), with Start Menu and desktop shortcuts.
- Database `BDCAPAS` on the SQL Server instance chosen in the installer (Windows authentication), created if missing, then `DAL\script.sql` is executed.

## Prerequisites on the target machine

- Windows 10 or 11.
- .NET Framework 4.8.
- A local SQL Server instance, with the installing user allowed to create databases (Windows authentication). The installer lets the user pick the instance, but the connection string is still hardcoded to the default instance (`.`) in `DAL\Acceso.cs`, so the app only connects if the default instance was chosen (or the connection string is changed).

## SQL Server instance selection

A `SqlInstanceDlg` dialog sits between the install folder page and the final confirmation page (Back and Next are wired both ways; the override of the folder page's Next repeats the valid-path condition of `WixUI_InstallDir`, so an invalid folder still blocks the wizard).

- `Instalador\CustomActions` is a net472 class library using `WixToolset.Dtf.CustomAction` 7.0.0. It builds `CustomActions.CA.dll`, which `Instalador.wixproj` references and `Package.wxs` embeds as a `<Binary>`.
- The immediate custom action `DetectSqlInstances` runs in `InstallUISequence` (only on a fresh install). It reads the value names under `HKLM\SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL` in both the 64-bit and 32-bit registry views, removes duplicates and inserts temporary rows into the `ComboBox` table for the `SQLINSTANCE` property.
- `MSSQLSERVER` maps to `.` (shown as "(local) - instancia predeterminada (MSSQLSERVER)"); any other name `N` maps to `.\N`. `SQLINSTANCE` defaults to `.` when the default instance exists, otherwise to the first instance found.
- `SqlDatabase` uses `Server="[SQLINSTANCE]"`, so `.\SQLEXPRESS` works without a separate `Instance`.
- If no instance is found (`SQLINSTANCEFOUND` empty) the dialog shows a message saying SQL Server must be installed first and disables Next; only Back and Cancel are available.
- Silent installs do not run the UI sequence: they use the `SQLINSTANCE` default (`.`) or a value passed on the command line.

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
