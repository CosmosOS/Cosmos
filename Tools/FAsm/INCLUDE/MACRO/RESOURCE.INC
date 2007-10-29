
; Macroinstructions for making resource section

macro directory [type,label]
 { common
    local max,count
    count = 0
    max = 0
   forward
    count = count + 1
    if type > max
     max = type
    end if
   common
    root@resource dd 0,%t,0,count shl 16
    repeat max
   forward
    if % = type
     dd type,80000000h+label-root@resource
    end if
   common
    end repeat }

macro resource dir,[id,lang,label]
 { common
    dir:
    local min,max,count,current
   forward
    min = id
    max = id
   common
    count = 0
   forward
    count = count + 1
    if id < min
     min = id
    else if id > max
     max = id
    end if
   common
    dd 0,%t,0,count shl 16
    repeat max-min+1
     current = $
   forward
     if min+%-1 = id
      if current = $
       dd id,80000000h+label#.directory-root@resource
      end if
     end if
   common
    end repeat
    repeat max-min+1
     current = $
   forward
     if min+%-1 = id
      if current = $
       label#.directory dd 0,%t,0,10000h,lang,label-root@resource
       count = 1
      else
       dd lang,label-root@resource
       count = count + 1
      end if
     end if
     label#.resid = id
   common
     local x,y,z,v1,v2
     if count > 1
      store word count at current+0Eh
      x = count shr 1
      while x > 0
       y = x
       while y < count
	z = y
	while z-x >= 0
	 load v1 dword from current+10h+z*8
	 load v2 dword from current+10h+(z-x)*8
	 if v1<v2
	  store dword v1 at current+10h+(z-x)*8
	  store dword v2 at current+10h+z*8
	  load v1 dword from current+10h+z*8+4
	  load v2 dword from current+10h+(z-x)*8+4
	  store dword v1 at current+10h+(z-x)*8+4
	  store dword v2 at current+10h+z*8+4
	 else
	  break
	 end if
	 z = z-x
	end while
	y = y+1
       end while
       x = x shr 1
      end while
     end if
    end repeat }

macro bitmap label,bitmap_file
 { local data,size
   label dd RVA data,size,0,0
   data file bitmap_file:0Eh
   size = $ - data
   align 4 }

macro icon group,[label,icon_file]
 { common local count
    count = 0
   forward local data,size,position
    label dd RVA data,size,0,0
    virtual at 0
     file icon_file:6,16
     load size dword from 8
     load position dword from 12
    end virtual
    data file icon_file:position,size
    count = count+1
   common local header
    align 4
    group dd RVA header,6+count*14,0,0
    header dw 0,1,count
   forward
    file icon_file:6,12
    dw label#.resid
   common
    align 4 }

macro cursor group,[label,cursor_file]
 { common local count
    count = 0
   forward local data,width,height,size,position
    label dd RVA data,size+4,0,0
    virtual at 0
     file cursor_file:6,16
     load width byte from 0
     load height byte from 1
     load size dword from 8
     load position dword from 12
    end virtual
    data file cursor_file:10,4
	 file cursor_file:position,size
    count = count+1
   common local header
    align 4
    group dd RVA header,6+count*14,0,0
    header dw 0,2,count
   forward
    dw width,height,1,0
    dd size+4
    dw label#.resid
   common
    align 4 }

macro menu label
 { local data,size
   label dd RVA data,size,0,0
   data dw 1,4,0,0
   menu_size equ size = $ - data
   menu_level = 1 }

macro menuitem string,id,resinfo,status,type
 { dd MFT_STRING or type+0,status+0,id
   dw resinfo+0
   du string,0
   align 4
   if ~ resinfo eq
    if resinfo and MFR_END
     menu_level = menu_level - 1
    end if
    if resinfo and MFR_POPUP
     menu_level = menu_level + 1
     dd 0
    end if
   end if
   if menu_level = 0
    menu_size
   end if }

macro menuseparator resinfo
 { dd MFT_SEPARATOR,0,0
   dw resinfo+0,0
   if ~ resinfo eq
    if resinfo and MFR_END
     menu_level = menu_level - 1
    end if
   end if
   if menu_level = 0
    menu_size
   end if }

