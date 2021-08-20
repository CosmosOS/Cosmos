EXTERN start_dotnet
GLOBAL _initial_stack_top
GLOBAL start

SECTION .text

[BITS 64]

    start:
        mov rsp, 0
        mov rbp, 0
        mov esp, _initial_stack_top
        mov ebp, _initial_stack_top

        call check_multiboot

        mov rdi, 0
        mov rsi, 0
        mov edi, ebx ;push multiboot2 address to first argument (RDI)
        mov esi, _initial_stack_top ;push heap base address to second argument (RSI)

        call start_dotnet

        cli
        hlt

    ; Throw error if eax doesn't contain the Multiboot2 magic value (0x36d76289).
    check_multiboot:
        cmp eax, 0x36d76289
        jne error
        ret

    ;halt CPU
    error:
        mov rax, 0xbadcafe
        mov rdx, 0xdeadbabe
        cli
        .hlt:
        hlt
        jmp .hlt ; make sure our program definitely halts, even if HLT is cancelled

SECTION .bss

    ALIGN 16
    _initial_stack_bottom:
        resb 4096
    _initial_stack_top:
