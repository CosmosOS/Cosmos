using Cosmos.Kernel.Core.Bridge;

namespace Cosmos.Kernel.Core.Memory;

/// <summary>
/// Native SIMD imports live in Bridge/Import/SimdNative.cs.
/// </summary>
internal static unsafe class MemoryOp
{
    public static void InitializeHeap(ulong heapBase, ulong heapSize) =>
        PageAllocator.InitializeHeap((byte*)heapBase, heapSize);

    public static void* Alloc(uint size) => Heap.Heap.Alloc(size);

    public static void Free(void* ptr) => Heap.Heap.Free(ptr);


    #region MemSet

    public static void MemSet(byte* dest, byte value, int count)
    {
        if (count <= 0)
        {
            return;
        }

        if (count >= 16)
        {
            // SIMD path - broadcast byte to 32-bit value for SIMD fill
            uint fillValue = (uint)(value | (value << 8) | (value << 16) | (value << 24));
            FillWithSimd(dest, fillValue, count);
        }
        else
        {
            // Scalar path for small fills
            for (int i = 0; i < count; i++)
            {
                dest[i] = value;
            }
        }
    }

    public static void MemSet(uint* dest, uint value, int count)
    {
        if (count <= 0)
        {
            return;
        }

        if (count >= 4)
        {
            // SIMD path
            FillWithSimd((byte*)dest, value, count * sizeof(uint));
        }
        else
        {
            // Scalar path
            for (int i = 0; i < count; i++)
            {
                dest[i] = value;
            }
        }
    }

    private static void FillWithSimd(byte* dest, uint value, int count)
    {
        int offset = 0;

        // Fill 16-byte blocks using SIMD
        int blockCount = count / 16;
        if (blockCount > 0)
        {
            SimdNative.Fill16Blocks(dest, (int)value, blockCount);
            offset = blockCount * 16;
        }

        // Fill remaining bytes using scalar, preserving the 32-bit pattern phase:
        // dest[0] carries byte 0 of the pattern and SIMD blocks are 16 bytes (a whole
        // number of patterns), so byte offset & 3 selects the right pattern byte.
        // Using only the low byte here corrupted non-uniform fills (e.g. ARGB pixels)
        // whenever the byte count was not a multiple of 16.
        while (offset < count)
        {
            dest[offset] = (byte)(value >> ((offset & 3) * 8));
            offset++;
        }
    }

    #endregion

    #region MemCopy

    /// <summary>
    /// Memory copy. Does NOT handle overlapping regions - use MemMove for that.
    /// </summary>
    public static void MemCopy(byte* dest, byte* src, int count)
    {
        if (count <= 0)
        {
            return;
        }

        if (count >= 16)
        {
            // SIMD path
            CopyWithSimd(dest, src, count);
        }
        else
        {
            // Scalar path for small copies
            CopyScalar(dest, src, count);
        }
    }

    private static void CopyScalar(byte* dest, byte* src, int count)
    {
        // Use 64-bit copies where possible for better performance
        int i = 0;

        // Copy 8 bytes at a time
        while (i + 8 <= count)
        {
            *(ulong*)(dest + i) = *(ulong*)(src + i);
            i += 8;
        }

        // Copy remaining bytes
        while (i < count)
        {
            dest[i] = src[i];
            i++;
        }
    }

    private static void CopyWithSimd(byte* dest, byte* src, int count)
    {
        int offset = 0;

        // Copy 128-byte blocks in a single native call: the stub loops internally,
        // one P/Invoke per block would dominate large copies (e.g. framebuffer swaps).
        int blockCount = count / 128;
        if (blockCount > 0)
        {
            SimdNative.Copy128Blocks(dest, src, blockCount);
            offset = blockCount * 128;
        }

        // Copy 64-byte chunk
        if (count - offset >= 64)
        {
            SimdNative.Copy64(dest + offset, src + offset);
            offset += 64;
        }

        // Copy 32-byte chunk
        if (count - offset >= 32)
        {
            SimdNative.Copy32(dest + offset, src + offset);
            offset += 32;
        }

        // Copy 16-byte chunk
        if (count - offset >= 16)
        {
            SimdNative.Copy16(dest + offset, src + offset);
            offset += 16;
        }

        // Copy remaining bytes using scalar
        while (offset < count)
        {
            dest[offset] = src[offset];
            offset++;
        }
    }

    public static void MemCopy(uint* dest, uint* src, int count)
    {
        MemCopy((byte*)dest, (byte*)src, count * sizeof(uint));
    }

    /// <summary>
    /// Memory copy using non-temporal stores: the destination bypasses the cache.
    /// Use for large write-only targets that are never read back (framebuffer blits) —
    /// regular stores would evict a full frame's worth of cache every copy.
    /// Requires a 16-byte aligned destination; falls back to MemCopy otherwise.
    /// </summary>
    public static void MemCopyNonTemporal(byte* dest, byte* src, int count)
    {
        if (count < 128 || ((ulong)dest & 15) != 0)
        {
            MemCopy(dest, src, count);
            return;
        }

        int blockCount = count / 128;
        SimdNative.CopyNT128Blocks(dest, src, blockCount);

        int offset = blockCount * 128;
        if (count > offset)
        {
            MemCopy(dest + offset, src + offset, count - offset);
        }
    }

    #endregion

    #region MemMove

    /// <summary>
    /// Memory move that handles overlapping regions correctly.
    /// </summary>
    public static void MemMove(byte* dest, byte* src, int count)
    {
        if (dest == src || count == 0)
        {
            return;
        }

        // Check for overlap
        if (dest < src || dest >= src + count)
        {
            // No overlap - safe to use forward copy (with SIMD)
            MemCopy(dest, src, count);
        }
        else
        {
            // Overlap detected - use backward copy
            CopyBackward(dest, src, count);
        }
    }

    private static void CopyBackward(byte* dest, byte* src, int count)
    {
        // Use 64-bit copies where possible for better performance
        int i = count;

        // Copy 8 bytes at a time from the end
        while (i >= 8)
        {
            i -= 8;
            *(ulong*)(dest + i) = *(ulong*)(src + i);
        }

        // Copy remaining bytes
        while (i > 0)
        {
            i--;
            dest[i] = src[i];
        }
    }

    #endregion

    #region MemCmp

    public static bool MemCmp(uint* dest, uint* src, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (dest[i] != src[i])
            {
                return false;
            }
        }

        return true;
    }

    #endregion
}
