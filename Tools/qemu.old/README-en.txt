QEMU on Windows

    QEMU is an emulator for various CPUs. It works on Linux, Windows, FreeBSD and Mac OS X.
 Windows version is in an alpha stage.

    Double hyphen "--" is no more needed. Please use a single hyphen for all options.

1. Install

    Please extract ziped file. When extracted, you are ready.

2. Starting a program

    There are two ways to start a program.

2.1 Using a batch file

    A file "qemu-win.bat" starts QEMU. Double click boots Linux on your desktop. 

When you use Command Prompt(MS-DOS Prompt), type

	qemu.exe -L . -hda linux.img

    An option "-hda" set hard disk image and an option "-L" set bios location. 

    When a mouse cursor is missing, type Ctrl and Alt key together. Please use Alt
and Tab on WindowsMe.

    To exit from Linux, press Ctrl-Alt-2 keys together then you will see (qemu) prompt.
Then type quit.

        (qemu) quit

2.2 Using a shortcut

    Create a new shortcut of qemu.exe. Right click on the shortcut, append appropriate
options like "C:\...\qemu.exe" -L . -hda linux.img at link to text field. 
Double click the shortcut.
 
3. Test

    There is a benchmark in linux.img. When Linux boots, type

	sh-2.05b# cd nbench
	sh-2.05b# ./nbench

    INTEGER INDEX and FLOATING-POINT INDEX are index to Pentium 90MHz.

4. x86_64 emulation

    A file "qemu-x86_64.bat" also boots linux on your desktop. It boots 32 bit and 
64 bit OS.

5. Hard disk image

   A hard disk image file can be created by qemu-img.exe.
   To create a 10MB hard disk image file, type in command prompt:

   qemu-img.exe create harddisk.img 10M

6. Floppy and CD-ROM

    You can change a floppy and a CD-ROM image from QEMU Monitor.
    QEMU Monitor can be seen by pressing Ctrl, Alt, 2 key together.
Pressing Ctrl, Alt, 1 key together returns to your guest OS.

    It needs to convert floppy and CD-ROM to image file to use it.

    For floppy, there are many converters. Please choose your favorite one.

    If you have CD-R writing software, you can use it. If you don't have it, 
you can use mkisofs command in cdrtools. 

    To use them together, type:

    qemu.exe -L . -m 128 -boot a -fda floppy.img -hda harddisk.img -cdrom cdimage.iso

  -L : bios location
  -m : memory size (Mbyte)
  -boot : boot device  floppy(a), hard disk(c), CD-ROM(d)
  -fda : floppy image
  -hda : hard disk image
  -cdrom : CD-ROM image
 
When you want to change a floppy and CD-ROM image, type:
(qemu) change fda filename.img
or
(qemu) change cdrom fdimage.iso

7. Uninstall

    Please delete the extracted folder. If you use an installer, the program uses a registry.

8. Notes

    Don't start two QEMU on one hard disk image. It will break the hard disk image.

9. License

    Please refer to files in License folder.
    This program is provided "as is" and without any warranty. Please use it 
at your own risk.

10. Links

  QEMU
	http://fabrice.bellard.free.fr/qemu/
  Bochs BIOS
	http://bochs.sourceforge.net/ 
  VGA BIOS
	http://www.nongnu.org/vgabios/
  MinGW
	http://www.mingw.org/ 
  SDL Library
	http://www.libsdl.org/

Have fun,
    kazu 