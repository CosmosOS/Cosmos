
; Macroinstruction for making export section

macro export dllname,[label,string]
 { common
    local module,addresses,names,ordinal,count
    count = 0
   forward
    count = count+1
   common
    dd 0,0,0,RVA module,1
    dd count,count,RVA addresses,RVA names,RVA ordinal
    addresses:
   forward
    dd RVA label
   common
    names:
   forward
    local name
    dd RVA name
   common
    ordinal: count = 0
   forward
    dw count
    count = count+1
   common
    module db dllname,0
   forward
    name db string,0
   common
    local x,y,z,str1,str2,v1,v2
    x = count shr 1
    while x > 0
     y = x
     while y < count
      z = y
      while z-x >= 0
       load v1 dword from names+z*4
       str1=($-RVA $)+v1
       load v2 dword from names+(z-x)*4
       str2=($-RVA $)+v2
       while v1 > 0
	load v1 from str1+%-1
	load v2 from str2+%-1
	if v1 <> v2
	 break
	end if
       end while
       if v1<v2
	load v1 dword from names+z*4
	load v2 dword from names+(z-x)*4
	store dword v1 at names+(z-x)*4
	store dword v2 at names+z*4
	load v1 word from ordinal+z*2
	load v2 word from ordinal+(z-x)*2
	store word v1 at ordinal+(z-x)*2
	store word v2 at ordinal+z*2
       else
	break
       end if
       z = z-x
      end while
      y = y+1
     end while
     x = x shr 1
    end while }
