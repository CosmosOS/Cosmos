@echo off
cls

set NuGet=Build\Tools\nuget.exe
set VSWhere=Build\Tools\vswhere.exe

:NuGet
echo Running NuGet restore
%NuGet% restore Builder.sln
%NuGet% restore Cosmos.sln

:MSBuild
echo Looking for MSBuild
for /f "usebackq tokens=1* delims=: " %%i in (`%VSWhere% -latest -version "[15.0,16.0)" -requires "Microsoft.Component.MSBuild" -property "installationPath"`) do (
  set InstallDir=%%j
)

if exist "%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe" (
  set MSBuild="%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe"
)

if exist %MSBuild% (
  %MSBuild% Builder.sln /nologo /maxcpucount /p:Configuration="Debug" /p:Platform="Any CPU"
)

start "Cosmos Builder" "source\Cosmos.Build.Builder\bin\Debug\Cosmos.Build.Builder.exe" "-bat" "-VS2017" "-VSPATH=%InstallDir%"
