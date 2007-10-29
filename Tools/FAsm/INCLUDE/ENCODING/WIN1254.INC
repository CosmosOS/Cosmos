
; Windows 1254

macro du [arg]
 { local ..data,char
   ..data du arg
   if arg eqtype ''
    repeat ($-..data)/2
     load char byte from ..data+(%-1)*2
     if char > 7Fh & char < 0A0h
      virtual at 0
      if char < 90h
       dw 20ACh,?,201Ah,192h,201Eh,2026h,2020h,2021h,2C6h,2030h,160h,2039h,152h,?,?,?
      else
       dw ?,2018h,2019h,201Ch,201Dh,2022h,2013h,2014h,2DCh,2122h,161h,203Ah,153h,?,?,178h
      end if
      load char word from (char and 0Fh)*2
      end virtual
      store word char at ..data+(%-1)*2
     else if char = 0D0h
      store word 11Eh at ..data+(%-1)*2
     else if char = 0DDh
      store word 130h at ..data+(%-1)*2
     else if char = 0DEh
      store word 15Eh at ..data+(%-1)*2
     else if char = 0F0h
      store word 11Fh at ..data+(%-1)*2
     else if char = 0FDh
      store word 131h at ..data+(%-1)*2
     else if char = 0FEh
      store word 15Fh at ..data+(%-1)*2
     end if
    end repeat
   end if }

struc du [args]
 { common label . word
   du args }
