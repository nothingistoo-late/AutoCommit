@echo off
chcp 65001 > nul
cd /d "%~dp0\.."
title AutoCommit - Đăng Ký Task Scheduler

:: Tự động kiểm tra và yêu cầu quyền Administrator nếu chưa có
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [!] Đang yêu cầu quyền Administrator để đăng ký Task Scheduler...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

echo ===================================================
echo   ⚙️ ĐĂNG KÝ WINDOWS TASK SCHEDULER (ADMIN + S4U)
echo ===================================================
echo.
set /p TASK_TIME="Nhập giờ chạy hàng ngày (Mặc định: 09:30): "
if "%TASK_TIME%"=="" set TASK_TIME=09:30

echo.
echo Đang đăng ký Task Scheduler chạy vào lúc %TASK_TIME% hàng ngày...
echo.

if exist "AutoCommit\bin\Release\net8.0\AutoCommit.exe" (
    "AutoCommit\bin\Release\net8.0\AutoCommit.exe" --install-task --time "%TASK_TIME%"
) else (
    dotnet run --project AutoCommit\AutoCommit.csproj -- --install-task --time "%TASK_TIME%"
)

echo.
echo ===================================================
echo   Nhấn phím bất kỳ để đóng cửa sổ này...
echo ===================================================
pause > nul
