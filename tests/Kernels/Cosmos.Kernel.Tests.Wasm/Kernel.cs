// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Runtime.InteropServices;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.Tests.Wasm.Benchmark;
using Cosmos.Kernel.Tests.Wasm.Modules;
using Cosmos.Kernel.Tests.Wasm.Vms;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.Wasm;

/// <summary>
/// Runs the same two wasm modules on DotWasm and on each WACS interpreter
/// mode. Every VM loads them, then runs each workload once to warm up and
/// <see cref="TimedRuns"/> more times, each run checked against the checksum
/// of the native build. A test's duration covers all its runs; the table
/// printed before the suite finishes holds the best run of each.
/// </summary>
public class Kernel : Sys.Kernel
{
    private const int TimedRuns = 3;
    private const long MicrosecondsPerSecond = 1_000_000;

    protected override void BeforeRun()
    {
        IWasmVm[] vms =
        [
            new DotWasmVm(),
            new WacsVm(WacsMode.Polymorphic),
            new WacsVm(WacsMode.PolymorphicSuper),
            new WacsVm(WacsMode.Switch),
            new WacsVm(WacsMode.SwitchSuper),
        ];
        Workload[] workloads = Workload.CreateAll(RuntimeInformation.ProcessArchitecture == Architecture.Arm64);
        byte[] benchModule = WasmModules.Bench.ToArray();
        byte[] hostModule = WasmModules.Host.ToArray();
        BenchmarkReport report = new(vms, workloads, TimedRuns);

        TR.Start("Wasm Tests", expectedTests: (ushort)(vms.Length * (workloads.Length + 1)));

        for (int v = 0; v < vms.Length; v++)
        {
            IWasmVm vm = vms[v];
            int vmIndex = v;
            bool loaded = false;
            TR.Run($"{vm.Name}_Load", () =>
            {
                loaded = Load(vm, benchModule, hostModule, report, vmIndex);
            });

            for (int w = 0; w < workloads.Length; w++)
            {
                Workload workload = workloads[w];
                int workloadIndex = w;
                string testName = $"{vm.Name}_{workload.Export}";
                if (!loaded)
                {
                    TR.Skip(testName, $"{vm.Name} did not load the modules");
                    continue;
                }

                TR.Run(testName, () =>
                {
                    RunWorkload(vm, workload, report, vmIndex, workloadIndex);
                });
            }
        }

        report.Print();
        TR.Finish();
    }

    protected override void Run()
    {
        Stop();
    }

    protected override void AfterRun()
    {
        TR.Complete();
        Sys.Power.Halt();
    }

    private static bool Load(IWasmVm vm, byte[] benchModule, byte[] hostModule, BenchmarkReport report, int vmIndex)
    {
        long start = Stopwatch.GetTimestamp();
        try
        {
            vm.Load(benchModule, hostModule);
        }
        catch (Exception e)
        {
            Fail($"{vm.Name} load", e);
            return false;
        }

        report.RecordLoad(vmIndex, ElapsedMicroseconds(start));
        return true;
    }

    private static void RunWorkload(IWasmVm vm, Workload workload, BenchmarkReport report, int vmIndex, int workloadIndex)
    {
        long best = long.MaxValue;
        for (int run = 0; run <= TimedRuns; run++)
        {
            long start = Stopwatch.GetTimestamp();
            uint result;
            try
            {
                result = (uint)vm.Invoke(workload.Module, workload.Export, workload.Arguments);
            }
            catch (Exception e)
            {
                Fail($"{vm.Name} {workload.Label}", e);
                return;
            }

            long elapsed = ElapsedMicroseconds(start);
            if (result != workload.Expected)
            {
                Assert.Fail($"{vm.Name} {workload.Label} returned {result}, expected {workload.Expected}");
                return;
            }

            // Run 0 warms up: caches, lazily built invokers, the switch's tables.
            if (run > 0)
            {
                best = Math.Min(best, elapsed);
            }
        }

        report.RecordWorkload(vmIndex, workloadIndex, best);
    }

    private static void Fail(string what, Exception e)
    {
        string message = $"{what} threw {e.GetType().Name}: {e.Message}";
        Log.WriteString($"[Wasm] {message}\n");
        Assert.Fail(message);
    }

    private static long ElapsedMicroseconds(long start)
    {
        return (Stopwatch.GetTimestamp() - start) * MicrosecondsPerSecond / Stopwatch.Frequency;
    }
}
