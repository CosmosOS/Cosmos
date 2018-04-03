@echo off

set DOTNET_VERSION=4.7.1

echo Check for installed .NET Framework %DOTNET_VERSION%

reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework\v4.0.30319\SKUs\.NETFramework,Version=v%DOTNET_VERSION%" /ve >nul 2>&1

if %ERRORLEVEL% neq 0 (
  echo ".NET Framework %DOTNET_VERSION% is not installed. Please download it from https://www.microsoft.com/net/download/visual-studio-sdks"
  pause
  goto:eof
)

set "ARCH=%PROCESSOR_ARCHITECTURE%"
if %ARCH%==x86 set "VSWhere=%ProgramFiles%\Microsoft Visual Studio\Installer\vswhere.exe"
if %ARCH%==AMD64 set "VSWhere=%ProrgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
REM set "VSWhere=%ProgramFiles%\Microsoft Visual Studio\Installer\vswhere.exe"

:MSBuild
echo Looking for MSBuild
for /f "usebackq delims=" %%i in (`^""%VSWhere%" -latest -version "[15.0,16.0)" -products * -requires "Microsoft.Component.MSBuild" -property "installationPath"^"`) do (
  set InstallDir=%%i
)

set "MSBuild=%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe"

if not exist "%MSBuild%" (
  echo MSBuild not found. Please make sure Visual Studio 15.2+ is installed.
  pause
  goto:eof
)

echo Building Builder.sln
"%MSBuild%" Builder.sln /nologo /maxcpucount /nodeReuse:false /verbosity:minimal /t:Restore;Build

start "Cosmos Builder" "source\Cosmos.Build.Builder\bin\Debug\net471\Cosmos.Build.Builder.exe" "-VSPATH=%InstallDir%" %*
