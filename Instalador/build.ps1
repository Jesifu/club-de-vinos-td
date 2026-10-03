# Compila la solución en Release con MSBuild y luego genera el MSI.
# Uso: .\build.ps1   (desde cualquier carpeta)
$ErrorActionPreference = 'Stop'

$raiz = Split-Path -Parent $PSScriptRoot
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path $vswhere)) { throw 'vswhere.exe no encontrado: instale Visual Studio con la carga de trabajo de MSBuild.' }

$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (-not $msbuild) { throw 'No se encontró MSBuild.exe con vswhere.' }
Write-Host "MSBuild: $msbuild"

# 1) Solución completa (proyectos .NET Framework: no se compilan con dotnet CLI).
& $msbuild (Join-Path $raiz 'TP_IS.sln') -restore -p:Configuration=Release -v:minimal -nologo
if ($LASTEXITCODE -ne 0) { throw "Falló la compilación de la solución (código $LASTEXITCODE)." }

# 2) Instalador MSI (requiere la herramienta 'wix' y haber aceptado la EULA: wix eula accept wix7).
& dotnet build (Join-Path $PSScriptRoot 'Instalador.wixproj') -c Release -nologo
if ($LASTEXITCODE -ne 0) { throw "Falló la compilación del MSI (código $LASTEXITCODE)." }

$msi = Join-Path $PSScriptRoot 'bin\x64\Release\ClubDeVinos.msi'
Write-Host "MSI generado: $msi ($([math]::Round((Get-Item $msi).Length / 1MB, 1)) MB)"
