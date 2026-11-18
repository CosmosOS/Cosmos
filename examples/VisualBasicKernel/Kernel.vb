' This code is licensed under the BSD 3-Clause license (see LICENSE for details)

Imports Sys = Cosmos.Kernel.System

''' <summary>
''' The simplest kernel: prints a line once booted, then echoes back every
''' line typed. <c>examples/CSharpKernel</c> is the same kernel in C#.
''' </summary>
Public Class Kernel
    Inherits Sys.Kernel

    Protected Overrides Sub BeforeRun()
        Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.")
    End Sub

    Protected Overrides Sub Run()
        Console.Write("Input: ")
        Dim input As String = Console.ReadLine()
        Console.Write("Text typed: ")
        Console.WriteLine(input)
    End Sub
End Class
