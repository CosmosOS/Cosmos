Imports System
Imports System.Collections.Generic
Imports System.Text

Namespace $safeprojectname$

	Public Class Kernel
		Inherits Cosmos.System.Kernel

		Protected Overrides Sub BeforeRun()
			Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.")
		End Sub

		Protected Overrides Sub Run()
			Console.Write("Input: ")
			Console.ReadLine()
			Console.Write("Text typed: ")
		End Sub

	End Class

End Namespace