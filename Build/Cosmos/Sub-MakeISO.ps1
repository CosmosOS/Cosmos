# ----------- Build ISO
remove-item ISO\files\output.obj -ea SilentlyContinue
move-item output.obj ISO\files\output.obj

remove-item ISO\cosmos.iso -ea SilentlyContinue
attrib ISO\files\syslinux\isolinux.bin -r

cd iso
..\..\..\Tools\mkisofs\mkisofs -R -b syslinux/isolinux.bin -no-emul-boot -boot-load-size 4 -boot-info-table -o Cosmos.iso files
cd ..
