
; SHELL32.DLL structures and constants

struct NOTIFYICONDATA
  cbSize	   dd ?
  hWnd		   dd ?
  uID		   dd ?
  uFlags	   dd ?
  uCallbackMessage dd ?
  hIcon 	   dd ?
  szTip 	   TCHAR 64 dup (?)
ends

struct NOTIFYICONDATAA
  cbSize	   dd ?
  hWnd		   dd ?
  uID		   dd ?
  uFlags	   dd ?
  uCallbackMessage dd ?
  hIcon 	   dd ?
  szTip 	   db 64 dup (?)
ends

struct NOTIFYICONDATAW
  cbSize	   dd ?
  hWnd		   dd ?
  uID		   dd ?
  uFlags	   dd ?
  uCallbackMessage dd ?
  hIcon 	   dd ?
  szTip 	   du 64 dup (?)
ends

; Taskbar icon messages

NIM_ADD        = 0
NIM_MODIFY     = 1
NIM_DELETE     = 2
NIM_SETFOCUS   = 3
NIM_SETVERSION = 4

; Taskbar icon flags

NIF_MESSAGE    = 01h
NIF_ICON       = 02h
NIF_TIP        = 04h
NIF_STATE      = 08h
NIF_INFO       = 10h
NIF_GUID       = 20h
