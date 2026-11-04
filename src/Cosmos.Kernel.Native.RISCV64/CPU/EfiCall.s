.global _native_efi_call

.text
.balign 4

// ulong _native_efi_call(void* function, void* arg1, void* arg2)
//
// Calls a UEFI firmware entry point with two arguments. EFIAPI on RISC-V 64
// is the standard LP64D convention the kernel already uses, so this only
// shifts the arguments down one register and tail-calls.
//
// In:  a0 = function, a1 = arg1, a2 = arg2
// Out: a0 = EFI_STATUS
_native_efi_call:
    mv      t0, a0
    mv      a0, a1
    mv      a1, a2
    jr      t0
