@REM This file is used to build the Cosmos sources.

@REM This label is used for the 'error1' label if the user trying again.
:retry
cls
del source\Cosmos.Build.Builder\bin\Debug\Cosmos.Build.Builder.*

"%ProgramFiles(x86)%\MSBuild\14.0\Bin\msbuild.exe" "source\Builder.sln" /nologo /maxcpucount /p:Configuration=Debug /p:Platform=x86

if not exist source\Cosmos.Build.Builder\bin\Debug\Cosmos.Build.Builder.exe goto error1
start source\Cosmos.Build.Builder\bin\Debug\Cosmos.Build.Builder.exe -bat -VS2015 %1 %2 %3 %4 %5 %6 %7 %8 %9

@REM This is the message for the building successfuly completed.
@echo -----------------------------------------
@echo Building done. Press any key to close the window.
@pause
@exit

@REM This is the message if the Cosmos Builder Tool does not exists
:error1
@echo -----------------------------------------
@echo Error: The Cosmos Builder Tool (from source\Cosmos.Build.Builder\bin\Debug\Cosmos.Build.Builder.exe) does not exists.
@echo Press any key to try again.
@pause
@REM At this moment, when the user press any key, it will go to the 'retry' label.
goto retry
