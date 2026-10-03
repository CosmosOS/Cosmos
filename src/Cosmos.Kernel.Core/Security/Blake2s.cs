// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Cosmos.Kernel.Core.Security;

/// <summary>
/// BLAKE2s (RFC 7693): a 32-bit hash with digests of 1 to 32 bytes and an
/// optional key of up to 32 bytes. <see cref="KernelRandom"/> hashes its
/// entropy pool with BLAKE2s-256, as Linux's random driver does.
/// </summary>
/// <remarks>
/// <para>The state lives in the struct itself, with no heap allocation, so a
/// static field or a stack local holds a whole hash in progress.</para>
/// <para>Pure arithmetic with no kernel dependency, so the same file compiles
/// and is known-answer tested on a desktop. The span overloads check their
/// arguments; the pointer overloads check nothing and never throw, which is
/// what the generator calls while it holds its spin lock.</para>
/// </remarks>
internal unsafe struct Blake2s
{
    /// <summary>Largest digest, in bytes; also the size the generator uses.</summary>
    public const int HashSize = 32;

    /// <summary>Largest key, in bytes.</summary>
    public const int KeySize = 32;

    /// <summary>Size of one compressed block, in bytes.</summary>
    public const int BlockSize = 64;

    // The initialization vector, shared with SHA-256.
    private const uint Iv0 = 0x6A09E667;
    private const uint Iv1 = 0xBB67AE85;
    private const uint Iv2 = 0x3C6EF372;
    private const uint Iv3 = 0xA54FF53A;
    private const uint Iv4 = 0x510E527F;
    private const uint Iv5 = 0x9B05688C;
    private const uint Iv6 = 0x1F83D9AB;
    private const uint Iv7 = 0x5BE0CD19;

    /// <summary>Chained hash value h[0..7].</summary>
    private fixed uint _h[8];

    /// <summary>The block not compressed yet: the last block is only compressed by <see cref="Finish(byte*)"/>.</summary>
    private fixed byte _buffer[BlockSize];

    /// <summary>Bytes compressed so far, the counter t of the RFC.</summary>
    private ulong _counter;

    /// <summary>Bytes held in <see cref="_buffer"/>.</summary>
    private int _bufferLength;

    /// <summary>Digest size chosen at initialization.</summary>
    private int _hashSize;

    /// <summary>
    /// The message word schedule of each of the ten rounds (RFC 7693 section 2.7).
    /// Backed by RVA data: no allocation and no static constructor.
    /// </summary>
    private static ReadOnlySpan<byte> Sigma =>
    [
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
        14, 10, 4, 8, 9, 15, 13, 6, 1, 12, 0, 2, 11, 7, 5, 3,
        11, 8, 12, 0, 5, 2, 15, 13, 10, 14, 3, 6, 7, 1, 9, 4,
        7, 9, 3, 1, 13, 12, 11, 14, 2, 6, 5, 10, 4, 0, 15, 8,
        9, 0, 5, 7, 2, 4, 10, 15, 14, 1, 11, 12, 6, 8, 3, 13,
        2, 12, 6, 10, 0, 11, 8, 3, 4, 13, 7, 5, 15, 14, 1, 9,
        12, 5, 1, 15, 14, 13, 4, 10, 0, 7, 6, 3, 9, 2, 8, 11,
        13, 11, 7, 14, 12, 1, 3, 9, 5, 0, 15, 4, 8, 6, 2, 10,
        6, 15, 14, 9, 11, 3, 0, 8, 12, 2, 13, 7, 1, 4, 10, 5,
        10, 2, 8, 4, 7, 6, 1, 5, 15, 11, 9, 14, 3, 12, 13, 0,
    ];

