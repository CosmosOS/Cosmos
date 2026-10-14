// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit.Resources;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.CosmSl;
using Cosmos.Kernel.System.Graphics.Rendering3D;

namespace Cosmos.Kernel.Drivers.Virtio.Display.VirtioGpu;

/// <summary>
/// <see cref="ICosmEngine"/> on the virtio-gpu driver's virgl path. The
/// engine turns the driver into a GL-class 3D device: it creates a virgl
/// context plus render targets through the control queue, compiles its
/// built-in shader pair set from CosmSl sources through
/// <see cref="Cosmos.Kernel.System.Graphics.CosmSl.CosmCompiler"/> and <see cref="TgsiEmitter"/>, and
/// drives draws by composing virgl command streams submitted with
/// SUBMIT_3D. Fails gracefully: <see cref="Ready"/> is false when the
/// context setup was rejected, and every other member becomes a no-op.
/// Thread context; one caller at a time, as the ring's canvas is.
/// </summary>
/// <remarks>
/// Presentation reads the rendered color target back into its guest
/// backing and copies it, bottom-up, into the scanout framebuffer the 2D
/// path draws into, so the virgl frame shows up through the ordinary
/// virtio-gpu scanout path and 2D drawing keeps working on top of it. The
/// host composites bottom-up, hence the row flip on present.
/// </remarks>
internal sealed class Virtio3DEngine : ICosmEngine
{
    // --- Constants ---

    // One virgl context per engine; the id is arbitrary but must be nonzero.
    private const uint ContextId = 1;

    // Context object handles (virgl object namespace, ids are per context).
    private const uint BlendHandle = 30;
    private const uint DsaHandle = 31;
    private const uint RasterizerHandle = 32;
    private const uint SamplerStateHandle = 33;
    private const uint ColorSurfaceHandle = 34;
    private const uint DepthSurfaceHandle = 35;
    private const uint ColoredVertexShaderHandle = 40;
    private const uint ColoredFragmentShaderHandle = 41;
    private const uint TexturedVertexShaderHandle = 42;
    private const uint TexturedFragmentShaderHandle = 43;
    private const uint FirstMeshElementHandle = 100;
    private const uint FirstSamplerViewHandle = 1000;

    // The uniform buffer holds the clip-space matrix rows of the current
    // draw (4 vec4). Sized to a round 256 bytes for headroom.
    private const uint UniformBufferLength = 256;
    private const int MatrixFloats = 16;

    // Bytes of one page, the alignment every backing asks for.
    private const int PageBytes = VirtioGpuState.PageBytes;

    // --- Private fields ---

    private readonly VirtioGpu3DState _state;
    private readonly List<uint> _stream = [];
    private readonly List<VirtioTextureData> _textures = [];
    private readonly float[] _uniforms = new float[MatrixFloats];

    private DmaBuffer? _colorStaging;
    private DmaBuffer? _uniformBacking;
    private uint _colorResourceId;
    private uint _depthResourceId;
    private uint _uniformResourceId;
    private bool _ready;
    private bool _sceneOpen;
    private Matrix4x4 _view = Matrix4x4.Identity;
    private Matrix4x4 _projection = Matrix4x4.Identity;
    private uint _nextMeshElementHandle = FirstMeshElementHandle;
    private uint _nextSamplerViewHandle = FirstSamplerViewHandle;

    // --- Properties ---

    /// <inheritdoc/>
    public string Name => "VirtioGpu3D";

    /// <inheritdoc/>
    public bool Ready => _ready;

    // --- Constructor ---

    /// <summary>
    /// Builds the 3D engine on the driver's virgl state: the context, the
    /// render targets, the built-in shaders and the fixed-function states
    /// go up here, so the canvas that installs the engine draws as soon as
    /// its constructor returns. <see cref="Ready"/> stays false when a
    /// step was rejected; the canvas then renders 2D only.
    /// </summary>
    /// <param name="state">The virgl state the engine submits through.</param>
    internal Virtio3DEngine(VirtioGpu3DState state)
    {
        _state = state;
        Initialize();
    }

    // --- ICosmEngine ---

