// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
//
// Port of BitStreamReader from dotnet/runtime/src/coreclr/inc/gcinfodecoder.h. Reads the
// LSB-first, word-granular bit stream that ILC emits for GCInfo blobs. 64-bit only
// (BITS_PER_SIZE_T == 64 on both x64 and arm64). See issue #346.

namespace Cosmos.Kernel.Core.Memory.GarbageCollector.GcInfo;

/// <summary>
/// Reads a GCInfo bit stream. Mutable struct — pass by <c>ref</c> for sequential reads;
/// copy by value to fork an independent cursor (matches <c>BitStreamReader</c>'s copy semantics).
/// </summary>
internal unsafe struct GcInfoBitStreamReader
{
    private const int BitsPerWord = 64;

    private readonly ulong* _pBuffer;
    private readonly int _initialRelPos;
    private ulong* _pCurrent;
    private int _relPos;
    private ulong _current;

    public GcInfoBitStreamReader(byte* pBuffer)
    {
        // Align the start down to a word boundary; track the bit offset within that word.
        _pCurrent = _pBuffer = (ulong*)((nuint)pBuffer & ~(nuint)(sizeof(ulong) - 1));
        _relPos = _initialRelPos = (int)((nuint)pBuffer % sizeof(ulong)) * 8;
        // There is always at least a header, so it is safe to prefetch the first word.
        _current = *_pCurrent >> _relPos;
    }

    /// <summary>Read <paramref name="numBits"/> bits (1..64), LSB-first.</summary>
    public ulong Read(int numBits)
    {
        ulong result = _current;
        _current >>= numBits;
        int newRelPos = _relPos + numBits;
        if (newRelPos > BitsPerWord)
        {
            _pCurrent++;
            _current = *_pCurrent;
            newRelPos -= BitsPerWord;
            ulong extraBits = _current << (numBits - newRelPos);
            result |= extraBits;
            _current >>= newRelPos;
        }
        _relPos = newRelPos;
        result &= ulong.MaxValue >> (BitsPerWord - numBits);
        return result;
    }

    /// <summary>Read a single bit.</summary>
    public ulong ReadOneFast()
    {
        if (_relPos == BitsPerWord)
        {
            _pCurrent++;
            _current = *_pCurrent;
            _relPos = 0;
        }
        _relPos++;
        ulong result = _current & 1;
        _current >>= 1;
        return result;
    }

    /// <summary>Current bit position relative to the (unaligned) start of the buffer.</summary>
    public readonly ulong GetCurrentPos()
        => (ulong)(_pCurrent - _pBuffer) * BitsPerWord + (ulong)(_relPos - _initialRelPos);

    public void SetCurrentPos(ulong pos)
    {
        ulong adjPos = pos + (ulong)_initialRelPos;
        _pCurrent = _pBuffer + adjPos / BitsPerWord;
        _relPos = (int)(adjPos % BitsPerWord);
        _current = *_pCurrent >> _relPos;
    }

    public void Skip(long numBitsToSkip)
    {
        ulong adjPos = (ulong)((long)GetCurrentPos() + numBitsToSkip) + (ulong)_initialRelPos;
        _pCurrent = _pBuffer + adjPos / BitsPerWord;
        _relPos = (int)(adjPos % BitsPerWord);
        // Skipping to a word boundary may land at the edge-exclusive end of the stream;
        // do not prefetch past it.
        if (_relPos == 0)
        {
            _pCurrent--;
            _relPos = BitsPerWord;
            _current = 0;
        }
        else
        {
            _current = *_pCurrent >> _relPos;
        }
    }

    // --- Variable-length number decoding (see BitStreamWriter for the format) ---

    private ulong DecodeVarLengthUnsignedMore(int @base)
    {
        ulong numEncodings = (ulong)1 << @base;
        ulong result = numEncodings;
        for (int shift = @base; ; shift += @base)
        {
            ulong currentChunk = Read(@base + 1);
            result ^= (currentChunk & (numEncodings - 1)) << shift;
            if ((currentChunk & numEncodings) == 0)
            {
                return result;
            }
        }
    }

    public ulong DecodeVarLengthUnsigned(int @base)
    {
        ulong result = Read(@base + 1);
        if ((result & ((ulong)1 << @base)) != 0)
        {
            result ^= DecodeVarLengthUnsignedMore(@base);
        }
        return result;
    }

    public long DecodeVarLengthSigned(int @base)
    {
        ulong numEncodings = (ulong)1 << @base;
        long result = 0;
        for (int shift = 0; ; shift += @base)
        {
            ulong currentChunk = Read(@base + 1);
            result |= (long)((currentChunk & (numEncodings - 1)) << shift);
            if ((currentChunk & numEncodings) == 0)
            {
                int sbits = BitsPerWord - (shift + @base);
                result <<= sbits;
                result >>= sbits;   // arithmetic shift — sign-extends
                return result;
            }
        }
    }
}
