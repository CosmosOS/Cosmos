using System;
using Cosmos.Kernel.Core.Memory;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

/// <summary>
/// SVGA3D command layer on top of <see cref="SvgaIIDriver"/>: surfaces,
/// contexts, shaders, render state and DMA transfers, all submitted through
/// the driver's command FIFO. Only meaningful when the device negotiated 3D
/// support (<see cref="SvgaIIDriver.Is3DEnabled"/>) — QEMU's vmware-svga
/// exposes no 3D capability, so this layer is only exercised on real VMware.
/// </summary>
internal unsafe class VMWareSVGAII3D
{
    private readonly SvgaIIDriver _driver;

    public bool Is3DEnabled => _driver.Is3DEnabled;
    public uint HW3DVer => _driver.HW3DVer;

    public VMWareSVGAII3D(SvgaIIDriver driver)
    {
        _driver = driver;

        s_dmaSize = _driver.VideoMemory.Size / 8;
        uint dmaStartOffset = (_driver.VideoMemory.Size - s_dmaSize) & ~3u;

        s_dmaStart = new SVGAGuestPtr { gmrId = SVGA_GMR_FRAMEBUFFER, offset = dmaStartOffset };
        s_nextPtr.offset = s_dmaStart.offset;
    }

    private uint _contextId;

    private uint GetNextContextId() => ++_contextId;


    private uint _surfaceId;

    private uint GetNextSurfaceId() => ++_surfaceId;

    private bool _fifoFenceSupported;
    private uint _guestFenceCounter = 1;

    public uint _lastDMASize = 0;

    private int[] _imagebuffer = [];

    private void SyncToFence(uint fence)
    {
        if (_fifoFenceSupported)
        {
            while (_driver.ReadFifo3D(Register3D.SVGA_FIFO_FENCE) < fence) { }
        }
        else
        {
            _driver.WriteRegister(Register.Sync, 1);
            while (_driver.ReadRegister(Register.Busy) != 0) { }
        }
    }

    private uint InsertFence()
    {
        uint fence = ++_guestFenceCounter;

        if (_fifoFenceSupported)
        {
            _driver.WriteFifo3D(Register3D.SVGA_FIFO_FENCE, fence);
        }
        else
        {
            _driver.WriteRegister(Register.Sync, fence);
        }

        return fence;
    }

    public void* ReserveFIFO3D(uint cmd, uint cmdSize)
    {
        SVGA3dCmdHeader* header;

        header = (SVGA3dCmdHeader*)_driver.ReserveFIFO((uint)sizeof(SVGA3dCmdHeader) + cmdSize);
        header->id = cmd;
        header->size = cmdSize;

        return &header[1];
    }

    private void BeginSurfaceDefinition(
        uint sid,
        SVGA3dSurfaceFlags flags,
        SVGA3dSurfaceFormat format,
        ref uint* faces,
        out SVGA3dSize* mipSizes,
        uint numMipSizes)
    {
        SVGA3dCmdDefineSurface* cmd = (SVGA3dCmdDefineSurface*)ReserveFIFO3D(
            (uint)FIFOCommand.DEFINE_SURFACE,
            (uint)sizeof(SVGA3dCmdDefineSurface) + (uint)(numMipSizes * sizeof(SVGA3dSize)));

        cmd->sid = sid;
        cmd->flags = flags;
        cmd->format = format;

        faces = &cmd->face[0];
        mipSizes = (SVGA3dSize*)&cmd[1];

        MemoryOp.MemSet((byte*)faces, 0, sizeof(uint) * 6);
        MemoryOp.MemSet((byte*)mipSizes, 0, sizeof(SVGA3dSize) * (int)numMipSizes);
    }


    public SVGA3dSurfaceImageId DefineSurface(uint width, uint height, SVGA3dSurfaceFormat format)
    {
        uint sid = GetNextSurfaceId();
        SVGA3dSize* mipSizes;
        uint* faces = null;

        BeginSurfaceDefinition(sid, 0, format, ref faces, out mipSizes, 1);

        faces[0] = 1;
        mipSizes[0].width = width;
        mipSizes[0].height = height;
        mipSizes[0].depth = 1;

        return new() { sid = sid, face = 0, mipmap = 0 };
    }

