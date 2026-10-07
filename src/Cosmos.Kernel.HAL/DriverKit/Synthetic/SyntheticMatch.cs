// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Synthetic;

/// <summary>A match over synthetic identities: one key (specificity 1) or any synthetic device (specificity 0).</summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class SyntheticMatch : DeviceMatch
{
    private readonly string? _key;

    /// <inheritdoc/>
    public override int Specificity => _key is null ? 0 : 1;

    private SyntheticMatch(string? key)
    {
        _key = key;
    }

    /// <summary>Matches the synthetic device published under <paramref name="key"/>.</summary>
    /// <param name="key">The key.</param>
    public static SyntheticMatch Key(string key)
    {
        return new(key);
    }

    /// <summary>Matches every synthetic device.</summary>
    public static SyntheticMatch Any()
    {
        return new(null);
    }

    /// <inheritdoc/>
    public override bool Matches(DeviceIdentity identity)
    {
        return identity is SyntheticIdentity synthetic && (_key is null || string.Equals(_key, synthetic.Key));
    }
}
