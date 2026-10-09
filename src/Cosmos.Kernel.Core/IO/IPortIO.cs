namespace Cosmos.Kernel.Core.IO;

/// <summary>
/// Platform-specific port I/O. The x64 implementation issues in/out
/// instructions; ARM64 has no I/O port space and maps port numbers onto
/// an MMIO base instead.
/// </summary>
internal interface IPortIO
{
    /// <summary>
    /// Read one byte from an I/O port.
    /// </summary>
    /// <param name="port">I/O port address.</param>
    /// <returns>The byte read from the port.</returns>
    byte ReadByte(ushort port);

    /// <summary>
    /// Read a 16-bit word from an I/O port.
    /// </summary>
    /// <param name="port">I/O port address.</param>
    /// <returns>The word read from the port.</returns>
    ushort ReadWord(ushort port);

    /// <summary>
    /// Read a 32-bit dword from an I/O port.
    /// </summary>
    /// <param name="port">I/O port address.</param>
    /// <returns>The dword read from the port.</returns>
    uint ReadDWord(ushort port);

    /// <summary>
    /// Write one byte to an I/O port.
    /// </summary>
    /// <param name="port">I/O port address.</param>
    /// <param name="value">Byte to write.</param>
    void WriteByte(ushort port, byte value);

    /// <summary>
    /// Write a 16-bit word to an I/O port.
    /// </summary>
    /// <param name="port">I/O port address.</param>
    /// <param name="value">Word to write.</param>
    void WriteWord(ushort port, ushort value);

    /// <summary>
    /// Write a 32-bit dword to an I/O port.
    /// </summary>
    /// <param name="port">I/O port address.</param>
    /// <param name="value">Dword to write.</param>
    void WriteDWord(ushort port, uint value);
}
