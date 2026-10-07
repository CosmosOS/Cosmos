// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.Core.X64.Bridge;

namespace Cosmos.Kernel.Core.X64.Cpu;

/// <summary>
/// Local APIC (Advanced Programmable Interrupt Controller) driver.
/// Each CPU has its own Local APIC for receiving interrupts.
/// </summary>
public static class LocalApic
{
    // Local APIC register offsets (from base address)
    private const uint LAPIC_ID = 0x20;             // Local APIC ID
    private const uint LAPIC_VERSION = 0x30;        // Local APIC Version
    private const uint LAPIC_TPR = 0x80;            // Task Priority Register
    private const uint LAPIC_EOI = 0xB0;            // End of Interrupt
    private const uint LAPIC_SVR = 0xF0;            // Spurious Interrupt Vector Register
    private const uint LAPIC_ISR1 = 0x110;          // In-Service Register 1: vectors 32-63 (ISR spans 0x100-0x170)
    private const uint LAPIC_IRR1 = 0x210;          // Interrupt Request Register 1: vectors 32-63 (IRR spans 0x200-0x270)
    private const uint LAPIC_ESR = 0x280;           // Error Status Register
    private const uint LAPIC_ICR_LOW = 0x300;       // Interrupt Command Register (low)
    private const uint LAPIC_ICR_HIGH = 0x310;      // Interrupt Command Register (high)
    private const uint LAPIC_TIMER_LVT = 0x320;     // LVT Timer Register
    private const uint LAPIC_THERMAL_LVT = 0x330;   // LVT Thermal Sensor Register
    private const uint LAPIC_PERF_LVT = 0x340;      // LVT Performance Counter Register
    private const uint LAPIC_LINT0 = 0x350;         // LVT LINT0 Register
    private const uint LAPIC_LINT1 = 0x360;         // LVT LINT1 Register
    private const uint LAPIC_ERROR_LVT = 0x370;     // LVT Error Register
    private const uint LAPIC_TIMER_INIT = 0x380;    // Timer Initial Count Register
    private const uint LAPIC_TIMER_CURRENT = 0x390; // Timer Current Count Register
    private const uint LAPIC_TIMER_DIVIDE = 0x3E0;  // Timer Divide Configuration Register

    // Register bits and fields
    /// <summary>Shift to extract the APIC ID from the Local APIC ID register (bits 31:24, Intel SDM Vol 3 §10.4.6).</summary>
    private const int APIC_ID_SHIFT = 24;
    /// <summary>ICR destination shorthand "self" (bits 19:18 = 01b, Intel SDM Vol 3 §10.6.1).</summary>
    private const uint ICR_DEST_SHORTHAND_SELF = 0b01u << 18;
    /// <summary>LVT mask bit (bit 16) - masks delivery of the LVT entry's interrupt (Intel SDM Vol 3 §10.5.1).</summary>
    private const uint LVT_MASKED = 0x10000;
    /// <summary>Timer LVT periodic mode bit (bit 17) - periodic vs one-shot.</summary>
    private const uint TIMER_PERIODIC = 0x20000;
    /// <summary>SVR APIC Software Enable bit (bit 8).</summary>
    private const uint SVR_ENABLE = 0x100;

    // Interrupt vector assignments
    public const byte TIMER_VECTOR = 0xEF;        // Timer interrupt vector (239) - separate from IRQ remapping range
    // Public: the dispatch path must recognize spurious deliveries — the
    // SDM forbids EOI for them (no ISR bit is set, so an EOI would retire
    // whatever real interrupt is currently in service instead).
    public const byte SPURIOUS_VECTOR = 0xFF;     // Spurious interrupt vector

    // Timer divide encodings (divide by 1, 2, 4, 8, 16, 32, 64, 128)
    private const uint TIMER_DIVIDE_BY_1 = 0xB;
    private const uint TIMER_DIVIDE_BY_16 = 0x3;

    // PIT calibration interface
    private const ushort PIT_CHANNEL0_DATA = 0x40;
    private const ushort PIT_COMMAND = 0x43;
    private const uint PIT_FREQUENCY = 1193182;   // PIT base frequency in Hz
    /// <summary>PIT command byte: channel 0, lobyte/hibyte access, mode 0 (one-shot), binary counting.</summary>
    private const byte PIT_CMD_CHANNEL0_ONESHOT = 0x30;
    /// <summary>PIT command byte: latch the current count of channel 0 for read-back.</summary>
    private const byte PIT_CMD_LATCH_CHANNEL0 = 0x00;

