@rem call msbuild3_5 d:\dotnet\il2asm\repos\source\IL2CPU.sln

@REM ----------- Compile with IL2CPU
del output.asm
..\..\..\source\il2cpu\bin\Debug\il2cpu -in:..\..\..\source\Cosmos\Cosmos.Shell.Console\bin\Debug\Cosmos.Shell.Console.exe -plug:..\..\..\source\Cosmos\Cosmos.Kernel.Plugs\bin\Debug\Cosmos.Kernel.Plugs.dll -out:output.obj -platform:nativex86 -asm:output.asm
copy /Y output.obj files\output.obj
pause

@REM ----------- Build ISO
del cosmos.iso
attrib files\syslinux\isolinux.bin -r
..\..\..\Tools\mkisofs\mkisofs -R -b syslinux/isolinux.bin -no-emul-boot -boot-load-size 4 -boot-info-table -o Cosmos.iso files
pause

REM ----------- Start QEMU
del serial-debug.txt
cd ..\..\..\tools\qemu\
qemu.exe -L . -cdrom ..\..\build\Cosmos\ISO\Cosmos.iso -boot d -hda ..\..\build\Cosmos\ISO\C-drive.img -serial file:..\..\build\Cosmos\ISO\serial-debug.txt -kernel-kqemu
pause