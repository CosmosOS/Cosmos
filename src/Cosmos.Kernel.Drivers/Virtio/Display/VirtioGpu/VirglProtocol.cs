// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Virtio.Display.VirtioGpu;

/// <summary>
/// VirGL wire protocol constants for the 3D command stream submitted through
/// VIRTIO_GPU_CMD_SUBMIT_3D (virglrenderer's virgl_protocol.h / virgl_hw.h).
/// The stream is an array of 32-bit dwords; every command starts with a
/// header dword holding the command id, the object type and the payload
/// length in dwords.
/// </summary>
internal static class Virgl
{
    // Command header: command id (bits 0-7), object type (bits 8-15) and
    // payload length in dwords (bits 16-31). VIRGL_CMD0(cmd, obj, len).
    public static uint Cmd(uint cmd, uint objectType, uint length) => cmd | (objectType << 8) | (length << 16);

    // --- Context commands (virgl_protocol.h enum virgl_context_cmd) ---

    public const uint CmdCreateObject = 1;
    public const uint CmdBindObject = 2;
    public const uint CmdDestroyObject = 3;
    public const uint CmdSetViewportState = 4;
    public const uint CmdSetFramebufferState = 5;
    public const uint CmdSetVertexBuffers = 6;
    public const uint CmdClear = 7;
    public const uint CmdDrawVbo = 8;
    public const uint CmdSetSamplerViews = 10;
    public const uint CmdSetIndexBuffer = 11;
    public const uint CmdSetUniformBuffer = 27;
    public const uint CmdSetConstantBuffer = 12;
    public const uint CmdBindSamplerStates = 18;
    public const uint CmdBindShader = 31;
    public const uint CmdTransfer3D = 43;

    // --- Object types (virgl_protocol.h enum virgl_object_type) ---

    public const uint ObjectBlend = 1;
    public const uint ObjectRasterizer = 2;
    public const uint ObjectDsa = 3;
    public const uint ObjectShader = 4;
    public const uint ObjectVertexElements = 5;
    public const uint ObjectSamplerView = 6;
    public const uint ObjectSamplerState = 7;
    public const uint ObjectSurface = 8;

    // --- Shader object payload (VIRGL_OBJ_SHADER_*): the shader body is a
    // NUL-terminated TGSI assembly string, parsed by the host through
    // tgsi_text_translate. ---

    public const uint ObjShaderType = 2;
    public const uint ObjShaderOffset = 3;
    public const uint ObjShaderNumTokens = 4;
    public const uint ObjShaderSoNumOutputs = 5;
    public const uint ObjShaderTextStart = 6;

    // Shader stages (PIPE_SHADER_VERTEX / PIPE_SHADER_FRAGMENT).
    public const uint ShaderVertex = 0;
    public const uint ShaderFragment = 1;

    // --- PIPE_RESOURCE values ---

    // pipe_texture_target
    public const uint PipeBuffer = 0;
    public const uint PipeTexture2D = 2;

    // pipe_bind_flag
    public const uint BindDepthStencil = 1u << 0;
    public const uint BindRenderTarget = 1u << 1;
    public const uint BindSamplerView = 1u << 3;
    public const uint BindVertexBuffer = 1u << 4;
    public const uint BindIndexBuffer = 1u << 5;
    public const uint BindConstantBuffer = 1u << 6;

    // pipe_resource_usage
    public const uint UsageDefault = 0;
    public const uint UsageImmutable = 1;
    public const uint UsageDynamic = 2;

    // pipe_compare_func
    public const uint FuncLess = 1;

    // pipe_cull_mode: none is 0, so a cull-free rasterizer just leaves the
    // field cleared.
    public const uint CullNone = 0;

    // pipe_prim_type
    public const uint PrimTriangles = 4;
    public const uint PrimLines = 1;

    // pipe_tex_wrap / pipe_tex_filter / pipe_tex_mipfilter
    public const uint TexWrapClampToEdge = 2;
    public const uint TexMipFilterNone = 2;
    public const uint TexFilterNearest = 0;
    public const uint TexFilterLinear = 1;

    // pipe_transfer usage flags
    public const uint TransferWrite = 1u << 1;

    // VIRGL_TRANSFER3D direction values (virgl_protocol.h).
    public const uint TransferToHost = 1;
    public const uint TransferFromHost = 2;

    // --- Formats (virgl_hw.h enum virgl_formats): the numbering the host
    // expects everywhere a format dword crosses the wire. ---

    public const uint FormatNone = 0;
    public const uint FormatB8G8R8A8Unorm = 1;
    public const uint FormatZ32Float = 18;
    public const uint FormatR32Float = 28;
    public const uint FormatR32G32Float = 29;
    public const uint FormatR32G32B32Float = 30;
    public const uint FormatR32G32B32A32Float = 31;
    public const uint FormatR8Unorm = 64;
    public const uint FormatR8G8B8A8Unorm = 67;
    public const uint FormatR16Uint = 185;

