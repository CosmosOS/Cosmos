// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Drivers.Svga;
using Cosmos.Kernel.HAL.DriverKit;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// The SVGA3D command layer over <see cref="VmwareSvgaFifo"/>: surfaces,
/// contexts, shaders, render state, draws and DMA transfers, each one a
/// header and a body reserved in the FIFO and written with
/// <c>MemoryMarshal</c>, or built in a bounce buffer and appended through
/// the wrap when the command would straddle the FIFO's end. The DMA scratch is the tail of VRAM (its last
/// eighth), addressed through <c>SVGA_GMR_FRAMEBUFFER</c>, which every
/// SVGA II accepts; the visible frame never reaches it at the listed
/// modes. Surface ids and context ids start at 1 and count up, one
/// sequence per instance. Only meaningful when the host negotiated 3D;
/// QEMU exposes no 3D, so on QEMU the commands sit in FIFO memory for the
/// wire tests to inspect while the adapter is disabled. Thread context;
/// one caller at a time, as the ring's canvas is.
/// </summary>
internal sealed class VmwareSvga3D
{
    // --- Constants ---

    /// <summary>SVGA_GMR_FRAMEBUFFER: the guest memory region id that addresses VRAM, valid on every SVGA II.</summary>
    private const uint FramebufferGmr = 0xFFFFFFFEu;

    /// <summary>Bytes of one pixel of the surfaces the canvas reads back and uploads.</summary>
    private const uint BytesPerPixel = 4;

    /// <summary>The DMA scratch is this fraction of VRAM, at its end.</summary>
    private const uint ScratchDivisor = 8;

    /// <summary>Rounds a scratch allocation up to a dword.</summary>
    private const uint DwordMask = ~3u;

    /// <summary>The size of a shader constant register in bytes.</summary>
    private const int ShaderConstantBytes = 16;

    // --- Private fields ---

    private readonly VmwareSvgaFifo _fifo;
    private readonly DeviceRegion _vram;
    private readonly uint _scratchStart;
    private readonly uint _scratchEnd;
    private uint _scratchNext;
    private uint _lastScratchSize;
    private uint _contextId;
    private uint _surfaceId;
    private uint _shaderIdVertex;
    private uint _shaderIdPixel;
    private uint _guestFenceCounter = 1;
    private uint _syncedFence;
    private uint _lastFence = 1;

    /// <summary>The command a straddling reservation is built in before <see cref="CommitCommand"/> appends it through the wrap; grown to the largest such command.</summary>
    private byte[] _bounce = [];

    /// <summary>Bytes of the command waiting in <see cref="_bounce"/>, 0 when the current command was placed in FIFO memory.</summary>
    private uint _bounceBytes;

    // --- Constructor ---

    /// <summary>
    /// Creates the layer over an adapter's FIFO and VRAM, with the DMA
    /// scratch at the last eighth of VRAM. Thread context.
    /// </summary>
    /// <param name="fifo">The adapter's registers and FIFO.</param>
    /// <param name="vram">The adapter's VRAM, BAR 1.</param>
    internal VmwareSvga3D(VmwareSvgaFifo fifo, DeviceRegion vram)
    {
        _fifo = fifo;
        _vram = vram;
        uint vramBytes = (uint)Math.Min(vram.Length, uint.MaxValue);
        uint scratchBytes = vramBytes / ScratchDivisor;
        _scratchStart = (vramBytes - scratchBytes) & DwordMask;
        _scratchEnd = vramBytes;
        _scratchNext = _scratchStart;
    }

    // --- Synchronisation ---

    /// <summary>
    /// Waits until the host has consumed the FIFO up to <paramref name="fence"/>.
    /// Returns at once while the adapter is disabled: a disabled adapter
    /// never consumes the FIFO, so waiting would spin forever; mesh and
    /// texture uploads then queue their commands and copy their data
    /// without waiting. A fence already waited for costs nothing. Thread
    /// context.
    /// </summary>
    /// <param name="fence">A value <see cref="InsertFence"/> returned.</param>
    internal void SyncToFence(uint fence)
    {
        if (!_fifo.IsEnabled)
        {
            return;
        }

        if (fence <= _syncedFence)
        {
            return;
        }

        _fifo.WaitForFifo();
        _syncedFence = _guestFenceCounter;
    }

