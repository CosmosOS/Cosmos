// DMA ordering barriers for RISC-V 64
//
// The pair Linux uses on RISC-V: fence r,r and fence w,w order accesses to
// main memory as a DMA-capable device observes it. _native_riscv64_io_fence
// is the full barrier, device accesses included, that orders descriptor
// stores in RAM before the MMIO store that rings the doorbell.

.global _native_dma_rmb
.global _native_dma_wmb
.global _native_riscv64_io_fence

.text
.balign 4

// void _native_dma_rmb(void)
// Orders earlier loads before later loads, so fields a device wrote are not
// read ahead of the flag that says they are valid.
_native_dma_rmb:
    fence   r, r
    ret

// void _native_dma_wmb(void)
// Orders earlier stores before later stores, so a descriptor is visible to
// the device before the store that hands it over.
_native_dma_wmb:
    fence   w, w
    ret

// void _native_riscv64_io_fence(void)
_native_riscv64_io_fence:
    fence   iorw, iorw
    ret
