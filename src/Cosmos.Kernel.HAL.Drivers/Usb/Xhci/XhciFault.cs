// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Rings;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci;

/// <summary>
/// A fault the event handler saw in interrupt context, where no log line
/// can be built: recorded under the event lock, and written to the log by
/// the hot-plug thread, which the handler wakes for it.
/// </summary>
internal readonly struct XhciFault
{
    internal XhciFaultKind Kind { get; }

    /// <summary>The slot of the endpoint at fault; 0 for a host controller event.</summary>
    internal byte SlotId { get; }

    /// <summary>Device Context Index of the endpoint at fault; 0 for a host controller event.</summary>
    internal byte EndpointId { get; }

    internal CompletionCode Code { get; }

    internal XhciFault(XhciFaultKind kind, byte slotId, byte endpointId, CompletionCode code)
    {
        Kind = kind;
        SlotId = slotId;
        EndpointId = endpointId;
        Code = code;
    }

    /// <summary>The log line, without the driver's prefix.</summary>
    internal string Describe() => Kind switch
    {
        XhciFaultKind.EndpointReset => $"slot {SlotId} endpoint {EndpointId}: transfer error, completion code {(byte)Code}; resetting endpoint",
        XhciFaultKind.EndpointAbandoned => $"slot {SlotId} endpoint {EndpointId}: transfer error, completion code {(byte)Code}; giving up",
        XhciFaultKind.RecoveryFailed => $"slot {SlotId} endpoint {EndpointId}: recovery failed, completion code {(byte)Code}",
        _ => $"host controller event, completion code {(byte)Code}"
    };
}

/// <summary>What an <see cref="XhciFault"/> records.</summary>
internal enum XhciFaultKind : byte
{
    /// <summary>An interrupt endpoint's transfer failed, and the endpoint is being reset.</summary>
    EndpointReset,

    /// <summary>An interrupt endpoint failed once too often without a good transfer, and is stopped for good.</summary>
    EndpointAbandoned,

    /// <summary>A step of an interrupt endpoint's recovery failed, and the endpoint is stopped for good.</summary>
    RecoveryFailed,

    /// <summary>The controller reported an error of its own (xHCI 1.2 §6.4.2.6).</summary>
    HostControllerEvent
}
