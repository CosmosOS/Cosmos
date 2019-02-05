@echo off

if not exist "%ProgramFiles(x86)%" (
  set "ProgramFiles(x86)=%ProgramFiles%"
)

set "VSWhere=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"

:MSBuild
echo Looking for MSBuild
for /f "usebackq delims=" %%i in (`^""%VSWhere%" -latest -prerelease -version "[16.0,17.0)" -products * -requires "Microsoft.Component.MSBuild" -property "installationPath"^"`) do (
  set InstallDir=%%i
)

set "MSBuild=%InstallDir%\MSBuild\Current\Bin\MSBuild.exe"

if not exist "%MSBuild%" (
  echo MSBuild not found. Please make sure Visual Studio 16.0+ is installed.
  pause
  goto:eof
)

echo Building Builder.sln
"%MSBuild%" Builder.sln /nologo /maxcpucount /nodeReuse:false /verbosity:minimal /t:Restore;Build

start "Cosmos Builder" "source\Cosmos.Build.Builder\bin\Debug\net471\Cosmos.Build.Builder.exe" "-VSPATH=%InstallDir%" %*
