// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>Why a bound device is being taken away from its driver.</summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public enum DetachCause
{
    /// <summary>The bus retracted the node itself: hot-unplug, or a test retracting a synthetic node.</summary>
    Retracted,

    /// <summary>A parent node went away, taking every child with it.</summary>
    ParentRetracted,
}
