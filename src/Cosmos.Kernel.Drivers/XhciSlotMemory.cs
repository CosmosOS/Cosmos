// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// The four DMA pages one addressed device needs, and the event its control
/// transfers wait on: the output device context the controller keeps, the
/// input context the commands are built in, the default control endpoint's
/// ring and the bounce page of a control data stage. Allocated once on the
/// host's binding, never freed early (the kit frees every DMA page in the
/// binding's teardown alone), and pooled by <see cref="XhciState"/>: a
/// released set goes back to the free list and the next Address Device
/// takes it, with a fresh <see cref="XhciSlot"/> over it.
/// </summary>
internal sealed class XhciSlotMemory
{
    /// <summary>Index of the Input Control Context in the input page.</summary>
    private const int InputControlIndex = 0;

    /// <summary>Index of the Slot Context in the input page.</summary>
    private const int InputSlotIndex = 1;

    /// <summary>The Slot Context is the first context of the output page.</summary>
    private const int OutputSlotIndex = 0;

    /// <summary>Takes the pages and the event the host allocated. Thread context.</summary>
    /// <param name="outputContext">One page: the output device context.</param>
    /// <param name="inputContext">One page: the input context.</param>
    /// <param name="controlRing">One page: the default control endpoint's transfer ring.</param>
    /// <param name="controlBuffer">One page: the data stage bounce buffer.</param>
    /// <param name="controlEvent">The event the control transfer completion signals.</param>
    /// <param name="contextSize">32 or 64, the controller's context size.</param>
    public XhciSlotMemory(DmaBuffer outputContext, DmaBuffer inputContext, DmaBuffer controlRing, DmaBuffer controlBuffer, DeviceEvent controlEvent, int contextSize)
    {
        OutputContext = outputContext;
        InputContext = inputContext;
        ControlRing = new XhciRing(controlRing);
        ControlBuffer = controlBuffer;
        ControlEvent = controlEvent;
        ContextSize = contextSize;
    }

    /// <summary>The output device context: the Slot Context and the 31 Endpoint Contexts the controller keeps up to date.</summary>
    public DmaBuffer OutputContext { get; }

    /// <summary>The input context: the Input Control Context, then a Slot Context and 31 Endpoint Contexts.</summary>
    public DmaBuffer InputContext { get; }

    /// <summary>The default control endpoint's transfer ring.</summary>
    public XhciRing ControlRing { get; }

    /// <summary>The bounce page of a control transfer's data stage.</summary>
    public DmaBuffer ControlBuffer { get; }

    /// <summary>The event a control transfer's completion signals, created once per set.</summary>
    public DeviceEvent ControlEvent { get; }

    /// <summary>The controller's context size, 32 or 64 bytes.</summary>
    public int ContextSize { get; }

    /// <summary>The Input Control Context. Any context.</summary>
    public Span<uint> InputControl() => XhciContext.Context(InputContext, ContextSize, InputControlIndex);

    /// <summary>The input Slot Context. Any context.</summary>
    public Span<uint> InputSlot() => XhciContext.Context(InputContext, ContextSize, InputSlotIndex);

    /// <summary>The input Endpoint Context of DCI <paramref name="dci"/>, at index <c>dci + 1</c>. Any context.</summary>
    /// <param name="dci">The Device Context Index, 1 to 31.</param>
    public Span<uint> InputEndpoint(byte dci) => XhciContext.Context(InputContext, ContextSize, dci + 1);

    /// <summary>The output Slot Context. Any context.</summary>
    public Span<uint> OutputSlot() => XhciContext.Context(OutputContext, ContextSize, OutputSlotIndex);

    /// <summary>The output Endpoint Context of DCI <paramref name="dci"/>, at index <c>dci</c>. Any context.</summary>
    /// <param name="dci">The Device Context Index, 1 to 31.</param>
    public Span<uint> OutputEndpoint(byte dci) => XhciContext.Context(OutputContext, ContextSize, dci);

    /// <summary>Zeroes the input context alone: the first step of every command built in it. Thread context.</summary>
    public void ClearInput() => InputContext.Span.Clear();

    /// <summary>Zeroes both contexts and the bounce page and resets the ring, for a set taken from the pool. Thread context, before the slot is addressed.</summary>
    public void Clear()
    {
        OutputContext.Span.Clear();
        InputContext.Span.Clear();
        ControlBuffer.Span.Clear();
        ControlRing.Reset();
    }
}
