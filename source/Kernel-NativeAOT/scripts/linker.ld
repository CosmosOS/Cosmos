/* The bootloader will look at this image and start execution at the symbol
   designated as the entry point. */
ENTRY(start)

/* Tell where the various sections of the object files will be put in the final
   kernel image. */
SECTIONS {
    /* Begin putting sections at 1 MiB, a conventional place for kernels to be
      loaded at by the bootloader. */
    . = 1M;

    /* First put the multiboot header, as it is required to be put very early
      early in the image or the bootloader won't recognize the file format. */
    .boot :
    {
        /* ensure that the multiboot header is at the beginning */
        *(.multiboot2)
    }

    /* .text section */
    .text :
    {
        *(.text)
    }
}