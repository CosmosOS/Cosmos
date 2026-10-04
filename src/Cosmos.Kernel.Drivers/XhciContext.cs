// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Usb;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// Field layout of the Input Control, Slot and Endpoint contexts (xHCI 1.2
/// section 6.2) over a <c>Span&lt;uint&gt;</c>. Contexts are 32 or 64 bytes
/// (HCCPARAMS1.CSZ, the controller's <see cref="XhciState.ContextSize"/>);
/// every field lives in the first 32, so the helpers work on eight dwords
/// either way and the size is only a stride. In the input context index 0
/// is the Input Control Context, 1 the Slot Context and <c>dci + 1</c> the
/// context of endpoint DCI <c>dci</c>; in the output context 0 is the Slot
/// Context and <c>dci</c> the endpoint.
/// </summary>
internal static class XhciContext
{
    /// <summary>Input Control Context add flag A0: the Slot Context is part of the command.</summary>
    public const uint SlotFlag = 1u << 0;

    /// <summary>Bytes of a context's fields, whatever its stride.</summary>
    private const int ContextFieldBytes = 32;

    // Input Control Context (xHCI 1.2 section 6.2.5.1).
    private const int DropFlagsDword = 0;
    private const int AddFlagsDword = 1;

    // Slot Context (xHCI 1.2 section 6.2.2).
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

    // Endpoint Context (xHCI 1.2 section 6.2.3).
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

    /// <summary>The eight dwords of context <paramref name="index"/> in <paramref name="page"/>, at stride <paramref name="contextSize"/>. Any context, allocation-free.</summary>
    /// <param name="page">An input or output context page.</param>
    /// <param name="contextSize">32 or 64, the controller's context size.</param>
    /// <param name="index">The context's index in the page.</param>
    public static Span<uint> Context(DmaBuffer page, int contextSize, int index) =>
        MemoryMarshal.Cast<byte, uint>(page.Span.Slice(index * contextSize, ContextFieldBytes));

    /// <summary>Input Control Context add or drop flag for the endpoint at Device Context Index <paramref name="endpointId"/>.</summary>
    /// <param name="endpointId">The DCI, 1 to 31.</param>
    public static uint EndpointFlag(byte endpointId) => 1u << endpointId;

    /// <summary>Sets both flag sets of the Input Control Context. A context both dropped and added is reinitialized from the input context.</summary>
    /// <param name="inputControl">The Input Control Context.</param>
    /// <param name="dropFlags">Dword 0.</param>
    /// <param name="addFlags">Dword 1.</param>
    public static void SetFlags(Span<uint> inputControl, uint dropFlags, uint addFlags)
    {
        inputControl[DropFlagsDword] = dropFlags;
        inputControl[AddFlagsDword] = addFlags;
    }

    /// <summary>Writes a Slot Context for Address Device: route string, speed and Context Entries in dword 0, the root port in dword 1, the TT fields in dword 2, dword 3 zero.</summary>
    /// <param name="slot">The input Slot Context.</param>
    /// <param name="routeString">The route string, bits 19:0.</param>
    /// <param name="speed">The device speed, bits 23:20.</param>
    /// <param name="contextEntries">The highest DCI the device context covers, bits 31:27.</param>
    /// <param name="rootPort">The root hub port, dword 1 bits 23:16.</param>
    /// <param name="ttHubSlotId">The slot of the hub whose TT serves the device, 0 when none.</param>
    /// <param name="ttPort">That hub's port the device hangs off.</param>
    public static void WriteSlot(Span<uint> slot, uint routeString, UsbSpeed speed, byte contextEntries, byte rootPort, byte ttHubSlotId, byte ttPort)
    {
        slot[0] = (routeString & RouteStringMask) | ((uint)speed << SpeedShift) | ((uint)contextEntries << ContextEntriesShift);
        slot[1] = (uint)rootPort << RootPortShift;
        slot[2] = ttHubSlotId | ((uint)ttPort << TtPortShift);
        slot[3] = 0;
    }

    /// <summary>Copies the controller's current Slot Context into an input context before changing it.</summary>
    /// <param name="source">The output Slot Context.</param>
    /// <param name="destination">The input Slot Context.</param>
    public static void CopySlot(Span<uint> source, Span<uint> destination)
    {
        for (int i = 0; i < SlotContextDwords; i++)
        {
            destination[i] = source[i];
        }
    }

    /// <summary>Marks the slot as a hub (single-TT: the multi-TT alternate setting is never selected, so MTT stays clear).</summary>
    /// <param name="slot">The input Slot Context.</param>
    /// <param name="portCount">bNbrPorts, dword 1 bits 31:24.</param>
    /// <param name="thinkTime">TT think time, dword 2 bits 17:16.</param>
    public static void SetHub(Span<uint> slot, byte portCount, byte thinkTime)
    {
        slot[0] |= HubBit;
        slot[1] = (slot[1] & ~PortCountMask) | ((uint)portCount << PortCountShift);
        slot[2] = (slot[2] & ~ThinkTimeMask) | ((uint)thinkTime << ThinkTimeShift);
    }

    /// <summary>Context Entries of a Slot Context, dword 0 bits 31:27.</summary>
    /// <param name="slot">A Slot Context.</param>
    public static byte GetContextEntries(Span<uint> slot) => (byte)((slot[0] >> ContextEntriesShift) & ContextEntriesMask);

    /// <summary>Sets Context Entries of a Slot Context.</summary>
    /// <param name="slot">The input Slot Context.</param>
    /// <param name="entries">The highest DCI the device context covers.</param>
    public static void SetContextEntries(Span<uint> slot, byte entries) =>
        slot[0] = (slot[0] & ~(ContextEntriesMask << ContextEntriesShift)) | ((uint)entries << ContextEntriesShift);

    /// <summary>EP State of an Output Endpoint Context, the one the controller keeps up to date: a volatile read of dword 0. Any context.</summary>
    /// <param name="endpoint">The output Endpoint Context.</param>
    public static XhciEndpointState GetEndpointState(Span<uint> endpoint) =>
        (XhciEndpointState)(Volatile.Read(ref endpoint[0]) & EndpointStateMask);

    /// <summary>Writes an Endpoint Context: interval and Max ESIT Payload high in dword 0, CErr 3, type, burst and packet size in dword 1, the dequeue pointer with DCS in dwords 2 and 3, the average TRB length and Max ESIT Payload low in dword 4.</summary>
    /// <param name="endpoint">The input Endpoint Context.</param>
    /// <param name="type">The endpoint type.</param>
    /// <param name="maxPacketSize">wMaxPacketSize bits 10:0.</param>
    /// <param name="maxBurst">Max Burst Size: the additional transactions of a high-speed periodic endpoint, bMaxBurst of a SuperSpeed one.</param>
    /// <param name="interval">The interval in the 2^n x 125 us encoding.</param>
    /// <param name="dequeuePointer">The ring's first TRB.</param>
    /// <param name="cycleState">The ring's cycle state, DCS.</param>
    /// <param name="averageTrbLength">The average TRB length the spec recommends for the type.</param>
    /// <param name="maxEsitPayload">Max ESIT Payload, 0 for control and bulk.</param>
    public static void WriteEndpoint(Span<uint> endpoint, XhciEndpointType type, ushort maxPacketSize, byte maxBurst, byte interval,
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