    // Discovery
    /// <summary>IA32_APIC_BASE: the Local APIC's physical base and its global enable bit (Intel SDM Vol 3 §11.4.4).</summary>
    private const uint Ia32ApicBaseMsr = 0x1B;
    /// <summary>IA32_APIC_BASE bit 11, APIC global enable: clear when firmware left the Local APIC off.</summary>
    private const ulong ApicBaseGlobalEnable = 1UL << 11;
    /// <summary>IA32_APIC_BASE bits 51:12, the 4 KiB-aligned physical base of the register page.</summary>
    private const ulong ApicBaseAddressMask = 0x000F_FFFF_FFFF_F000;
    /// <summary>CPUID leaf 1, the processor feature flags.</summary>
    private const int CpuidFeatureLeaf = 1;
    /// <summary>CPUID.01H:EDX bit 9: an on-chip APIC, and with it the IA32_APIC_BASE MSR.</summary>
    private const int CpuidEdxApic = 1 << 9;

    // Software policy
    /// <summary>Mask selecting the low byte of a 16-bit PIT count.</summary>
    private const int LowByteMask = 0xFF;
    /// <summary>Shift between the low and high bytes of a 16-bit PIT count.</summary>
    private const int ByteShift = 8;
    /// <summary>Duration of the PIT-referenced calibration window in milliseconds.</summary>
    private const uint CalibrationDurationMs = 10;
    /// <summary>Maximum LAPIC timer initial count (32-bit register, all bits set) used during calibration.</summary>
    private const uint TimerMaxInitialCount = 0xFFFFFFFF;
    /// <summary>Number of initial timer ticks that are always logged.</summary>
    private const uint TimerLogInitialTicks = 5;
    /// <summary>Period, in ticks, of the recurring timer-tick log message.</summary>
    private const uint TimerLogPeriodTicks = 100;

    private static ulong s_baseAddress;
    private static bool s_initialized;
    private static uint s_ticksPerMs;
    private static bool s_timerCalibrated;
    private static uint s_timerIntervalMs;
    private static ulong s_timerIntervalNs;

    /// <summary>
    /// Gets the base address of the Local APIC.
    /// </summary>
    public static ulong BaseAddress => s_baseAddress;

    /// <summary>
    /// Gets whether the Local APIC is initialized.
    /// </summary>
    public static bool IsInitialized => s_initialized;

    /// <summary>
    /// Reads the Local APIC's physical base from IA32_APIC_BASE. The register
    /// is architectural, so it names the Local APIC on every boot, with or
    /// without ACPI; the MADT's Local APIC address only repeats it. False when
    /// the CPU reports no APIC (the MSR does not exist then) or firmware left
    /// the Local APIC globally disabled. Allocation-free.
    /// </summary>
    /// <param name="baseAddress">The physical address of the register page, or 0.</param>
    internal static bool TryReadBaseAddress(out ulong baseAddress)
    {
        baseAddress = 0;
        if ((X86Base.CpuId(CpuidFeatureLeaf, 0).Edx & CpuidEdxApic) == 0)
        {
            return false;
        }

        ulong apicBase = X64CpuNative.ReadMsr(Ia32ApicBaseMsr);
        if ((apicBase & ApicBaseGlobalEnable) == 0)
        {
            return false;
        }

        baseAddress = apicBase & ApicBaseAddressMask;
        return true;
    }

