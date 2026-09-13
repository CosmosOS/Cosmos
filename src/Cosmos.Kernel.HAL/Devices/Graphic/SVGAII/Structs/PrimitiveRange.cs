using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dPrimitiveRange
{
    public SVGA3dPrimitiveType primType;
    public uint primitiveCount;
    public SVGA3dArray indexArray;
    public uint indexWidth;
    public int indexBias;
}