    /// <inheritdoc/>
    public object CreateMeshData(
        ReadOnlySpan<Vector3> positions,
        ReadOnlySpan<uint> colors,
        ReadOnlySpan<Vector2> uvs,
        Texture? texture,
        ReadOnlySpan<ushort> indices,
        MeshTopology topology)
    {
        if (!_ready)
        {
            throw new InvalidOperationException("The virtio-gpu 3D engine is not ready.");
        }

        bool textured = !uvs.IsEmpty;
        bool colored = !colors.IsEmpty;

        // Stream order matches the TGSI input declarations of the shader
        // pair: position first, then color or uv, and the vertex elements
        // object follows the same order because the host binds attributes
        // positionally.
        int streamCount = 1 + (colored ? 1 : 0) + (textured ? 1 : 0);
        uint[] streamResourceIds = new uint[streamCount];

        if (CreateBufferResource((uint)(positions.Length * 12), out streamResourceIds[0]) is DmaBuffer positionBuffer)
        {
            positions.CopyTo(MemoryMarshal.Cast<byte, Vector3>(positionBuffer.Span));
            UploadBuffer(streamResourceIds[0], (uint)(positions.Length * 12));
        }

        int stream = 1;
        if (colored)
        {
            if (CreateBufferResource((uint)(colors.Length * 4), out streamResourceIds[stream]) is DmaBuffer colorBuffer)
            {
                colors.CopyTo(MemoryMarshal.Cast<byte, uint>(colorBuffer.Span));
                UploadBuffer(streamResourceIds[stream], (uint)(colors.Length * 4));
            }
            stream++;
        }

        if (textured)
        {
            if (CreateBufferResource((uint)(uvs.Length * 8), out streamResourceIds[stream]) is DmaBuffer uvBuffer)
            {
                uvs.CopyTo(MemoryMarshal.Cast<byte, Vector2>(uvBuffer.Span));
                UploadBuffer(streamResourceIds[stream], (uint)(uvs.Length * 8));
            }
            stream++;
        }

        uint indexResourceId = 0;
        if (CreateBufferResource((uint)(indices.Length * 2), out indexResourceId) is DmaBuffer indexBuffer)
        {
            indices.CopyTo(MemoryMarshal.Cast<byte, ushort>(indexBuffer.Span));
            UploadBuffer(indexResourceId, (uint)(indices.Length * 2));
        }

        uint elementsHandle = CreateVertexElements(streamCount, colored, textured);

        return new VirtioMeshData
        {
            StreamResourceIds = streamResourceIds,
            IndexResourceId = indexResourceId,
            VertexElementsHandle = elementsHandle,
            VertexCount = positions.Length,
            IndexCount = indices.Length,
            Lines = topology == MeshTopology.Lines,
            HasTexture = textured,
            Texture = texture,
        };
    }

    /// <inheritdoc/>
    public void DestroyMeshData(object meshData)
    {
        if (!_ready || meshData is not VirtioMeshData data)
        {
            return;
        }

        foreach (uint resourceId in data.StreamResourceIds)
        {
            DestroyBufferResource(resourceId);
        }

        DestroyBufferResource(data.IndexResourceId);
        data.StreamResourceIds = [];
        data.IndexResourceId = 0;

        _stream.Clear();
        Cmd(Virgl.CmdDestroyObject, Virgl.ObjectVertexElements, 1);
        U(data.VertexElementsHandle);
        Submit();

        data.VertexElementsHandle = 0;
    }

    /// <inheritdoc/>
    public object CreateTextureData(Image image)
    {
        if (!_ready)
        {
            throw new InvalidOperationException("The virtio-gpu 3D engine is not ready.");
        }

        uint resourceId = _state.ReserveResourceId();
        uint width = (uint)image.Width;
        uint height = (uint)image.Height;

        if (!_state.Create3DResource(resourceId, Virgl.PipeTexture2D, Virgl.FormatB8G8R8A8Unorm,
                                     Virgl.BindSamplerView, width, height, 1, 1, 0, 1))
        {
            return new VirtioTextureData();
        }

        uint size = width * height * 4;
        DmaBuffer backing = _state.AllocateBacking((int)size, PageBytes);
        image.RawData.AsSpan().CopyTo(MemoryMarshal.Cast<byte, int>(backing.Span));

        if (!_state.AttachBacking(resourceId, backing))
        {
            return new VirtioTextureData();
        }

        if (!_state.AttachResourceToContext(ContextId, resourceId))
        {
            // Unref on the device and leave the backing to the ledger.
            _state.Destroy3DResource(resourceId);
            return new VirtioTextureData();
        }

        UploadTexture(resourceId, width, height);

