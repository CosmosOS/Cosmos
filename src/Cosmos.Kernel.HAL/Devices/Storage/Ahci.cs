// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Interfaces;
using Cosmos.Kernel.HAL.Pci;
using Cosmos.Kernel.HAL.Pci.Enums;

namespace Cosmos.Kernel.HAL.Devices.Storage;

/// <summary>
/// AHCI driver entry point. Scans PCI for every
/// <see cref="ClassId.MassStorageController"/> /
/// <see cref="SubclassId.SataController"/> device and brings each one up
/// independently — a machine with two SATA HBAs gets two
/// <see cref="AhciController"/> instances and all of their attached SATA
/// drives show up in <see cref="Ports"/>.
/// </summary>
internal static class Ahci
{
    private static List<AhciController>? s_controllers;
    private static List<BlockDevice>? s_ports;
    private static bool s_initialized;

    /// <summary>Discovered AHCI controllers (empty if none were found).</summary>
    public static IReadOnlyList<AhciController> Controllers =>
        (IReadOnlyList<AhciController>?)s_controllers ?? Array.Empty<AhciController>();

    /// <summary>All SATA drive ports across every controller this driver bound to.</summary>
    public static IReadOnlyList<BlockDevice> Ports =>
        (IReadOnlyList<BlockDevice>?)s_ports ?? Array.Empty<BlockDevice>();

    /// <summary>
    /// Initialize the AHCI driver: PCI scan, controller bring-up, port
    /// discovery. Idempotent — subsequent calls are no-ops.
    /// </summary>
    public static void Initialize()
    {
        if (s_initialized)
        {
            return;
        }
        s_initialized = true;

        Serial.WriteString("[AHCI] Looking for AHCI controllers...\n");

        s_controllers = new List<AhciController>();
        s_ports = new List<BlockDevice>();

        List<PciDevice> devices = PciManager.GetAllDevicesClass(
            ClassId.MassStorageController, SubclassId.SataController);
        if (devices.Count == 0)
        {
            Serial.WriteString("[AHCI] No AHCI controllers found\n");
            return;
        }

        Serial.WriteString("[AHCI] Found ");
        Serial.WriteNumber((uint)devices.Count);
        Serial.WriteString(" SATA-class device(s)\n");

        // q35 (and many real chipsets) expose multiple SATA-class PCI
        // functions — typically one AHCI-mode controller plus an IDE-compat
        // legacy one. Only the AHCI-ProgIf devices belong to this driver;
        // the others are not addressable through MMIO BARs and would fail
        // any attempt to bring them up. Skip them here, not in the
        // controller bring-up code, so init never has to throw.
        for (int i = 0; i < devices.Count; i++)
        {
            PciDevice device = devices[i];
            if ((ProgramIf)device.ProgIf != ProgramIf.SataAhci)
            {
                Serial.WriteString("[AHCI] Skipping non-AHCI SATA controller #");
                Serial.WriteNumber((uint)i);
                Serial.WriteString(" (ProgIf=0x");
                Serial.WriteHex(device.ProgIf);
                Serial.WriteString(")\n");
                continue;
            }

            AhciController controller = new(device);
            if (!controller.Initialize())
            {
                Serial.WriteString("[AHCI] Controller #");
                Serial.WriteNumber((uint)i);
                Serial.WriteString(" init failed; skipping\n");
                continue;
            }

            s_controllers.Add(controller);
            for (int p = 0; p < controller.Ports.Count; p++)
            {
                s_ports.Add(controller.Ports[p]);
            }
        }
    }

    /// <summary>
    /// Busy-wait for the given number of "AHCI ticks". One tick is a small,
    /// hardware-dependent delay implemented via 4 reads to the legacy
    /// post/0x80 port. Stateless — safe to call before any controller is
    /// initialized.
    /// </summary>
    public static void Wait(int ticks)
    {
        // Platform-provided delay: x64 paces on port-0x80 reads, ARM64 on
        // the generic timer. The old direct port-0x80 loop was only a real
        // delay on x64 — ARM64MemoryIO turned it into reads of an arbitrary
        // physical address, so COMRESET hold times were accidental there.
        IPlatformInitializer? init = PlatformHAL.Initializer;
        if (init != null)
        {
            init.DelayMicroseconds((uint)ticks);
            return;
        }

        for (int i = 0; i < ticks; i++)
        {
            PlatformHAL.PortIO.ReadByte(PlatformHAL.LegacyPostPort);
            PlatformHAL.PortIO.ReadByte(PlatformHAL.LegacyPostPort);
            PlatformHAL.PortIO.ReadByte(PlatformHAL.LegacyPostPort);
            PlatformHAL.PortIO.ReadByte(PlatformHAL.LegacyPostPort);
        }
    }
}
