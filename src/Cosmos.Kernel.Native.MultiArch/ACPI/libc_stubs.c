// Minimal libc stubs for kernel environment
// Provides the C library functions uACPI and other C code need that the
// runtime does not export (it exports memmove and memset).
// Uses Cosmos MemoryOp for memory operations

#include <stdint.h>
#include <stddef.h>

// Forward declare bridge functions (implemented in C#)
extern void* __cosmos_memcpy(void* dest, void* src, size_t n);
extern int __cosmos_memcmp(const void* s1, const void* s2, size_t n);

// Memory copy function - uses Cosmos MemoryOp bridge
void* memcpy(void* dest, const void* src, size_t n) {
    return __cosmos_memcpy(dest, (void*)src, n);
}

// Memory comparison function - uses Cosmos MemoryOp bridge
int memcmp(const void* s1, const void* s2, size_t n) {
    return __cosmos_memcmp((void*)s1, (void*)s2, n);
}

