// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.System.Diagnostics;

namespace DevKernel.Diagnostics.Smp;

// One thread of the registry as the monitor follows it from frame to frame.
internal struct SmpThread
{
    public bool Live;
    public uint Id;
    public uint Cpu;
    public KernelThreadState State;
    public SmpThreadRole Role;

    // The worker's index or the pool thread's rank, for the roles that have one.
    public int Ordinal;

    public ulong RuntimeNs;

    // Frames left of the highlight a thread gets when it changes CPU.
    public int MovedFrames;

    // Where its chip is drawn, which trails behind where it belongs.
    public double ChipX;
    public double ChipY;
    public bool ChipPlaced;
    public bool ChipVisible;
}

// What a thread is to the SMP monitor, which decides its color and its label.
internal enum SmpThreadRole
{
    // A thread the kernel started itself: Main, a session's shell, the finalizer, the
    // alarm thread, the driver kit's.
    Kernel,

    // A thread started as a System.Threading.Thread that is none of the scenario's: a
    // pool thread that ran no work item of it, a server's thread.
    Managed,

    // The thread that plays the scenario.
    Director,

    // The thread that draws the monitor.
    Monitor,

    // A thread of the pool, seen running one of the scenario's work items.
    Pool,

    // One of the scenario's workers.
    Worker,
}
