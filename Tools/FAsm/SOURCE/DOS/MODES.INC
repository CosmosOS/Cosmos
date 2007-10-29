
; flat assembler interface for DOS
; Copyright (c) 1999-2007, Tomasz Grysztar.
; All rights reserved.

segment modes use16

real32:
	mov	ax,7202h
	push	ax
	popf
	pushf
	pop	bx
	cmp	ax,bx
	je	processor_ok
	call	init_error
	db	'required 80386 or better',24h
    processor_ok:
	mov	eax,ds
	shl	eax,4
	mov	[program_base],eax
	mov	eax,buffer
	shl	eax,4
	sub	eax,[program_base]
	mov	[buffer_address],eax
	smsw	ax
	test	al,1
	jnz	dpmi
	mov	eax,cs			; calculate linear address of GDT
	shl	eax,4
	or	dword [cs:real32_GDT+10],eax
	or	dword [cs:real16_GDT+10],eax
	add	[cs:real32_GDT_address],eax
	add	[cs:real16_GDT_address],eax
	cli
	lgdt	[cs:real32_GDTR]
	mov	eax,cr0
	or	al,1
	mov	cr0,eax
	jmp	1 shl 3:test_pm32
    no_rm32:
	sti
	jmp	dpmi
    test_pm32:
	use32
	mov	eax,cr0
	and	al,not 1
	mov	cr0,eax
	mov	ebx,0FFFFh
	jmp	modes:test_rm32
    test_rm32:
	inc	ebx
	jz	short no_rm32
	lgdt	[cs:real16_GDTR]
	mov	eax,cr0
	or	al,1
	mov	cr0,eax
	jmp	1 shl 3:test_pm16
    test_pm16:
	use16
	mov	eax,cr0
	and	al,not 1
	mov	cr0,eax
	jmp	modes:test_rm16
    test_rm16:
	sti
	mov	bx,(400h+(100h*interrupt.size)) shr 4
	mov	ah,48h
	int	21h
	jnc	mem_ok
    not_enough_memory:
	call	init_error
	db	'not enough conventional memory',24h
    mem_ok:
	push	ds es
	mov	es,ax
	push	cs
	pop	ds
	movzx	eax,ax
	shl	eax,4
	mov	[real32_IDT_base],eax
	mov	dx,100h
	xor	bx,bx
	mov	di,400h
    init_interrupts:
	mov	si,interrupt
	mov	[si+interrupt.vector],bx
	mov	word [es:bx],di
	mov	word [es:bx+2],es
	mov	cx,interrupt.size
	rep	movsb
	add	bx,4
	dec	dx
	jnz	init_interrupts
	pop	es ds
	mov	[mode],real32
	call	modes:switch_real32
	use32
	retfw
	use16
    init_error:
	push	cs
	pop	ds
	mov	dx,init_error_prefix
	mov	ah,9
	int	21h
	pop	dx
	int	21h
	mov	dx,init_error_suffix
	int	21h
	mov	ax,04CFFh
	int	21h

switch_real32:
	pushfw
	push	eax
	push	word ds
	push	word es
	push	word fs
	push	word gs
	cli
	mov	eax,ss
	mov	cr3,eax
	lgdt	[cs:real32_GDTR]
	mov	eax,cr0 		; switch to protected mode
	or	al,1
	mov	cr0,eax
	jmp	1 shl 3:pm32_start
    pm32_start:
	use32
	mov	ax,2 shl 3		; load 32-bit data descriptor
	mov	ds,ax			; to all data segment registers
	mov	es,ax
	mov	fs,ax
	mov	gs,ax
	mov	ss,ax
	mov	eax,cr0 		; switch back to real mode
	and	al,not 1
	mov	cr0,eax
	jmp	modes:pm32_end
    pm32_end:
	mov	eax,cr3
	mov	ss,ax
	lidt	[cs:real32_IDTR]
	pop	word gs
	pop	word fs
	pop	word es
	pop	word ds
	pop	eax
	popfw
	retfw

