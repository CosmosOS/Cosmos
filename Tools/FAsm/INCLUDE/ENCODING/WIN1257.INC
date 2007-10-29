
; Windows 1257

macro du [arg]
 { local ..data,char
   ..data du arg
   if arg eqtype ''
    repeat ($-..data)/2
     load char byte from ..data+(%-1)*2
     if char > 7Fh
      virtual at 0
      if char < 90h
       dw 20ACh,?,201Ah,?,201Eh,2026h,2020h,2021h,?,2030h,?,2039h,?,0A8h,2C7h,0B8h
      else if char < 0A0h
       dw ?,2018h,2019h,201Ch,201Dh,2022h,2013h,2014h,?,2122h,?,203Ah,?,0AFh,2DBh,?
      else if char < 0B0h
       dw 0A0h,?,0A2h,0A3h,0A4h,?,0A6h,0A7h,0D8h,0A9h,156h,0ABh,0ACh,0ADh,0AEh,0C6h
      else if char < 0C0h
       dw 0B0h,0B1h,0B2h,0B3h,0B4h,0B5h,0B6h,0B7h,0F8h,0B9h,157h,0BBh,0BCh,0BDh,0BEh,0E6h
      else if char < 0D0h
       dw 104h,12Eh,100h,106h,0C4h,0C5h,118h,112h,10Ch,0C9h,179h,116h,122h,136h,12Ah,13Bh
      else if char < 0E0h
       dw 160h,143h,145h,0D3h,14Ch,0D5h,0D6h,0D7h,172h,141h,15Ah,16Ah,0DCh,17Bh,17Dh,0DFh
      else if char < 0F0h
       dw 105h,12Fh,101h,107h,0E4h,0E5h,119h,113h,10Dh,0E9h,17Ah,117h,123h,137h,12Bh,13Ch
      else
       dw 161h,144h,146h,0F3h,14Dh,0F5h,0F6h,0F7h,173h,142h,15Bh,16Bh,0FCh,17Ch,17Eh,2D9h
      end if
      load char word from (char and 0Fh)*2
      end virtual
      store word char at ..data+(%-1)*2
     end if
    end repeat
   end if }

struc du [args]
 { common label . word
   du args }
