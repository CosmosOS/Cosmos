.global _native_io_read_byte
.global _native_io_read_word
.global _native_io_read_dword
.global _native_io_write_byte
.global _native_io_write_word
.global _native_io_write_dword

.text
.balign 4

// RISC-V has no port I/O space: devices are memory-mapped. These stubs keep
// the symbols the shared code imports, as on ARM64: reads return 0 and
// writes do nothing.

// byte _native_io_read_byte(uint16_t port)
_native_io_read_byte:
    li      a0, 0
    ret

// ushort _native_io_read_word(uint16_t port)
_native_io_read_word:
    li      a0, 0
    ret

// uint _native_io_read_dword(uint16_t port)
_native_io_read_dword:
    li      a0, 0
    ret

// void _native_io_write_byte(uint16_t port, uint8_t value)
_native_io_write_byte:
    ret

// void _native_io_write_word(uint16_t port, uint16_t value)
_native_io_write_word:
    ret

// void _native_io_write_dword(uint16_t port, uint32_t value)
_native_io_write_dword:
    ret
