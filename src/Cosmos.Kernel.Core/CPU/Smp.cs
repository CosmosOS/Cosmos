// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core.Bridge;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Scheduler;

namespace Cosmos.Kernel.Core.CPU;

/// <summary>
/// The CPUs of the machine, as the bootloader reports them, and their bring-up (SMP phase 2,
/// CosmosOS/Cosmos#3339). Limine's MP response lists every CPU with the identity its interrupt
/// controller knows it by (the Local APIC id on x64, the MPIDR affinity on ARM64) and names the
/// one that booted; the list, not the MADT, is the truth about how many CPUs exist, since it is
/// what the bootloader could start, and a machine without ACPI has one too. The boot CPU is
/// index 0 wherever the list puts it; the others take indices 1 onward in the list's order,
/// which is the order the scheduler's per-CPU states and idle threads follow.
/// </summary>
internal static unsafe class Smp
{
    /// <summary>
    /// How long the boot CPU waits for an application processor to report online after writing
    /// its <c>goto_address</c>, in <see cref="Stopwatch"/> seconds. A CPU the bootloader listed
    /// and parked comes up in microseconds; one that does not is a broken machine, and the
    /// kernel says so rather than schedule on a CPU count that is not true.
    /// </summary>
    private const long StartTimeoutSeconds = 2;

    // The boot CPU's reading of the timestamp counter just before it releases an application
    // processor; the processor compares its own on arrival (ApMain). On x64 Stopwatch reads the
    // TSC, which is per CPU: a skew shows here as a log line, not later as deadlines in the
    // wrong order.
    private static long s_releaseTimestamp;

    // How a CPU starts its scheduler tick, handed in by the ring that owns the platform
    // initializer: Core does not reference the HAL, and the tick source is the HAL's.
    private static Action? s_startTickOnCurrentCpu;

    /// <summary>
    /// The CPUs the bootloader reports, the boot CPU included: 1 when it did not answer the MP
    /// request (a loader without the feature, or a boot it could not enumerate the CPUs on),
    /// and never more than the per-CPU table holds.
    /// </summary>
    internal static uint CpuCount
    {
        get
        {
            ulong count = BootloaderCpuCount;
            if (count == 0)
            {
                return 1;
            }

            return count > (ulong)PerCpuBlock.MaxCpus ? (uint)PerCpuBlock.MaxCpus : (uint)count;
        }
    }

    /// <summary>
    /// The interrupt controller identity the bootloader reports for CPU <paramref name="index"/>
    /// (the Local APIC id on x64, the MPIDR affinity on ARM64): what the controller's own read
    /// on that CPU must agree with. False past the list.
    /// </summary>
    internal static bool TryGetHardwareId(uint index, out ulong hardwareId)
    {
        return TryGetEntry(index, out _, out _, out hardwareId);
    }

    /// <summary>
    /// Starts every application processor the bootloader parked and waits for each to come
    /// online, one after the other: the boot CPU, at the end of the kernel ring's initializer,
    /// once the scheduler has a state and an idle thread per CPU and its own tick runs, before
    /// the drivers come up. Each CPU's block is prepared here (its index, its address, the
    /// ExInfo chain head of its idle thread), then its <c>extra_argument</c> and, with a release
    /// store, its <c>goto_address</c>; the CPU lands in the trampoline and ends in
    /// <see cref="ApMain"/>. A CPU that does not report online within
    /// <see cref="StartTimeoutSeconds"/> is a panic.
    /// </summary>
    /// <param name="startTickOnCurrentCpu">Starts the scheduler tick on the calling CPU, as the boot CPU's was started.</param>
    internal static void StartApplicationProcessors(Action startTickOnCurrentCpu)
    {
        s_startTickOnCurrentCpu = startTickOnCurrentCpu;

        uint count = SchedulerManager.CpuCount;
        if (count <= 1)
        {
            Serial.WriteString("[SMP] One CPU\n");
            return;
        }

        nuint trampoline = CpuNative.GetApTrampolineAddress();

        for (uint index = 1; index < count; index++)
        {
            if (!TryGetEntry(index, out ulong* extraArgument, out nuint* gotoAddress, out ulong hardwareId))
            {
                break;
            }

            SchedulerThread? idleThread = SchedulerManager.GetCpuState(index)?.IdleThread;
            if (idleThread is null)
            {
                Panic.Halt("An application processor has no idle thread: the scheduler was not initialized for every CPU.");
            }

            // Everything the CPU reads through its register must be in place before the
            // release store on goto_address publishes it: the parked CPU observes that store
            // with acquire semantics.
            ref PerCpuBlock block = ref PerCpuBlock.At(index);
            block.Self = (nuint)Unsafe.AsPointer(ref block);
            block.CpuIndex = index;
            block.CurrentExInfoHead = idleThread.ExInfoHeadSlot;

            Serial.WriteString("[SMP] Starting CPU ");
            Serial.WriteNumber(index);
            Serial.WriteString(" (hw id 0x");
            Serial.WriteHex(hardwareId);
            Serial.WriteString(")\n");

            *extraArgument = block.Self;
            Volatile.Write(ref s_releaseTimestamp, Stopwatch.GetTimestamp());
            Volatile.Write(ref *gotoAddress, trampoline);

            long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * StartTimeoutSeconds;
            while (Volatile.Read(ref block.Online) == 0 && Stopwatch.GetTimestamp() < deadline)
            {
            }

            if (Volatile.Read(ref block.Online) == 0)
            {
                Serial.WriteString("[SMP] CPU ");
                Serial.WriteNumber(index);
                Serial.WriteString(" did not come online\n");
                Panic.Halt("An application processor the bootloader listed did not come online.");
            }
        }
    }

