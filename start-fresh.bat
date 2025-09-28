@echo off
setlocal enabledelayedexpansion

REM Configuración de colores y título
title VHouse - Sistema de Distribución Vegana
color 0A

REM Mostrar ayuda si se solicita primero
if /i "%~1"=="--help" goto showHelp
if /i "%~1"=="-?" goto showHelp
if /i "%~1"=="/?" goto showHelp

REM Parse command line parameters
set "HARD_MODE=false"
:parseArgs
if "%~1"=="" goto endParse
if /i "%~1"=="--hard" set "HARD_MODE=true"
if /i "%~1"=="-h" set "HARD_MODE=true"
shift
goto parseArgs
:endParse

if "%HARD_MODE%"=="true" (
    echo ========================================
    echo 🌱 VHouse - Arrancador COMPLETO
    echo    Por Bernard Uriza Orozco
    echo ========================================
    echo.
    echo ⚠  MODO AGRESIVO: Reset completo del sistema
) else (
    echo ========================================
    echo 🌱 VHouse - Arrancador RÁPIDO
    echo    Por Bernard Uriza Orozco
    echo ========================================
    echo.
    echo ⚡ MODO RÁPIDO: Solo compilar y arrancar
    echo    (Usa --hard para reset completo)
)
echo.

REM Verificar que estamos en el directorio correcto
if not exist "VHouse.sln" (
    echo ❌ ERROR: No se encontró VHouse.sln
    echo    Asegúrate de ejecutar este script desde la raíz del proyecto
    pause
    exit /b 1
)

REM Detener procesos previos (siempre)
echo [1/4] Deteniendo procesos previos...
taskkill /F /IM dotnet.exe 2>nul
if !errorlevel! == 0 (
    echo    ✓ Procesos dotnet terminados
    timeout /t 2 /nobreak >nul
) else (
    echo    ℹ No había procesos dotnet activos
)

REM Limpieza condicional basada en modo
if "%HARD_MODE%"=="true" (
    echo.
    echo [2/4] Limpieza COMPLETA de archivos...
    REM Limpiar base de datos en múltiples ubicaciones
    for %%f in (vhouse_clean.db vhouse_clean.db-shm vhouse_clean.db-wal) do (
        if exist "%%f" del /F /Q "%%f" 2>nul
        if exist "VHouse.Web\%%f" del /F /Q "VHouse.Web\%%f" 2>nul
    )
    echo    ✓ Base de datos limpiada

    REM Limpiar directorios de compilación
    if exist "VHouse.Web\bin" rd /s /q "VHouse.Web\bin" 2>nul
    if exist "VHouse.Web\obj" rd /s /q "VHouse.Web\obj" 2>nul
    for /d %%d in (src\*\bin src\*\obj) do if exist "%%d" rd /s /q "%%d" 2>nul
    echo    ✓ Directorios de compilación limpiados

    REM Limpiar caché de NuGet local
    dotnet nuget locals temp --clear >nul 2>&1
    echo    ✓ Caché de NuGet limpiado
) else (
    echo.
    echo [2/4] Limpieza RÁPIDA...
    echo    ⚡ Saltando limpieza de BD y bins (modo rápido)
    echo    💡 Usa --hard para limpieza completa
)

REM Restaurar y compilar (condicional)
if "%HARD_MODE%"=="true" (
    echo.
    echo [3/4] Restaurando paquetes NuGet...
    dotnet restore --verbosity quiet
    if errorlevel 1 (
        echo    ❌ Error al restaurar paquetes
        echo    Ejecuta manualmente: dotnet restore
        pause
        exit /b 1
    )
    echo    ✓ Paquetes restaurados

    echo.
    echo [4/4] Compilando proyecto COMPLETO...
    dotnet build --configuration Debug --no-restore --verbosity quiet
    if errorlevel 1 (
        echo    ❌ Error al compilar el proyecto
        echo    Revisa los errores de compilación
        pause
        exit /b 1
    )
    echo    ✓ Proyecto compilado exitosamente
) else (
    echo.
    echo [3/4] Compilación RÁPIDA...
    dotnet build --configuration Debug --verbosity minimal
    if errorlevel 1 (
        echo    ❌ Error al compilar el proyecto
        echo    💡 Intenta con: start-fresh.bat --hard
        pause
        exit /b 1
    )
    echo    ✓ Compilación rápida exitosa
)

REM Migraciones (condicional)
if "%HARD_MODE%"=="true" (
    echo.
    echo [Opcional] Aplicando migraciones de base de datos...
    cd VHouse.Web
    dotnet ef database update --project ..\src\VHouse.Infrastructure --startup-project . --verbose
    if errorlevel 1 (
        echo.
        echo    ⚠ Advertencia: No se pudieron aplicar las migraciones
        echo    La aplicación se ejecutará pero puede tener problemas con la BD
        echo.
        echo    Para crear una nueva migración usa:
        echo    dotnet ef migrations add NombreMigracion --project src\VHouse.Infrastructure
        echo.
        timeout /t 5 /nobreak >nul
    ) else (
        echo    ✓ Base de datos actualizada
    )
    cd ..
) else (
    echo.
    echo [Opcional] Saltando migraciones en modo rápido...
    echo    💡 BD existente será reutilizada
    echo    💡 Usa --hard si tienes problemas de BD
)

echo.
echo ========================================
echo    🚀 ARRANCANDO VHOUSE
echo ========================================
echo.
echo    📍 URL Local:    http://localhost:5000
echo    📍 URL Segura:   https://localhost:5001
echo    📍 Cliente Demo: http://localhost:5000/client/MONA_DONA
echo.
echo    💡 Presiona Ctrl+C para detener
echo.
echo ========================================
echo.

REM Configurar variables de entorno para desarrollo
set ASPNETCORE_ENVIRONMENT=Development
set ASPNETCORE_URLS=http://localhost:5000;https://localhost:5001

REM Ejecutar la aplicación
dotnet run --project VHouse.Web --no-build

REM Si llegamos aquí, la aplicación se detuvo
echo.
echo ========================================
echo    ⏹ VHouse detenido
echo ========================================
pause
goto end

:showHelp
echo.
echo ========================================
echo 🌱 VHouse - Ayuda del Arrancador
echo ========================================
echo.
echo USOS:
echo   start-fresh.bat           Modo RÁPIDO (recomendado)
echo   start-fresh.bat --hard    Modo COMPLETO (reset total)
echo   start-fresh.bat -h        Modo COMPLETO (alias)
echo   start-fresh.bat --help    Mostrar esta ayuda
echo.
echo MODOS:
echo   🚀 RÁPIDO (default):
echo      - Solo detiene procesos previos
echo      - Compilación incremental rápida
echo      - Reutiliza BD existente
echo      - Ideal para desarrollo diario
echo.
echo   ⚠  COMPLETO (--hard):
echo      - Limpia completamente BD y bins
echo      - Restaura paquetes NuGet
echo      - Compilación completa
echo      - Aplica migraciones
echo      - Usa cuando tengas problemas
echo.
echo EJEMPLOS:
echo   start-fresh.bat           # Arrancado rápido diario
echo   start-fresh.bat --hard    # Después de cambios de BD
echo.
echo Por Bernard Uriza Orozco - VHouse Distribución Vegana
echo ========================================
pause
goto end

:end