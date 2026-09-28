// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Platform;

/// <summary>
/// A match over platform identities: one compatible string (specificity 1)
/// or any platform device (specificity 0).
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class PlatformMatch : DeviceMatch
{
    private readonly string? _compatible;

    private PlatformMatch(string? compatible)
    {
        _compatible = compatible;
    }

    /// <inheritdoc/>
    public override int Specificity => _compatible is null ? 0 : 1;

    /// <summary>Matches every platform device whose <see cref="PlatformIdentity.Compatible"/> contains <paramref name="value"/>, compared ordinally.</summary>
    /// <param name="value">The compatible string.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> is empty.</exception>
    public static PlatformMatch Compatible(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        return new PlatformMatch(value);
    }

    /// <summary>Matches every platform device.</summary>
    public static PlatformMatch Any() => new(null);

    /// <inheritdoc/>
    public override bool Matches(DeviceIdentity identity) =>
        identity is PlatformIdentity platform && (_compatible is null || platform.IsCompatible(_compatible));
}
