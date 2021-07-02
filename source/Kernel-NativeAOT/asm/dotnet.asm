global start_dotnet

section .text
bits 64

start_dotnet:
  extern stack_top
  extern Kernel_Program__EntryPoint

  extern __modules_a
  extern __modules_z

  ; subtract __modules_z - __modules_a to get the size of memory for all module entries (8 bytes each)
  lea      rax, [__modules_z]
  lea      rcx, [__modules_a]
  sub      rax, rcx
  ; divide the size by 8 to get the number of modules
  sar      rax, 3
  mov      rsi, rax
  ; rsi is now the count of modules

  lea      rdi, [__modules_a]
  mov      rdx, stack_top
  call Kernel_Program__EntryPoint

  ret