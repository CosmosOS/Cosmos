GLOBAL start_dotnet

SECTION .text
[BITS 64]

  start_dotnet:
    extern stack_top
    extern Kernel_Program__EntryPoint

    call Kernel_Program__EntryPoint

    ret