# Running
Cosmos supports running on any Intel x86 environment. However for debugging
and ease of use, many virtual and physical setups have integrated support. If
you do not see your environment, this does not mean Cosmos will not work. You
can simply fall back to ISO which should work with any environment.

##  ISO

The ISO option creates an ISO image of the Cosmos output. This ISO file can be
mounted and booted by most virtualization technologies. In addition, a physical
optical disk can be burned and used to boot physical hardware.

## VMWare

WMWare Workstation and VMWare Player are the preferred testing environments
for Cosmos. This is because they are stable, reliable, and easily controlled
by the Cosmos Visual Studio integration package. This allows for easy debugging and code execution control.

VMWare Workstation is not free, however VMWare Player is. VMWare Player for
the typical Cosmos developer provides more than enough functionality.

### VMWare Player

VMWare Player can be downloaded [here](https://www.vmware.com/go/getplayer-win).

### VMWare Workstation

VMWare Workstation can be downloaded for free trial [here](https://www.vmware.com/go/getworkstation-win).

##  QEMU

Not officially supported at this time, but can be done. Just use the ISO option above with debugging turned off.

QEMU can be downloaded [here](https://qemu.weilnetz.de/w64/)

## Bochs

Bochs is officially supported.

Bochs can be downloaded [here](https://bochs.sourceforge.io/getcurrent.html)

## PXE

For PXE, no special software is required. Only an ethernet connection to your router. However, if you have a computer that does not support network booting, or if you must perform the action over WiFi, you may need to look at a PXE chainloader such as [gPXE](http://etherboot.org/wiki/).
To run your Cosmos kernel in PXE you need to follow these steps:

Step #1: disable Cosmos Remote Debugger, activate PXE and compilation in bin format.
Go to Properties and copy these settings:

![image](https://user-images.githubusercontent.com/98218366/178479237-a22a7b5f-250e-432d-ba74-0da502e82f7b.png)

Boot the machine and connect it via Ethernet and will show this screen:

![image](https://user-images.githubusercontent.com/98218366/178479800-c70faa5d-1fab-4da4-8f27-05eefee89b75.png)

Wait and your os will boot up.

## Hyper-V

Not supported at this time.
