# Testing

NativeAOT-Patcher has two complementary testing layers: **unit tests** that run in the host process, for the build-time toolchain (patcher, scanner, analyzer) and for kernel library logic that needs no hardware, and **kernel integration tests** that run compiled kernel images inside QEMU and report results over a binary UART protocol.

---

## Unit Tests

Unit tests live in the `tests/Cosmos.Tests.*` projects (xunit) and in `src/tests/Cosmos.Kernel.Tests.System` (NUnit), and are run with the standard .NET test runner. They do not require QEMU or any special infrastructure. CI runs each of them as its own job of the `.NET Tests` workflow (`.github/workflows/dotnet.yml`) on every push and pull request.

### Running Unit Tests

```bash
dotnet test tests/Cosmos.Tests.Patcher            # one toolchain project
dotnet test src/tests/Cosmos.Kernel.Tests.System   # the kernel library tests
```

### Test Projects

- **Cosmos.Tests.Build.Asm**: Verifies the assembly build task runs via clang.
  - `Test1`
- **Cosmos.Tests.Build.Analyzer.Patcher**: Validates that code does not contain plug architecture errors.
  - `Test_AnalyzeAccessedMember`
  - `Test_MethodNotImplemented`
  - `Test_StaticConstructorTooManyParameters`
  - `Test_StaticConstructorNotImplemented`
- **Cosmos.Tests.Scanner**: Validates that all required plugs are detected correctly.
  - `LoadPlugMethods_ShouldReturnPublicStaticMethods`
  - `LoadPlugMethods_ShouldReturnEmpty_WhenNoMethodsExist`
  - `LoadPlugMethods_ShouldContainAddMethod_WhenPlugged`
  - `LoadPlugs_ShouldFindPluggedClasses`
  - `LoadPlugs_ShouldIgnoreClassesWithoutPlugAttribute`
  - `LoadPlugs_ShouldHandleOptionalPlugs`
  - `FindPluggedAssemblies_ShouldReturnMatchingAssemblies`
- **Cosmos.Tests.Patcher**: Ensures that plugs are applied successfully to target methods and types.
  - `PatchAssembly_ShouldSkipWhenNoMatchingPlugs`
  - `PatchObjectWithAThis_ShouldPlugInstanceCorrectly`
  - `PatchConstructor_ShouldPlugCtorCorrectly`
  - `PatchProperty_ShouldPlugProperty`
  - `PatchType_ShouldReplaceAllMethodsCorrectly`
  - `PatchType_ShouldPlugAssembly`
  - `AddMethod_BehaviorBeforeAndAfterPlug`
- **Cosmos.Kernel.Tests.System**: Exercises `Cosmos.Kernel.System` logic that needs no hardware, in the host process (`Tcp` receive buffer, `Address` identity and formatting). One nested fixture per member under test, holding an `InternalsVisibleTo` grant from the library.
  - `AppendToData.WhenBothData_AndOtherAreEmpty_DataIsEmpty`
  - `AppendToData.WhenDataIsNotEmpty_AndOtherIsEmpty_DataDoesNotChange`
  - `AppendToData.WhenDataIsNotEmpty_AndOtherIsNotEmpty_OtherIsAppendedToData`
  - `AdvanceDataOffset.WhenAdvancingByZero_NoChangesAreMade`
  - `AdvanceDataOffset.WhenAdvancingByOneAndLengthIsTwo_OnlyLastElementRemains`
  - `AdvanceDataOffset.WhenAdvancingByTwoAndLengthIsTwo_DataLengthIsZero`
  - `Constructors.GivenPackedValue_SplitsItMostSignificantOctetFirst`
  - `Constructors.GivenBufferAndOffset_ReadsFourBytesFromTheOffset`
  - `Constructors.GivenSpanOfWrongLength_Throws`
  - `Id.GivenOctets_PacksThemMostSignificantFirst`
  - `Equality.GivenTheSameOctets_TwoInstancesAreEqualAndHashAlike`
  - `Equality.GivenDifferentOctets_TwoInstancesAreNotEqual`
  - `Equality.GivenNull_IsNotEqual`
  - `CompareTo.GivenTwoAddresses_OrdersByPackedValue`
  - `CompareTo.GivenNull_OrdersAfterIt`
  - `Formatting.GivenAddress_WritesDottedDecimal`
  - `IsBroadcastAddress.GivenAllOnes_IsTrue`
  - `IsBroadcastAddress.GivenAnyOtherAddress_IsFalse`
