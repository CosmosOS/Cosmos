// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>
/// A device a driver made available to the ring: its kind, its name, the
/// contract object the driver handed over, and the binding behind it. The
/// kit creates it on publish and withdraws it when the binding is torn down;
/// consumers receive it in both calls and may keep the reference.
/// </summary>
internal sealed class PublishedDevice
{
    private volatile bool _consumed;
    private volatile bool _withdrawn;

    internal PublishedDevice(DeviceKind kind, string name, object device, DeviceBinding? binding, DeviceProvenance provenance)
    {
        Kind = kind;
        Name = name;
        Device = device;
        Binding = binding;
        Provenance = provenance;
    }

    /// <summary>What kind of device this is.</summary>
    public DeviceKind Kind { get; }

    /// <summary>The name the driver gave it.</summary>
    public string Name { get; }

    /// <summary>The contract object the driver handed over: an <see cref="IKeyboard"/>, an <see cref="IPointer"/>, and so on by <see cref="Kind"/>.</summary>
    public object Device { get; }

    /// <summary>The binding that published the device; null for firmware provenance.</summary>
    public DeviceBinding? Binding { get; }

    /// <summary>Where the device came from.</summary>
    public DeviceProvenance Provenance { get; }

    /// <summary>True once a consumer of its kind received it.</summary>
    public bool IsConsumed => _consumed;

    /// <summary>True once withdrawn; sinks discard reports from then on.</summary>
    public bool IsWithdrawn => _withdrawn;

    /// <summary>Records that a consumer received the device. Registry only.</summary>
    internal void MarkConsumed() => _consumed = true;

    /// <summary>Stops every sink report from here on. Registry only.</summary>
    internal void MarkWithdrawn() => _withdrawn = true;
}
