cd tools\qemu
qemu -serial pipe:CosmosDebug -L \source\Cosmos\Build\Tools\qemu\ -cdrom \source\Cosmos\source2\Users\Kudzu\Breakpoints\bin\Debug\CosmosKernel.iso

pause
