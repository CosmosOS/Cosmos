namespace Cosmos.Kernel.HAL.Pci.Enums;

internal enum PciBist : byte
{
    CocdMask = 0x0f, /* Return result */
    Start = 0x40, /* 1 to start BIST, 2 secs or less */
    Capable = 0x80 /* 1 if BIST capable */
}
