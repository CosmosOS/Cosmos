using System.Collections.Generic;
using System.Text;
using Cosmos.TestRunner.Engine;
using Cosmos.TestRunner.Engine.Protocol;
using Cosmos.TestRunner.Protocol;

namespace Cosmos.Tests.Patcher;

[Collection("PatcherTests")]
public class UartMessageParserTests
{
    /// <summary>First byte of the Ds2Vs UART frame magic signature (0x07 0x08 0x74 0x19).</summary>
    private const byte FrameMagicByte0 = 0x07;

    /// <summary>Second byte of the Ds2Vs UART frame magic signature.</summary>
    private const byte FrameMagicByte1 = 0x08;

    /// <summary>Third byte of the Ds2Vs UART frame magic signature.</summary>
    private const byte FrameMagicByte2 = 0x74;

    /// <summary>Fourth byte of the Ds2Vs UART frame magic signature.</summary>
    private const byte FrameMagicByte3 = 0x19;

    /// <summary>Mask isolating a single byte of the little-endian 16-bit payload length field.</summary>
    private const int PayloadLengthByteMask = 0xFF;

    /// <summary>Shift extracting the high byte of the little-endian 16-bit payload length field.</summary>
    private const int PayloadLengthHighByteShift = 8;

    [Fact]
    public void ParseUartLog_DoesNotLoseFramesAfterArchitectureInfoMessage()
    {
        List<byte> stream = new();
        stream.AddRange(CreateFrame(Ds2Vs.TestSuiteStart, [1, 0, (byte)'S', (byte)'u', (byte)'i', (byte)'t', (byte)'e']));
        stream.AddRange(CreateFrame(Ds2Vs.ArchitectureInfo, [2, 1]));
        stream.AddRange(CreateFrame(Ds2Vs.TestStart, [1, 0, (byte)'A']));
        stream.AddRange(CreateFrame(Ds2Vs.TestPass, [1, 0, 5, 0, 0, 0]));
        stream.AddRange(CreateFrame(Ds2Vs.TestSuiteEnd, [1, 0, 1, 0, 0, 0, 0, 0]));

        string uartLog = Encoding.Latin1.GetString(stream.ToArray());

        TestResults results = UartMessageParser.ParseUartLog(uartLog, "x64");

        Assert.True(results.SuiteCompleted);
        Assert.Equal(1, results.TotalTests);
        Assert.Single(results.Tests);
        Assert.Equal(1, results.PassedTests);
        Assert.Equal(0, results.FailedTests);
    }

    [Fact]
    public void ParseUartLog_IgnoresPassWithoutStart()
    {
        // The kernel always emits TestStart before Pass/Fail/Skip, so a
        // result frame with no prior TestStart is a misaligned-frame
        // artifact. It must not fabricate a "Test N" entry; the lost test
        // still reddens the run through the expected-count gap.
        List<byte> stream = new();
        stream.AddRange(CreateFrame(Ds2Vs.TestSuiteStart, [1, 0, (byte)'S', (byte)'u', (byte)'i', (byte)'t', (byte)'e']));
        stream.AddRange(CreateFrame(Ds2Vs.TestPass, [1, 0, 5, 0, 0, 0]));
        stream.AddRange(CreateFrame(Ds2Vs.TestSuiteEnd, [1, 0, 1, 0, 0, 0, 0, 0]));

        string uartLog = Encoding.Latin1.GetString(stream.ToArray());
        TestResults results = UartMessageParser.ParseUartLog(uartLog, "x64");

        Assert.True(results.SuiteCompleted);
        Assert.Empty(results.Tests);
        Assert.Equal(1, results.TotalTests);
        Assert.Equal(0, results.PassedTests);
    }

    [Fact]
    public void ParseUartLog_DropsOrphanPassButKeepsLaterStart()
    {
        List<byte> stream = new();
        stream.AddRange(CreateFrame(Ds2Vs.TestSuiteStart, [1, 0, (byte)'S', (byte)'u', (byte)'i', (byte)'t', (byte)'e']));
        stream.AddRange(CreateFrame(Ds2Vs.TestPass, [1, 0, 5, 0, 0, 0]));
        stream.AddRange(CreateFrame(Ds2Vs.TestStart, [1, 0, (byte)'A']));
        stream.AddRange(CreateFrame(Ds2Vs.TestSuiteEnd, [1, 0, 1, 0, 0, 0, 0, 0]));

        string uartLog = Encoding.Latin1.GetString(stream.ToArray());
        TestResults results = UartMessageParser.ParseUartLog(uartLog, "x64");

        Assert.Single(results.Tests);
        Assert.Equal("A", results.Tests[0].TestName);
    }

