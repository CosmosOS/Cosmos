Imports Cosmos.Debug

Public Class Kernel

    Public Shared Sub Boot()
        Dim xBoot = New Cosmos.Sys.Boot()
        xBoot.Execute()

        Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back:")
        Dim xResult = Console.ReadLine()
        Console.Write("Text typed: ")
        Console.WriteLine(xResult)
    End Sub

End Class