    public void SetRenderTarget(uint cid, SVGA3dRenderTargetType type, SVGA3dSurfaceImageId target)
    {
        SVGA3dCmdSetRenderTarget* cmd;
        cmd = (SVGA3dCmdSetRenderTarget*)ReserveFIFO3D((uint)FIFOCommand.SET_RENDER_TARGET, (uint)sizeof(SVGA3dCmdSetRenderTarget));
        cmd->cid = cid;
        cmd->type = type;
        cmd->target = target;
    }

    public void SetViewport(uint cid, SVGA3dRect rect)
    {
        SVGA3dCmdSetViewport* cmd;
        cmd = (SVGA3dCmdSetViewport*)ReserveFIFO3D((uint)FIFOCommand.SET_VIEWPORT, (uint)sizeof(SVGA3dCmdSetViewport));
        cmd->cid = cid;
        cmd->rect = rect;
    }

    public void SetDepthRange(uint cid, float min, float max)
    {
        SVGA3dCmdSetZRange* cmd;
        cmd = (SVGA3dCmdSetZRange*)ReserveFIFO3D((uint)FIFOCommand.SET_ZRANGE, (uint)sizeof(SVGA3dCmdSetZRange));
        cmd->cid = cid;
        cmd->range.min = min;
        cmd->range.max = max;
    }

    private void BeginClear3D(
        uint cid,
        ClearFlags flags,
        uint color,
        float depth,
        uint stencil,
        SVGA3dRect** rects,
        uint numRects)
    {
        SVGA3dCmdClear* cmd;
        cmd = (SVGA3dCmdClear*)ReserveFIFO3D((uint)FIFOCommand.CLEAR, (uint)sizeof(SVGA3dCmdClear) + (uint)(numRects * sizeof(SVGA3dRect)));

        cmd->cid = cid;
        cmd->flag = flags;
        cmd->color = color;
        cmd->depth = depth;
        cmd->stencil = stencil;
        *rects = (SVGA3dRect*)&cmd[1];
    }

    public void Clear3D(uint cid, ClearFlags flags, SVGA3dRect ClearRect, uint color = 0, float depth = 1, uint stencil = 0)
    {
        SVGA3dRect* rect;

        BeginClear3D(cid, flags, color, depth, stencil, &rect, 1);
        rect->x = ClearRect.x;
        rect->y = ClearRect.y;
        rect->w = ClearRect.w;
        rect->h = ClearRect.h;
    }

    private void BeginPresent(uint sid, SVGA3dCopyRect** rects, uint numRects)
    {
        SVGA3dCmdPresent* cmd;
        cmd = (SVGA3dCmdPresent*)ReserveFIFO3D((uint)FIFOCommand.PRESENT, (uint)sizeof(SVGA3dCmdPresent) + (uint)(numRects * sizeof(SVGA3dCopyRect)));
        cmd->sid = sid;
        *rects = (SVGA3dCopyRect*)&cmd[1];
    }

    private uint _lastFence = 1;

    public void Present(SVGA3dSurfaceImageId image, SVGA3dRect PresentRect)
    {
        SVGA3dCopyRect* rect;

        SyncToFence(_lastFence);

        BeginPresent(image.sid, &rect, 1);
        MemoryOp.MemSet((byte*)rect, 0, sizeof(SVGA3dCopyRect));
        rect->x = PresentRect.x;
        rect->y = PresentRect.y;
        rect->w = PresentRect.w;
        rect->h = PresentRect.h;

        _lastFence = InsertFence();
    }

    public int[]? PresentToImage(SVGA3dSurfaceImageId image, SVGA3dRect rect)
    {
        uint width = rect.w;
        uint height = rect.h;

        if (width == 0 || height == 0)
        {
            return null;
        }

        const uint bytesPerPixel = 4u;
        uint size = width * height * bytesPerPixel;

        void* buffer = SVGA3DUtil_AllocDMABuffer(size, out SVGAGuestPtr gPtr);

        EnqueueSurfaceDma(image, gPtr, rect, SVGA3dTransferType.SVGA3D_READ_HOST_VRAM);

        uint fence = InsertFence();
        SyncToFence(fence);

        int pixelCount = (int)(width * height);
        if (_imagebuffer.Length != pixelCount)
        {
            _imagebuffer = new int[pixelCount];
        }

        fixed (int* pDest = &_imagebuffer[0])
        {
            MemoryOp.MemCopy((byte*)pDest, (byte*)buffer, (int)size);
        }

        PopDMABuffer();

        return _imagebuffer;
    }


