del source2\Build\Cosmos.Build.Builder\bin\Debug\Cosmos.Build.Builder.*

%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe "source2\Builder.sln" /maxcpucount /verbosity:normal /nologo /p:Configuration=Debug /p:Platform=x86

source2\Build\Cosmos.Build.Builder\bin\Debug\Cosmos.Build.Builder.exe %1 %2 %3 %4 %5 %6 %7 %8 %9

pause
