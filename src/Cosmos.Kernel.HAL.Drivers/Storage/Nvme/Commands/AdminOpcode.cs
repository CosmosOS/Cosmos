// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.Nvme.Commands;

/// <summary>
/// The admin command opcodes this driver issues (NVM Express 1.4 s5).
/// </summary>
internal enum AdminOpcode : byte
{
    /// <summary>Create I/O Submission Queue.</summary>
    CreateIoSubmissionQueue = 0x01,

    /// <summary>Create I/O Completion Queue.</summary>
    CreateIoCompletionQueue = 0x05,

    /// <summary>Identify.</summary>
    Identify = 0x06
}
