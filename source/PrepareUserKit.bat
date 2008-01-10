@echo off
mkdir ..\Build\GAC
copy ".\Cosmos\Cosmos.Shell.Console\bin\Debug\*.dll" "..\Build\GAC\" /Y
pause