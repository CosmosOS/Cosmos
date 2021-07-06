GLOBAL __load_end_addr

SECTION .data
[BITS 64]

    __load_end_addr:
    ; marker to be linked last so that we have a reference
    ; to the end of the load section