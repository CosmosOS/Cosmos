using System;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

internal enum SVGA3dTextureStateName : uint
{
    SVGA3D_TS_INVALID = 0,
    SVGA3D_TS_BIND_TEXTURE = 1,    /* SVGA3dSurfaceId */
    SVGA3D_TS_COLOROP = 2,    /* SVGA3dTextureCombiner */
    SVGA3D_TS_COLORARG1 = 3,    /* SVGA3dTextureArgData */
    SVGA3D_TS_COLORARG2 = 4,    /* SVGA3dTextureArgData */
    SVGA3D_TS_ALPHAOP = 5,    /* SVGA3dTextureCombiner */
    SVGA3D_TS_ALPHAARG1 = 6,    /* SVGA3dTextureArgData */
    SVGA3D_TS_ALPHAARG2 = 7,    /* SVGA3dTextureArgData */
    SVGA3D_TS_ADDRESSU = 8,    /* SVGA3dTextureAddress */
    SVGA3D_TS_ADDRESSV = 9,    /* SVGA3dTextureAddress */
    SVGA3D_TS_MIPFILTER = 10,   /* SVGA3dTextureFilter */
    SVGA3D_TS_MAGFILTER = 11,   /* SVGA3dTextureFilter */
    SVGA3D_TS_MINFILTER = 12,   /* SVGA3dTextureFilter */
    SVGA3D_TS_BORDERCOLOR = 13,   /* SVGA3dColor */
    SVGA3D_TS_TEXCOORDINDEX = 14,   /* uint32 */
    SVGA3D_TS_TEXTURETRANSFORMFLAGS = 15,   /* SVGA3dTexTransformFlags */
    SVGA3D_TS_TEXCOORDGEN = 16,   /* SVGA3dTextureCoordGen */
    SVGA3D_TS_BUMPENVMAT00 = 17,   /* float */
    SVGA3D_TS_BUMPENVMAT01 = 18,   /* float */
    SVGA3D_TS_BUMPENVMAT10 = 19,   /* float */
    SVGA3D_TS_BUMPENVMAT11 = 20,   /* float */
    SVGA3D_TS_TEXTURE_MIPMAP_LEVEL = 21,   /* uint32 */
    SVGA3D_TS_TEXTURE_LOD_BIAS = 22,   /* float */
    SVGA3D_TS_TEXTURE_ANISOTROPIC_LEVEL = 23,   /* uint32 */
    SVGA3D_TS_ADDRESSW = 24,   /* SVGA3dTextureAddress */


    /*
     * Sampler Gamma Level
     *
     * Sampler gamma effects the color of samples taken from the sampler.  A
     * value of 1.0 will produce linear samples.  If the value is <= 0.0 the
     * gamma value is ignored and a linear space is used.
     */

    SVGA3D_TS_GAMMA = 25,   /* float */
    SVGA3D_TS_BUMPENVLSCALE = 26,   /* float */
    SVGA3D_TS_BUMPENVLOFFSET = 27,   /* float */
    SVGA3D_TS_COLORARG0 = 28,   /* SVGA3dTextureArgData */
    SVGA3D_TS_ALPHAARG0 = 29,   /* SVGA3dTextureArgData */
    SVGA3D_TS_MAX
}
