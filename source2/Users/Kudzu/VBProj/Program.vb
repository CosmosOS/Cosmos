Imports Cosmos.Debug

Public Class Kernel

    Public Shared Sub Boot()
        Dim xBoot = New Cosmos.Sys.Boot()
        xBoot.Execute()

        Console.WriteLine("Test")
        Debugger.Send("Hello from Cosmos!")
        Console.WriteLine("3 Cosmos booted successfully. Type a line of text to get it echoed back.")
        Console.WriteLine("Test")
        while 
            Console.Write("Input: ")
            Dim xResult = Console.ReadLine()
            Console.Write("Text typed: ")
            Console.WriteLine(xResult)
        End While
    End Sub

End Class
