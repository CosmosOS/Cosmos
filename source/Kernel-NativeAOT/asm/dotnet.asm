EXTERN Kernel_Program__EntryPoint
GLOBAL start_dotnet

SECTION .text
[BITS 64]

    start_dotnet:
        call Kernel_Program__EntryPoint
        ret