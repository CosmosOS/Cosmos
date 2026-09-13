// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.Core.CPU;

/// <summary>
/// Architecture-neutral interrupt registration and routing. Owns the dense
/// 256-entry handler table and the dynamic-vector allocator used by MSI
/// programmers. The actual dispatch path (ack/EOI/LPI/fatal-fault handling)
/// is delegated to the platform <see cref="IInterruptController"/>
/// implementation in Cosmos.Kernel.Core.X64 / Cosmos.Kernel.Core.ARM64.
/// </summary>
internal static class InterruptManager
{
    /// <summary>
    /// Interrupt delegate signature.
    /// </summary>
    /// <param name="context">The interrupt context captured by the CPU.</param>
    public delegate void IrqDelegate(ref IRQContext context);

    internal static IrqDelegate?[]? s_irqHandlers;

    private static IInterruptController? s_controller;

    private const string NewLine = "\n";

    /// <summary>Number of entries in the interrupt handler table (one per CPU interrupt vector, 0x00-0xFF).</summary>
    private const int HandlerTableSize = 256;

    /// <summary>Base CPU vector for ISA hardware IRQs: IRQ n is dispatched on vector 0x20 + n (legacy PIC remap window).</summary>
    private const int IsaIrqVectorBase = 0x20;

    /// <summary>
    /// Whether interrupt support is enabled. Uses centralized feature flag.
    /// </summary>
    public static bool IsEnabled => CosmosFeatures.InterruptsEnabled;

    /// <summary>
    /// Initializes the interrupt manager with a platform-specific controller.
    /// </summary>
    /// <param name="controller">Platform-specific interrupt controller (X64 or ARM64).</param>
    public static void Initialize(IInterruptController controller)
    {
        Serial.Write("[InterruptManager.Initialize] Allocating handlers array...\n");
        s_irqHandlers = new IrqDelegate[HandlerTableSize];
        s_controller = controller;

        Serial.Write("[InterruptManager.Initialize] Initializing platform interrupt controller...\n");
        controller.Initialize();
        Serial.Write("[InterruptManager.Initialize] Interrupt system ready\n");
    }

    /// <summary>
    /// Registers a handler for an interrupt vector.
    /// </summary>
    /// <param name="vector">Interrupt vector index.</param>
    /// <param name="handler">Delegate to handle the interrupt.</param>
    public static void SetHandler(byte vector, IrqDelegate handler)
    {
        if (s_irqHandlers == null)
        {
            Serial.Write("[InterruptManager] ERROR: s_irqHandlers is null! Initialize() must be called first.\n");
            return;
        }

        // Same lock as AllocateVector/FreeVector: an unlocked write here
        // could land mid-scan and stomp a slot the allocator just claimed,
        // silently dropping one of the two handlers.
        s_allocLock.Acquire();
        try
        {
            s_irqHandlers[vector] = handler;
        }
        finally
        {
            s_allocLock.Release();
        }
    }

    // Dynamic vector allocations (MSI / MSI-X) start above the legacy
    // ISA-IRQ window (0x20–0x2F) and any future arch-reserved range
    // (0x30–0x3F), and stop below the platform-claimed high vectors: the
    // x64 LAPIC timer (0xEF) and APIC spurious (0xFF) are registered via
    // SetHandler and must never be handed out — or freed — as dynamic
    // slots.
    private const byte DynamicVectorMin = 0x40;
    private const byte DynamicVectorMax = 0xEE;
    private static int s_nextDynamicVector = DynamicVectorMin;

    // Guards s_irqHandlers RMW in AllocateVector so concurrent device probes
    // can't both grab the same slot and silently drop one handler.
    private static Scheduler.SpinLock s_allocLock;

    /// <summary>
    /// Allocates an unused interrupt vector in [0x40..0xEE], registers
    /// <paramref name="handler"/> for it, and returns the vector. Used by
    /// MSI / MSI-X programmers that need a fresh vector unique to their
    /// device. Throws if the dynamic range is exhausted.
    /// </summary>
    public static byte AllocateVector(IrqDelegate handler)
    {
        if (s_irqHandlers == null)
        {
            throw new System.InvalidOperationException("InterruptManager.Initialize must be called before AllocateVector");
        }

        s_allocLock.Acquire();
        try
        {
            for (int v = s_nextDynamicVector; v <= DynamicVectorMax; v++)
            {
                if (s_irqHandlers[v] == null)
                {
                    s_irqHandlers[v] = handler;
                    s_nextDynamicVector = v + 1;
                    return (byte)v;
                }
            }
            // Wrap once in case earlier vectors were freed.
            for (int v = DynamicVectorMin; v < s_nextDynamicVector; v++)
            {
                if (s_irqHandlers[v] == null)
                {
                    s_irqHandlers[v] = handler;
                    s_nextDynamicVector = v + 1;
                    return (byte)v;
                }
            }
        }
        finally
        {
            s_allocLock.Release();
        }
        throw new System.InvalidOperationException("InterruptManager: dynamic vector range exhausted");
    }

    /// <summary>
    /// Releases a vector previously returned by <see cref="AllocateVector"/>:
    /// clears its handler so the slot can be handed out again (the
    /// allocator's wrap pass picks freed slots back up). Without this, every
    /// consumer teardown would permanently leak one of the 175 dynamic slots
    /// and leave a stale delegate rooted — and invokable — in the table.
    /// Vectors outside the dynamic range — including the platform-claimed
    /// LAPIC timer and spurious vectors above it — are ignored.
    /// </summary>
    public static void FreeVector(byte vector)
    {
        if (s_irqHandlers == null || vector < DynamicVectorMin || vector > DynamicVectorMax)
        {
            return;
        }

        s_allocLock.Acquire();
        try
        {
            s_irqHandlers[vector] = null;
        }
        finally
        {
            s_allocLock.Release();
        }
    }

    /// <summary>
    /// Registers a handler for a hardware IRQ and routes it through the interrupt controller.
    /// </summary>
    /// <param name="irqNo">IRQ index (0-15 for ISA IRQs).</param>
    /// <param name="handler">IRQ handler delegate.</param>
    /// <param name="startMasked">If true, the IRQ starts masked and must be explicitly unmasked.</param>
    public static void SetIrqHandler(byte irqNo, IrqDelegate handler, bool startMasked = false)
    {
        byte vector = (byte)(IsaIrqVectorBase + irqNo);
        SetHandler(vector, handler);

        // Route the IRQ through the platform-specific controller
        if (s_controller != null && s_controller.IsInitialized)
        {
            Serial.Write("[InterruptManager] Routing IRQ ", irqNo, " -> vector 0x", vector.ToString("X"), NewLine);
            s_controller.RouteIrq(irqNo, vector, startMasked);
        }
    }

    /// <summary>
    /// Called by native bridge from the arch ASM stubs. Delegates to the
    /// platform controller; all arch-specific behaviour lives there.
    /// </summary>
    /// <param name="ctx">Context structure.</param>
    public static void Dispatch(ref IRQContext ctx)
    {
        s_controller?.Dispatch(ref ctx);
    }
}