    // --- Clear flags (virgl_protocol.h / pipe_clear_bits) ---

    public const uint ClearDepth = 1u << 0;
    public const uint ClearColor0 = 1u << 2;

    // --- Command payload layouts (dword offsets after the header dword) ---
    // Only the commands the engine emits are listed; every layout was taken
    // from virglrenderer's virgl_protocol.h.

    // CREATE_OBJECT payload: [1] handle, [2..] object specific (see the
    // per-object groups below).
    public const uint ObjCreateHandle = 1;

    // BLEND: [1] handle, [2] S0, [3] S1, [4+i] per-RT S2 dwords.
    public const uint ObjBlendS0 = 2;
    public const uint ObjBlendS1 = 3;
    public const uint ObjBlendS2 = 4;

    // DSA: [1] handle, [2] S0, [3] S1, [4] S2, [5] alpha reference.
    public const uint ObjDsaS0 = 2;
    public const uint ObjDsaS1 = 3;
    public const uint ObjDsaS2 = 4;
    public const uint ObjDsaAlphaRef = 5;

    // RASTERIZER: [1] handle, [2] S0, [3] point size, [4] sprite coord enable,
    // [5] S3, [6] line width, [7..9] polygon offsets.
    public const uint ObjRsS0 = 2;
    public const uint ObjRsPointSize = 3;
    public const uint ObjRsS3 = 5;
    public const uint ObjRsLineWidth = 6;

    // SURFACE: [1] handle, [2] resource, [3] format, [4] first layer/level,
    // [5] last layer/level.
    public const uint ObjSurfaceResource = 2;
    public const uint ObjSurfaceFormat = 3;
    public const uint ObjSurfaceFirstLayer = 4;
    public const uint ObjSurfaceLastLayer = 5;

    // SAMPLER_STATE: [1] handle, [2] S0, [3] lod bias, [4] min lod,
    // [5] max lod, [6..9] border color.
    public const uint ObjSamplerStateS0 = 2;
    public const uint ObjSamplerStateLodBias = 3;
    public const uint ObjSamplerStateMinLod = 4;
    public const uint ObjSamplerStateMaxLod = 5;
    public const uint ObjSamplerStateBorderColor = 6;

    // SAMPLER_VIEW: [1] handle, [2] resource, [3] format, [4] first/level,
    // [5] last/layer, [6] swizzle.
    public const uint ObjSamplerViewResource = 2;
    public const uint ObjSamplerViewFormat = 3;
    public const uint ObjSamplerViewSwizzle = 6;

    // VERTEX_ELEMENTS: [1] handle, then 4 dwords per element:
    // src offset, instance divisor, vertex buffer index, source format.
    public const uint ObjVertexElementsHandle = 1;
    public static uint VeSrcOffset(int element) => (uint)(element * 4 + 2);
    public static uint VeInstanceDivisor(int element) => (uint)(element * 4 + 3);
    public static uint VeVertexBufferIndex(int element) => (uint)(element * 4 + 4);
    public static uint VeSrcFormat(int element) => (uint)(element * 4 + 5);

    // SET_VIEWPORT_STATE: [1] count, then 6 dwords (scale xyz, translate xyz)
    // per viewport.
    public static uint ViewportScale(int index) => (uint)(2 + index * 6);

    // SET_FRAMEBUFFER_STATE: [1] color buffer count, [2] depth surface
    // handle, [3+i] color surface handles.
    public const uint FramebufferNrCbufs = 1;
    public const uint FramebufferZsurfHandle = 2;
    public static uint FramebufferCbufHandle(int index) => (uint)(3 + index);

    // SET_VERTEX_BUFFERS: 3 dwords per buffer: stride, offset, handle.
    public static uint VbStride(int index) => (uint)(index * 3 + 1);
    public static uint VbOffset(int index) => (uint)(index * 3 + 2);
    public static uint VbHandle(int index) => (uint)(index * 3 + 3);

    // SET_INDEX_BUFFER: [1] handle, [2] index size in bytes, [3] offset.
    public const uint IndexBufferHandle = 1;
    public const uint IndexBufferSize = 2;
    public const uint IndexBufferOffset = 3;

    // CLEAR: [1] buffers, [2..5] color as 4 floats, [6..7] depth as a double,
    // [8] stencil.
    public const uint ClearBuffers = 1;
    public const uint ClearColorOffset0 = 2;
    public const uint ClearDepthLow = 6;
    public const uint ClearDepthHigh = 7;
    public const uint ClearStencil = 8;

