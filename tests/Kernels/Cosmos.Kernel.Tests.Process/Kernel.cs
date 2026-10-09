using System;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Memory.VAS;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.System.Timer;
using Cosmos.TestRunner.Framework;
using TR = Cosmos.TestRunner.Framework.TestRunner;
using Sys = Cosmos.Kernel.System;
using SchedProcess = Cosmos.Kernel.Core.Scheduler.Process;
using SchedThread = Cosmos.Kernel.Core.Scheduler.SchedulerThread;
using SchedThreadState = Cosmos.Kernel.Core.Scheduler.SchedulerThreadState;

#if ARCH_X64
using Cosmos.Kernel.Core.X64.Cpu;
#endif

namespace Cosmos.Kernel.Tests.Process;

/// <summary>
/// Test kernel demonstrating manual construction of a ring-0 user process.
/// The kernel builds the address space, maps pages, and creates a thread by hand —
/// there is no built-in loader.
/// </summary>
public unsafe class Kernel : Sys.Kernel
{
    private const int ExpectedTestCount = 1;

    /// <summary>
    /// User code virtual address (RX).
    /// </summary>
    private const nuint UserCodeVa = 0x400000;

    /// <summary>
    /// User data virtual address (RW). Shared counter lives here.
    /// </summary>
    private const nuint UserDataVa = 0x401000;

    /// <summary>
    /// Top of the user stack.
    /// </summary>
    private static readonly nuint UserStackTop = unchecked((nuint)0x00007FFFFFFFF000);

    /// <summary>
    /// User stack size in pages (64 KiB).
    /// </summary>
    private const ulong UserStackPages = 16;

    /// <summary>
    /// x64 native code that increments the counter at 0x401000 and loops forever.
    /// </summary>
    private static readonly byte[] s_x64TestCode =
    [
        0x48, 0x8B, 0x04, 0x25, 0x00, 0x10, 0x40, 0x00, // mov rax, [0x401000]
        0x48, 0xFF, 0xC0,                                  // inc rax
        0x48, 0x89, 0x04, 0x25, 0x00, 0x10, 0x40, 0x00, // mov [0x401000], rax
        0xEB, 0xFE                                         // jmp $
    ];

    /// <summary>
    /// ARM64 native code that increments the counter at 0x401000 and loops forever.
    /// </summary>
    private static readonly byte[] s_arm64TestCode =
    [
        0x01, 0x08, 0xA0, 0xD2, // movz x1, #0x40, lsl #16   (x1 = 0x400000)
        0x01, 0x00, 0x82, 0xF2, // movk x1, #0x1000          (x1 = 0x401000)
        0x20, 0x00, 0x40, 0xF9, // ldr x0, [x1]
        0x00, 0x04, 0x00, 0x91, // add x0, x0, #1
        0x20, 0x00, 0x00, 0xF9, // str x0, [x1]
        0x00, 0x00, 0x00, 0x14  // b .
    ];

    protected override void BeforeRun()
    {
        Serial.WriteString("[ProcessTest] BeforeRun() reached\n");

        TR.Start("Process Tests", expectedTests: ExpectedTestCount);
        TR.Run("RawProcess_IncrementCounter", TestRawProcessIncrementCounter);
        TR.Finish();

        Serial.WriteString("\n[ProcessTest] Tests complete - halting\n");
    }

    protected override void Run()
    {
        // All tests ran in BeforeRun.
        Stop();
    }

    protected override void AfterRun()
    {
        TR.Complete();
        Cosmos.Kernel.System.Power.Halt();
    }

