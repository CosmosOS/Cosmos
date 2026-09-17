.global _native_efi_call

.text
.align 4

// ulong _native_efi_call(void* function, void* arg1, void* arg2)
//
// Calls a UEFI firmware entry point with two arguments. EFIAPI on AArch64 is
// AAPCS64, the convention the kernel already uses, so this only shifts the
// arguments down one register and tail-calls: the x64 build needs a real
// thunk for the Microsoft convention, and both import the same symbol.
//
// In:  X0 = function, X1 = arg1, X2 = arg2
// Out: X0 = EFI_STATUS
_native_efi_call:
    mov     x9, x0
    mov     x0, x1
    mov     x1, x2
    br      x9
