
; Win32 programming headers (ASCII)

include 'macro/struct.inc'
include 'macro/proc32.inc'
include 'macro/com32.inc'
include 'macro/import32.inc'
include 'macro/export.inc'
include 'macro/resource.inc'

struc TCHAR [val] { common match any, val \{ . db val \}
			   match , val \{ . db ? \} }

include 'equates/kernel32.inc'
include 'equates/user32.inc'
include 'equates/gdi32.inc'
include 'equates/comctl32.inc'
include 'equates/comdlg32.inc'
include 'equates/shell32.inc'
include 'equates/wsock32.inc'

macro api [name] { if used name
		    label name dword at name#A
		   end if }
