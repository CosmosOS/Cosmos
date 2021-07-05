global start_dotnet

section .text
bits 64

start_dotnet:
  extern stack_top
  extern Kernel_Program__EntryPoint

  call Kernel_Program__EntryPoint

  ret