- **Cosmos.Tests.NativeWrapper**: Contains runtime assets; no unit tests.
- **Cosmos.Tests.NativeLibrary**: Provides native code used in tests; no unit tests.

---

## Kernel Integration Tests

Kernel integration tests compile a real NativeAOT kernel, boot it in QEMU, and communicate results back to the host over a binary UART protocol. These tests exercise the full build and runtime pipeline.

### Test Suites

| Suite | Tests | Description |
|-------|-------|-------------|
| **HelloWorld** | 3 | Basic arithmetic, boolean logic, integer comparison |
| **Memory** | 85 | Boxing/unboxing, memory allocation, collections, memory copy, GC |

#### HelloWorld Tests

- `Test_BasicArithmetic`: Addition (2+2=4)
- `Test_BooleanLogic`: True/False assertions
- `Test_IntegerComparison`: Equality and comparison operators

#### Memory Tests

**Boxing/Unboxing (11 tests):**
- `Boxing_Char`, `Boxing_Int32`, `Boxing_Byte`, `Boxing_Long`
- `Boxing_Nullable`, `Boxing_Interface`, `Boxing_CustomStruct`
- `Boxing_ArrayCopy`, `Boxing_Enum`, `Boxing_ValueTuple`, `Boxing_NullInterface`

**Memory Allocation (8 tests):**
- `Memory_CharArray`, `Memory_StringAllocation`, `Memory_IntArray`
- `Memory_StringConcat`, `Memory_StringBuilder`
- `Memory_ZeroLengthArray`, `Memory_EmptyString`, `Memory_LargeAllocation`

**Generic Collections - List (14 tests):**
- `Collections_ListInt`, `Collections_ListString`, `Collections_ListByte`
- `Collections_ListLong`, `Collections_ListStruct`
- `Collections_ListContains`, `Collections_ListIndexOf`, `Collections_ListRemoveAt`
- `Collections_ListInsert`, `Collections_ListRemove`, `Collections_ListClear`
- `Collections_ListToArray`, `Collections_ListForeach`, `Collections_ListEmpty`

**Generic Collections - Dictionary (9 tests):**
- `Collections_DictCustomComparer`, `Collections_DictAddGet`, `Collections_DictIndexer`
- `Collections_DictContains`, `Collections_DictRemove`, `Collections_DictClear`
- `Collections_DictTryGetValue`, `Collections_DictKeysValues`, `Collections_DictEmpty`

**Generic Collections - IEnumerable (1 test):**
- `Collections_IEnumerable`

**Memory Copy / SIMD (15 tests):**
- `MemCopy_8Bytes`, `MemCopy_16Bytes`, `MemCopy_24Bytes`, `MemCopy_32Bytes`
- `MemCopy_48Bytes`, `MemCopy_64Bytes`, `MemCopy_80Bytes`, `MemCopy_128Bytes`
- `MemCopy_256Bytes`, `MemCopy_264Bytes`
- `MemSet_64Bytes`, `MemMove_Overlap`, `MemMove_Overlap_DestBeforeSrc`
- `MemCopy_0Bytes`, `MemCopy_1Byte`

**Array.Copy (5 tests):**
- `ArrayCopy_IntArray`, `ArrayCopy_ByteArray`, `ArrayCopy_LargeArray`
- `ArrayCopy_ZeroLength`, `ArrayCopy_Overlap`

**Garbage Collection (22 tests):**
- `GC_IsEnabled`, `GC_GetStats`, `GC_CollectBasic`, `GC_StatsIncrement`
- `GC_ExactCollectionCount`, `GC_ObjectSurvival`, `GC_StringSurvival`
- `GC_ArraySurvival`, `GC_ListSurvival`, `GC_UnreachableExactCount`
- `GC_ObjectGraphSurvival`, `GC_MixedTypeSurvival`, `GC_AllocAfterCollect`
- `GC_WeakReference`, `GC_LargeAllocCollect`, `GC_StructArraySurvival`
- `GC_DictSurvival`, `GC_PageAccounting`, `GC_DependentHandle`
- `GC_DependentHandleCleanup`, `GC_HandleStoreIntegrity`, `GC_PinnedHeapReuse`

