using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.System.Timer;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using Monitor = System.Threading.Monitor;
using SysThread = System.Threading.Thread;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.Threading;

public class Kernel : Sys.Kernel
{
    /// <summary>Total number of tests announced to the test runner for this suite.</summary>
    private const int ExpectedTestCount = 73;

    /// <summary>Lock/unlock increment iterations each worker thread performs in the lock and spinlock contention tests.</summary>
    private const int LockIterationsPerThread = 100;
    /// <summary>Expected final counter value after two workers each complete LockIterationsPerThread increments.</summary>
    private const int ExpectedTotalIncrements = 200;
    /// <summary>Increment iterations each worker performs in the multiple-threads test.</summary>
    private const int WorkerIterationCount = 5;
    /// <summary>Number of contender threads racing for the mutex in the three-contenders test.</summary>
    private const int ContenderCount = 3;

    /// <summary>Initial wait (ms) for a freshly started thread to be scheduled and run.</summary>
    private const int ThreadStartupWaitMs = 1000;
    /// <summary>Initial wait (ms) for both worker threads of the multiple-threads test to complete.</summary>
    private const int ThreadsCompletionWaitMs = 3000;
    /// <summary>Initial wait (ms) for the lock/spinlock contention workers to finish their iterations.</summary>
    private const int LockTestInitialWaitMs = 5000;
    /// <summary>Extra per-retry wait (ms) when a test result is not yet visible after the initial wait.</summary>
    private const int RetryWaitMs = 500;
    /// <summary>Maximum number of extra RetryWaitMs waits before giving up on a counter reaching its target.</summary>
    private const int MaxExtraWaitRetries = 10;
    /// <summary>Per-check delay (ms) while polling for the thread-execution flag.</summary>
    private const int ThreadPollWaitMs = 200;
    /// <summary>Maximum number of ThreadPollWaitMs checks for the thread-execution flag.</summary>
    private const int ThreadExecPollRetries = 5;

    /// <summary>Polling interval (ms) while waiting on scheduler-test flags (worker holding, parked, woke, ...).</summary>
    private const int FlagPollIntervalMs = 50;
    /// <summary>Maximum number of FlagPollIntervalMs polls while waiting on a scheduler-test flag.</summary>
    private const int FlagPollRetries = 100;
    /// <summary>Maximum number of FlagPollIntervalMs polls while waiting for all mutex contenders to acquire.</summary>
    private const int ContenderPollRetries = 200;
    /// <summary>Grace wait (ms) letting worker-thread exit paths finish inside the current test cell.</summary>
    private const int ExitGraceWaitMs = 200;
    /// <summary>Hold time (ms) the idle-contention worker keeps the mutex, spanning several scheduler ticks.</summary>
    private const int IdleMutexHoldMs = 300;
    /// <summary>Hold time (ms) each contender keeps the mutex so the others pile up in _waitingThreads.</summary>
    private const int MutexContenderHoldMs = 100;
    /// <summary>Wait (ms) giving the hand-off contender a few quanta to park in _waitingThreads.</summary>
    private const int ContenderParkWaitMs = 150;

    /// <summary>Polling interval (ms) while waiting for a ThreadPool/Task/async result to complete.</summary>
    private const int TaskPollIntervalMs = 100;
    /// <summary>Maximum number of TaskPollIntervalMs polls for a ThreadPool/Task/async result.</summary>
    private const int TaskPollRetries = 30;
    /// <summary>Wait (ms) for the second thread of the thread-statics test to finish.</summary>
    private const int ThreadStaticsWaitMs = 100;
    /// <summary>Delay (ms) between increments in each multiple-threads worker iteration.</summary>
    private const int WorkerStepDelayMs = 50;

    /// <summary>Duration (ms) of each CPU-share measurement window in the scheduling-policy tests.</summary>
    private const int PolicyMeasureMs = 1500;
    /// <summary>Interval (ms) between the spinner-progress samples of the quantum-preemption test.</summary>
    private const int PreemptSampleIntervalMs = 100;
    /// <summary>Iteration mask deciding how often a measured spinner re-checks its stop flag.</summary>
    private const ulong SpinStopCheckMask = 0xFFF;
    /// <summary>Stride tickets given to the favored spinner of the proportional-share tests.</summary>
    private const long HighTickets = 400;
    /// <summary>Stride tickets given to the other spinner (the Stride default).</summary>
    private const long LowTickets = 100;
    // The two policies are separated by one ratio, applied from both sides:
    // the 4x-ticket spinner must clear it under Stride, and neither spinner may
    // reach it under Round-Robin. 3/2 sits in the gap with margin on both
    // arches — measured A/B was 1.92 (arm64 TCG) to 2.20 (x64) under Stride,
    // and 0.88 to 1.13 under Round-Robin whatever priority was requested.
    // Stride's ideal is 4x; the OnThreadYield pass floor erodes it to ~2x.
    /// <summary>Numerator of the share ratio separating the two policies.</summary>
    private const ulong PolicySkewNumerator = 3;
    /// <summary>Denominator of the share ratio separating the two policies.</summary>
    private const ulong PolicySkewDenominator = 2;
    /// <summary>Number of probe threads in the Round-Robin FIFO-order test.</summary>
    private const int FifoWorkerCount = 3;
    /// <summary>Run-queue index far beyond any population this suite creates; must read as null.</summary>
    private const int OutOfRangeQueueIndex = 200;

    /// <summary>Barge-probe result: the releaser's TryAcquire outcome has not been recorded yet.</summary>
    private const int BargeResultPending = -1;
    /// <summary>Barge-probe result: the releaser's immediate TryAcquire failed (ownership was handed off).</summary>
    private const int BargeResultNoBarge = 0;
    /// <summary>Barge-probe result: the releaser's immediate TryAcquire re-took the mutex (barged in).</summary>
    private const int BargeResultBarged = 1;

    // Shared state for thread tests
    private static volatile bool _threadExecuted;
    private static volatile int _sharedCounter;
    private static volatile int _thread1Counter;
    private static volatile int _thread2Counter;
    private static Cosmos.Kernel.Core.Scheduler.SpinLock _testLock;

    // Shared state for Monitor/lock tests
    private static readonly object _lockObj = new object();
    private static volatile int _lockCounter;

    // Custom delegate types for delegate tests
    private delegate void VoidDelegate();
    private delegate int BinaryIntDelegate(int a, int b);

