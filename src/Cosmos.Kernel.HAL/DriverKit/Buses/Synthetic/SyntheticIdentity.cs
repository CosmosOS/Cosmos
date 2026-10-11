// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Buses.Synthetic;

/// <summary>The identity of a synthetic device: a key the test chose. Path is <c>synthetic:key</c>.</summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class SyntheticIdentity : DeviceIdentity
{
    /// <summary>The key the test published the device under.</summary>
    public string Key { get; }

    /// <inheritdoc/>
    public override string BusName => "synthetic";

    /// <inheritdoc/>
    public override string Address => Key;

    internal SyntheticIdentity(string key)
    {
        Key = key;
    }

    /// <inheritdoc/>
    public override string Describe()
    {
        return $"key {Key}";
    }
}
