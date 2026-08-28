@echo off
chcp 65001 > nul
cd /d "%~dp0\.."
title AutoCommit - Bù Commit Theo Khoảng Ngày

echo ===================================================
echo   📅 BÙ COMMIT THEO KHOẢNG NGÀY TRONG QUÁ KHỨ
echo ===================================================
echo.
set /p START_DATE="Nhập ngày bắt đầu (YYYY-MM-DD, vd: 2026-08-10): "
set /p END_DATE="Nhập ngày kết thúc (YYYY-MM-DD, vd: 2026-08-20): "
set /p COUNT="Nhập số commit mỗi ngày (Mặc định: 2): "
if "%COUNT%"=="" set COUNT=2

echo.
echo Đang tiến hành tạo commit bù từ %START_DATE% đến %END_DATE% (%COUNT% commit/ngày)...
echo.

if exist "AutoCommit\bin\Release\net8.0\AutoCommit.exe" (
    "AutoCommit\bin\Release\net8.0\AutoCommit.exe" --fill-range %START_DATE%:%END_DATE% --count %COUNT%
) else (
    dotnet run --project AutoCommit\AutoCommit.csproj -- --fill-range %START_DATE%:%END_DATE% --count %COUNT%
)

echo.
echo ===================================================
echo   Nhấn phím bất kỳ để đóng cửa sổ này...
echo ===================================================
pause > nul