### Running Kernel Tests

#### From VS Code

**Using Tasks (recommended):**
1. Press `Ctrl+Shift+P` → "Tasks: Run Task"
2. Select one of:
   - **Run Test: HelloWorld (x64)**: Console + XML output
   - **Run Test: HelloWorld (x64, Console Only)**: Console output only
   - **Run Test: HelloWorld (ARM64)**: ARM64 test with XML output
   - **Dev Test: HelloWorld (x64)**: Developer mode with verbose output

**Debug test runner:**
1. Open the Run & Debug panel (`Ctrl+Shift+D`)
2. Select a configuration:
   - **Debug Test Runner (HelloWorld x64)**
   - **Debug Test Runner (HelloWorld ARM64)**
3. Press `F5`

#### From the Command Line

```bash
# Run test with XML output
dotnet run --project tests/Cosmos.TestRunner.Engine/Cosmos.TestRunner.Engine.csproj -- \
  tests/Kernels/Cosmos.Kernel.Tests.HelloWorld \
  x64 \
  60 \
  test-results.xml

# Run test with console output only
dotnet run --project tests/Cosmos.TestRunner.Engine/Cosmos.TestRunner.Engine.csproj -- \
  tests/Kernels/Cosmos.Kernel.Tests.HelloWorld \
  x64 \
  60
```

**Arguments:**
1. Kernel project path (absolute or relative)
2. Architecture: `x64` or `arm64`
3. Timeout in seconds
4. *(Optional)* XML output path (JUnit format)
5. *(Optional)* Mode: `ci` or `dev`

**Recommended timeouts:**

| Suite | x64 | ARM64 |
|-------|-----|-------|
| HelloWorld | 60 s | 90 s |
| Memory | 180 s | 300 s |

### Output Formats

#### Console (colored)

```
================================================================================
Starting test suite: HelloWorld Basic Tests
Architecture: x64
Time: 2025-11-05 04:06:48
================================================================================
[1] Test_BasicArithmetic: PASSED (15ms)
[2] Test_BooleanLogic: PASSED (12ms)
[3] Test_IntegerComparison: PASSED (10ms)
================================================================================
Suite: HelloWorld Basic Tests
Total tests: 3 | Passed: 3 | Failed: 0 | Skipped: 0 | Duration: 0.04s
================================================================================
ALL TESTS PASSED
================================================================================
```

#### XML (JUnit format)

```xml
<?xml version="1.0" encoding="utf-16"?>
<testsuites name="HelloWorld Basic Tests" tests="3" failures="0" skipped="0" time="0.037">
  <testsuite name="HelloWorld Basic Tests" tests="3" failures="0" skipped="0" time="0.037">
    <properties>
      <property name="architecture" value="x64" />
    </properties>
    <testcase name="Test_BasicArithmetic" classname="HelloWorld Basic Tests" time="0.015" />
    <testcase name="Test_BooleanLogic" classname="HelloWorld Basic Tests" time="0.012" />
    <testcase name="Test_IntegerComparison" classname="HelloWorld Basic Tests" time="0.010" />
  </testsuite>
</testsuites>
```

### Exit Codes

| Code | Meaning |
|------|---------|
| 0 | All tests passed (skipped tests are acceptable) |
| 1 | Tests failed or execution error |
| 137 | Timeout (SIGKILL) |

---

## UART Debug Protocol

Test kernels communicate results back to the host engine over a binary protocol embedded in the QEMU serial (UART) stream. The protocol is defined in `tests/Cosmos.TestRunner.Protocol/` and is ported from the CosmosOS debug connector.

### Framing

Every message is prefixed with a 4-byte magic signature, followed by the command byte and a 2-byte little-endian payload length:

```
[Magic: 4 bytes][Command: 1 byte][Length: 2 bytes LE][Payload: N bytes]
```

- **Magic**: `0x07 0x08 0x74 0x19` (i.e. `0x19740807` in little-endian, `SerialSignature` in `Consts.cs`)
- **Command**: one of the `Ds2Vs` constants (see table below)
- **Length**: number of payload bytes that follow

After the final `TestSuiteEnd` message the kernel also sends an 8-byte termination marker (`0xDE 0xAD 0xBE 0xEF 0xCA 0xFE 0xBA 0xBE`) so the engine can kill QEMU immediately without waiting for the full timeout.

