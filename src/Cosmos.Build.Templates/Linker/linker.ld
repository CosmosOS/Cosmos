OUTPUT_FORMAT(elf64-x86-64)

ENTRY(kmain)

PHDRS
{
    text PT_LOAD;
    rodata PT_LOAD;
    data PT_LOAD;
    tls PT_TLS;
}

SECTIONS
{
    /* Kernel starts at higher half (-2GB canonical address) */
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
        __data_end = .;
    } :data

    .bss : {
        __bss_start = .;
        *(.bss .bss.*)
        *(COMMON)
        __bss_end = .;
    } :data
    
    /* initialized TLS data */
    .tdata : {
        __tdata_start = .;
        *(.tdata .tdata.*)
        __tdata_end = .;
    } :tls  /* <------ !!!! */

    __tbss_start = .;

    /* uninitialized TLS data */
    .tbss : {
        *(.tbss .tbss.*)
    } :tls  /* <------ !!!! */

    __tbss_end = .;

    __tls_size = __tbss_end - __tdata_start;

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
        *(.eh_frame*)
        *(.note .note.*)
    }
}