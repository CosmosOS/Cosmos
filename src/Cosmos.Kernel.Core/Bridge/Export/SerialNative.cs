using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.Core.Bridge;

/// <summary>
/// Bridge functions for C library code (ACPI, libc stubs) to call C# methods.
/// NOTE: These are NOT called from C bootstrap (kmain.c) - only from library code.
/// C bootstrap uses pure C implementations for clean architecture.
/// </summary>
internal static unsafe class SerialNative
{
    /// <summary>
    /// Initialize serial port (COM1 at 115200 baud, 8N1)
    /// Must be called before any serial output
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "__cosmos_serial_init")]
    public static void Init()
    {
        Serial.ComInit();
    }

    /// <summary>
    /// Write string to serial port for C library code logging
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "__cosmos_serial_write")]
    public static void Write(byte* str)
    {
        if (str == null)
        {
            return;
        }

        // C strings are null-terminated, write char by char
        // Use while loop with explicit pointer arithmetic to avoid potential codegen issues
        byte* p = str;
        while (*p != 0)
        {
            EarlyGop.PutChar((char)*p); // Echo to screen for early debugging
            Serial.ComWrite(*p);
            p++;
        }
    }

    /// <summary>
    /// Write a 32-bit value as hex to serial port
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "__cosmos_serial_write_hex_u32")]
    public static void WriteHexU32(uint value)
    {
        Serial.WriteHexWithPrefix(value);
    }

    /// <summary>
    /// Write a 64-bit value as hex to serial port
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "__cosmos_serial_write_hex_u64")]
    public static void WriteHexU64(ulong value)
    {
        Serial.WriteHexWithPrefix(value);
    }

    /// <summary>
    /// Write a 32-bit value as decimal to serial port
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "__cosmos_serial_write_dec_u32")]
    public static void WriteDecU32(uint value)
    {
        Serial.WriteNumber(value);
    }

    /// <summary>
    /// Write a 64-bit value as decimal to serial port
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "__cosmos_serial_write_dec_u64")]
    public static void WriteDecU64(ulong value)
    {
        Serial.WriteNumber(value);
    }
}