    /// <summary>Marks the point every command written so far sits before. Thread context.</summary>
    /// <returns>The fence, for <see cref="SyncToFence"/>.</returns>
    internal uint InsertFence() => ++_guestFenceCounter;

    // --- Contexts and surfaces ---

    /// <summary>CONTEXT_DEFINE with the next context id. Thread context.</summary>
    /// <returns>The context id, 1 for the first one.</returns>
    internal uint DefineContext()
    {
        uint cid = ++_contextId;
        SVGA3dCmdDefineContext command = new() { cid = cid };
        Span<byte> body = BeginCommand(FIFOCommand.DEFINE_CONTEXT, (uint)Unsafe.SizeOf<SVGA3dCmdDefineContext>());
        WriteBody(body, in command);
        CommitCommand();
        return cid;
    }

    /// <summary>CONTEXT_DESTROY. Thread context.</summary>
    /// <param name="cid">The context id.</param>
    internal void DestroyContext(uint cid)
    {
        Span<byte> body = BeginCommand(FIFOCommand.DESTROY_CONTEXT, sizeof(uint));
        WriteBody(body, in cid);
        CommitCommand();
    }

    /// <summary>SURFACE_DEFINE of a one-face, one-mip surface with the next surface id. Thread context.</summary>
    /// <param name="width">Width in pixels, or bytes for a buffer.</param>
    /// <param name="height">Height in pixels, or 1 for a buffer.</param>
    /// <param name="format">The surface format.</param>
    /// <returns>The surface's image id: face 0, mip 0.</returns>
    internal SVGA3dSurfaceImageId DefineSurface(uint width, uint height, SVGA3dSurfaceFormat format)
    {
        uint sid = ++_surfaceId;
        uint fixedBytes = (uint)Unsafe.SizeOf<SVGA3dCmdDefineSurface>();
        Span<byte> body = BeginCommand(FIFOCommand.DEFINE_SURFACE, fixedBytes + (uint)Unsafe.SizeOf<SVGA3dSize>());

        SVGA3dCmdDefineSurface command = new()
        {
            sid = sid,
            flags = 0,
            format = format,
        };
        command.face[0] = 1;
        WriteBody(body, in command);

        SVGA3dSize mip = new()
        {
            width = width,
            height = height,
            depth = 1,
        };
        WriteBody(body.Slice((int)fixedBytes), in mip);
        CommitCommand();

        return new SVGA3dSurfaceImageId { sid = sid, face = 0, mipmap = 0 };
    }

    /// <summary>SURFACE_DESTROY. Thread context.</summary>
    /// <param name="sid">The surface id.</param>
    internal void DestroySurface(uint sid)
    {
        Span<byte> body = BeginCommand(FIFOCommand.DESTROY_SURFACE, sizeof(uint));
        WriteBody(body, in sid);
        CommitCommand();
    }

    /// <summary>
    /// A buffer surface holding <paramref name="data"/>: SURFACE_DEFINE of a
    /// buffer as wide as the data, the data copied to the scratch, a
    /// SURFACE_DMA into the surface, a fence waited for (not while the
    /// adapter is disabled), the scratch popped. Thread context.
    /// </summary>
    /// <typeparam name="T">The element type, an unmanaged struct.</typeparam>
    /// <param name="data">The elements, copied as bytes.</param>
    /// <returns>The buffer surface's id.</returns>
    internal uint CreateStaticArrayBuffer<T>(ReadOnlySpan<T> data) where T : unmanaged
    {
        uint size = (uint)(data.Length * Unsafe.SizeOf<T>());
        uint sid = DefineSurface(size, 1, SVGA3dSurfaceFormat.SVGA3D_BUFFER).sid;

        uint scratch = AllocateScratch(size, out SVGAGuestPtr guest);
        MemoryMarshal.AsBytes(data).CopyTo(_vram.Span.Slice((int)scratch, (int)size));
        SurfaceDma2D(sid, in guest, SVGA3dTransferType.SVGA3D_WRITE_HOST_VRAM, size, 1);

        uint fence = InsertFence();
        SyncToFence(fence);
        PopScratch();
        return sid;
    }

