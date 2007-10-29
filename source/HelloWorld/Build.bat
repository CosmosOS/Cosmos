cd bin\debug
..\..\..\IL2CPU\bin\debug\IL2CPU.exe -in:HelloWorld.exe -out:output.asm
@REM FASM must be in the path
fasm.exe output.asm
@pause