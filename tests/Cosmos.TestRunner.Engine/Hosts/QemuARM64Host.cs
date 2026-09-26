using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.TestRunner.Engine.Protocol;
using Cosmos.TestRunner.Protocol;
using Cosmos.Tools.Launcher;

namespace Cosmos.TestRunner.Engine.Hosts;

/// <summary>
/// QEMU host for ARM64/AArch64 architecture. Argument construction lives in
/// <see cref="QemuLauncher"/> so this stays in sync with `cosmos run`.
/// </summary>
public class QemuARM64Host : IQemuHost
{
    // Suite-end marker the kernel emits once the whole suite finished:
    // 0xDE 0xAD 0xBE 0xEF 0xCA 0xFE 0xBA 0xBE.
    private static readonly byte[] TestEndMarker = Consts.SuiteEndMarker;

    // Test runner protocol needle: 0x19740807 magic little-endian + command
    // byte (Ds2Vs.TestPass). Used to detect "kernel reached at least one test"
    // so we can declare a stall when UART goes silent — handles destructive
    // ops (e.g. Power.Shutdown's LAI panic) that hang instead of cleanly
    // exiting QEMU.
    private static readonly byte[] TestPassMarker =
    {
        Consts.SerialSignatureByte0,
        Consts.SerialSignatureByte1,
        Consts.SerialSignatureByte2,
        Consts.SerialSignatureByte3,
        Ds2Vs.TestPass
    };

    public string Architecture => "arm64";

    private readonly string? _qemuBinaryOverride;
    private readonly int _memoryMb;

    public QemuARM64Host(
        string? qemuBinary = null,
        string? uefiFirmwarePath = null,
        int memoryMb = QemuHostDefaults.DefaultMemoryMb)
    {
        _qemuBinaryOverride = qemuBinary;
        _memoryMb = memoryMb;
        // uefiFirmwarePath ignored — QemuLauncher.ResolveArm64Firmware() handles it.
    }

    public async Task<QemuRunResult> RunKernelAsync(string isoPath, string uartLogPath, int timeoutSeconds = QemuHostDefaults.DefaultTimeoutSeconds, bool showDisplay = false, bool enableNetworkTesting = false, IReadOnlyList<DiskAttachment>? disks = null, IReadOnlyDictionary<string, string>? machineOptions = null, ProfileDevices? devices = null, QemuHotPlug? hotPlug = null)
    {
        if (!File.Exists(isoPath))
        {
            return new QemuRunResult
            {
                ExitCode = -1,
                ErrorMessage = $"ISO file not found: {isoPath}"
            };
        }

        string? logDir = Path.GetDirectoryName(uartLogPath);
        if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }

        if (File.Exists(uartLogPath))
        {
            File.Delete(uartLogPath);
        }

        QemuLaunchPlan plan;
        try
        {
            plan = await QemuLauncher.BuildAsync(new QemuLaunchOptions
            {
                Architecture = "arm64",
                IsoPath = isoPath,
                MemoryMb = _memoryMb,
                Headless = !showDisplay,
                SerialOutputFile = uartLogPath,
                EnableNetworkTesting = enableNetworkTesting,
                Disks = disks ?? Array.Empty<DiskAttachment>(),
                MachineOptions = machineOptions ?? new Dictionary<string, string>(),
                NetworkCard = devices?.NetworkCard,
                KeyboardDevice = devices?.KeyboardDevice,
                MouseDevice = devices?.MouseDevice,
                VgaAdapter = devices?.VgaAdapter,
                GpuDevice = devices?.GpuDevice,
                Devices = devices?.Devices ?? Array.Empty<DeviceAttachment>(),
                UsbDevices = devices?.UsbDevices ?? Array.Empty<string>(),
                MonitorPort = hotPlug?.Port,
                AllowGuestShutdown = true
            });
        }
        catch (FileNotFoundException ex)
        {
            return new QemuRunResult { ExitCode = -1, ErrorMessage = ex.Message };
        }
        ProcessStartInfo startInfo = QemuLauncher.ToProcessStartInfo(plan);
        if (_qemuBinaryOverride is not null)
        {
            startInfo.FileName = _qemuBinaryOverride;
        }

