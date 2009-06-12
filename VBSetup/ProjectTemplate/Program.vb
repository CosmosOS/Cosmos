Imports System
Imports Cosmos.Builder

Namespace $safeprojectname$
    Class Program
#Region "Cosmos Builder Logic"
        ' Most users wont touch this. This will call the Cosmos Build tool
        <STAThread()> Private Shared Sub Main(ByVal args As String())
            BuildUI.Run()
        End Sub
#End Region

        ' Main entry point of the kernel
        Public Shared Sub Init()

            'Boot your kernel
            New Cosmos.Sys.Boot().Execute()

            'Your custom implementation
            Console.WriteLine("Welcome to COSMOS! You just booted VB code.")

            'Shutdown
            Cosmos.Sys.Deboot.ShutDown()
        End Sub
    End Class
End Namespace