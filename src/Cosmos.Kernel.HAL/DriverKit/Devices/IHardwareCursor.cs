// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>A display that composes a cursor itself. Optional facet on an <see cref="IDisplay"/>, found by a type test on the published object. Thread context.</summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public interface IHardwareCursor
{
    /// <summary>Uploads a cursor image: <paramref name="width"/> x <paramref name="height"/> premultiplied ARGB pixels, row-major.</summary>
    /// <param name="hotspotX">The hotspot's column within the image.</param>
    /// <param name="hotspotY">The hotspot's row within the image.</param>
    /// <param name="width">Width of the image in pixels.</param>
    /// <param name="height">Height of the image in pixels.</param>
    /// <param name="pixels">The pixels, <paramref name="width"/> times <paramref name="height"/> of them.</param>
    /// <returns>False when the display cannot take this image (unsupported size or format); the previous image stays.</returns>
    bool TryDefine(int hotspotX, int hotspotY, int width, int height, ReadOnlySpan<uint> pixels);

    /// <summary>Moves the cursor and shows or hides it.</summary>
    /// <param name="x">The hotspot's column on the display.</param>
    /// <param name="y">The hotspot's row on the display.</param>
    /// <param name="visible">True to show the cursor, false to hide it.</param>
    void Set(int x, int y, bool visible);
}
