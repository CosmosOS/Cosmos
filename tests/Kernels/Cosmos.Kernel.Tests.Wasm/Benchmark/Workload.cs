// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Tests.Wasm.Modules;

namespace Cosmos.Kernel.Tests.Wasm.Benchmark;

/// <summary>
/// One export call timed on every VM, with the checksum the native build of
/// the same C source returns for those arguments.
/// </summary>
internal sealed class Workload
{
    public required ModuleKind Module { get; init; }

    public required string Export { get; init; }

    public required int[] Arguments { get; init; }

    public required uint Expected { get; init; }

    /// <summary>The call as the report prints it, <c>fib(27)</c>.</summary>
    public string Label => $"{Export}({string.Join(',', Arguments)})";

    /// <summary>
    /// The suite's workloads: full size on x64, which runs under KVM, and
    /// about an eighth of the work on arm64, which QEMU emulates. The
    /// expected checksums come from the native build of bench.c and host.c.
    /// </summary>
    public static Workload[] CreateAll(bool reduced)
    {
        return reduced
            ?
            [
                Bench("fib", [23], 28657),
                Bench("lcg", [125_000], 2534285970),
                Bench("sieve", [60_000], 6057),
                Bench("matmul", [32], 196097),
                Bench("mandel", [48, 32, 64], 32852),
                Bench("crc32", [8192, 4], 2076403928),
                Bench("grow", [1], 1),
                Host("host_calls", [40_000], 799980000),
            ]
            :
            [
                Bench("fib", [27], 196418),
                Bench("lcg", [1_000_000], 3677697813),
                Bench("sieve", [500_000], 41538),
                Bench("matmul", [64], 1572090),
                Bench("mandel", [128, 96, 64], 258320),
                Bench("crc32", [65536, 4], 836731736),
                Bench("grow", [1], 1),
                Host("host_calls", [300_000], 2050177040),
            ];
    }

    private static Workload Bench(string export, int[] arguments, uint expected)
    {
        return new Workload { Module = ModuleKind.Bench, Export = export, Arguments = arguments, Expected = expected };
    }

    private static Workload Host(string export, int[] arguments, uint expected)
    {
        return new Workload { Module = ModuleKind.Host, Export = export, Arguments = arguments, Expected = expected };
    }
}
