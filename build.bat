@echo off
echo ========================================
echo SSHHelper Windows Build Script
echo ========================================

echo.
echo [1/4] Restoring dependencies...
dotnet restore
if %errorlevel% neq 0 exit /b %errorlevel%

echo.
echo [2/4] Building Release...
dotnet build --configuration Release --no-restore
if %errorlevel% neq 0 exit /b %errorlevel%

echo.
echo [3/4] Publishing Windows x64 (Self-contained)...
dotnet publish src/SSHHelper.App/SSHHelper.App.csproj ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:EnableCompressionInSingleFile=true ^
    -o ./publish/win-x64
if %errorlevel% neq 0 exit /b %errorlevel%

echo.
echo [4/4] Publishing Windows x86 (Self-contained)...
dotnet publish src/SSHHelper.App/SSHHelper.App.csproj ^
    -c Release ^
    -r win-x86 ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:EnableCompressionInSingleFile=true ^
    -o ./publish/win-x86
if %errorlevel% neq 0 exit /b %errorlevel%

echo.
echo ========================================
echo Build Complete!
echo ========================================
echo.
echo Output files:
echo   - publish\win-x64\SSHHelper.App.exe
echo   - publish\win-x86\SSHHelper.App.exe
echo.
pause