### Commands (Kernel → Host, `Ds2Vs`)

Test-runner-specific commands occupy the range **100-109**. The original CosmosOS debug commands (0-25) are also defined but are not used by the test runner.

| Command | Value | Payload format | Description |
|---------|-------|----------------|-------------|
| `TestSuiteStart` | 100 | `[ExpectedTests: 2 LE][SuiteName: UTF-8]` | Sent once when the test suite begins |
| `TestStart` | 101 | `[TestNumber: 2 LE][TestName: UTF-8]` | Sent before each test executes |
| `TestPass` | 102 | `[TestNumber: 2 LE][DurationMs: 4 LE]` | Sent when a test passes |
| `TestFail` | 103 | `[TestNumber: 2 LE][ErrorMessage: UTF-8]` | Sent when an assertion fails |
| `TestSkip` | 104 | `[TestNumber: 2 LE][SkipReason: UTF-8]` | Sent when a test is explicitly skipped |
| `TestSuiteEnd` | 105 | `[Total: 2 LE][Passed: 2 LE][Failed: 2 LE]` | Sent once when the test suite ends |
| `ArchitectureInfo` | 106 | `[ArchId: 1][CpuCount: 1]` | Sent on kernel startup (arch IDs: 1=x86, 2=x64, 3=ARM32, 4=ARM64) |
| `CoverageData` | 107 | `[HitCount: 2 LE][MethodId: 2 LE]...` | Sent after the suite ends by a kernel built with coverage |
| `TestDestructiveReached` | 108 | `[TestNumber: 2 LE]` | Sent by `TR.RunDestructive` right before an action that never returns (reboot, shutdown) |
| `HostRequest` | 109 | `[Request: ASCII]` | Asks the engine to change the machine under the running guest (see below) |

### Message Flow

A typical session looks like this:

```
→ TestSuiteStart  (suiteName="HelloWorld Basic Tests", expectedTests=3)
→ TestStart       (testNumber=1, testName="Test_BasicArithmetic")
→ TestPass        (testNumber=1, durationMs=15)
→ TestStart       (testNumber=2, testName="Test_BooleanLogic")
→ TestPass        (testNumber=2, durationMs=12)
→ TestStart       (testNumber=3, testName="Test_IntegerComparison")
→ TestPass        (testNumber=3, durationMs=10)
→ TestSuiteEnd    (total=3, passed=3, failed=0)
→ [0xDE 0xAD 0xBE 0xEF 0xCA 0xFE 0xBA 0xBE]  ← termination marker
```

### Parser

`tests/Cosmos.TestRunner.Engine/Protocol/UartMessageParser.cs` reads the raw UART log captured by QEMU (`uart-output.log`), scans byte-by-byte for the magic signature, validates the command byte and length, then dispatches to the appropriate parse helper.

Corruption detection: the `TestSuiteEnd` payload is validated by checking `total == passed + failed`. If this invariant does not hold (e.g. due to a timer-interrupt interleave corrupting UART bytes), the end message is ignored and results fall back to the individually tracked counters.

### Host Requests

A test calls `TR.RequestHost(request)` when it needs the machine changed under it, and the engine carries the request out through QEMU's QMP monitor as soon as the frame shows up on the UART:

| Request | Effect |
|---------|--------|
| `usb-unplug [n]` | Pulls USB stick `n` (0 by default) off the xHCI controller |
| `usb-plug [n]` | Plugs it back in, on the same disk image |

Nothing replies. The test waits for the change itself, and must see it within 10 s: that long without a protocol message and the engine takes the kernel for hung. Only a profile that attaches a USB disk launches QEMU with a monitor; the engine logs and drops a request from any other. The Storage suite's `UsbHotPlug_*` tests are the example.

### Host → Kernel Commands (`Vs2Ds`)

The test runner currently does not send commands to the kernel. The `Vs2Ds` class (`Noop=0`, `Continue=4`, `Ping=17`) is inherited from the CosmosOS debug connector and reserved for future use.

---

## Project Structure

