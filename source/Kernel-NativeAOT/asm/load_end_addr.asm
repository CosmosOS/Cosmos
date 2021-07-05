global __load_end_addr

section .data
bits 64

__load_end_addr:
  ; marker to be linked last so that we have a reference
  ; to the end of the load section