    /// <summary>
    /// What an application processor runs, on its boot stack, with interrupts masked, once the
    /// trampoline gave it its per-CPU register: its idle thread is already its current thread
    /// (the boot CPU set every CPU's up), so thread statics, allocation and class constructors
    /// work. It records its boot stack top, brings up its share of the interrupt hardware,
    /// logs its counter against the boot CPU's, starts its tick, reports online, enables
    /// interrupts and halts; its first tick finds nothing queued and keeps halting, and the
    /// first thread placed on it (phase 5) arrives by a reschedule interrupt. Nothing here
    /// allocates: the heap has no lock across CPUs until phase 4, and a tick on this CPU must
    /// not allocate either.
    /// </summary>
    /// <param name="bootStackTop">The top of this CPU's boot stack, as the trampoline aligned it.</param>
    internal static void ApMain(nuint bootStackTop)
    {
        ref PerCpuBlock block = ref PerCpuBlock.Current;
        block.BootStackTop = bootStackTop;

        // This CPU's IDT load or vector table, its Local APIC or its GIC redistributor and CPU
        // interface, the lines of its timer and its reschedule interrupt.
        InterruptManager.InitializeCurrentCpu();

        long skew = Stopwatch.GetTimestamp() - Volatile.Read(ref s_releaseTimestamp);

        Serial.WriteString("[SMP] CPU ");
        Serial.WriteNumber(block.CpuIndex);
        Serial.WriteString(" up, hw id 0x");
        Serial.WriteHex(block.HardwareId);
        Serial.WriteString(", counter skew ");
        Serial.WriteNumber(skew);
        Serial.WriteString(" ticks\n");

        s_startTickOnCurrentCpu?.Invoke();

        // This CPU runs its idle thread from here on: mark it on-CPU before a switch can stage
        // its clear, as SetupIdleThread does for the boot CPU.
        SchedulerThread? idle = SchedulerManager.GetCpuState((uint)block.CpuIndex)?.IdleThread;
        if (idle is not null)
        {
            Volatile.Write(ref idle._onCpu, 1);
        }

        Volatile.Write(ref block.Online, 1);

        InternalCpu.EnableInterrupts();

        while (true)
        {
            InternalCpu.Halt();
        }
    }

    /// <summary>The MP response's count, 0 when there is no response.</summary>
    private static ulong BootloaderCpuCount
    {
        get
        {
#if ARCH_ARM64
            LimineMpResponseArm64* response = (LimineMpResponseArm64*)Limine.Mp.Response;
#else
            LimineMpResponseX64* response = (LimineMpResponseX64*)Limine.Mp.Response;
#endif
            return response == null ? 0 : response->CpuCount;
        }
    }

    /// <summary>
    /// The bootloader's entry for CPU <paramref name="index"/>: the boot CPU's for 0, wherever
    /// the list puts it, then the others in the list's order. Hands out the two fields the kernel
    /// writes and the identity it reads.
    /// </summary>
    private static bool TryGetEntry(uint index, out ulong* extraArgument, out nuint* gotoAddress, out ulong hardwareId)
    {
        extraArgument = null;
        gotoAddress = null;
        hardwareId = 0;

#if ARCH_ARM64
        LimineMpResponseArm64* response = (LimineMpResponseArm64*)Limine.Mp.Response;
#else
        LimineMpResponseX64* response = (LimineMpResponseX64*)Limine.Mp.Response;
#endif
        if (response == null)
        {
            return false;
        }

        uint next = 1;
        for (ulong i = 0; i < response->CpuCount; i++)
        {
#if ARCH_ARM64
            LimineMpInfoArm64* entry = response->Cpus[i];
            ulong id = entry->Mpidr;
            bool isBoot = entry->Mpidr == response->BspMpidr;
#else
            LimineMpInfoX64* entry = response->Cpus[i];
            ulong id = entry->LapicId;
            bool isBoot = entry->LapicId == response->BspLapicId;
#endif
            uint entryIndex;
            if (isBoot)
            {
                entryIndex = 0;
            }
            else
            {
                entryIndex = next;
                next++;
            }

            if (entryIndex == index)
            {
                extraArgument = &entry->ExtraArgument;
                gotoAddress = &entry->GotoAddress;
                hardwareId = id;
                return true;
            }
        }

        return false;
    }
}
