// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.RISCV64.Bridge;

namespace Cosmos.Kernel.Core.RISCV64.Cpu;

/// <summary>
/// Calls into the SBI firmware (RISC-V SBI specification v2.0): the M-mode
/// layer under the kernel, OpenSBI on QEMU. The kernel uses it for what S-mode
/// cannot do itself: program the timer on a CPU without Sstc and reset or
/// power off the machine. Each extension is probed once, in
/// <see cref="Initialize"/>; the legacy v0.1 calls are the fallback.
/// </summary>
internal static unsafe class Sbi
{
    /// <summary>Base extension (EID 0x10).</summary>
    private const ulong ExtensionBase = 0x10;
    /// <summary>Timer extension (EID "TIME").</summary>
    private const ulong ExtensionTime = 0x54494D45;
    /// <summary>System reset extension (EID "SRST").</summary>
    private const ulong ExtensionSystemReset = 0x53525354;
    /// <summary>Debug console extension (EID "DBCN").</summary>
    private const ulong ExtensionDebugConsole = 0x4442434E;

    /// <summary>Legacy (v0.1) set_timer: EID 0x00.</summary>
    private const ulong LegacySetTimer = 0x00;
    /// <summary>Legacy (v0.1) console_putchar: EID 0x01.</summary>
    private const ulong LegacyConsolePutChar = 0x01;
    /// <summary>Legacy (v0.1) shutdown: EID 0x08.</summary>
    private const ulong LegacyShutdown = 0x08;

    /// <summary>Base extension function: sbi_get_spec_version.</summary>
    private const ulong BaseGetSpecVersion = 0;
    /// <summary>Base extension function: sbi_probe_extension.</summary>
    private const ulong BaseProbeExtension = 3;
    /// <summary>TIME function: sbi_set_timer.</summary>
    private const ulong TimeSetTimer = 0;
    /// <summary>SRST function: sbi_system_reset.</summary>
    private const ulong SystemResetFunction = 0;
    /// <summary>DBCN function: sbi_debug_console_write_byte.</summary>
    private const ulong DebugConsoleWriteByte = 2;

    /// <summary>sbi_system_reset type: shut down.</summary>
    public const uint ResetTypeShutdown = 0;
    /// <summary>sbi_system_reset type: cold reboot.</summary>
    public const uint ResetTypeColdReboot = 1;
    /// <summary>sbi_system_reset reason: no reason.</summary>
    public const uint ResetReasonNone = 0;

    private static bool s_initialized;
    private static bool s_hasTime;
    private static bool s_hasSystemReset;
    private static bool s_hasDebugConsole;
    private static ulong s_specVersion;

    /// <summary>The SBI specification version the firmware implements: major in bits 30:24, minor in bits 23:0.</summary>
    public static ulong SpecVersion => s_specVersion;

    /// <summary>
    /// Probes the extensions the kernel uses. Safe to call more than once.
    /// </summary>
    public static void Initialize()
    {
        if (s_initialized)
        {
            return;
        }

        ulong version = 0;
        if (SbiNative.Call(ExtensionBase, BaseGetSpecVersion, 0, 0, 0, &version) == 0)
        {
            s_specVersion = version;
        }

        s_hasTime = HasExtension(ExtensionTime);
        s_hasSystemReset = HasExtension(ExtensionSystemReset);
        s_hasDebugConsole = HasExtension(ExtensionDebugConsole);
        s_initialized = true;
    }

    /// <summary>Whether the firmware implements extension <paramref name="extension"/>.</summary>
    /// <param name="extension">The extension ID.</param>
    public static bool HasExtension(ulong extension)
    {
        ulong available = 0;
        return SbiNative.Call(ExtensionBase, BaseProbeExtension, extension, 0, 0, &available) == 0 && available != 0;
    }

    /// <summary>
    /// Programs the next timer interrupt at <paramref name="time"/>, a value of
    /// the time CSR. Writing a time also clears the pending timer interrupt.
    /// </summary>
    /// <param name="time">The time CSR value to interrupt at.</param>
    public static void SetTimer(ulong time)
    {
        Initialize();
        if (s_hasTime)
        {
            SbiNative.Call(ExtensionTime, TimeSetTimer, time, 0, 0, null);
        }
        else
        {
            SbiNative.Call(LegacySetTimer, 0, time, 0, 0, null);
        }
    }

    /// <summary>
    /// Resets the machine: <paramref name="type"/> is <see cref="ResetTypeShutdown"/>
    /// or <see cref="ResetTypeColdReboot"/>. Returns only when the firmware refused.
    /// </summary>
    /// <param name="type">The reset type.</param>
    /// <param name="reason">The reset reason.</param>
    public static void SystemReset(uint type, uint reason)
    {
        Initialize();
        if (s_hasSystemReset)
        {
            SbiNative.Call(ExtensionSystemReset, SystemResetFunction, type, reason, 0, null);
        }

        if (type == ResetTypeShutdown)
        {
            SbiNative.Call(LegacyShutdown, 0, 0, 0, 0, null);
        }
    }

    /// <summary>
    /// Writes one byte to the firmware's debug console: output that does not
    /// depend on the UART being mapped. Slow, one ecall per byte.
    /// </summary>
    /// <param name="value">The byte.</param>
    public static void DebugConsoleWrite(byte value)
    {
        if (s_hasDebugConsole)
        {
            SbiNative.Call(ExtensionDebugConsole, DebugConsoleWriteByte, value, 0, 0, null);
        }
        else
        {
            SbiNative.Call(LegacyConsolePutChar, 0, value, 0, 0, null);
        }
    }
}
