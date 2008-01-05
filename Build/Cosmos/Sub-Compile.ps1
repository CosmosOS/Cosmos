# ----------- Compile with IL2CPU
remove-item output.asm -ea SilentlyContinue

..\..\source\il2cpu\bin\Debug\il2cpu "-in:..\..\source\Cosmos\Cosmos.Shell.Console\bin\Debug\Cosmos.Shell.Console.exe" "-plug:..\..\source\Cosmos\Cosmos.Kernel.Plugs\bin\Debug\Cosmos.Kernel.Plugs.dll" "-out:output.obj" "-platform:nativex86" "-asm:.\"
