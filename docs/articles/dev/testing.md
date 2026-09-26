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
| **Drivers** | 47 per cell | The driver kit: registration, ranking, interrupts, work items, events, publications and teardown of registered drivers, on hardware no built-in driver claims |

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

#### Drivers Tests

The driver kit. Each [profile](#hardware-profiles) attaches hardware that no built-in driver claims. The kernel is built with `CosmosEnableStorage=false`, so the built-in NVMe driver never takes the `nvme` profile's controller; every other switch keeps its default, and Network and Mouse must stay on, since the drivers publish network links and mice to their managers. The kernel registers its test drivers, PCI and USB, from its constructor, the driver pass in `Global.StartKernel` offers them the free PCI functions and USB interfaces, and the tests check what came of it. On the `usb-hid` cell the tests then have the engine move, pull out and plug back in the USB devices through [host requests](#host-requests), which exercises the kit on the USB hot-plug thread: a device plugged in after the pass, and the unplug teardown.

| Profile | Architectures | Hardware |
|---------|---------------|----------|
| `edu` | x64, arm64 | QEMU's edu test device (1234:11e8) |
| `rtl8139` | x64, arm64 | A Realtek RTL8139 NIC (10ec:8139) with a user-mode netdev, whose DHCP server the suite's RTL8139 driver gets a lease from, and no other NIC |
| `e1000e-arm64` | arm64 | An Intel 82574L NIC (8086:10d3), which the E1000E built-in claims on x64 only |
| `usb-hid` | x64, arm64 | A USB mouse, a USB tablet and a USB keyboard on `qemu-xhci` root ports, entries 0, 1 and 2 of its `usb` list, and no USB stick. All three present QEMU's HID IDs, 0627:0001: the mouse a HID boot mouse interface (3/1/2), the tablet a HID interface with no boot subclass or protocol (3/0/0), and the keyboard a HID boot keyboard interface (3/1/1), which the built-in USB keyboard driver takes |
| `nvme` | x64, arm64 | QEMU's NVMe controller (1b36:0010), free because the suite builds without Storage |

The `gicv2` and `gicv3` modifiers run every profile on both GICs on arm64, so x64 has 4 cells and arm64 has 15. MSI-X is expected on x64 and on the `+gicv3` cells, whose ITS routes it; the arm64 cells without it (the bare cell is GICv2) expect polled interrupts. The same drivers are registered on every cell:

- an `edu-class` driver matching edu by class, registered first, and three drivers matching edu by device ID: `edu-throws`, which maps BAR 0, allocates DMA memory, requests interrupts, creates an event and two work items, schedules one of them, and publishes a mouse and a network link, then throws from Probe; `edu-reentrant`, which tries to register a driver from its factory and its Probe and then declines; and `edu`, which binds, drives the device, publishes a mouse, requests interrupts (polled: edu has no MSI-X) and raises one it keeps pending until Probe returns
- four Ethernet class drivers: two, with and without the programming interface, that decline; `e1000e-irq`, registered after them, which binds the 82574L only and requests its interrupts; and `rtl8139`, registered last, which binds the RTL8139 only, through memory BAR 1 on both architectures, with 32-bit DMA buffers, polled interrupts (the chip has no MSI-X) and a receive work item, and publishes a network link whose transmit handler feeds the chip
- `nvme-identify`, matching NVMe by class, and `nvme-fails`, registered after it but matching by device ID, which requests interrupts, allocates 64 pages of DMA memory and fails; `nvme-identify` then binds the controller, requests interrupts again and submits an Identify Controller command whose completion arrives during Probe
- six USB drivers, in this order: `usb-hid-class`, matching HID by class alone, which would bind any interface it were offered; `usb-qemu-hid`, matching QEMU's HID devices by vendor and product ID, which tries to register a driver from its factory and its Probe, asks for endpoints it must be refused, and declines having opened nothing; `usb-mouse-declines`, matching the boot mouse interface and declining; `usb-boot-mouse`, the driver kit's sample boot mouse driver matching the same interface, which selects the boot protocol, asks for an idle report every 4 ms, publishes a mouse, creates an event and a work item, opens the interrupt IN endpoint, lets it run for 50 ms and reads the device descriptor before it binds, and records its Remove; `usb-tablet-fails`, matching the tablet's interface exactly, which the first time it is offered one publishes a mouse, opens the interrupt IN endpoint, asks for an idle report every 4 ms and fails, and declines every tablet after that having opened nothing; and `usb-tablet-link`, matching HID interfaces with no boot subclass, which ranks below it, publishes a network link whose transmit handler only counts frames, and binds

Every cell reports the same 80 tests, and each test is skipped on the cells whose profile attaches other hardware:

- `Profile_Recognized`: the cell's profile is one of the five above, since a profile the suite does not know would otherwise skip everything
- `Pci_ProfileFunctionEnumeratedOnce`, `Pci_ProfileFunctionClassMatches`: the profile's PCI function was enumerated exactly once, with the expected base class and subclass
- `Pci_ProfileFunctionUnownedBeforePass`, `Pci_ProfileFunctionOwnerAfterPass`: the function had no owner when the kernel was constructed; after the pass, edu is owned by `edu`, the 82574L by `e1000e-irq`, NVMe by `nvme-identify`, and the RTL8139 by `rtl8139`
- `Usb_XhciOwnedByXhci`, `Usb_MouseEnumeratedOnce`, `Usb_TabletEnumeratedOnce`, `Usb_MouseInterfaceBoundByPass`: the xHCI controller is owned by `xhci`; exactly one device presents a HID boot mouse interface (class 3, subclass 1, protocol 2) and one the tablet's (class 3, subclass 0, protocol 0); no class driver had bound the mouse interface when the kernel was constructed, and after the pass it is `usb-boot-mouse`'s, held for the USB stack by the kit's class driver along with the binding's context
- `Register_TakenNameRefused`, `Register_InvalidRegistrationThrows`, `Register_AfterPassThrows`, `Register_FromDriverCallbackThrows`: a second registration under a taken name, or a built-in's name such as `xhci` or `gop`, is refused; an empty name, no match entries or a default `PciMatch` throw `ArgumentException`; `Register` after the pass, or from a factory or a Probe, throws `InvalidOperationException`, from a factory or a Probe with the re-entrancy check's message, since the closed registration alone would refuse those calls too
- `Register_UsbTakenNameRefused`, `Register_UsbInvalidRegistrationThrows`, `Register_UsbAfterPassThrows`, `Register_FromUsbDriverCallbackThrows`: the same rules for USB registrations, in the name space PCI and USB share: a USB registration named after a PCI one, a PCI registration named after a USB one, and either named after a built-in of either bus (`hub`, `HID boot keyboard`, `mass storage`, `xhci`) are refused
- `Ranking_DeviceMatchBeatsClassMatch`, `Ranking_FailedAndDeclinedFallThrough`, `Ranking_ClassWithInterfaceBeatsClass`: device matches are offered edu before the class match registered ahead of them, in registration order, each failed or declined attempt passing the function on, and on NVMe the failing device match is offered the controller before the class match registered ahead of it; on the NIC cells the class match with a programming interface is offered the function before those without, which follow in registration order: `rtl8139` comes last, after `e1000e-irq` declined the RTL8139
- `UsbRanking_DeviceMatchFallsThroughToBootMouse`: the mouse interface is offered to `usb-qemu-hid`, then to `usb-mouse-declines` and `usb-boot-mouse` in registration order, and `usb-hid-class`, registered first, is never offered anything
- `UsbBind_BootMouseProbeSucceeded`, `UsbContext_ControlInReadsDeviceDescriptor`, `UsbContext_ProbeOnlyMembersThrowAfterProbe`, `UsbContext_OpenRefusesOtherEndpoints`: in the boot mouse driver's Probe, SET_PROTOCOL and SET_IDLE succeed, `OpenInterruptIn` returns true and GET_DESCRIPTOR(device) reads 18 bytes; after Bound, the same request with a 64-byte buffer reports the 18 bytes the device sent, with QEMU's IDs; the bound context refuses endpoints, events, work items and publications, is present, sits at `usb/1-<root port>:1.0` and describes the mouse; and `usb-qemu-hid` got false from `TryOpenBulk` on an interrupt endpoint and from `OpenInterruptIn` on an endpoint the interface lacks, which opened nothing, so both its declines passed the interface on
- `UsbInterrupts_ReportsOnlyAfterBound`, `UsbBind_PublishedMouseMovesPointer`: none of the reports the mouse sent while Probe ran reached the handler, and they do once Bound; SET_IDLE back to reports on change succeeds through the bound context; and a report through the mouse the driver published moves `MouseManager`'s pointer and sets its buttons
- `UsbNoFallThrough_TabletOfferingEnds`, `UsbTeardown_FailedAttemptInvalidated`: the tablet is offered to `usb-qemu-hid`, then to `usb-tablet-fails`, which opened its endpoint and failed, and to no one after, so neither `usb-tablet-link` nor `usb-hid-class` gets it and it stays without a driver; the failed attempt's context throws on control requests, its mouse never reached the mouse manager, and none of the reports the tablet keeps sending reaches its handler
- `UsbHotPlug_PointerMoveReachesBootMouseDriver`: the hot-plug thread runs; QEMU moves the mouse (`usb-pointer-move 0 12 7 1`) and the report reaches `MouseManager` through the kit and `usb-boot-mouse`, whose own sums of what it passed on match the movement and the left button, so the move did not come through x64's PS/2 mouse; a second request releases the button. On the GICv2 cells the xHCI is polled by the hot-plug thread every 250 ms, and every hot-plug wait allows for it
- `UsbUnplug_RemoveRunsOnceAfterWithdrawal`, `UsbUnplug_MouseLeavesMouseManager`, `UsbUnplug_StaleContextAnswersDisconnected`, `UsbUnplug_WorkItemsAndEventsCancelled`, `UsbUnplug_InterfaceLeavesDeviceList`: the mouse is pulled out (`usb-device-unplug 0`) while its binding's work item runs on `driver-work` and its left button is held (`usb-pointer-move 0 12 7 1`); `Remove` runs once, with `IsPresent` already false, the published mouse already withdrawn, and only after the running work item returned; the mouse is out of `MouseManager`, its handler cleared, the left button it held released, and a report through its stale reporter moves nothing; the context answers `Disconnected` to control requests, ends `Removed`, refuses the Probe-only members, and the interface keeps no binding; the work item refuses a `Schedule` and never runs again, and a `Wait` on the event returns false at once; and the interface leaves the device list
- `UsbReplug_NewDriverBindsOnHotPlugThread`, `UsbReplug_NewMouseRegisteredAndListed`, `UsbReplug_ReportsReachNewBinding`: plugged back in (`usb-device-plug 0`), the mouse is offered as at boot, `usb-qemu-hid`, `usb-mouse-declines`, `usb-boot-mouse`, on the hot-plug thread, not the boot thread, and a new driver instance binds it through a new context, with no report reaching it during Probe and no second `Remove`; its mouse is registered with `MouseManager` and listed in the device list; and QEMU's pointer movement reaches `MouseManager` through it
- `UsbTabletUnplug_UnboundInterfaceLeavesDeviceList`, `UsbTabletReplug_LinkDriverBindsAfterDeclines`: the tablet, which no driver took, is pulled out and leaves the device list; plugged back in, it is offered to `usb-qemu-hid` and `usb-tablet-fails`, which both decline having opened nothing, then to `usb-tablet-link`, which binds it on the hot-plug thread; its link joins `NetworkManager` after every device already there, which keeps the primary device, or becomes primary on the arm64 cells whose virtio-net NIC has no interrupt path and never registers
- `UsbTabletUnplug_LinkLeavesNetworkManager`, `UsbTabletReplug_StaleAdapterNamesNoNewLink`: the link, configured with an address and carrying a frame, leaves `NetworkManager` when the tablet is pulled out, before `Remove` runs; the primary device is what it was before, or none; sends through it fail before its transmit handler; the stack forgets its addresses, configuration and receive handler; and the `NetworkAdapter` taken for it names no device. Plugged back in again, the tablet's new link takes the index the first had, and the stale handle still names no device
- `LinkWithdraw_SendInFlightNeverTransmits`, `LinkWithdraw_DeliverInFlightNeverReachesStack`, `LinkWithdraw_RouteLookupSurvivesRemoval`: the races a USB network link's unplug runs against other threads, made on purpose 100 times each, since a tablet pulled out meets them only by chance. A thread sends back to back through a link while the test withdraws it each time the scheduler hands it the CPU back, wherever the sender was, and puts a new one in its place: the transmit handler never runs for a link already withdrawn, even for a send preempted between the link's first check and the handler. The same with a thread delivering frames: none reaches the receive handler once the link is withdrawn. A thread looks up the source address of a destination on the second of two configurations the test adds, while the test adds a third configuration and takes it out again each time it gets the CPU back: no lookup throws or finds another address
- `UsbKeyboardUnplug_BuiltInLetsGo`, `UsbKeyboardReplug_BuiltInKeepsItsInterface`: the keyboard, whose IDs and class `usb-qemu-hid` and `usb-hid-class` match, was the built-in keyboard driver's before the pass, which offered it to no registered driver; pulled out, it leaves the device list, and plugged back in it is the built-in's again, which the USB stack offers every interface before the kit, and no registered driver is probed
- `Teardown_RestoresCommandRegister`, `Teardown_InvalidatesRegionAndBuffer`: a failed or declined attempt writes the Command register back as it found it but with bus mastering off, and every attempt starts with bus mastering off, so the driver that binds finds the register as the kernel constructor did, bus mastering off, plus the INTx disable, and the region and DMA buffer it handed out throw from then on
- `Context_ProbeOnlyMembersThrowAfterProbe`, `Context_WriteConfigSparesHeader`, `Function_ReadsConfigSpace`, `Mmio_MapsWholeBar`, `Mmio_RefusesBadAccesses`: the bound edu context refuses resources, interrupts, events, work items and publications after Probe, and config writes below 0x40, reads config space and finds edu's MSI capability, and maps the whole 1 MiB BAR behind bounds and alignment checks
- `Edu_Identification`, `Edu_Liveness`, `Edu_Factorial`, `Edu_Dma`: the edu driver reads 0x010000ed from the identification register, the complement of what it wrote from the liveness register and 10! from the factorial unit, and round-trips a buffer through edu's DMA engine, whose 28-bit limit the allocator can only meet on x64: on arm64, whose RAM starts at 1 GiB, the test passes when the allocation fails and the lowest free page lies above the limit
- `Interrupts_RequestOncePerAttempt`, `Interrupts_HandlerIdleUntilBound`, `Interrupts_PolledHandlerServicesDevice`, `Interrupts_TornDownHandlerNeverRuns`: on edu both attempts are granted polled interrupts and a second request throws; the interrupt raised in Probe is still pending when Probe returns and is serviced once Bound; an interrupt raised through edu's `0x60` register reaches the handler, which acknowledges it through `0x64` and signals an event a thread then waits on; and the failed attempt's handler, never armed, never runs, and its poll timer is off the platform timer
- `Interrupts_E1000EModeFollowsGic`: on the `+gicv3` cell the 82574L gets MSI-X through the ITS, entry 0 masked during Probe and unmasked on Bound with bus mastering on; on the others it is polled, and its handler runs on every tick
- `WorkItem_ScheduledInProbeRunsAfterBound`, `WorkItem_ScheduledFromHandlerRunsOnDriverWork`, `WorkItem_DroppedWithFailedAttempt`: a work item scheduled in Probe (a second Schedule returning false while it is pending) runs once, after Bound, on the `driver-work` thread; one scheduled from the interrupt handler runs there too; the failed attempt's scheduled work item never runs, and neither it nor the one it never scheduled can be scheduled any more
- `Event_WaitThrowsInProbe`, `Event_TornDownWaitReturnsFalse`: `DeviceEvent.Wait` throws inside Probe and returns false on a failed attempt's event
- `Lock_IrqSafeLockMasksInterrupts`: while an `IrqSafeLock` is held the polled edu handler is not called, it is again once the scope ends, and the lock can be entered again
- `Publish_MouseMovesPointer`, `Publish_DroppedWithFailedAttempt`: a report the edu handler makes, in interrupt context, through the mouse edu's Probe published moves `MouseManager`'s pointer, wheel and buttons; the mouse and the network link the failed attempt published never reach their managers: the network device count is what it was when the kernel was constructed, and a report through that mouse moves nothing
- `Rtl8139_LinkRegistered`, `Rtl8139_PolledHandlerRuns`, `Rtl8139_DhcpLease`: the RTL8139 driver's link is the one registered network device and the primary one, with the chip's MAC address, the driver's name and path, and its link up; its polled handler runs; and `DhcpClient` gets a lease through it, the Discover and Request leaving through the transmit handler and the Offer and Ack coming back through the handler, the receive work item on the `driver-work` thread and the link's `Deliver`
- `Nvme_FailedAttemptRequestedInterrupts`, `Nvme_TeardownReleasedInterrupts`, `Nvme_RebindGetsInterruptsAgain`, `Nvme_CompletionInterruptAfterBound`: the failing attempt gets MSI-X where it is expected (entry 0 masked, one interrupt vector bound on x64) and polling elsewhere; after its teardown MSI-X Enable is off, the vector is free again on x64, the poll timer is off the platform timer on the polled cells, and its DMA pages are back; `nvme-identify` gets interrupts the same way again, with the entry unmasked and bus mastering on once Bound; and the Identify's completion, which arrived during Probe, reaches the handler only after Bound, from the replayed MSI-X message or the first poll, with a successful status and QEMU's vendor ID in the data
- `DeviceList_MatchesEveryFunction`, `DeviceList_ListsEveryUsbInterface`: the engine's device list holds every enumerated function once, first and in bus order, with its owner and IDs, then every interface of every configured USB device, at its `usb/` path, with its owner (once every device was plugged back in: `usb-boot-mouse` for the mouse, `usb-tablet-link` for the tablet, `HID boot keyboard` for the keyboard), its device's vendor and product ID and its class

The suite reads HAL internals, Core's interrupt vector table, and the mouse and network managers' and the network stack's internals, through temporary `InternalsVisibleTo` grants, which go away once the driver kit's public API lands. The PCI context's poll timer, a private field, is read through `[UnsafeAccessor]`.

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

### Hardware Profiles

A suite can run its whole test list against more than one machine. The hardware shapes, called profiles, and the knobs that compose with them, called modifiers, are declared once in `tests/profiles.json`, whose header comment is the reference. A suite opts in with `<CosmosTestProfile Include="..." />` and `<CosmosTestModifier Include="..." />` items in its csproj. The engine runs one cell per profile and per conflict-free combination of the modifiers that apply to it, and prefixes each test name with `[profile]` or `[profile+modifier]`. A suite that opts into nothing runs once, on the architecture defaults.

A profile describes its hardware with these keys:

| Key | Value | QEMU effect |
|-----|-------|-------------|
| `disks` | `[{ "type": "ahci" \| "nvme" \| "usb", "options": { ... } }]` | A fresh 256 MiB image per disk. AHCI disks share one `ich9-ahci`, each NVMe disk gets its own controller, USB sticks share one `qemu-xhci`. |
| `nic` | A NIC model, or `none` | Replaces QEMU's default NIC with a user-mode one. |
| `keyboard`, `mouse` | A model, or `ps2`/`none` for nothing | A bare `-device` line. |
| `vga` | A `-vga` backend | Replaces the default display adapter. |
| `gpu` | A model | Adds a display adapter beside the default one. |
| `devices` | A list of models: `edu`, `rtl8139`, `e1000e` | One `-device` line each. A NIC model gets a user-mode netdev of its own (`devnet0`, `devnet1`, ...), and like `nic` that removes QEMU's default NIC. |
| `usb` | A list of models: `usb-mouse`, `usb-kbd`, `usb-tablet` | One `qemu-xhci` controller (`usbxhci0`) with each device on its root hub, entry n as `usbdev<n>`. A profile that also has a USB disk puts both on that same controller. |
| `machineOptions` | `-M` properties keyed by architecture | For example `{ "arm64": { "gic-version": "3" } }`. |
| `architectures` | `["x64"]`, `["arm64"]` or both | Pins the profile to the architectures that can present its hardware. |

Use `usb` for USB devices. `"mouse": "usb-mouse"` emits a bare `-device usb-mouse`, and neither q35 nor virt has a USB bus of its own for it, so QEMU refuses to start unless a USB disk happens to bring a controller.

`devices` and `usb` take plain model names, checked against the list in `tests/Cosmos.TestRunner.Engine/ProfileDeviceModels.cs`. The catalog load fails with a message naming the profile when a model is unknown, listed twice, already attached by one of the profile's `nic`, `keyboard`, `mouse` or `gpu` keys, or when a list is empty or has a blank entry. `ProfileCatalogTests` loads the real catalog for every suite on both architectures, so such a mistake fails the host test job before any kernel is built. A model is added to that list once QEMU attaches it on both q35 and virt, since the loader has no per-architecture check of its own.

```jsonc
{
  // Example only: QEMU's edu test device and a USB mouse and keyboard.
  "name": "edu-and-hid",
  "devices": ["edu"],
  "usb": ["usb-mouse", "usb-kbd"]
}
```

A modifier overlays `machineOptions` (for example `gic-version`) or per-disk-kind `deviceOptions` (for example NVMe `msix=off`) onto every profile it applies to. Its own `architectures` filter skips it on the other architecture.

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

A test calls `TR.RequestHost(request)` when it needs the machine changed under it, and the engine carries the request out through QEMU's QMP monitor as soon as the frame shows up on the UART (`QemuHotPlug`, which `HostRequest` parses for, both in `tests/Cosmos.TestRunner.Engine/Hosts/`). The framework has a helper for each request on the profile's `usb` devices:

| Request | Helper | Effect |
|---------|--------|--------|
| `usb-unplug [n]` | | Pulls USB stick `n` (0 by default) off the xHCI controller |
| `usb-plug [n]` | | Plugs it back in, on the same disk image |
| `usb-device-unplug n` | `TR.RequestUsbDeviceUnplug(n)` | Pulls entry `n` of the profile's `usb` list off the xHCI controller |
| `usb-device-plug n` | `TR.RequestUsbDevicePlug(n)` | Plugs a new device of the same model back in, on the same controller |
| `usb-pointer-move n dx dy [buttons]` | `TR.RequestUsbPointerMove(n, dx, dy, buttons)` | Moves entry `n`, a `usb-mouse`, by `dx` right and `dy` down, then holds `buttons` (1 left, 2 right, 4 middle; none by default) |

Numbers are decimal and `n` counts from 0. A request that is malformed, names a stick or device the profile lacks, unplugs what is already out, plugs what is already in, or moves anything but a plugged-in `usb-mouse` is logged (`[HotPlug] ... failed: ...`) and dropped.

A device comes back under a new QEMU id (`usbdev0p1`, `usbstick0p2`, ...), since QEMU may not have released the old one yet; later requests use the new id. A pointer request makes the mouse QEMU's current one (`mouse_set`) and then sends the movement and each button that changes as one `input-send-event`, which QEMU folds into the mouse's next report, so a click takes two requests. `input-send-event` is not given the mouse's id: its `device` argument names a display, and QEMU 10.2.2 aborts when it names a USB device instead (`Property 'qemu-fixed-text-console.device' not found`).

Nothing replies. The test waits for the change itself, and must see it within 10 s: that long without a protocol message and the engine takes the kernel for hung. Only a profile that attaches a USB disk or a `usb` device launches QEMU with a monitor; the engine logs and drops a request from any other. The Storage suite's `UsbHotPlug_*` tests are the example for a stick, and the Drivers suite's `UsbHotPlug_*`, `UsbUnplug_*`, `UsbReplug_*`, `UsbTablet*` and `UsbKeyboard*` tests for `usb` devices and pointer movement.

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
