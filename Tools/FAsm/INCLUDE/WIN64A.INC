
; Win64 programming headers (ASCII)

include 'macro/struct.inc'
include 'macro/proc64.inc'
include 'macro/com64.inc'
include 'macro/import64.inc'
include 'macro/export.inc'
include 'macro/resource.inc'

struc TCHAR [val] { common match any, val \{ . db val \}
			   match , val \{ . db ? \} }

include 'equates/kernel64.inc'
include 'equates/user64.inc'
include 'equates/gdi32.inc'
include 'equates/comctl64.inc'
include 'equates/comdlg64.inc'
include 'equates/shell64.inc'

macro api [name] { if used name
		    label name qword at name#A
		   end if }

