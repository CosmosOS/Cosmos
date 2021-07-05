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

address_tag:
    dw 2                              ; type
    dw 0                              ; flags
    dd address_tag_end - address_tag  ; size
    dd mb_header                      ; header_addr
    dd start                          ; load_addr
    dd __load_end_addr                ; load_end_addr
    dd stack_top                      ; bss_end_addr
address_tag_end:

entry_addr_tag:
    dw 3                                   ; type
    dw 0                                   ; flags
    dd entry_addr_tag_end - entry_addr_tag ; size
    dd start                               ; header_addr
entry_addr_tag_end:
    ; insert optional multiboot tags here

    ; required end tag
    dw 0    ; type
    dw 0    ; flags
    dd 0    ; size
mb_header_end: