@echo off
chcp 65001 > nul
cd /d "%~dp0\.."
title AutoCommit - Hướng Dẫn & Trợ Giúp

echo ===================================================
echo   📖 TOÀN BỘ TÙY CHỌN TRỢ GIÚP (CLI HELP)
echo ===================================================
echo.

if exist "AutoCommit\bin\Release\net8.0\AutoCommit.exe" (
    "AutoCommit\bin\Release\net8.0\AutoCommit.exe" --help
) else (
    dotnet run --project AutoCommit\AutoCommit.csproj -- --help
)

echo.
echo ===================================================
echo   Nhấn phím bất kỳ để đóng cửa sổ này...
echo ===================================================
pause > nul
