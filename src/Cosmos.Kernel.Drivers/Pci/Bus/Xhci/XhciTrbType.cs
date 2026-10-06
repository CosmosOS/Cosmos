// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Pci.Bus.Xhci;

/// <summary>TRB type field values the xHCI driver uses (xHCI 1.2 table 6-91).</summary>
internal enum XhciTrbType : byte
{
    /// <summary>A Normal TRB: one buffer of a bulk or interrupt transfer.</summary>
    Normal = 1,

    /// <summary>The Setup Stage of a control transfer, the SETUP packet as immediate data.</summary>
    SetupStage = 2,

    /// <summary>The Data Stage of a control transfer.</summary>
    DataStage = 3,

    /// <summary>The Status Stage of a control transfer.</summary>
    StatusStage = 4,

    /// <summary>A Link TRB back to the start of the ring.</summary>
    Link = 6,

    /// <summary>Enable Slot.</summary>
    EnableSlotCommand = 9,

    /// <summary>Disable Slot.</summary>
    DisableSlotCommand = 10,

    /// <summary>Address Device.</summary>
    AddressDeviceCommand = 11,

    /// <summary>Configure Endpoint.</summary>
    ConfigureEndpointCommand = 12,

    /// <summary>Evaluate Context.</summary>
    EvaluateContextCommand = 13,

    /// <summary>Reset Endpoint.</summary>
    ResetEndpointCommand = 14,

    /// <summary>Stop Endpoint.</summary>
    StopEndpointCommand = 15,

    /// <summary>Set TR Dequeue Pointer.</summary>
    SetTrDequeuePointerCommand = 16,

    /// <summary>A Transfer Event: one TRB of a transfer ring completed.</summary>
    TransferEvent = 32,

    /// <summary>A Command Completion Event.</summary>
    CommandCompletionEvent = 33,

    /// <summary>A Port Status Change Event.</summary>
    PortStatusChangeEvent = 34,

    /// <summary>A Host Controller Event: the controller reports an error of its own.</summary>
    HostControllerEvent = 37
}
