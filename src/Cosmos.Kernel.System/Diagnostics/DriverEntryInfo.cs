// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Diagnostics;

/// <summary>
/// One entry of the driver manifest, produced by
/// <see cref="DriverInfo.TryGetDriver"/>. The index it was read at is its
/// manifest position, which is the last arbitration key: when two drivers
/// match a node with the same priority and the same specificity, the one
/// earlier in the manifest is offered first.
/// </summary>
public readonly struct DriverEntryInfo
{
    internal DriverEntryInfo(string name, int priority)
    {
        Name = name;
        Priority = priority;
    }

    /// <summary>The driver's name, as it appears in the <c>[Drivers]</c> log.</summary>
    public string Name { get; }

    /// <summary>
    /// Arbitration priority among drivers matching the same node: higher is
    /// offered first. Zero for a driver that does not override it.
    /// </summary>
    public int Priority { get; }
}
