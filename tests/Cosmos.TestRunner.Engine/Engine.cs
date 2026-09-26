using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cosmos.TestRunner.Engine.Hosts;
using Cosmos.TestRunner.Engine.OutputHandlers;
using Cosmos.TestRunner.Engine.Protocol;
using Cosmos.TestRunner.Protocol;
using Cosmos.Tools.Launcher;

namespace Cosmos.TestRunner.Engine;

/// <summary>
/// Main test runner engine - orchestrates build, launch, monitor, and result collection
/// </summary>
public partial class Engine
{
    /// <summary>Size of each per-profile sparse raw test disk image (256 MiB).</summary>
    private const long TestDiskSizeBytes = 256L * 1024 * 1024;

    /// <summary>
    /// Cap on the number of boots a single suite can ask for. Any test that
    /// triggers a guest reboot/shutdown (Power.Reboot, Power.Shutdown) ends
    /// its boot without emitting the suite-end marker; the engine then
    /// re-launches the kernel, advancing <c>skip=N</c> on the Limine cmdline
    /// so the kernel knows which destructive test already fired.
    /// </summary>
    private const int MaxBoots = 4;

    /// <summary>Maximum plausible excess of ExpectedTestCount over the tests that actually ran; larger gaps are treated as UART corruption of the TestSuiteStart count field.</summary>
    private const int ExpectedTestCountCorruptionSlack = 10000;

    /// <summary>Scale factor converting a 0-1 ratio to a percentage.</summary>
    private const int PercentScale = 100;

    /// <summary>Minimum tab-separated field count of a coverage-map.txt line (id, assembly, type, method).</summary>
    private const int CoverageMapMinFields = 4;

    private readonly TestConfiguration _config;
    private readonly IQemuHost _qemuHost;
    private readonly OutputHandlerBase _outputHandler;

