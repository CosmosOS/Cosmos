// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>Where one TD slot of a memory set is; written under the controller's lock or in its handler.</summary>
internal enum OhciTdState : byte
{
    /// <summary>The slot holds no transfer the controller may still report: the dummy, or a TD whose done entry was processed or that was never retired.</summary>
    Free,

    /// <summary>The slot was queued on its ED and the controller has not delivered its done entry yet.</summary>
    InFlight
}