    // DRAW_VBO: 12 dwords.
    public const uint DrawStart = 1;
    public const uint DrawCount = 2;
    public const uint DrawMode = 3;
    public const uint DrawIndexed = 4;
    public const uint DrawInstanceCount = 5;
    public const uint DrawIndexBias = 6;
    public const uint DrawStartInstance = 7;
    public const uint DrawPrimitiveRestart = 8;
    public const uint DrawRestartIndex = 9;
    public const uint DrawMinIndex = 10;
    public const uint DrawMaxIndex = 11;
    public const uint DrawCountFromSo = 12;

    // SET_SAMPLER_VIEWS / BIND_SAMPLER_STATES: [1] shader stage,
    // [2] start slot, [3+i] handles.
    public const uint SamplerViewsShaderType = 1;
    public const uint SamplerViewsStartSlot = 2;
    public const uint SamplerViewsHandle0 = 3;

    public const uint BindSamplerStatesShaderType = 1;
    public const uint BindSamplerStatesStartSlot = 2;
    public const uint BindSamplerStatesHandle0 = 3;

    // SET_UNIFORM_BUFFER: [1] shader stage, [2] index, [3] offset,
    // [4] length, [5] resource handle.
    public const uint UniformBufferShaderType = 1;
    public const uint UniformBufferIndex = 2;
    public const uint UniformBufferOffset = 3;
    public const uint UniformBufferLength = 4;
    public const uint UniformBufferResHandle = 5;

    // BIND_SHADER: [1] shader object handle, [2] shader stage.
    public const uint BindShaderHandle = 1;
    public const uint BindShaderType = 2;

    // TRANSFER3D: [1..11] resource, level, usage, stride, layer stride and
    // the 6 box dwords, [12] byte offset into the resource, [13] direction.
    public const uint Transfer3DHandle = 1;
    public const uint Transfer3DLevel = 2;
    public const uint Transfer3DUsage = 3;
    public const uint Transfer3DStride = 4;
    public const uint Transfer3DLayerStride = 5;
    public const uint Transfer3DX = 6;
    public const uint Transfer3DY = 7;
    public const uint Transfer3DZ = 8;
    public const uint Transfer3DW = 9;
    public const uint Transfer3DH = 10;
    public const uint Transfer3DD = 11;
    public const uint Transfer3DOffset = 12;
    public const uint Transfer3DDirection = 13;
    public const uint Transfer3DSize = 13;

    // --- Object state bit fields ---

    // RASTERIZER S0 (VIRGL_OBJ_RS_S0_*).
    public const uint RsS0DepthClip = 1u << 1;
    public const uint RsS0ClipHalfZ = 1u << 2;
    public static uint RsS0FillFront(int mode) => (uint)((mode & 0x3) << 10);
    public static uint RsS0FillBack(int mode) => (uint)((mode & 0x3) << 12);

    // DSA S0 (VIRGL_OBJ_DSA_S0_*).
    public const uint DsaS0DepthEnable = 1u << 0;
    public const uint DsaS0DepthWritemask = 1u << 1;
    public static uint DsaS0DepthFunc(uint func) => (func & 0x7) << 2;

    // SAMPLER_STATE S0 (VIRGL_OBJ_SAMPLE_STATE_S0_*).
    public static uint SamplerS0(uint wrapS, uint wrapT, uint wrapR, uint minFilter, uint mipFilter, uint magFilter) =>
        (wrapS & 0x7) | ((wrapT & 0x7) << 3) | ((wrapR & 0x7) << 6) |
        ((minFilter & 0x1) << 9) | ((mipFilter & 0x3) << 11) | ((magFilter & 0x1) << 13);

    // SAMPLER_VIEW swizzle (VIRGL_OBJ_SAMPLER_VIEW_SWIZZLE_*): one of
    // PIPE_SWIZZLE_X/Y/Z/W (0-3) per channel, packed 3 bits apart.
    public const uint SwizzleX = 0;
    public const uint SwizzleY = 1;
    public const uint SwizzleZ = 2;
    public const uint SwizzleW = 3;

    public static uint SamplerViewSwizzle(uint r, uint g, uint b, uint a) =>
        (r & 0x7) | ((g & 0x7) << 3) | ((b & 0x7) << 6) | ((a & 0x7) << 9);

    // PIPE_POLYGON_MODE fill values for the rasterizer fill fields.
    public const uint PolygonModeFill = 0;

    // --- TGSI semantic names as the host's tgsi_semantic_names indexes them
    // (only the ones CosmSl can produce). Used by the text emitter. ---

    internal const string SemanticPositionName = "POSITION";
    internal const string SemanticColorName = "COLOR";
    internal const string SemanticTexCoordName = "TEXCOORD";
}
