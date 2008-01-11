@echo off
mkdir ..\Build\GAC
copy ".\Cosmos\Cosmos.Shell.Console\bin\Debug\*.dll" "..\Build\GAC\" /Y
copy ".\Cosmos\Cosmos.Shell.Console\bin\Debug\IL2CPU.exe" "..\Build\GAC\IL2CPU.exe" /Y
del /Q "..\Build\Config\*.pdb"
del /Q "..\Build\Config\*.vshost.exe"
del /Q "..\Build\Cosmos.iso"
del /Q "..\Build\serial-debug.txt"
del /Q "..\Build\output.bin"
del /Q "..\Build\setup.pdb"
del /Q "..\Build\ISO\output.bin"
del /Q "..\Build\Tools\asm\*.*"
del /Q "..\Build\Tools\Cosmos.Kernel.Plugs\*.pdb"
"..\Tools\7zip\7z.exe" a -tzip Test.zip "..\Build\*.*" -r
pause