    /// <summary>
    /// An A8R8G8B8 surface holding an image: SURFACE_DEFINE, the pixels
    /// copied to the scratch, a SURFACE_DMA into the surface, a fence waited
    /// for (not while the adapter is disabled), the scratch popped. Thread
    /// context.
    /// </summary>
    /// <param name="pixels">The pixels, width times height of them, row-major.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <returns>The surface's image id.</returns>
    internal SVGA3dSurfaceImageId DefineSurfaceFromImage(ReadOnlySpan<int> pixels, uint width, uint height)
    {
        uint sid = DefineSurface(width, height, SVGA3dSurfaceFormat.SVGA3D_A8R8G8B8).sid;
        uint size = width * height * BytesPerPixel;

        uint scratch = AllocateScratch(size, out SVGAGuestPtr guest);
        MemoryMarshal.AsBytes(pixels.Slice(0, (int)(width * height))).CopyTo(_vram.Span.Slice((int)scratch, (int)size));
        SurfaceDma2D(sid, in guest, SVGA3dTransferType.SVGA3D_WRITE_HOST_VRAM, width, height);

        uint fence = InsertFence();
        SyncToFence(fence);
        PopScratch();
        return new SVGA3dSurfaceImageId { sid = sid, face = 0, mipmap = 0 };
    }

    /// <summary>
    /// Reads a rectangle of a surface back: a SURFACE_DMA out of the surface
    /// into the scratch, a fence waited for, the pixels copied to
    /// <paramref name="destination"/>, the scratch popped. Thread context.
    /// </summary>
    /// <param name="image">The surface to read.</param>
    /// <param name="rect">The rectangle, in pixels.</param>
    /// <param name="destination">Receives the pixels, row-major; at least the rectangle's area long.</param>
    /// <returns>False when the rectangle is empty or the destination too short; nothing is read then.</returns>
    internal bool PresentToImage(SVGA3dSurfaceImageId image, SVGA3dRect rect, Span<int> destination)
    {
        int pixelCount = (int)(rect.w * rect.h);
        if (rect.w == 0 || rect.h == 0 || destination.Length < pixelCount)
        {
            return false;
        }

        uint size = rect.w * rect.h * BytesPerPixel;
        uint scratch = AllocateScratch(size, out SVGAGuestPtr guest);
        EnqueueSurfaceDma(image, guest, rect, SVGA3dTransferType.SVGA3D_READ_HOST_VRAM);

        uint fence = InsertFence();
        SyncToFence(fence);

        _vram.Span.Slice((int)scratch, (int)size).CopyTo(MemoryMarshal.AsBytes(destination.Slice(0, pixelCount)));
        PopScratch();
        return true;
    }

    /// <summary>
    /// Places a single-box SURFACE_DMA in the FIFO, transferring the given
    /// surface rectangle from or to tightly packed guest memory at
    /// <paramref name="target"/>. The caller owns the synchronisation: the
    /// transfer only completes after a fence inserted behind it is reached.
    /// Thread context.
    /// </summary>
    /// <param name="image">The surface.</param>
    /// <param name="target">The guest memory.</param>
    /// <param name="rect">The rectangle of the surface.</param>
    /// <param name="transfer">The direction.</param>
    internal void EnqueueSurfaceDma(SVGA3dSurfaceImageId image, SVGAGuestPtr target, SVGA3dRect rect, SVGA3dTransferType transfer)
    {
        SVGA3dCopyBox box = new()
        {
            x = rect.x,
            y = rect.y,
            z = 0,
            w = rect.w,
            h = rect.h,
            d = 1,
            srcx = 0,
            srcy = 0,
            srcz = 0,
        };
        BeginSurfaceDma(in target, in image, transfer, in box);
    }

    // --- Render state ---

    /// <summary>SETRENDERTARGET. Thread context.</summary>
    /// <param name="cid">The context id.</param>
    /// <param name="type">The slot.</param>
    /// <param name="target">The surface image.</param>
    internal void SetRenderTarget(uint cid, SVGA3dRenderTargetType type, SVGA3dSurfaceImageId target)
    {
        SVGA3dCmdSetRenderTarget command = new()
        {
            cid = cid,
            type = type,
            target = target,
        };
        Span<byte> body = BeginCommand(FIFOCommand.SET_RENDER_TARGET, (uint)Unsafe.SizeOf<SVGA3dCmdSetRenderTarget>());
        WriteBody(body, in command);
        CommitCommand();
    }

    /// <summary>SETVIEWPORT. Thread context.</summary>
    /// <param name="cid">The context id.</param>
    /// <param name="rect">The viewport.</param>
    internal void SetViewport(uint cid, SVGA3dRect rect)
    {
        SVGA3dCmdSetViewport command = new()
        {
            cid = cid,
            rect = rect,
        };
        Span<byte> body = BeginCommand(FIFOCommand.SET_VIEWPORT, (uint)Unsafe.SizeOf<SVGA3dCmdSetViewport>());
        WriteBody(body, in command);
        CommitCommand();
    }

    /// <summary>SETZRANGE. Thread context.</summary>
    /// <param name="cid">The context id.</param>
    /// <param name="min">The near depth.</param>
    /// <param name="max">The far depth.</param>
    internal void SetDepthRange(uint cid, float min, float max)
    {
        SVGA3dCmdSetZRange command = new()
        {
            cid = cid,
            range = new SVGA3dZRange { min = min, max = max },
        };
        Span<byte> body = BeginCommand(FIFOCommand.SET_ZRANGE, (uint)Unsafe.SizeOf<SVGA3dCmdSetZRange>());
        WriteBody(body, in command);
        CommitCommand();
    }

    /// <summary>CLEAR of one rectangle. Thread context.</summary>
    /// <param name="cid">The context id.</param>
    /// <param name="flags">Which buffers to clear.</param>
    /// <param name="rect">The rectangle.</param>
    /// <param name="color">The colour, raw ARGB.</param>
    /// <param name="depth">The depth value.</param>
    /// <param name="stencil">The stencil value.</param>
    internal void Clear3D(uint cid, ClearFlags flags, SVGA3dRect rect, uint color = 0, float depth = 1f, uint stencil = 0)
    {
        SVGA3dCmdClear command = new()
        {
            cid = cid,
            flag = flags,
            color = color,
            depth = depth,
            stencil = stencil,
        };
        uint fixedBytes = (uint)Unsafe.SizeOf<SVGA3dCmdClear>();
        Span<byte> body = BeginCommand(FIFOCommand.CLEAR, fixedBytes + (uint)Unsafe.SizeOf<SVGA3dRect>());
        WriteBody(body, in command);
        WriteBody(body.Slice((int)fixedBytes), in rect);
        CommitCommand();
    }

    /// <summary>
    /// PRESENT of one rectangle of a surface onto the screen, after the
    /// previous present's fence. Thread context.
    /// </summary>
    /// <param name="image">The surface.</param>
    /// <param name="rect">The rectangle.</param>
    internal void Present(SVGA3dSurfaceImageId image, SVGA3dRect rect)
    {
        SyncToFence(_lastFence);

        SVGA3dCmdPresent command = new() { sid = image.sid };
        SVGA3dCopyRect copy = new()
        {
            x = rect.x,
            y = rect.y,
            w = rect.w,
            h = rect.h,
            srcx = 0,
            srcy = 0,
        };
        uint fixedBytes = (uint)Unsafe.SizeOf<SVGA3dCmdPresent>();
        Span<byte> body = BeginCommand(FIFOCommand.PRESENT, fixedBytes + (uint)Unsafe.SizeOf<SVGA3dCopyRect>());
        WriteBody(body, in command);
        WriteBody(body.Slice((int)fixedBytes), in copy);
        CommitCommand();

        _lastFence = InsertFence();
    }

    /// <summary>SETRENDERSTATE of several states. Thread context.</summary>
    /// <param name="cid">The context id.</param>
    /// <param name="states">The states, at least one.</param>
    internal void SetRenderState(uint cid, ReadOnlySpan<SVGA3dRenderState> states)
    {
        SVGA3dCmdSetRenderState command = new() { cid = cid };
        uint fixedBytes = (uint)Unsafe.SizeOf<SVGA3dCmdSetRenderState>();
        Span<byte> body = BeginCommand(FIFOCommand.SETRENDERSTATE, fixedBytes + (uint)(states.Length * Unsafe.SizeOf<SVGA3dRenderState>()));
        WriteBody(body, in command);
        WriteArray(body.Slice((int)fixedBytes), states);
        CommitCommand();
    }

    /// <summary>SETTEXTURESTATE of several states. Thread context.</summary>
    /// <param name="cid">The context id.</param>
    /// <param name="states">The states, at least one.</param>
    internal void SetTextureState(uint cid, ReadOnlySpan<SVGA3dTextureState> states)
    {
        SVGA3dCmdSetTextureState command = new() { cid = cid };
        uint fixedBytes = (uint)Unsafe.SizeOf<SVGA3dCmdSetTextureState>();
        Span<byte> body = BeginCommand(FIFOCommand.SETTEXTURESTATE, fixedBytes + (uint)(states.Length * Unsafe.SizeOf<SVGA3dTextureState>()));
        WriteBody(body, in command);
        WriteArray(body.Slice((int)fixedBytes), states);
        CommitCommand();
    }

    /// <summary>SETTRANSFORM of a 4x4 matrix, row-major as <see cref="Matrix4x4"/> lays it out. Thread context.</summary>
    /// <param name="cid">The context id.</param>
    /// <param name="type">Which matrix.</param>
    /// <param name="matrix">The matrix.</param>
    internal void SetTransform(uint cid, SVGA3dTransformType type, in Matrix4x4 matrix)
    {
        SVGA3dCmdSetTransform command = new()
        {
            cid = cid,
            type = type,
        };
        Span<float> floats = command.matrix;
        MemoryMarshal.Write(MemoryMarshal.AsBytes(floats), in matrix);
        Span<byte> body = BeginCommand(FIFOCommand.SETTRANSFORM, (uint)Unsafe.SizeOf<SVGA3dCmdSetTransform>());
        WriteBody(body, in command);
        CommitCommand();
    }

    /// <summary>DRAW_PRIMITIVES with the given vertex declarations and primitive ranges. Thread context.</summary>
    /// <param name="cid">The context id.</param>
    /// <param name="decls">One declaration per vertex stream.</param>
    /// <param name="ranges">The primitive ranges.</param>
    internal void DrawPrimitives(uint cid, ReadOnlySpan<SVGA3dVertexDecl> decls, ReadOnlySpan<SVGA3dPrimitiveRange> ranges)
    {
        SVGA3dCmdDrawPrimitives command = new()
        {
            cid = cid,
            numVertexDecls = (uint)decls.Length,
            numRanges = (uint)ranges.Length,
        };
        uint fixedBytes = (uint)Unsafe.SizeOf<SVGA3dCmdDrawPrimitives>();
        uint declBytes = (uint)(decls.Length * Unsafe.SizeOf<SVGA3dVertexDecl>());
        uint rangeBytes = (uint)(ranges.Length * Unsafe.SizeOf<SVGA3dPrimitiveRange>());
        Span<byte> body = BeginCommand(FIFOCommand.DRAW_PRIMITIVES, fixedBytes + declBytes + rangeBytes);
        WriteBody(body, in command);
        WriteArray(body.Slice((int)fixedBytes), decls);
        WriteArray(body.Slice((int)(fixedBytes + declBytes)), ranges);
        CommitCommand();
    }

    // --- Lights and materials ---

    /// <summary>SETLIGHTENABLED. Thread context.</summary>
    /// <param name="cid">The context id.</param>
    /// <param name="index">The light index.</param>
    /// <param name="enabled">Whether the light is on.</param>
    internal void SetLightEnable(uint cid, uint index, bool enabled)
    {
        SVGA3dCmdSetLightEnabled command = new()
        {
            cid = cid,
            index = index,
            enabled = enabled ? 1u : 0u,
        };
        Span<byte> body = BeginCommand(FIFOCommand.SETLIGHTENABLE, (uint)Unsafe.SizeOf<SVGA3dCmdSetLightEnabled>());
        WriteBody(body, in command);
        CommitCommand();
    }

    /// <summary>SETLIGHTDATA. Thread context.</summary>
    /// <param name="cid">The context id.</param>
    /// <param name="index">The light index.</param>
    /// <param name="data">The light.</param>
    internal void SetLightData(uint cid, uint index, in SVGA3dLightData data)
    {
        SVGA3dCmdSetLightData command = new()
        {
            cid = cid,
            index = index,
            data = data,
        };
        Span<byte> body = BeginCommand(FIFOCommand.SETLIGHTDATA, (uint)Unsafe.SizeOf<SVGA3dCmdSetLightData>());
        WriteBody(body, in command);
        CommitCommand();
    }

    /// <summary>SETMATERIAL. Thread context.</summary>
    /// <param name="cid">The context id.</param>
    /// <param name="face">Which faces the material applies to.</param>
    /// <param name="material">The material.</param>
    internal void SetMaterial(uint cid, Face face, in SVGA3dMaterial material)
    {
        SVGA3dCmdSetMaterial command = new()
        {
            cid = cid,
            face = face,
            material = material,
        };
        Span<byte> body = BeginCommand(FIFOCommand.SETMATERIAL, (uint)Unsafe.SizeOf<SVGA3dCmdSetMaterial>());
        WriteBody(body, in command);
        CommitCommand();
    }

    // --- Shaders ---

    /// <summary>SHADER_DEFINE with the next shader id of its type, the bytecode following the body. Thread context.</summary>
    /// <param name="cid">The context id.</param>
    /// <param name="type">Vertex or pixel.</param>
    /// <param name="bytecode">The shader's bytecode, a multiple of 4 bytes.</param>
    /// <returns>The shader id.</returns>
    internal uint DefineShader(uint cid, SVGA3dShaderType type, ReadOnlySpan<byte> bytecode)
    {
        uint shid = NextShaderId(type);
        SVGA3dCmdDefineShader command = new()
        {
            cid = cid,
            shid = shid,
            type = type,
        };
        uint fixedBytes = (uint)Unsafe.SizeOf<SVGA3dCmdDefineShader>();
        Span<byte> body = BeginCommand(FIFOCommand.SHADER_DEFINE, fixedBytes + (uint)bytecode.Length);
        WriteBody(body, in command);
        WriteArray(body.Slice((int)fixedBytes), bytecode);
        CommitCommand();
        return shid;
    }

    /// <summary>SHADER_DESTROY. Thread context.</summary>
    /// <param name="cid">The context id.</param>
    /// <param name="shid">The shader id.</param>
    /// <param name="type">Vertex or pixel.</param>
    internal void DestroyShader(uint cid, uint shid, SVGA3dShaderType type)
    {
        SVGA3dCmdDestroyShader command = new()
        {
            cid = cid,
            shid = shid,
            type = type,
        };
        Span<byte> body = BeginCommand(FIFOCommand.DESTROY_SHADER, (uint)Unsafe.SizeOf<SVGA3dCmdDestroyShader>());
        WriteBody(body, in command);
        CommitCommand();
    }

