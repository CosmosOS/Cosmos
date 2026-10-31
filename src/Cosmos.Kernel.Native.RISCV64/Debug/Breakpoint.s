.global _native_debug_breakpoint
.global _native_debug_breakpoint_soft

.text
.balign 4

// void breakpoint()
// Triggers a software breakpoint that a debugger can catch
_native_debug_breakpoint:
    ebreak
    ret

// void breakpoint_soft()
// A softer breakpoint that can be stepped over
_native_debug_breakpoint_soft:
    nop                     // Debugger breakpoint location
    nop
    nop
    ret
