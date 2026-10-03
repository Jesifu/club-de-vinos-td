# Club de Vinos installer (A01)

Simple MSI installer built with WiX Toolset 7. Output: `bin\x64\Release\ClubDeVinos.msi`.

## What it installs

- `CAPAS.exe` and its runtime files to `C:\Program Files\Club de Vinos` (per-machine, 64-bit), with Start Menu and desktop shortcuts.
- Database `BDCAPAS` on the default local SQL Server instance (`.`, Windows authentication), created if missing, then `DAL\script.sql` is executed.

## Prerequisites on the target machine

- Windows 10 or 11.
- .NET Framework 4.8.
- SQL Server running as the default instance (`.`), with the installing user allowed to create databases (Windows authentication). The connection string is hardcoded in `DAL\Acceso.cs`.

## Build

Requirements: Visual Studio (MSBuild), .NET SDK, and the WiX CLI (`dotnet tool install --global wix --version 7.0.0`). WiX 7 requires accepting the OSMF EULA once per machine: `wix eula accept wix7`.

```powershell
.\Instalador\build.ps1
```

The script builds `TP_IS.sln` in Release with MSBuild (found through `vswhere`) and then builds `Instalador.wixproj`.

## Notes

- `Instalador.wixproj` is intentionally NOT part of `TP_IS.sln`, so the solution still opens without the WiX extension for Visual Studio.
- `DAL\script.sql` is UTF-8 with BOM, and the WiX SQL custom action does not decode UTF-8. The build converts it to UTF-16 LE (`Instalador\obj\script.utf16.sql`); the source file is never modified.
- Uninstall keeps the `BDCAPAS` database on purpose (it is never dropped).
- The script is re-run on every install or upgrade; it is idempotent.
- At runtime the app writes remito PDFs to `Documents\ClubDeVinos\Remitos` and `integridad_error.log` to `%LocalAppData%\ClubDeVinos`, because Program Files is not writable by regular users.