        uint samplerViewHandle = _nextSamplerViewHandle++;
        _stream.Clear();
        Cmd(Virgl.CmdCreateObject, Virgl.ObjectSamplerView, 6);
        U(samplerViewHandle);
        U(resourceId);
        U(Virgl.FormatB8G8R8A8Unorm);
        U(0); // first level
        U(0); // last level
        U(Virgl.SamplerViewSwizzle(Virgl.SwizzleX, Virgl.SwizzleY, Virgl.SwizzleZ, Virgl.SwizzleW));
        Submit();

        VirtioTextureData data = new()
        {
            ResourceId = resourceId,
            SamplerViewHandle = samplerViewHandle,
            Backing = backing,
        };
        return data;
    }

    /// <summary>
    /// Binds the ring texture the canvas created for
    /// <paramref name="textureData"/> to it, so textured draws resolve the
    /// sampler view of the texture a mesh maps. Thread context.
    /// </summary>
    /// <param name="texture">The ring texture holding <paramref name="textureData"/>.</param>
    /// <param name="textureData">The data <see cref="CreateTextureData"/> built.</param>
    internal void BindTexture(Texture texture, object textureData)
    {
        if (textureData is VirtioTextureData data)
        {
            data.Bound = texture;
            _textures.Add(data);
        }
    }

