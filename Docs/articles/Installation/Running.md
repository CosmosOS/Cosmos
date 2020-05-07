Cosmos supports running on any Intel x86 environment. However for debugging
and ease of use, many virtual and physical setups have integrated support. If
you do not see your environment, this does not mean Cosmos will not work. You
can simply fall back to ISO which should work with any environment.

###  ISO

The ISO option creates an ISO image of the Cosmos output. This ISO file can be
mounted and booted by most virtualization technologies. In addition, a physical
optical disk can be burned and used to boot physical hardware.

###  VMWare Workstation/Player

WMWare Workstation and VMWare Player are the preferred testing environments
for Cosmos. This is because they are stable, reliable, and easily controlled
by the Cosmos Visual Studio integration package. This allows for easy debugging and code execution control.

VMWare Workstation is not free, however VMWare Player is. VMWare Player for
the typical Cosmos developer provides more than enough functionality.

###  VMWare Player

VMWare Player can be downloaded [ here](http://vmware.com/download/player/).

###  VMWare Server

&nbsp;_place_holder;

###  QEMU

Not officially supported at this time, but can be done. Just use the ISO option above with debugging turned off.

Bochs

Support is underway.

PXE

For PXE, no special software is required. Only an ethernet connection to your router. However, if you have a computer that does not support network booting, or if you must perform the action over WiFi, you may need to look at a PXE chainloader such as [gPXE](#).

Virtual PC

Not officially supported at this time, but can be done. Just use the ISO option above with debugging turned off.

Hyper-V

Not supported at this time.
