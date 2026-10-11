// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>The two child nodes <see cref="BusDriver"/> published for one bound device.</summary>
public sealed class BusState
{
    internal BusState(DeviceNode child, DeviceNode orphan)
    {
        Child = child;
        Orphan = orphan;
    }

    /// <summary>The child <see cref="ChildDriver"/> binds.</summary>
    public DeviceNode Child { get; }

    /// <summary>The child no driver matches.</summary>
    public DeviceNode Orphan { get; }
}
