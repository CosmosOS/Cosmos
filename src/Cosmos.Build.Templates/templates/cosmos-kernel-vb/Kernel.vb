Imports Sys = Cosmos.Kernel.System

''' <summary>
''' Main kernel class - inherits from Cosmos.Kernel.System.Kernel.
''' </summary>
Public Class Kernel
    Inherits Sys.Kernel

    Protected Overrides Sub BeforeRun()
        Console.WriteLine("Cosmos booted successfully!")
        Console.WriteLine("Type a command to get it executed.")
    End Sub

    Protected Overrides Sub Run()
        Console.Write("> ")
        Dim input As String = Console.ReadLine()

        If String.IsNullOrEmpty(input) Then
            Return
        End If

        Select Case input.ToLower()
            Case "help"
                Console.WriteLine("Available commands:")
                Console.WriteLine("  help     - Show this help message")
                Console.WriteLine("  clear    - Clear the screen")
                Console.WriteLine("  halt     - Halt the system")

            Case "clear"
                Console.Clear()

            Case "halt"
                Console.WriteLine("Halting system...")
                Me.Stop()

            Case Else
                Console.WriteLine($"""{input}"" is not a command")
        End Select
    End Sub
End Class
