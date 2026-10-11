// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// One entry of a driver's match table: a predicate over a
/// <see cref="DeviceIdentity"/> plus how specific it is. When several
/// drivers match a node, the kit offers the highest priority first, then the
/// most specific match, then the earliest manifest position.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public abstract class DeviceMatch
{
    /// <summary>Number of identity fields the match constrains; more is more specific.</summary>
    public abstract int Specificity { get; }

    /// <summary>Whether <paramref name="identity"/> is a device this entry describes.</summary>
    /// <param name="identity">The identity a bus reported.</param>
    public abstract bool Matches(DeviceIdentity identity);
}
