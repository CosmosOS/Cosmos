@REM Add this to post build event command line in VS for any file that needs update to bin cache.
@REM   call "$(SolutionDir)UpdateBinCache.bat" "$(TargetPath)" $(TargetFileName)
@REM But see required hack:
@REM    https://github.com/dotnet/project-system/issues/1569

@echo off

cd %~dp0
echo Path %1
echo File %2

if not exist build\bin\%2 goto copy
echo Deleting...
del build\bin\%2

:copy
echo Copying...
copy "%1" build\bin\
echo ---Done
