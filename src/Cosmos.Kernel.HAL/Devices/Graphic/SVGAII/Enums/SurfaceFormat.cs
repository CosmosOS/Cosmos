using System;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

internal enum SVGA3dSurfaceFormat : uint
{
    SVGA3D_FORMAT_INVALID = 0,

    SVGA3D_X8R8G8B8 = 1,
    SVGA3D_A8R8G8B8 = 2,

    SVGA3D_Z_D32 = 7,
    SVGA3D_Z_D16 = 8,

    SVGA3D_LUMINANCE8 = 11,
    SVGA3D_LUMINANCE4_ALPHA4 = 12,
    SVGA3D_LUMINANCE16 = 13,
    SVGA3D_LUMINANCE8_ALPHA8 = 14,

    SVGA3D_DXT1 = 15,
    SVGA3D_DXT2 = 16,
    SVGA3D_DXT3 = 17,
    SVGA3D_DXT4 = 18,
    SVGA3D_DXT5 = 19,

    SVGA3D_BUMPU8V8 = 20,
    SVGA3D_BUMPL6V5U5 = 21,
    SVGA3D_BUMPX8L8V8U8 = 22,
    SVGA3D_BUMPL8V8U8 = 23,

    SVGA3D_ARGB_S10E5 = 24,   /* 16-bit floating-point ARGB */
    SVGA3D_ARGB_S23E8 = 25,   /* 32-bit floating-point ARGB */

    SVGA3D_A2R10G10B10 = 26,

    /* signed formats */
    SVGA3D_V8U8 = 27,
    SVGA3D_Q8W8V8U8 = 28,
    SVGA3D_CxV8U8 = 29,

    /* mixed formats */
    SVGA3D_X8L8V8U8 = 30,
    SVGA3D_A2W10V10U10 = 31,

    SVGA3D_ALPHA8 = 32,

    /* Single- and dual-component floating point formats */
    SVGA3D_R_S10E5 = 33,
    SVGA3D_R_S23E8 = 34,
    SVGA3D_RG_S10E5 = 35,
    SVGA3D_RG_S23E8 = 36,

    /*
     * Any surface can be used as a buffer object, but SVGA3D_BUFFER is
     * the most efficient format to use when creating new surfaces
     * expressly for index or vertex data.
     */

    SVGA3D_BUFFER = 37,

    SVGA3D_Z_D24X8 = 38,

    SVGA3D_V16U16 = 39,

    SVGA3D_G16R16 = 40,
    SVGA3D_A16B16G16R16 = 41,

    /* Packed Video formats */
    SVGA3D_UYVY = 42,
    SVGA3D_YUY2 = 43,

    /* Planar video formats */
    SVGA3D_NV12 = 44,

    /* Video format with alpha */
    SVGA3D_AYUV = 45,

    SVGA3D_BC4_UNORM = 108,
    SVGA3D_BC5_UNORM = 111,

    /* Advanced D3D9 depth formats. */
    SVGA3D_Z_DF16 = 118,
    SVGA3D_Z_DF24 = 119,
    SVGA3D_Z_D24S8_INT = 120,

    SVGA3D_FORMAT_MAX
}
