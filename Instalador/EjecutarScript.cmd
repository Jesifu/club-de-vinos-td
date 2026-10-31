@echo off
rem Crea la base BDCAPAS (si falta) y ejecuta script.sql. Uso: EjecutarScript.cmd [servidor\instancia]  (por defecto: .)
rem Guarda la salida de sqlcmd en %TEMP%\EjecutarScript_BDCAPAS.log (Program Files no es escribible sin permisos).
setlocal
set "SRV=%~1"
if "%SRV%"=="" set "SRV=."
set "LOG=%TEMP%\EjecutarScript_BDCAPAS.log"
set "ERRS=%TEMP%\EjecutarScript_BDCAPAS.err"

where sqlcmd >nul 2>&1
if errorlevel 1 goto sinsqlcmd

sqlcmd -S "%SRV%" -E -b -Q "IF DB_ID(N'BDCAPAS') IS NULL CREATE DATABASE [BDCAPAS]" > "%LOG%" 2>&1
if errorlevel 9009 goto sinsqlcmd
if errorlevel 1 goto fallo

sqlcmd -S "%SRV%" -d BDCAPAS -E -I -f 65001 -i "%~dp0script.sql" >> "%LOG%" 2>&1
if errorlevel 9009 goto sinsqlcmd
if errorlevel 1 goto fallo

rem Errores esperados al re-ejecutar: 2714 (objeto ya existe), 1913 (indice ya existe), 1779 (clave primaria ya existe), 1781 (default ya existe).
rem sqlcmd escribe "Msg N," o "Mensaje N," segun el idioma del equipo.
findstr /i /r /c:"^Msg [0-9]*," /c:"^Mensaje [0-9]*," /c:"^Sqlcmd: Error" "%LOG%" | findstr /v /i /c:"Msg 2714," /c:"Mensaje 2714," /c:"Msg 1913," /c:"Mensaje 1913," /c:"Msg 1779," /c:"Mensaje 1779," /c:"Msg 1781," /c:"Mensaje 1781," > "%ERRS%"
set "TAM=0"
for %%F in ("%ERRS%") do set "TAM=%%~zF"
if not "%TAM%"=="0" goto advertencias

echo Base de datos BDCAPAS lista en %SRV%.
exit /b 0

:advertencias
echo La base de datos BDCAPAS en %SRV% termino con advertencias, revise %LOG%
echo Errores inesperados:
type "%ERRS%"
exit /b 3

:sinsqlcmd
echo No se encontro sqlcmd. Instale las herramientas de linea de comandos de SQL Server (sqlcmd) o ejecute este script desde un equipo que las tenga.
exit /b 9

:fallo
echo No se pudo completar. Verifique que SQL Server este iniciado, el nombre del servidor\instancia y sus permisos. Detalle en %LOG%
exit /b 1