    /// <summary>
    /// Initializes the Local APIC at the base <see cref="TryReadBaseAddress"/> read.
    /// </summary>
    /// <param name="baseAddress">The physical address of the Local APIC registers.</param>
    public static void Initialize(ulong baseAddress)
    {
        s_baseAddress = baseAddress;

        Serial.Write("[LocalAPIC] Initializing at 0x", baseAddress.ToString("X"), "\n");

        // Read APIC ID and version for diagnostics
        uint id = Read(LAPIC_ID);
        uint version = Read(LAPIC_VERSION);

        Serial.Write("[LocalAPIC] ID: ", (id >> APIC_ID_SHIFT), "\n");
        Serial.Write("[LocalAPIC] Version: 0x", version.ToString("X"), "\n");

        // Clear error status register (requires two writes)
        Write(LAPIC_ESR, 0);
        Write(LAPIC_ESR, 0);

        // Set Task Priority to 0 to accept all interrupts
        Write(LAPIC_TPR, 0);

        // Configure spurious interrupt vector and enable APIC
        uint svr = SPURIOUS_VECTOR | SVR_ENABLE;
        Write(LAPIC_SVR, svr);

        Serial.Write("[LocalAPIC] SVR set to 0x", svr.ToString("X"), "\n");

        // Mask LINT0 and LINT1 (we use I/O APIC for external interrupts)
        Write(LAPIC_LINT0, LVT_MASKED);  // Masked
        Write(LAPIC_LINT1, LVT_MASKED);  // Masked

        // Mask timer and error LVT entries
        Write(LAPIC_TIMER_LVT, LVT_MASKED);  // Masked
        Write(LAPIC_ERROR_LVT, LVT_MASKED);  // Masked

        s_initialized = true;

        // Register the x64 MSI binder for HAL-level PCI MSI-X code. Intel
        // SDM Vol 3 §10.11: address [31:20]=0xFEE, [19:12]=destination APIC
        // ID (physical, no redirection); data low byte = IDT vector,
        // delivery mode = fixed (0), trigger = edge. No per-device state.
        MsiRouting.RegisterBinder(new X64MsiBinder());

        Serial.Write("[LocalAPIC] Initialization complete\n");
    }

    /// <summary>
    /// Sends End of Interrupt signal to the Local APIC.
    /// Must be called at the end of every interrupt handler.
    /// </summary>
    public static void SendEOI()
    {
        if (s_initialized)
        {
            Write(LAPIC_EOI, 0);
        }
    }

    /// <summary>
    /// Sends a fixed-delivery IPI to this CPU itself on the given vector
    /// (ICR destination shorthand "self", so ICR high is untouched). Runs
    /// the full hardware interrupt path — IDT stub, dispatch, EOI —
    /// without any external device; tests use it to exercise ISR-side
    /// code on demand.
    /// </summary>
    public static void SendSelfIpi(byte vector)
    {
        if (s_initialized)
        {
            Write(LAPIC_ICR_LOW, vector | ICR_DEST_SHORTHAND_SELF);
        }
    }

    /// <summary>
    /// Reads the In-Service Register to check which interrupts are being serviced.
    /// Returns the ISR value for vectors 32-63 (ISR1).
    /// </summary>
    public static uint GetISR1()
    {
        // ISR is at offset 0x100-0x170 (8 32-bit registers covering vectors 0-255)
        // ISR1 at 0x110 covers vectors 32-63
        return Read(LAPIC_ISR1);
    }

    /// <summary>
    /// Reads the Interrupt Request Register to check pending interrupts.
    /// Returns the IRR value for vectors 32-63 (IRR1).
    /// </summary>
    public static uint GetIRR1()
    {
        // IRR is at offset 0x200-0x270 (8 32-bit registers covering vectors 0-255)
        // IRR1 at 0x210 covers vectors 32-63
        return Read(LAPIC_IRR1);
    }

    /// <summary>
    /// Gets the current Local APIC ID.
    /// </summary>
    public static byte GetId()
    {
        return (byte)(Read(LAPIC_ID) >> APIC_ID_SHIFT);
    }

    /// <summary>
    /// Reads a 32-bit value from a Local APIC register.
    /// </summary>
    private static uint Read(uint offset)
    {
        return Native.MMIO.Read32(s_baseAddress + offset);
    }

    /// <summary>
    /// Writes a 32-bit value to a Local APIC register.
    /// </summary>
    private static void Write(uint offset, uint value)
    {
        Native.MMIO.Write32(s_baseAddress + offset, value);
    }

    /// <summary>
    /// Gets whether the LAPIC timer has been calibrated.
    /// </summary>
    public static bool IsTimerCalibrated => s_timerCalibrated;

