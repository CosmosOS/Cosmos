// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dRenderTargetType: the slot a SET_RENDER_TARGET binds, colour 0 or depth.</summary>
internal enum SVGA3dRenderTargetType : uint
{
    Color = 2,
    Depth = 0,
}
