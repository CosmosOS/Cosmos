

### IL2CPU

This is one of the most important pieces of code in Cosmos. It's an AOT
(Ahead-Of-Time) compiler.

When you compile your C# (or any .NET language) program, it is compiled into the IL (intermediate
language). The IL is then interpreted and executed by a Virtual Machine when
you open your exe.

Cosmos is written in C# and Visual Studio compiles it into IL as always. But a
PC does not come with an interpreter for the IL code. And writing a virtual
machine for running an operating system is not always ideal.

This is where IL2CPU comes in. IL2CPU takes the IL code and translates it to processor opcodes. Currently, only x86 opcodes are available at the moment. However, more architectures are planned for the future (ARM, PowerPC, x86-64).

At this point, IL2CPU performs some more magic before finally converting the entire file into a bootable binary file, which can be loaded by a bootloader on any system (Cosmos uses Syslinux).

As you might think, IL2CPU is a fundamental part of the development
of Cosmos. IL2CPU is responsible for the final output, which is why most optimizations added are for IL2CPU.
