
; Assembly Editor mode flags

AEMODE_OVERWRITE   = 1
AEMODE_VERTICALSEL = 2
AEMODE_NOUNDO	   = 4

; Assembly Editor search flags

AEFIND_CASESENSITIVE = 1
AEFIND_WHOLEWORDS    = 2
AEFIND_BACKWARD      = 4

; Assembly Editor styles

AES_AUTOINDENT	   = 0001h
AES_AUTOBRACKETS   = 0002h
AES_SMARTTABS	   = 0004h
AES_SECURESEL	   = 0008h
AES_OPTIMALFILL    = 0010h
AES_CONSOLECARET   = 0020h
AES_REVIVEDEADKEYS = 0040h

; Assembly Editor messages

AEM_SETMODE	       = WM_USER + 0
AEM_GETMODE	       = WM_USER + 1
AEM_SETPOS	       = WM_USER + 2
AEM_GETPOS	       = WM_USER + 3
AEM_SETSYNTAXHIGHLIGHT = WM_USER + 4
AEM_SETRIGHTCLICKMENU  = WM_USER + 5
AEM_SETTEXTCOLOR       = WM_USER + 6
AEM_SETSELCOLOR        = WM_USER + 7
AEM_FINDFIRST	       = WM_USER + 8
AEM_FINDNEXT	       = WM_USER + 9
AEM_CANFINDNEXT        = WM_USER + 10
AEM_GETLINELENGTH      = WM_USER + 11
AEM_GETLINE	       = WM_USER + 12
AEM_GETWORDATCARET     = WM_USER + 13

; Assembly Editor notifications

AEN_SETFOCUS	= 01h
AEN_KILLFOCUS	= 02h
AEN_TEXTCHANGE	= 03h
AEN_POSCHANGE	= 04h
AEN_MODECHANGE	= 05h
AEN_OUTOFMEMORY = 0Fh

; Assembly Editor position structure

struct AEPOS
  selectionPosition dd ?
  selectionLine     dd ?
  caretPosition     dd ?
  caretLine	    dd ?
ends
