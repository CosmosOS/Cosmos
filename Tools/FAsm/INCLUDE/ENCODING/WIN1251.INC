
; Windows 1251

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
	dw 401h,403h,201Ah,453h,201Eh,2026h,2020h,2021h,20ACh,2030h,409h,2039h,40Ah,40Ch,40Bh,40Fh
       else if char < 0A0h
	dw 452h,2018h,2019h,201Ch,201Dh,2022h,2013h,2014h,?,2122h,459h,203Ah,45Ah,45Ch,45Bh,45Fh
       else if char < 0B0h
	dw 0A0h,40Eh,45Eh,408h,0A4h,490h,0A6h,0A7h,401h,0A9h,404h,0ABh,0ACh,0ADh,0AEh,407h
       else
	dw 0B0h,0B1h,406h,456h,491h,0B5h,0B6h,0B7h,451h,2116h,454h,0BBh,458h,405h,455h,457h
       end if
       load char word from (char and 0Fh)*2
       end virtual
      else
       char = 410h+char-0C0h
      end if
      store word char at ..data+(%-1)*2
     end if
    end repeat
   end if }

struc du [args]
 { common label . word
   du args }
