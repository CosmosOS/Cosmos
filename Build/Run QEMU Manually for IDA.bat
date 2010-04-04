@REM qemu\qemu.exe -L . -cdrom \source\Cosmos\source2\Users\Kudzu\Breakpoints\bin\Debug\CosmosKernel.iso -boot d -S -s \source\Cosmos\Build\Tools\qemu\

cd tools\qemu
qemu -S -s -L \source\Cosmos\Build\Tools\qemu\ -cdrom \source\Cosmos\source2\Users\Kudzu\Breakpoints\bin\Debug\CosmosKernel.iso

pause
