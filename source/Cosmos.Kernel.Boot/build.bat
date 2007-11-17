@rem call msbuild3_5 d:\dotnet\il2asm\repos\source\IL2CPU.sln
..\il2cpu\bin\debug\il2cpu.exe -in:bin\debug\Cosmos.Kernel.Boot.exe -out:bin\debug\output.obj -platform:nativex86 -asm:output.asm
del cosmos.xml
"..\..\tools\qemu\qemu.exe" -L "..\..\tools\qemu" -fda ..\..\tools\Templates\Cosmos.img -serial file:cosmos.xml -boot a -hda fat:bin\debug\