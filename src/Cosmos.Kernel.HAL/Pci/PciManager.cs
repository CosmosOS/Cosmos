// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.HAL.Pci.Enums;

namespace Cosmos.Kernel.HAL.Pci;

internal class PciManager
{
    /// <summary>Maximum number of PCI devices tracked in the device cache.</summary>
    private const int MaxDevices = 64;

    /// <summary>Number of device slots per PCI bus (PCI spec: 5-bit device field).</summary>
    private const int MaxDevicesPerBus = 32;

    /// <summary>Number of functions per PCI device (PCI spec: 3-bit function field).</summary>
    private const int MaxFunctionsPerDevice = 8;

    /// <summary>Bit 7 of the PCI header type register: set when the device is multi-function.</summary>
    private const int MultifunctionBit = 0x80;

    /// <summary>Vendor ID returned for a non-existent PCI function (all bits set).</summary>
    private const ushort InvalidVendorId = 0xFFFF;

    /// <summary>PCI class code 0x06 - Bridge device.</summary>
    private const int BridgeClassCode = 0x6;

    /// <summary>PCI subclass 0x00 - host bridge.</summary>
    private const int HostBridgeSubclass = 0x0;

    /// <summary>PCI subclass 0x04 - PCI-to-PCI bridge.</summary>
    private const int PciToPciBridgeSubclass = 0x4;

    public static PciDevice[]? Devices { get; private set; }

    public static uint Count { get; private set; }

    /// <summary>
    /// The function <see cref="Setup"/> reserved as <see cref="PciOwner.Gop"/>
    /// because it scans out the Limine framebuffer, or null when no display
    /// function's BAR holds the framebuffer.
    /// </summary>
    /// <remarks>
    /// Kept apart from <see cref="PciDevice.Owner"/> because a built-in display
    /// driver replaces the gop reservation with its own name, after which the
    /// owner no longer says which function the console was scanned out of.
    /// </remarks>
    public static PciDevice? BootDisplay { get; private set; }

    public static void Setup()
    {
        Serial.WriteString("[PciManager] Setup .\n");
        Serial.WriteString("[PciManager] Setup Clearing List.\n");
        Devices = new PciDevice[MaxDevices];
        Serial.WriteString("[PciManager] Setup Cleared List.\n");
        CheckBus(0x0);

        // A multi-function 00:00 may hold one host bridge per root bus,
        // function N serving bus N. Other functions live there too (the
        // IOMMU at 00:00.2 on AMD) and the numbering has gaps, so every
        // function is looked at and only host bridges add a bus.
        if ((PciDevice.GetHeaderType(0x0, 0x0, 0x0) & MultifunctionBit) != 0)
        {
            for (ushort fn = 1; fn < MaxFunctionsPerDevice; fn++)
            {
                if (PciDevice.GetVendorId(0x0, 0x0, fn) == InvalidVendorId)
                {
                    continue;
                }

                PciDevice function = new(0x0, 0x0, fn);
                if (function.ClassCode == BridgeClassCode && function.Subclass == HostBridgeSubclass)
                {
                    CheckBus(fn);
                }
            }
        }

        for (int i = 0; i < Count; i++)
        {
            PciDevice device = Devices[i];
            Serial.WriteString("[PciManager] Found - ");
            Serial.WriteString(device.GetDeviceString());
            Serial.WriteString(" --- ");
            Serial.WriteString(device.GetTypeString());
            Serial.WriteString(" \n");
        }

        Serial.WriteString("[PciManager] Found Count ");
        Serial.WriteNumber(Count);
        Serial.WriteString("\n");

        ReserveBootDisplay();
    }