    protected override void BeforeRun()
    {
        Serial.WriteString("[Threading] BeforeRun() reached!\n");
        Serial.WriteString("[Threading] Starting tests...\n");

        TR.Start("Threading Tests", expectedTests: ExpectedTestCount);

        // SpinLock tests
        TR.Run("SpinLock_InitialState_IsUnlocked", TestSpinLockInitialState);
        TR.Run("SpinLock_Acquire_SetsLockedState", TestSpinLockAcquire);
        TR.Run("SpinLock_Release_ClearsLockedState", TestSpinLockRelease);
        TR.Run("SpinLock_TryAcquire_SucceedsOnUnlocked", TestSpinLockTryAcquireSuccess);
        TR.Run("SpinLock_TryAcquire_FailsOnLocked", TestSpinLockTryAcquireFail);

        // Monitor/lock tests
        TR.Run("Monitor_Enter_Exit_BasicLocking", TestMonitorEnterExitBasic);
        TR.Run("Monitor_Enter_Reentrant_SameThread", TestMonitorReentrant);
        TR.Run("Monitor_Enter_RefBool_SetsLockTaken", TestMonitorEnterRefBool);
        TR.Run("Monitor_TryEnter_Succeeds", TestMonitorTryEnter);
        TR.Run("Lock_Statement_BasicExecution", TestLockStatementBasic);
        TR.Run("Lock_Statement_ProtectsSharedData", TestLockProtectsSharedData);
        TR.Run("Lock_Statement_Reentrant", TestLockReentrant);
        TR.Run("Monitor_Exit_WithoutEnter_DoesNotCrash", TestMonitorExitWithoutEnter);

        // Thread tests
        TR.Run("Thread_Start_ExecutesDelegate", TestThreadExecution);
        TR.Run("Thread_Multiple_CanRunConcurrently", TestMultipleThreads);
        TR.Run("SpinLock_ProtectsSharedData_AcrossThreads", TestSpinLockWithThreads);
        TR.Run("Thread_ThreadStatics", TestThreadStatics);
        TR.Run("Thread_EnsureSufficientExecutionStack_Passes", TestEnsureSufficientStackInThread);
        TR.Run("Thread_RecordToString_Works", TestRecordToStringInThread);
        TR.Run("MainThread_RecordToString_Works", TestRecordToStringOnMainThread);
        TR.Run("Thread_MaxStackSize_IsHonored", TestThreadMaxStackSizeHonored);
        TR.Run("Thread_MaxStackSize_TinyRequestIsFloored", TestThreadTinyStackSizeFloored);
        TR.Run("Mutex_IdleThreadContention_KeepsTicketAccounting", TestMutexIdleThreadContention);
        TR.Run("InterruptEvent_TwoWaiters_BothWake", TestInterruptEventTwoWaiters);
        TR.Run("Mutex_ThreeContenders_AllAcquire", TestMutexThreeContenders);
        TR.Run("Mutex_ReleaseHandsOffToParkedWaiter", TestMutexReleaseHandsOff);

        // ThreadPool / Task / async-await tests (validate fix for #245, #246)
        TR.Run("ThreadPool_QueueUserWorkItem_ExecutesCallback", TestThreadPoolQueueUserWorkItem);
        TR.Run("Task_FromResult_IsCompleted", TestTaskFromResult);
        TR.Run("Task_Run_ExecutesAction", TestTaskRunExecutesAction);
        TR.Run("Task_Run_ReturnsResult", TestTaskRunReturnsResult);
        TR.Run("Async_Method_ReturnsValueViaCompletedTask", TestAsyncCompletedTask);
        TR.Run("Async_Await_TaskRun_ReturnsValue", TestAsyncAwaitsTaskRun);
        TR.Run("Async_Chain_PropagatesValue", TestAsyncChain);

        // Delegate tests
        TR.Run("Delegate_Action_BasicInvoke", TestDelegateActionBasicInvoke);
        TR.Run("Delegate_Func_ReturnsValue", TestDelegateFuncReturnsValue);
        TR.Run("Delegate_ActionT_WithParameter", TestDelegateActionWithParameter);
        TR.Run("Delegate_FuncT_Transform", TestDelegateFuncTransform);
        TR.Run("Delegate_CustomType_VoidNoParam", TestDelegateCustomVoid);
        TR.Run("Delegate_CustomType_WithReturn", TestDelegateCustomWithReturn);
        TR.Run("Delegate_StaticMethod", TestDelegateStaticMethod);
        TR.Run("Delegate_InstanceMethod", TestDelegateInstanceMethod);
        TR.Run("Delegate_Multicast_BothCalled", TestDelegateMulticastBothCalled);
        TR.Run("Delegate_Multicast_InvocationOrder", TestDelegateMulticastOrder);
        TR.Run("Delegate_Multicast_Remove", TestDelegateMulticastRemove);
        TR.Run("Delegate_Multicast_GetInvocationList", TestDelegateMulticastGetInvocationList);
        TR.Run("Delegate_Closure_CapturesLocal", TestDelegateClosureCapturesLocal);
        TR.Run("Delegate_Closure_MutableCapture", TestDelegateClosureMutableCapture);
        TR.Run("Delegate_Closure_SharedCapture", TestDelegateClosureSharedCapture);
        TR.Run("Delegate_Null_SafeInvoke", TestDelegateNullSafeInvoke);
        TR.Run("Delegate_Equality_SameMethod", TestDelegateEqualitySameMethod);
        TR.Run("Delegate_Equality_DifferentMethod", TestDelegateEqualityDifferentMethod);
        TR.Run("Delegate_AsParameter", TestDelegateAsParameter);
        TR.Run("Delegate_AsReturnValue", TestDelegateAsReturnValue);
        TR.Run("Delegate_Generic_ValueType", TestDelegateGenericValueType);
        TR.Run("Delegate_Predicate", TestDelegatePredicate);
        TR.Run("Delegate_Comparison", TestDelegateComparison);
        TR.Run("Delegate_Chaining_Pipeline", TestDelegateChaining);
        TR.Run("Delegate_EventPattern_Multicast", TestDelegateEventPattern);

        // Scheduling-policy tests: the default Stride behavior, a live switch
        // to the user-style Round-Robin policy (RoundRobinScheduler.cs), the
        // Round-Robin semantics, and the switch back. Kept last: they replace
        // the installed policy, so nothing else should run between the swaps.
        TR.Run("RoundRobin_Hooks_ReadyTail_PickHead", TestRoundRobinHooksFifoOrder);
        TR.Run("RoundRobin_Hooks_DoubleReady_QueuesOnce", TestRoundRobinHooksReadyIsIdempotent);
        TR.Run("RoundRobin_Hooks_QuantumAccounting", TestRoundRobinHooksQuantumAccounting);
        TR.Run("RoundRobin_Hooks_BlockYieldExit", TestRoundRobinHooksBlockAndYield);
        TR.Run("Scheduler_BootPolicy_IsStride", TestBootPolicyIsStride);
        TR.Run("Stride_ProportionalShare_FollowsTickets", TestStrideProportionalShare);
        TR.Run("SetScheduler_InstallsRoundRobinPolicy", TestSetSchedulerInstallsRoundRobin);
        TR.Run("RoundRobin_ThreadRuns_UnderNewPolicy", TestThreadRunsUnderRoundRobin);
        TR.Run("RoundRobin_DispatchesEveryThread", TestRoundRobinDispatchesEveryThread);
        TR.Run("RoundRobin_QuantumExpiry_PreemptsSpinner", TestRoundRobinQuantumPreemption);
        TR.Run("RoundRobin_EqualPriorities_ShareEvenly", TestRoundRobinEqualShare);
        TR.Run("RoundRobin_SetPriority_DoesNotSkewShares", TestRoundRobinIgnoresPriority);
        TR.Run("RoundRobin_RunQueue_ExposesReadyThreads", TestRoundRobinRunQueueDiagnostics);
        TR.Run("RoundRobin_BlockedThread_LeavesRunQueue", TestRoundRobinBlockedLeavesQueue);
        TR.Run("SetScheduler_RestoresStrideDefault", TestSetSchedulerRestoresStride);
        TR.Run("SetScheduler_RehomesLiveThreads", TestSetSchedulerRehomesLiveThreads);

        // Finish test suite
        TR.Finish();

        Serial.WriteString("\n[Tests Complete - System Halting]\n");
    }

    protected override void Run()
    {
        // All tests ran in BeforeRun; stop the main loop after one iteration
        Stop();
    }

    protected override void AfterRun()
    {
        // Flush coverage data and signal QEMU to terminate
        TR.Complete();
        Cosmos.Kernel.System.Power.Halt();
    }

    // ==================== Monitor/Lock Tests ====================

    private static void TestMonitorEnterExitBasic()
    {
        object obj = new object();
        Monitor.Enter(obj);
        bool isEntered = Monitor.IsEntered(obj);
        Monitor.Exit(obj);
        Assert.True(isEntered, "Monitor.IsEntered should return true while lock is held");
    }

    private static void TestMonitorReentrant()
    {
        object obj = new object();
        Monitor.Enter(obj);
        Monitor.Enter(obj);
        Monitor.Enter(obj);
        // If we got here without deadlock, reentrant acquisition works
        Monitor.Exit(obj);
        Monitor.Exit(obj);
        Monitor.Exit(obj);
        Assert.True(true, "Reentrant Monitor.Enter should not deadlock");
    }

    private static void TestMonitorEnterRefBool()
    {
        object obj = new object();
        bool lockTaken = false;
        Monitor.Enter(obj, ref lockTaken);
        Assert.True(lockTaken, "lockTaken should be true after Monitor.Enter");
        Monitor.Exit(obj);
    }

    private static void TestMonitorTryEnter()
    {
        object obj = new object();
        bool result = Monitor.TryEnter(obj);
        Assert.True(result, "TryEnter should succeed on uncontested object");
        if (result)
        {
            Monitor.Exit(obj);
        }
    }

    private static void TestLockStatementBasic()
    {
        object obj = new object();
        bool bodyExecuted = false;
        lock (obj)
        {
            bodyExecuted = true;
        }
        Assert.True(bodyExecuted, "lock statement body should execute");
    }

    private static void TestLockProtectsSharedData()
    {
        Serial.WriteString("[Test] Testing lock with threads...\n");
        _lockCounter = 0;

        SysThread thread1 = new SysThread(() =>
        {
            for (int i = 0; i < LockIterationsPerThread; i++)
            {
                lock (_lockObj)
                {
                    _lockCounter++;
                }
            }
        });

        SysThread thread2 = new SysThread(() =>
        {
            for (int i = 0; i < LockIterationsPerThread; i++)
            {
                lock (_lockObj)
                {
                    _lockCounter++;
                }
            }
        });

        thread1.Start();
        thread2.Start();

        TimerManager.Wait(LockTestInitialWaitMs);

        for (int i = 0; i < MaxExtraWaitRetries && _lockCounter < ExpectedTotalIncrements; i++)
        {
            TimerManager.Wait(RetryWaitMs);
        }

        Serial.WriteString("[Test] Lock counter: ");
        Serial.WriteNumber((uint)_lockCounter);
        Serial.WriteString("\n");

        Assert.Equal(ExpectedTotalIncrements, _lockCounter);
    }

    private static void TestLockReentrant()
    {
        object obj = new object();
        lock (obj)
        {
            lock (obj)
            {
                // Nested lock on same object should not deadlock
            }
        }
        Assert.True(true, "Nested lock on same object should not deadlock");
    }

    private static void TestMonitorExitWithoutEnter()
    {
        object obj = new object();
        try
        {
            Monitor.Exit(obj); // Should not crash        
            Assert.Fail("Monitor.Exit should throw if owning thread doesn't own the lock");
        }
        catch(SynchronizationLockException)
        {
            Assert.True(true, "Monitor.Exit without prior Enter should not crash");
        }
    }

    // ==================== SpinLock Tests ====================

    private static void TestSpinLockInitialState()
    {
        var spinLock = new Cosmos.Kernel.Core.Scheduler.SpinLock();
        Assert.False(spinLock.IsLocked, "New spinlock should be unlocked");
    }

    private static void TestSpinLockAcquire()
    {
        var spinLock = new Cosmos.Kernel.Core.Scheduler.SpinLock();
        spinLock.Acquire();
        Assert.True(spinLock.IsLocked, "Spinlock should be locked after Acquire");
        spinLock.Release();
    }

    private static void TestSpinLockRelease()
    {
        var spinLock = new Cosmos.Kernel.Core.Scheduler.SpinLock();
        spinLock.Acquire();
        spinLock.Release();
        Assert.False(spinLock.IsLocked, "Spinlock should be unlocked after Release");
    }

    private static void TestSpinLockTryAcquireSuccess()
    {
        var spinLock = new Cosmos.Kernel.Core.Scheduler.SpinLock();
        bool acquired = spinLock.TryAcquire();
        Assert.True(acquired, "TryAcquire should succeed on unlocked spinlock");
        Assert.True(spinLock.IsLocked, "Spinlock should be locked after TryAcquire succeeds");
        spinLock.Release();
    }

    private static void TestSpinLockTryAcquireFail()
    {
        var spinLock = new Cosmos.Kernel.Core.Scheduler.SpinLock();
        spinLock.Acquire();
        // Try to acquire from same context - should fail
        bool acquired = spinLock.TryAcquire();
        Assert.False(acquired, "TryAcquire should fail on already locked spinlock");
        spinLock.Release();
    }

    // ==================== Thread Tests ====================

    private static void TestThreadExecution()
    {
        Serial.WriteString("[Test] Testing thread execution...\n");
        _threadExecuted = false;

        var thread = new global::System.Threading.Thread(ThreadExecutionWorker);

        Serial.WriteString("[Test] Starting thread...\n");
        thread.Start();

        // Wait longer for thread to execute (give scheduler more time)
        Serial.WriteString("[Test] Waiting for thread execution...\n");
        TimerManager.Wait(ThreadStartupWaitMs);

        // Check multiple times with delays
        for (int i = 0; i < ThreadExecPollRetries && !_threadExecuted; i++)
        {
            TimerManager.Wait(ThreadPollWaitMs);
        }

        Assert.True(_threadExecuted, "Thread delegate should have executed");
        Serial.WriteString("[Test] Thread execution test complete\n");
    }