    /// <summary>
    /// One-shot hash: writes the digest of <paramref name="data"/>, keyed with
    /// <paramref name="key"/> when it is not empty, into
    /// <paramref name="hash"/>, whose length (1 to 32) is the digest size.
    /// </summary>
    public static void Hash(ReadOnlySpan<byte> key, ReadOnlySpan<byte> data, Span<byte> hash)
    {
        Blake2s state = default;
        state.Initialize(hash.Length, key);
        state.Update(data);
        state.Finish(hash);
    }

    /// <summary>Starts an unkeyed hash with a digest of <paramref name="hashSize"/> bytes (1 to 32).</summary>
    public void Initialize(int hashSize)
    {
        Initialize(hashSize, ReadOnlySpan<byte>.Empty);
    }

    /// <summary>
    /// Starts a hash with a digest of <paramref name="hashSize"/> bytes (1 to
    /// 32), keyed with <paramref name="key"/> (0 to 32 bytes; empty means
    /// unkeyed).
    /// </summary>
    public void Initialize(int hashSize, ReadOnlySpan<byte> key)
    {
        if (hashSize < 1 || hashSize > HashSize)
        {
            throw new ArgumentOutOfRangeException(nameof(hashSize), "BLAKE2s digests are 1 to 32 bytes.");
        }

        if (key.Length > KeySize)
        {
            throw new ArgumentException("BLAKE2s keys are at most 32 bytes.", nameof(key));
        }

        fixed (byte* k = key)
        {
            Initialize(hashSize, k, key.Length);
        }
    }

    /// <summary>Absorbs <paramref name="data"/>.</summary>
    public void Update(ReadOnlySpan<byte> data)
    {
        fixed (byte* d = data)
        {
            Update(d, data.Length);
        }
    }

    /// <summary>
    /// Writes the digest into the first bytes of <paramref name="hash"/>, as
    /// many as the digest size given to <see cref="Initialize(int, ReadOnlySpan{byte})"/>,
    /// then wipes the state.
    /// </summary>
    public void Finish(Span<byte> hash)
    {
        if (_hashSize == 0)
        {
            throw new InvalidOperationException("The hash was not initialized.");
        }

        if (hash.Length < _hashSize)
        {
            throw new ArgumentException("The destination is shorter than the digest.", nameof(hash));
        }

        fixed (byte* h = hash)
        {
            Finish(h);
        }
    }

    /// <summary>
    /// Unchecked <see cref="Initialize(int, ReadOnlySpan{byte})"/>: the caller
    /// guarantees 1 &lt;= <paramref name="hashSize"/> &lt;= 32 and
    /// 0 &lt;= <paramref name="keyLength"/> &lt;= 32.
    /// </summary>
    internal void Initialize(int hashSize, byte* key, int keyLength)
    {
        // Parameter block: digest length, key length, fanout 1, depth 1.
        _h[0] = Iv0 ^ 0x01010000u ^ ((uint)keyLength << 8) ^ (uint)hashSize;
        _h[1] = Iv1;
        _h[2] = Iv2;
        _h[3] = Iv3;
        _h[4] = Iv4;
        _h[5] = Iv5;
        _h[6] = Iv6;
        _h[7] = Iv7;
        _counter = 0;
        _bufferLength = 0;
        _hashSize = hashSize;

        for (int i = 0; i < BlockSize; i++)
        {
            _buffer[i] = 0;
        }

        // A key is absorbed as a first block of its own, zero padded.
        if (keyLength > 0)
        {
            for (int i = 0; i < keyLength; i++)
            {
                _buffer[i] = key[i];
            }

            _bufferLength = BlockSize;
        }
    }

    /// <summary>Unchecked <see cref="Update(ReadOnlySpan{byte})"/>.</summary>
    internal void Update(byte* data, int length)
    {
        while (length > 0)
        {
            if (_bufferLength == BlockSize)
            {
                // More input follows, so the buffered block is not the last.
                _counter += BlockSize;
                Compress(false);
                _bufferLength = 0;
            }

            int take = BlockSize - _bufferLength;
            if (take > length)
            {
                take = length;
            }

            for (int i = 0; i < take; i++)
            {
                _buffer[_bufferLength + i] = data[i];
            }

            _bufferLength += take;
            data += take;
            length -= take;
        }
    }

