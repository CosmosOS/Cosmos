cd bin\debug
..\..\..\IL2CPU\bin\debug\IL2CPU -in:HelloWorld.exe -out:output.asm
..\..\..\..\tools\fasm\fasm output.asm
@pause