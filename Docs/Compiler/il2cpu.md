

### IL2CPU

This is one of the most important piece of code of Cosmos. It is an AOT
(Ahead-Of-Time) compiler.

When you compile your C# program, it is compiled into the IL (intermediate
language). The IL is then interpreted and executed by a Virtual Machine when
you open your exe

Cosmos is written in C# and Visual Studio compiles it into IL as always. But a
PC does not came with an interpreter for the IL code. And writing a virtual
machine for running an operating system is not always ideal.

IL2CPU takes the IL code and translates it to processor opcodes. Actually it
only supports the x86 architecture. So it translates from IL to x86 opcodes,
but it can be ported to any architecture.

As you might think, the develop of IL2CPU is a fundamental step for the grow
of Cosmos. IL2CPU is responsible for the final output of code and should
optimize it as much as possible.

**TODO: explain this better.** Before finally creating the binary, IL2CPU does some more magic with files created in "C#/ASM". They are executed and from their execution an assembly file is get and integrated into the output.