    /// <summary>
    /// Unchecked <see cref="Finish(Span{byte})"/>: writes the digest size given
    /// to <see cref="Initialize(int, byte*, int)"/> in bytes, then wipes the state.
    /// </summary>
    internal void Finish(byte* hash)
    {
        _counter += (ulong)_bufferLength;
        for (int i = _bufferLength; i < BlockSize; i++)
        {
            _buffer[i] = 0;
        }

        Compress(true);

        for (int i = 0; i < _hashSize; i++)
        {
            hash[i] = (byte)(_h[i >> 2] >> (8 * (i & 3)));
        }

        Clear();
    }

    /// <summary>Overwrites the whole state, chained value and buffered input, with zeros.</summary>
    public void Clear()
    {
        CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(new Span<Blake2s>(ref this)));
    }

    /// <summary>The compression function F (RFC 7693 section 3.2) over the buffered block.</summary>
    private void Compress(bool last)
    {
        uint* m = stackalloc uint[16];
        uint* v = stackalloc uint[16];

        for (int i = 0; i < 16; i++)
        {
            int at = i * 4;
            m[i] = _buffer[at] | ((uint)_buffer[at + 1] << 8) | ((uint)_buffer[at + 2] << 16) | ((uint)_buffer[at + 3] << 24);
        }

        for (int i = 0; i < 8; i++)
        {
            v[i] = _h[i];
        }

        v[8] = Iv0;
        v[9] = Iv1;
        v[10] = Iv2;
        v[11] = Iv3;
        v[12] = Iv4 ^ (uint)_counter;
        v[13] = Iv5 ^ (uint)(_counter >> 32);
        v[14] = last ? ~Iv6 : Iv6;
        v[15] = Iv7;

        fixed (byte* sigma = Sigma)
        {
            for (int round = 0; round < 10; round++)
            {
                byte* s = sigma + (round * 16);
                G(v, 0, 4, 8, 12, m[s[0]], m[s[1]]);
                G(v, 1, 5, 9, 13, m[s[2]], m[s[3]]);
                G(v, 2, 6, 10, 14, m[s[4]], m[s[5]]);
                G(v, 3, 7, 11, 15, m[s[6]], m[s[7]]);
                G(v, 0, 5, 10, 15, m[s[8]], m[s[9]]);
                G(v, 1, 6, 11, 12, m[s[10]], m[s[11]]);
                G(v, 2, 7, 8, 13, m[s[12]], m[s[13]]);
                G(v, 3, 4, 9, 14, m[s[14]], m[s[15]]);
            }
        }

        for (int i = 0; i < 8; i++)
        {
            _h[i] ^= v[i] ^ v[i + 8];
        }

        // The message words are the input, a key or a seed among them, and the
        // working vector is derived from it: neither may outlive the call.
        CryptographicOperations.ZeroMemory(new Span<byte>(m, 16 * sizeof(uint)));
        CryptographicOperations.ZeroMemory(new Span<byte>(v, 16 * sizeof(uint)));
    }

    /// <summary>The mixing function G (RFC 7693 section 3.1) with BLAKE2s's rotations 16, 12, 8 and 7.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void G(uint* v, int a, int b, int c, int d, uint x, uint y)
    {
        v[a] = v[a] + v[b] + x;
        v[d] = BitOperations.RotateRight(v[d] ^ v[a], 16);
        v[c] = v[c] + v[d];
        v[b] = BitOperations.RotateRight(v[b] ^ v[c], 12);
        v[a] = v[a] + v[b] + y;
        v[d] = BitOperations.RotateRight(v[d] ^ v[a], 8);
        v[c] = v[c] + v[d];
        v[b] = BitOperations.RotateRight(v[b] ^ v[c], 7);
    }
}
