@echo off
echo ========================================
echo 🌱 VHouse - Arrancador con Reset
echo ========================================
echo.

echo [1/3] Deteniendo procesos previos...
taskkill /F /IM dotnet.exe 2>nul
timeout /t 2 /nobreak >nul

echo [2/3] Reseteando base de datos...
if exist vhouse_clean.db (
    del /F vhouse_clean.db
    echo    ✓ Base de datos eliminada
) else (
    echo    ℹ No habia base de datos previa
)

echo    → Aplicando migraciones...
dotnet ef database update --project src\VHouse.Infrastructure
if errorlevel 1 (
    echo    ✗ Error al aplicar migraciones
    pause
    exit /b 1
)

echo [3/3] Arrancando aplicacion...
echo.
echo ========================================
echo    🚀 VHouse corriendo en:
echo    http://localhost:5000
echo ========================================
echo.

dotnet run --project src\VHouse.Web