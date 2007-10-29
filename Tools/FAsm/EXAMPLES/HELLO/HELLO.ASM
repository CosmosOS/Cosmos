
; example of simplified Win32 programming using complex macro features

include 'win32ax.inc'

.code

  start:
	invoke	MessageBox,HWND_DESKTOP,"Hi! I'm the example program!","Win32 Assembly",MB_OK
	invoke	ExitProcess,0

.end start
