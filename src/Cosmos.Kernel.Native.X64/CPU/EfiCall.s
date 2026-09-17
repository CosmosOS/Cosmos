.intel_syntax noprefix

.global _native_efi_call

.text

// ulong _native_efi_call(void* function, void* arg1, void* arg2)
//
// Calls a UEFI firmware entry point with two arguments. EFIAPI on x86-64 is
// the Microsoft x64 convention: arguments in RCX, RDX, R8, R9 and a 32-byte
// shadow area above the return address. The kernel is compiled for the
// System V convention (RDI, RSI, RDX, RCX, R8, R9, no shadow area), so a
// direct call hands the firmware whatever RCX and RDX happen to hold.
//
// Every register the Microsoft callee may clobber (RAX, RCX, RDX, R8-R11,
// XMM0-XMM5) is caller-saved under System V, and every register System V
// expects preserved (RBX, RBP, R12-R15) is preserved by the Microsoft callee
// too, so the arguments are the only thing to re-home.
//
// In:  RDI = function, RSI = arg1, RDX = arg2
// Out: RAX = EFI_STATUS
_native_efi_call:
    push    rbp
    mov     rbp, rsp
    mov     rax, rdi            // function
    mov     rcx, rsi            // arg1 -> first Microsoft argument
                                // arg2 is already in RDX, the second one
    sub     rsp, 32             // shadow area; keeps RSP 16-byte aligned
    call    rax
    mov     rsp, rbp
    pop     rbp
    ret