macro dialog label,title,x,y,cx,cy,style,exstyle,menu,fontname,fontsize
 { local data,size,items
   label dd RVA data,size,0,0
   data dd style or DS_SETFONT,exstyle +0
   dw items,x,y,cx,cy
   if menu+0 <> 0
    dw 0FFFFh
   end if
   du menu+0,0,title,0
   if fontname eq
    du 8,'MS Sans Serif',0
   else
    du fontsize+0,fontname,0
   end if
   align 4
   dialog_size equ size = $ - data
   dialog_items equ items = dialog_items_counter
   dialog_items_counter = 0 }

macro dialogitem class,title,id,x,y,cx,cy,style,exstyle
 { dd style or WS_CHILD,exstyle +0
   dw x,y,cx,cy,id
   if class eq 'BUTTON'
    dw 0FFFFh,80h
   else if class eq 'EDIT'
    dw 0FFFFh,81h
   else if class eq 'STATIC'
    dw 0FFFFh,82h
   else if class eq 'LISTBOX'
    dw 0FFFFh,83h
   else if class eq 'SCROLLBAR'
    dw 0FFFFh,84h
   else if class eq 'COMBOBOX'
    dw 0FFFFh,85h
   else
    du class,0
   end if
   if title eqtype 0
    dw 0FFFFh,title
   else
    du title,0
   end if
   dw 0
   align 4
   dialog_items_counter = dialog_items_counter + 1 }

macro enddialog
 { dialog_items
   dialog_size }

macro accelerator label,[fvirt,key,cmd]
 { common
    local data,size
    label dd RVA data,size,0,0
    data:
    accel_count = 0
   forward
    accel_count = accel_count + 1
   common
    size = accel_count * 8
   forward
    accel_count = accel_count - 1
    if accel_count = 0
     dw fvirt or 80h,key
    else
     dw fvirt,key
    end if
    dd cmd }

macro versioninfo label,fileos,filetype,filesubtype,lang,cp,[name,value]
 { common
    local data,size,vivalue,visize
    label dd RVA data,size,0,0
    data dw size,visize,0
    du 'VS_VERSION_INFO',0,0
    vivalue dd 0FEEF04BDh,00010000h
    local version,count,shift,char,filever,productver
    filever = 0
    productver = 0
   forward
    if name eq 'FileVersion' | name eq 'ProductVersion'
     virtual at 0
      db value
      count = $
      version = 0
      shift = 16
      repeat count
       load char from %-1
       if char='.'
	if shift mod 32
	 shift = shift-16
	else
	 shift = shift+32+16
	end if
       else
	version = (version and not (0FFFFh shl shift)) or ((version shr shift and 0FFFFh)*10+char-'0') shl shift
       end if
      end repeat
     end virtual
     if name eq 'FileVersion'
      filever = version
     else if name eq 'ProductVersion'
      productver = version
     end if
    end if
   common
    dq filever,productver
    dd 0,0,fileos,filetype+0,filesubtype+0,0,0
    visize = $ - vivalue
    local sfi_data,sfi_size
    sfi_data dd sfi_size
    du 1,'StringFileInfo',0
    local str_data,str_size
    str_data dd str_size
    du 1,'040904E4',0
   forward
    local vs_data,vs_size,value_data,value_size
    align 4
    vs_data dw vs_size,value_size/2
    du 1,name,0
    align 4
    value_data du value,0
    value_size = $ - value_data
    vs_size = $ - vs_data
   common
    align 4
    str_size = $ - str_data
    sfi_size = $ - sfi_data
    local vfi_data,vfi_size,var_data,var_size
    vfi_data dd vfi_size
    du 1,'VarFileInfo',0,0
    var_data dw var_size,4
    du 0,'Translation',0,0
    dw lang,cp+0
    var_size = $ - var_data
    vfi_size = $ - vfi_data
    size = $ - data }

macro resdata label
{ local data,size
  label dd RVA data,size,0,0
  data = $
  ressize equ size = $ - data }

macro endres
{ ressize
  align 4 }
