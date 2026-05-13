\
@echo off
setlocal

cd /d "%~dp0.."

echo Restoring...
dotnet restore src\KyneticKlicker\KyneticKlicker.csproj

echo.
echo Publishing standalone single-file Windows EXE...
dotnet publish src\KyneticKlicker\KyneticKlicker.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:EnableCompressionInSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:IncludeAllContentForSelfExtract=true

if %ERRORLEVEL% neq 0 (
    echo.
    echo BUILD FAILED.
    echo.
    echo Common fixes:
    echo 1. Install the .NET 10 SDK, not only the runtime.
    echo 2. Run this on Windows.
    echo 3. Run: dotnet --info
    echo.
    pause
    exit /b %ERRORLEVEL%
)

if not exist dist mkdir dist
copy /Y "src\KyneticKlicker\bin\Release\net10.0-windows\win-x64\publish\KyneticKlicker.exe" "dist\KyneticKlicker.exe" >nul

echo.
echo BUILD COMPLETE.
echo Standalone EXE:
echo dist\KyneticKlicker.exe
echo.
echo This EXE is self-contained and should run without installing .NET.
echo.
pause
