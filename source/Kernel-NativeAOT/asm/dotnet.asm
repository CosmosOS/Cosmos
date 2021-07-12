EXTERN Kernel_Program__EntryPoint
EXTERN _initial_stack_top
GLOBAL start_dotnet
GLOBAL Out8
GLOBAL Out16
GLOBAL Out32
GLOBAL In8
GLOBAL In16
GLOBAL In32
GLOBAL Halt
GLOBAL ClearInterrupts

SECTION .text
[BITS 64]

    start_dotnet:
        call Kernel_Program__EntryPoint
        ret

    ; rdi port
    ; rsi value
    Out8:
        push rax
        push rdx
        mov rdx, rdi
        mov rax, rsi
        ; rax has the value now
        ; rsi has the port
        out dx, al
        pop rdx
        pop rax
        ret

    ; rdi port
    ; rsi value
    Out16:
        push rax
        push rdx
        mov rdx, rdi
        mov rax, rsi
        ; rax has the value now
        ; rsi has the port
        out dx, ax
        pop rdx
        pop rax
        ret
  
    ; rdi port
    ; rsi value
    Out32:
        push rax
        push rdx
        mov rdx, rdi
        mov rax, rsi
        ; rax has the value now
        ; rsi has the port
        out dx, eax
        pop rdx
        pop rax
        ret

    ; rdi port
    In8:
        push rdx
        mov rdx, rdi
        ; rdx has the port
        in al, dx
        pop rdx
        ret

    ; rdi port
    In16:
        push rdx
        mov rdx, rdi
        ; rdx has the port
        in ax, dx
        pop rdx
        ret

    ; rdi port
    In32:
        push rdx
        mov rdx, rdi
        ; rdx has the port
        in eax, dx
        pop rdx
        ret

    Halt:
        hlt
        ret

    ClearInterrupts:
        cli
        ret
