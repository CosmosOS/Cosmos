@ECHO OFF

REM %1 == ToolPath
REM %2 == ElfFile
REM %3 == MapFile

"%~1\objdump.exe" --wide --syms "%~2" > "%~3"