    /// <summary>
    /// Places a single-box SURFACE_DMA command in the FIFO, transferring the
    /// given surface rectangle from or to tightly packed guest memory at
    /// <paramref name="target"/>. The caller owns synchronization: the
    /// transfer only completes after a fence inserted behind it is reached.
    /// </summary>
    public void EnqueueSurfaceDma(SVGA3dSurfaceImageId image, SVGAGuestPtr target, SVGA3dRect rect, SVGA3dTransferType transfer)
    {
        SVGA3dGuestImage guestImage;
        guestImage.ptr = target;
        guestImage.pitch = 0;

        SVGA3dCopyBox* boxes;
        BeginSurfaceDMA(&guestImage, &image, transfer, &boxes, 1);

        boxes[0].x = rect.x;
        boxes[0].y = rect.y;
        boxes[0].w = rect.w;
        boxes[0].h = rect.h;
        boxes[0].d = 1;
    }

    private void BeginSurfaceDMA(
        SVGA3dGuestImage* guestImage,
        SVGA3dSurfaceImageId* hostImage,
        SVGA3dTransferType transfer,
        SVGA3dCopyBox** boxes,
        uint numBoxes)
    {
        SVGA3dCmdSurfaceDMA* cmd;
        uint boxesSize = (uint)sizeof(SVGA3dCopyBox) * numBoxes;

        cmd = (SVGA3dCmdSurfaceDMA*)ReserveFIFO3D((uint)FIFOCommand.SURFACE_DMA, (uint)sizeof(SVGA3dCmdSurfaceDMA) + boxesSize);

        cmd->guest = *guestImage;
        cmd->host = *hostImage;
        cmd->transfer = transfer;
        *boxes = (SVGA3dCopyBox*)&cmd[1];

        MemoryOp.MemSet((byte*)*boxes, 0, (int)boxesSize);
    }

    private void SurfaceDMA2D(
        uint sid,
        SVGAGuestPtr* guestPtr,
        SVGA3dTransferType transfer,
        uint width,
        uint height)
    {
        SVGA3dCopyBox* boxes;
        SVGA3dGuestImage guestImage;
        SVGA3dSurfaceImageId hostImage = new() { sid = sid };

        guestImage.ptr = *guestPtr;
        guestImage.pitch = 0;

        BeginSurfaceDMA(&guestImage, &hostImage, transfer, &boxes, 1);
        boxes[0].w = width;
        boxes[0].h = height;
        boxes[0].d = 1;
    }

    public uint CreateStaticArrayBuffer<T>(ReadOnlySpan<T> data) where T : unmanaged
    {
        uint size = (uint)(data.Length * sizeof(T));

        uint sid = DefineSurface(size, 1, SVGA3dSurfaceFormat.SVGA3D_BUFFER).sid;

        SVGAGuestPtr gPtr;
        void* buffer = SVGA3DUtil_AllocDMABuffer(size, out gPtr);

        fixed (T* pData = data)
        {
            MemoryOp.MemCopy((byte*)buffer, (byte*)pData, (int)size);
        }

        SurfaceDMA2D(sid, &gPtr, SVGA3dTransferType.SVGA3D_WRITE_HOST_VRAM, size, 1);

        uint fence = InsertFence();
        SyncToFence(fence);

        PopDMABuffer();

        return sid;
    }


    public uint TestDebugBuffer()
    {
        void* buffer;
        SVGAGuestPtr gPtr;
        uint sid = DefineSurface(1280, 720, SVGA3dSurfaceFormat.SVGA3D_A8R8G8B8).sid;

        buffer = SVGA3DUtil_AllocDMABuffer(1280 * 720 * 4, out gPtr);

        MemoryOp.MemSet((byte*)buffer, 0x30, 1280 * 720 * 4);

        SurfaceDMA2D(sid, &gPtr, SVGA3dTransferType.SVGA3D_WRITE_HOST_VRAM, 1280, 720);

        uint fence = InsertFence();
        SyncToFence(fence);

        PopDMABuffer();

        return sid;
    }

