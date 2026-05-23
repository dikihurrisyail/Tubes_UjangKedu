@echo off
REM UjangKeduDorDor.cmd - Jalankan bot dalam mode development atau release
REM Set MODE=dev untuk development (default, selalu rebuild)
REM Set MODE=release untuk release (hanya jalankan jika bin ada)

set MODE=dev

if "%MODE%"=="dev" (
    REM Development mode: selalu bersihkan, build, dan jalankan
    rmdir /s /q bin obj >nul 2>&1
    dotnet build >nul
    dotnet run --no-build >nul
) else if "%MODE%"=="release" (
    REM Release mode: tidak rebuild jika bin sudah ada
    if exist bin\ (
        dotnet run --no-build >nul
    ) else (
        dotnet build >nul
        dotnet run --no-build >nul
    )
) else (
    echo Error: Nilai MODE tidak valid. Gunakan "dev" atau "release".
)
