// RISC-V 64 Context Switch Support
// The variables the managed scheduler sets during an interrupt and the trap
// entry (Interrupts.s) reads on its way out.

.global _native_set_context_switch_sp
.global _native_get_context_switch_sp
.global _native_get_sp
.global _native_set_context_switch_new_thread
.global _context_switch_target_sp
.global _context_switch_is_new_thread

.section .bss

// Context switch target SP (0 = no switch, non-zero = switch to this SP).
// Single CPU for now.
.balign 8
_context_switch_target_sp:
    .quad 0

// Whether the target thread is new (1) or resumed (0). The trap exit restores
// both the same way (see Interrupts.s) and only clears it.
.balign 8
_context_switch_is_new_thread:
    .quad 0

.section .text

// void _native_set_context_switch_sp(nuint newSp)
// a0 = SP of the context to switch to (pointing at its saved frame)
.balign 4
_native_set_context_switch_sp:
    la      t0, _context_switch_target_sp
    sd      a0, 0(t0)
    ret

// nuint _native_get_context_switch_sp(void)
.balign 4
_native_get_context_switch_sp:
    la      t0, _context_switch_target_sp
    ld      a0, 0(t0)
    ret

// nuint _native_get_sp(void)
.balign 4
_native_get_sp:
    mv      a0, sp
    ret

// void _native_set_context_switch_new_thread(int isNew)
.balign 4
_native_set_context_switch_new_thread:
    la      t0, _context_switch_is_new_thread
    sd      a0, 0(t0)
    ret
