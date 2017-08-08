Run("C:\Program Files (x86)\IDA\idag.exe")

WinWaitActive("About")
Send("{SPACE}");

WinWaitActive("[CLASS:TIdaWindow]")
Sleep(250) 

Send("!fo")
Sleep(1000) ; Must wait else the d often gets eaten
Send("d:\source\Cosmos\source2\Users\Kudzu\Breakpoints\bin\Debug\CosmosKernel.obj", 1)
Send("{ENTER}")

WinWaitActive("Load a new file")
Send("{ENTER}")

WinWaitActive("[CLASS:TIdaWindow]")

;MsgBox(4096, "Test", ControlGetText("[CLASS:TIdaWindow]", "", "[ID:1380126]"))
;while StringInStr(ControlGetText("[CLASS:TIdaWindow]", "", "TListBox1"), "The initial autoanalysis has been finished.") = 0
	;MsgBox(4096, "Test", ControlGetText("[CLASS:TIdaWindow]", "", "[ID:1380126]"))
;	Sleep(200)
;WEnd

;Send("{F9}") ; Debug
;Send("!y") ; Are you sure?
;Send("!y") ; Attach to remote?