    public SVGA3dSurfaceImageId DefineSurfaceFromImage(int[] image, uint width, uint height)
    {
        void* buffer;
        SVGAGuestPtr gPtr;
        uint sid = DefineSurface(width, height, SVGA3dSurfaceFormat.SVGA3D_A8R8G8B8).sid;

        buffer = SVGA3DUtil_AllocDMABuffer(width * height * sizeof(int), out gPtr);

        fixed (int* rawDataPtr = image)
        {
            MemoryOp.MemCopy((byte*)buffer, (byte*)rawDataPtr, image.Length * sizeof(int));
        }

        SurfaceDMA2D(sid, &gPtr, SVGA3dTransferType.SVGA3D_WRITE_HOST_VRAM, width, height);

        uint fence = InsertFence();
        SyncToFence(fence);

        PopDMABuffer();

        return new() { sid = sid, face = 0, mipmap = 0 };
    }

    private void BeginSetRenderState(uint cid, SVGA3dRenderState** states, uint numstates)
    {
        SVGA3dCmdSetRenderState* cmd;
        cmd = (SVGA3dCmdSetRenderState*)ReserveFIFO3D((uint)FIFOCommand.SETRENDERSTATE, (uint)(sizeof(SVGA3dCmdSetRenderState) + sizeof(SVGA3dRenderState) * numstates));

        cmd->cid = cid;

        *states = (SVGA3dRenderState*)&cmd[1];
    }

    public void SetLightEnable(uint cid, uint index, bool enabled)
    {
        SVGA3dCmdSetLightEnabled* cmd;
        cmd = (SVGA3dCmdSetLightEnabled*)ReserveFIFO3D((uint)FIFOCommand.SETLIGHTENABLE, (uint)sizeof(SVGA3dCmdSetLightEnabled));
        cmd->cid = cid;
        cmd->index = index;
        cmd->enabled = enabled ? 1u : 0u;
    }

    public void SetLightData(uint cid, uint index, SVGA3dLightData data)
    {
        SVGA3dCmdSetLightData* cmd;
        cmd = (SVGA3dCmdSetLightData*)ReserveFIFO3D((uint)FIFOCommand.SETLIGHTDATA, (uint)sizeof(SVGA3dCmdSetLightData));
        cmd->cid = cid;
        cmd->index = index;

        MemoryOp.MemCopy((byte*)&cmd->data, (byte*)&data, sizeof(SVGA3dLightData));
    }

    public void DestroyContext(uint cid)
    {
        uint* cmd;
        cmd = (uint*)ReserveFIFO3D((uint)FIFOCommand.DESTROY_CONTEXT, sizeof(uint));
        *cmd = cid;
    }

    public void DestroySurface(uint sid)
    {
        uint* cmd;
        cmd = (uint*)ReserveFIFO3D((uint)FIFOCommand.DESTROY_SURFACE, sizeof(uint));
        *cmd = sid;
    }

    public void DestroyShader(uint cid, uint shid, SVGA3dShaderType type)
    {
        SVGA3dCmdDestroyShader* cmd;
        cmd = (SVGA3dCmdDestroyShader*)ReserveFIFO3D((uint)FIFOCommand.DESTROY_SHADER, (uint)sizeof(SVGA3dCmdDestroyShader));
        cmd->cid = cid;
        cmd->shid = shid;
        cmd->type = type;
    }

    public void SetMaterial(uint cid, Face face, SVGA3dMaterial material)
    {
        SVGA3dCmdSetMaterial* cmd;
        cmd = (SVGA3dCmdSetMaterial*)ReserveFIFO3D((uint)FIFOCommand.SETMATERIAL, (uint)sizeof(SVGA3dCmdSetMaterial));
        cmd->cid = cid;
        cmd->face = face;

        MemoryOp.MemCopy((byte*)&cmd->material, (byte*)&material, sizeof(SVGA3dMaterial));
    }

    public void SetRenderState(uint cid, SVGA3dRenderState[] states)
    {
        SVGA3dRenderState* rs;
        BeginSetRenderState(cid, &rs, (uint)states.Length);

        fixed (SVGA3dRenderState* statesPtr = &states[0])
        {
            MemoryOp.MemCopy((byte*)rs, (byte*)statesPtr, sizeof(SVGA3dRenderState) * states.Length);
        }
    }

