// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Cosmos.Kernel.Core.Security;

/// <summary>
/// The ChaCha20 block function of RFC 8439 section 2.3, and the keystream it
/// produces (section 2.4). It is the output generator of
/// <see cref="KernelRandom"/>.
/// </summary>
/// <remarks>
/// Pure arithmetic with no kernel dependency, so the same file compiles and is
/// known-answer tested on a desktop. The span overloads check their arguments;
/// the pointer overloads check nothing and never throw, which is what the
/// generator calls while it holds its spin lock.
/// </remarks>
internal static unsafe class ChaCha20
{
    /// <summary>Key size in bytes (256 bits).</summary>
    public const int KeySize = 32;

    /// <summary>Nonce size in bytes (96 bits).</summary>
    public const int NonceSize = 12;

    /// <summary>Size of one keystream block in bytes.</summary>
    public const int BlockSize = 64;

    // "expand 32-byte k" as four little-endian words.
    private const uint Constant0 = 0x61707865;
    private const uint Constant1 = 0x3320646e;
    private const uint Constant2 = 0x79622d32;
    private const uint Constant3 = 0x6b206574;

    /// <summary>
    /// Writes the 64-byte block for <paramref name="key"/>, block
    /// <paramref name="counter"/> and <paramref name="nonce"/>.
    /// </summary>
    /// <param name="key">The 32-byte key.</param>
    /// <param name="counter">The block counter.</param>
    /// <param name="nonce">The 12-byte nonce.</param>
    /// <param name="block">Receives the serialized block; at least 64 bytes.</param>
    public static void Block(ReadOnlySpan<byte> key, uint counter, ReadOnlySpan<byte> nonce, Span<byte> block)
    {
        CheckKeyAndNonce(key, nonce);
        if (block.Length < BlockSize)
        {
            throw new ArgumentException("The block needs 64 bytes.", nameof(block));
        }

        fixed (byte* k = key)
        fixed (byte* n = nonce)
        fixed (byte* b = block)
        {
            Block(k, counter, n, b);
        }
    }

    /// <summary>
    /// Fills <paramref name="output"/> with the keystream for
    /// <paramref name="key"/> and <paramref name="nonce"/>, starting at block
    /// <paramref name="counter"/>. Encryption is the XOR of this keystream
    /// with the plaintext.
    /// </summary>
    /// <param name="key">The 32-byte key.</param>
    /// <param name="counter">The counter of the first block.</param>
    /// <param name="nonce">The 12-byte nonce.</param>
    /// <param name="output">
    /// Receives the keystream; any length that does not run the 32-bit block
    /// counter past 2^32 - 1, after which the keystream would repeat.
    /// </param>
    public static void Keystream(ReadOnlySpan<byte> key, uint counter, ReadOnlySpan<byte> nonce, Span<byte> output)
    {
        CheckKeyAndNonce(key, nonce);
        ulong blocks = ((ulong)output.Length + BlockSize - 1) / BlockSize;
        if (counter + blocks > 1UL << 32)
        {
            throw new ArgumentException("The keystream would wrap the 32-bit block counter.", nameof(output));
        }

        fixed (byte* k = key)
        fixed (byte* n = nonce)
        fixed (byte* o = output)
        {
            Keystream(k, counter, n, o, output.Length);
        }
    }

    /// <summary>
    /// Unchecked <see cref="Keystream(ReadOnlySpan{byte}, uint, ReadOnlySpan{byte}, Span{byte})"/>.
    /// The counter is 32 bits wide: a caller must not ask for more than
    /// 2^32 - <paramref name="counter"/> blocks, which an <see langword="int"/>
    /// length can never reach from a counter of 1.
    /// </summary>
    internal static void Keystream(byte* key, uint counter, byte* nonce, byte* output, int length)
    {
        while (length >= BlockSize)
        {
            Block(key, counter, nonce, output);
            counter++;
            output += BlockSize;
            length -= BlockSize;
        }

        if (length > 0)
        {
            byte* last = stackalloc byte[BlockSize];
            Block(key, counter, nonce, last);
            for (int i = 0; i < length; i++)
            {
                output[i] = last[i];
            }

            CryptographicOperations.ZeroMemory(new Span<byte>(last, BlockSize));
        }
    }

