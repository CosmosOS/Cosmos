EXTERN start_dotnet
GLOBAL _initial_stack_top
GLOBAL start
GLOBAL multiboot_magic_var
GLOBAL multiboot_tags_address_var

SECTION .text

[BITS 64]

    start:
        mov esp, _initial_stack_top
        call check_multiboot
        mov rdi, rax ;put multiboot in first funcion argument
        call start_dotnet

    ;halt CPU
    error:
        hlt

    ; Throw error 0 if eax doesn't contain the Multiboot 2 magic value (0x36d76289).
    check_multiboot:
        cmp eax, 0x36d76289
        jne .no_multiboot
        ret
    .no_multiboot:
        mov al, "0"
        jmp error

SECTION .bss
    _initial_stack_bottom:
        resb 0x20000
    _initial_stack_top: