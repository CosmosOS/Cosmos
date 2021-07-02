global mb_header
global _start
extern start
extern stack_top
extern __load_end_addr

section .text
bits 32

_start:
mb_header:
    dd 0xe85250d6                              ; magic number (multiboot 2)
    dd 0                                       ; architecture 0 (protected mode i386)
    dd mb_header_end - mb_header ; header length
    ; checksum
    dd 0x100000000 - (0xe85250d6 + 0 + (mb_header_end - mb_header))

    ; required end tag
    dw 0    ; type
    dw 0    ; flags
    dd 0    ; size
mb_header_end: