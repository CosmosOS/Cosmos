// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.Nvme.Commands;

/// <summary>
/// The Controller or Namespace Structure an Identify command returns,
/// chosen by its CNS field in command dword 10 (NVM Express 1.4 s5.15).
/// </summary>
internal enum IdentifyCns : byte
{
    /// <summary>The Identify Namespace data of the namespace the command names.</summary>
    Namespace = 0x00,

    /// <summary>The Identify Controller data.</summary>
    Controller = 0x01,

    /// <summary>The active namespace IDs, in increasing order, zero-terminated.</summary>
    ActiveNamespaceList = 0x02
}
