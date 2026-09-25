using System;
using System.Diagnostics;
using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.System.Diagnostics;

namespace Cosmos.TestRunner.Framework
{
    /// <summary>
    /// Test runner for kernel-side test execution.
    /// Sends test results via UART using the binary protocol.
    /// </summary>
    public static class TestRunner
    {
        /// <summary>Milliseconds per second; converts a Stopwatch tick delta divided by frequency into ms. Public so test kernels can share the same conversion factor.</summary>
        public const int MillisecondsPerSecond = 1000;
        /// <summary>Upper bound (5 minutes, in ms) for a plausible single-test duration; larger raw readings are clamped (QEMU TCG CNTPCT_EL0 anomaly).</summary>
        private const long MaxSaneDurationMs = 5L * 60L * 1000L;
        /// <summary>Length of the "skip=" token skipped over when parsing the Limine cmdline.</summary>
        private const int SkipTokenLength = 5;
        /// <summary>Length of the "profile=" token skipped over when parsing the Limine cmdline.</summary>
        private const int ProfileTokenLength = 8;
        /// <summary>Base of the decimal number system, used when accumulating parsed digits.</summary>
        private const int DecimalBase = 10;

        /// <summary>End marker telling the QEMU host to kill the VM.</summary>
        private static ReadOnlySpan<byte> QemuKillMarker => new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xCA, 0xFE, 0xBA, 0xBE };

        private static string? s_currentSuite;
        private static ushort s_testCount;
        private static ushort s_expectedTestCount;
        private static ushort s_passedCount;
        private static ushort s_failedCount;
        private static ushort s_skippedCount;
        private static ushort s_currentTestNumber;
        private static long s_testStartTicks;

        /// <summary>
        /// Start a test suite
        /// </summary>
        /// <param name="suiteName">Name of the test suite</param>
        /// <param name="expectedTests">Total number of tests that will be registered (0 = unknown)</param>
        public static void Start(string suiteName, ushort expectedTests = 0)
        {
            s_currentSuite = suiteName;
            s_testCount = 0;
            s_expectedTestCount = expectedTests;
            s_passedCount = 0;
            s_failedCount = 0;
            s_skippedCount = 0;
            s_currentTestNumber = 0;

            // Send TestSuiteStart message with expected test count
            SendTestSuiteStart(suiteName, expectedTests);
        }

        /// <summary>
        /// Run a test with automatic failure detection
        /// </summary>
        public static void Run(string testName, Action testAction)
        {
            s_currentTestNumber++;
            s_testCount++;

            // Send TestStart message
            SendTestStart(s_currentTestNumber, testName);

            // Reset assertion state
            Assert.Reset();

            // Record start time
            s_testStartTicks = Stopwatch.GetTimestamp();

            // Execute test
            testAction();

            // Calculate duration. The raw CNTPCT_EL0 delta has produced
            // multi-million-second readings on github-CI arm64 (QEMU TCG)
            // for a sub-second test, propagating into the JUnit XML as
            // bogus times. When that happens, clamp to a sane max and
            // emit a UART warning with the raw inputs so the cause can be
            // debugged from the log.
            long endTicks = Stopwatch.GetTimestamp();
            long elapsedTicks = endTicks - s_testStartTicks;
            long freq = Stopwatch.Frequency;
            long rawMs = (freq > 0 && elapsedTicks > 0)
                ? (elapsedTicks * MillisecondsPerSecond) / freq
                : 0;

            uint durationMs;
            if (rawMs < 0 || rawMs > MaxSaneDurationMs)
            {
                Log.WriteString("[TestRunner] WARN clamped durationMs=");
                Log.WriteNumber(rawMs);
                Log.WriteString(" startTicks=");
                Log.WriteNumber(s_testStartTicks);
                Log.WriteString(" endTicks=");
                Log.WriteNumber(endTicks);
                Log.WriteString(" elapsedTicks=");
                Log.WriteNumber(elapsedTicks);
                Log.WriteString(" freq=");
                Log.WriteNumber(freq);
                Log.WriteString("\n");
                durationMs = (uint)MaxSaneDurationMs;
            }
            else
            {
                durationMs = (uint)rawMs;
            }

            // Check if test failed via Assert
            if (Assert.Failed)
            {
                s_failedCount++;
                SendTestFail(s_currentTestNumber, Assert.FailureMessage ?? "Test failed");
            }
            else
            {
                s_passedCount++;
                SendTestPass(s_currentTestNumber, durationMs);
            }
        }

