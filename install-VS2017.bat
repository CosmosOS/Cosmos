@echo off
cls

set DOTNET_VERSION=4.7.1

echo Check for installed .NET Framework %DOTNET_VERSION%

reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework\v4.0.30319\SKUs\.NETFramework,Version=v%DOTNET_VERSION%" /ve >nul 2>&1

if %ERRORLEVEL% neq 0 (
  echo .NET Framework %DOTNET_VERSION% is not installed. Please download it from https://www.microsoft.com/net/download/visual-studio-sdks
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

start "Cosmos Builder" "source\Cosmos.Build.Builder\bin\Debug\Cosmos.Build.Builder.exe" "-VSPATH=%InstallDir%" %*
