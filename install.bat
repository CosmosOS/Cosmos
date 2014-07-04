@REM Delete so if builder fails it wont exist
del source2\Build\Cosmos.Build.Builder\bin\Debug\Cosmos.Build.Builder.*

%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe "source2\Builder.sln" /nologo /maxcpucount /p:Configuration=Debug /p:Platform=x86

@if not exist source2\Build\Cosmos.Build.Builder\bin\Debug\Cosmos.Build.Builder.exe goto error

@for /f "tokens=4-6 delims=.] " %%i in ('ver') do set VERSION=%%i.%%j.%%k

start source2\Build\Cosmos.Build.Builder\bin\Debug\Cosmos.Build.Builder.exe -bat %TASK% %1 %2 %3 %4 %5 %6 %7 %8 %9

@exit

:Error
@echo _
@echo _
@echo Something went wrong.
@pause