        /// <summary>
        /// Run <paramref name="testAction"/> only when <paramref name="condition"/> is
        /// true; otherwise emit a <see cref="Skip(string, string)"/> with
        /// <paramref name="skipReason"/>. Use to gate a test on a feature that may or
        /// may not be present in the current QEMU profile (specific device kind, MSI-X
        /// capability, GIC version) — keeps the test in the report as Skipped instead
        /// of either silently disappearing or failing for a reason that's not a code
        /// regression.
        /// </summary>
        public static void RunIf(bool condition, string testName, Action testAction, string skipReason)
        {
            if (condition)
            {
                Run(testName, testAction);
            }
            else
            {
                Skip(testName, skipReason);
            }
        }

        /// <summary>
        /// Run a test that adapts to a capability instead of skipping. The
        /// <paramref name="condition"/> is passed into <paramref name="test"/>
        /// so the body can assert the capable path when true and the fallback
        /// path when false — both branches stay in the report as a real run.
        /// Use this when the cell always has something to assert but the
        /// expected outcome differs by profile (e.g. NVMe MSI-X vs polled),
        /// as opposed to <see cref="RunIf(bool, string, Action, string)"/>
        /// which reports a Skip when the feature is simply absent. Named
        /// distinctly from RunIf on purpose: an overload on delegate arity
        /// would silently flip between gate-execution and pass-the-flag
        /// semantics when a test method's signature changes.
        /// </summary>
        public static void RunWithExpectation(bool expectation, string testName, Action<bool> test)
        {
            Run(testName, () => test(expectation));
        }

        /// <summary>
        /// Run a destructive test whose action is expected to never return
        /// (e.g. a successful Power.Reboot / Power.Shutdown). The test is
        /// pre-emptively reported as passed before invoking the action; if
        /// the action returns the pre-emptive pass is overridden by a fail
        /// message and the call returns normally so the suite can finalise.
        /// </summary>
        public static void RunDestructive(string testName, Action testAction, string failureMessage)
        {
            s_currentTestNumber++;
            s_testCount++;

            // Pre-send TestStart + TestPass so a successful destructive op
            // (which never returns) still leaves a passing record in the log.
            SendTestStart(s_currentTestNumber, testName);
            SendTestPass(s_currentTestNumber, 0);
            s_passedCount++;

            // Distinct sentinel for the engine's re-launch heuristic. A regular
            // TestPass alone is ambiguous (every passing test emits one), so
            // without this the engine would misread a mid-suite crash as a
            // destructive op and burn boot attempts on skip=N+1 re-launches.
            SendTestDestructiveReached(s_currentTestNumber);

            testAction();

            // Action returned — destructive op didn't fire. Demote to fail
            // (last write wins in the parser).
            s_passedCount--;
            s_failedCount++;
            SendTestFail(s_currentTestNumber, failureMessage);
        }

        /// <summary>
        /// Asks the test engine to change the machine under the running
        /// guest: <c>usb-unplug</c> pulls the profile's USB stick out and
        /// <c>usb-plug</c> puts it back (both take an optional stick index,
        /// 0 by default). Returns at once, since nothing replies: the test
        /// waits for the change to show up, and must see it within the
        /// engine's stall window (10 s without a protocol message).
        /// </summary>
        public static void RequestHost(string request) => SendMessage(HostRequest, EncodeString(request));

        /// <summary>
        /// Reads the <c>skip=N</c> integer from the Limine kernel cmdline.
        /// The test runner sets this on each re-launch when a previous boot
        /// fired a test that exited QEMU (Reboot, Shutdown). Returns 0 if
        /// the cmdline is missing or has no <c>skip=</c> token (default
        /// first-boot behaviour).
        /// </summary>
        public static unsafe int GetSkipCount()
        {
            byte* cmdline = Limine.Cmdline;
            if (cmdline == null)
            {
                return 0;
            }

            // Walk the null-terminated cmdline looking for "skip=" then digits.
            byte* p = cmdline;
            while (*p != 0)
            {
                if (p[0] == (byte)'s' && p[1] == (byte)'k' && p[2] == (byte)'i' &&
                    p[3] == (byte)'p' && p[4] == (byte)'=')
                {
                    p += SkipTokenLength;
                    int value = 0;
                    while (*p >= (byte)'0' && *p <= (byte)'9')
                    {
                        value = value * DecimalBase + (*p - (byte)'0');
                        p++;
                    }
                    return value;
                }
                p++;
            }
            return 0;
        }