    /// <summary>
    /// Gets the calibrated ticks per millisecond.
    /// </summary>
    public static uint TicksPerMs => s_ticksPerMs;

    /// <summary>
    /// Gets the current timer interval in nanoseconds.
    /// </summary>
    public static ulong TimerIntervalNs => s_timerIntervalNs;

    /// <summary>
    /// Calibrates the LAPIC timer using the PIT as a reference.
    /// Must be called after Initialize() and before using timer functions.
    /// </summary>
    public static void CalibrateTimer()
    {
        if (!s_initialized)
        {
            Serial.Write("[LocalAPIC] ERROR: Cannot calibrate timer - APIC not initialized\n");
            return;
        }

        Serial.Write("[LocalAPIC] Calibrating timer using PIT...\n");

        // Set timer divide to 16
        Write(LAPIC_TIMER_DIVIDE, TIMER_DIVIDE_BY_16);

        // Configure PIT channel 0 for one-shot mode, ~10ms delay
        // 10ms = 11932 ticks at 1193182 Hz
        const ushort pitCount = 11932;

        // PIT command: channel 0, lobyte/hibyte, one-shot mode, binary
        Native.IO.Write8(PIT_COMMAND, PIT_CMD_CHANNEL0_ONESHOT);
        Native.IO.Write8(PIT_CHANNEL0_DATA, (byte)(pitCount & LowByteMask));
        Native.IO.Write8(PIT_CHANNEL0_DATA, (byte)(pitCount >> ByteShift));

        // Set LAPIC timer to max initial count (one-shot, masked)
        Write(LAPIC_TIMER_LVT, LVT_MASKED);
        Write(LAPIC_TIMER_INIT, TimerMaxInitialCount);

        // Wait for PIT to count down by polling
        // Read back current count until it wraps or reaches near zero
        ushort lastCount = pitCount;
        while (true)
        {
            // Latch count for channel 0
            Native.IO.Write8(PIT_COMMAND, PIT_CMD_LATCH_CHANNEL0);
            byte lo = Native.IO.Read8(PIT_CHANNEL0_DATA);
            byte hi = Native.IO.Read8(PIT_CHANNEL0_DATA);
            ushort currentCount = (ushort)(lo | (hi << ByteShift));

            // PIT counts down, check if we've passed our target
            if (currentCount > lastCount || currentCount == 0)
            {
                break;
            }

            lastCount = currentCount;
        }

        // Read how many LAPIC ticks elapsed
        uint lapicTicksElapsed = TimerMaxInitialCount - Read(LAPIC_TIMER_CURRENT);

        // Stop the timer
        Write(LAPIC_TIMER_INIT, 0);

        // Calculate ticks per ms (we waited ~10ms)
        // Account for divide by 16
        s_ticksPerMs = lapicTicksElapsed / CalibrationDurationMs;

        s_timerCalibrated = true;
        Serial.Write("[LocalAPIC] Timer calibrated: ", s_ticksPerMs, " ticks/ms\n");
    }

    /// <summary>
    /// Blocks for the specified number of milliseconds using the LAPIC timer.
    /// </summary>
    /// <param name="ms">Number of milliseconds to wait.</param>
    public static void Wait(uint ms)
    {
        if (!s_timerCalibrated)
        {
            Serial.Write("[LocalAPIC] ERROR: Timer not calibrated\n");
            return;
        }

        if (ms == 0)
        {
            return;
        }

        // Set timer divide to 16 (same as calibration)
        Write(LAPIC_TIMER_DIVIDE, TIMER_DIVIDE_BY_16);

        // Calculate ticks needed
        uint ticks = s_ticksPerMs * ms;

        // Set up one-shot timer (masked - we poll instead of interrupt)
        Write(LAPIC_TIMER_LVT, LVT_MASKED);
        Write(LAPIC_TIMER_INIT, ticks);

        // Poll until timer reaches zero
        while (Read(LAPIC_TIMER_CURRENT) > 0)
        {
            // Busy wait
        }

        // Re-arm the periodic scheduler tick this wait hijacked: the LVT was
        // masked and switched to one-shot above, and without this the
        // scheduler tick stays dead after the first Wait call.
        if (s_timerIntervalMs != 0)
        {
            Write(LAPIC_TIMER_LVT, TIMER_PERIODIC | TIMER_VECTOR);
            Write(LAPIC_TIMER_INIT, s_ticksPerMs * s_timerIntervalMs);
        }
    }

