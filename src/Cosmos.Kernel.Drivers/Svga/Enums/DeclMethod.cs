// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dDeclMethod: the D3D9 declaration method of a vertex stream; default for every stream here.</summary>
internal enum SVGA3dDeclMethod
{
    SVGA3D_DECLMETHOD_DEFAULT = 0,
    SVGA3D_DECLMETHOD_PARTIALU,
    SVGA3D_DECLMETHOD_PARTIALV,
    SVGA3D_DECLMETHOD_CROSSUV,
    SVGA3D_DECLMETHOD_UV,
    SVGA3D_DECLMETHOD_LOOKUP,
    SVGA3D_DECLMETHOD_LOOKUPPRESAMPLED,
}