        /// <summary>
        /// Reads the <c>profile=&lt;name&gt;</c> token from the Limine kernel
        /// cmdline. The test engine sets this to the active QEMU profile-matrix
        /// cell name (e.g. <c>nvme+gicv3</c> or <c>nvme+gicv2+acpi-off</c>) so a
        /// suite can assert the hardware path that cell was meant to exercise.
        /// Returns an empty string when no profile token is present (a suite
        /// that opts into no profiles, or a non-test boot).
        /// </summary>
        public static unsafe string GetProfileName()
        {
            byte* cmdline = Limine.Cmdline;
            if (cmdline == null)
            {
                return string.Empty;
            }

            // Walk the null-terminated cmdline looking for "profile=" then read
            // the value up to the next space or the terminating null. The value
            // may contain '+' (composed cell names), which is preserved.
            byte* p = cmdline;
            while (*p != 0)
            {
                if (p[0] == (byte)'p' && p[1] == (byte)'r' && p[2] == (byte)'o' &&
                    p[3] == (byte)'f' && p[4] == (byte)'i' && p[5] == (byte)'l' &&
                    p[6] == (byte)'e' && p[7] == (byte)'=')
                {
                    p += ProfileTokenLength;
                    int len = 0;
                    while (p[len] != 0 && p[len] != (byte)' ')
                    {
                        len++;
                    }
                    char[] chars = new char[len];
                    for (int i = 0; i < len; i++)
                    {
                        chars[i] = (char)p[i];
                    }
                    return new string(chars);
                }
                p++;
            }
            return string.Empty;
        }

        // Cached profile cell name backing the prefix/contains helpers; reading
        // the Limine cmdline is cheap but they are called repeatedly. Each
        // matrix cell is a fresh boot, so the static starts empty per cell.
        private static string? s_profileName;

        /// <summary>
        /// The active profile-matrix cell name (cached <see cref="GetProfileName"/>),
        /// e.g. <c>nvme+gicv3</c>. Empty for a suite that opts into no profiles.
        /// </summary>
        public static string ProfileName
        {
            get
            {
                if (s_profileName == null)
                {
                    s_profileName = GetProfileName();
                }
                return s_profileName;
            }
        }

