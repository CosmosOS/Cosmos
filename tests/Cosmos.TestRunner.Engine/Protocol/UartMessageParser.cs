using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.TestRunner.Protocol;

namespace Cosmos.TestRunner.Engine.Protocol;

/// <summary>
/// Parses binary protocol messages from UART log output
/// </summary>
public class UartMessageParser
{
    /// <summary>Offset of the second magic byte within a frame header.</summary>
    private const int MagicByte1Offset = 1;
    /// <summary>Offset of the third magic byte within a frame header.</summary>
    private const int MagicByte2Offset = 2;
    /// <summary>Offset of the fourth magic byte within a frame header.</summary>
    private const int MagicByte3Offset = 3;
    /// <summary>Offset of the Command byte within a frame header ([MAGIC:4][Command:1][Length:2]).</summary>
    private const int CommandOffset = 4;
    /// <summary>Offset of the low byte of the payload Length field within a frame header.</summary>
    private const int LengthLowOffset = 5;
    /// <summary>Offset of the high byte of the payload Length field within a frame header.</summary>
    private const int LengthHighOffset = 6;
    /// <summary>Bit shift applied to the high Length byte when assembling the little-endian 16-bit payload length.</summary>
    private const int LengthHighShift = 8;
    /// <summary>Total frame header size in bytes: [MAGIC:4][Command:1][Length:2].</summary>
    private const int HeaderLengthBytes = 7;

    /// <summary>Maximum accepted payload length for a CoverageData message (full 16-bit range).</summary>
    private const int MaxCoveragePayloadBytes = 65535;
    /// <summary>Maximum accepted payload length for any non-coverage message (sanity limit).</summary>
    private const int MaxStandardPayloadBytes = 1024;

    /// <summary>Size in bytes of a little-endian 16-bit protocol field (TestNumber, ExpectedTests, HitCount).</summary>
    private const int UInt16FieldBytes = 2;
    /// <summary>Minimum TestPass payload size: [TestNumber:2][DurationMs:4].</summary>
    private const int TestPassPayloadBytes = 6;
    /// <summary>Minimum TestSuiteEnd payload size: [Total:2][Passed:2][Failed:2][Skipped:2].</summary>
    private const int SuiteEndPayloadBytes = 8;
    /// <summary>Offset of the Passed counter within a TestSuiteEnd payload.</summary>
    private const int SuiteEndPassedOffset = 2;
    /// <summary>Offset of the Failed counter within a TestSuiteEnd payload.</summary>
    private const int SuiteEndFailedOffset = 4;
    /// <summary>Offset of the Skipped counter within a TestSuiteEnd payload.</summary>
    private const int SuiteEndSkippedOffset = 6;

    /// <summary>First printable ASCII character; anything below is a control character (0x00..0x1F).</summary>
    private const int MinPrintableChar = 0x20;

    /// <summary>
    /// Parse UART log and extract test results
    /// </summary>
    public static TestResults ParseUartLog(string uartLog, string architecture)
    {
        var results = new TestResults { Architecture = architecture };

        // Extract binary data from UART log (filter out ANSI codes and text)
        var binaryData = ExtractBinaryData(uartLog);

        Console.WriteLine($"[UartParser] UART log length: {uartLog.Length} bytes");
        Console.WriteLine($"[UartParser] Binary data length: {binaryData.Length} bytes");

        // Parse protocol messages
        int offset = 0;
        int messagesFound = 0;
        while (offset < binaryData.Length)
        {
            int oldOffset = offset;
            if (!TryParseMessage(binaryData, ref offset, results))
            {
                // Skip byte if we can't parse a valid message
                offset++;
            }
            else if (offset > oldOffset)
            {
                messagesFound++;
            }
        }

        Console.WriteLine($"[UartParser] Found {messagesFound} protocol messages");
        Console.WriteLine($"[UartParser] Suite name: {results.SuiteName}");
        Console.WriteLine($"[UartParser] Tests found: {results.Tests.Count}");

        // A TestStart with no Pass/Fail/Skip behind it is a test the kernel
        // died in (page fault, hang, triple fault). Its placeholder status is
        // Passed only so a later result frame has an entry to update; once
        // the log has ended without a validated TestSuiteEnd, no such frame
        // is coming. A completed suite is left alone: there a missing frame
        // is UART corruption of a test that did finish, and the run is
        // judged by the counters TestSuiteEnd carries.
        if (!results.SuiteCompleted)
        {
            foreach (TestResult test in results.Tests)
            {
                if (!test.HasResult)
                {
                    test.Status = TestStatus.Failed;
                    test.ErrorMessage = "Test started but produced no result (kernel crashed or hung)";
                }
            }
        }

        return results;
    }

    private static byte[] ExtractBinaryData(string uartLog)
    {
        // Convert entire UART log to bytes
        // Protocol messages are embedded in the byte stream alongside text output
        return Encoding.Latin1.GetBytes(uartLog);
    }

