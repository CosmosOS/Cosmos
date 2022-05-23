# Build test kernel
Remove the .iso file, and run the commands in the iso root dir:

Compiling  output.bin :

```cmd
nasm -f bin -o output.bin kernel.asm 
```

Build iso:

```cmd 
mkisofs -R -b isolinux.bin -no-emul-boot -boot-load-size 4 -boot-info-table -o boot.iso
```

then use boot.iso as boot image.
