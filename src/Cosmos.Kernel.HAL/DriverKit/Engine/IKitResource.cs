// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.DriverKit.Engine;

/// <summary>
/// Something the kit hands out and takes back: an entry of a binding's
/// ledger, or a bus's own allocation for a node. Released in thread context
/// by the kit, in the reverse of the order acquired.
/// </summary>
internal interface IKitResource
{
    /// <summary>Gives the resource back. Called once, by the kit.</summary>
    void Release();
}
