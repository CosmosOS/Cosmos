// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using Cosmos.Kernel.Core.Runtime;

namespace Cosmos.Kernel.Tests.Iso9660;

/// <summary>
/// The fixture images the build embedded in this kernel (see the project
/// file), each handed out as a fresh <see cref="MemoryBlockDevice"/> in the
/// block size a cell asks for.
/// </summary>
internal static class FixtureImages
{
    /// <summary>A CD sector.</summary>
    public const ulong CdBlockSize = 2048;

    /// <summary>A disk sector: the geometry of an image written to a USB stick.</summary>
    public const ulong DiskBlockSize = 512;

    private const string RockRidgeResource = "Cosmos.Kernel.Tests.Iso9660.rockridge.iso";
    private const string Level1Resource = "Cosmos.Kernel.Tests.Iso9660.level1.iso";

    /// <summary>The Rock Ridge image as a device.</summary>
    public static MemoryBlockDevice RockRidge(string name, ulong blockSize)
    {
        return new MemoryBlockDevice(name, blockSize, Load(RockRidgeResource));
    }

    /// <summary>The plain level 1 image as a device.</summary>
    public static MemoryBlockDevice Level1(string name, ulong blockSize)
    {
        return new MemoryBlockDevice(name, blockSize, Load(Level1Resource));
    }

    private static byte[] Load(string resourceName)
    {
        ReadOnlySpan<byte> image = ResourceManager.GetResourceAsSpan(resourceName);
        if (image.Length == 0)
        {
            throw new InvalidOperationException($"fixture image {resourceName} is not embedded");
        }

        return image.ToArray();
    }
}