    /// <summary>
    /// Marks the display function that scans out the Limine framebuffer as
    /// owned by <see cref="PciOwner.Gop"/>, so a driver bound later never
    /// resizes or reprograms the BAR the console draws into.
    /// </summary>
    /// <remarks>
    /// Nothing here knows a BAR's size: sizing one means writing all ones to
    /// it, and enumeration must not disturb the device the console is drawing
    /// through. Two facts stand in for the size. BARs never overlap, so among
    /// every display function's memory BARs, the one with the highest base at
    /// or below the framebuffer is the only one that can contain it, even with
    /// an integrated and a discrete GPU both on the bus. And a BAR is aligned
    /// to its own power-of-two size, so the lowest set bit of its base bounds
    /// how far it can reach. That bound is what keeps a framebuffer in RAM
    /// (ramfb on arm64, a UEFI GOP backed by system memory) from being pinned
    /// on a display function whose BAR merely sits below it.
    /// </remarks>
    private static unsafe void ReserveBootDisplay()
    {
        ThrowIfNotSetup();

        LimineFramebufferResponse* response = Limine.Framebuffer.Response;
        if (response == null || response->FramebufferCount == 0)
        {
            Serial.WriteString("[PciManager] No boot framebuffer, no display function reserved\n");
            return;
        }

        // Framebuffer 0 is the one the console draws into (EarlyGop, GopCanvas).
        LimineFramebuffer* framebuffer = response->Framebuffers[0];
        ulong start = PageAllocator.VirtualToPhysical((ulong)framebuffer->Address);
        ulong end = start + framebuffer->Pitch * framebuffer->Height;

        PciDevice? display = null;
        ulong displayBarBase = 0;
        for (uint i = 0; i < Count; i++)
        {
            PciDevice device = Devices[i];
            if ((ClassId)device.ClassCode != ClassId.DisplayController)
            {
                continue;
            }

            ulong barBase = FindBarHolding(device, start, end);
            if (barBase > displayBarBase)
            {
                display = device;
                displayBarBase = barBase;
            }
        }

        if (display is null)
        {
            Serial.WriteString("[PciManager] No display function holds the boot framebuffer at 0x");
            Serial.WriteHex(start);
            Serial.WriteString(", none reserved\n");
            return;
        }

        if (display.TryClaim(PciOwner.Gop))
        {
            BootDisplay = display;
            Serial.WriteString("[PciManager] Reserved boot display ");
            Serial.WriteNumber(display.Bus);
            Serial.WriteString(":");
            Serial.WriteNumber(display.Slot);
            Serial.WriteString(".");
            Serial.WriteNumber(display.Function);
            Serial.WriteString(" as gop, framebuffer at 0x");
            Serial.WriteHex(start);
            Serial.WriteString("\n");
        }
    }

    /// <summary>
    /// Returns the base of <paramref name="device"/>'s memory BAR that can hold
    /// the physical range [<paramref name="start"/>, <paramref name="end"/>):
    /// the highest base at or below <paramref name="start"/> whose alignment
    /// leaves room up to <paramref name="end"/>. Returns 0 when no BAR can.
    /// </summary>
    private static ulong FindBarHolding(PciDevice device, ulong start, ulong end)
    {
        PciBaseAddressBar[]? bars = device.BaseAddressBar;
        if (bars is null)
        {
            return 0;
        }

        // The CPU writes the framebuffer through this function, which it can
        // only do with memory decoding on. With decoding off, whatever the
        // BARs hold is not an address the function answers at.
        if ((device.Command & PciCommand.Memory) == 0)
        {
            return 0;
        }

        ulong best = 0;
        int slot = 0;
        while (slot < bars.Length)
        {
            PciBaseAddressBar bar = bars[slot];
            if (!bar.IsIo)
            {
                // Read live: the snapshot keeps only the low half of a 64-bit
                // BAR, and prefetchable framebuffer BARs are often 64-bit.
                ulong barBase = device.GetBar64Address(slot);

                // The lowest set bit of the base is the largest size a BAR
                // aligned there can have.
                ulong largestSize = barBase & (~barBase + 1);
                if (barBase != 0 && barBase <= start && end - barBase <= largestSize && barBase > best)
                {
                    best = barBase;
                }
            }

            // A 64-bit BAR's upper half fills the next slot, which is not a
            // BAR of its own.
            slot += bar.Is64Bit ? 2 : 1;
        }

        return best;
    }

    /// <summary>
    /// Check bus.
    /// </summary>
    /// <param name="xBus">A bus to check.</param>
    private static void CheckBus(ushort xBus)
    {
        Serial.WriteString("[PciManager] CheckBus(");
        Serial.WriteNumber(xBus);
        Serial.WriteString(")\n");
        for (ushort device = 0; device < MaxDevicesPerBus; device++)
        {
            Serial.WriteString("[PciManager] CheckBus - ");
            Serial.WriteNumber(device);
            ushort vendorId = PciDevice.GetVendorId(xBus, device, 0x0);
            Serial.WriteString(" VID: 0x");
            Serial.WriteHex(vendorId);
            Serial.WriteString("\n");
            if (vendorId == InvalidVendorId)
            {
                continue;
            }

            CheckFunction(new PciDevice(xBus, device, 0x0));
            if ((PciDevice.GetHeaderType(xBus, device, 0x0) & MultifunctionBit) != 0)
            {
                for (ushort fn = 1; fn < MaxFunctionsPerDevice; fn++)
                {
                    if (PciDevice.GetVendorId(xBus, device, fn) != InvalidVendorId)
                    {
                        CheckFunction(new PciDevice(xBus, device, fn));
                    }
                }
            }
        }
    }

    private static void CheckFunction(PciDevice xPciDevice)
    {
        Serial.WriteString("[PciManager] CheckFunction - ");
        Serial.WriteString(xPciDevice.GetDeviceString());
        Serial.WriteString(" --- ");
        Serial.WriteString(xPciDevice.GetTypeString());
        Serial.WriteString(" \n");
        Add(xPciDevice);
        Serial.WriteString("[PciManager] Cached\n");
        if (xPciDevice.ClassCode == BridgeClassCode && xPciDevice.Subclass == PciToPciBridgeSubclass)
        {
            CheckBus(xPciDevice.SecondaryBusNumber);
        }
    }

    [MemberNotNull(nameof(Devices))]
    private static void ThrowIfNotSetup()
    {
        if (Devices is null)
        {
            throw new Exception($"{nameof(PciManager)} requires call to Setup() before using it");
        }
    }

    private static void Add(PciDevice xPciDevice)
    {
        ThrowIfNotSetup();

        if (Count >= Devices.Length)
        {
            Serial.WriteString("[PciManager] Device array full, cannot add more devices\n");
            return;
        }
        Devices[Count] = xPciDevice;
        Count++;
    }

    public static bool Exists(PciDevice pciDevice) =>
        GetDevice((VendorId)pciDevice.VendorId, (DeviceId)pciDevice.DeviceId) is not null;

    public static bool Exists(VendorId aVendorID, DeviceId aDeviceID) => GetDevice(aVendorID, aDeviceID) is not null;

    /// <summary>
    /// Get device.
    /// </summary>
    /// <param name="aVendorID">A vendor ID.</param>
    /// <param name="aDeviceID">A device ID.</param>
    /// <returns></returns>
    public static PciDevice? GetDevice(VendorId aVendorID, DeviceId aDeviceID)
    {
        ThrowIfNotSetup();

        for (uint i = 0; i < Count; i++)
        {
            PciDevice xDevice = Devices[i];
            if ((VendorId)xDevice.VendorId == aVendorID &&
                (DeviceId)xDevice.DeviceId == aDeviceID)
            {
                return xDevice;
            }
        }

        return null;
    }

    /// <summary>
    /// Get device.
    /// </summary>
    /// <param name="bus">Bus ID.</param>
    /// <param name="slot">Slot position ID.</param>
    /// <param name="function">Function ID.</param>
    /// <returns></returns>
    public static PciDevice? GetDevice(uint bus, uint slot, uint function)
    {
        ThrowIfNotSetup();

        for (uint i = 0; i < Count; i++)
        {
            PciDevice xDevice = Devices[i];
            if (xDevice.Bus == bus &&
                xDevice.Slot == slot &&
                xDevice.Function == function)
            {
                return xDevice;
            }
        }

        return null;
    }

    public static PciDevice? GetDeviceClass(ClassId classId, SubclassId subClass)
    {
        ThrowIfNotSetup();

        for (uint i = 0; i < Count; i++)
        {
            PciDevice xDevice = Devices[i];
            if ((ClassId)xDevice.ClassCode == classId &&
                (SubclassId)xDevice.Subclass == subClass)
            {
                return xDevice;
            }
        }

        return null;
    }

    public static PciDevice? GetDeviceClass(ClassId aClass, SubclassId aSubClass, ProgramIf aProgIF)
    {
        ThrowIfNotSetup();

        for (uint i = 0; i < Count; i++)
        {
            PciDevice xDevice = Devices[i];
            if ((ClassId)xDevice.ClassCode == aClass &&
                (SubclassId)xDevice.Subclass == aSubClass &&
                (ProgramIf)xDevice.ProgIf == aProgIF)
            {
                return xDevice;
            }
        }

        return null;
    }

    /// <summary>
    /// Return every PCI device whose class + subclass match. Drivers that
    /// can bind to multiple controllers of the same kind (e.g. multiple
    /// NVMe SSDs) iterate this instead of <see cref="GetDeviceClass(ClassId, SubclassId)"/>.
    /// </summary>
    public static List<PciDevice> GetAllDevicesClass(ClassId aClass, SubclassId aSubClass)
    {
        ThrowIfNotSetup();

        List<PciDevice> matches = [];
        for (uint i = 0; i < Count; i++)
        {
            PciDevice xDevice = Devices[i];
            if ((ClassId)xDevice.ClassCode == aClass &&
                (SubclassId)xDevice.Subclass == aSubClass)
            {
                matches.Add(xDevice);
            }
        }
        return matches;
    }
}
