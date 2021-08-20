EXTERN Kernel_Program__EntryPoint
EXTERN _initial_stack_top
GLOBAL start_dotnet
GLOBAL Halt
GLOBAL ClearInterrupts

SECTION .text
[BITS 64]

    start_dotnet:
        call Kernel_Program__EntryPoint
        ret

    Halt:
        hlt
        ret

    ClearInterrupts:
        cli
        ret
