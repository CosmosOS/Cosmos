// ARM64 Context Switch Support
// Functions for preemptive multithreading on ARM64. The context switch is staged in the calling
// CPU's per-CPU block (Core/CPU/PerCpuBlock.cs, the layout's home), reached through TPIDR_EL1: the vector in
// Interrupts.s reads the same slots on every exception exit, so the staging and the switch that
// consumes it never see another CPU's.

.equ PERCPU_SWITCH_TARGET_SP,     0x18
.equ PERCPU_SWITCH_IS_NEW_THREAD, 0x20
.equ PERCPU_SWITCH_PREV_ON_CPU,   0x60

.global _native_set_context_switch_sp
.global _native_get_context_switch_sp
.global _native_get_sp
.global _native_set_context_switch_new_thread
.global _native_set_context_switch_prev_on_cpu

.section .text

// void _native_set_context_switch_prev_on_cpu(nuint slot)
// Stages the address of the outgoing thread's on-CPU marker: the vector
// stores 0 through it once it has switched stacks, so another CPU may resume
// that thread. 0 for none.
// x0 = address of the marker
.balign 4
_native_set_context_switch_prev_on_cpu:
    mrs     x1, tpidr_el1
    str     x0, [x1, #PERCPU_SWITCH_PREV_ON_CPU]
    ret

// void _native_set_context_switch_sp(nuint newSp)
// Stages a context switch on the calling CPU: the vector that runs on this
// exception's exit switches to the saved context at newSp. Called from
// managed code during the timer interrupt.
// x0 = new SP to switch to (pointing to saved context)
.balign 4
_native_set_context_switch_sp:
    mrs     x1, tpidr_el1
    str     x0, [x1, #PERCPU_SWITCH_TARGET_SP]
    ret

// nuint _native_get_context_switch_sp(void)
// The switch staged on the calling CPU, 0 when none is (the "already
// staged" guard of the IRQ-exit reschedule)
.balign 4
_native_get_context_switch_sp:
    mrs     x1, tpidr_el1
    ldr     x0, [x1, #PERCPU_SWITCH_TARGET_SP]
    ret

// nuint _native_get_sp(void)
// Gets the current SP value
.balign 4
_native_get_sp:
    mov     x0, sp
    ret

// void _native_set_context_switch_new_thread(int isNew)
// Sets whether the target thread is NEW (1) or RESUMED (0)
// w0 = isNew flag (an int: the upper half of x0 is not part of the argument)
.balign 4
_native_set_context_switch_new_thread:
    mov     w0, w0
    mrs     x1, tpidr_el1
    str     x0, [x1, #PERCPU_SWITCH_IS_NEW_THREAD]
    ret
