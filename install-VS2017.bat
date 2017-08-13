@echo off
cls

set NuGet=Build\Tools\nuget.exe
set VSWhere=Build\Tools\vswhere.exe
set IL2CPU=..\IL2CPU\IL2CPU.sln
set XS=..\XSharp\XSharp.sln

echo Finding XSharp repo directory
IF EXIST %XS% (echo XSharp solution found!) else ( 
echo XSharp not found!
goto NOI2CXS 
)

echo Looking for IL2CPU repo directory...
IF EXIST %IL2CPU% (echo IL2CPU solution found!) else (
echo IL2CPU not found!
goto NOI2CXS )


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


:NOI2CXS
echo install-vs2017.bat failed to find the required IL2CPU or XSharp directories
echo Be sure to see the Cosmos documentation for instructions on building Cosmos