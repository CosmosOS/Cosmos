using System;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

internal enum SVGA3dRenderStateName : uint
{
    SVGA3D_RS_INVALID = 0,
    SVGA3D_RS_ZENABLE = 1,     /* SVGA3dBool */
    SVGA3D_RS_ZWRITEENABLE = 2,     /* SVGA3dBool */
    SVGA3D_RS_ALPHATESTENABLE = 3,     /* SVGA3dBool */
    SVGA3D_RS_DITHERENABLE = 4,     /* SVGA3dBool */
    SVGA3D_RS_BLENDENABLE = 5,     /* SVGA3dBool */
    SVGA3D_RS_FOGENABLE = 6,     /* SVGA3dBool */
    SVGA3D_RS_SPECULARENABLE = 7,     /* SVGA3dBool */
    SVGA3D_RS_STENCILENABLE = 8,     /* SVGA3dBool */
    SVGA3D_RS_LIGHTINGENABLE = 9,     /* SVGA3dBool */
    SVGA3D_RS_NORMALIZENORMALS = 10,    /* SVGA3dBool */
    SVGA3D_RS_POINTSPRITEENABLE = 11,    /* SVGA3dBool */
    SVGA3D_RS_POINTSCALEENABLE = 12,    /* SVGA3dBool */
    SVGA3D_RS_STENCILREF = 13,    /* uint32 */
    SVGA3D_RS_STENCILMASK = 14,    /* uint32 */
    SVGA3D_RS_STENCILWRITEMASK = 15,    /* uint32 */
    SVGA3D_RS_FOGSTART = 16,    /* float */
    SVGA3D_RS_FOGEND = 17,    /* float */
    SVGA3D_RS_FOGDENSITY = 18,    /* float */
    SVGA3D_RS_POINTSIZE = 19,    /* float */
    SVGA3D_RS_POINTSIZEMIN = 20,    /* float */
    SVGA3D_RS_POINTSIZEMAX = 21,    /* float */
    SVGA3D_RS_POINTSCALE_A = 22,    /* float */
    SVGA3D_RS_POINTSCALE_B = 23,    /* float */
    SVGA3D_RS_POINTSCALE_C = 24,    /* float */
    SVGA3D_RS_FOGCOLOR = 25,    /* SVGA3dColor */
    SVGA3D_RS_AMBIENT = 26,    /* SVGA3dColor */
    SVGA3D_RS_CLIPPLANEENABLE = 27,    /* SVGA3dClipPlanes */
    SVGA3D_RS_FOGMODE = 28,    /* SVGA3dFogMode */
    SVGA3D_RS_FILLMODE = 29,    /* SVGA3dFillMode */
    SVGA3D_RS_SHADEMODE = 30,    /* SVGA3dShadeMode */
    SVGA3D_RS_LINEPATTERN = 31,    /* SVGA3dLinePattern */
    SVGA3D_RS_SRCBLEND = 32,    /* SVGA3dBlendOp */
    SVGA3D_RS_DSTBLEND = 33,    /* SVGA3dBlendOp */
    SVGA3D_RS_BLENDEQUATION = 34,    /* SVGA3dBlendEquation */
    SVGA3D_RS_CULLMODE = 35,    /* SVGA3dFace */
    SVGA3D_RS_ZFUNC = 36,    /* SVGA3dCmpFunc */
    SVGA3D_RS_ALPHAFUNC = 37,    /* SVGA3dCmpFunc */
    SVGA3D_RS_STENCILFUNC = 38,    /* SVGA3dCmpFunc */
    SVGA3D_RS_STENCILFAIL = 39,    /* SVGA3dStencilOp */
    SVGA3D_RS_STENCILZFAIL = 40,    /* SVGA3dStencilOp */
    SVGA3D_RS_STENCILPASS = 41,    /* SVGA3dStencilOp */
    SVGA3D_RS_ALPHAREF = 42,    /* float (0.0 .. 1.0) */
    SVGA3D_RS_FRONTWINDING = 43,    /* SVGA3dFrontWinding */
    SVGA3D_RS_COORDINATETYPE = 44,    /* SVGA3dCoordinateType */
    SVGA3D_RS_ZBIAS = 45,    /* float */
    SVGA3D_RS_RANGEFOGENABLE = 46,    /* SVGA3dBool */
    SVGA3D_RS_COLORWRITEENABLE = 47,    /* SVGA3dColorMask */
    SVGA3D_RS_VERTEXMATERIALENABLE = 48,    /* SVGA3dBool */
    SVGA3D_RS_DIFFUSEMATERIALSOURCE = 49,    /* SVGA3dVertexMaterial */
    SVGA3D_RS_SPECULARMATERIALSOURCE = 50,    /* SVGA3dVertexMaterial */
    SVGA3D_RS_AMBIENTMATERIALSOURCE = 51,    /* SVGA3dVertexMaterial */
    SVGA3D_RS_EMISSIVEMATERIALSOURCE = 52,    /* SVGA3dVertexMaterial */
    SVGA3D_RS_TEXTUREFACTOR = 53,    /* SVGA3dColor */
    SVGA3D_RS_LOCALVIEWER = 54,    /* SVGA3dBool */
    SVGA3D_RS_SCISSORTESTENABLE = 55,    /* SVGA3dBool */
    SVGA3D_RS_BLENDCOLOR = 56,    /* SVGA3dColor */
    SVGA3D_RS_STENCILENABLE2SIDED = 57,    /* SVGA3dBool */
    SVGA3D_RS_CCWSTENCILFUNC = 58,    /* SVGA3dCmpFunc */
    SVGA3D_RS_CCWSTENCILFAIL = 59,    /* SVGA3dStencilOp */
    SVGA3D_RS_CCWSTENCILZFAIL = 60,    /* SVGA3dStencilOp */
    SVGA3D_RS_CCWSTENCILPASS = 61,    /* SVGA3dStencilOp */
    SVGA3D_RS_VERTEXBLEND = 62,    /* SVGA3dVertexBlendFlags */
    SVGA3D_RS_SLOPESCALEDEPTHBIAS = 63,    /* float */
    SVGA3D_RS_DEPTHBIAS = 64,    /* float */


