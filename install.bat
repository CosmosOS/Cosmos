del source2\Build\Cosmos.Build.Builder\bin\Debug\Cosmos.Build.Builder.*

%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe "source2\Builder.sln" /maxcpucount /verbosity:normal /nologo /p:Configuration=Debug /p:Platform=x86

source2\Build\Cosmos.Build.Builder\bin\Debug\Cosmos.Build.Builder.exe

pause
