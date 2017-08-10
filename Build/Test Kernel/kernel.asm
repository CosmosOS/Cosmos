org 0x1000000
use32
[map symbols]

begin:
_begin:
	jmp entry

align 4
	
multiboot_header:
MULTIBOOT_HEADER_MAGIC dd 0x1BADB002
MULTIBOOT_HEADER_FLAGS dd 0x00010003
Checksum dd -(0x1BADB002 + 0x00010003)
header_addr dd multiboot_header
load_addr dd _begin
load_end dd _end
bss_end dd _end
entry_val dd entry

entry:
	mov byte [0xB8000], 65
	
_loop:
	jmp _loop

_end: