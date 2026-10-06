// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.System.Storage;

/// <summary>
/// The storage manager's consumer of the driver kit: every block device a
/// kit driver publishes is registered with <see cref="StorageManager"/>,
/// which scans it for partitions, and is unregistered when withdrawn. The
/// consumer logs nothing: the registered line is
/// <see cref="StorageManager.TryRegister"/>'s. Installed by
/// <see cref="StorageManager.Initialize"/>, before the driver stage runs.
/// <para>
/// Contexts: <see cref="OnPublished"/> runs inside the publishing driver's
/// probe on the kit worker, so the partition scan inside
/// <see cref="StorageManager.TryRegister"/> reads the device from a thread
/// context in which the driver has already made the device operational,
/// and the scan's I/O lengthens the driver stage by its duration.
/// <see cref="OnWithdrawn"/> runs in the binding's teardown, before the
/// driver's interrupts are disconnected and before its <c>OnDetach</c> and
/// the memory release, so an I/O in flight completes or fails against live
/// hardware; the ring does not wrap the driver's object, so a later call
/// through a <see cref="Partition"/> of a withdrawn kit disk gets the kit's
/// <see cref="InvalidOperationException"/> (a window or DMA buffer torn
/// down) or the driver's own detach exception (an I/O exception for the
/// shipped NVMe, USB mass storage and virtio-blk drivers); the ring
/// translates nothing. That path is reached when a USB stick or a PCI function behind a hot-plug slot
/// is pulled out, and through the synthetic bus. The two never run
/// concurrently, which is what makes the copy-on-write array safe without a
/// lock.
/// </para>
/// </summary>
internal sealed class KitBlockConsumer : BlockConsumer
{
    /// <summary>The devices this consumer accepted, keyed by reference; replaced on every change, never changed in place.</summary>
    private PublishedDevice[] _published = [];

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">The device has no binding, the manager's table is full, the device is already registered, or the manager is not initialized: the kit leaves the device unconsumed and the publishing probe fails with that reason, rather than binding a disk the ring never sees.</exception>
    public override void OnPublished(PublishedDevice device)
    {
        IBlockDevice block = (IBlockDevice)device.Device;
        DeviceBinding binding = device.Binding ?? throw new InvalidOperationException("a block device without a binding");

        BlockRegistration registration = StorageManager.TryRegister(block, binding.Node.Path, binding.Driver.Name);
        switch (registration)
        {
            case BlockRegistration.Full:
                throw new InvalidOperationException("the storage manager's device table is full");
            case BlockRegistration.Duplicate:
                throw new InvalidOperationException("the block device is already registered");
            case BlockRegistration.Unavailable:
                throw new InvalidOperationException("the storage manager is not initialized");
        }

        PublishedDevice[] current = _published;
        PublishedDevice[] published = new PublishedDevice[current.Length + 1];
        Array.Copy(current, published, current.Length);
        published[current.Length] = device;
        _published = published;
    }

    /// <inheritdoc/>
    public override void OnWithdrawn(PublishedDevice device)
    {
        PublishedDevice[] current = _published;
        int index = IndexOf(current, device);
        if (index < 0)
        {
            return;
        }

        PublishedDevice[] published = new PublishedDevice[current.Length - 1];
        Array.Copy(current, published, index);
        Array.Copy(current, index + 1, published, index, current.Length - index - 1);
        _published = published;
        StorageManager.UnregisterDevice((IBlockDevice)device.Device);
    }

    /// <summary>The position of <paramref name="device"/> in <paramref name="published"/> by reference, or -1. Allocation-free.</summary>
    private static int IndexOf(PublishedDevice[] published, PublishedDevice device)
    {
        for (int i = 0; i < published.Length; i++)
        {
            if (ReferenceEquals(published[i], device))
            {
                return i;
            }
        }

        return -1;
    }
}
