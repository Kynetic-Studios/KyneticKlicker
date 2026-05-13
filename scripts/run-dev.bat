@echo off
setlocal

cd /d "%~dp0.."
dotnet run --project src\KyneticKlicker\KyneticKlicker.csproj
pause
