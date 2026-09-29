// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dRenderState: one render state name and its value, a dword or a float, 8 bytes.</summary>
[StructLayout(LayoutKind.Explicit, Pack = 1)]
internal struct SVGA3dRenderState
{
    /// <summary>Which render state.</summary>
    [FieldOffset(0)]
    public SVGA3dRenderStateName state;
    /// <summary>The value as a dword.</summary>
    [FieldOffset(4)]
    public uint uintValue;
    /// <summary>The value as a float, sharing the dword.</summary>
    [FieldOffset(4)]
    public float floatValue;

    /// <summary>A render state with a dword value.</summary>
    public SVGA3dRenderState(SVGA3dRenderStateName State, uint value)
    {
        this.state = State;
        this.floatValue = 0;
        this.uintValue = value;
    }

    /// <summary>A render state with a float value.</summary>
    public SVGA3dRenderState(SVGA3dRenderStateName State, float value)
    {
        this.state = State;
        this.uintValue = 0;
        this.floatValue = value;
    }
}