    private static bool TryParseMessage(byte[] data, ref int offset, TestResults results)
    {
        // Need at least 7 bytes: [MAGIC:4][Command:1][Length:2]
        if (offset + HeaderLengthBytes > data.Length)
        {
            return false;
        }

        // Check for magic signature (0x19740807 little-endian)
        if (data[offset] != Consts.SerialSignatureByte0 || data[offset + MagicByte1Offset] != Consts.SerialSignatureByte1 ||
            data[offset + MagicByte2Offset] != Consts.SerialSignatureByte2 || data[offset + MagicByte3Offset] != Consts.SerialSignatureByte3)
        {
            return false;
        }

        byte command = data[offset + CommandOffset];

        // Only proceed if this looks like a valid protocol command
        if (command < Ds2Vs.TestSuiteStart || command > Ds2Vs.TestDestructiveReached)
        {
            return false;
        }

        ushort length = (ushort)(data[offset + LengthLowOffset] | (data[offset + LengthHighOffset] << LengthHighShift));

        // Sanity check: coverage data can be large, other messages should be small
        int maxLength = (command == Ds2Vs.CoverageData) ? MaxCoveragePayloadBytes : MaxStandardPayloadBytes;
        if (length > maxLength)
        {
            return false;
        }

        // Validate we have enough data for payload
        if (offset + HeaderLengthBytes + length > data.Length)
        {
            return false;
        }

        byte[] payload = new byte[length];
        Array.Copy(data, offset + HeaderLengthBytes, payload, 0, length);

        // Only advance offset after we've validated this is a real message
        offset += HeaderLengthBytes + length;

        // Parse based on command
        switch (command)
        {
            case Ds2Vs.TestSuiteStart:
                ParseTestSuiteStart(payload, results);
                return true;

            case Ds2Vs.TestStart:
                ParseTestStart(payload, results);
                return true;

            case Ds2Vs.TestPass:
                ParseTestPass(payload, results);
                return true;

            case Ds2Vs.TestFail:
                ParseTestFail(payload, results);
                return true;

            case Ds2Vs.TestSkip:
                ParseTestSkip(payload, results);
                return true;

            case Ds2Vs.TestSuiteEnd:
                ParseTestSuiteEnd(payload, results);
                return true;

            case Ds2Vs.CoverageData:
                ParseCoverageData(payload, results);
                return true;

            case Ds2Vs.ArchitectureInfo:
                // Architecture bootstrap message (arch + cpu count).
                // Not used for test assertions, but must be consumed as a valid frame.
                return true;

            case Ds2Vs.TestDestructiveReached:
                // Sentinel for the engine's re-launch heuristic. The frame must be
                // consumed as a valid message; no parsing into TestResults needed.
                return true;

            case Ds2Vs.HostRequest:
                // Acted on live by the QEMU host while the guest ran.
                return true;

            default:
                return false;
        }
    }

    private static void ParseTestSuiteStart(byte[] payload, TestResults results)
    {
        if (payload.Length < UInt16FieldBytes)
        {
            string shortName = Encoding.UTF8.GetString(payload);
            if (HasControlChars(shortName))
            {
                return;
            }

            results.SuiteName = shortName;
            results.ExpectedTestCount = 0;
            return;
        }

        // Payload: [ExpectedTests:2][SuiteName:string]
        string suiteName = Encoding.UTF8.GetString(payload, UInt16FieldBytes, payload.Length - UInt16FieldBytes);
        if (HasControlChars(suiteName))
        {
            return;
        }

        results.ExpectedTestCount = BitConverter.ToUInt16(payload, 0);
        results.SuiteName = suiteName;
    }

    private static void ParseTestStart(byte[] payload, TestResults results)
    {
        // Payload: [TestNumber:2][TestName:string]
        if (payload.Length < UInt16FieldBytes)
        {
            return;
        }

        int testNumber = BitConverter.ToUInt16(payload, 0);
        string testName = Encoding.UTF8.GetString(payload, UInt16FieldBytes, payload.Length - UInt16FieldBytes);

        // Defensive: a non-protocol byte sequence in UART (e.g. an IRQ handler
        // calling Serial.WriteString mid-frame) can interleave with a real
        // TestStart in a way that produces a false-positive magic match here,
        // and the resulting "name" carries control characters (0x00..0x1F)
        // that downstream XML emission rejects. Drop messages whose name
        // contains anything below 0x20 — real test names are ASCII
        // identifiers. Same defense on every string-bearing frame (suite
        // start, fail, skip).
        if (HasControlChars(testName))
        {
            return;
        }

        TestResult? existingTest = results.Tests.Find(t => t.TestNumber == testNumber);
        if (existingTest != null)
        {
            existingTest.TestName = testName;
            return;
        }

        // Add test with pending status
        results.Tests.Add(new TestResult
        {
            TestNumber = testNumber,
            TestName = testName,
            Status = TestStatus.Passed // Will be updated by Pass/Fail/Skip
        });
    }