        /// <summary>
        /// True when the active profile cell name starts with <paramref name="prefix"/>.
        /// A cell name always leads with its base profile (e.g. the "nvme" in
        /// "nvme+gicv2"), so a prefix check identifies the profile. Lives here
        /// because the kernel runtime does not plug <c>string.StartsWith</c>.
        /// </summary>
        public static bool ProfileHasPrefix(string prefix)
        {
            string profile = ProfileName;
            if (profile.Length < prefix.Length)
            {
                return false;
            }
            for (int i = 0; i < prefix.Length; i++)
            {
                if (profile[i] != prefix[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// True when <paramref name="needle"/> occurs anywhere in the active
        /// profile cell name. Detects a composed modifier such as the "gicv2" in
        /// "nvme+gicv2+acpi-off". Lives here because the kernel runtime does not
        /// plug <c>string.Contains</c>.
        /// </summary>
        public static bool ProfileContains(string needle)
        {
            string profile = ProfileName;
            int limit = profile.Length - needle.Length;
            for (int i = 0; i <= limit; i++)
            {
                int j = 0;
                while (j < needle.Length && profile[i + j] == needle[j])
                {
                    j++;
                }
                if (j == needle.Length)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Skip a test
        /// </summary>
        public static void Skip(string testName, string reason)
        {
            s_currentTestNumber++;
            s_testCount++;

            s_skippedCount++;
            SendTestStart(s_currentTestNumber, testName);
            SendTestSkip(s_currentTestNumber, reason);
        }

        /// <summary>
        /// Finish the test suite and send summary.
        /// Does NOT flush coverage or send the QEMU kill marker.
        /// Call Complete() after AfterRun() for that.
        /// </summary>
        public static void Finish()
        {
            // Use expected count if provided, otherwise actual count
            ushort totalToReport = s_expectedTestCount > 0 ? s_expectedTestCount : s_testCount;

            SendTestSuiteEnd(totalToReport, s_passedCount, s_failedCount, s_skippedCount);

            // Also send a text message for fallback/debugging
            Log.WriteString("\nTest Suite: ");
            Log.WriteString(s_currentSuite ?? "Unknown");
            Log.WriteString("\nTotal: ");
            Log.WriteNumber(s_testCount);
            if (s_expectedTestCount > 0 && s_expectedTestCount != s_testCount)
            {
                Log.WriteString(" / ");
                Log.WriteNumber(s_expectedTestCount);
                Log.WriteString(" expected");
            }
            Log.WriteString("  Passed: ");
            Log.WriteNumber(s_passedCount);
            Log.WriteString("  Failed: ");
            Log.WriteNumber(s_failedCount);
            Log.WriteString("\n");
        }

        /// <summary>
        /// Final step: flush coverage data and send the QEMU termination marker.
        /// Call this in AfterRun() so that Run() and AfterRun() are covered.
        /// After this call, the test engine will kill QEMU.
        /// </summary>
        public static void Complete()
        {
            // Flush coverage data (no-op if not instrumented)
            CoverageTracker.Flush();

            // Send unique end marker: 0xDE 0xAD 0xBE 0xEF 0xCA 0xFE 0xBA 0xBE
            // This sequence tells the QEMU host to kill the VM
            Log.WriteBytes(QemuKillMarker);
        }

        #region Protocol Message Sending

        // Protocol constants (must match Cosmos.TestRunner.Protocol/Consts.cs)
        private const byte TestSuiteStart = 100;
        private const byte TestStart = 101;
        private const byte TestPass = 102;
        private const byte TestFail = 103;
        private const byte TestSkip = 104;
        private const byte TestSuiteEnd = 105;
        private const byte TestDestructiveReached = 108;
        private const byte HostRequest = 109;

        /// <summary>Byte 0 (least significant) of the protocol magic signature 0x19740807 (SerialSignature from Consts.cs), sent little-endian.</summary>
        private const byte SerialSignatureByte0 = 0x07;
        /// <summary>Byte 1 of the protocol magic signature 0x19740807.</summary>
        private const byte SerialSignatureByte1 = 0x08;
        /// <summary>Byte 2 of the protocol magic signature 0x19740807.</summary>
        private const byte SerialSignatureByte2 = 0x74;
        /// <summary>Byte 3 (most significant) of the protocol magic signature 0x19740807.</summary>
        private const byte SerialSignatureByte3 = 0x19;

        /// <summary>Mask isolating the low 8 bits when serializing multi-byte values little-endian.</summary>
        private const int ByteMask = 0xFF;
        /// <summary>Shift extracting byte 1 of a little-endian multi-byte value.</summary>
        private const int Byte1Shift = 8;
        /// <summary>Shift extracting byte 2 of a little-endian multi-byte value.</summary>
        private const int Byte2Shift = 16;
        /// <summary>Shift extracting byte 3 of a little-endian multi-byte value.</summary>
        private const int Byte3Shift = 24;

        /// <summary>Size in bytes of the frame header: 4-byte magic, command byte, little-endian ushort length.</summary>
        private const int HeaderSizeBytes = 7;
        /// <summary>Size in bytes of a little-endian ushort payload field (test number, expected count).</summary>
        private const int UInt16FieldSizeBytes = 2;
        /// <summary>Payload size of a TestPass message: ushort test number + uint duration in ms.</summary>
        private const int TestPassPayloadSizeBytes = 6;
        /// <summary>Payload size of a TestSuiteEnd message: four little-endian ushort counters (total, passed, failed, skipped).</summary>
        private const int SuiteEndPayloadSizeBytes = 8;

        /// <summary>
        /// Send a protocol message with format: [MAGIC:4][Command:1][Length:2][Payload:N]
        /// Magic signature = 0x19740807 (SerialSignature from Consts.cs)
        /// </summary>
        internal static void SendMessage(byte command, byte[] payload)
        {
            // The protocol shares the UART with diagnostic traces written from IRQ handlers
            // and other threads. A frame must go out as one uninterrupted byte sequence, so
            // it is assembled up front and emitted through the atomic Log.WriteBytes.
            ushort length = (ushort)payload.Length;
            byte[] frame = new byte[HeaderSizeBytes + payload.Length];
            frame[0] = SerialSignatureByte0;
            frame[1] = SerialSignatureByte1;
            frame[2] = SerialSignatureByte2;
            frame[3] = SerialSignatureByte3;
            frame[4] = command;
            frame[5] = (byte)(length & ByteMask);
            frame[6] = (byte)((length >> Byte1Shift) & ByteMask);
            Array.Copy(payload, 0, frame, HeaderSizeBytes, payload.Length);
            Log.WriteBytes(frame);
        }

        /// <summary>Lowest character the parser accepts inside a protocol string.</summary>
        private const char MinProtocolChar = ' ';
        /// <summary>Highest character that survives one-byte encoding unchanged.</summary>
        private const char MaxProtocolChar = (char)0x7E;
        /// <summary>Stand-in written for a character the protocol cannot carry.</summary>
        private const byte UnsupportedCharByte = (byte)'?';

        /// <summary>
        /// Encode a protocol string as one byte per character. Anything
        /// outside printable ASCII becomes <c>?</c>, because the engine's
        /// parser treats a byte below 0x20 in a string field as proof that
        /// the frame was assembled from interleaved UART bytes and drops the
        /// whole message. Truncating a wider character produces exactly that:
        /// an em dash (U+2014) truncates to 0x14, and a skip reason carrying
        /// one was dropped as noise, which left the test sitting at the pass
        /// its TestStart had already implied. The same held for a failure
        /// message, so a real failure could be reported green.
        /// </summary>
        /// <param name="str">String to encode.</param>
        private static byte[] EncodeString(string str)
        {
            byte[] bytes = new byte[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                bytes[i] = c is >= MinProtocolChar and <= MaxProtocolChar ? (byte)c : UnsupportedCharByte;
            }
            return bytes;
        }

        private static void SendTestSuiteStart(string suiteName, ushort expectedTests)
        {
            var nameBytes = EncodeString(suiteName);
            var payload = new byte[UInt16FieldSizeBytes + nameBytes.Length];
            // First 2 bytes: expected test count
            payload[0] = (byte)(expectedTests & ByteMask);
            payload[1] = (byte)((expectedTests >> Byte1Shift) & ByteMask);
            // Rest: suite name
            Array.Copy(nameBytes, 0, payload, UInt16FieldSizeBytes, nameBytes.Length);
            SendMessage(TestSuiteStart, payload);
        }

        private static void SendTestStart(ushort testNumber, string testName)
        {
            var nameBytes = EncodeString(testName);
            var payload = new byte[UInt16FieldSizeBytes + nameBytes.Length];
            payload[0] = (byte)(testNumber & ByteMask);
            payload[1] = (byte)((testNumber >> Byte1Shift) & ByteMask);
            Array.Copy(nameBytes, 0, payload, UInt16FieldSizeBytes, nameBytes.Length);
            SendMessage(TestStart, payload);
        }

        private static void SendTestPass(ushort testNumber, uint durationMs)
        {
            var payload = new byte[TestPassPayloadSizeBytes];
            payload[0] = (byte)(testNumber & ByteMask);
            payload[1] = (byte)((testNumber >> Byte1Shift) & ByteMask);
            payload[2] = (byte)(durationMs & ByteMask);
            payload[3] = (byte)((durationMs >> Byte1Shift) & ByteMask);
            payload[4] = (byte)((durationMs >> Byte2Shift) & ByteMask);
            payload[5] = (byte)((durationMs >> Byte3Shift) & ByteMask);
            SendMessage(TestPass, payload);
        }

        private static void SendTestFail(ushort testNumber, string errorMessage)
        {
            var errorBytes = EncodeString(errorMessage);
            var payload = new byte[UInt16FieldSizeBytes + errorBytes.Length];
            payload[0] = (byte)(testNumber & ByteMask);
            payload[1] = (byte)((testNumber >> Byte1Shift) & ByteMask);
            Array.Copy(errorBytes, 0, payload, UInt16FieldSizeBytes, errorBytes.Length);
            SendMessage(TestFail, payload);
        }

        private static void SendTestSkip(ushort testNumber, string skipReason)
        {
            var reasonBytes = EncodeString(skipReason);
            var payload = new byte[UInt16FieldSizeBytes + reasonBytes.Length];
            payload[0] = (byte)(testNumber & ByteMask);
            payload[1] = (byte)((testNumber >> Byte1Shift) & ByteMask);
            Array.Copy(reasonBytes, 0, payload, UInt16FieldSizeBytes, reasonBytes.Length);
            SendMessage(TestSkip, payload);
        }

        private static void SendTestDestructiveReached(ushort testNumber)
        {
            var payload = new byte[UInt16FieldSizeBytes];
            payload[0] = (byte)(testNumber & ByteMask);
            payload[1] = (byte)((testNumber >> Byte1Shift) & ByteMask);
            SendMessage(TestDestructiveReached, payload);
        }

        private static void SendTestSuiteEnd(ushort total, ushort passed, ushort failed, ushort skipped)
        {
            var payload = new byte[SuiteEndPayloadSizeBytes];
            payload[0] = (byte)(total & ByteMask);
            payload[1] = (byte)((total >> Byte1Shift) & ByteMask);
            payload[2] = (byte)(passed & ByteMask);
            payload[3] = (byte)((passed >> Byte1Shift) & ByteMask);
            payload[4] = (byte)(failed & ByteMask);
            payload[5] = (byte)((failed >> Byte1Shift) & ByteMask);
            payload[6] = (byte)(skipped & ByteMask);
            payload[7] = (byte)((skipped >> Byte1Shift) & ByteMask);
            SendMessage(TestSuiteEnd, payload);
        }

        #endregion
    }
}
