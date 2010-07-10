Run("C:\Program Files (x86)\IDA\idag.exe")

WinWaitActive("About")
Send("{SPACE}");

WinWaitActive("[CLASS:TIdaWindow]")
Sleep(250)

Send("!fo")
Send("d:\source\Cosmos\source2\Users\", 1)
Sleep(250)
Send("Kudzu\Breakpoints\bin\Debug\CosmosKernel.obj", 1)
Send("{ENTER}")

WinWaitActive("Load a new file")
Send("{ENTER}")

WinWaitActive("[CLASS:TIdaWindow]")
Sleep(1000)
Send("{F9}") ; Debug
Send("!y") ; Are you sure?
Send("!y") ; Attach to remote?





