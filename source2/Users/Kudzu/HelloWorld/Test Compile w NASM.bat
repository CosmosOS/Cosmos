REM Tools\nasm\nasm -g -f elf -F stabs -o output.obj Tools\asm\main.asm

del nasm.log
..\..\..\..\build\Tools\nasm\nasm.exe -g -f bin -o output.obj output.asm -E nasm.log
  
pause


