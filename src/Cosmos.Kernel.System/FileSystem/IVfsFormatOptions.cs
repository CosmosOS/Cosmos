// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.FileSystem;

/// <summary>
/// Marker for filesystem-specific format parameters. Each driver casts to
/// its own concrete type (e.g. <see cref="Fat.FatFormatOptions"/>); a null value means
/// "use driver defaults."
/// </summary>
public interface IVfsFormatOptions
{
}