    private static void ThreadExecutionWorker()
    {
        Serial.WriteString("[Thread] Delegate executing!\n");
        _threadExecuted = true;
        Serial.WriteString("[Thread] Delegate completed!\n");
    }

    // ===== EnsureSufficientExecutionStack in spawned threads (#433) =====
    // The generated ToString/PrintMembers of a record calls
    // RuntimeHelpers.EnsureSufficientExecutionStack, which sizes the thread's
    // stack via RhGetCurrentThreadStackBounds. Bounds narrower than CoreLib's
    // 128KB MinExecutionStackSize (or bounds fabricated from the current SP)
    // make every call throw InsufficientExecutionStackException.

    /// <summary>Record whose generated ToString exercises EnsureSufficientExecutionStack (#433).</summary>
    private record StackProbeRecord(int Answer, string Label);

    private static volatile bool _stackProbeDone;
    private static volatile bool _stackProbeSufficient;
    private static string? _stackProbeError;
    private static string? _stackProbeToString;

    private static void TestEnsureSufficientStackInThread()
    {
        _stackProbeDone = false;
        _stackProbeSufficient = false;
        _stackProbeError = null;

        var thread = new SysThread(EnsureSufficientStackWorker);
        thread.Start();

        for (int i = 0; i < TaskPollRetries && !_stackProbeDone; i++)
        {
            TimerManager.Wait(TaskPollIntervalMs);
        }

        Assert.True(_stackProbeDone, "EnsureSufficientExecutionStack worker should finish");
        Assert.True(_stackProbeError == null, "EnsureSufficientExecutionStack should not throw in a spawned thread");
        Assert.True(_stackProbeSufficient, "TryEnsureSufficientExecutionStack should report sufficient stack in a spawned thread");
    }

