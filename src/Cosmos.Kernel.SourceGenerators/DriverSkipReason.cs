// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.SourceGenerators;

/// <summary>
/// Why a class marked <c>[Driver]</c> cannot be constructed by the manifest.
/// Anything but <see cref="None"/> is reported as <c>COSMOSGEN001</c> and the
/// class is left out.
/// </summary>
internal enum DriverSkipReason
{
    /// <summary>The class can be registered.</summary>
    None,

    /// <summary>The class is abstract.</summary>
    Abstract,

    /// <summary>The class is static.</summary>
    Static,

    /// <summary>The class, or a type it is nested in, is generic.</summary>
    Generic,

    /// <summary>The class does not derive from <c>Cosmos.Kernel.HAL.DriverKit.Driver</c>.</summary>
    NotADriver,

    /// <summary>The class has no parameterless constructor the kernel assembly can call.</summary>
    NoConstructor,

    /// <summary>The class is not accessible from the kernel assembly.</summary>
    Inaccessible,
}
