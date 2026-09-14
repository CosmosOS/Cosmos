using System;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Memory.Heap;

namespace Cosmos.Kernel.Core.Utilities;

/// <summary>
/// Utility class that expose helpers to parse cmdlines.
/// </summary>
internal static unsafe class ArgvParser
{
    private const byte DoubleQuote = (byte)'"';
    private const byte Backslash = (byte)'\\';
    private static readonly byte[] s_cosmosBytes = [
        (byte)'c', (byte)'o', (byte)'s', (byte)'m', (byte)'o', (byte)'s', 0
    ];

    private static byte* CosmosPtr
    {
        get
        {
            fixed (byte* p = s_cosmosBytes)
            {
                return p;
            }
        }
    }

    /// <summary>
    /// Utility to parse byte* into argv style.
    /// </summary>
    /// <param name="input">Input pointer to be parsed</param>
    /// <param name="argc">Pointer to Save the number of parameters</param>
    /// <returns>The argv pointer</returns>
    /// <remarks>The string "cosmos" is added as the parameter at index 0 in result, this is to take place of the exe.</remarks>
    public static byte** BuildArgv(byte* input, int* argc)
    {
        byte** result;

        // If there is no input then return default argv
        if (input == null)
        {
            result = NewArgb(0); //Only the default argument (the exe)
            *argc = 1;
            return result;
        }

        // Count input length
        uint len = 0;
        while (input[len] != 0)
        {
            len++;
        }

        if (len == 0)
        {
            result = NewArgb(0); //Only the default argument (the exe)
            *argc = 1;
            return result;
        }

        byte* argvBuffer = Heap.Alloc(len + 1);

        MemoryOp.MemCopy(argvBuffer, input, (int)(len + 1));

        uint count = PrepareBuffer(new(argvBuffer, (int)(len + 1)));

        result = NewArgb(count);

        int argcount = 1;

        Span<byte> buffer = new(argvBuffer, (int)(1 + len));
        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i] == 0)
            {
                continue;
            }

            result[argcount++] = argvBuffer + i;

            while (buffer[i] != 0)
            {
                i++;
            }
        }

        result[argcount] = null;

        *argc = argcount;

        return result;
    }

    /// <summary>
    /// Replace Spaces for Nulls and Count Number of parameters.
    /// </summary>
    /// <param name="buffer">Argv Buffer</param>
    /// <returns>Number of parameters found in the buffer</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint PrepareBuffer(Span<byte> buffer)
    {
        uint count = 0;

        for (int i = 0; i < buffer.Length; i++)
        {
            while (buffer[i] != 0 && char.IsWhiteSpace((char)buffer[i]))
            {
                buffer[i] = 0;
                i++;
            }

            if (buffer[i] == 0)
            {
                break;
            }

            if (buffer[i] == DoubleQuote)
            {
                // Move buffer one position down
                Span<byte> slice = buffer.Slice(i + 1);
                slice.CopyTo(buffer.Slice(i));

                while (buffer[i] != 0)
                {
                    if (buffer[i] == DoubleQuote && buffer[i] != DoubleQuote)
                    {
                        slice = buffer.Slice(i + 1);
                        slice.CopyTo(buffer.Slice(i));
                        i++;
                        break;
                    }
                    else if (buffer[i] == DoubleQuote && buffer[i] == DoubleQuote
                        || buffer[i] == Backslash && buffer[i] == DoubleQuote
                        || buffer[i] == Backslash && buffer[i] == Backslash)
                    {
                        // Remove starting '\' or '"'
                        slice = buffer.Slice(i + 1);
                        slice.CopyTo(buffer.Slice(i));
                        i++;
                    }
                    else
                    {
                        i++;
                    }
                }
                count++;
            }
            else
            {
                while (buffer[i] != 0 && !char.IsWhiteSpace((char)buffer[i]))
                {
                    i++;
                }
                i--;
                count++;
            }
        }

        return count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte** NewArgb(uint length)
    {
        byte** argv = (byte**)Heap.Alloc((length + 2) * (uint)sizeof(byte*));

        argv[0] = CosmosPtr;

        return argv;
    }
}