    private static void EnsureSufficientStackWorker()
    {
        _stackProbeSufficient = RuntimeHelpers.TryEnsureSufficientExecutionStack();
        try
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
        }
        catch (InsufficientExecutionStackException e)
        {
            _stackProbeError = e.Message;
            Serial.WriteString("[StackProbe] EnsureSufficientExecutionStack threw: ");
            Serial.WriteString(e.Message);
            Serial.WriteString("\n");
        }
        _stackProbeDone = true;
    }

    private static void TestRecordToStringInThread()
    {
        _stackProbeDone = false;
        _stackProbeToString = null;
        _stackProbeError = null;

        var thread = new SysThread(RecordToStringWorker);
        thread.Start();

        for (int i = 0; i < TaskPollRetries && !_stackProbeDone; i++)
        {
            TimerManager.Wait(TaskPollIntervalMs);
        }

        Assert.True(_stackProbeDone, "record ToString worker should finish");
        Assert.True(_stackProbeError == null, "record ToString should not throw in a spawned thread");
        Assert.True(_stackProbeToString != null && _stackProbeToString.Contains("42"),
            "record ToString should contain the property value");
    }

    private static void RecordToStringWorker()
    {
        try
        {
            _stackProbeToString = new StackProbeRecord(42, "yuki").ToString();
            Serial.WriteString("[StackProbe] record ToString: ");
            Serial.WriteString(_stackProbeToString);
            Serial.WriteString("\n");
        }
        catch (InsufficientExecutionStackException e)
        {
            _stackProbeError = e.Message;
            Serial.WriteString("[StackProbe] record ToString threw: ");
            Serial.WriteString(e.Message);
            Serial.WriteString("\n");
        }
        _stackProbeDone = true;
    }

    private static void TestRecordToStringOnMainThread()
    {
        // The main kernel thread is the scheduler's idle thread and runs on the
        // bootloader-provided stack, so this exercises the boot-stack branch of
        // RhGetCurrentThreadStackBounds (Limine stack-size request + captured top).
        try
        {
            string s = new StackProbeRecord(7, "root").ToString();
            Assert.True(s.Contains("7"), "record ToString on the main thread should contain the property value");
        }
        catch (InsufficientExecutionStackException)
        {
            Assert.Fail("record ToString should not throw InsufficientExecutionStackException on the main thread");
        }
    }

    // ===== Thread constructor maxStackSize (#435) =====
    // Upstream Thread.CreateThread resolves the constructor's maxStackSize
    // (<= 0 means RhGetDefaultStackSize) and passes it to the exported
    // SystemNative_CreateThread, which page-aligns it and floors it at 64KB.
    // The worker reads its own scheduler thread's StackSize to observe what
    // was actually allocated.

    /// <summary>Requested stack size for the honored-request test (512KB, above the default).</summary>
    private const int LargeRequestedStackSize = 512 * 1024;
    /// <summary>Requested stack size for the floored-request test (16KB, below the 64KB floor).</summary>
    private const int TinyRequestedStackSize = 16 * 1024;
    /// <summary>Smallest stack SystemNative_CreateThread allocates for a managed thread.</summary>
    private const int MinThreadStackSize = 64 * 1024;

    private static volatile bool _stackSizeProbeDone;
    private static ulong _observedStackSize;
    private static volatile bool _observedStackSufficient;

    private static void StackSizeProbeWorker()
    {
        _observedStackSize = SchedulerManager.GetCpuState(SchedulerManager.GetCurrentCpuId())!.CurrentThread!.StackSize;
        _observedStackSufficient = RuntimeHelpers.TryEnsureSufficientExecutionStack();
        _stackSizeProbeDone = true;
    }

    private static void RunStackSizeProbe(int requestedStackSize)
    {
        _stackSizeProbeDone = false;
        _observedStackSize = 0;
        _observedStackSufficient = false;

        var thread = new SysThread(StackSizeProbeWorker, requestedStackSize);
        thread.Start();

        for (int i = 0; i < TaskPollRetries && !_stackSizeProbeDone; i++)
        {
            TimerManager.Wait(TaskPollIntervalMs);
        }
    }

    private static void TestThreadMaxStackSizeHonored()
    {
        RunStackSizeProbe(LargeRequestedStackSize);

        Assert.True(_stackSizeProbeDone, "stack-size probe worker should finish");
        Assert.True(_observedStackSize == LargeRequestedStackSize,
            "scheduler thread should get the requested 512KB stack");
        Assert.True(_observedStackSufficient,
            "a 512KB stack should pass TryEnsureSufficientExecutionStack");
    }

    private static void TestThreadTinyStackSizeFloored()
    {
        RunStackSizeProbe(TinyRequestedStackSize);

        Assert.True(_stackSizeProbeDone, "floored stack-size probe worker should finish");
        Assert.True(_observedStackSize == MinThreadStackSize,
            "a 16KB request should be floored to the 64KB minimum");
        Assert.False(_observedStackSufficient,
            "a 64KB stack sits below CoreLib's 128KB reserve, so TryEnsureSufficientExecutionStack reports false (upstream-faithful)");
    }

    // ===== Mutex idle-thread contention (scheduler Mutex, not System.Threading) =====
    // The main kernel thread is the scheduler's idle thread. Blocking it
    // (BlockThread) only gets it resurrected by the PickNext ?? IdleThread
    // fallback on the next tick, which re-runs Mutex.Acquire's retry loop:
    // every pass calls OnThreadBlocked again and subtracts tickets that
    // OnThreadReady never added, so TotalTickets drifts (and underflows).
    private static Cosmos.Kernel.Core.Scheduler.Mutex? _idleMutex;
    private static volatile bool _mutexWorkerHolding;
    private static volatile bool _mutexMainContending;
    private static volatile bool _mutexTestDone;
    private static volatile bool _mutexWorkerExited;

    private static void TestMutexIdleThreadContention()
    {
        uint cpuId = SchedulerManager.GetCurrentCpuId();
        Cosmos.Kernel.Core.Scheduler.Stride.StrideCpuData? cpuData =
            SchedulerManager.GetCpuState(cpuId)?.SchedulerData
                as Cosmos.Kernel.Core.Scheduler.Stride.StrideCpuData;
        Assert.True(cpuData != null, "stride per-CPU data should exist");

        _idleMutex = new Cosmos.Kernel.Core.Scheduler.Mutex();
        _mutexWorkerHolding = false;
        _mutexMainContending = false;
        _mutexTestDone = false;
        _mutexWorkerExited = false;

        var worker = new global::System.Threading.Thread(MutexIdleWorker);
        worker.Start();

        for (int i = 0; i < FlagPollRetries && !_mutexWorkerHolding; i++)
        {
            TimerManager.Wait(FlagPollIntervalMs);
        }
        Assert.True(_mutexWorkerHolding, "worker should hold the mutex");

        // Both reads happen with the worker runnable (it spins on the flags,
        // never blocking), so any delta comes from the idle thread's own
        // block/ready churn inside Acquire.
        ulong before = cpuData!.TotalTickets;

        _mutexMainContending = true;
        _idleMutex.Acquire();
        _idleMutex.Release();

        ulong after = cpuData.TotalTickets;
        _mutexTestDone = true;

        // Keep the cell hermetic: wait for the worker to leave its spin and
        // give its exit path time to finish inside THIS cell, so the
        // scheduler bookkeeping of the exit can't interleave with the next
        // cell's thread creation.
        for (int i = 0; i < FlagPollRetries && !_mutexWorkerExited; i++)
        {
            TimerManager.Wait(FlagPollIntervalMs);
        }
        TimerManager.Wait(ExitGraceWaitMs);

        Assert.True(before == after, "idle-thread contention must not drift TotalTickets");
    }

    private static void MutexIdleWorker()
    {
        _idleMutex!.Acquire();
        _mutexWorkerHolding = true;
        while (!_mutexMainContending)
        {
            // spin until the main (idle) thread is about to contend
        }
        // Hold across several scheduler ticks so the contending idle thread
        // goes through its block/resurrect cycle more than once.
        TimerManager.Wait(IdleMutexHoldMs);
        _idleMutex.Release();
        while (!_mutexTestDone)
        {
            // stay runnable until the main thread has sampled TotalTickets
        }
        _mutexWorkerExited = true;
    }

    // ===== Multi-waiter paths (List<Thread> scans on non-empty lists) =====
    // The single-waiter driver flow keeps _waiters/_waitingThreads empty at
    // the Contains call, so the list-scan path (EqualityComparer<Thread>)
    // is otherwise never exercised: the second parked waiter/contender here
    // is what actually walks a non-empty list.
    private static Cosmos.Kernel.Core.Scheduler.InterruptEvent? _twoWaiterEvent;
    private static volatile bool _waiterAParked, _waiterBParked;
    private static volatile bool _waiterAWoke, _waiterBWoke;

    private static void TestInterruptEventTwoWaiters()
    {
        _twoWaiterEvent = new Cosmos.Kernel.Core.Scheduler.InterruptEvent();
        _waiterAParked = _waiterBParked = false;
        _waiterAWoke = _waiterBWoke = false;

        var w1 = new global::System.Threading.Thread(TwoWaiterWorkerA);
        var w2 = new global::System.Threading.Thread(TwoWaiterWorkerB);
        w1.Start();
        w2.Start();

        // Let both workers reach Wait() and park; the second one walks the
        // one-element waiter list on its way in.
        for (int i = 0; i < FlagPollRetries && !(_waiterAParked && _waiterBParked); i++)
        {
            TimerManager.Wait(FlagPollIntervalMs);
        }
        TimerManager.Wait(FlagPollIntervalMs);

        _twoWaiterEvent.Signal();
        _twoWaiterEvent.Signal();

        for (int i = 0; i < FlagPollRetries && !(_waiterAWoke && _waiterBWoke); i++)
        {
            TimerManager.Wait(FlagPollIntervalMs);
        }
        TimerManager.Wait(ExitGraceWaitMs);
        Assert.True(_waiterAWoke && _waiterBWoke, "both parked waiters must be woken by two signals");
    }

    private static void TwoWaiterWorkerA()
    {
        _waiterAParked = true;
        _twoWaiterEvent!.Wait();
        _waiterAWoke = true;
    }

    private static void TwoWaiterWorkerB()
    {
        _waiterBParked = true;
        _twoWaiterEvent!.Wait();
        _waiterBWoke = true;
    }

    private static Cosmos.Kernel.Core.Scheduler.Mutex? _contendedMutex;
    private static volatile int _contenderAcquisitions;

    private static void TestMutexThreeContenders()
    {
        _contendedMutex = new Cosmos.Kernel.Core.Scheduler.Mutex();
        _contenderAcquisitions = 0;

        var c1 = new global::System.Threading.Thread(MutexContenderWorker);
        var c2 = new global::System.Threading.Thread(MutexContenderWorker);
        var c3 = new global::System.Threading.Thread(MutexContenderWorker);
        c1.Start();
        c2.Start();
        c3.Start();

        // The holder keeps the mutex across several ticks, so the two other
        // contenders both queue up — the last one scans a non-empty
        // _waitingThreads list.
        for (int i = 0; i < ContenderPollRetries && _contenderAcquisitions < ContenderCount; i++)
        {
            TimerManager.Wait(FlagPollIntervalMs);
        }
        TimerManager.Wait(ExitGraceWaitMs);
        Assert.Equal(ContenderCount, _contenderAcquisitions, "all three contenders must acquire the mutex in turn");
    }

    private static void MutexContenderWorker()
    {
        _contendedMutex!.Acquire();
        // Hold across a few ticks so the other contenders pile up in
        // _waitingThreads; the increment is protected by the mutex itself.
        TimerManager.Wait(MutexContenderHoldMs);
        _contenderAcquisitions++;
        _contendedMutex.Release();
    }

    // ===== Release hand-off (anti-barging) =====
    // Release used to clear ownership and merely ready the parked waiter;
    // until that waiter's retry ran, ANY thread could re-take the mutex and
    // send the waiter to the back of the queue again — repeatable, so a
    // waiter on a contended mutex could starve. The releaser's immediate
    // TryAcquire is the deterministic probe: with ownership handed off in
    // Release it must fail.
    private static Cosmos.Kernel.Core.Scheduler.Mutex? _handoffMutex;
    private static volatile bool _handoffWorkerHolding;
    private static volatile bool _handoffReleaseRequested;
    private static volatile bool _handoffContenderAcquired;
    private static volatile int _handoffBargeResult; // -1 pending, 0 no barge, 1 barged

    private static void TestMutexReleaseHandsOff()
    {
        _handoffMutex = new Cosmos.Kernel.Core.Scheduler.Mutex();
        _handoffWorkerHolding = false;
        _handoffReleaseRequested = false;
        _handoffContenderAcquired = false;
        _handoffBargeResult = BargeResultPending;

        var holder = new global::System.Threading.Thread(HandoffHolderWorker);
        holder.Start();
        for (int i = 0; i < FlagPollRetries && !_handoffWorkerHolding; i++)
        {
            TimerManager.Wait(FlagPollIntervalMs);
        }
        Assert.True(_handoffWorkerHolding, "holder should own the mutex");

        var contender = new global::System.Threading.Thread(HandoffContenderWorker);
        contender.Start();
        // Give the contender a few quanta to park in _waitingThreads.
        TimerManager.Wait(ContenderParkWaitMs);

        _handoffReleaseRequested = true;
        for (int i = 0; i < FlagPollRetries && _handoffBargeResult == BargeResultPending; i++)
        {
            TimerManager.Wait(FlagPollIntervalMs);
        }
        for (int i = 0; i < FlagPollRetries && !_handoffContenderAcquired; i++)
        {
            TimerManager.Wait(FlagPollIntervalMs);
        }

        // Exit grace: both worker threads finished their work above; give
        // their exit paths time to complete inside this cell (see the
        // idle-contention cell for the rationale).
        TimerManager.Wait(ExitGraceWaitMs);

        Assert.Equal(BargeResultNoBarge, _handoffBargeResult,
            "Release must hand the mutex to the parked waiter; the releaser's immediate TryAcquire barged in");
        Assert.True(_handoffContenderAcquired, "the parked waiter must end up owning the mutex");
    }

    private static void HandoffHolderWorker()
    {
        _handoffMutex!.Acquire();
        _handoffWorkerHolding = true;
        while (!_handoffReleaseRequested)
        {
            // spin: stay runnable so the contender has to park behind us
        }
        _handoffMutex.Release();
        bool barged = _handoffMutex.TryAcquire();
        _handoffBargeResult = barged ? BargeResultBarged : BargeResultNoBarge;
        if (barged)
        {
            _handoffMutex.Release();
        }
    }

    private static void HandoffContenderWorker()
    {
        _handoffMutex!.Acquire();
        _handoffContenderAcquired = true;
        _handoffMutex.Release();
    }

    private static void TestMultipleThreads()
    {
        Serial.WriteString("[Test] Testing multiple threads...\n");
        _thread1Counter = 0;
        _thread2Counter = 0;

        var thread1 = new global::System.Threading.Thread(Thread1Worker);
        var thread2 = new global::System.Threading.Thread(Thread2Worker);

        thread1.Start();
        thread2.Start();

        // Wait much longer for both threads to complete (they each do 5 iterations with 50ms waits = 250ms minimum)
        // But scheduler overhead means we need more time
        TimerManager.Wait(ThreadsCompletionWaitMs);

        // Additional waiting if not complete
        for (int i = 0; i < MaxExtraWaitRetries && (_thread1Counter < WorkerIterationCount || _thread2Counter < WorkerIterationCount); i++)
        {
            TimerManager.Wait(RetryWaitMs);
        }

        Serial.WriteString("[Test] Thread1 counter: ");
        Serial.WriteNumber((uint)_thread1Counter);
        Serial.WriteString(", Thread2 counter: ");
        Serial.WriteNumber((uint)_thread2Counter);
        Serial.WriteString("\n");

        Assert.Equal(WorkerIterationCount, _thread1Counter);
        Assert.Equal(WorkerIterationCount, _thread2Counter);
    }

    private static void Thread1Worker()
    {
        Serial.WriteString("[Thread1] Started\n");
        for (int i = 0; i < WorkerIterationCount; i++)
        {
            _thread1Counter++;
            TimerManager.Wait(WorkerStepDelayMs);
        }
        Serial.WriteString("[Thread1] Completed\n");
    }

    private static void Thread2Worker()
    {
        Serial.WriteString("[Thread2] Started\n");
        for (int i = 0; i < WorkerIterationCount; i++)
        {
            _thread2Counter++;
            TimerManager.Wait(WorkerStepDelayMs);
        }
        Serial.WriteString("[Thread2] Completed\n");
    }

    private static void TestSpinLockWithThreads()
    {
        Serial.WriteString("[Test] Testing spinlock with threads...\n");
        _sharedCounter = 0;
        _testLock = new Cosmos.Kernel.Core.Scheduler.SpinLock();

        var thread1 = new global::System.Threading.Thread(SpinLockThread1Worker);
        var thread2 = new global::System.Threading.Thread(SpinLockThread2Worker);

        thread1.Start();
        thread2.Start();

        // Wait much longer for threads to complete (100 lock/unlock iterations each)
        TimerManager.Wait(LockTestInitialWaitMs);

        // Additional waiting if not complete
        for (int i = 0; i < MaxExtraWaitRetries && _sharedCounter < ExpectedTotalIncrements; i++)
        {
            TimerManager.Wait(RetryWaitMs);
        }

        Serial.WriteString("[Test] Final counter: ");
        Serial.WriteNumber((uint)_sharedCounter);
        Serial.WriteString("\n");

        // With proper locking, counter should be exactly 200
        Assert.Equal(ExpectedTotalIncrements, _sharedCounter);
    }

    private static void SpinLockThread1Worker()
    {
        Serial.WriteString("[Thread1] Starting increments\n");
        for (int i = 0; i < LockIterationsPerThread; i++)
        {
            _testLock.Acquire();
            _sharedCounter++;
            _testLock.Release();
        }
        Serial.WriteString("[Thread1] Done\n");
    }

    private static void SpinLockThread2Worker()
    {
        Serial.WriteString("[Thread2] Starting increments\n");
        for (int i = 0; i < LockIterationsPerThread; i++)
        {
            _testLock.Acquire();
            _sharedCounter++;
            _testLock.Release();
        }
        Serial.WriteString("[Thread2] Done\n");
    }

    [ThreadStatic]
    private static int StaticValue;
    private static void TestThreadStatics()
    {
        int secondThreadValue = 0;
        StaticValue = 18;

        SysThread thread = new SysThread(() =>
        {
            StaticValue = 42;
            secondThreadValue = StaticValue;
        });

        thread.Start();

        TimerManager.Wait(ThreadStaticsWaitMs); // Wait 10ms for the thread to finish.

        Assert.Equal(18, StaticValue);
        Assert.Equal(42, secondThreadValue);
    }

    // ==================== ThreadPool / Task / Async-Await Tests ====================

    private static volatile bool _threadPoolExecuted;

    private static void TestThreadPoolQueueUserWorkItem()
    {
        Serial.WriteString("[Test] Testing ThreadPool.QueueUserWorkItem...\n");
        _threadPoolExecuted = false;

        ThreadPool.QueueUserWorkItem(_ => { _threadPoolExecuted = true; });

        for (int i = 0; i < TaskPollRetries && !_threadPoolExecuted; i++)
        {
            TimerManager.Wait(TaskPollIntervalMs);
        }

        Assert.True(_threadPoolExecuted, "ThreadPool work item should execute");
    }

    private static void TestTaskFromResult()
    {
        Task<int> t = Task.FromResult(42);
        Assert.True(t.IsCompleted, "Task.FromResult should be already completed");
        Assert.Equal(42, t.Result, "Task.FromResult should expose the value via .Result");
    }

    private static void TestTaskRunExecutesAction()
    {
        Serial.WriteString("[Test] Testing Task.Run with Action...\n");
        bool ran = false;

        Task t = Task.Run(() => { ran = true; });

        for (int i = 0; i < TaskPollRetries && !t.IsCompleted; i++)
        {
            TimerManager.Wait(TaskPollIntervalMs);
        }

        Assert.True(t.IsCompleted, "Task.Run task should reach completion");
        Assert.True(ran, "Task.Run delegate should have executed");
    }

    private static void TestTaskRunReturnsResult()
    {
        Serial.WriteString("[Test] Testing Task.Run<int>...\n");
        Task<int> t = Task.Run(() => 7 * 6);

        for (int i = 0; i < TaskPollRetries && !t.IsCompleted; i++)
        {
            TimerManager.Wait(TaskPollIntervalMs);
        }

        Assert.True(t.IsCompleted, "Task<int>.Run task should reach completion");
        Assert.Equal(42, t.Result, "Task<int>.Run should return computed value");
    }

    private static async Task<int> AsyncReturnsValue()
    {
        await Task.CompletedTask;
        return 42;
    }

    private static void TestAsyncCompletedTask()
    {
        Serial.WriteString("[Test] Testing async method with awaited completed task...\n");
        Task<int> t = AsyncReturnsValue();

        for (int i = 0; i < TaskPollRetries && !t.IsCompleted; i++)
        {
            TimerManager.Wait(TaskPollIntervalMs);
        }

        Assert.True(t.IsCompleted, "Async method should complete");
        Assert.Equal(42, t.Result, "Async method should return 42 via await");
    }

    private static async Task<int> AsyncAwaitsTaskRun()
    {
        return await Task.Run(() => 21 + 21);
    }

    private static void TestAsyncAwaitsTaskRun()
    {
        Serial.WriteString("[Test] Testing async method awaiting Task.Run...\n");
        Task<int> t = AsyncAwaitsTaskRun();

        for (int i = 0; i < TaskPollRetries && !t.IsCompleted; i++)
        {
            TimerManager.Wait(TaskPollIntervalMs);
        }

        Assert.True(t.IsCompleted, "Async method awaiting Task.Run should complete");
        Assert.Equal(42, t.Result, "Awaited Task.Run should yield 42");
    }

    private static async Task<int> InnerAsync(int x)
    {
        await Task.CompletedTask;
        return x + 1;
    }

    private static async Task<int> OuterAsync()
    {
        int a = await InnerAsync(10);
        int b = await InnerAsync(31);
        return a + b;
    }

    private static void TestAsyncChain()
    {
        Serial.WriteString("[Test] Testing async chain composition...\n");
        Task<int> t = OuterAsync();

        for (int i = 0; i < TaskPollRetries && !t.IsCompleted; i++)
        {
            TimerManager.Wait(TaskPollIntervalMs);
        }

        Assert.True(t.IsCompleted, "Chained async method should complete");
        Assert.Equal(43, t.Result, "Async chain (10+1) + (31+1) should equal 43");
    }

    // ==================== Delegate Tests ====================

    // --- Basic invocation ---

    private static void TestDelegateActionBasicInvoke()
    {
        bool invoked = false;
        Action action = () => { invoked = true; };
        action();
        Assert.True(invoked, "Action delegate should set invoked flag when called");
    }

    private static void TestDelegateFuncReturnsValue()
    {
        Func<int> getAnswer = () => 42;
        int result = getAnswer();
        Assert.Equal(42, result, "Func<int> should return 42");
    }

    private static void TestDelegateActionWithParameter()
    {
        int received = 0;
        Action<int> action = (x) => { received = x; };
        action(99);
        Assert.Equal(99, received, "Action<int> should receive and store the parameter");
    }

    private static void TestDelegateFuncTransform()
    {
        Func<int, int> doubler = x => x * 2;
        int result = doubler(21);
        Assert.Equal(42, result, "Func<int,int> should double the input");
    }

    // --- Custom delegate types ---

    private static void TestDelegateCustomVoid()
    {
        bool called = false;
        VoidDelegate d = () => { called = true; };
        d();
        Assert.True(called, "Custom void delegate should be invoked");
    }

    private static void TestDelegateCustomWithReturn()
    {
        BinaryIntDelegate add = (a, b) => a + b;
        int result = add(10, 32);
        Assert.Equal(42, result, "Custom BinaryIntDelegate should add the two parameters");
    }

    // --- Static and instance method delegates ---

    private static int StaticMultiply(int x, int y) => x * y;

    private static void TestDelegateStaticMethod()
    {
        Func<int, int, int> multiply = StaticMultiply;
        int result = multiply(6, 7);
        Assert.Equal(42, result, "Delegate bound to static method should compute 6*7=42");
    }

    private class DelegateAccumulator
    {
        public int Total { get; private set; }
        public void Add(int value) => Total += value;
    }

    private static void TestDelegateInstanceMethod()
    {
        var accumulator = new DelegateAccumulator();
        Action<int> add = accumulator.Add;
        add(10);
        add(32);
        Assert.Equal(42, accumulator.Total, "Instance method delegate should accumulate values into the bound object");
    }

    // --- Multicast delegates ---

    private static void TestDelegateMulticastBothCalled()
    {
        int callCount = 0;
        Action a = () => { callCount++; };
        Action b = () => { callCount++; };
        Action combined = a + b;
        combined();
        Assert.Equal(2, callCount, "Multicast delegate should invoke both handlers");
    }

    private static void TestDelegateMulticastOrder()
    {
        // Verify that multicast delegates invoke handlers in registration order
        int[] log = new int[3];
        int index = 0;

        Action first = () => { log[index] = 1; index++; };
        Action second = () => { log[index] = 2; index++; };
        Action third = () => { log[index] = 3; index++; };

        Action combined = first + second + third;
        combined();

        Assert.Equal(1, log[0], "First handler should be invoked first");
        Assert.Equal(2, log[1], "Second handler should be invoked second");
        Assert.Equal(3, log[2], "Third handler should be invoked third");
    }

    private static void TestDelegateMulticastRemove()
    {
        int callCount = 0;
        Action a = () => { callCount++; };
        Action b = () => { callCount += 10; };

        Action combined = a + b;
        combined -= b;
        combined();

        // Only 'a' should remain: callCount == 1, not 11
        Assert.Equal(1, callCount, "After removing handler b, only handler a should fire");
    }

    private static void TestDelegateMulticastGetInvocationList()
    {
        Action a = () => { };
        Action b = () => { };
        Action c = () => { };

        Action combined = a + b + c;
        Delegate[] list = combined.GetInvocationList();

        Assert.Equal(3, list.Length, "GetInvocationList should return 3 delegates after combining three");
    }

    // --- Closures ---

    private static void TestDelegateClosureCapturesLocal()
    {
        int x = 10;
        Func<int> getX = () => x;
        int result = getX();
        Assert.Equal(10, result, "Closure should capture the local variable value at invocation time");
    }

    private static void TestDelegateClosureMutableCapture()
    {
        // Lambda mutates the captured variable; outer scope sees the change
        int counter = 0;
        Action increment = () => { counter++; };

        increment();
        increment();
        increment();

        Assert.Equal(3, counter, "Closure should mutate the captured variable; outer scope should see 3");
    }

    private static void TestDelegateClosureSharedCapture()
    {
        // Two distinct lambdas capturing the same local variable share the same closure slot
        int shared = 0;
        Action addTen = () => { shared += 10; };
        Action addFive = () => { shared += 5; };

        addTen();
        addFive();

        Assert.Equal(15, shared, "Both closures sharing a captured variable should both modify it (10 + 5 = 15)");
    }

    // --- Null delegate ---

    private static void TestDelegateNullSafeInvoke()
    {
        // ?. on a null delegate must not throw; it's a no-op
        Action? nullDelegate = null;
        nullDelegate?.Invoke();
        // Reaching here without a fault means the test passes
        Assert.True(true, "Null?.Invoke() should be a safe no-op and not fault");
    }

    // --- Delegate equality ---

    private static void DelegateEqualityTarget1() { }
    private static void DelegateEqualityTarget2() { }

    private static void TestDelegateEqualitySameMethod()
    {
        // Two delegates wrapping the same static method must compare equal
        Action a = DelegateEqualityTarget1;
        Action b = DelegateEqualityTarget1;
        Assert.True(a == b, "Delegates wrapping the same static method should be equal");
    }

    private static void TestDelegateEqualityDifferentMethod()
    {
        // Delegates wrapping different methods must compare unequal
        Action a = DelegateEqualityTarget1;
        Action b = DelegateEqualityTarget2;
        Assert.True(a != b, "Delegates wrapping different methods should not be equal");
    }

    // --- Delegate as parameter and return value ---

    private static int ApplyTransform(int value, Func<int, int> transform)
    {
        return transform(value);
    }

    private static void TestDelegateAsParameter()
    {
        Func<int, int> square = x => x * x;
        int result = ApplyTransform(7, square);
        Assert.Equal(49, result, "Delegate passed as parameter should be invoked: 7*7=49");
    }

    private static Func<int, int> CreateAdder(int amount)
    {
        return x => x + amount;
    }

    private static void TestDelegateAsReturnValue()
    {
        // CreateAdder captures 'amount' in a closure and returns the delegate
        Func<int, int> addTen = CreateAdder(10);
        int result = addTen(32);
        Assert.Equal(42, result, "Factory-returned delegate should close over 'amount': 32+10=42");
    }

    // --- Generic delegates with value types ---

    private static void TestDelegateGenericValueType()
    {
        Func<long, long> negate = x => -x;
        long result = negate(42L);
        Assert.Equal(-42L, result, "Generic Func<long,long> should negate the input");
    }

    // --- Predicate<T> ---

    private static void TestDelegatePredicate()
    {
        Predicate<int> isEven = x => (x % 2) == 0;

        Assert.True(isEven(4), "Predicate: 4 should be even");
        Assert.False(isEven(7), "Predicate: 7 should be odd");
        Assert.True(isEven(0), "Predicate: 0 should be even");
        Assert.False(isEven(1), "Predicate: 1 should be odd");
    }

    // --- Comparison<T> ---

    private static void TestDelegateComparison()
    {
        // Descending comparator: larger value sorts first
        Comparison<int> descending = (a, b) => b - a;

        // a=5, b=3 → b-a = -2 < 0 → a (5) comes before b (3) in descending order ✓
        int result = descending(5, 3);
        Assert.True(result < 0, "Descending comparison: compare(5,3) should be negative (5 before 3)");

        result = descending(3, 5);
        Assert.True(result > 0, "Descending comparison: compare(3,5) should be positive (3 after 5)");

        result = descending(4, 4);
        Assert.Equal(0, result, "Descending comparison: compare(4,4) should be zero (equal)");
    }

    // --- Delegate chaining / composition ---

    private static void TestDelegateChaining()
    {
        Func<int, int> addOne = x => x + 1;
        Func<int, int> multiplyByThree = x => x * 3;

        // Manual pipeline: (13 + 1) * 3 = 42
        Func<int, int> pipeline = x => multiplyByThree(addOne(x));
        int result = pipeline(13);
        Assert.Equal(42, result, "Composed pipeline (13+1)*3 should equal 42");
    }

    // --- Event-style multicast pattern ---

    private static void TestDelegateEventPattern()
    {
        int eventFireCount = 0;
        string? lastEventData = null;

        // Simulate an event using a nullable multicast delegate
        Action<string>? handlers = null;

        // Subscribe two handlers
        handlers += (data) => { eventFireCount++; lastEventData = data; };
        handlers += (_) => { eventFireCount++; };

        // Fire the event
        handlers?.Invoke("hello");

        Assert.Equal(2, eventFireCount, "Both event handlers should fire");
        Assert.Equal("hello", lastEventData, "First handler should receive the event payload");
    }

    // ==================== Scheduling-Policy Tests ====================
    // Stride is the boot default; RoundRobinScheduler.cs implements the
    // plugging guide's Round-Robin sketch over the public seam, exactly as a
    // user kernel would. The cells below validate Stride's proportional
    // share, the live Stride -> Round-Robin switch, Round-Robin's semantics
    // (FIFO first-runs, quantum preemption, equal shares, priority ignored,
    // run-queue membership), and the switch back. Both swaps happen at
    // quiescent points — every worker of the previous cells has exited — so
    // no thread is parked or queued across the policy change.

    private static RoundRobinScheduler? s_roundRobin;

    // Two measured CPU-bound spinners (the proportional-share/equal-share cells)
    private static volatile bool _spinGo;
    private static volatile bool _spinStop;
    private static volatile bool _spinAReady, _spinBReady;
    private static volatile bool _spinADone, _spinBDone;
    private static SchedulerThread? _spinAThread, _spinBThread;
    // Written by each worker before its volatile done flag; read by main after it.
    private static ulong _spinACount, _spinBCount;
    private static long _observedPriorityA, _observedPriorityB;

    private static SchedulerThread? CurrentSchedulerThread()
    {
        return SchedulerManager.GetCpuState(SchedulerManager.GetCurrentCpuId())!.CurrentThread;
    }

    /// <summary>
    /// True when <paramref name="thread"/> currently sits in the installed
    /// policy's run queue, via the read-only diagnostics hooks. While main is
    /// executing this scan, every other runnable thread is Ready, so a
    /// spinning worker must be queued and a parked one must not.
    /// </summary>
    private static bool RunQueueHolds(SchedulerThread? thread)
    {
        if (thread == null)
        {
            return false;
        }

        IScheduler? scheduler = SchedulerManager.Current;
        PerCpuState? state = SchedulerManager.GetCpuState(SchedulerManager.GetCurrentCpuId());
        if (scheduler == null || state == null)
        {
            return false;
        }

        // One mask over the whole scan: the count and the per-index reads each
        // mask individually, so between them the tick could rotate the queue
        // and move the target past an index already visited. Nesting the
        // policy's own guards inside this one is harmless.
        using (SchedulerManager.MaskInterrupts())
        {
            int count = scheduler.GetRunQueueCount(state);
            for (int i = 0; i < count; i++)
            {
                if (ReferenceEquals(scheduler.GetRunQueueThread(state, i), thread))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void MeasuredSpinWorkerA()
    {
        _spinAThread = CurrentSchedulerThread();
        _spinAReady = true;
        while (!_spinGo)
        {
            // wait for main to apply priorities and open the measurement window
        }

        ulong count = 0;
        while (true)
        {
            count++;
            if ((count & SpinStopCheckMask) == 0 && _spinStop)
            {
                break;
            }
        }

        _spinACount = count;
        _spinADone = true;
    }

    private static void MeasuredSpinWorkerB()
    {
        _spinBThread = CurrentSchedulerThread();
        _spinBReady = true;
        while (!_spinGo)
        {
            // wait for main to apply priorities and open the measurement window
        }

        ulong count = 0;
        while (true)
        {
            count++;
            if ((count & SpinStopCheckMask) == 0 && _spinStop)
            {
                break;
            }
        }

        _spinBCount = count;
        _spinBDone = true;
    }

    /// <summary>
    /// Runs the two CPU-bound spinners for <see cref="PolicyMeasureMs"/> with
    /// the given priorities applied through <see cref="SchedulerManager.SetPriority"/>,
    /// leaving their loop counts in _spinACount/_spinBCount and the priorities
    /// the policy reported (read while both spinners were alive) in
    /// _observedPriorityA/_observedPriorityB.
    /// </summary>
    private static void RunTwoSpinnersMeasured(long priorityA, long priorityB)
    {
        _spinGo = false;
        _spinStop = false;
        _spinAReady = _spinBReady = false;
        _spinADone = _spinBDone = false;
        _spinAThread = _spinBThread = null;
        _spinACount = _spinBCount = 0;
        _observedPriorityA = _observedPriorityB = 0;

        SysThread workerA = new(MeasuredSpinWorkerA);
        SysThread workerB = new(MeasuredSpinWorkerB);
        workerA.Start();
        workerB.Start();

        for (int i = 0; i < FlagPollRetries && !(_spinAReady && _spinBReady); i++)
        {
            TimerManager.Wait(FlagPollIntervalMs);
        }

        uint cpuId = SchedulerManager.GetCurrentCpuId();
        if (_spinAThread != null && _spinBThread != null)
        {
            // SetPriority runs under a spinlock only (see the plugging guide's
            // kernel constraints); mask the tick around it ourselves.
            using (SchedulerManager.MaskInterrupts())
            {
                SchedulerManager.SetPriority(cpuId, _spinAThread, priorityA);
                SchedulerManager.SetPriority(cpuId, _spinBThread, priorityB);
            }

            // Capture what the policy reports while the threads are alive:
            // OnThreadExit drops the bookkeeping GetPriority reads.
            _observedPriorityA = SchedulerManager.GetPriority(_spinAThread);
            _observedPriorityB = SchedulerManager.GetPriority(_spinBThread);
        }

        _spinGo = true;
        TimerManager.Wait(PolicyMeasureMs);
        _spinStop = true;

        for (int i = 0; i < FlagPollRetries && !(_spinADone && _spinBDone); i++)
        {
            TimerManager.Wait(FlagPollIntervalMs);
        }
        TimerManager.Wait(ExitGraceWaitMs);

        Serial.WriteString("[PolicyTest] policy=");
        Serial.WriteString(SchedulerManager.Current?.Name ?? "none");
        Serial.WriteString(" prioA=");
        Serial.WriteNumber(priorityA);
        Serial.WriteString(" prioB=");
        Serial.WriteNumber(priorityB);
        Serial.WriteString(" countA=");
        Serial.WriteNumber((long)_spinACount);
        Serial.WriteString(" countB=");
        Serial.WriteNumber((long)_spinBCount);
        Serial.WriteString("\n");
    }

    private static void TestBootPolicyIsStride()
    {
        IScheduler? current = SchedulerManager.Current;
        Assert.True(current != null, "a scheduling policy must be installed at boot");
        Assert.Equal("Stride", current!.Name, "the boot default policy should be Stride");
    }

    private static void TestStrideProportionalShare()
    {
        RunTwoSpinnersMeasured(HighTickets, LowTickets);

        Assert.True(_spinAReady && _spinBReady, "both measured spinners should start");
        Assert.True(_spinADone && _spinBDone, "both measured spinners should finish the window");
        Assert.True(_observedPriorityA == HighTickets && _observedPriorityB == LowTickets,
            "Stride should report the tickets set through SetPriority");
        Assert.True(_spinACount > 0 && _spinBCount > 0,
            "both spinners must make progress under Stride (proportional share, not starvation)");
        Assert.True(_spinACount * PolicySkewDenominator >= _spinBCount * PolicySkewNumerator,
            "a 4x ticket edge must yield a clearly larger (>= 1.5x) CPU share under Stride");
    }

    private static void TestSetSchedulerInstallsRoundRobin()
    {
        s_roundRobin = new RoundRobinScheduler();

        // Mask the tick across the swap so nothing can observe the window
        // between ShutdownCpu (old policy) and InitializeCpu (new policy).
        using (SchedulerManager.MaskInterrupts())
        {
            SchedulerManager.SetScheduler(s_roundRobin);
        }

        Assert.True(ReferenceEquals(SchedulerManager.Current, s_roundRobin),
            "SchedulerManager.Current should be the installed Round-Robin instance");
        Assert.Equal("RoundRobin", SchedulerManager.Current!.Name,
            "the installed policy should report its own name");

        PerCpuState? state = SchedulerManager.GetCpuState(SchedulerManager.GetCurrentCpuId());
        Assert.True(state != null && state.SchedulerData is RoundRobinCpuData,
            "InitializeCpu should attach fresh Round-Robin per-CPU data");
        if (state != null)
        {
            Assert.Equal(0, s_roundRobin!.GetRunQueueCount(state),
                "the incoming policy should start from an empty run queue");
        }
    }

    private static volatile bool _policyProbeRan;

    private static void PolicyProbeWorker()
    {
        _policyProbeRan = true;
    }

    private static void RunPolicyProbeWorker()
    {
        _policyProbeRan = false;
        SysThread probe = new(PolicyProbeWorker);
        probe.Start();

        for (int i = 0; i < FlagPollRetries && !_policyProbeRan; i++)
        {
            TimerManager.Wait(FlagPollIntervalMs);
        }
        TimerManager.Wait(ExitGraceWaitMs);
    }

    private static void TestThreadRunsUnderRoundRobin()
    {
        RunPolicyProbeWorker();
        Assert.True(_policyProbeRan,
            "a thread created under the Round-Robin policy must be scheduled and run");
    }

    // ===== Round-Robin hooks driven directly (deterministic) =====
    // FIFO order is a property of the policy's run structure, and observing it
    // through real dispatch is racy: a worker preempted anywhere inside
    // InvokeCurrentThreadStart's preamble is re-queued at the TAIL by
    // OnThreadYield, so the order threads reach their delegate is not the
    // order they became ready. These cells drive the hooks on a synthetic
    // PerCpuState instead — the policy keeps all its state in the data slots,
    // so a throwaway instance over throwaway Thread objects exercises the real
    // logic with no timer, no dispatch and no timing assumption at all.

    private static void TestRoundRobinHooksFifoOrder()
    {
        RoundRobinScheduler policy = new();
        PerCpuState state = new();
        policy.InitializeCpu(state);

        SchedulerThread first = new();
        SchedulerThread second = new();
        SchedulerThread third = new();
        policy.OnThreadCreate(state, first);
        policy.OnThreadCreate(state, second);
        policy.OnThreadCreate(state, third);

        policy.OnThreadReady(state, first);
        policy.OnThreadReady(state, second);
        policy.OnThreadReady(state, third);

        Assert.Equal(FifoWorkerCount, policy.GetRunQueueCount(state),
            "each readied thread should be queued once");
        Assert.True(ReferenceEquals(policy.GetRunQueueThread(state, 0), first)
            && ReferenceEquals(policy.GetRunQueueThread(state, 1), second)
            && ReferenceEquals(policy.GetRunQueueThread(state, 2), third),
            "OnThreadReady must enqueue at the tail, preserving ready order");

        Assert.True(ReferenceEquals(policy.PickNext(state), first),
            "PickNext must dequeue the head (first ready runs first)");
        Assert.True(ReferenceEquals(policy.PickNext(state), second),
            "PickNext must continue in FIFO order");
        Assert.True(ReferenceEquals(policy.PickNext(state), third),
            "PickNext must continue in FIFO order");
        Assert.True(policy.PickNext(state) == null,
            "an empty run queue must pick nothing (the mechanism runs idle)");
    }

    private static void TestRoundRobinHooksReadyIsIdempotent()
    {
        RoundRobinScheduler policy = new();
        PerCpuState state = new();
        policy.InitializeCpu(state);

        SchedulerThread thread = new();
        policy.OnThreadCreate(state, thread);

        policy.OnThreadReady(state, thread);
        policy.OnThreadReady(state, thread);

        Assert.Equal(1, policy.GetRunQueueCount(state),
            "a thread readied twice must not occupy two queue slots (it would get double turns)");
    }

    private static void TestRoundRobinHooksQuantumAccounting()
    {
        RoundRobinScheduler policy = new();
        PerCpuState state = new();
        policy.InitializeCpu(state);

        SchedulerThread running = new();
        SchedulerThread waiting = new();
        policy.OnThreadCreate(state, running);
        policy.OnThreadCreate(state, waiting);
        policy.OnThreadReady(state, waiting);

        // The quantum spans two ticks, so the first must not preempt.
        bool midQuantum = policy.OnTick(state, running, SchedulerManager.DefaultQuantumNs);
        bool atExpiry = policy.OnTick(state, running, SchedulerManager.DefaultQuantumNs);

        Assert.False(midQuantum, "a half-spent quantum must not request a reschedule");
        Assert.True(atExpiry, "quantum expiry with another thread waiting must request a reschedule");
        Assert.Equal(2UL * SchedulerManager.DefaultQuantumNs, running.TotalRuntime,
            "OnTick must charge the elapsed time to the running thread");

        // Sole runnable thread: expiry grants a fresh slice in place rather
        // than bouncing through the idle thread and back.
        policy.PickNext(state);
        SchedulerThread alone = new();
        policy.OnThreadCreate(state, alone);
        bool aloneAtExpiry = policy.OnTick(state, alone, RoundRobinScheduler.QuantumNs);

        Assert.False(aloneAtExpiry,
            "quantum expiry with an empty run queue must not request a pointless switch");
    }

    private static void TestRoundRobinHooksBlockAndYield()
    {
        RoundRobinScheduler policy = new();
        PerCpuState state = new();
        policy.InitializeCpu(state);

        SchedulerThread parked = new();
        SchedulerThread other = new();
        policy.OnThreadCreate(state, parked);
        policy.OnThreadCreate(state, other);
        policy.OnThreadReady(state, parked);
        policy.OnThreadReady(state, other);

        policy.OnThreadBlocked(state, parked);

        Assert.Equal(1, policy.GetRunQueueCount(state),
            "a blocked thread must leave the run queue");
        Assert.True(ReferenceEquals(policy.GetRunQueueThread(state, 0), other),
            "blocking must remove the blocked thread, not its neighbour");

        // A preempted-but-still-runnable thread goes to the tail, behind the
        // thread that was already waiting — that rotation is Round-Robin.
        policy.OnThreadYield(state, parked);
        Assert.True(ReferenceEquals(policy.GetRunQueueThread(state, 0), other)
            && ReferenceEquals(policy.GetRunQueueThread(state, 1), parked),
            "OnThreadYield must re-enqueue at the tail");

        policy.OnThreadExit(state, other);
        Assert.Equal(1, policy.GetRunQueueCount(state),
            "an exited thread must leave the run queue");
        Assert.True(other.SchedulerData == null,
            "OnThreadExit must drop the thread's bookkeeping");
    }

    // ===== Round-Robin live dispatch =====
    private static volatile int _fifoRecorded;

    private static void FifoWorker()
    {
        using (SchedulerManager.MaskInterrupts())
        {
            _fifoRecorded++;
        }
    }

    private static void TestRoundRobinDispatchesEveryThread()
    {
        _fifoRecorded = 0;

        // Liveness, not order: FIFO bounds every thread's wait at
        // quantum * queue depth, so all three must reach their delegate.
        // The exact order they get there is asserted deterministically in the
        // hook cells above, where no dispatch preamble can perturb it.
        SysThread w0 = new(FifoWorker);
        SysThread w1 = new(FifoWorker);
        SysThread w2 = new(FifoWorker);
        w0.Start();
        w1.Start();
        w2.Start();

        for (int i = 0; i < FlagPollRetries && _fifoRecorded < FifoWorkerCount; i++)
        {
            TimerManager.Wait(FlagPollIntervalMs);
        }
        TimerManager.Wait(ExitGraceWaitMs);

        Assert.Equal(FifoWorkerCount, _fifoRecorded,
            "Round-Robin must dispatch every ready thread (no starvation)");
    }

    // ===== Round-Robin quantum preemption =====
    private static volatile bool _preemptStop;
    private static volatile bool _preemptDone;
    private static volatile uint _preemptCounter;

    private static void PreemptProbeSpinner()
    {
        while (!_preemptStop)
        {
            _preemptCounter++;
        }
        _preemptDone = true;
    }

    private static void TestRoundRobinQuantumPreemption()
    {
        _preemptStop = false;
        _preemptDone = false;
        _preemptCounter = 0;

        SysThread spinner = new(PreemptProbeSpinner);
        spinner.Start();

        // The spinner never blocks, so every sample below requires the tick
        // to preempt it at quantum expiry and rotate main back in — merely
        // reaching the asserts proves FIFO's bounded latency for main.
        TimerManager.Wait(PreemptSampleIntervalMs);
        uint sample1 = _preemptCounter;
        TimerManager.Wait(PreemptSampleIntervalMs);
        uint sample2 = _preemptCounter;
        TimerManager.Wait(PreemptSampleIntervalMs);
        uint sample3 = _preemptCounter;

        _preemptStop = true;
        for (int i = 0; i < FlagPollRetries && !_preemptDone; i++)
        {
            TimerManager.Wait(FlagPollIntervalMs);
        }
        TimerManager.Wait(ExitGraceWaitMs);

        Assert.True(_preemptDone, "the preempted spinner should observe stop and exit");
        Assert.True(sample1 != sample2 && sample2 != sample3,
            "the spinner must keep progressing between main-thread samples (quantum rotation)");
    }

    private static void TestRoundRobinEqualShare()
    {
        RunTwoSpinnersMeasured(LowTickets, LowTickets);

        Assert.True(_spinADone && _spinBDone, "both measured spinners should finish the window");
        Assert.True(_spinACount > 0 && _spinBCount > 0,
            "both spinners must make progress under Round-Robin");
        Assert.True(_spinACount * PolicySkewDenominator < _spinBCount * PolicySkewNumerator
            && _spinBCount * PolicySkewDenominator < _spinACount * PolicySkewNumerator,
            "equal-quantum turns must yield shares within the 1.5x band under Round-Robin");
    }

    private static void TestRoundRobinIgnoresPriority()
    {
        RunTwoSpinnersMeasured(HighTickets, LowTickets);

        Assert.True(_spinADone && _spinBDone, "both measured spinners should finish the window");
        Assert.True(_observedPriorityA == 0 && _observedPriorityB == 0,
            "Round-Robin defines no priorities, so GetPriority should report 0 for both");
        Assert.True(_spinACount * PolicySkewDenominator < _spinBCount * PolicySkewNumerator
            && _spinBCount * PolicySkewDenominator < _spinACount * PolicySkewNumerator,
            "a 4x priority request must not push Round-Robin past the share ratio Stride clears");
    }

    // ===== Round-Robin run-queue diagnostics =====
    private static volatile bool _gateRelease;
    private static volatile bool _gateAStarted, _gateBStarted, _gateCStarted;
    private static SchedulerThread? _gateAThread, _gateBThread, _gateCThread;

    private static void GateWorkerA()
    {
        _gateAThread = CurrentSchedulerThread();
        _gateAStarted = true;
        while (!_gateRelease)
        {
            // stay runnable so main can observe us in the run queue
        }
    }

    private static void GateWorkerB()
    {
        _gateBThread = CurrentSchedulerThread();
        _gateBStarted = true;
        while (!_gateRelease)
        {
            // stay runnable so main can observe us in the run queue
        }
    }

    private static void GateWorkerC()
    {
        _gateCThread = CurrentSchedulerThread();
        _gateCStarted = true;
        while (!_gateRelease)
        {
            // stay runnable so main can observe us in the run queue
        }
    }

    private static void TestRoundRobinRunQueueDiagnostics()
    {
        _gateRelease = false;
        _gateAStarted = _gateBStarted = _gateCStarted = false;
        _gateAThread = _gateBThread = _gateCThread = null;

        SysThread a = new(GateWorkerA);
        SysThread b = new(GateWorkerB);
        SysThread c = new(GateWorkerC);
        a.Start();
        b.Start();
        c.Start();

        for (int i = 0; i < FlagPollRetries && !(_gateAStarted && _gateBStarted && _gateCStarted); i++)
        {
            TimerManager.Wait(FlagPollIntervalMs);
        }
        Assert.True(_gateAStarted && _gateBStarted && _gateCStarted, "all gate spinners should start");

        IScheduler scheduler = SchedulerManager.Current!;
        PerCpuState state = SchedulerManager.GetCpuState(SchedulerManager.GetCurrentCpuId())!;

        // Main is running, so all three spinners are Ready and must be queued.
        Assert.True(scheduler.GetRunQueueCount(state) >= FifoWorkerCount,
            "the run queue should hold the three spinning workers");
        Assert.True(RunQueueHolds(_gateAThread) && RunQueueHolds(_gateBThread) && RunQueueHolds(_gateCThread),
            "each spinning worker should be visible through the diagnostics hooks");
        Assert.True(scheduler.GetRunQueueThread(state, OutOfRangeQueueIndex) == null,
            "an out-of-range run-queue index must read as null");

        _gateRelease = true;
        for (int i = 0; i < FlagPollRetries
            && (RunQueueHolds(_gateAThread) || RunQueueHolds(_gateBThread) || RunQueueHolds(_gateCThread)); i++)
        {
            TimerManager.Wait(FlagPollIntervalMs);
        }
        TimerManager.Wait(ExitGraceWaitMs);

        Assert.True(!RunQueueHolds(_gateAThread) && !RunQueueHolds(_gateBThread) && !RunQueueHolds(_gateCThread),
            "exited workers must leave the run queue");
    }

    // ===== Round-Robin blocked-thread queue membership =====
    private static Cosmos.Kernel.Core.Scheduler.Mutex? _blockProbeMutex;
    private static volatile bool _blockWorkerStarted;
    private static volatile bool _blockWorkerThrough;
    private static volatile bool _blockRelease;
    private static SchedulerThread? _blockWorkerThread;

    private static void BlockProbeWorker()
    {
        _blockWorkerThread = CurrentSchedulerThread();
        _blockWorkerStarted = true;
        _blockProbeMutex!.Acquire();   // parks: main holds the mutex
        _blockWorkerThrough = true;
        while (!_blockRelease)
        {
            // stay runnable so main can observe us back in the run queue
        }
        _blockProbeMutex.Release();
    }

    private static void TestRoundRobinBlockedLeavesQueue()
    {
        _blockProbeMutex = new Cosmos.Kernel.Core.Scheduler.Mutex();
        _blockWorkerStarted = false;
        _blockWorkerThrough = false;
        _blockRelease = false;
        _blockWorkerThread = null;

        _blockProbeMutex.Acquire();   // uncontended: taken immediately

        SysThread worker = new(BlockProbeWorker);
        worker.Start();

        for (int i = 0; i < FlagPollRetries && !_blockWorkerStarted; i++)
        {
            TimerManager.Wait(FlagPollIntervalMs);
        }
        Assert.True(_blockWorkerStarted, "the block-probe worker should start");
        if (_blockWorkerThread == null)
        {
            // Asserts don't throw in this framework; bail before dereferencing,
            // and release so the parked worker can't leak into the next cell.
            _blockRelease = true;
            _blockProbeMutex.Release();
            return;
        }

        for (int i = 0; i < FlagPollRetries
            && _blockWorkerThread!.State != Cosmos.Kernel.Core.Scheduler.SchedulerThreadState.Blocked; i++)
        {
            TimerManager.Wait(FlagPollIntervalMs);
        }
        Assert.True(_blockWorkerThread!.State == Cosmos.Kernel.Core.Scheduler.SchedulerThreadState.Blocked,
            "the worker should park on the held mutex");
        Assert.False(RunQueueHolds(_blockWorkerThread),
            "a mutex-parked thread must leave the Round-Robin run queue");

        _blockProbeMutex.Release();   // hand-off wakes the parked worker

        for (int i = 0; i < FlagPollRetries && !_blockWorkerThrough; i++)
        {
            TimerManager.Wait(FlagPollIntervalMs);
        }
        Assert.True(_blockWorkerThrough, "the woken worker should acquire the handed-off mutex");
        Assert.True(RunQueueHolds(_blockWorkerThread),
            "a woken, spinning thread must re-enter the Round-Robin run queue");

        _blockRelease = true;
        for (int i = 0; i < FlagPollRetries && RunQueueHolds(_blockWorkerThread); i++)
        {
            TimerManager.Wait(FlagPollIntervalMs);
        }
        TimerManager.Wait(ExitGraceWaitMs);
    }

    private static void TestSetSchedulerRestoresStride()
    {
        // Restoring the boot default needs Core's internal StrideScheduler
        // type (this suite is InternalsVisibleTo); a user kernel installs its
        // policy once at boot and never swaps back. The swap leaves the
        // remaining lifecycle (Finish/AfterRun) on the stock policy.

        // Every Round-Robin worker above exited (each cell waits for it), so
        // the run queue is empty here. That is a statement about the cells
        // above, not a precondition of the swap: SetScheduler re-homes every
        // live thread, and SetScheduler_RehomesLiveThreads below keeps one
        // alive across both directions on purpose.
        PerCpuState preSwapState = SchedulerManager.GetCpuState(SchedulerManager.GetCurrentCpuId())!;
        Assert.Equal(0, SchedulerManager.Current!.GetRunQueueCount(preSwapState),
            "every Round-Robin worker of the cells above should have exited");

        Cosmos.Kernel.Core.Scheduler.Stride.StrideScheduler stride = new();
        using (SchedulerManager.MaskInterrupts())
        {
            SchedulerManager.SetScheduler(stride);
        }

        Assert.True(ReferenceEquals(SchedulerManager.Current, stride),
            "SchedulerManager.Current should be the restored Stride instance");
        Assert.Equal("Stride", SchedulerManager.Current!.Name,
            "the restored policy should report the Stride name");

        RunPolicyProbeWorker();
        Assert.True(_policyProbeRan,
            "a thread created after restoring Stride must be scheduled and run");
    }

    private static volatile bool _rehomeStop;
    private static volatile bool _rehomeDone;
    private static volatile uint _rehomeCounter;

    private static void RehomeProbeSpinner()
    {
        while (!_rehomeStop)
        {
            _rehomeCounter++;
        }
        _rehomeDone = true;
    }

    // A thread alive across a policy swap must keep running under the new
    // policy. While main (the boot thread, which is the one calling
    // SetScheduler) runs, the spinner sits in the installed policy's run
    // queue. Before SetScheduler re-homed live threads, ShutdownCpu discarded
    // that queue with the spinner in it: the thread stayed Ready, no hook ever
    // saw it again, and its counter froze. Both directions are checked, so
    // the stock Stride policy is exercised as the incoming side too.
    private static void TestSetSchedulerRehomesLiveThreads()
    {
        _rehomeStop = false;
        _rehomeDone = false;
        _rehomeCounter = 0;

        SysThread spinner = new(RehomeProbeSpinner);
        spinner.Start();
        TimerManager.Wait(PreemptSampleIntervalMs);
        uint underStride = _rehomeCounter;

        RoundRobinScheduler roundRobin = new();
        using (SchedulerManager.MaskInterrupts())
        {
            SchedulerManager.SetScheduler(roundRobin);
        }
        TimerManager.Wait(PreemptSampleIntervalMs);
        uint underRoundRobin = _rehomeCounter;

        Cosmos.Kernel.Core.Scheduler.Stride.StrideScheduler stride = new();
        using (SchedulerManager.MaskInterrupts())
        {
            SchedulerManager.SetScheduler(stride);
        }
        TimerManager.Wait(PreemptSampleIntervalMs);
        uint backUnderStride = _rehomeCounter;

        _rehomeStop = true;
        for (int i = 0; i < FlagPollRetries && !_rehomeDone; i++)
        {
            TimerManager.Wait(FlagPollIntervalMs);
        }
        TimerManager.Wait(ExitGraceWaitMs);

        Assert.True(underStride > 0, "the spinner must run under the boot policy before the swap");
        Assert.True(underRoundRobin > underStride,
            "a thread alive across the swap must keep running under Round-Robin");
        Assert.True(backUnderStride > underRoundRobin,
            "a thread alive across the swap back must keep running under Stride");
        Assert.True(_rehomeDone, "the re-homed spinner should observe stop and exit");
    }
}