    private void BeginSetTextureState(uint cid, SVGA3dTextureState** states, uint numStates)
    {
        SVGA3dCmdSetTextureState* cmd;
        cmd = (SVGA3dCmdSetTextureState*)ReserveFIFO3D((uint)FIFOCommand.SETTEXTURESTATE, (uint)(sizeof(SVGA3dCmdSetTextureState) + sizeof(SVGA3dTextureState) * numStates));
        cmd->cid = cid;

        *states = (SVGA3dTextureState*)&cmd[1];
    }

    public void SetTextureState(uint cid, SVGA3dTextureState[] states)
    {
        SVGA3dTextureState* ts;
        BeginSetTextureState(cid, &ts, (uint)states.Length);

        fixed (SVGA3dTextureState* statesPtr = &states[0])
        {
            MemoryOp.MemCopy((byte*)ts, (byte*)statesPtr, sizeof(SVGA3dTextureState) * states.Length);
        }
    }

    private void BeginDrawPrimitives(
        uint cid,
        SVGA3dVertexDecl** decls,
        uint numVertexDecls,
        SVGA3dPrimitiveRange** ranges,
        uint numRanges)
    {
        SVGA3dCmdDrawPrimitives* cmd;
        SVGA3dVertexDecl* declArray;
        SVGA3dPrimitiveRange* rangeArray;
        uint declSize = (uint)sizeof(SVGA3dVertexDecl) * numVertexDecls;
        uint rangeSize = (uint)sizeof(SVGA3dPrimitiveRange) * numRanges;

        cmd = (SVGA3dCmdDrawPrimitives*)ReserveFIFO3D((uint)FIFOCommand.DRAW_PRIMITIVES, (uint)sizeof(SVGA3dCmdDrawPrimitives) + declSize + rangeSize);

        cmd->cid = cid;
        cmd->numVertexDecls = numVertexDecls;
        cmd->numRanges = numRanges;

        declArray = (SVGA3dVertexDecl*)&cmd[1];
        rangeArray = (SVGA3dPrimitiveRange*)&declArray[numVertexDecls];

        MemoryOp.MemSet((byte*)declArray, 0, (int)declSize);
        MemoryOp.MemSet((byte*)rangeArray, 0, (int)rangeSize);

        *decls = declArray;
        *ranges = rangeArray;
    }

    public void DrawPrimitives(
        uint cid,
        SVGA3dVertexDecl[] decls,
        SVGA3dPrimitiveRange[] ranges)
    {
        SVGA3dVertexDecl* vdecls;
        SVGA3dPrimitiveRange* pranges;
        BeginDrawPrimitives(cid, &vdecls, (uint)decls.Length, &pranges, (uint)ranges.Length);

        fixed (SVGA3dVertexDecl* statesPtr = &decls[0])
        {
            MemoryOp.MemCopy((byte*)vdecls, (byte*)statesPtr, sizeof(SVGA3dVertexDecl) * decls.Length);
        }
        fixed (SVGA3dPrimitiveRange* statesPtr = &ranges[0])
        {
            MemoryOp.MemCopy((byte*)pranges, (byte*)statesPtr, sizeof(SVGA3dPrimitiveRange) * ranges.Length);
        }
    }

    private void InternalSetTransform(uint cid, SVGA3dTransformType type, float* matrix)
    {
        SVGA3dCmdSetTransform* cmd;
        cmd = (SVGA3dCmdSetTransform*)ReserveFIFO3D((uint)FIFOCommand.SETTRANSFORM, (uint)sizeof(SVGA3dCmdSetTransform));
        cmd->cid = cid;
        cmd->type = type;

        MemoryOp.MemCopy((byte*)&cmd->matrix[0], (byte*)matrix, sizeof(float) * 16);
    }

    public void SetTransform<T>(uint cid, SVGA3dTransformType type, T matrix4x4)
    {
        if (sizeof(T) == 16 * sizeof(float))
        {
            InternalSetTransform(cid, type, (float*)&matrix4x4);
        }
        else
        {
            throw new ArgumentException("Matrix must be 4x4 float");
        }
    }

    private uint _shaderIdVS = 0;
    private uint _shaderIdPS = 0;

    uint GetNextShaderId(SVGA3dShaderType type)
    {
        switch (type)
        {
            case SVGA3dShaderType.SVGA3D_SHADERTYPE_VS: return _shaderIdVS++;
            case SVGA3dShaderType.SVGA3D_SHADERTYPE_PS: return _shaderIdPS++;

            default: return 0;
        }
    }

