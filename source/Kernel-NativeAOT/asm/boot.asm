EXTERN start_dotnet
GLOBAL _initial_stack_top
GLOBAL start

SECTION .text

[BITS 64]

    start:
        mov rsp, _initial_stack_top
        mov rbp, _initial_stack_top

        call check_multiboot

        mov edi, ebx ;push multiboot address to first argument (RDI)
        mov esi, _initial_stack_top ;push heap base address to second argument (RSI)

        call start_dotnet
        ret

    ; Throw error if eax doesn't contain the Multiboot2 magic value (0x36d76289).
    check_multiboot:
        cmp eax, 0x36d76289
        jne error
        ret

    ;halt CPU
    error:
        mov rcx, 0xbadcafe
        mov rdx, 0xdeadbabe
        mov rdi, 0xbadcafe
        mov rsi, 0xdeadbabe
        cli
        .hlt:
        hlt
        jmp .hlt ; make sure our program definitely halts, even if HLT is cancelled

SECTION .bss
    _initial_stack_bottom:
        resb 4096
    _initial_stack_top:
