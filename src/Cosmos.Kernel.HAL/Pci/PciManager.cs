// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.IO;
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

    /// <summary>PCI subclass 0x04 - PCI-to-PCI bridge.</summary>
    private const int PciToPciBridgeSubclass = 0x4;

    public static PciDevice[]? Devices;

    public static uint Count = 0;

    public static void Setup()
    {
        Serial.WriteString("[PciManager] Setup .\n");
        Serial.WriteString("[PciManager] Setup Clearing List.\n");
        Devices = new PciDevice[MaxDevices];
        Serial.WriteString("[PciManager] Setup Cleared List.\n");
        if ((PciDevice.GetHeaderType(0x0, 0x0, 0x0) & MultifunctionBit) == 0)
        {
            CheckBus(0x0);
        }
        else
        {
            for (ushort fn = 0; fn < MaxFunctionsPerDevice; fn++)
            {
                Serial.WriteString("[PciManager] Setup ");
                Serial.WriteNumber(fn);
                Serial.WriteString("\n");
                if (PciDevice.GetVendorId(0x0, 0x0, fn) != InvalidVendorId)
                {
                    break;
                }

                CheckBus(fn);
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
        GetDevice((VendorId)pciDevice.VendorId, (DeviceId)pciDevice.DeviceId) != null;

    public static bool Exists(VendorId aVendorID, DeviceId aDeviceID) => GetDevice(aVendorID, aDeviceID) != null;

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

        List<PciDevice> matches = new();
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
