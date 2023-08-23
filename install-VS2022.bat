@echo off
setlocal
for /f "tokens=4-5 delims=. " %%i in ('ver') do set VERSION=%%i.%%j

if "%version%" == "6.1" goto seven

:start
cd /D "%~dp0"

if not exist "%ProgramFiles(x86)%" (
  set "ProgramFiles(x86)=%ProgramFiles%"
)

set "VSWhere=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"

:MSBuild
echo Looking for MSBuild
for /f "usebackq delims=" %%i in (`^""%VSWhere%" -latest -prerelease -version "[17.0,18.0)" -products * -requires "Microsoft.Component.MSBuild" -property "installationPath"^"`) do (
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
if %ERRORLEVEL% neq 0 (
  echo Failed to build Cosmos Builder. Exiting...
  exit /b %ERRORLEVEL%
)

start "Cosmos Builder" "source\Cosmos.Build.Builder\bin\Debug\Cosmos.Build.Builder.exe" "-VSPATH=%InstallDir%" %*
exit

:seven
echo Detected Windows 7! Checking for update KB3140245...

for /f "delims=" %%i in ("wmic qfe | find /c ""KB3140245""") do set update=%%i
if "%update%" == "0" (
    echo "The update KB3140245 isn't installed on your computer! It is mandatory on Windows 7 to install it in order to build Cosmos."
    pause
)

reg add "HKLM\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client" /v DisabledByDefault /t REG_DWORD /d 0 /f /reg:32
reg add "HKLM\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client" /v DisabledByDefault /t REG_DWORD /d 0 /f /reg:64
reg add "HKLM\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client" /v Enabled /t REG_DWORD /d 1 /f /reg:32
reg add "HKLM\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client" /v Enabled /t REG_DWORD /d 1 /f /reg:64
goto start