    private static void TestRawProcessIncrementCounter()
    {
        Serial.WriteString("[ProcessTest] Building process manually...\n");

        // 1. Capture the kernel address space (already initialized at boot).
        AddressSpace kernelSpace = AddressSpace.KernelSpace
            ?? throw new InvalidOperationException("Kernel address space not initialized");

        // 2. Create a new address space sharing only the kernel/higher-half mappings.
        AddressSpace procSpace = AddressSpace.CloneHigherHalf()
            ?? throw new InvalidOperationException("Failed to clone kernel address space");

        // 3. Allocate physical backing pages.
        byte* codePages = (byte*)PageAllocator.AllocPages(PageType.Unmanaged, 1);
        byte* dataPages = (byte*)PageAllocator.AllocPages(PageType.Unmanaged, 1);
        byte* stackPages = (byte*)PageAllocator.AllocPages(PageType.Unmanaged, UserStackPages);

        if (codePages == null || dataPages == null || stackPages == null)
        {
            throw new InvalidOperationException("Failed to allocate process pages");
        }

        // Clear pages.
        MemoryOp.MemSet(codePages, 0, 4096);
        MemoryOp.MemSet(dataPages, 0, 4096);
        MemoryOp.MemSet(stackPages, 0, (int)(UserStackPages * PageAllocator.PageSize));

        procSpace.Map(
            UserCodeVa,
            PageAllocator.VirtualToPhysical((ulong)codePages),
            1,
            PageFlags.Read | PageFlags.Execute);

        procSpace.Map(
            UserDataVa + PageAllocator.PageSize,
            PageAllocator.VirtualToPhysical((ulong)dataPages),
            1,
            PageFlags.Read | PageFlags.Write);

        procSpace.Map(
            UserStackTop - (UserStackPages * PageAllocator.PageSize),
            PageAllocator.VirtualToPhysical((ulong)stackPages),
            UserStackPages,
            PageFlags.Read | PageFlags.Write);

        // 5. Copy the architecture-specific test code into the code page.
        byte[] testCode = GetTestCode();
        fixed (byte* src = testCode)
        {
            MemoryOp.MemCopy(codePages, src, (int)testCode.Length);
        }

        // 6. Build the process object.
        SchedProcess process = new SchedProcess
        {
            Id = ProcessManager.AllocateId(),
            AddressSpace = procSpace,
            Ring = 0
        };

        // 7. Build a native thread inside the process.
        SchedThread thread = new SchedThread
        {
            Id = SchedulerManager.AllocateThreadId(),
            CpuId = 0,
            State = SchedThreadState.Created,
            AddressSpace = procSpace,
            Flags = SchedulerThreadFlags.NativeProcess
        };

        nuint entryPoint = UserCodeVa;

#if ARCH_X64
        ushort cs = (ushort)Idt.GetCurrentCodeSelector();
#else
        ushort cs = 0;
#endif

        thread.InitializeStack(
            entryPoint,
            cs,
            arg: 0,
            stackSize: (nuint)(UserStackPages * PageAllocator.PageSize));

        // User threads run in the process address space, so their stack pointer
        // must be a user virtual address. Override the default HHDM stack top
        // set by InitializeStack.
        ThreadContext* ctx = thread.GetContext();
#if ARCH_X64
        ctx->Rsp = (UserStackTop & ~(nuint)0xF) - 8;
#elif ARCH_ARM64
        ctx->Sp = (UserStackTop & ~(nuint)0xF);
#endif

        process.Threads.Add(thread);

        // 8. Register and run.
        ProcessManager.RegisterProcess(process);
        SchedulerManager.CreateThread(0, thread);
        SchedulerManager.ReadyThread(0, thread);

        Serial.WriteString("[ProcessTest] Process ");
        Serial.WriteNumber(process.Id);
        Serial.WriteString(" thread ");
        Serial.WriteNumber(thread.Id);
        Serial.WriteString(" ready; waiting for counter increment...\n");

        // 9. Poll the shared counter (via the kernel's HHDM alias of the data page).
        ulong* counter = (ulong*)dataPages;
        const int MaxPolls = 100;
        for (int i = 0; i < MaxPolls && *counter == 0; i++)
        {
            TimerManager.Wait(50);
        }

        ulong value = *counter;
        Serial.WriteString("[ProcessTest] Counter value: ");
        Serial.WriteNumber((uint)value);
        Serial.WriteString("\n");

        Assert.True(value > 0, "User process should have incremented the shared counter");
    }

    private static byte[] GetTestCode()
    {
#if ARCH_X64
        return s_x64TestCode;
#elif ARCH_ARM64
        return s_arm64TestCode;
#else
        throw new NotSupportedException("Unknown architecture");
#endif
    }
}
