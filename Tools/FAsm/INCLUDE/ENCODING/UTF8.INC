
; UTF-8

macro du [arg]
 { local current,char
   if arg eqtype ''
    virtual at 0
     db arg
     count = $
    end virtual
    current = 0
    while current < count
     virtual at 0
     db arg
     load char byte from current
     wide = char
     current = current + 1
     if char > 0C0h
      if char < 0E0h
       wide = char and 11111b
       load char byte from current
       wide = wide shl 6 + (char and 111111b)
       current = current + 1
      else if char < 0F0h
       wide = char and 1111b
       load char byte from current
       wide = wide shl 6 + (char and 111111b)
       load char byte from current+1
       wide = wide shl 6 + (char and 111111b)
       current = current + 2
      else if char < 0F8h
       wide = char and 111b
       load char byte from current
       wide = wide shl 6 + (char and 111111b)
       load char byte from current+1
       wide = wide shl 6 + (char and 111111b)
       load char byte from current+2
       wide = wide shl 6 + (char and 111111b)
       current = current + 3
      else if char < 0FCh
       wide = char and 11b
       load char byte from current
       wide = wide shl 6 + (char and 111111b)
       load char byte from current+1
       wide = wide shl 6 + (char and 111111b)
       load char byte from current+2
       wide = wide shl 6 + (char and 111111b)
       load char byte from current+3
       wide = wide shl 6 + (char and 111111b)
       current = current + 4
      else
       wide = char and 1
       load char byte from current
       wide = wide shl 6 + (char and 111111b)
       load char byte from current+1
       wide = wide shl 6 + (char and 111111b)
       load char byte from current+2
       wide = wide shl 6 + (char and 111111b)
       load char byte from current+3
       wide = wide shl 6 + (char and 111111b)
       load char byte from current+4
       wide = wide shl 6 + (char and 111111b)
       current = current + 5
      end if
     end if
     end virtual
     if wide < 10000h
      dw wide
     else
      dw 0D7C0h + wide shr 10,0DC00h or (wide and 3FFh)
     end if
    end while
   else
    dw arg
   end if }

struc du [args]
 { common label . word
   du args }
