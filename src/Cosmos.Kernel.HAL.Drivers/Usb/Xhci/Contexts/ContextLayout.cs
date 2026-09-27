// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Usb;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Contexts;

/// <summary>
/// Field layout of the Input Control, Slot and Endpoint contexts (xHCI 1.2
/// §6.2). Contexts are 32 or 64 bytes (HCCPARAMS1.CSZ); every field lives
/// in the first 32, so the helpers work on the context's dwords either way,
/// which a device slot hands out as spans over its DMA pages.
/// </summary>
internal static class ContextLayout
{
    /// <summary>Input Control Context add flag A0: the Slot Context is part of the command.</summary>
    internal const uint SlotFlag = 1u << 0;

    // Input Control Context (xHCI 1.2 §6.2.5.1).
    private const int DropFlagsDword = 0;
    private const int AddFlagsDword = 1;

    // Slot Context (xHCI 1.2 §6.2.2).
    private const uint RouteStringMask = 0xFFFFF;
    private const int SpeedShift = 20;
    private const uint HubBit = 1u << 26;
    private const int ContextEntriesShift = 27;
    private const uint ContextEntriesMask = 0x1F;
    private const int RootPortShift = 16;
    private const int PortCountShift = 24;
    private const uint PortCountMask = 0xFFu << PortCountShift;
    private const int TtPortShift = 8;
    private const int ThinkTimeShift = 16;
    private const uint ThinkTimeMask = 0x3u << ThinkTimeShift;
    private const int SlotContextDwords = 4;

    // Endpoint Context (xHCI 1.2 §6.2.3).
    private const uint EndpointStateMask = 0x7;
    private const int IntervalShift = 16;
    private const int MaxEsitPayloadHighShift = 24;
    private const int MaxEsitPayloadHighInputShift = 16;
    private const uint MaxEsitPayloadLowMask = 0xFFFF;
    private const int ErrorCountShift = 1;
    private const int EndpointTypeShift = 3;
    private const int MaxBurstShift = 8;
    private const int MaxPacketSizeShift = 16;
    private const int MaxEsitPayloadLowShift = 16;
    private const uint DequeueCycleState = 1u;
    private const int UpperDwordShift = 32;

    /// <summary>CErr: retries on a transaction error before the endpoint halts; 3 is the value the spec recommends.</summary>
    private const uint DefaultErrorCount = 3;

    /// <summary>Input Control Context add flag for the endpoint at Device Context Index <paramref name="endpointId"/>.</summary>
    internal static uint EndpointFlag(byte endpointId) => 1u << endpointId;

    internal static void SetAddFlags(Span<uint> inputControl, uint flags) => SetFlags(inputControl, 0, flags);

    /// <summary>
    /// Sets both flag sets of the Input Control Context. A context both
    /// dropped and added is reinitialized from the input context.
    /// </summary>
    internal static void SetFlags(Span<uint> inputControl, uint dropFlags, uint addFlags)
    {
        inputControl[DropFlagsDword] = dropFlags;
        inputControl[AddFlagsDword] = addFlags;
    }

    internal static void WriteSlot(Span<uint> slot, uint routeString, UsbSpeed speed, byte contextEntries, byte rootPort, byte ttHubSlotId, byte ttPort)
    {
        slot[0] = (routeString & RouteStringMask) | ((uint)speed << SpeedShift) | ((uint)contextEntries << ContextEntriesShift);
        slot[1] = (uint)rootPort << RootPortShift;
        slot[2] = ttHubSlotId | ((uint)ttPort << TtPortShift);
        slot[3] = 0;
    }

    /// <summary>Copies the controller's current Slot Context into an input context before changing it.</summary>
    internal static void CopySlot(ReadOnlySpan<uint> source, Span<uint> destination) =>
        source[..SlotContextDwords].CopyTo(destination);

    /// <summary>
    /// Marks the slot as a hub (single-TT: the multi-TT alternate setting
    /// is never selected, so MTT stays clear).
    /// </summary>
    internal static void SetHub(Span<uint> slot, byte portCount, byte thinkTime)
    {
        slot[0] |= HubBit;
        slot[1] = (slot[1] & ~PortCountMask) | ((uint)portCount << PortCountShift);
        slot[2] = (slot[2] & ~ThinkTimeMask) | ((uint)thinkTime << ThinkTimeShift);
    }

    internal static byte GetContextEntries(ReadOnlySpan<uint> slot) => (byte)((slot[0] >> ContextEntriesShift) & ContextEntriesMask);

    internal static void SetContextEntries(Span<uint> slot, byte entries) =>
        slot[0] = (slot[0] & ~(ContextEntriesMask << ContextEntriesShift)) | ((uint)entries << ContextEntriesShift);

    /// <summary>
    /// EP State of an Output Endpoint Context, the one the controller keeps
    /// up to date. Volatile: the controller changes it behind the driver's
    /// back.
    /// </summary>
    internal static EndpointState GetEndpointState(Span<uint> endpoint) =>
        (EndpointState)(Volatile.Read(ref endpoint[0]) & EndpointStateMask);

    internal static void WriteEndpoint(Span<uint> endpoint, EndpointType type, ushort maxPacketSize, byte maxBurst, byte interval,
        ulong dequeuePointer, bool cycleState, ushort averageTrbLength, uint maxEsitPayload)
    {
        endpoint[0] = ((uint)interval << IntervalShift) | ((maxEsitPayload >> MaxEsitPayloadHighInputShift) << MaxEsitPayloadHighShift);
        endpoint[1] = (DefaultErrorCount << ErrorCountShift)
            | ((uint)type << EndpointTypeShift)
            | ((uint)maxBurst << MaxBurstShift)
            | ((uint)maxPacketSize << MaxPacketSizeShift);
        endpoint[2] = (uint)dequeuePointer | (cycleState ? DequeueCycleState : 0);
        endpoint[3] = (uint)(dequeuePointer >> UpperDwordShift);
        endpoint[4] = averageTrbLength | ((maxEsitPayload & MaxEsitPayloadLowMask) << MaxEsitPayloadLowShift);
    }
}