    /// <summary>
    /// Unchecked <see cref="Block(ReadOnlySpan{byte}, uint, ReadOnlySpan{byte}, Span{byte})"/>:
    /// reads 32 key bytes and 12 nonce bytes and writes 64 block bytes.
    /// </summary>
    internal static void Block(byte* key, uint counter, byte* nonce, byte* block)
    {
        uint k0 = Load32(key), k1 = Load32(key + 4), k2 = Load32(key + 8), k3 = Load32(key + 12);
        uint k4 = Load32(key + 16), k5 = Load32(key + 20), k6 = Load32(key + 24), k7 = Load32(key + 28);
        uint n0 = Load32(nonce), n1 = Load32(nonce + 4), n2 = Load32(nonce + 8);

        uint x0 = Constant0, x1 = Constant1, x2 = Constant2, x3 = Constant3;
        uint x4 = k0, x5 = k1, x6 = k2, x7 = k3;
        uint x8 = k4, x9 = k5, x10 = k6, x11 = k7;
        uint x12 = counter, x13 = n0, x14 = n1, x15 = n2;

        // Ten double rounds: a column round, then a diagonal round.
        for (int i = 0; i < 10; i++)
        {
            QuarterRound(ref x0, ref x4, ref x8, ref x12);
            QuarterRound(ref x1, ref x5, ref x9, ref x13);
            QuarterRound(ref x2, ref x6, ref x10, ref x14);
            QuarterRound(ref x3, ref x7, ref x11, ref x15);

            QuarterRound(ref x0, ref x5, ref x10, ref x15);
            QuarterRound(ref x1, ref x6, ref x11, ref x12);
            QuarterRound(ref x2, ref x7, ref x8, ref x13);
            QuarterRound(ref x3, ref x4, ref x9, ref x14);
        }

        // Add the input state back in and serialize little-endian.
        Store32(block, x0 + Constant0);
        Store32(block + 4, x1 + Constant1);
        Store32(block + 8, x2 + Constant2);
        Store32(block + 12, x3 + Constant3);
        Store32(block + 16, x4 + k0);
        Store32(block + 20, x5 + k1);
        Store32(block + 24, x6 + k2);
        Store32(block + 28, x7 + k3);
        Store32(block + 32, x8 + k4);
        Store32(block + 36, x9 + k5);
        Store32(block + 40, x10 + k6);
        Store32(block + 44, x11 + k7);
        Store32(block + 48, x12 + counter);
        Store32(block + 52, x13 + n0);
        Store32(block + 56, x14 + n1);
        Store32(block + 60, x15 + n2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void QuarterRound(ref uint a, ref uint b, ref uint c, ref uint d)
    {
        a += b;
        d = BitOperations.RotateLeft(d ^ a, 16);
        c += d;
        b = BitOperations.RotateLeft(b ^ c, 12);
        a += b;
        d = BitOperations.RotateLeft(d ^ a, 8);
        c += d;
        b = BitOperations.RotateLeft(b ^ c, 7);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Load32(byte* p)
    {
        return p[0] | ((uint)p[1] << 8) | ((uint)p[2] << 16) | ((uint)p[3] << 24);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Store32(byte* p, uint value)
    {
        p[0] = (byte)value;
        p[1] = (byte)(value >> 8);
        p[2] = (byte)(value >> 16);
        p[3] = (byte)(value >> 24);
    }

    private static void CheckKeyAndNonce(ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce)
    {
        if (key.Length != KeySize)
        {
            throw new ArgumentException("The key must be 32 bytes.", nameof(key));
        }

        if (nonce.Length != NonceSize)
        {
            throw new ArgumentException("The nonce must be 12 bytes.", nameof(nonce));
        }
    }
}
