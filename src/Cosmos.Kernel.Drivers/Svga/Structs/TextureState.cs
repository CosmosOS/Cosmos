// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dTextureState: one texture state of one stage and its value, a dword or a float, 12 bytes.</summary>
[StructLayout(LayoutKind.Explicit, Pack = 1)]
internal struct SVGA3dTextureState
{
    /// <summary>The texture stage.</summary>
    [FieldOffset(0)]
    public uint stage;
    /// <summary>Which texture state.</summary>
    [FieldOffset(4)]
    public SVGA3dTextureStateName state;
    /// <summary>The value as a dword.</summary>
    [FieldOffset(8)]
    public uint value;
    /// <summary>The value as a float, sharing the dword.</summary>
    [FieldOffset(8)]
    public float floatValue;

    /// <summary>A texture state of a stage with a dword value.</summary>
    public SVGA3dTextureState(SVGA3dTextureStateName State, uint value, uint stage = 0u)
    {
        this.stage = stage;
        this.state = State;
        this.floatValue = 0;
        this.value = value;
    }

    /// <summary>A texture state of a stage with a float value.</summary>
    public SVGA3dTextureState(SVGA3dTextureStateName State, float value, uint stage = 0u)
    {
        this.stage = stage;
        this.state = State;
        this.value = 0;
        this.floatValue = value;
    }
}
