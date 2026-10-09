// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime;
using System.Runtime.CompilerServices;

namespace Cosmos.Kernel.Core;

/// <summary>
/// Native low-level hardware access primitives.
/// Architecture-specific implementations for I/O operations.
/// </summary>
internal static class Native
{
    /// <summary>
    /// x86-64 Port I/O operations.
    /// Only available on x86-64 architecture (uses in/out instructions).
    /// </summary>
    public static class IO
    {
#if ARCH_X64
        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "_native_io_write_byte")]
        public static extern void Write8(ushort Port, byte Value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "_native_io_write_word")]
        public static extern void Write16(ushort Port, ushort Value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "_native_io_write_dword")]
        public static extern void Write32(ushort Port, uint Value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "_native_io_read_byte")]
        public static extern byte Read8(ushort Port);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "_native_io_read_word")]
        public static extern ushort Read16(ushort Port);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "_native_io_read_dword")]
        public static extern uint Read32(ushort Port);
#else
        // ARM64 does not support port I/O - these are compile-time errors to prevent misuse
        public static void Write8(ushort Port, byte Value) => throw new System.PlatformNotSupportedException("Port I/O not available on ARM64");
        public static void Write16(ushort Port, ushort Value) => throw new System.PlatformNotSupportedException("Port I/O not available on ARM64");
        public static void Write32(ushort Port, uint Value) => throw new System.PlatformNotSupportedException("Port I/O not available on ARM64");
        public static byte Read8(ushort Port) => throw new System.PlatformNotSupportedException("Port I/O not available on ARM64");
        public static ushort Read16(ushort Port) => throw new System.PlatformNotSupportedException("Port I/O not available on ARM64");
        public static uint Read32(ushort Port) => throw new System.PlatformNotSupportedException("Port I/O not available on ARM64");
#endif
    }

    /// <summary>
    /// Memory-Mapped I/O operations.
    /// Available on all architectures for accessing hardware via memory addresses.
    /// NoInlining prevents compiler from reordering these operations.
    /// </summary>
    public static unsafe class MMIO
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write8(ulong Address, byte Value)
        {
            *(byte*)Address = Value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write16(ulong Address, ushort Value)
        {
            *(ushort*)Address = Value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write32(ulong Address, uint Value)
        {
            *(uint*)Address = Value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write64(ulong Address, ulong Value)
        {
            *(ulong*)Address = Value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static byte Read8(ulong Address)
        {
            return *(byte*)Address;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ushort Read16(ulong Address)
        {
            return *(ushort*)Address;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static uint Read32(ulong Address)
        {
            return *(uint*)Address;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ulong Read64(ulong Address)
        {
            return *(ulong*)Address;
        }
    }

    public static class Debug
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "_native_debug_breakpoint")]
        public static extern void Breakpoint();

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "_native_debug_breakpoint_soft")]
        public static extern void BreakpointSoft();
    }
}
