del cosmos.iso
attrib files\boot\grub\stage2_eltorito -r
..\..\Tools\mkisofs\mkisofs -R -b boot/grub/stage2_eltorito -no-emul-boot -boot-load-size 4 -boot-info-table -o Cosmos.iso files
pause
..\..\tools\qemu\qemu.exe -L ..\..\tools\qemu -cdrom Cosmos.iso -boot d
pause