    private static void ParseTestPass(byte[] payload, TestResults results)
    {
        // Payload: [TestNumber:2][DurationMs:4]
        if (payload.Length < TestPassPayloadBytes)
        {
            return;
        }

        int testNumber = BitConverter.ToUInt16(payload, 0);
        uint durationMs = BitConverter.ToUInt32(payload, UInt16FieldBytes);

        TestResult? test = FindTestResult(results, testNumber);
        if (test == null)
        {
            return;
        }

        // Mirror the kernel-side clamp (TestRunner.Run): if a non-protocol
        // byte sequence in UART interleaves with a real TestPass frame, the
        // 4 raw duration bytes can be anything — that's where the
        // "1,129,536-second test" rows came from. Anything beyond 5 minutes
        // is a misaligned-frame artifact, not a measurement; pin it to the
        // cap so the report stays interpretable. Real tests finish in
        // seconds; this gives ~300x headroom.
        const uint MaxSaneDurationMs = 5u * 60u * 1000u;
        if (durationMs > MaxSaneDurationMs)
        {
            durationMs = MaxSaneDurationMs;
        }

        test.Status = TestStatus.Passed;
        test.DurationMs = durationMs;
        test.HasResult = true;
    }

    private static void ParseTestFail(byte[] payload, TestResults results)
    {
        // Payload: [TestNumber:2][ErrorMessage:string]
        if (payload.Length < UInt16FieldBytes)
        {
            return;
        }

        int testNumber = BitConverter.ToUInt16(payload, 0);
        string errorMessage = Encoding.UTF8.GetString(payload, UInt16FieldBytes, payload.Length - UInt16FieldBytes);
        if (HasControlChars(errorMessage))
        {
            return;
        }

        // The kernel always emits TestStart before Pass/Fail/Skip, so a
        // number with no prior TestStart is a misaligned-frame artifact —
        // fabricating a result for it turns corruption into a phantom
        // "Test 51234" failure. A genuinely lost test still fails the run
        // via the expected-count gap ("Test did not execute").
        TestResult? test = FindTestResult(results, testNumber);
        if (test == null)
        {
            return;
        }

        test.Status = TestStatus.Failed;
        test.ErrorMessage = errorMessage;
        test.HasResult = true;
    }

    private static void ParseTestSkip(byte[] payload, TestResults results)
    {
        // Payload: [TestNumber:2][Reason:string]
        if (payload.Length < UInt16FieldBytes)
        {
            return;
        }

        int testNumber = BitConverter.ToUInt16(payload, 0);
        string reason = Encoding.UTF8.GetString(payload, UInt16FieldBytes, payload.Length - UInt16FieldBytes);
        if (HasControlChars(reason))
        {
            return;
        }

        TestResult? test = FindTestResult(results, testNumber);
        if (test == null)
        {
            return;
        }

        test.Status = TestStatus.Skipped;
        test.ErrorMessage = reason;
        test.HasResult = true;
    }

    private static void ParseTestSuiteEnd(byte[] payload, TestResults results)
    {
        // Payload: [Total:2][Passed:2][Failed:2][Skipped:2]
        if (payload.Length < SuiteEndPayloadBytes)
        {
            return;
        }

        ushort total = BitConverter.ToUInt16(payload, 0);
        ushort passed = BitConverter.ToUInt16(payload, SuiteEndPassedOffset);
        ushort failed = BitConverter.ToUInt16(payload, SuiteEndFailedOffset);
        ushort skipped = BitConverter.ToUInt16(payload, SuiteEndSkippedOffset);

        // Validate: total must equal passed + failed + skipped (catches corruption
        // from timer interrupt interleaving). Skips count: TR.Finish reports the
        // expected total, and a suite with skip cells is still a completed suite.
        if (total == (ushort)(passed + failed + skipped))
        {
            // Use the validated total from the end message as the authoritative expected count.
            // This overrides the potentially corrupted value from TestSuiteStart.
            results.ExpectedTestCount = total;
            results.SuiteCompleted = true;
        }
    }

    private static void ParseCoverageData(byte[] payload, TestResults results)
    {
        // Payload: [HitCount:2][HitId1:2][HitId2:2]...
        if (payload.Length < UInt16FieldBytes)
        {
            return;
        }

        ushort hitCount = BitConverter.ToUInt16(payload, 0);

        Console.WriteLine($"[UartParser] Coverage data: {hitCount} methods hit");

        for (int i = 0; i < hitCount && (UInt16FieldBytes + i * UInt16FieldBytes + 1) < payload.Length; i++)
        {
            ushort methodId = BitConverter.ToUInt16(payload, UInt16FieldBytes + i * UInt16FieldBytes);
            results.CoverageHitMethodIds.Add(methodId);
        }
    }

    private static TestResult? FindTestResult(TestResults results, int testNumber)
        => results.Tests.Find(t => t.TestNumber == testNumber);

    // Real protocol strings are ASCII identifiers / prose; anything below
    // 0x20 means the frame was assembled from interleaved UART bytes.
    private static bool HasControlChars(string value)
    {
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i] < MinPrintableChar)
            {
                return true;
            }
        }

        return false;
    }
}
