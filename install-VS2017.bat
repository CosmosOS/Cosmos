@echo off
cls

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
