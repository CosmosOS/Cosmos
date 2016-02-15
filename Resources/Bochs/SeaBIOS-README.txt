--------------------------------------------
| SeaBIOS (http://www.seabios.org/SeaBIOS) |
--------------------------------------------

SeaBIOS is an open source implementation of a 16bit X86 BIOS.

SeaBIOS can run in an emulator or it can run natively on X86 hardware with
the use of coreboot (http://www.coreboot.org).

SeaBIOS is the default BIOS for qemu and kvm.

SeaBIOS may be distributed under the terms of the GNU LGPLv3 license.
Both source code and binaries are available.


--------------------------------------------
|    coreboot (http://www.coreboot.org)    | 
--------------------------------------------

coreboot is a Free Software project aimed at replacing the proprietary BIOS
(firmware) found in most computers. coreboot performs a little bit of
hardware initialization and then executes additional boot logic, called a
payload.

With the separation of hardware initialization and later boot logic,
coreboot can scale from specialized applications that run directly from
firmware, run operating systems in flash, load custom bootloaders, or
implement firmware standards, like  PC BIOS services or  UEFI. This allows
for systems to only include the features necessary in the target
application, reducing the amount of code and flash space required.

coreboot currently supports over 230 different mainboards. Check the
Support page to see if your system is supported.

coreboot was formerly known as LinuxBIOS.
