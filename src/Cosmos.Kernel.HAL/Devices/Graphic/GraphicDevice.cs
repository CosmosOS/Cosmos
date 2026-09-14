// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.Devices.Graphic;

/// <summary>
/// Abstract base class for all graphic devices.
/// </summary>
internal abstract class GraphicDevice : Device, IGraphicDevice
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
}