switch_real16:
	pushfw
	push	eax
	cli
	lgdt	[cs:real16_GDTR]
	mov	eax,cr0 		; switch to protected mode
	or	al,1
	mov	cr0,eax
	jmp	1 shl 3:pm16_start
    pm16_start:
	use16
	mov	eax,cr0 		; switch back to real mode
	and	al,not 1
	mov	cr0,eax
	jmp	modes:pm16_end
    pm16_end:
	lidt	[cs:real16_IDTR]
	pop	eax
	popfw
	retfd
	use32

interrupt:
	call	modes:switch_real16
	use16
	movzx	esp,sp
	push	word [esp+4]
	push	cs
	call	.real16
	pushfw
	pop	word [esp+4]
	call	modes:switch_real32
	use32
	iretw
      .real16:
	use16
	push	eax
	push	ds
	xor	ax,ax
	mov	ds,ax
	mov	eax,[word 0]
	label	.vector word at $-2-interrupt
	pop	ds
	xchg	eax,[esp]
	retfw
      .size = $-interrupt

label real32_GDTR pword
real32_GDT_limit dw 3*8-1		; limit of GDT
real32_GDT_address dd real32_GDT	; linear address of GDT
real32_GDT rw 4 			; null descriptor
	   dw 0FFFFh,0,9A00h,0CFh	; 32-bit code descriptor
	   dw 0FFFFh,0,9200h,08Fh	; 4 GB data descriptor
label real16_GDTR pword
real16_GDT_limit dw 2*8-1		; limit of GDT
real16_GDT_address dd real16_GDT	; linear address of GDT
real16_GDT rw 4 			; null descriptor
	   dw 0FFFFh,0,9A00h,0		; 16-bit code descriptor

label real32_IDTR pword
real32_IDT_limit dw 3FFh
real32_IDT_base dd ?
label real16_IDTR pword
real16_IDT_limit dw 3FFh
real16_IDT_base dd 0

init_error_prefix db 0Dh,0Ah,'error: ',24h
init_error_suffix db '.',0Dh,0Ah,24h

dpmi:
	mov	ax,1687h
	int	2Fh
	or	ax,ax			; DPMI installed?
	jnz	no_dpmi
	test	bl,1			; 32-bit programs supported?
	jz	no_dpmi
	mov	word [cs:mode_switch],di
	mov	word [cs:mode_switch+2],es
	mov	bx,si			; allocate memory for DPMI data
	mov	ah,48h
	int	21h
	jc	not_enough_memory
	mov	ds,[environment_segment]
	mov	es,ax
	mov	ax,1
	call	far [cs:mode_switch]	; switch to protected mode
	jc	no_dpmi
	mov	cx,1
	xor	ax,ax
	int	31h			; allocate descriptor for code
	mov	si,ax
	xor	ax,ax
	int	31h			; allocate descriptor for data
	mov	di,ax
	mov	dx,cs
	lar	cx,dx
	shr	cx,8
	or	cx,0C000h
	mov	bx,si
	mov	ax,9
	int	31h			; set code descriptor access rights
	mov	dx,ds
	lar	cx,dx
	shr	cx,8
	or	cx,0C000h
	mov	bx,di
	int	31h			; set data descriptor access rights
	mov	ecx,main
	shl	ecx,4
	mov	dx,cx
	shr	ecx,16
	mov	ax,7
	int	31h			; set data descriptor base address
	movzx	ecx,word [esp+2]
	shl	ecx,4
	mov	dx,cx
	shr	ecx,16
	mov	bx,si
	int	31h			; set code descriptor base address
	mov	cx,0FFFFh
	mov	dx,0FFFFh
	mov	ax,8			; set segment limit to 4 GB
	int	31h
	mov	bx,di
	int	31h
	mov	ax,ds
	mov	ds,di
	mov	[psp_segment],es
	mov	[environment_segment],ax
	mov	es,di
	mov	[mode],dpmi
	pop	ebx
	movzx	ebx,bx
	push	esi
	push	ebx
	retfd

no_dpmi:
	smsw	ax
	test	al,1
	jz	no_real32
	call	init_error
	db	'system is in protected mode without DPMI services',24h
no_real32:
	call	init_error
	db	'processor is not able to enter 32-bit real mode',24h

mode_switch dd ?
