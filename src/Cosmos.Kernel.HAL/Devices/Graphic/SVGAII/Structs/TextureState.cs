using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
internal struct SVGA3dTextureState
{
    [FieldOffset(0)]
    public uint stage;
    [FieldOffset(4)]
    public SVGA3dTextureStateName state;
    [FieldOffset(8)]
    public uint value;
    [FieldOffset(8)]
    public float floatValue;

    public SVGA3dTextureState(SVGA3dTextureStateName State, uint value, uint stage = 0u)
    {
        this.stage = stage;
        this.state = State;
        this.floatValue = 0;
        this.value = value;
    }

    public SVGA3dTextureState(SVGA3dTextureStateName State, float value, uint stage = 0u)
    {
        this.stage = stage;
        this.state = State;
        this.value = 0;
        this.floatValue = value;
    }
}