    /// <summary>
    /// Starts the LAPIC timer in periodic mode with the given interval.
    /// Will fire interrupt on TIMER_VECTOR (0xEF = 239).
    /// </summary>
    /// <param name="intervalMs">Interval in milliseconds between interrupts.</param>
    public static void StartPeriodicTimer(uint intervalMs)
    {
        if (!s_timerCalibrated)
        {
            Serial.Write("[LocalAPIC] ERROR: Timer not calibrated\n");
            return;
        }

        uint ticks = s_ticksPerMs * intervalMs;
        s_timerIntervalMs = intervalMs;
        s_timerIntervalNs = (ulong)intervalMs * SchedulerManager.NanosecondsPerMillisecond;  // Convert ms to ns

        Serial.Write("[LocalAPIC] Starting periodic timer:\n");
        Serial.Write("[LocalAPIC]   TicksPerMs: ", s_ticksPerMs, "\n");
        Serial.Write("[LocalAPIC]   Interval: ", intervalMs, "ms\n");
        Serial.Write("[LocalAPIC]   Ticks: ", ticks, "\n");
        Serial.Write("[LocalAPIC]   Vector: 0x", TIMER_VECTOR.ToString("X"), "\n");

        // Set timer divide to 16
        Write(LAPIC_TIMER_DIVIDE, TIMER_DIVIDE_BY_16);

        // Configure timer: periodic mode, unmasked, vector 0xEF
        Write(LAPIC_TIMER_LVT, TIMER_PERIODIC | TIMER_VECTOR);

        // Set initial count to start the timer
        Write(LAPIC_TIMER_INIT, ticks);
    }

    /// <summary>
    /// Stops the LAPIC timer.
    /// </summary>
    public static void StopTimer()
    {
        // Mask the timer and set count to 0
        Write(LAPIC_TIMER_LVT, LVT_MASKED);
        Write(LAPIC_TIMER_INIT, 0);
    }

    /// <summary>
    /// Registers the LAPIC timer interrupt handler.
    /// Should be called after InterruptManager is initialized.
    /// </summary>
    public static void RegisterTimerHandler()
    {
        Serial.Write("[LocalAPIC] Registering timer handler for vector 0x", TIMER_VECTOR.ToString("X"), "\n");
        InterruptManager.SetHandler(TIMER_VECTOR, HandleTimerInterrupt);
    }

    /// <summary>
    /// LAPIC timer interrupt handler - triggers scheduler for preemptive multitasking.
    /// </summary>
    private static uint s_timerTickCount;
    private static unsafe void HandleTimerInterrupt(ref IRQContext context)
    {
        s_timerTickCount++;

        // Get current CPU ID from APIC
        uint cpuId = (uint)GetId();

        // Calculate RSP pointing to saved context for context switching
        nuint contextPtr = (nuint)Unsafe.AsPointer(ref context);
        nuint currentRsp = contextPtr - X64InterruptController.XmmSaveAreaSizeBytes;  // RSP points to start of XMM save area

        // Sanity check RSP - should be in kernel space (0xFFFF800000000000+)
        if ((currentRsp & AddressSpace.KernelSpaceCanonicalMask) != AddressSpace.KernelSpaceCanonicalMask)
        {
            Serial.Write("[LAPIC] ERROR: Invalid RSP at tick ", s_timerTickCount, ": ");
            Serial.WriteHex((ulong)currentRsp);
            Serial.Write("\n");
            return;  // Don't call scheduler with bad RSP
        }

        // Log first few and then periodically
        if (s_timerTickCount <= TimerLogInitialTicks || s_timerTickCount % TimerLogPeriodTicks == 0)
        {
            Serial.Write("[LAPIC] Timer tick ", s_timerTickCount, " RSP=");
            Serial.WriteHex((ulong)currentRsp);
            Serial.Write("\n");
        }

        // Call scheduler with elapsed time
        SchedulerManager.OnTimerInterrupt(cpuId, currentRsp, s_timerIntervalNs);

        // EOI is sent by InterruptManager.Dispatch after handler returns
    }
}
