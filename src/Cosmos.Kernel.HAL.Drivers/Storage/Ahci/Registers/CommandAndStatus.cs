// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.Ahci.Registers;

/// <summary>
/// PxCMD bits this driver reads or writes (AHCI 1.3.1 s3.3.7).
/// </summary>
[Flags]
internal enum CommandAndStatus : uint
{
    /// <summary>ATAPI: the device attached is an ATAPI device.</summary>
    ATAPIDevice = 1 << 24,

    /// <summary>CR: the command list engine is running.</summary>
    CMDListRunning = 1 << 15,

    /// <summary>FR: the FIS receive engine is running.</summary>
    FISReceiveRunning = 1 << 14,

    /// <summary>FRE: FIS receive enable.</summary>
    FISReceiveEnable = 1 << 4,

    /// <summary>CLO: command list override.</summary>
    CMDListOverride = 1 << 3,

    /// <summary>ST: start processing the command list.</summary>
    StartProcess = 1 << 0
}
