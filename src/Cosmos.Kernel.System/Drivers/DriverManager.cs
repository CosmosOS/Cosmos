// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.HAL.DriverKit.Engine;
using Cosmos.Kernel.HAL.DriverKit.Pci;
using Cosmos.Kernel.HAL.DriverKit.Usb;

namespace Cosmos.Kernel.System.Drivers;

/// <summary>
/// Where a kernel registers its own PCI and USB class drivers, and reads
/// which driver owns every device. The built-in drivers HAL brings up
/// itself (virtio) bind during HAL bring-up, before any kernel code runs,
/// and keep what they take. The drivers registered here, and the built-in
/// drivers written against the driver kit (xHCI, AHCI, NVMe, E1000E,
/// virtio-net and USB mass storage), are offered what those left, by one pass
/// that <see cref="Global.StartKernel"/> runs right after
/// <see cref="Kernel.RegisterDrivers"/> and before
/// <see cref="Kernel.OnBoot"/>. A USB driver is offered the interfaces of
/// the devices behind the host controllers the pass bound, once HAL's hub
/// and keyboard drivers had their pick, then of every device plugged in
/// later. A kit built-in wins a tie against a driver registered here, and a
/// registration whose match is strictly more specific takes the device
/// from it. Registration closes when that pass starts.
/// </summary>
/// <remarks>
/// Register from the kernel's <see cref="Kernel.RegisterDrivers"/> override,
/// which runs with interrupts on and only in a kernel built with PCI; the
/// kernel's constructor works too. Guard each registration with the
/// <see cref="KernelFeatures"/> switches its driver needs, one switch per
/// <c>if</c>, so a kernel built without them trims the driver.
/// </remarks>
[Experimental(Cosmos.Kernel.HAL.DriverKit.Experimentals.DriverKitDiagId)]
public static class DriverManager
{
    /// <summary>
    /// The list <see cref="Devices"/> last built, with the engine list it was
    /// built from. One object, so a thread that reads it never pairs one
    /// engine list with another's copy; null until the first read.
    /// </summary>
    private static DeviceListCache? s_devices;

    /// <summary>
    /// Every PCI function, in bus, device and function order, then every
    /// interface of every configured USB device, each with the driver that
    /// owns it, the built-in drivers included. Empty before the driver pass
    /// ran, and in a kernel built without PCI. The list changes after boot
    /// only when a USB device is plugged in or pulled out: each change
    /// replaces it whole, so a list read from here never changes under its
    /// reader, and reading again while nothing changed returns the same
    /// list. An owner a built-in records after the pass, as the VMware SVGA
    /// driver does when a full-screen canvas is first asked for, shows from
    /// the next change on. Thread context: the first read after a change
    /// builds the list.
    /// </summary>
    public static IReadOnlyList<DeviceInfo> Devices
    {
        get
        {
            // PCI's switch alone, so ILC folds it and a kernel without PCI
            // never reaches the engine from here.
            if (!CosmosFeatures.PCIEnabled)
            {
                return Array.Empty<DeviceInfo>();
            }

            IReadOnlyList<DeviceRecord> records = DriverCore.Devices;
            if (records.Count == 0)
            {
                return Array.Empty<DeviceInfo>();
            }

            // The engine publishes a new list on every change and never
            // edits one, so the same list means the same devices.
            DeviceListCache? cached = Volatile.Read(ref s_devices);
            if (cached is not null && ReferenceEquals(cached.Records, records))
            {
                return cached.Devices;
            }

            DeviceInfo[] devices = new DeviceInfo[records.Count];
            for (int i = 0; i < devices.Length; i++)
            {
                devices[i] = new DeviceInfo(records[i]);
            }

            // Read-only, since every caller shares the list until the next
            // change. Two threads racing here each publish a complete pair,
            // and either one is right.
            DeviceListCache built = new(records, new ReadOnlyCollection<DeviceInfo>(devices));
            Volatile.Write(ref s_devices, built);
            return built.Devices;
        }
    }

    /// <summary>
    /// Registers a PCI driver, to be offered the PCI functions no built-in
    /// driver took during HAL bring-up when the driver pass runs, most
    /// specific match first, until one driver's Probe returns Bound. Of two
    /// equally specific matches, a built-in driver the kit binds, such as
    /// AHCI's class match, is offered the function first; a device match
    /// (<see cref="PciMatch.Device"/>) comes ahead of it. Thread context,
    /// from the kernel's <see cref="Kernel.RegisterDrivers"/> override or its
    /// constructor.
    /// </summary>
    /// <param name="registration">The driver's name, factory and match table.</param>
    /// <returns>
    /// True when the driver will take part in the pass. False when the kernel
    /// is built without PCI, or when the name is taken: by another
    /// registration, PCI or USB, or by a built-in driver, the boot display's
    /// <c>gop</c> reservation included.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="registration"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Registration has closed: the driver pass already started. Or the
    /// call comes from a driver's factory, Probe or Remove.
    /// </exception>
    public static bool Register(PciDriverRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);

        // The switch alone, so ILC folds it and a kernel without PCI trims
        // the engine along with the driver.
        if (!CosmosFeatures.PCIEnabled)
        {
            return false;
        }

        return DriverCore.Register(registration);
    }

    /// <summary>
    /// Registers a USB class driver, to be offered the interfaces HAL's hub
    /// and keyboard drivers left, most specific match first, behind the
    /// built-in mass storage driver on a tie: those present at boot when the
    /// driver pass runs, and those of every device plugged in later. Thread context,
    /// from the kernel's <see cref="Kernel.RegisterDrivers"/> override or its
    /// constructor.
    /// </summary>
    /// <param name="registration">The driver's name, factory and match table.</param>
    /// <returns>
    /// True when the driver will be offered interfaces. False when the
    /// kernel is built without USB, which a kernel without PCI always is, or
    /// when the name is taken: by another registration, PCI or USB, or by a
    /// built-in driver.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="registration"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Registration has closed: the driver pass already started. Or the
    /// call comes from a driver's factory, Probe or Remove.
    /// </exception>
    public static bool Register(UsbDriverRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);

        // One switch per if, so ILC folds each: without PCI the pass never
        // runs and nothing would offer the driver an interface, and without
        // USB there is no interface to offer.
        if (!CosmosFeatures.PCIEnabled)
        {
            return false;
        }

        if (!CosmosFeatures.UsbEnabled)
        {
            return false;
        }

        return DriverCore.Register(registration);
    }

    /// <summary>
    /// A published <see cref="Devices"/> list and the engine list it copies,
    /// swapped as one reference.
    /// </summary>
    private sealed class DeviceListCache
    {
        /// <summary>The engine's list the copy was built from.</summary>
        internal IReadOnlyList<DeviceRecord> Records { get; }

        /// <summary>The copy <see cref="DriverManager.Devices"/> hands out.</summary>
        internal IReadOnlyList<DeviceInfo> Devices { get; }

        /// <summary>Pairs <paramref name="devices"/> with the engine list <paramref name="records"/> it copies.</summary>
        internal DeviceListCache(IReadOnlyList<DeviceRecord> records, IReadOnlyList<DeviceInfo> devices)
        {
            Records = records;
            Devices = devices;
        }
    }
}