```
tests/
├── Cosmos.TestRunner.Engine/        # Host-side test runner
│   ├── Engine.cs                    # Main orchestration
│   ├── Engine.Build.cs              # NativeAOT build pipeline
│   ├── Program.cs                   # CLI entry point
│   ├── TestConfiguration.cs         # Configuration
│   ├── TestResults.cs               # Result model
│   ├── Hosts/                       # QEMU host implementations
│   │   ├── IQemuHost.cs
│   │   ├── QemuX64Host.cs
│   │   └── QemuARM64Host.cs
│   ├── OutputHandlers/              # Result output formats
│   │   ├── OutputHandlerBase.cs
│   │   ├── OutputHandlerConsole.cs  # Colored terminal output
│   │   ├── OutputHandlerXml.cs      # JUnit XML output
│   │   └── MultiplexingOutputHandler.cs
│   └── Protocol/
│       └── UartMessageParser.cs     # Binary message parser
├── Cosmos.TestRunner.Framework/     # In-kernel test framework
│   ├── TestRunner.cs                # Start / Run / Skip / Finish
│   └── Assert.cs                    # Assertion helpers
├── Cosmos.TestRunner.Protocol/      # Shared protocol definitions
│   ├── Consts.cs                    # Magic signature and constants
│   └── Messages.cs                  # Typed message classes
├── Cosmos.Tests.Build.Asm/          # Unit tests: Clang assembly build task
├── Cosmos.Tests.Build.Analyzer.Patcher/ # Unit tests: plug analyzer
├── Cosmos.Tests.Scanner/            # Unit tests: plug scanner
├── Cosmos.Tests.Patcher/            # Unit tests: IL patcher
├── Cosmos.Tests.NativeWrapper/      # Runtime assets (no tests)
├── Cosmos.Tests.NativeLibrary/      # Native code for tests (no tests)
└── Kernels/                         # Kernel test projects
    ├── Cosmos.Kernel.Tests.HelloWorld/
    │   ├── Kernel.cs
    │   └── Bootloader/limine.conf
    └── Cosmos.Kernel.Tests.Memory/
        ├── Kernel.cs
        └── Bootloader/limine.conf
```

`src/tests/Cosmos.Kernel.Tests.System/` holds the host-side tests of `Cosmos.Kernel.System`. It sits under `src/` because it takes a project reference on the library and an `InternalsVisibleTo` grant from it.

---

## Writing a Test Kernel

### Minimal Example

Test kernels inherit from `Cosmos.Kernel.System.Kernel` and run all tests inside `BeforeRun()`. Use the `TR` alias for `TestRunner` and `Assert` for assertions.

```csharp
using Cosmos.Kernel.Core.IO;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.MyTests;

public class Kernel : Sys.Kernel
{
    protected override void BeforeRun()
    {
        // Initialize test suite (expectedTests must equal the total number of TR.Run + TR.Skip calls)
        TR.Start("My Test Suite", expectedTests: 3);

        TR.Run("Test_Addition", () =>
        {
            int result = 2 + 2;
            Assert.Equal(4, result);
        });

        TR.Run("Test_StringOps", () =>
        {
            string str = "Hello";
            Assert.Equal("Hello", str);
            Assert.NotNull(str);
        });

        // Mark unsupported operations as skipped rather than letting them crash
        // TR.Skip also counts toward expectedTests
        TR.Skip("Test_Unsupported", "Feature not implemented");

        TR.Finish();

        Serial.WriteString("[Tests Complete - System Halting]\n");
        Stop();
    }

    protected override void Run()
    {
        // Tests completed in BeforeRun, nothing to do here
    }

    protected override void AfterRun()
    {
        Cosmos.Kernel.Kernel.Halt();
    }
}
```

### Kernel Lifecycle

The `Sys.Kernel` base class drives a fixed lifecycle:

1. `OnBoot()`: system initialization (called automatically, rarely overridden)
2. `BeforeRun()`: **run all tests here**, then call `Stop()`
3. `Run()`: called in a loop until `Stop()` is invoked; leave empty for test kernels
4. `AfterRun()`: called once after the loop exits; call `Cosmos.Kernel.Kernel.Halt()` here

### Available Assertions

```csharp
// Equality: typed overloads (int, uint, long, byte, bool, string, byte[], int[])
Assert.Equal(expected, actual);
Assert.Equal<T>(expected, actual);   // Generic overload (requires IEquatable<T>)

// Null checks
Assert.Null(obj);
Assert.NotNull(obj);

// Boolean
Assert.True(condition);
Assert.True(condition, "message");
Assert.False(condition);

// Manual failure
Assert.Fail("Custom error message");
```

