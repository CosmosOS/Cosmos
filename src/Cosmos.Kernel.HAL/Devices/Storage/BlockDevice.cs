// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.Devices.Storage;

/// <summary>
/// Abstract base class for all block storage devices.
/// </summary>
internal abstract class BlockDevice : Device, IBlockDevice
{
    /// <inheritdoc />
    public ulong BlockCount { get; protected set; }

    /// <inheritdoc />
    public ulong BlockSize { get; protected set; }

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract void ReadBlock(ulong blockNo, ulong blockCount, Span<byte> data);

    /// <inheritdoc />
    public abstract void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data);

    /// <inheritdoc />
    /// <remarks>Default is a no-op for devices without a volatile write cache.</remarks>
    public virtual void Flush()
    {
    }

    // Device ctors run during early kernel init (before exception handlers
    // and the late module initializers), where CoreLib int formatting
    // (ToString / "" + int / $"") reproducibly triple-faults even though it
    // works fine once the kernel is up. Names are therefore built digit by
    // digit, like Serial.WriteNumber.

    /// <summary>Builds a device name like "sata0" without CoreLib int formatting.</summary>
    protected static string BuildDeviceName(string prefix, uint number)
    {
        Span<char> buffer = stackalloc char[MaxNameLength];
        int pos = Append(buffer, 0, prefix);
        pos = AppendDigits(buffer, pos, number);
        return new string(buffer[..pos]);
    }

    /// <summary>Builds a partition name like "sata0p1" without CoreLib int formatting.</summary>
    protected internal static string BuildDeviceName(string prefix, string infix, uint number)
    {
        Span<char> buffer = stackalloc char[MaxNameLength];
        int pos = Append(buffer, 0, prefix);
        pos = Append(buffer, pos, infix);
        pos = AppendDigits(buffer, pos, number);
        return new string(buffer[..pos]);
    }

    /// <summary>Builds a device name like "nvme0n1" without CoreLib int formatting.</summary>
    protected static string BuildDeviceName(string prefix, uint number, string infix, uint secondNumber)
    {
        Span<char> buffer = stackalloc char[MaxNameLength];
        int pos = Append(buffer, 0, prefix);
        pos = AppendDigits(buffer, pos, number);
        pos = Append(buffer, pos, infix);
        pos = AppendDigits(buffer, pos, secondNumber);
        return new string(buffer[..pos]);
    }

    // Longest possible name: a worst-case host name ("nvme" + 10 digits +
    // "n" + 10 digits) plus a partition suffix ("p" + 10 digits).
    private const int MaxNameLength = 48;

    /// <summary>Radix used when converting a number to its decimal digits.</summary>
    private const uint DecimalBase = 10;

    private static int Append(Span<char> buffer, int pos, string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            buffer[pos++] = text[i];
        }

        return pos;
    }

    private static int AppendDigits(Span<char> buffer, int pos, uint value)
    {
        if (value == 0)
        {
            buffer[pos++] = '0';
            return pos;
        }

        int start = pos;
        while (value != 0)
        {
            buffer[pos++] = (char)('0' + value % DecimalBase);
            value /= DecimalBase;
        }

        for (int i = start, j = pos - 1; i < j; i++, j--)
        {
            (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
        }

        return pos;
    }
}
