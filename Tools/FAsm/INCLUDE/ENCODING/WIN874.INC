
; Windows 874

macro du [arg]
 { local ..data,char
   ..data du arg
   if arg eqtype ''
    repeat ($-..data)/2
     load char byte from ..data+(%-1)*2
     if char > 7Fh
      if char < 0A0h
       virtual at 0
       if char < 90h
	dw 20ACh,?,?,?,?,2026h,?,?,?,?,?,?,?,?,?,?
       else
	dw ?,2018h,2019h,201Ch,201Dh,2022h,2013h,2014h,?,?,?,?,?,?,?,?
       end if
       load char word from (char and 0Fh)*2
       end virtual
      else if char > 0A0h
       char = 0E00h+char-0A0h
      end if
      store word char at ..data+(%-1)*2
     end if
    end repeat
   end if }

struc du [args]
 { common label . word
   du args }
