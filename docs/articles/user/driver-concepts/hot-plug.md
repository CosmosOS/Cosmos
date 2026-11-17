# Hot-plug

Hot-plug means a device can arrive or leave while the machine runs. Arrival is the easier half: the bus driver notices the new device, describes it as it would at boot, and the kernel offers it to drivers. Removal is harder, because the driver bound to the device may be in the middle of using it. An orderly removal asks first: on a PCI Express slot the user presses the slot's attention button, the operating system stops the device and powers the slot off, and only then is the card pulled. A surprise removal does not ask, and the card or cable is gone before any software hears of it.

After a surprise removal, touching the device is the trap. A read from a PCI device that is no longer there returns all ones, which a driver can take for a status register with every bit set, or wait on forever for a bit to clear. A write can fault, or reach whatever device the address range is given to next. A driver's teardown must therefore know whether the hardware is still there before it stops it.

PCI Express adds a second problem. The firmware gave addresses only to the devices present at boot, so a function that arrives later has unassigned BARs, which must be placed inside the address windows the port above the slot forwards ([PCI](pci.md)).

[USB](usb.md) and [PS/2](ps2.md) sit at the two extremes. USB was designed for hot-plug: hubs report every connect and disconnect, and the host enumerates each new device. PS/2 was not, and its keyboard and mouse are found once, at boot.

In the driver kit, `OnDetach` receives a `DetachReason` whose `HardwarePresent` is `false` once the device is gone, and the driver must not touch its registers then ([Teardown and OnDetach](../drivers.md#teardown-and-ondetach)). `PcieRootPortDriver` publishes a function that arrives in a hot-plug slot with its unassigned BARs placed inside the port's windows, and retracts it on removal ([Hot-plug slots](../drivers.md#hot-plug-slots)). A USB interface's `UsbAccess.IsDisconnected` turns true once its device is unplugged ([The USB access object](../drivers.md#the-usb-access-object)), while PS/2 ports are only published at boot ([The PS/2 class drivers](../drivers.md#the-ps2-class-drivers)).