> **Note:** `Assert` uses static failure state (no exceptions) for NativeAOT compatibility.
> Only the first failure per test is recorded; subsequent assertions in the same `TR.Run` block are still evaluated.

### Test Status

| Status | When |
|--------|------|
| **Passed** | Test completed without assertion failures |
| **Failed** | Assertion set the failure state inside `TR.Run` |
| **Skipped** | Test explicitly marked via `TR.Skip(name, reason)` |

### Adding a New Test Suite

1. Create a kernel project under `tests/Kernels/Cosmos.Kernel.Tests.{Name}/`
2. Copy `.csproj` and `Bootloader/limine.conf` from an existing suite; update the ELF path in `limine.conf`
3. Implement tests using `TestRunner.Framework`
4. Add a CI job in `.github/workflows/kernel-tests.yml`:
   - Copy an existing `*-tests` job, rename it, and update the kernel path
   - Add a corresponding `{name}-results` job, which renders the PR comment
   - Add the new job to the `test-summary` dependencies
5. Add VS Code tasks in `.vscode/tasks.json`

---

## CI Integration

The CI workflow (`.github/workflows/kernel-tests.yml`) runs kernel integration tests on both x64 and ARM64.

**Jobs:**
- `helloworld-tests`: Matrix build for x64/arm64
- `helloworld-results`: Renders the combined PR comment into a `pr-comment-helloworld` artifact
- `memory-tests`: Matrix build for x64/arm64
- `memory-results`: Renders the combined PR comment into a `pr-comment-memory` artifact
- `test-summary`: Final status summary

**Triggers:**
- Push to `main`
- Pull requests (any branch)
- Manual dispatch with architecture selection

**PR Comments:** Each test suite gets one comment with separate rows for x64 and arm64, showing test counts, duration, and links to artifacts. The `*-results` jobs only render the comment: a `pull_request` run for a pull request from a fork holds a read-only token and cannot post. `.github/workflows/kernel-test-comment.yml` runs on `workflow_run` once Kernel Tests or Kernel Coverage completes, in the base repository and with write access whatever the origin of the pull request, downloads the `pr-comment-*` artifacts and posts or updates the comments. It trusts the PR number in an artifact only when that pull request's head is the run's head commit, and GitHub runs it from the default branch, so a change to it takes effect after merge.

**Artifacts (30-day retention):**
- `test-results-{suite}-{arch}.xml`: JUnit XML results
- `uart-log-{suite}-{arch}`: Full UART output
- `{Suite}-Test-ISO-{arch}`: Bootable kernel ISO + ELF

### Example CI Step

```yaml
- name: Run Cosmos Tests
  run: |
    dotnet run --project tests/Cosmos.TestRunner.Engine/Cosmos.TestRunner.Engine.csproj -- \
      tests/Kernels/Cosmos.Kernel.Tests.HelloWorld \
      x64 \
      120 \
      test-results.xml \
      ci

- name: Publish Test Results
  uses: dorny/test-reporter@v1
  if: always()
  with:
    name: Cosmos Tests
    path: test-results.xml
    reporter: java-junit
```

---

## Performance Reference

| Stage | x64 | ARM64 |
|-------|-----|-------|
| Kernel build | ~60 s | ~70 s |
| HelloWorld execution | 2-5 s | 5-10 s |
| Memory execution | 60-120 s | 120-240 s |

---

## Troubleshooting

### Timeout
- Increase the timeout argument
- Check `uart-output.log` for boot issues
- Verify QEMU is installed: `qemu-system-x86_64 --version`

### Build Failures
- Run `.devcontainer/postCreateCommand.sh` to rebuild the framework
- Restore NuGet packages: `dotnet restore`
- Verify .NET 9 SDK is installed

### ARM64 Issues
- Ensure UEFI firmware is present: `~/.cosmos/tools/qemu/share/qemu/edk2-aarch64-code.fd`
- Use a longer timeout (90 s+ for HelloWorld, 180 s+ for Memory)

### #UD Exceptions (Invalid Opcode)
Some operations may trigger Invalid Opcode faults depending on the runtime state. Use `TR.Skip()` to mark them instead of letting the kernel crash.
