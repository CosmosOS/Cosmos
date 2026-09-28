// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>
/// The published devices and, per kind, the one consumer the ring installed
/// for them. Drivers publish through their binding, which calls
/// <see cref="Publish"/> and <see cref="Withdraw"/> on the worker; sinks read
/// the consumer slot on every report. A consumer installed after a device
/// was published is not told about it: managers install theirs before the
/// driver stage runs.
/// </summary>
internal static class DeviceRegistry
{
    private const int KindCount = 5;

    private static readonly DeviceConsumer?[] s_consumers = new DeviceConsumer?[KindCount];
    private static PublishedDevice[] s_devices = [];
    private static SchedSpinLock s_lock;

    /// <summary>Every device currently published, a snapshot.</summary>
    public static IReadOnlyList<PublishedDevice> Devices => s_devices;

    /// <summary>
    /// Installs the consumer for a kind, or removes it with null. Thread
    /// context; the ring manager of that kind, one per kind.
    /// </summary>
    /// <param name="kind">The kind of device the consumer wants.</param>
    /// <param name="consumer">The consumer, or null to remove the current one.</param>
    public static void SetConsumer(DeviceKind kind, DeviceConsumer? consumer) =>
        Volatile.Write(ref s_consumers[(int)kind], consumer);

    /// <summary>The consumer installed for <paramref name="kind"/>, read at call time. Allocation-free; any context.</summary>
    /// <param name="kind">The kind.</param>
    internal static DeviceConsumer? ConsumerOf(DeviceKind kind) => Volatile.Read(ref s_consumers[(int)kind]);

    /// <summary>Publishes a device and tells the consumer of its kind. Worker only.</summary>
    internal static PublishedDevice Publish(DeviceKind kind, string name, object device, DeviceBinding? binding, DeviceProvenance provenance)
    {
        PublishedDevice published = new(kind, name, device, binding, provenance);
        using (s_lock.AcquireIrqSafe())
        {
            PublishedDevice[] devices = new PublishedDevice[s_devices.Length + 1];
            Array.Copy(s_devices, devices, s_devices.Length);
            devices[s_devices.Length] = published;
            s_devices = devices;
        }

        DeviceConsumer? consumer = ConsumerOf(kind);
        if (consumer is not null)
        {
            published.MarkConsumed();
            consumer.OnPublished(published);
        }

        return published;
    }

    /// <summary>Withdraws a device: sinks go quiet, the consumer that had it is told. Worker only.</summary>
    internal static void Withdraw(PublishedDevice device)
    {
        if (device.IsWithdrawn)
        {
            return;
        }

        device.MarkWithdrawn();
        using (s_lock.AcquireIrqSafe())
        {
            int index = IndexOfLocked(device);
            if (index >= 0)
            {
                PublishedDevice[] devices = new PublishedDevice[s_devices.Length - 1];
                Array.Copy(s_devices, devices, index);
                Array.Copy(s_devices, index + 1, devices, index, s_devices.Length - index - 1);
                s_devices = devices;
            }
        }

        if (device.IsConsumed)
        {
            ConsumerOf(device.Kind)?.OnWithdrawn(device);
        }
    }

    private static int IndexOfLocked(PublishedDevice device)
    {
        for (int i = 0; i < s_devices.Length; i++)
        {
            if (ReferenceEquals(s_devices[i], device))
            {
                return i;
            }
        }

        return -1;
    }
}