    /*
     * Output Gamma Level
     *
     * Output gamma effects the gamma curve of colors that are output from the
     * rendering pipeline.  A value of 1.0 specifies a linear color space. If the
     * value is <= 0.0, gamma correction is ignored and linear color space is
     * used.
     */

    SVGA3D_RS_OUTPUTGAMMA = 65,    /* float */
    SVGA3D_RS_ZVISIBLE = 66,    /* SVGA3dBool */
    SVGA3D_RS_LASTPIXEL = 67,    /* SVGA3dBool */
    SVGA3D_RS_CLIPPING = 68,    /* SVGA3dBool */
    SVGA3D_RS_WRAP0 = 69,    /* SVGA3dWrapFlags */
    SVGA3D_RS_WRAP1 = 70,    /* SVGA3dWrapFlags */
    SVGA3D_RS_WRAP2 = 71,    /* SVGA3dWrapFlags */
    SVGA3D_RS_WRAP3 = 72,    /* SVGA3dWrapFlags */
    SVGA3D_RS_WRAP4 = 73,    /* SVGA3dWrapFlags */
    SVGA3D_RS_WRAP5 = 74,    /* SVGA3dWrapFlags */
    SVGA3D_RS_WRAP6 = 75,    /* SVGA3dWrapFlags */
    SVGA3D_RS_WRAP7 = 76,    /* SVGA3dWrapFlags */
    SVGA3D_RS_WRAP8 = 77,    /* SVGA3dWrapFlags */
    SVGA3D_RS_WRAP9 = 78,    /* SVGA3dWrapFlags */
    SVGA3D_RS_WRAP10 = 79,    /* SVGA3dWrapFlags */
    SVGA3D_RS_WRAP11 = 80,    /* SVGA3dWrapFlags */
    SVGA3D_RS_WRAP12 = 81,    /* SVGA3dWrapFlags */
    SVGA3D_RS_WRAP13 = 82,    /* SVGA3dWrapFlags */
    SVGA3D_RS_WRAP14 = 83,    /* SVGA3dWrapFlags */
    SVGA3D_RS_WRAP15 = 84,    /* SVGA3dWrapFlags */
    SVGA3D_RS_MULTISAMPLEANTIALIAS = 85,    /* SVGA3dBool */
    SVGA3D_RS_MULTISAMPLEMASK = 86,    /* uint32 */
    SVGA3D_RS_INDEXEDVERTEXBLENDENABLE = 87,    /* SVGA3dBool */
    SVGA3D_RS_TWEENFACTOR = 88,    /* float */
    SVGA3D_RS_ANTIALIASEDLINEENABLE = 89,    /* SVGA3dBool */
    SVGA3D_RS_COLORWRITEENABLE1 = 90,    /* SVGA3dColorMask */
    SVGA3D_RS_COLORWRITEENABLE2 = 91,    /* SVGA3dColorMask */
    SVGA3D_RS_COLORWRITEENABLE3 = 92,    /* SVGA3dColorMask */
    SVGA3D_RS_SEPARATEALPHABLENDENABLE = 93,    /* SVGA3dBool */
    SVGA3D_RS_SRCBLENDALPHA = 94,    /* SVGA3dBlendOp */
    SVGA3D_RS_DSTBLENDALPHA = 95,    /* SVGA3dBlendOp */
    SVGA3D_RS_BLENDEQUATIONALPHA = 96,    /* SVGA3dBlendEquation */
    SVGA3D_RS_TRANSPARENCYANTIALIAS = 97,    /* SVGA3dTransparencyAntialiasType */
    SVGA3D_RS_LINEAA = 98,    /* SVGA3dBool */
    SVGA3D_RS_LINEWIDTH = 99,    /* float */
    SVGA3D_RS_MAX

}
