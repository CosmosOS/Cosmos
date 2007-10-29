
; Windows 1253

macro du [arg]
 { local ..data,char
   ..data du arg
   if arg eqtype ''
    repeat ($-..data)/2
     load char byte from ..data+(%-1)*2
     if char > 7Fh
      if char < 0C0h
       virtual at 0
       if char < 90h
	dw 20ACh,?,201Ah,192h,201Eh,2026h,2020h,2021h,?,2030h,?,2039h,?,?,?,?
       else if char < 0A0h
	dw ?,2018h,2019h,201Ch,201Dh,2022h,2013h,2014h,?,2122h,?,203Ah,?,?,?,?
       else if char < 0B0h
	dw 0A0h,385h,386h,0A3h,0A4h,0A5h,0A6h,0A7h,0A8h,0A9h,?,0ABh,0ACh,0ADh,0AEh,2015h
       else
	dw 0B0h,0B1h,0B2h,0B3h,384h,0B5h,0B6h,0B7h,288h,389h,38Ah,0BBh,38Ch,0BDh,38Eh,38Fh
       end if
       load char word from (char and 0Fh)*2
       end virtual
      else
       char = 390h+char-0C0h
      end if
      store word char at ..data+(%-1)*2
     end if
    end repeat
   end if }

struc du [args]
 { common label . word
   du args }
