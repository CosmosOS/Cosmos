@echo off
cls

echo Check for installed .NET Framework 4.7.1

FOR /F "tokens=2*" %%A IN ('REG.EXE QUERY "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /V "Release" 2^>NUL ^| FIND "REG_DWORD"') DO SET Ver=%%B
IF "%Ver%"=="" (
  echo .NET Framework 4.7.1 or 4.5 or above 4.5 is not installed. Please download from https://www.microsoft.com/net/download/visual-studio-sdks
  pause
  goto:eof
)
SET "NET471_CREATORS=0x709fc"

if NOT "%NET471_CREATORS%"=="%Ver%" (
  echo .NET Framework 4.7.1 is required.
  pause
  goto:eof
)

set NuGet=Build\Tools\nuget.exe
set VSWhere=Build\Tools\vswhere.exe

:NuGet
echo Running NuGet restore
%NuGet% restore Builder.sln

:MSBuild
echo Looking for MSBuild
for /f "usebackq delims=" %%i in (`%VSWhere% -latest -version "[15.0,16.0)" -requires "Microsoft.Component.MSBuild" -property "installationPath"`) do (
  set InstallDir=%%i
)

if exist "%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe" (
  set MSBuild="%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe"
)

if exist %MSBuild% (
  %MSBuild% Builder.sln /nologo /maxcpucount /nodeReuse:false /p:Configuration="Debug" /p:Platform="Any CPU"
)

start "Cosmos Builder" "source\Cosmos.Build.Builder\bin\Debug\Cosmos.Build.Builder.exe" "-VSPATH=%InstallDir%" %1 %2 %3 %4 %5 %6 %7 %8 %9