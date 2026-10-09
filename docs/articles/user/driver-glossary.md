# Driver glossary

These notes give the background on the hardware and operating system concepts that [Writing a driver](drivers.md) builds on, for a reader who has not written a driver before. Each page explains the concept on its own, then closes with how the Cosmos driver kit applies it. The table groups them in three parts: how a driver reaches a device, how the device and the driver signal each other, then the buses. Interrupt context also builds on two scheduling notes from the contributor documentation, [preemption](../dev/sched-concepts/preemption.md) and the [instruction boundary](../dev/sched-concepts/instruction-boundary.md).

| Concept | Summary |
|---------|---------|
| [MMIO and port I/O](driver-concepts/registers.md) | Reaching device registers through memory or ports, and how mappings cache them |
| [Physical addresses](driver-concepts/physical-addresses.md) | Virtual against physical addresses, physical contiguity, and the alignment devices require |
| [DMA](driver-concepts/dma.md) | Devices reading and writing memory themselves: bus mastering, coherence, 32-bit limits |
| [Memory barriers](driver-concepts/barriers.md) | Ordering CPU loads and stores so a device sees them in order |
| [Descriptor rings](driver-concepts/descriptor-rings.md) | Circular descriptor arrays shared with a device: ownership, head and tail, doorbells |
| [Interrupt delivery](driver-concepts/interrupt-delivery.md) | IRQs, acknowledging the device, edge and level triggering, INTx against MSI-X |
| [Interrupt context](driver-concepts/interrupt-context.md) | Why a handler cannot wait, why a driver's handler must not allocate, and how its work is split |
| [Buses and enumeration](driver-concepts/buses.md) | Enumerable buses, platform devices described by firmware, and the node tree |
| [Firmware handoff](driver-concepts/firmware-handoff.md) | What UEFI configures before the kernel runs, and what Cosmos keeps |
| [PCI](driver-concepts/pci.md) | The PCI function, its configuration space, class codes, capabilities and BARs |
| [Hot-plug](driver-concepts/hot-plug.md) | Devices arriving and leaving at run time, and surprise removal |
| [Virtio](driver-concepts/virtio.md) | Paravirtual devices: transports, the status handshake, feature negotiation and virtqueues |
| [USB](driver-concepts/usb.md) | Interfaces, endpoints, transfer types, polled interrupt endpoints, enumeration, the HID boot protocol |
| [PS/2 and the 8042 controller](driver-concepts/ps2.md) | The 8042 controller, its two ports, commands, acknowledgements and scanning |
