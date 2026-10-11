// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Security.Cryptography;

namespace Cosmos.Kernel.Core.Security;

/// <summary>
/// ChaCha20 output with fast key erasure, the construction of Linux's crng
/// and OpenBSD's arc4random. Every request runs the keystream of the current
/// key: the first 32 bytes of its first block become the next key and are
/// never output, and the request's output is the rest of that keystream. A
/// key therefore serves exactly one request and is gone once the request has
/// its keystream, so capturing the generator state later reveals nothing
/// already handed out.
/// </summary>
/// <remarks>
/// <para>A request is split in two so that a shared key needs a lock only for
/// a moment: <see cref="Ratchet"/> runs one block with the lock held, and
/// <see cref="Expand"/> produces the output afterwards from a private copy of
/// the retired key. The nonce is always zero and the counter starts at 0 for
/// each key; since no key is ever used for two requests, no (key, counter)
/// pair repeats.</para>
/// <para>Pure arithmetic with no kernel dependency, so the same file compiles
/// and is tested on a desktop. Neither method checks its arguments or
/// throws.</para>
/// </remarks>
internal static unsafe class FastKeyErasure
{
    /// <summary>Output bytes the first block carries after the next key.</summary>
    public const int FirstBlockOutput = ChaCha20.BlockSize - ChaCha20.KeySize;

    /// <summary>
    /// Retires <paramref name="key"/>: copies it to <paramref name="requestKey"/>,
    /// writes its block 0 to <paramref name="firstBlock"/>, and replaces it with
    /// the first 32 bytes of that block. Pure arithmetic over fixed-size
    /// buffers, safe to run with a spin lock held.
    /// </summary>
    /// <param name="key">The shared 32-byte key; holds the next key on return.</param>
    /// <param name="requestKey">Receives the retired key (32 bytes).</param>
    /// <param name="firstBlock">Receives block 0 of the retired key (64 bytes).</param>
    internal static void Ratchet(byte* key, byte* requestKey, byte* firstBlock)
    {
        byte* nonce = stackalloc byte[ChaCha20.NonceSize];
        for (int i = 0; i < ChaCha20.NonceSize; i++)
        {
            nonce[i] = 0;
        }

        for (int i = 0; i < ChaCha20.KeySize; i++)
        {
            requestKey[i] = key[i];
        }

        ChaCha20.Block(requestKey, 0, nonce, firstBlock);

        for (int i = 0; i < ChaCha20.KeySize; i++)
        {
            key[i] = firstBlock[i];
        }
    }

    /// <summary>
    /// Writes a request's output: the 32 bytes of <paramref name="firstBlock"/>
    /// after the next key, then the retired key's keystream from block 1 on.
    /// Wipes <paramref name="requestKey"/> and <paramref name="firstBlock"/>
    /// before returning.
    /// </summary>
    /// <param name="requestKey">The retired key from <see cref="Ratchet"/>.</param>
    /// <param name="firstBlock">The block from <see cref="Ratchet"/>.</param>
    /// <param name="output">Receives <paramref name="length"/> bytes.</param>
    /// <param name="length">Output size, 0 or more.</param>
    internal static void Expand(byte* requestKey, byte* firstBlock, byte* output, int length)
    {
        int head = length < FirstBlockOutput ? length : FirstBlockOutput;
        for (int i = 0; i < head; i++)
        {
            output[i] = firstBlock[ChaCha20.KeySize + i];
        }

        if (length > head)
        {
            byte* nonce = stackalloc byte[ChaCha20.NonceSize];
            for (int i = 0; i < ChaCha20.NonceSize; i++)
            {
                nonce[i] = 0;
            }

            // Block 0 went to the next key and the head; the rest starts at
            // block 1. An int length stays far below the 2^32 blocks the
            // 32-bit counter could address.
            ChaCha20.Keystream(requestKey, 1, nonce, output + head, length - head);
        }

        CryptographicOperations.ZeroMemory(new Span<byte>(requestKey, ChaCha20.KeySize));
        CryptographicOperations.ZeroMemory(new Span<byte>(firstBlock, ChaCha20.BlockSize));
    }
}
