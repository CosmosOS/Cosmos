OUTPUT_FORMAT(elf64-littleriscv)

ENTRY(kmain)

PHDRS
{
    text PT_LOAD;
    rodata PT_LOAD;
    data PT_LOAD;
}

SECTIONS
{
    /* RISC-V kernel in the top 2 GiB, where Limine loads it. The C and
     * assembly objects use the medany code model, whose PC-relative
     * addressing reaches any symbol within 2 GiB of the code. */
    . = 0xffffffff80000000;
    __kernel_start = .;

    . = ALIGN(CONSTANT(MAXPAGESIZE));

    .text : {
        __text_start = .;
        *(.text .text.*)
        __text_end = .;
    } :text

    . = ALIGN(CONSTANT(MAXPAGESIZE));

    .rodata : {
        __rodata_start = .;
        *(.rodata .rodata.*)
        *(.srodata .srodata.*)

        /* .NET NativeAOT EH table - needed for exception handling */
        . = ALIGN(8);
        __dotnet_eh_table_start = .;
        *(.dotnet_eh_table)
        __dotnet_eh_table_end = .;

        /* eh_frame for DWARF CFI - needed to find method boundaries */
        . = ALIGN(8);
        __eh_frame_start = .;
        *(.eh_frame .eh_frame.*)
        __eh_frame_end = .;

        /* eh_frame_hdr for fast lookup */
        . = ALIGN(8);
        __eh_frame_hdr_start = .;
        *(.eh_frame_hdr)
        __eh_frame_hdr_end = .;

        __rodata_end = .;
    } :rodata

    . = ALIGN(CONSTANT(MAXPAGESIZE));

    __modules : {
      __Modules_start = .;     /* Symbol at start of section */
      *(__modules)             /* Include all input sections named __Modules */
      __Modules_end = .;       /* Symbol at end of section */
    }

    . = ALIGN(CONSTANT(MAXPAGESIZE));

    .data : {
        __data_start = .;
        *(.data .data.*)
        /* Small data the compiler places near the global pointer. The kernel
         * never sets gp, so these are reached like any other data. */
        *(.sdata .sdata.*)
        __data_end = .;
    } :data

    .bss : {
        __bss_start = .;
        *(.sbss .sbss.*)
        *(.bss .bss.*)
        *(COMMON)
        __bss_end = .;
    } :data

    /* Cosmos OS Memory Structure:
     * Mark the end of the kernel image - heap will start after this + alignment
     * Following Cosmos convention:
     * - Kernel image ends here
     * - 1KB safety gap
     * - Align to 4KB page boundary
     * - Heap starts (managed by PageAllocator using Limine memory map)
     * - RAT at end of heap (1 byte per 4KB page)
     */
    . = ALIGN(CONSTANT(MAXPAGESIZE));
    __kernel_end = .;

    /DISCARD/ : {
        *(.note .note.*)
        *(.comment)
        *(.riscv.attributes)
    }
}
