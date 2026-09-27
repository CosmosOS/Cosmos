// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.HAL.DriverKit.Pci;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// The PCI functions enumeration found, read from the HAL's internal
/// <c>PciManager</c>, as the ground truth the cells hold the driver kit's
/// public device list against, and for what that list cannot show: a
/// function's owner before the driver pass, when
/// <see cref="Cosmos.Kernel.System.Drivers.DriverManager.Devices"/> is still
/// empty, its Command register at that point, and its bus position. Thread
/// context.
/// </summary>
/// <remarks>
/// Reached through <see cref="UnsafeAccessorAttribute"/> and
/// <see cref="UnsafeAccessorTypeAttribute"/>, with no InternalsVisibleTo
/// grant (docs/articles/dev/accessing-internals.md): ILC binds each accessor
/// by name and signature, and one whose target moved compiles into a stub
/// that throws <see cref="MissingMethodException"/> or
/// <see cref="MissingFieldException"/>, failing the cell rather than passing
/// it. A function's IDs and configuration space are read through a
/// <see cref="PciFunction"/>, the seam's own view, built over the enumerated
/// object with the constructor the kit uses.
/// </remarks>
internal static class PciFunctions
{
    private const string PciManagerType = "Cosmos.Kernel.HAL.Pci.PciManager, Cosmos.Kernel.HAL";
    private const string PciDeviceType = "Cosmos.Kernel.HAL.Pci.PciDevice, Cosmos.Kernel.HAL";
    private const string PciDeviceArrayType = "Cosmos.Kernel.HAL.Pci.PciDevice[], Cosmos.Kernel.HAL";

    // An xHCI controller: serial bus controller, USB, programming interface 0x30.
    private const byte SerialBusClassCode = 0x0C;
    private const byte UsbSubclass = 0x03;
    private const byte XhciProgrammingInterface = 0x30;

    /// <summary>Every enumerated function, in the order PCI enumeration recorded them, with its owner as it is now.</summary>
    /// <returns>The functions; empty when PCI was not set up.</returns>
    public static PciFunctionState[] Enumerate()
    {
        // PciDevice[] converts to object[]: arrays of references are covariant.
        if (GetDevices(null) is not object[] devices)
        {
            return [];
        }

        uint count = GetCount(null);
        PciFunctionState[] functions = new PciFunctionState[count];
        for (int i = 0; i < functions.Length; i++)
        {
            object device = devices[i];
            functions[i] = new PciFunctionState(NewFunction(device), BusField(device), SlotField(device), FunctionField(device), GetOwner(device));
        }

        return functions;
    }

    /// <summary>Every enumerated function with the given vendor and device ID.</summary>
    /// <param name="vendorId">The vendor ID.</param>
    /// <param name="deviceId">The device ID.</param>
    /// <returns>How many there are.</returns>
    public static int Count(ushort vendorId, ushort deviceId)
    {
        int count = 0;
        foreach (PciFunctionState function in Enumerate())
        {
            if (function.Function.VendorId == vendorId && function.Function.DeviceId == deviceId)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>The first enumerated function with the given vendor and device ID, or null.</summary>
    /// <param name="vendorId">The vendor ID.</param>
    /// <param name="deviceId">The device ID.</param>
    /// <returns>The function as it is now.</returns>
    public static PciFunctionState? Find(ushort vendorId, ushort deviceId)
    {
        foreach (PciFunctionState function in Enumerate())
        {
            if (function.Function.VendorId == vendorId && function.Function.DeviceId == deviceId)
            {
                return function;
            }
        }

        return null;
    }

    /// <summary>The first enumerated xHCI controller, or null.</summary>
    /// <returns>The controller's function as it is now.</returns>
    public static PciFunctionState? FindXhci()
    {
        foreach (PciFunctionState function in Enumerate())
        {
            PciFunction id = function.Function;
            if (id.BaseClass == SerialBusClassCode && id.Subclass == UsbSubclass && id.ProgrammingInterface == XhciProgrammingInterface)
            {
                return function;
            }
        }

        return null;
    }

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_Devices")]
    [return: UnsafeAccessorType(PciDeviceArrayType)]
    private static extern object? GetDevices([UnsafeAccessorType(PciManagerType)] object? manager);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_Count")]
    private static extern uint GetCount([UnsafeAccessorType(PciManagerType)] object? manager);

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern PciFunction NewFunction([UnsafeAccessorType(PciDeviceType)] object device);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "Bus")]
    private static extern ref uint BusField([UnsafeAccessorType(PciDeviceType)] object device);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "Slot")]
    private static extern ref uint SlotField([UnsafeAccessorType(PciDeviceType)] object device);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "Function")]
    private static extern ref uint FunctionField([UnsafeAccessorType(PciDeviceType)] object device);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Owner")]
    private static extern string? GetOwner([UnsafeAccessorType(PciDeviceType)] object device);
}
