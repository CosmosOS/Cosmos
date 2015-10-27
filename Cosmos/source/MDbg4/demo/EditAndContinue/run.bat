@echo *******************************************************************************
@echo		Step 1 - Use ilasm to pre-create edits that are later applied
@echo *******************************************************************************

ilasm /debug=IMPL HelloWorld.il /ENC=HelloWorld_v1.il /out=HelloWorld.exe

REM Demonstration of edit for inactive function
@echo *******************************************************************************
@echo		Step 2 - Demonstration of edit for inactive function
@echo *******************************************************************************
..\..\bin\debug\mdbg !run -enc HelloWorld.exe !lo enc !enc HelloWorld.exe HelloWorld_v1.il !go !quit


REM Demonstration of edit for active function
@echo *******************************************************************************
@echo		Step 3 - Demonstration of edit for active function
@echo *******************************************************************************
..\..\bin\debug\mdbg !run -enc HelloWorld.exe !lo enc !next !step !enc HelloWorld.exe HelloWorld_v1.il !go !remap 0 !go !go !quit


