@echo off
chcp 65001 > nul
cd /d "%~dp0\.."
title AutoCommit - Chạy Tự Động

echo ===================================================
echo   🚀 ĐANG CHẠY AUTOCOMMIT (BÌNH THƯỜNG)
echo ===================================================
echo.

if exist "AutoCommit\bin\Release\net8.0\AutoCommit.exe" (
    "AutoCommit\bin\Release\net8.0\AutoCommit.exe"
) else (
    dotnet run --project AutoCommit\AutoCommit.csproj
)

echo.
echo ===================================================
echo   Nhấn phím bất kỳ để đóng cửa sổ này...
echo ===================================================
pause > nul
