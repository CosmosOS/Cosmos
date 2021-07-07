EXTERN Kernel_Program__EntryPoint
EXTERN _initial_stack_top
GLOBAL start_dotnet

SECTION .text
[BITS 64]

    start_dotnet:
        mov rdx, _initial_stack_top
        call Kernel_Program__EntryPoint
        ret