    [Fact]
    public void ParseUartLog_IgnoresFailWithoutStartAndCorruptedStrings()
    {
        // Regression for the phantom "Test 51234" failure: a misaligned
        // TestFail frame (bogus number, NUL-bearing message) must neither
        // create a result nor fail the real test.
        List<byte> stream = new();
        stream.AddRange(CreateFrame(Ds2Vs.TestSuiteStart, [1, 0, (byte)'S', (byte)'u', (byte)'i', (byte)'t', (byte)'e']));
        stream.AddRange(CreateFrame(Ds2Vs.TestStart, [1, 0, (byte)'A']));
        stream.AddRange(CreateFrame(Ds2Vs.TestPass, [1, 0, 5, 0, 0, 0]));
        stream.AddRange(CreateFrame(Ds2Vs.TestFail, [0xC2, 0xC8, (byte)'x', 0x00, (byte)'y']));
        stream.AddRange(CreateFrame(Ds2Vs.TestSuiteEnd, [1, 0, 1, 0, 0, 0, 0, 0]));

        string uartLog = Encoding.Latin1.GetString(stream.ToArray());
        TestResults results = UartMessageParser.ParseUartLog(uartLog, "x64");

        Assert.True(results.SuiteCompleted);
        Assert.Single(results.Tests);
        Assert.Equal(1, results.PassedTests);
        Assert.Equal(0, results.FailedTests);
    }

    [Fact]
    public void ParseUartLog_FailsStartedTestWithoutResultWhenSuiteIncomplete()
    {
        // A kernel that page-faults inside test 2 emits TestStart for it and
        // then nothing: no result frame, no TestSuiteEnd. The placeholder
        // status a TestStart creates must not survive as a pass.
        List<byte> stream = new();
        stream.AddRange(CreateFrame(Ds2Vs.TestSuiteStart, [2, 0, (byte)'S', (byte)'u', (byte)'i', (byte)'t', (byte)'e']));
        stream.AddRange(CreateFrame(Ds2Vs.TestStart, [1, 0, (byte)'A']));
        stream.AddRange(CreateFrame(Ds2Vs.TestPass, [1, 0, 5, 0, 0, 0]));
        stream.AddRange(CreateFrame(Ds2Vs.TestStart, [2, 0, (byte)'B']));

        string uartLog = Encoding.Latin1.GetString(stream.ToArray());
        TestResults results = UartMessageParser.ParseUartLog(uartLog, "x64");

        Assert.False(results.SuiteCompleted);
        Assert.Equal(2, results.Tests.Count);
        Assert.Equal(1, results.PassedTests);
        Assert.Equal(1, results.FailedTests);
        Assert.Equal(TestStatus.Failed, results.Tests[1].Status);
        Assert.False(results.Tests[1].HasResult);
        Assert.False(results.AllTestsPassed);
    }

    [Fact]
    public void ParseUartLog_KeepsStartedTestWithoutResultWhenSuiteCompleted()
    {
        // With a validated TestSuiteEnd the suite did finish, so a missing
        // result frame is UART corruption of a test that ran, not a crash;
        // the placeholder pass stands and the run is judged by the counters.
        List<byte> stream = new();
        stream.AddRange(CreateFrame(Ds2Vs.TestSuiteStart, [1, 0, (byte)'S', (byte)'u', (byte)'i', (byte)'t', (byte)'e']));
        stream.AddRange(CreateFrame(Ds2Vs.TestStart, [1, 0, (byte)'A']));
        stream.AddRange(CreateFrame(Ds2Vs.TestSuiteEnd, [1, 0, 1, 0, 0, 0, 0, 0]));

        string uartLog = Encoding.Latin1.GetString(stream.ToArray());
        TestResults results = UartMessageParser.ParseUartLog(uartLog, "x64");

        Assert.True(results.SuiteCompleted);
        Assert.Equal(0, results.FailedTests);
        Assert.False(results.Tests[0].HasResult);
    }

    private static byte[] CreateFrame(byte command, byte[] payload)
    {
        List<byte> bytes = new();
        bytes.Add(FrameMagicByte0);
        bytes.Add(FrameMagicByte1);
        bytes.Add(FrameMagicByte2);
        bytes.Add(FrameMagicByte3);
        bytes.Add(command);

        ushort payloadLength = (ushort)payload.Length;
        bytes.Add((byte)(payloadLength & PayloadLengthByteMask));
        bytes.Add((byte)((payloadLength >> PayloadLengthHighByteShift) & PayloadLengthByteMask));

        bytes.AddRange(payload);
        return bytes.ToArray();
    }
}
