// DMA ordering barriers for ARM64
// The dma_rmb / dma_wmb pair: DMB in the Outer Shareable domain, which is
// where DMA-capable masters observe memory. DMB rather than DSB because
// only the order of accesses matters here, not their completion; the
// heavier dsb sy + isb stays for page-table and system-register work.

.global _native_dma_rmb
.global _native_dma_wmb

.text
.align 4

// void _native_dma_rmb(void)
// Orders earlier loads before later loads and stores, so fields a device
// wrote are not read ahead of the flag that says they are valid.
_native_dma_rmb:
    dmb     oshld
    ret

// void _native_dma_wmb(void)
// Orders earlier stores before later stores, so a descriptor is visible
// to the device before the store that hands it over.
_native_dma_wmb:
    dmb     oshst
    ret
