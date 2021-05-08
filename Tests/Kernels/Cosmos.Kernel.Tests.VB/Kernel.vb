Public Class Kernel
    Inherits System.Kernel
    Protected Overrides Sub BeforeRun()
        Console.WriteLine("Cosmos booted successfully. Starting VB tests.")
    End Sub

    Protected Overrides Sub Run()
        Try
            mDebugger.Send("Run")

            RunTests()

            TestRunner.TestController.Completed()
        Catch ex As Exception
            mDebugger.Send("Exceptions occurred: " & ex.Message)
            TestRunner.TestController.Failed()
        End Try
    End Sub
End Class
