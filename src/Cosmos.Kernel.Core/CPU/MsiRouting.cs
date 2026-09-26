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
/// destination LAPIC ID. No device-side prep.</item>
/// <item>ARM64 ITS: data = EventID (per-device index); address = the
/// shared <c>GITS_TRANSLATER</c> doorbell. The ITS must be told the
/// (DeviceID, EventID) → LPI mapping out of band before the device fires.
/// </item>
/// </list>
///
/// This file abstracts both behind <see cref="IMsiBinder"/>: each platform
/// initializer registers a binder, then HAL-level PCI MSI-X code calls
/// <see cref="PrepareDevice"/> once per device and <see cref="BindEntry"/>
/// per MSI-X table entry, and on teardown <see cref="UnbindEntry"/> per
/// entry and <see cref="ReleaseDevice"/> once. The binder owns vector / LPI
/// allocation and any device-specific bookkeeping (ITT allocation, MAPD,
/// MAPTI, DISCARD on ARM64).
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
    public static bool IsAvailable => s_binder is not null && s_binder.IsAvailable;

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
    /// <see cref="BindEntry"/>, <see cref="UnbindEntry"/> and
    /// <see cref="ReleaseDevice"/>. Thread context only.
    /// </summary>
    public static object? PrepareDevice(uint bus, uint slot, uint function, int entryCount)
    {
        if (s_binder is null)
        {
            throw new System.PlatformNotSupportedException("MSI binder not registered");
        }
        return s_binder.PrepareDevice(bus, slot, function, entryCount);
    }

    /// <summary>
    /// Bind one MSI-X table entry: allocate a vector / LPI for
    /// <paramref name="handler"/>, wire any platform-specific routing,
    /// and return the (address, data) pair the device should program into
    /// the entry. Thread context only.
    /// </summary>
    public static void BindEntry(object? deviceCtx, int entryIndex, InterruptManager.IrqDelegate handler,
                                  uint targetCpu, out ulong address, out uint data)
    {
        if (s_binder is null)
        {
            throw new System.PlatformNotSupportedException("MSI binder not registered");
        }
        s_binder.BindEntry(deviceCtx, entryIndex, handler, targetCpu, out address, out data);
    }

    /// <summary>
    /// Return the vector / LPI bound to entry <paramref name="entryIndex"/>
    /// to its allocator. Mask the entry before calling: once this returns,
    /// a late message from the device finds no handler and is dropped.
    /// No-op for an unbound entry. Thread context only: the allocators
    /// take plain spinlocks.
    /// </summary>
    public static void UnbindEntry(object? deviceCtx, int entryIndex)
    {
        if (s_binder is null)
        {
            throw new System.PlatformNotSupportedException("MSI binder not registered");
        }
        s_binder.UnbindEntry(deviceCtx, entryIndex);
    }

    /// <summary>
    /// Release the per-device state <see cref="PrepareDevice"/> created,
    /// unbinding any entry still bound. <paramref name="deviceCtx"/> is dead
    /// afterwards. Thread context only: the allocators and the ARM64 ITS
    /// command queue take plain spinlocks.
    /// </summary>
    public static void ReleaseDevice(object? deviceCtx)
    {
        if (s_binder is null)
        {
            throw new System.PlatformNotSupportedException("MSI binder not registered");
        }
        s_binder.ReleaseDevice(deviceCtx);
    }
}
