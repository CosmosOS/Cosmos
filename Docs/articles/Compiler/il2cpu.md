# IL2CPU

IL2CPU is one of the most important pieces of code in Cosmos. It's an AOT (Ahead-Of-Time) compiler.

When you compile your C# (or any other .NET language) program, it is compiled into CIL (common intermediate language). The CIL is then interpreted and executed by a virtual machine when you open your .exe.

Cosmos is written in C# and Visual Studio compiles it into CIL like usual. But a PC does not come with an interpreter for the CIL code, and writing a virtual machine for running an operating system is not ideal.

This is where IL2CPU comes in. IL2CPU takes the CIL code and translates it to processor op-codes. Currently, only x86 opcodes are available at the moment. However, more architectures are planned for the future (ARM, PowerPC, x86-64).

At this point, IL2CPU performs some more magic before finally converting the entire file into a bootable binary file, which can be loaded by a bootloader on any system (Cosmos uses the [Limine bootloader](https://github.com/limine-bootloader/limine).

As you might think, IL2CPU is a fundamental part of the development of Cosmos. IL2CPU is responsible for the final output, which is why most optimizations added are for IL2CPU.

*Last updated on 24 March 2024.*