    /// <summary>Resolves the engine data of a bound texture, or null when it was not bound.</summary>
    private VirtioTextureData? TextureDataOf(Texture? texture)
    {
        if (texture is null)
        {
            return null;
        }

        foreach (VirtioTextureData data in _textures)
        {
            if (ReferenceEquals(data.Bound, texture))
            {
                return data;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public void DestroyTextureData(object textureData)
    {
        if (!_ready || textureData is not VirtioTextureData data || data.ResourceId == 0)
        {
            return;
        }

        _stream.Clear();
        Cmd(Virgl.CmdDestroyObject, Virgl.ObjectSamplerView, 1);
        U(data.SamplerViewHandle);
        Submit();

        for (int i = 0; i < _textures.Count; i++)
        {
            if (ReferenceEquals(_textures[i], data))
            {
                _textures.RemoveAt(i);
                break;
            }
        }

        _state.Destroy3DResource(data.ResourceId);
        data.ResourceId = 0;
        data.Backing = null;
    }

    /// <inheritdoc/>
    public void SetCamera(in Matrix4x4 view, in Matrix4x4 projection)
    {
        _view = view;
        _projection = projection;
    }

    /// <inheritdoc/>
    public void ClearScene(Color color, float depth)
    {
        if (!_ready)
        {
            return;
        }

        _stream.Clear();
        Cmd(Virgl.CmdClear, 0, 8);
        U(Virgl.ClearColorOffset0 | Virgl.ClearDepth);
        F(color.R / 255f);
        F(color.G / 255f);
        F(color.B / 255f);
        F(color.A / 255f);
        DepthBits(depth);
        U(0);
        Submit();

        _sceneOpen = true;
    }

    /// <inheritdoc/>
    public void DrawMesh(object meshData, in Matrix4x4 world)
    {
        if (!_ready || meshData is not VirtioMeshData data)
        {
            return;
        }

        // The clip transform composes in row-vector order, matching the
        // System.Numerics matrices the canvas layer feeds in.
        Matrix4x4 clip = world * _view * _projection;
        _uniforms[0] = clip.M11;
        _uniforms[1] = clip.M12;
        _uniforms[2] = clip.M13;
        _uniforms[3] = clip.M14;
        _uniforms[4] = clip.M21;
        _uniforms[5] = clip.M22;
        _uniforms[6] = clip.M23;
        _uniforms[7] = clip.M24;
        _uniforms[8] = clip.M31;
        _uniforms[9] = clip.M32;
        _uniforms[10] = clip.M33;
        _uniforms[11] = clip.M34;
        _uniforms[12] = clip.M41;
        _uniforms[13] = clip.M42;
        _uniforms[14] = clip.M43;
        _uniforms[15] = clip.M44;

        DmaBuffer? uniformBacking = _uniformBacking;
        if (uniformBacking is null)
        {
            return;
        }

        _uniforms.CopyTo(MemoryMarshal.Cast<byte, float>(uniformBacking.Span));

        _stream.Clear();

        // Vertex buffers, in the same order as the vertex elements object.
        Cmd(Virgl.CmdSetVertexBuffers, 0, (uint)data.StreamResourceIds.Length * 3);
        for (int i = 0; i < data.StreamResourceIds.Length; i++)
        {
            U(data.StreamResourceIds[i] == 0 ? 0 : StreamStride(data, i));
            U(0);
            U(data.StreamResourceIds[i]);
        }

        Cmd(Virgl.CmdBindObject, Virgl.ObjectVertexElements, 1);
        U(data.VertexElementsHandle);

        Cmd(Virgl.CmdSetIndexBuffer, 0, 3);
        U(data.IndexResourceId);
        U(2);
        U(0);

        // The clip matrix rows ride in uniform buffer slot 0; uploaded per
        // draw because the world matrix changes constantly.
        AppendTransfer3D(_uniformResourceId, MatrixFloats * 4);

        Cmd(Virgl.CmdSetUniformBuffer, 0, 5);
        U(Virgl.ShaderVertex);
        U(0);
        U(0);
        U(MatrixFloats * 4);
        U(_uniformResourceId);

        VirtioTextureData? textureData = TextureDataOf(data.Texture);
        if (data.HasTexture && textureData is not null)
        {
            Cmd(Virgl.CmdSetSamplerViews, 0, 3);
            U(Virgl.ShaderFragment);
            U(0);
            U(textureData.SamplerViewHandle);

            Cmd(Virgl.CmdBindSamplerStates, 0, 3);
            U(Virgl.ShaderFragment);
            U(0);
            U(SamplerStateHandle);
        }

        Cmd(Virgl.CmdDrawVbo, 0, 12);
        U(0); // start
        U((uint)data.IndexCount);
        U(data.Lines ? Virgl.PrimLines : Virgl.PrimTriangles);
        U(1); // indexed
        U(1); // instance count
        U(0); // index bias
        U(0); // start instance
        U(0); // primitive restart
        U(0); // restart index
        U(0); // min index
        U((uint)(data.VertexCount - 1));
        U(0); // count from streamout
        Submit();

        _sceneOpen = true;
    }

    /// <inheritdoc/>
    public void Present()
    {
        if (!_ready || !_sceneOpen)
        {
            return;
        }

        DmaBuffer? colorStaging = _colorStaging;
        DeviceRegion? framebuffer = _state.Framebuffer;
        if (colorStaging is null || framebuffer is null)
        {
            return;
        }

        int width = _state.Width;
        int height = _state.Height;

        // Read the rendered frame back into the color target's guest
        // backing, then copy it bottom-up into the 2D framebuffer so it
        // goes out through the ordinary scanout path.
        if (!_state.TransferBox3D(_colorResourceId, toHost: false, 0, 0, 0, (uint)width, (uint)height, 0))
        {
            return;
        }

        Span<uint> source = MemoryMarshal.Cast<byte, uint>(colorStaging.Span);
        Span<uint> target = framebuffer.As<uint>();
        int scanoutPitch = _state.Mode.Pitch / 4;
        for (int row = 0; row < height; row++)
        {
            int flip = height - 1 - row;
            source.Slice(flip * width, width).CopyTo(target.Slice(row * scanoutPitch, width));
        }

        _state.Flush(0, 0, width, height);
        _sceneOpen = false;
    }

    // --- Methods for the canvas ---

    /// <summary>
    /// Reads a rectangle of the last rendered frame into
    /// <paramref name="destination"/>, one ARGB dword per pixel in
    /// <paramref name="width"/> stride, top-down like the screen shows it.
    /// Thread context.
    /// </summary>
    /// <param name="x">Left edge of the rectangle on the canvas.</param>
    /// <param name="y">Top edge of the rectangle on the canvas.</param>
    /// <param name="width">Width of the rectangle.</param>
    /// <param name="height">Height of the rectangle.</param>
    /// <param name="destination">The pixels, <paramref name="width"/> dwords per row.</param>
    /// <returns>False when the engine is not ready or the readback was refused.</returns>
    internal bool ReadPixels(int x, int y, int width, int height, Span<int> destination)
    {
        DmaBuffer? colorStaging = _colorStaging;
        if (!_ready || colorStaging is null || width <= 0 || height <= 0)
        {
            return false;
        }

        // The host composites bottom-up, so the box's top edge sits
        // height rows above the screen rectangle's bottom edge and the
        // rows come back flipped.
        uint boxY = (uint)(_state.Height - y - height);
        if (!_state.TransferBox3D(_colorResourceId, toHost: false, 0, (uint)x, boxY, (uint)width, (uint)height, (uint)(width * 4)))
        {
            return false;
        }

        Span<uint> source = MemoryMarshal.Cast<byte, uint>(colorStaging.Span);
        for (int row = 0; row < height; row++)
        {
            int flip = height - 1 - row;
            source.Slice(flip * width, width).CopyTo(MemoryMarshal.Cast<int, uint>(destination.Slice(row * width, width)));
        }

        return true;
    }

    /// <summary>Destroys the context and every object it held; the engine is dead afterwards. Thread context.</summary>
    internal void Shutdown()
    {
        if (!_ready)
        {
            return;
        }

        _state.DestroyContext(ContextId);
        _ready = false;
        _sceneOpen = false;
    }

    /// <summary>Destroys and rebuilds the render targets at the canvas size, dropping the open scene. Thread context.</summary>
    internal void RecreateRenderTargets()
    {
        if (!_ready)
        {
            return;
        }

        DestroyRenderTargets();
        if (CreateRenderTargets() && CreateFixedStates() && ApplySceneDefaults())
        {
            _sceneOpen = false;
        }
        else
        {
            _ready = false;
        }
    }

    // --- Initialization ---

    private void Initialize()
    {
        if (!_state.CreateContext(ContextId, "Cosmos3D"))
        {
            _state.Log("Virtio3D: the device refused the virgl context; 3D unavailable");
            return;
        }

        if (!CreateRenderTargets())
        {
            _state.Log("Virtio3D: render target setup failed");
            return;
        }

        if (!CompileShaders())
        {
            _state.Log("Virtio3D: built-in shader setup failed");
            return;
        }

        if (!CreateFixedStates() || !ApplySceneDefaults())
        {
            _state.Log("Virtio3D: fixed state setup failed");
            return;
        }

        _ready = true;
        _state.Log($"Virtio3D: engine ready ({_state.Width}x{_state.Height})");
    }

    private bool CreateRenderTargets()
    {
        uint width = (uint)_state.Width;
        uint height = (uint)_state.Height;

        // Color target: host texture the pipeline renders into, with guest
        // backing for the present-time readback.
        _colorResourceId = _state.ReserveResourceId();
        _colorStaging = _state.AllocateBacking((int)(width * height * 4), PageBytes);
        if (!_state.Create3DResource(_colorResourceId, Virgl.PipeTexture2D, Virgl.FormatB8G8R8A8Unorm,
                                     Virgl.BindRenderTarget, width, height, 1, 1, 0, 1)
            || !_state.AttachBacking(_colorResourceId, _colorStaging)
            || !_state.AttachResourceToContext(ContextId, _colorResourceId))
        {
            return false;
        }

        _depthResourceId = _state.ReserveResourceId();
        if (!_state.Create3DResource(_depthResourceId, Virgl.PipeTexture2D, Virgl.FormatZ32Float,
                                     Virgl.BindDepthStencil, width, height, 1, 1, 0, 1)
            || !_state.AttachResourceToContext(ContextId, _depthResourceId))
        {
            return false;
        }

        // Uniform buffer: guest-side constant store for the clip matrix.
        _uniformResourceId = _state.ReserveResourceId();
        _uniformBacking = _state.AllocateBacking((int)UniformBufferLength, PageBytes);
        if (!_state.Create3DResource(_uniformResourceId, Virgl.PipeBuffer, Virgl.FormatR8Unorm,
                                     Virgl.BindConstantBuffer, UniformBufferLength, 1, 1, 1, 0, 1)
            || !_state.AttachBacking(_uniformResourceId, _uniformBacking)
            || !_state.AttachResourceToContext(ContextId, _uniformResourceId))
        {
            return false;
        }

        return true;
    }

    private void DestroyRenderTargets()
    {
        if (_colorResourceId != 0)
        {
            _state.Destroy3DResource(_colorResourceId);
            _colorResourceId = 0;
        }

        if (_depthResourceId != 0)
        {
            _state.Destroy3DResource(_depthResourceId);
            _depthResourceId = 0;
        }

        if (_uniformResourceId != 0)
        {
            _state.Destroy3DResource(_uniformResourceId);
            _uniformResourceId = 0;
        }

        _colorStaging = null;
        _uniformBacking = null;
    }

    /// <summary>
    /// Compiles the four built-in shaders from their CosmSl sources and
    /// uploads them as virgl shader objects. The payload is the TGSI assembly
    /// the host parses with its own assembler, which keeps the guest free of
    /// host opcode numbering assumptions.
    /// </summary>
    private bool CompileShaders()
    {
        return CompileShader(ColoredVertexShaderHandle, Virgl.ShaderVertex, ColoredVertexShader)
            && CompileShader(ColoredFragmentShaderHandle, Virgl.ShaderFragment, ColoredFragmentShader)
            && CompileShader(TexturedVertexShaderHandle, Virgl.ShaderVertex, TexturedVertexShader)
            && CompileShader(TexturedFragmentShaderHandle, Virgl.ShaderFragment, TexturedFragmentShader);
    }

    private bool CompileShader(uint handle, uint stage, string source)
    {
        ShaderProgramNode ast;
        TgsiShader shader;
        try
        {
            ast = CosmCompiler.Parse(source);
            shader = TgsiEmitter.Emit(ast, stage == Virgl.ShaderVertex
                ? ShaderStage.Vertex
                : ShaderStage.Fragment);
        }
        catch (CosmSlException error)
        {
            _state.Log($"Virtio3D: built-in shader failed to compile: {error.Message}");
            return false;
        }

        // Payload: handle, stage, byte length (with NUL), token budget,
        // streamout count (0), then the NUL-terminated TGSI text.
        int textBytes = shader.Text.Length + 1;
        int textDwords = (textBytes + 3) / 4;

        _stream.Clear();
        Cmd(Virgl.CmdCreateObject, Virgl.ObjectShader, (uint)(5 + textDwords));
        U(handle);
        U(stage);
        U((uint)textBytes);
        U((uint)(textDwords + 128));
        U(0);

        // The TGSI text is ASCII; pack it into the stream four bytes per
        // dword, NUL-padded to the text's dword count.
        for (int i = 0; i < textDwords; i++)
        {
            uint word = 0;
            for (int b = 0; b < 4; b++)
            {
                int index = i * 4 + b;
                byte value = index < shader.Text.Length ? (byte)shader.Text[index] : (byte)0;
                word |= (uint)value << (b * 8);
            }
            U(word);
        }

        Cmd(Virgl.CmdBindShader, 0, 2);
        U(handle);
        U(stage);

        return Submit();
    }

    private bool CreateFixedStates()
    {
        _stream.Clear();

        // Blend: disabled everywhere, but keep the full RGBA write mask.
        Cmd(Virgl.CmdCreateObject, Virgl.ObjectBlend, 11);
        U(BlendHandle);
        U(0);
        U(0);
        for (int i = 0; i < 8; i++)
        {
            U(0xFu << 27);
        }

        // Depth-stencil: depth test and write enabled, LESS compare.
        Cmd(Virgl.CmdCreateObject, Virgl.ObjectDsa, 5);
        U(DsaHandle);
        U(Virgl.DsaS0DepthEnable | Virgl.DsaS0DepthWritemask | Virgl.DsaS0DepthFunc(Virgl.FuncLess));
        U(0);
        U(0);
        U(0);

        // Rasterizer: no culling (Canvas3D draws both faces), depth clip on,
        // and the half-z clip convention the System.Numerics projection uses.
        Cmd(Virgl.CmdCreateObject, Virgl.ObjectRasterizer, 9);
        U(RasterizerHandle);
        U(Virgl.RsS0DepthClip | Virgl.RsS0ClipHalfZ);
        F(1f);
        U(0);
        U(0);
        F(1f);
        F(0f);
        F(0f);
        F(0f);

        // Sampler: clamp to edge, linear filtering, no mipmaps.
        Cmd(Virgl.CmdCreateObject, Virgl.ObjectSamplerState, 9);
        U(SamplerStateHandle);
        U(Virgl.SamplerS0(Virgl.TexWrapClampToEdge, Virgl.TexWrapClampToEdge, Virgl.TexWrapClampToEdge,
                          Virgl.TexFilterLinear, Virgl.TexMipFilterNone, Virgl.TexFilterLinear));
        F(0f);
        F(0f);
        F(0f);
        U(0);
        U(0);
        U(0);
        U(0);

        // Surfaces bound as render targets.
        Cmd(Virgl.CmdCreateObject, Virgl.ObjectSurface, 5);
        U(ColorSurfaceHandle);
        U(_colorResourceId);
        U(Virgl.FormatB8G8R8A8Unorm);
        U(0);
        U(0);

        Cmd(Virgl.CmdCreateObject, Virgl.ObjectSurface, 5);
        U(DepthSurfaceHandle);
        U(_depthResourceId);
        U(Virgl.FormatZ32Float);
        U(0);
        U(0);

        return Submit();
    }

    private bool ApplySceneDefaults()
    {
        _stream.Clear();

        Cmd(Virgl.CmdBindObject, Virgl.ObjectBlend, 1);
        U(BlendHandle);

        Cmd(Virgl.CmdBindObject, Virgl.ObjectDsa, 1);
        U(DsaHandle);

        Cmd(Virgl.CmdBindObject, Virgl.ObjectRasterizer, 1);
        U(RasterizerHandle);

        Cmd(Virgl.CmdBindObject, Virgl.ObjectSamplerState, 1);
        U(SamplerStateHandle);

        Cmd(Virgl.CmdSetFramebufferState, 0, 3);
        U(1);
        U(DepthSurfaceHandle);
        U(ColorSurfaceHandle);

        uint width = (uint)_state.Width;
        uint height = (uint)_state.Height;
        Cmd(Virgl.CmdSetViewportState, 0, 7);
        U(0); // start slot
        F(width * 0.5f);
        F(height * 0.5f);
        F(0.5f);
        F(width * 0.5f);
        F(height * 0.5f);
        F(0.5f);

        return Submit();
    }

    // --- Mesh resource helpers ---

    private DmaBuffer? CreateBufferResource(uint byteLength, out uint resourceId)
    {
        resourceId = _state.ReserveResourceId();
        if (!_state.Create3DResource(resourceId, Virgl.PipeBuffer, Virgl.FormatR8Unorm,
                                     Virgl.BindVertexBuffer | Virgl.BindIndexBuffer | Virgl.BindConstantBuffer,
                                     byteLength, 1, 1, 1, 0, 1))
        {
            resourceId = 0;
            return null;
        }

        DmaBuffer backing = _state.AllocateBacking((int)byteLength, PageBytes);
        if (!_state.AttachBacking(resourceId, backing)
            || !_state.AttachResourceToContext(ContextId, resourceId))
        {
            resourceId = 0;
            return null;
        }

        return backing;
    }

    private void DestroyBufferResource(uint resourceId)
    {
        // The backing went to the device with the attach; the ledger frees
        // the memory with the binding, so only the unref rides here.
        if (resourceId != 0)
        {
            _state.Destroy3DResource(resourceId);
        }
    }

    /// <summary>
    /// Uploads buffer contents: the bytes land in the resource's guest
    /// backing first, then a TRANSFER3D in the stream pushes them host-side.
    /// </summary>
    private void UploadBuffer(uint resourceId, uint byteLength)
    {
        _stream.Clear();
        AppendTransfer3D(resourceId, byteLength);
        Submit();
    }

    private void UploadTexture(uint resourceId, uint width, uint height)
    {
        _stream.Clear();
        Cmd(Virgl.CmdTransfer3D, 0, Virgl.Transfer3DSize);
        U(resourceId);
        U(0); // level
        U(Virgl.TransferWrite);
        U(0); // stride (host inferred)
        U(0); // layer stride
        U(0);
        U(0);
        U(0);
        U(width);
        U(height);
        U(1);
        U(0); // byte offset
        U(Virgl.TransferToHost);
        Submit();
    }

    /// <summary>Appends a full-buffer upload transfer to the current stream.</summary>
    private void AppendTransfer3D(uint resourceId, uint byteLength)
    {
        Cmd(Virgl.CmdTransfer3D, 0, Virgl.Transfer3DSize);
        U(resourceId);
        U(0); // level
        U(Virgl.TransferWrite);
        U(0); // stride (host inferred)
        U(0); // layer stride
        U(0);
        U(0);
        U(0);
        U(byteLength);
        U(1);
        U(1);
        U(0); // byte offset
        U(Virgl.TransferToHost);
    }

    private uint CreateVertexElements(int streamCount, bool colored, bool textured)
    {
        uint handle = _nextMeshElementHandle++;

        _stream.Clear();
        Cmd(Virgl.CmdCreateObject, Virgl.ObjectVertexElements, (uint)(1 + streamCount * 4));
        U(handle);

        int element = 0;
        AppendVertexElement(element++, 0, Virgl.FormatR32G32B32Float);

        if (colored)
        {
            AppendVertexElement(element++, 1, Virgl.FormatB8G8R8A8Unorm);
        }

        if (textured)
        {
            AppendVertexElement(element++, 1, Virgl.FormatR32G32Float);
        }

        Submit();
        return handle;
    }

    private void AppendVertexElement(int element, int bufferIndex, uint format)
    {
        U(0); // src offset
        U(0); // instance divisor
        U((uint)bufferIndex);
        U(format);
    }

    // --- Stream helpers ---

    private void Cmd(uint cmd, uint objectType, uint length) => _stream.Add(Virgl.Cmd(cmd, objectType, length));

    private void U(uint value) => _stream.Add(value);

    private void F(float value) => _stream.Add((uint)BitConverter.SingleToInt32Bits(value));

    /// <summary>Splits a double into the two dwords the CLEAR command expects.</summary>
    private void DepthBits(float depth)
    {
        ulong bits = (ulong)BitConverter.DoubleToInt64Bits((double)depth);
        U((uint)bits);
        U((uint)(bits >> 32));
    }

    private bool Submit()
    {
        if (_stream.Count == 0)
        {
            return true;
        }

        bool ok = _state.Submit3D(ContextId, CollectionsMarshal.AsSpan(_stream));
        if (!ok)
        {
            _state.Log("Virtio3D: SUBMIT_3D failed");
        }

        return ok;
    }

    private static uint StreamStride(VirtioMeshData data, int stream)
    {
        // Stream 0 is positions; colored meshes carry packed ARGB colors on
        // stream 1, textured meshes carry vec2 UVs there.
        if (stream == 0)
        {
            return 12;
        }

        return data.HasTexture ? 8u : 4u;
    }

    // --- Built-in shaders (CosmSl) ---

    /// <summary>
    /// The CosmSl sources of the built-in shader pairs. They double as the
    /// reference for how the language lowers: the vertex shaders push the
    /// clip-space position through the uniform matrix, the fragment shaders
    /// resolve the final color.
    /// </summary>
    private const string ColoredVertexShader =
        """
        struct VertexInput {
            vec4 position : POSITION;
            vec4 color : COLOR;
        };

        struct VertexOutput {
            vec4 position : SV_POSITION;
            vec4 color : COLOR;
        };

        uniform mat4 u_mvp;

        VertexOutput main(VertexInput input) {
            VertexOutput output;
            output.position = mul(u_mvp, input.position);
            output.color = input.color;
            return output;
        }
        """;

    private const string ColoredFragmentShader =
        """
        struct FragmentInput {
            vec4 color; // Defaults semantic to COLOR
        };

        vec4 main(FragmentInput input) : SV_TARGET {
            return input.color;
        }
        """;

    private const string TexturedVertexShader =
        """
        struct VertexInput {
            vec4 position : POSITION;
            vec2 uv : TEXCOORD0;
        };

        struct VertexOutput {
            vec4 position : SV_POSITION;
            vec2 uv : TEXCOORD0;
        };

        uniform mat4 u_mvp;

        VertexOutput main(VertexInput input) {
            VertexOutput output;
            output.position = mul(u_mvp, input.position);
            output.uv = input.uv;
            return output;
        }
        """;

    private const string TexturedFragmentShader =
        """
        struct FragmentInput {
            vec2 uv; // Defaults semantic to TEXCOORD0
        };

        uniform sampler2D u_tex;

        vec4 main(FragmentInput input) : SV_TARGET {
            return tex2D(u_tex, input.uv);
        }
        """;
}

/// <summary>
/// Device-side resources of a mesh on the virgl backend: one buffer resource
/// per vertex attribute stream, the index buffer, and the vertex elements
/// object that binds the streams to the shader inputs.
/// </summary>
internal sealed class VirtioMeshData
{
    /// <summary>One buffer resource id per vertex attribute stream.</summary>
    public uint[] StreamResourceIds = [];

    /// <summary>The buffer resource id holding the 16-bit indices.</summary>
    public uint IndexResourceId;

    /// <summary>The vertex elements object bound before every draw.</summary>
    public uint VertexElementsHandle;

    /// <summary>The number of vertices in the mesh.</summary>
    public int VertexCount;

    /// <summary>The number of indices in the mesh.</summary>
    public int IndexCount;

    /// <summary>Whether the indices assemble into lines instead of triangles.</summary>
    public bool Lines;

    /// <summary>Whether the mesh maps a texture.</summary>
    public bool HasTexture;

    /// <summary>The texture mapped onto the mesh, if any.</summary>
    public Texture? Texture;
}

/// <summary>
/// Device-side resources of a texture on the virgl backend: the host-side
/// image resource, its sampler view object, and the guest backing that fed
/// the upload.
/// </summary>
internal sealed class VirtioTextureData
{
    /// <summary>The host texture resource id; 0 marks a failed upload.</summary>
    public uint ResourceId;

    /// <summary>The sampler view object bound for textured draws.</summary>
    public uint SamplerViewHandle;

    /// <summary>The guest backing the image was uploaded from.</summary>
    public DmaBuffer? Backing;

    /// <summary>The ring texture the canvas bound the data to, for the sampler lookup of a draw.</summary>
    public Texture? Bound;
}
