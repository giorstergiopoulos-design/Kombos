@echo off
cd /d "%~dp0"
if exist "bin\Debug\net8.0-windows\Kombos.exe" (
    start "" "bin\Debug\net8.0-windows\Kombos.exe"
) else (
    echo Kombos.exe not found - building first...
    dotnet build
    start "" "bin\Debug\net8.0-windows\Kombos.exe"
)
