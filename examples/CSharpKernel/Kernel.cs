// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using Sys = Cosmos.Kernel.System;

namespace CSharpKernel;

/// <summary>
/// The simplest kernel: prints a line once booted, then echoes back every
/// line typed. <c>examples/VisualBasicKernel</c> is the same kernel in Visual Basic.
/// </summary>
public class Kernel : Sys.Kernel
{
    protected override void BeforeRun()
    {
        Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
    }

    protected override void Run()
    {
        Console.Write("Input: ");
        string? input = Console.ReadLine();
        Console.Write("Text typed: ");
        Console.WriteLine(input);
    }
}