    public Engine(TestConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));

        // Select QEMU host based on architecture
        _qemuHost = _config.Architecture.ToLowerInvariant() switch
        {
            "x64" => new QemuX64Host(),
            "arm64" => new QemuARM64Host(),
            _ => throw new ArgumentException($"Unsupported architecture: {_config.Architecture}")
        };

        // Setup output handler(s)
        _outputHandler = SetupOutputHandler();
    }

    private OutputHandlerBase SetupOutputHandler()
    {
        // If user provided a custom handler, use it
        if (_config.OutputHandler != null)
        {
            return _config.OutputHandler;
        }

        // Default: console output
        var consoleHandler = new OutputHandlerConsole(useColors: true, verbose: false);

        // If XML output requested, multiplex console + XML
        if (!string.IsNullOrEmpty(_config.XmlOutputPath))
        {
            var xmlHandler = new OutputHandlerXml(_config.XmlOutputPath);
            return new MultiplexingOutputHandler(consoleHandler, xmlHandler);
        }

        return consoleHandler;
    }

    /// <summary>
    /// Main execution flow: Build → (per profile) Launch → Monitor → Results → Aggregate
    /// </summary>
    public async Task<TestResults> ExecuteAsync()
    {
        Console.WriteLine($"[Engine] Starting test execution for {_config.KernelProjectPath}");
        Console.WriteLine($"[Engine] Architecture: {_config.Architecture}");
        Console.WriteLine($"[Engine] Timeout: {_config.TimeoutSeconds}s");

        var stopwatch = Stopwatch.StartNew();

        // Get suite name from project path (use GetFileName, not GetFileNameWithoutExtension,
        // because the path is a directory like "Cosmos.Kernel.Tests.HelloWorld" and
        // GetFileNameWithoutExtension would strip ".HelloWorld" as an extension)
        string suiteName = Path.GetFileName(_config.KernelProjectPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

        try
        {
            // Notify start
            _outputHandler.OnTestSuiteStart(suiteName, _config.Architecture);

            // Step 1: Build kernel to ISO
            Console.WriteLine("[Engine] Building kernel...");
            string isoPath = await BuildKernelAsync();
            Console.WriteLine($"[Engine] Build complete: {isoPath}");

            // Step 2: Run the kernel once per QEMU profile the suite opts into
            // (CosmosTestProfile + CosmosTestModifier items in the csproj,
            // resolved against tests/profiles.json). A suite that opts into
            // nothing gets a single anonymous profile.
            IReadOnlyList<TestProfile> profiles = TestProfileLoader.LoadFor(_config.KernelProjectPath, _config.Architecture);
            if (profiles.Count > 1)
            {
                Console.WriteLine($"[Engine] {profiles.Count} QEMU profiles declared: {string.Join(", ", profiles.Select(p => p.Name))}");
            }

            TestResults results = new()
            {
                SuiteName = suiteName,
                Architecture = _config.Architecture
            };

            for (int p = 0; p < profiles.Count; p++)
            {
                TestProfile profile = profiles[p];
                if (!profile.IsDefault)
                {
                    Console.WriteLine($"[Engine] === Profile {p + 1}/{profiles.Count}: {profile.Name} ===");
                }

                Console.WriteLine("[Engine] Launching QEMU...");
                QemuRunResult qemuResult = await LaunchAndMonitorAsync(isoPath, profile);
                Console.WriteLine($"[Engine] QEMU execution complete (Exit: {qemuResult.ExitCode}, TimedOut: {qemuResult.TimedOut})");

                Console.WriteLine("[Engine] Parsing test results...");
                TestResults profileResults = ParseResults(qemuResult);
                MergeProfileResults(results, profileResults, profile);
            }

            results.TotalDuration = stopwatch.Elapsed;

            Console.WriteLine($"[Engine] Results: {results.PassedTests}/{results.TotalTests} passed");

            // Report coverage if data was received
            if (results.CoverageHitMethodIds.Count > 0)
            {
                ReportCoverage(results);
            }

            // Notify individual test results
            foreach (var test in results.Tests)
            {
                _outputHandler.OnTestStart(test.TestNumber, test.TestName);

                switch (test.Status)
                {
                    case TestStatus.Passed:
                        _outputHandler.OnTestPass(test.TestNumber, test.TestName, test.DurationMs);
                        break;
                    case TestStatus.Failed:
                        _outputHandler.OnTestFail(test.TestNumber, test.TestName, test.ErrorMessage, test.DurationMs);
                        break;
                    case TestStatus.Skipped:
                        _outputHandler.OnTestSkip(test.TestNumber, test.TestName, test.ErrorMessage);
                        break;
                }
            }

            // Notify end
            _outputHandler.OnTestSuiteEnd(results);
            _outputHandler.Complete();

            // Step 4: Cleanup (optional)
            if (!_config.KeepBuildArtifacts && !string.IsNullOrEmpty(isoPath))
            {
                CleanupBuildArtifacts(isoPath);
            }

            return results;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Engine] ERROR: {ex.Message}");
            _outputHandler.OnError(ex.Message);

            var results = new TestResults
            {
                SuiteName = suiteName,
                Architecture = _config.Architecture,
                ErrorMessage = ex.Message,
                TotalDuration = stopwatch.Elapsed
            };

            _outputHandler.OnTestSuiteEnd(results);
            _outputHandler.Complete();

            return results;
        }
    }

    private async Task<QemuRunResult> LaunchAndMonitorAsync(string isoPath, TestProfile profile)
    {
        // Setup UART log path. When several profiles run back-to-back, each
        // gets its own log file so a failure in one doesn't lose the other's
        // output and the per-profile boot-N derivatives stay disjoint.
        string baseUartLogPath = _config.UartLogPath;
        if (string.IsNullOrEmpty(baseUartLogPath))
        {
            string baseName = profile.IsDefault ? "uart.log" : $"uart-{profile.Name}.log";
            baseUartLogPath = Path.Combine(
                Path.GetDirectoryName(isoPath) ?? ".",
                baseName
            );
        }
        else if (!profile.IsDefault)
        {
            string dir = Path.GetDirectoryName(baseUartLogPath) ?? ".";
            string stem = Path.GetFileNameWithoutExtension(baseUartLogPath);
            string ext = Path.GetExtension(baseUartLogPath);
            baseUartLogPath = Path.Combine(dir, $"{stem}-{profile.Name}{ext}");
        }

        // Detect if this is a network test kernel
        bool enableNetworkTesting = _config.KernelProjectPath.Contains("Network", StringComparison.OrdinalIgnoreCase);

        // Allocate fresh 256 MiB sparse raw images per profile so back-to-back
        // profile runs don't see each other's writes.
        IReadOnlyList<DiskAttachment> disks = CreateProfileDisks(profile);

        StringBuilder combinedLog = new();
        QemuRunResult? lastResult = null;

        try
        {
            for (int boot = 0; boot < MaxBoots; boot++)
            {
                string bootIsoPath = await PrepareBootIsoAsync(isoPath, boot, profile);
                string bootLogPath = boot == 0 ? baseUartLogPath : $"{baseUartLogPath}.boot{boot}";

                if (boot > 0)
                {
                    Console.WriteLine($"[Engine] Re-launching kernel for boot #{boot} (skip={boot})");
                }

                // Every boot starts with the sticks plugged in, and a QEMU of
                // its own to connect a monitor.
                await using QemuHotPlug? hotPlug = QemuHotPlug.For(disks);
                QemuRunResult result = await _qemuHost.RunKernelAsync(
                    bootIsoPath, bootLogPath, _config.TimeoutSeconds, _config.ShouldShowDisplay, enableNetworkTesting, disks, profile.MachineOptions,
                    new ProfileDevices(profile.NetworkCard, profile.KeyboardDevice, profile.MouseDevice, profile.VgaAdapter, profile.GpuDevice, profile.Devices, profile.UsbDevices),
                    hotPlug);

                combinedLog.Append(result.UartLog);
                lastResult = result;

                // Suite finished cleanly (kernel emitted the end marker).
                if (result.SuiteMarkerSeen)
                {
                    break;
                }

                // No suite-end marker: either the boot reached a destructive test
                // (RunDestructive — Power.Reboot/Shutdown) and the guest exited /
                // hung on purpose, or the kernel crashed mid-suite. The two are
                // distinguished by the TestDestructiveReached sentinel emitted by
                // RunDestructive immediately before invoking the destructive
                // action. Without that marker, treat this boot as a real failure
                // and let the suite fail — re-launching would just mask the bug.
                if (!UartLogShowsDestructiveProgress(result.UartLog))
                {
                    break;
                }

                string exitReason = result.TimedOut ? "timed out" : "guest exited";
                Console.WriteLine($"[Engine] Boot #{boot} {exitReason} after a destructive test was reached — re-launching.");
            }
        }
        finally
        {
            // The images must survive every boot of the profile (the
            // destructive reboot cell reads boot-0 writes back on boot 1),
            // so they are only deleted once the whole profile run is over.
            CleanupProfileDisks(disks);
        }

        return new QemuRunResult
        {
            ExitCode = lastResult?.ExitCode ?? -1,
            UartLog = combinedLog.ToString(),
            TimedOut = lastResult?.TimedOut ?? false,
            ErrorMessage = lastResult?.ErrorMessage ?? string.Empty,
            SuiteMarkerSeen = lastResult?.SuiteMarkerSeen ?? false
        };
    }

    private void CleanupProfileDisks(IReadOnlyList<DiskAttachment> disks)
    {
        // Same opt-out as the ISO clones: KeepBuildArtifacts pins every
        // run input for post-mortem, including the disk images.
        if (_config.KeepBuildArtifacts)
        {
            return;
        }

        foreach (DiskAttachment disk in disks)
        {
            try
            {
                if (File.Exists(disk.Path))
                {
                    File.Delete(disk.Path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Engine] Warning: failed to delete test disk {disk.Path}: {ex.Message}");
            }
        }
    }

    private IReadOnlyList<DiskAttachment> CreateProfileDisks(TestProfile profile)
    {
        if (profile.Disks.Count == 0)
        {
            return Array.Empty<DiskAttachment>();
        }

        string suite = Path.GetFileName(_config.KernelProjectPath.TrimEnd('/', '\\'));
        string profileTag = profile.IsDefault ? "default" : profile.Name;
        List<DiskAttachment> attachments = new(profile.Disks.Count);

        for (int i = 0; i < profile.Disks.Count; i++)
        {
            TestProfileDisk disk = profile.Disks[i];
            string kindTag = disk.Kind.ToString().ToLowerInvariant();
            string path = Path.Combine(
                Path.GetTempPath(),
                $"cosmos-test-disk-{suite}-{_config.Architecture}-{profileTag}-{kindTag}{i}.img");

            if (File.Exists(path))
            {
                File.Delete(path);
            }
            using (FileStream fs = new(path, FileMode.Create, FileAccess.Write))
            {
                fs.SetLength(TestDiskSizeBytes);
            }
            Console.WriteLine($"[Engine] Created test disk: {path} (256 MiB sparse)");

            attachments.Add(new DiskAttachment
            {
                Path = path,
                Kind = disk.Kind,
                ExtraDeviceOptions = disk.FormatOptions()
            });
        }
        return attachments;
    }

    /// <summary>
    /// Fold one profile's parsed results into the suite-wide aggregate. Test
    /// names get a `[profile]` prefix so the same logical test from different
    /// profiles stays distinguishable, and TestNumber is renumbered across the
    /// combined list so output handlers see a contiguous sequence.
    /// </summary>
    private void MergeProfileResults(TestResults aggregate, TestResults profileResults, TestProfile profile)
    {
        string prefix = profile.IsDefault ? string.Empty : $"[{profile.Name}] ";
        bool isFirstProfile = aggregate.Tests.Count == 0 && aggregate.ExpectedTestCount == 0;

        // A cell whose kernel died before emitting TestSuiteStart (driver
        // bring-up crash, triple fault, QEMU launch failure) contributes
        // zero tests and zero expected count. Without a sentinel it is
        // invisible to AllTestsPassed once other cells contribute passing
        // tests — the exact cells this matrix exists to pin (gicv2/gicv3/
        // acpi-off hardware paths) would vanish from CI without failing it.
        if (profileResults.Tests.Count == 0 && profileResults.ExpectedTestCount == 0)
        {
            profileResults.Tests.Add(new TestResult
            {
                TestName = "boot",
                Status = TestStatus.Failed,
                ErrorMessage = string.IsNullOrEmpty(profileResults.ErrorMessage)
                    ? "kernel produced no test protocol output (crashed or hung before TestSuiteStart)"
                    : profileResults.ErrorMessage,
            });
        }

        foreach (TestResult t in profileResults.Tests)
        {
            t.TestNumber = aggregate.Tests.Count + 1;
            if (prefix.Length > 0)
            {
                t.TestName = prefix + t.TestName;
            }
            aggregate.Tests.Add(t);
        }

        aggregate.ExpectedTestCount += profileResults.ExpectedTestCount;
        aggregate.UartLog += profileResults.UartLog;
        aggregate.CoverageHitMethodIds.AddRange(profileResults.CoverageHitMethodIds);

        if (profileResults.TimedOut)
        {
            aggregate.TimedOut = true;
        }
        if (!string.IsNullOrEmpty(profileResults.ErrorMessage))
        {
            aggregate.ErrorMessage = string.IsNullOrEmpty(aggregate.ErrorMessage)
                ? profileResults.ErrorMessage
                : $"{aggregate.ErrorMessage}; {profileResults.ErrorMessage}";
        }

        // The aggregate is "completed" only when every profile in the suite
        // emitted its end-marker. Seed with the first profile, then AND.
        aggregate.SuiteCompleted = isFirstProfile
            ? profileResults.SuiteCompleted
            : aggregate.SuiteCompleted && profileResults.SuiteCompleted;
    }

    /// <summary>
    /// Returns true if the per-boot UART log contains at least one
    /// TestDestructiveReached frame from the binary protocol (magic 0x19740807
    /// + command 108). Used to distinguish "destructive test was reached, then
    /// the kernel exited/hung as expected" (continue to next boot, advancing
    /// skip=N) from "the kernel crashed or hung in a non-destructive test"
    /// (real failure — bail out and let the suite fail).
    /// </summary>
    private static bool UartLogShowsDestructiveProgress(string uartLog)
    {
        if (string.IsNullOrEmpty(uartLog))
        {
            return false;
        }
        // Magic 0x19740807 little-endian = bytes 07 08 74 19, then command byte.
        // Command 108 = TestDestructiveReached (emitted only by RunDestructive).
        byte[] needle = { Consts.SerialSignatureByte0, Consts.SerialSignatureByte1, Consts.SerialSignatureByte2, Consts.SerialSignatureByte3, Ds2Vs.TestDestructiveReached };
        byte[] haystack = System.Text.Encoding.Latin1.GetBytes(uartLog);
        for (int i = 0; i + needle.Length <= haystack.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < needle.Length; j++)
            {
                if (haystack[i + j] != needle[j]) { match = false; break; }
            }
            if (match)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns the ISO path to use for boot <paramref name="bootIndex"/> of
    /// <paramref name="profile"/>. Boot 0 of a suite with no profiles uses the
    /// as-built ISO unchanged. Otherwise the ISO is cloned and
    /// /boot/limine/limine.conf rewritten with a <c>cmdline:</c> carrying
    /// <c>profile=&lt;name&gt;</c> (so the kernel can assert the cell's hardware
    /// path) and <c>skip=N</c> (so a re-launched boot knows which destructive
    /// test already fired). The profile token is set on every boot of a profiled
    /// cell, including boot 0.
    /// </summary>
    private async Task<string> PrepareBootIsoAsync(string baseIsoPath, int bootIndex, TestProfile profile)
    {
        bool patchProfile = !profile.IsDefault;

        // Fast path: boot 0 of a non-profiled suite needs no cmdline at all.
        if (bootIndex == 0 && !patchProfile)
        {
            return baseIsoPath;
        }

        string tag = profile.IsDefault ? "default" : profile.Name;
        string bootIsoPath = bootIndex == 0
            ? $"{baseIsoPath}.{tag}.iso"
            : $"{baseIsoPath}.{tag}.boot{bootIndex}.iso";
        File.Copy(baseIsoPath, bootIsoPath, overwrite: true);

        // Build this boot's cmdline tokens: the active profile name (for path
        // assertions) and skip=N (for the destructive-test re-launch loop).
        var tokens = new List<string>();
        if (patchProfile)
        {
            tokens.Add($"profile={profile.Name}");
        }
        tokens.Add($"skip={bootIndex}");
        string cmdline = string.Join(' ', tokens);

        // Rewrite /boot/limine/limine.conf from the kernel project's Bootloader
        // template (no need to extract from the ISO). Test templates carry no
        // cmdline line, so append one; if a template does declare cmdline, merge
        // our tokens in after dropping any stale profile=/skip= for idempotency.
        string sourceLimineConf = Path.Combine(_config.KernelProjectPath, "Bootloader", "limine.conf");
        string template = await File.ReadAllTextAsync(sourceLimineConf);
        Match existing = Regex.Match(template, @"^(?<indent>[ \t]*)cmdline:(?<args>.*)$", RegexOptions.Multiline);
        string patched;
        if (existing.Success)
        {
            string priorArgs = Regex.Replace(existing.Groups["args"].Value, @"\s*\b(?:profile|skip)=\S+", string.Empty).Trim();
            string merged = priorArgs.Length > 0 ? $"{priorArgs} {cmdline}" : cmdline;
            string newLine = $"{existing.Groups["indent"].Value}cmdline: {merged}";
            patched = template.Remove(existing.Index, existing.Length).Insert(existing.Index, newLine);
        }
        else
        {
            patched = template + $"\n    cmdline: {cmdline}\n";
        }

        string patchedConfPath = Path.Combine(
            Path.GetDirectoryName(bootIsoPath) ?? ".",
            bootIndex == 0 ? $"limine.{tag}.conf" : $"limine.{tag}.boot{bootIndex}.conf");
        await File.WriteAllTextAsync(patchedConfPath, patched);

        // Splice the patched limine.conf into the cloned ISO. `-boot_image any
        // keep` preserves the existing Limine boot record so the ISO stays
        // bootable; `-map` replaces /boot/limine/limine.conf in place.
        var psi = new ProcessStartInfo
        {
            FileName = "xorriso",
            ArgumentList =
            {
                "-boot_image", "any", "keep",
                "-dev", bootIsoPath,
                "-map", patchedConfPath, "/boot/limine/limine.conf",
                "-commit_eject", "all"
            },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var proc = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start xorriso");
        await proc.WaitForExitAsync();
        if (proc.ExitCode != 0)
        {
            string stderr = await proc.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"xorriso failed (exit {proc.ExitCode}): {stderr}");
        }

        return bootIsoPath;
    }

    private TestResults ParseResults(QemuRunResult qemuResult)
    {
        // Always try to parse UART log, even on timeout - kernel may have completed tests
        var results = UartMessageParser.ParseUartLog(qemuResult.UartLog ?? string.Empty, _config.Architecture);
        results.TimedOut = qemuResult.TimedOut;
        results.UartLog = qemuResult.UartLog ?? string.Empty;
        results.ErrorMessage = qemuResult.ErrorMessage ?? string.Empty;

        // If the suite completed normally (TestSuiteEnd received and validated), all tests ran —
        // no need to synthesise failures for missing tests.
        if (!results.SuiteCompleted && results.ExpectedTestCount > 0 && results.Tests.Count < results.ExpectedTestCount)
        {
            int actualCount = results.Tests.Count;

            // Sanity check: timer interrupts can corrupt the ExpectedTestCount field in the
            // TestSuiteStart message (high byte replaced by '[' = 0x5B from "[GenericTimer]" text).
            // If the expected count is implausibly large compared to what actually ran, ignore it.
            int maxPlausible = actualCount + ExpectedTestCountCorruptionSlack;
            if (results.ExpectedTestCount > maxPlausible)
            {
                Console.WriteLine($"[ParseResults] Warning: ExpectedTestCount={results.ExpectedTestCount} seems corrupted (actual={actualCount}), ignoring.");
            }
            else
            {
                for (int i = actualCount + 1; i <= results.ExpectedTestCount; i++)
                {
                    results.Tests.Add(new TestResult
                    {
                        TestNumber = i,
                        TestName = $"Test {i}",
                        Status = TestStatus.Failed,
                        ErrorMessage = "Test did not execute (kernel crashed or timed out)",
                        DurationMs = 0
                    });
                }
            }
        }

        return results;
    }

    private void ReportCoverage(TestResults results)
    {
        string? mapPath = FindCoverageMap();

        if (mapPath == null)
        {
            Console.WriteLine($"[Coverage] Coverage map not found");
            Console.WriteLine($"[Coverage] {results.CoverageHitMethodIds.Count} methods hit (no method names available)");
            return;
        }

        Console.WriteLine($"[Coverage] Using coverage map: {mapPath}");

        // Parse coverage map — one ID may map to multiple methods (plug aliases share target ID)
        var allMethods = new List<(int Id, string Assembly, string Type, string Method)>();
        foreach (var line in File.ReadAllLines(mapPath))
        {
            if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split('\t');
            if (parts.Length >= CoverageMapMinFields && int.TryParse(parts[0], out int id))
            {
                allMethods.Add((id, parts[1], parts[2], parts[3]));
            }
        }

        int totalMethods = allMethods.Count;
        var hitSet = new HashSet<int>(results.CoverageHitMethodIds.Select(id => (int)id));
        int hitMethods = allMethods.Count(m => hitSet.Contains(m.Id));
        double percentage = totalMethods > 0 ? (double)hitMethods / totalMethods * PercentScale : 0;

        Console.WriteLine($"[Coverage] {hitMethods}/{totalMethods} methods covered ({percentage:F1}%)");

        // Per-assembly breakdown
        var assemblyStats = allMethods
            .GroupBy(m => m.Assembly)
            .Select(g => new
            {
                Assembly = g.Key,
                Total = g.Count(),
                Hit = g.Count(m => hitSet.Contains(m.Id))
            })
            .OrderByDescending(a => a.Total);

        foreach (var asm in assemblyStats)
        {
            double asmPct = asm.Total > 0 ? (double)asm.Hit / asm.Total * PercentScale : 0;
            Console.WriteLine($"[Coverage]   {asm.Assembly}: {asm.Hit}/{asm.Total} ({asmPct:F1}%)");
        }

        // Write coverage results JSON for CI (includes per-method hit data for cross-suite aggregation)
        string coverageOutputPath = Path.Combine(
            Path.GetDirectoryName(_config.XmlOutputPath ?? _config.KernelProjectPath) ?? ".",
            $"coverage-{_config.Architecture}.json");

        try
        {
            // Build per-assembly method lists (all methods + which were hit)
            var assemblyMethods = allMethods
                .GroupBy(m => m.Assembly)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(m => new
                    {
                        Key = $"{m.Type}::{m.Method}",
                        Hit = hitSet.Contains(m.Id)
                    }).ToList()
                );

            using var writer = new StreamWriter(coverageOutputPath);
            writer.WriteLine("{");
            writer.WriteLine($"  \"suite\": \"{results.SuiteName}\",");
            writer.WriteLine($"  \"architecture\": \"{_config.Architecture}\",");
            writer.WriteLine($"  \"totalMethods\": {totalMethods},");
            writer.WriteLine($"  \"hitMethods\": {hitMethods},");
            writer.WriteLine($"  \"percentage\": {percentage:F1},");
            writer.WriteLine("  \"assemblies\": [");

            var asmList = assemblyStats.ToList();
            for (int i = 0; i < asmList.Count; i++)
            {
                var asm = asmList[i];
                double asmPct = asm.Total > 0 ? (double)asm.Hit / asm.Total * PercentScale : 0;
                string comma = i < asmList.Count - 1 ? "," : "";

                // Collect hit method signatures for this assembly
                var methods = assemblyMethods.GetValueOrDefault(asm.Assembly);
                var hitMethodNames = methods?
                    .Where(m => m.Hit)
                    .Select(m => m.Key)
                    .ToList() ?? [];
                var allMethodNames = methods?
                    .Select(m => m.Key)
                    .ToList() ?? [];

                writer.WriteLine($"    {{");
                writer.WriteLine($"      \"name\": \"{EscapeJson(asm.Assembly)}\",");
                writer.WriteLine($"      \"total\": {asm.Total},");
                writer.WriteLine($"      \"hit\": {asm.Hit},");
                writer.WriteLine($"      \"percentage\": {asmPct:F1},");
                writer.WriteLine($"      \"methods\": [");
                for (int j = 0; j < allMethodNames.Count; j++)
                {
                    string mComma = j < allMethodNames.Count - 1 ? "," : "";
                    bool mHit = hitMethodNames.Contains(allMethodNames[j]);
                    writer.WriteLine($"        {{ \"name\": \"{EscapeJson(allMethodNames[j])}\", \"hit\": {(mHit ? "true" : "false")} }}{mComma}");
                }
                writer.WriteLine($"      ]");
                writer.WriteLine($"    }}{comma}");
            }

            writer.WriteLine("  ]");
            writer.WriteLine("}");

            Console.WriteLine($"[Coverage] Report written to: {coverageOutputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Coverage] Warning: Could not write coverage JSON: {ex.Message}");
        }
    }

    /// <summary>
    /// Search for coverage-map.txt in standard and artifacts output layouts.
    /// </summary>
    private string? FindCoverageMap()
    {
        string projectDir = _config.KernelProjectPath;
        string projectName = Path.GetFileName(projectDir);
        string rid = _config.Architecture == "arm64" ? "linux-arm64" : "linux-x64";

        // Candidate paths: standard obj layout and UseArtifactsOutput layout
        string[] candidates =
        [
            Path.Combine(projectDir, "obj", "Debug", "net10.0", rid, "cosmos", "coverage-map.txt"),
            Path.Combine(projectDir, "..", "..", "artifacts", "obj", projectName, $"debug_{rid}", "cosmos", "coverage-map.txt"),
        ];

        foreach (var path in candidates)
        {
            var fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        // Fallback: recursive search from project directory upward
        var dir = new DirectoryInfo(projectDir);
        while (dir != null)
        {
            var artifactsDir = Path.Combine(dir.FullName, "artifacts", "obj");
            if (Directory.Exists(artifactsDir))
            {
                var found = Directory.GetFiles(artifactsDir, "coverage-map.txt", SearchOption.AllDirectories);
                if (found.Length > 0)
                {
                    return found[0];
                }
            }
            dir = dir.Parent;
        }

        return null;
    }

    private void CleanupBuildArtifacts(string isoPath)
    {
        try
        {
            if (File.Exists(isoPath))
            {
                File.Delete(isoPath);
            }

            var outputDir = Path.GetDirectoryName(isoPath);
            if (!string.IsNullOrEmpty(outputDir) && Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, recursive: true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Engine] Warning: Failed to cleanup: {ex.Message}");
        }
    }

    private static string EscapeJson(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
