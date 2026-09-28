// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// A driver's interrupt handler. Runs in interrupt context with interrupts
/// masked on the interrupted stack: it acknowledges the device through a
/// <see cref="RegisterWindow"/>, reads what it must from DMA memory, and
/// hands everything else to a thread through <paramref name="context"/>. It
/// must not allocate, block, or reach the binding.
/// </summary>
/// <param name="context">What a handler may do: mask its source, signal an event, schedule a work item.</param>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public delegate void InterruptHandler(InterruptContext context);
