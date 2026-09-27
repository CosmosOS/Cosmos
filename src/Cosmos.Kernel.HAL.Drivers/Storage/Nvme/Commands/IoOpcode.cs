// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.Nvme.Commands;

/// <summary>
/// The NVM command set opcodes this driver issues (NVM Express 1.4 s6).
/// </summary>
internal enum IoOpcode : byte
{
    /// <summary>Flush: commit the volatile write cache to stable media.</summary>
    Flush = 0x00,

    /// <summary>Write.</summary>
    Write = 0x01,

    /// <summary>Read.</summary>
    Read = 0x02
}
