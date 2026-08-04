// This code is licensed under MIT license (see LICENSE for details)

using System;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.Devices.Graphic;

/// <summary>
/// Abstract base class for all graphic devices.
/// </summary>
public abstract class GraphicDevice : Device, IGraphicDevice
{
    /// <summary>
    /// Initialize the graphic device.
    /// </summary>
    public abstract void Initialize();
    public abstract void ClearScreen(uint color);
    public abstract void DrawPixel(uint color, int x, int y);
    public abstract uint GetPixel(int x, int y);
    public abstract void GetVRAM(int sourceByteOffset, int[] dest, int destIndex, int count);
    public abstract void CopyBuffer(ReadOnlyMemory<uint> pixels, int x, int y, int width, int height);
    public abstract void CopyBuffer(ReadOnlyMemory<int> pixels, int x, int y, int width, int height);
    public abstract void Swap();

    /// <summary>
    /// Presents only the given rect of the back buffer instead of the whole screen.
    /// Default falls back to a full <see cref="Swap"/> for devices that don't
    /// implement a partial present; devices backed by a full-framebuffer copy (e.g.
    /// GopDriver) should override this to avoid paying that cost on every small,
    /// frequent update (e.g. one character cell per keystroke).
    /// </summary>
    public virtual void SwapRect(int x, int y, int width, int height) => Swap();
}
