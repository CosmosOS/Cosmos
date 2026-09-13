using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.System.Diagnostics;

/// <summary>
/// Kernel debug log, written to the platform serial port (COM1 on x64,
/// PL011 on ARM64). This is the supported way for kernels to emit debug
/// output that a host can capture; on QEMU the stream appears on the
/// <c>-serial</c> device. Writes are synchronous, allocation-free for
/// strings and numbers, and usable from the earliest phase of
/// <c>BeforeRun</c> onward.
/// </summary>
public static class Log
{
    /// <summary>
    /// Writes a string to the log without appending a line terminator.
    /// </summary>
    /// <param name="text">Text to write.</param>
    public static void WriteString(string text) => Serial.WriteString(text);

    /// <summary>
    /// Writes an unsigned 64-bit number in decimal. Use
    /// <see cref="WriteHex(ulong)"/> for base-16 digits.
    /// </summary>
    /// <param name="number">Value to write.</param>
    public static void WriteNumber(ulong number) => Serial.WriteNumber(number);

    /// <summary>
    /// Writes an unsigned 32-bit number in decimal. Use
    /// <see cref="WriteHex(uint)"/> for base-16 digits.
    /// </summary>
    /// <param name="number">Value to write.</param>
    public static void WriteNumber(uint number) => Serial.WriteNumber(number);

    /// <summary>
    /// Writes a signed 32-bit number in decimal. Negative values are
    /// prefixed with <c>-</c>.
    /// </summary>
    /// <param name="number">Value to write.</param>
    public static void WriteNumber(int number) => Serial.WriteNumber(number);

    /// <summary>
    /// Writes a signed 64-bit number in decimal. Negative values are
    /// prefixed with <c>-</c>.
    /// </summary>
    /// <param name="number">Value to write.</param>
    public static void WriteNumber(long number) => Serial.WriteNumber(number);

    /// <summary>
    /// Writes an unsigned 64-bit number as hexadecimal digits without a
    /// <c>0x</c> prefix.
    /// </summary>
    /// <param name="number">Value to write.</param>
    public static void WriteHex(ulong number) => Serial.WriteHex(number);

    /// <summary>
    /// Writes an unsigned 32-bit number as hexadecimal digits without a
    /// <c>0x</c> prefix.
    /// </summary>
    /// <param name="number">Value to write.</param>
    public static void WriteHex(uint number) => Serial.WriteHex(number);

    /// <summary>
    /// Writes an unsigned 64-bit number as hexadecimal digits with a
    /// <c>0x</c> prefix.
    /// </summary>
    /// <param name="number">Value to write.</param>
    public static void WriteHexWithPrefix(ulong number) => Serial.WriteHexWithPrefix(number);

    /// <summary>
    /// Writes an unsigned 32-bit number as hexadecimal digits with a
    /// <c>0x</c> prefix.
    /// </summary>
    /// <param name="number">Value to write.</param>
    public static void WriteHexWithPrefix(uint number) => Serial.WriteHexWithPrefix(number);

    /// <summary>
    /// Writes each value in order: strings and characters as text, integers
    /// in decimal, bytes and byte arrays in hexadecimal, booleans as
    /// <c>true</c>/<c>false</c>, <see langword="null"/> as <c>null</c>, and
    /// anything else via <see cref="object.ToString"/>. The argument list
    /// itself costs nothing: the compiler builds it on the stack. A value
    /// type passed here is still boxed, which does allocate, so prefer
    /// <see cref="WriteString"/> and the <c>WriteNumber</c> overloads on
    /// allocation-sensitive paths.
    /// </summary>
    /// <param name="args">Values to write.</param>
    public static void Write(params ReadOnlySpan<object?> args) => Serial.Write(args);

    /// <summary>
    /// Writes raw bytes to the log stream as one uninterrupted sequence.
    /// Interrupts are masked for the duration of the write, so traces
    /// logged from IRQ handlers cannot interleave into the middle of the
    /// data. Use this for binary wire formats that share the serial port
    /// with text output, such as the kernel test protocol.
    /// </summary>
    /// <param name="bytes">Bytes to write.</param>
    public static void WriteBytes(ReadOnlySpan<byte> bytes)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                Serial.ComWrite(bytes[i]);
            }
        }
    }
}
