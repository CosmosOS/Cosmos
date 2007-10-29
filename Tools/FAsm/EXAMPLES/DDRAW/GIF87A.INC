
virtual at buffer
  GIFHEADER:
    .ID 	dd ?
    .ver	dw ?
    .width	dw ?
    .height	dw ?
    .bits	db ?
    .background db ?
    .reserved	db ?
    .length	=  $ - GIFHEADER
end virtual

load_picture:

	invoke	CreateFileA,esi,GENERIC_READ,0,0,OPEN_EXISTING,0,0
	mov	edi,eax
	invoke	ReadFile,edi,GIFHEADER,40000h,bytes_count,0
	invoke	CloseHandle,edi

	cmp	[GIFHEADER.ID],'GIF8'
	jne	picture_error
	cmp	[GIFHEADER.ver],'7a'
	jne	picture_error

	mov	al,[GIFHEADER.bits]
	and	al,111b
	cmp	al,111b
	jne	picture_error

	add	[bytes_count],buffer

	mov	esi,buffer+GIFHEADER.length+256*3
	mov	edi,esi

    find_image:
	cmp	esi,[bytes_count]
	jae	picture_error
	lodsb
	cmp	al,','
	jne	find_image
	add	esi,4
	xor	eax,eax
	lodsw
	mov	ebx,eax
	lodsw
	add	esi,2

	mov	[ddsd.dwSize],sizeof.DDSURFACEDESC
	mov	[ddsd.dwFlags],DDSD_CAPS+DDSD_WIDTH+DDSD_HEIGHT+DDSD_CKSRCBLT
	mov	[ddsd.ddsCaps.dwCaps],DDSCAPS_OFFSCREENPLAIN+DDSCAPS_SYSTEMMEMORY
	mov	[ddsd.dwWidth],ebx
	mov	[ddsd.dwHeight],eax
	movzx	eax,[GIFHEADER.background]
	mov	[ddsd.ddckCKSrcBlt.dwColorSpaceLowValue],eax
	mov	[ddsd.ddckCKSrcBlt.dwColorSpaceHighValue],eax
	cominvk DDraw,CreateSurface,\
		ddsd,DDSPicture,0
	or	eax,eax
	jnz	picture_error
	cominvk DDSPicture,Lock,\
		0,ddsd,DDLOCK_WAIT,0

	mov	edi,esi
	mov	edx,esi
	mov	ebx,buffer
	add	ebx,[bytes_count]
    link_streams:
	cmp	esi,[bytes_count]
	jae	picture_error
	lodsb
	movzx	ecx,al
	rep	movsb
	or	al,al
	jnz	link_streams

	mov	edi,[ddsd.lpSurface]
	mov	ebx,edx
	mov	[LZW_bits],0
    LZW_clear:
	xor	edx,edx
    LZW_decompress_loop:
	mov	ch,9
	cmp	edx,(100h-2)*8
	jbe	LZW_read_bits
	mov	ch,10
	cmp	edx,(300h-2)*8
	jbe	LZW_read_bits
	mov	ch,11
	cmp	edx,(700h-2)*8
	jbe	LZW_read_bits
	mov	ch,12
    LZW_read_bits:
	mov	cl,[LZW_bits]
	mov	eax,[ebx]
	shr	eax,cl
	xchg	cl,ch
	mov	esi,1
	shl	esi,cl
	dec	esi
	and	eax,esi
	add	cl,ch
    LZW_read_bits_count:
	cmp	cl,8
	jbe	LZW_read_bits_ok
	sub	cl,8
	inc	ebx
	jmp	LZW_read_bits_count
    LZW_read_bits_ok:
	mov	[LZW_bits],cl
	cmp	eax,100h
	jb	LZW_single_byte
	je	LZW_clear
	sub	eax,102h
	jc	LZW_end
	shl	eax,3
	cmp	eax,edx
	ja	picture_error
	mov	ecx,[LZW_table+eax]
	mov	esi,[LZW_table+eax+4]
	mov	[LZW_table+edx+4],edi
	rep	movsb
	mov	eax,[LZW_table+eax]
	inc	eax
	mov	[LZW_table+edx],eax
	jmp	LZW_decompress_next
    LZW_single_byte:
	mov	[LZW_table+edx],2
	mov	[LZW_table+edx+4],edi
	stosb
    LZW_decompress_next:
	add	edx,8
	jmp	LZW_decompress_loop
    LZW_end:

	cominvk DDSPicture,Unlock,0

	mov	eax,[DDSPicture]
	clc
	ret

    picture_error:
	stc
	ret

load_palette:

	invoke	CreateFileA,esi,GENERIC_READ,0,0,OPEN_EXISTING,0,0
	mov	edi,eax
	invoke	ReadFile,edi,buffer,GIFHEADER.length+256*3,bytes_count,0
	cmp	[bytes_count],GIFHEADER.length+256*3
	jne	picture_error
	invoke	CloseHandle,edi

	cmp	[GIFHEADER.ID],'GIF8'
	jne	picture_error
	cmp	[GIFHEADER.ver],'7a'
	jne	picture_error
	mov	al,[GIFHEADER.bits]
	and	al,111b
	cmp	al,111b
	jne	picture_error

	mov	esi,buffer+GIFHEADER.length
	mov	edi,buffer+400h
	mov	ecx,256
     convert_palette:
	movsw
	movsb
	xor	al,al
	stosb
	loop	convert_palette

	cominvk DDraw,CreatePalette,\
		DDPCAPS_8BIT+DDPCAPS_ALLOW256,buffer+400h,DDPalette,0
	or	eax,eax
	jnz	picture_error

	mov	eax,[DDPalette]
	clc
	ret