    /// <summary>SET_SHADER. Thread context.</summary>
    /// <param name="cid">The context id.</param>
    /// <param name="type">Vertex or pixel.</param>
    /// <param name="shid">The shader id.</param>
    internal void SetShader(uint cid, SVGA3dShaderType type, uint shid)
    {
        SVGA3dCmdSetShader command = new()
        {
            cid = cid,
            type = type,
            shid = shid,
        };
        Span<byte> body = BeginCommand(FIFOCommand.SET_SHADER, (uint)Unsafe.SizeOf<SVGA3dCmdSetShader>());
        WriteBody(body, in command);
        CommitCommand();
    }

    /// <summary>SET_SHADER_CONST of one register from up to 16 bytes of <paramref name="value"/>, zero padded. Thread context.</summary>
    /// <typeparam name="T">The value's type, an unmanaged struct.</typeparam>
    /// <param name="cid">The context id.</param>
    /// <param name="register">The constant register.</param>
    /// <param name="type">Vertex or pixel.</param>
    /// <param name="constantType">The element type of the register.</param>
    /// <param name="value">The value.</param>
    internal void SetShaderUniform<T>(uint cid, uint register, SVGA3dShaderType type, SVGA3dShaderConstType constantType, in T value) where T : unmanaged
    {
        SVGA3dCmdSetShaderConst command = new()
        {
            cid = cid,
            reg = register,
            type = type,
            ctype = constantType,
        };
        Span<uint> values = command.values;
        Span<byte> valueBytes = MemoryMarshal.AsBytes(values);
        valueBytes.Clear();
        T copy = value;
        ReadOnlySpan<byte> source = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref copy, 1));
        source.Slice(0, Math.Min(source.Length, ShaderConstantBytes)).CopyTo(valueBytes);
        Span<byte> body = BeginCommand(FIFOCommand.SET_SHADER_CONST, (uint)Unsafe.SizeOf<SVGA3dCmdSetShaderConst>());
        WriteBody(body, in command);
        CommitCommand();
    }

    // --- Private methods ---

    /// <summary>
    /// Begins a command: a header and a body of <paramref name="bodyBytes"/>.
    /// When the FIFO places the command contiguously the header is written
    /// in FIFO memory and the body span is the area after it; when the
    /// command would straddle MAX the header goes to the bounce buffer and
    /// the body span is the rest of it, for <see cref="CommitCommand"/> to
    /// append through the wrap. Every command ends with
    /// <see cref="CommitCommand"/> once its body is written.
    /// </summary>
    /// <returns>The body, <paramref name="bodyBytes"/> long.</returns>
    private Span<byte> BeginCommand(FIFOCommand command, uint bodyBytes)
    {
        uint headerBytes = (uint)Unsafe.SizeOf<SVGA3dCmdHeader>();
        uint commandBytes = headerBytes + bodyBytes;
        SVGA3dCmdHeader header = new()
        {
            id = (uint)command,
            size = bodyBytes,
        };

        Span<byte> area;
        if (_fifo.TryReserveFifo(commandBytes, out uint offset))
        {
            _bounceBytes = 0;
            area = _fifo.FifoBytes.Slice((int)offset, (int)commandBytes);
        }
        else
        {
            if (_bounce.Length < commandBytes)
            {
                _bounce = new byte[commandBytes];
            }

            _bounceBytes = commandBytes;
            area = _bounce.AsSpan(0, (int)commandBytes);
        }

        WriteBody(area, in header);
        return area.Slice((int)headerBytes);
    }

    /// <summary>Ends the command <see cref="BeginCommand"/> began: a bounced command is appended to the FIFO dword by dword through the wrap; a command placed in FIFO memory is already there.</summary>
    private void CommitCommand()
    {
        if (_bounceBytes == 0)
        {
            return;
        }

        _fifo.WriteToFifo(_bounce.AsSpan(0, (int)_bounceBytes));
        _bounceBytes = 0;
    }

    /// <summary>Writes a struct at the start of a command area.</summary>
    private static void WriteBody<T>(Span<byte> destination, in T value) where T : unmanaged
    {
        MemoryMarshal.Write(destination, in value);
    }

    /// <summary>Writes the bytes of a span of structs at the start of a command area.</summary>
    private static void WriteArray<T>(Span<byte> destination, ReadOnlySpan<T> items) where T : unmanaged
    {
        MemoryMarshal.AsBytes(items).CopyTo(destination);
    }

    /// <summary>The fixed part of a single-box SURFACE_DMA followed by the box.</summary>
    private void BeginSurfaceDma(in SVGAGuestPtr guest, in SVGA3dSurfaceImageId host, SVGA3dTransferType transfer, in SVGA3dCopyBox box)
    {
        SVGA3dCmdSurfaceDMA command = new()
        {
            guest = new SVGA3dGuestImage { ptr = guest, pitch = 0 },
            host = host,
            transfer = transfer,
        };
        uint fixedBytes = (uint)Unsafe.SizeOf<SVGA3dCmdSurfaceDMA>();
        Span<byte> body = BeginCommand(FIFOCommand.SURFACE_DMA, fixedBytes + (uint)Unsafe.SizeOf<SVGA3dCopyBox>());
        WriteBody(body, in command);
        WriteBody(body.Slice((int)fixedBytes), in box);
        CommitCommand();
    }

    /// <summary>A SURFACE_DMA of a whole 2D surface from or to the scratch.</summary>
    private void SurfaceDma2D(uint sid, in SVGAGuestPtr guest, SVGA3dTransferType transfer, uint width, uint height)
    {
        SVGA3dSurfaceImageId host = new() { sid = sid, face = 0, mipmap = 0 };
        SVGA3dCopyBox box = new()
        {
            x = 0,
            y = 0,
            z = 0,
            w = width,
            h = height,
            d = 1,
            srcx = 0,
            srcy = 0,
            srcz = 0,
        };
        BeginSurfaceDma(in guest, in host, transfer, in box);
    }

    /// <summary>The next shader id of a type; vertex and pixel shaders count separately.</summary>
    private uint NextShaderId(SVGA3dShaderType type)
    {
        switch (type)
        {
            case SVGA3dShaderType.SVGA3D_SHADERTYPE_VS:
                return _shaderIdVertex++;
            case SVGA3dShaderType.SVGA3D_SHADERTYPE_PS:
                return _shaderIdPixel++;
            default:
                return 0;
        }
    }

    /// <summary>
    /// Takes <paramref name="size"/> bytes, rounded up to a dword, from the
    /// scratch, as a byte offset into VRAM and as the guest pointer the host
    /// reads it by. <see cref="PopScratch"/> gives the last allocation back.
    /// </summary>
    /// <exception cref="InvalidOperationException">The scratch has too little room left.</exception>
    private uint AllocateScratch(uint size, out SVGAGuestPtr guest)
    {
        uint alignedSize = (size + 3u) & DwordMask;
        if (_scratchNext + alignedSize > _scratchEnd)
        {
            throw new InvalidOperationException($"DMA scratch request of {alignedSize} bytes exceeds the {_scratchEnd - _scratchNext} bytes left of the {_scratchEnd - _scratchStart} byte scratch; split the upload.");
        }

        guest = new SVGAGuestPtr
        {
            gmrId = FramebufferGmr,
            offset = _scratchNext,
        };
        uint offset = _scratchNext;
        _scratchNext += alignedSize;
        _lastScratchSize = alignedSize;
        return offset;
    }

    /// <summary>Gives the last scratch allocation back.</summary>
    private void PopScratch()
    {
        if (_lastScratchSize <= _scratchNext - _scratchStart)
        {
            _scratchNext -= _lastScratchSize;
        }

        _lastScratchSize = 0;
    }
}
