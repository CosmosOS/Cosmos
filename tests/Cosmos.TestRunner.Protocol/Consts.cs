using System;

namespace Cosmos.TestRunner.Protocol
{
    /// <summary>
    /// Protocol constants for test runner communication.
    /// Ported from CosmosOS Cosmos.Debug.DebugConnectors with test-specific extensions.
    /// </summary>
    public static class Consts
    {
        // Frame magic

        /// <summary>Frame magic every protocol frame starts with; sent little-endian on the wire.</summary>
        public const uint SerialSignature = 0x19740807;

        /// <summary>Byte 0 (least significant) of the frame magic <see cref="SerialSignature"/> (0x19740807) as it appears little-endian on the wire.</summary>
        public const byte SerialSignatureByte0 = 0x07;

        /// <summary>Byte 1 of the frame magic <see cref="SerialSignature"/> (0x19740807, little-endian on the wire).</summary>
        public const byte SerialSignatureByte1 = 0x08;

        /// <summary>Byte 2 of the frame magic <see cref="SerialSignature"/> (0x19740807, little-endian on the wire).</summary>
        public const byte SerialSignatureByte2 = 0x74;

        /// <summary>Byte 3 (most significant) of the frame magic <see cref="SerialSignature"/> (0x19740807, little-endian on the wire).</summary>
        public const byte SerialSignatureByte3 = 0x19;

        /// <summary>Length of the frame magic <see cref="SerialSignature"/> (0x19740807 little-endian) on the wire, in bytes.</summary>
        public const int SerialSignatureLengthBytes = 4;

        // Suite-end marker

        /// <summary>
        /// Marker the kernel emits after the whole test suite finished, telling the
        /// host it can kill QEMU. Must match the kernel-side QemuKillMarkerByte0..7
        /// sequence in Cosmos.TestRunner.Framework/TestRunner.cs (separate assembly,
        /// duplicated by design as a wire contract).
        /// </summary>
        public static readonly byte[] SuiteEndMarker = { 0xDE, 0xAD, 0xBE, 0xEF, 0xCA, 0xFE, 0xBA, 0xBE };
    }

    /// <summary>
    /// Messages from Guest (Kernel) to Host (Test Runner)
    /// Extended with test-specific message types (100-109)
    /// </summary>
    public static class Ds2Vs
    {
        // Original CosmosOS debug messages (0-25). Reserved — unused by this
        // runner except Message; kept as CosmosOS wire-protocol documentation.
        public const byte Noop = 0;
        public const byte TracePoint = 1;
        public const byte Message = 192;
        public const byte BreakPoint = 3;
        public const byte Error = 4;
        public const byte Pointer = 5;
        public const byte Started = 6;
        public const byte MethodContext = 7;
        public const byte MemoryData = 8;
        public const byte CmdCompleted = 9;
        public const byte Registers = 10;
        public const byte Frame = 11;
        public const byte Stack = 12;
        public const byte Pong = 13;
        public const byte BreakPointAsm = 14;
        public const byte StackCorruptionOccurred = 15;
        public const byte MessageBox = 16;
        public const byte NullReferenceOccurred = 17;
        public const byte SimpleNumber = 18;
        public const byte SimpleLongNumber = 19;
        public const byte ComplexNumber = 20;
        public const byte ComplexLongNumber = 21;
        public const byte StackOverflowOccurred = 22;
        public const byte InterruptOccurred = 23;
        public const byte CoreDump = 24;
        public const byte KernelPanic = 25;

        // Test runner specific messages (100-107)
        /// <summary>
        /// Sent when test suite starts. Payload: string (suite name)
        /// </summary>
        public const byte TestSuiteStart = 100;

        /// <summary>
        /// Sent when individual test starts. Payload: ushort (test number) + string (test name)
        /// </summary>
        public const byte TestStart = 101;

        /// <summary>
        /// Sent when test passes. Payload: ushort (test number) + uint (duration in milliseconds)
        /// </summary>
        public const byte TestPass = 102;

        /// <summary>
        /// Sent when test fails. Payload: ushort (test number) + string (error message)
        /// </summary>
        public const byte TestFail = 103;

        /// <summary>
        /// Sent when test is skipped. Payload: ushort (test number) + string (skip reason)
        /// </summary>
        public const byte TestSkip = 104;

        /// <summary>
        /// Sent when test suite ends. Payload: ushort (total) + ushort (passed) + ushort (failed)
        /// </summary>
        public const byte TestSuiteEnd = 105;

        /// <summary>
        /// Sent on kernel startup to identify architecture. Payload: byte (arch_id) + byte (cpu_count)
        /// Architecture IDs: 1=x86, 2=x64, 3=ARM32, 4=ARM64
        /// </summary>
        public const byte ArchitectureInfo = 106;

        /// <summary>
        /// Sent after test suite ends with code coverage data.
        /// Payload: [HitCount:2][HitId1:2][HitId2:2]...
        /// Method IDs correspond to the coverage-map.txt generated at build time.
        /// </summary>
        public const byte CoverageData = 107;

        /// <summary>
        /// Sent by RunDestructive immediately before invoking a test action that
        /// is expected to never return (e.g. Power.Reboot, Power.Shutdown). The
        /// engine treats this marker — not a regular TestPass — as evidence that
        /// the boot reached a destructive test, so a kernel crash mid-suite is
        /// not misclassified as a successful destructive op. Payload: ushort
        /// (test number).
        /// </summary>
        public const byte TestDestructiveReached = 108;

        /// <summary>
        /// Sent when a test needs the host to change the machine under the
        /// running guest, such as pulling a USB stick out. Payload: string
        /// (the request, e.g. <c>usb-unplug</c> or <c>usb-plug 0</c>). No
        /// reply comes back: the test waits for the change to show up.
        /// </summary>
        public const byte HostRequest = 109;
    }

    /// <summary>
    /// Messages from Host (Test Runner) to Guest (Kernel)
    /// For test runner, we primarily receive messages, so this is minimal.
    /// Reserved — unused by this runner, kept as CosmosOS wire-protocol documentation.
    /// </summary>
    public static class Vs2Ds
    {
        public const byte Noop = 0;
        public const byte Continue = 4;
        public const byte Ping = 17;

        // Make sure this is always the last entry
        public const byte Max = 21;
    }

    /// <summary>
    /// Architecture identifiers for ArchitectureInfo message
    /// </summary>
    public enum Architecture : byte
    {
        Unknown = 0,
        x86 = 1,
        x64 = 2,
        ARM32 = 3,
        ARM64 = 4
    }
}
