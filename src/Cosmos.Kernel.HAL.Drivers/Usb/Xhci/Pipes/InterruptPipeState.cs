// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Pipes;

/// <summary>Where an interrupt pipe is in its lifecycle.</summary>
internal enum InterruptPipeState
{
    Running,

    /// <summary>A Reset Endpoint command is in flight after a transfer error.</summary>
    ResettingEndpoint,

    /// <summary>A Set TR Dequeue Pointer command is in flight, moving past the abandoned transfers.</summary>
    SettingDequeuePointer,

    /// <summary>Recovery gave up; no transfers are queued any more.</summary>
    Stopped
}
