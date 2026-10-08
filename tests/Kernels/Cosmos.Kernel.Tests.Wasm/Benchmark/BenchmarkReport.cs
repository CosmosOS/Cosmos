// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.Tests.Wasm.Vms;

namespace Cosmos.Kernel.Tests.Wasm.Benchmark;

/// <summary>
/// The times the suite measured, one row per workload plus the load, one
/// column per VM, printed to the serial log as a table.
/// </summary>
internal sealed class BenchmarkReport
{
    private const string Prefix = "[Wasm] ";
    private const int LabelWidth = 22;
    private const int ColumnWidth = 18;
    private const long NotMeasured = -1;

    private readonly string[] _vmNames;
    private readonly string[] _rowLabels;
    private readonly long[][] _microseconds;
    private readonly int _timedRuns;

    public BenchmarkReport(IWasmVm[] vms, Workload[] workloads, int timedRuns)
    {
        _vmNames = new string[vms.Length];
        for (int v = 0; v < vms.Length; v++)
        {
            _vmNames[v] = vms[v].Name;
        }

        _rowLabels = new string[workloads.Length + 1];
        _rowLabels[0] = "load";
        for (int w = 0; w < workloads.Length; w++)
        {
            _rowLabels[w + 1] = workloads[w].Label;
        }

        _microseconds = new long[_rowLabels.Length][];
        for (int row = 0; row < _rowLabels.Length; row++)
        {
            _microseconds[row] = new long[vms.Length];
            Array.Fill(_microseconds[row], NotMeasured);
        }

        _timedRuns = timedRuns;
    }

    public void RecordLoad(int vmIndex, long microseconds)
    {
        _microseconds[0][vmIndex] = microseconds;
    }

    public void RecordWorkload(int vmIndex, int workloadIndex, long microseconds)
    {
        _microseconds[workloadIndex + 1][vmIndex] = microseconds;
    }

    /// <summary>
    /// Prints the table: milliseconds per cell, <c>-</c> where the VM failed,
    /// and the fastest VM of each row.
    /// </summary>
    public void Print()
    {
        Log.WriteString($"{Prefix}Load: one cold run. Workloads: best of {_timedRuns} runs after one warm-up run. Times in ms.\n");

        string header = "workload".PadRight(LabelWidth);
        foreach (string name in _vmNames)
        {
            header += name.PadLeft(ColumnWidth);
        }
        Log.WriteString($"{Prefix}{header}{"fastest".PadLeft(ColumnWidth)}\n");

        for (int row = 0; row < _rowLabels.Length; row++)
        {
            string line = _rowLabels[row].PadRight(LabelWidth);
            int fastest = -1;
            for (int v = 0; v < _vmNames.Length; v++)
            {
                long time = _microseconds[row][v];
                line += (time == NotMeasured ? "-" : FormatMilliseconds(time)).PadLeft(ColumnWidth);
                if (time != NotMeasured && (fastest < 0 || time < _microseconds[row][fastest]))
                {
                    fastest = v;
                }
            }
            Log.WriteString($"{Prefix}{line}{(fastest < 0 ? "-" : _vmNames[fastest]).PadLeft(ColumnWidth)}\n");
        }
    }

    private static string FormatMilliseconds(long microseconds)
    {
        return $"{microseconds / 1000}.{(microseconds % 1000).ToString().PadLeft(3, '0')}";
    }
}
