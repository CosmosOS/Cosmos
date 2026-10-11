// DMA ordering barriers for x64
// Counterparts of the ARM64 dmb oshld / dmb oshst pair. x86-64 is TSO for
// the write-back memory DMA buffers live in: loads are not reordered with
// older loads and stores are not reordered with older stores, so neither
// barrier needs an instruction. They stay real calls so the compiler still
// treats each one as an opaque memory clobber and cannot move a DMA-memory
// access across it.

.intel_syntax noprefix

.global _native_dma_rmb
.global _native_dma_wmb

.text

// void _native_dma_rmb(void)
// Orders earlier loads from DMA memory before later loads.
_native_dma_rmb:
    ret

// void _native_dma_wmb(void)
// Orders earlier stores to DMA memory before later stores.
_native_dma_wmb:
    ret
