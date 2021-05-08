Imports Cosmos.TestRunner

Friend Module LanguageTests
    Friend Sub RunTests()
        Assert.IsTrue("Hello World" = "Hello World", "String equality")
        Assert.IsTrue("Hello " & "World" = "Hello World", "String concatenation")
        Assert.IsFalse("Hello" & "World" = "Hello World", "String concatenation")
    End Sub
End Module
