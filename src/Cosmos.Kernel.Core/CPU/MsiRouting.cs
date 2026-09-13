// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Core.CPU;

/// <summary>
/// Architecture-neutral PCI MSI / MSI-X binding. Both x64 (LAPIC Fixed-mode
/// messages) and ARM64 (GICv3 ITS GITS_TRANSLATER writes) ultimately program
/// the same shape into a device's MSI-X table — a 64-bit address plus a
/// 32-bit data dword — but the routing model differs:
///
/// <list type="bullet">
/// <item>x64: data = IDT vector chosen by the kernel; address encodes the
/// destination LAPIC ID. No per-device prep.</item>
/// <item>ARM64 ITS: data = EventID (per-device index); address = the
/// shared <c>GITS_TRANSLATER</c> doorbell. The ITS must be told the
/// (DeviceID, EventID) → LPI mapping out of band before the device fires.
/// </item>
/// </list>
///
/// This file abstracts both behind <see cref="IMsiBinder"/>: each platform
/// initializer registers a binder, then HAL-level PCI MSI-X code calls
/// <see cref="PrepareDevice"/> once per device and <see cref="BindEntry"/>
/// per MSI-X table entry. The binder owns vector / LPI allocation and any
/// device-specific bookkeeping (ITT allocation, MAPD, MAPTI on ARM64).
///
/// PCI device identity is passed in raw (bus, slot, function) form to keep
/// this file free of <c>HAL/Pci</c> dependencies — Core can't reference
/// HAL upstream.
/// </summary>
internal static class MsiRouting
{
    private static IMsiBinder? s_binder;

    /// <summary>
    /// True once a platform has registered a binder (x64 LAPIC, ARM64 ITS, …).
    /// </summary>
    public static bool IsAvailable => s_binder != null && s_binder.IsAvailable;

    /// <summary>
    /// Called once by the platform interrupt-controller initializer.
    /// </summary>
    public static void RegisterBinder(IMsiBinder binder)
    {
        s_binder = binder;
    }

    /// <summary>
    /// Per-device prep called once by <c>MsiX.Enable</c> after the cap is
    /// located. Returns an opaque context the platform can pass back into
    /// <see cref="BindEntry"/>; may be null on platforms that don't need
    /// per-device state (x64).
    /// </summary>
    public static object? PrepareDevice(uint bus, uint slot, uint function, int entryCount)
    {
        if (s_binder == null)
        {
            throw new System.PlatformNotSupportedException("MSI binder not registered");
        }
        return s_binder.PrepareDevice(bus, slot, function, entryCount);
    }

    /// <summary>
    /// Bind one MSI-X table entry: allocate a vector / LPI for
    /// <paramref name="handler"/>, wire any platform-specific routing,
    /// and return the (address, data) pair the device should program into
    /// the entry.
    /// </summary>
    public static void BindEntry(object? deviceCtx, int entryIndex, InterruptManager.IrqDelegate handler,
                                  uint targetCpu, out ulong address, out uint data)
    {
        if (s_binder == null)
        {
            throw new System.PlatformNotSupportedException("MSI binder not registered");
        }
        s_binder.BindEntry(deviceCtx, entryIndex, handler, targetCpu, out address, out data);
    }
}
