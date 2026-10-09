// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Boot;

namespace Cosmos.Kernel.HAL.DriverKit.Engine;

/// <summary>
/// Maps device memory windows for the kit. Both device mappers install
/// their mappings one <see cref="MappingBlockSize"/> block at a time (x64 an
/// uncacheable 2 MiB page above 4 GiB, ARM64 a 2 MiB Device block in
/// TTBR1), so a window is mapped by asking for every block it touches:
/// asking for its two ends alone leaves the middle of a window wider than
/// two blocks unmapped. A write-combining window gets that attribute in the
/// blocks it covers whole, since a block's attributes apply to all of it,
/// and device memory in a block it shares. Thread context.
/// </summary>
internal static class DeviceMemory
{
    /// <summary>The block size both device mappers install: 2 MiB.</summary>
    public const ulong MappingBlockSize = 2 * 1024 * 1024;

    /// <summary>
    /// Asks the platform to map every block of the window that starts at
    /// <paramref name="physicalBase"/> and spans <paramref name="length"/>
    /// bytes, from the block holding the first byte to the block holding the
    /// last. A zero length maps the block holding the base. Thread context.
    /// </summary>
    /// <param name="physicalBase">Physical address of the first byte.</param>
    /// <param name="length">Length in bytes.</param>
    /// <param name="writeCombining">Whether the blocks the window covers whole are mapped write-combining, for a framebuffer.</param>
    /// <returns>False when there is no platform initializer, the window wraps the address space, or a block cannot be mapped; nothing past the first refusal is asked for.</returns>
    public static bool EnsureWindowMapped(ulong physicalBase, ulong length, bool writeCombining = false)
    {
        IPlatformInitializer? initializer = PlatformHAL.Initializer;
        if (initializer is null)
        {
            return false;
        }

        ulong last = length == 0 ? physicalBase : physicalBase + length - 1;
        if (last < physicalBase)
        {
            return false;
        }

        ulong lastBlock = last & ~(MappingBlockSize - 1);
        ulong block = physicalBase & ~(MappingBlockSize - 1);

        // The first block is asked for by the exact base address, so a
        // platform that refuses address zero as "unassigned" sees the same
        // request it saw before windows were walked block by block.
        ulong address = physicalBase;
        while (true)
        {
            bool whole = block >= physicalBase && last - block >= MappingBlockSize - 1;
            bool mapped = writeCombining && whole
                ? initializer.EnsureWriteCombiningMapped(address)
                : initializer.EnsureMmioMapped(address);
            if (!mapped)
            {
                return false;
            }

            if (block == lastBlock)
            {
                return true;
            }

            block += MappingBlockSize;
            address = block;
        }
    }
}
