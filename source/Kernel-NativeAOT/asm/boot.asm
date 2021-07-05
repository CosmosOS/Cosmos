global start
global stack_top
global __load_end_addr
extern start_dotnet

section .text

start:
    ; print `OK` to screen
    mov dword [0xb8000], 0x2f4b2f4f

    mov esp, stack_top

    call check_multiboot

    extern start_dotnet
    call start_dotnet 

; Prints `ERR: ` and the given error code to screen and hangs.
; parameter: error code (in ascii) in al
error:
    mov dword [0xb8000], 0x4f524f45
    mov dword [0xb8004], 0x4f3a4f52
    mov dword [0xb8008], 0x4f204f20
    mov byte  [0xb800a], al
    hlt

; Throw error 0 if eax doesn't contain the Multiboot 2 magic value (0x36d76289).
check_multiboot:
    cmp eax, 0x36d76289
    jne .no_multiboot
    ret
.no_multiboot:
    mov al, "0"
    jmp error

section .bss
align 4096

stack_bottom:
    resb 4096 * 2
stack_top: