namespace Cosmos.Kernel.HAL.Pci;

internal class PciBaseAddressBar
{
    private ushort _prefetchable;
    private ushort _type;

    public PciBaseAddressBar(uint raw)
    {
        IsIo = (raw & 0x01) == 1;

        if (IsIo)
        {
            BaseAddress = raw & 0xFFFFFFFC;
        }
        else
        {
            _type = (ushort)((raw >> 1) & 0x03);
            _prefetchable = (ushort)((raw >> 3) & 0x01);
            // Type 0x00 = 32-bit, 0x02 = 64-bit (lower 32 bits), 0x01 = reserved
            // For all memory BARs, mask off the lower 4 bits to get the base address
            BaseAddress = raw & 0xFFFFFFF0;
        }
    }

    public uint BaseAddress { get; }

    public bool IsIo { get; }

    /// <summary>
    /// Returns true if this is a 64-bit BAR (type 0x02).
    /// The next BAR contains the upper 32 bits of the address.
    /// </summary>
    public bool Is64Bit => !IsIo && _type == 0x02;
}