    public uint DefineShader(uint cid, SVGA3dShaderType type, byte[] bytecode)
    {
        SVGA3dCmdDefineShader* cmd;

        cmd = (SVGA3dCmdDefineShader*)ReserveFIFO3D((uint)FIFOCommand.SHADER_DEFINE, (uint)sizeof(SVGA3dCmdDefineShader) + (uint)bytecode.Length);

        var shid = GetNextShaderId(type);

        cmd->cid = cid;
        cmd->shid = shid;
        cmd->type = type;

        fixed (byte* bytecodePtr = &bytecode[0])
        {
            MemoryOp.MemCopy((byte*)&cmd[1], bytecodePtr, bytecode.Length);
        }

        return shid;
    }

    public void SetShader(uint cid, SVGA3dShaderType type, uint shid)
    {
        SVGA3dCmdSetShader* cmd;

        cmd = (SVGA3dCmdSetShader*)ReserveFIFO3D((uint)FIFOCommand.SET_SHADER, (uint)sizeof(SVGA3dCmdSetShader));
        cmd->cid = cid;
        cmd->type = type;
        cmd->shid = shid;
    }

    public void SetShaderUniform<T>(uint cid, uint reg, SVGA3dShaderType type, SVGA3dShaderConstType ctype, T value) where T : unmanaged
    {
        SVGA3dCmdSetShaderConst* cmd;
        cmd = (SVGA3dCmdSetShaderConst*)ReserveFIFO3D((uint)FIFOCommand.SET_SHADER_CONST, (uint)sizeof(SVGA3dCmdSetShaderConst));

        cmd->cid = cid;
        cmd->reg = reg;
        cmd->type = type;
        cmd->ctype = ctype;

        cmd->values[0] = 0;
        cmd->values[1] = 0;
        cmd->values[2] = 0;
        cmd->values[3] = 0;

        byte* src = (byte*)&value;
        byte* dst = (byte*)cmd->values;

        int size = sizeof(T);
        if (size > 16)
        {
            size = 16;
        }
        for (int i = 0; i < size; i++)
        {
            dst[i] = src[i];
        }
    }


    public uint DefineContext()
    {
        uint cid = GetNextContextId();

        SVGA3dCmdDefineContext* cmd;
        cmd = (SVGA3dCmdDefineContext*)ReserveFIFO3D((uint)FIFOCommand.DEFINE_CONTEXT, (uint)sizeof(SVGA3dCmdDefineContext));
        cmd->cid = cid;

        return cid;
    }

    private static SVGAGuestPtr s_nextPtr = new SVGAGuestPtr { gmrId = 0, offset = 0 };
    private static SVGAGuestPtr s_dmaStart = new SVGAGuestPtr { gmrId = 0, offset = 0 };

    private static uint s_dmaSize = 0;
    private const uint SVGA_GMR_FRAMEBUFFER = 0xFFFFFFFEu;

    public void PopDMABuffer()
    {
        if (_lastDMASize <= (s_nextPtr.offset - s_dmaStart.offset))
        {
            s_nextPtr.offset -= _lastDMASize;
        }
        _lastDMASize = 0;
    }

    public void* SVGA3DUtil_AllocDMABuffer(uint size, out SVGAGuestPtr ptr)
    {
        uint alignedSize = (size + 3u) & ~3u;
        if ((_driver.Capabilities & (uint)Capability.Gmr) == 0)
        {
            throw new InvalidOperationException("SVGA device does not support GMR — cannot allocate framebuffer-backed guest pointer.");
        }

        if (s_nextPtr.offset + alignedSize > _driver.VideoMemory.Size)
        {
            throw new OutOfMemoryException(
                $"DMA scratch buffer request of {alignedSize} bytes exceeds remaining scratch space " +
                $"({_driver.VideoMemory.Size - s_nextPtr.offset} bytes free, region size {s_dmaSize} bytes). " +
                $"Consider splitting the upload into smaller chunks."
            );
        }

        ptr = new SVGAGuestPtr
        {
            gmrId = SVGA_GMR_FRAMEBUFFER,
            offset = s_nextPtr.offset
        };

        void* buffer = (void*)(_driver.VideoMemory.Base + s_nextPtr.offset);

        s_nextPtr.offset += alignedSize;
        _lastDMASize = alignedSize;

        return buffer;
    }
}