        using Process process = new() { StartInfo = startInfo };
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(timeoutSeconds));

        UdpTestServer? udpServer = null;
        TcpTestServer? tcpServer = null;
        IcmpTestServer? icmpServer = null;
        if (enableNetworkTesting)
        {
            udpServer = new UdpTestServer();
            tcpServer = new TcpTestServer();
            icmpServer = new IcmpTestServer();
        }

        bool testSuiteCompleted = false;

        try
        {
            // Start test servers for network tests. The ICMP server must be
            // listening before QEMU starts: the stream netdev connects at
            // startup and aborts the VM if the connection is refused.
            udpServer?.Start();
            tcpServer?.Start();
            icmpServer?.Start();

            process.Start();
            hotPlug?.Attach(cts.Token);

            // Capture stderr asynchronously for diagnostics
            Task<string> stderrTask = process.StandardError.ReadToEndAsync();

            // Monitor UART log for the suite-end marker or a stall after a test
            // was reached, while waiting for QEMU to exit on its own.
            Task<UartMonitorOutcome> monitorTask = MonitorUartLogAsync(uartLogPath, hotPlug, cts.Token);
            Task processTask = process.WaitForExitAsync(cts.Token);

            Task completedTask = await Task.WhenAny(monitorTask, processTask);

            if (completedTask == monitorTask)
            {
                UartMonitorOutcome outcome = await monitorTask;
                testSuiteCompleted = outcome == UartMonitorOutcome.EndMarkerSeen;
                if (!process.HasExited)
                {
                    await Task.Delay(QemuHostDefaults.KillGraceDelayMs);
                    process.Kill(entireProcessTree: true);
                    await process.WaitForExitAsync();
                }
            }
            else if (!process.HasExited)
            {
                // Process task completed (process exited on its own — guest reboot/shutdown)
                await processTask;
            }

            // Give UART log a moment to flush
            await Task.Delay(QemuHostDefaults.UartFlushDelayMs);

            if (udpServer is not null)
            {
                await udpServer.StopAsync();
            }

            if (tcpServer is not null)
            {
                await tcpServer.StopAsync();
            }

            if (icmpServer is not null)
            {
                await icmpServer.StopAsync();
            }

            // Log stderr for diagnostics
            string stderr = await stderrTask;
            if (!string.IsNullOrWhiteSpace(stderr))
            {
                Console.WriteLine($"[QEMU stderr] {stderr.Trim()}");
            }

            string uartLog = string.Empty;
            if (File.Exists(uartLogPath))
            {
                uartLog = await File.ReadAllTextAsync(uartLogPath, Encoding.Latin1);
            }

            return new QemuRunResult
            {
                ExitCode = testSuiteCompleted ? 0 : process.ExitCode,
                UartLog = uartLog,
                TimedOut = false,
                SuiteMarkerSeen = testSuiteCompleted
            };
        }
        catch (OperationCanceledException)
        {
            // Timeout - kill QEMU
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }

            // Give UART log a moment to flush
            await Task.Delay(QemuHostDefaults.UartFlushDelayMs);

            if (udpServer is not null)
            {
                await udpServer.StopAsync();
            }

            if (tcpServer is not null)
            {
                await tcpServer.StopAsync();
            }

            if (icmpServer is not null)
            {
                await icmpServer.StopAsync();
            }

            // Read whatever UART output we got
            string uartLog = string.Empty;
            if (File.Exists(uartLogPath))
            {
                uartLog = await File.ReadAllTextAsync(uartLogPath, Encoding.Latin1);
            }

            return new QemuRunResult
            {
                ExitCode = -1,
                UartLog = uartLog,
                TimedOut = true,
                ErrorMessage = $"QEMU timed out after {timeoutSeconds}s"
            };
        }
        catch (Exception ex)
        {
            if (udpServer is not null)
            {
                await udpServer.StopAsync();
            }

            if (tcpServer is not null)
            {
                await tcpServer.StopAsync();
            }

            if (icmpServer is not null)
            {
                await icmpServer.StopAsync();
            }

            return new QemuRunResult
            {
                ExitCode = -1,
                ErrorMessage = $"Failed to run QEMU: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Monitor UART log for the suite-end marker or a stall after a test was
    /// reached. See <see cref="QemuX64Host"/> for the full rationale.
    /// </summary>
    private static async Task<UartMonitorOutcome> MonitorUartLogAsync(string uartLogPath, QemuHotPlug? hotPlug, CancellationToken cancellationToken)
    {
        long lastPosition = 0;
        int endMarkerIndex = 0;
        int testPassMarkerIndex = 0;
        bool sawTestPass = false;
        // See QemuX64Host for the rationale: track the last protocol-frame
        // magic, not raw UART bytes — a hung kernel keeps spamming scheduler
        // text but stops emitting protocol frames.
        DateTime lastMagicAt = DateTime.UtcNow;
        HostRequestScanner hostRequests = new();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (File.Exists(uartLogPath))
                {
                    using FileStream fs = new(uartLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    if (fs.Length > lastPosition)
                    {
                        fs.Seek(lastPosition, SeekOrigin.Begin);
                        byte[] buffer = new byte[fs.Length - lastPosition];
                        int bytesRead = await fs.ReadAsync(buffer, cancellationToken);
                        lastPosition += bytesRead;

                        for (int i = 0; i < bytesRead; i++)
                        {
                            byte b = buffer[i];

                            // The guest's test waits for what it asked, so it is
                            // done now rather than after the run.
                            if (hostRequests.Feed(b) is string request)
                            {
                                await QemuHotPlug.DispatchAsync(hotPlug, request, cancellationToken);
                                lastMagicAt = DateTime.UtcNow;
                            }

                            if (b == TestEndMarker[endMarkerIndex])
                            {
                                endMarkerIndex++;
                                if (endMarkerIndex == TestEndMarker.Length)
                                {
                                    return UartMonitorOutcome.EndMarkerSeen;
                                }
                            }
                            else
                            {
                                endMarkerIndex = (b == TestEndMarker[0]) ? 1 : 0;
                            }

                            if (b == TestPassMarker[testPassMarkerIndex])
                            {
                                testPassMarkerIndex++;
                                if (testPassMarkerIndex == Consts.SerialSignatureLengthBytes)
                                {
                                    lastMagicAt = DateTime.UtcNow;
                                }
                                if (testPassMarkerIndex == TestPassMarker.Length)
                                {
                                    sawTestPass = true;
                                    testPassMarkerIndex = 0;
                                }
                            }
                            else
                            {
                                testPassMarkerIndex = (b == TestPassMarker[0]) ? 1 : 0;
                            }
                        }
                    }

                    if (sawTestPass && (DateTime.UtcNow - lastMagicAt).TotalSeconds >= QemuHostDefaults.StallSecondsAfterTestPass)
                    {
                        return UartMonitorOutcome.Stalled;
                    }
                }
            }
            catch (IOException)
            {
                // File might be locked, try again
            }

            await Task.Delay(QemuHostDefaults.UartPollIntervalMs, cancellationToken);
        }

        return UartMonitorOutcome.NotFinished;
    }
}
