@echo off
chcp 65001 > nul
cd /d "%~dp0\.."
title AutoCommit - Gỡ Bỏ Task Scheduler

:: Tự động kiểm tra và yêu cầu quyền Administrator nếu chưa có
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [!] Đang yêu cầu quyền Administrator để gỡ bỏ Task Scheduler...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

echo ===================================================
echo   🗑️ GỠ BỎ WINDOWS TASK SCHEDULER
echo ===================================================
echo.

if exist "AutoCommit\bin\Release\net8.0\AutoCommit.exe" (
    "AutoCommit\bin\Release\net8.0\AutoCommit.exe" --uninstall-task
) else (
    dotnet run --project AutoCommit\AutoCommit.csproj -- --uninstall-task
)

echo.
echo ===================================================
echo   Nhấn phím bất kỳ để đóng cửa sổ này...
echo ===================================================
pause > nul
