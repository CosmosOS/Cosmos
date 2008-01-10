@echo off
mkdir ..\Build\GAC
copy ".\Cosmos\Cosmos.Shell.Console\bin\Debug\*.dll" "..\Build\GAC\" /Y
del "..\Build\Config\*.pdb"
del "..\Build\Config\*.vshost.exe"
"..\Tools\7zip\7z.exe" a -t7z -sfx Test.exe "..\Build\*.*" -r
pause