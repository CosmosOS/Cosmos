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
- **Cosmos.Tests.SourceGenerators**: Runs `CosmosEntryPointGenerator` on in-memory compilations and checks the generated entry point, the driver manifest and the `COSMOSGEN` diagnostics exactly; `FeatureParityTests` keeps `DriverFeature`, `KernelFeatures` and the test stubs in step, and `ReferencedDriverTests` covers drivers from referenced assemblies, public and under an `InternalsVisibleTo` grant.
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
| **Drivers** | 34 | Driver kit over the synthetic bus: manifest, arbitration, publish, interrupts, deferred work, teardown, children, a display reaching the display manager, a block device reaching the storage manager; on x64, the PCI host and the E1000E driver on q35's default NIC |
| **Pci** | 9 | The legacy PCI manager's configuration space reads, and the driver kit's PCI host node and its children through `DriverInfo` and the nodes' `PciAccess` |
| **Virtio** | 12 | The virtio drivers over the driver kit on both transports: the net and input nodes bound, the interface consumed with its link, MAC and interrupt, and the PCI function or the MMIO slot bound by its transport driver |
| **Graphic** | 24 | The 2D canvas, fonts and images on every cell; the display manager and the primary display per cell; the virtio-gpu display driver on the virtio-gpu cells; the VMware SVGA II adapter through its facets and its SVGA3D FIFO wire tests on vmware-svga |
| **Storage** | 78 per cell | The storage manager and the block devices behind it on three profiles, `ahci`, `nvme` and `usb`, each combined with the `acpi-off` modifier on x64 (6 cells) and with `gicv2`, `gicv3` and `acpi-off` on arm64 (18 cells): the disk the kit's `AhciDriver` or `NvmeDriver` published, or the USB stack registered, under the same block I/O, partition table, partition lifecycle and reboot assertions; the kit's view of the disk in `DriverInfo`; the NVMe interrupt mode per cell; USB hot-plug on the usb cells |

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

The suite is two projects. `tests/Kernels/Cosmos.Kernel.Tests.Drivers` is the kernel: the harness (`Kernel.cs` and `TestKeyboardConsumer`) with an `InternalsVisibleTo` grant from `Cosmos.Kernel.HAL`, which it spends on its consumer and, in the hardware group, on reaching the shipped E1000E's state through `DriverEngine.Nodes`: `TestKeyboardConsumer` derives from the internal `KeyboardConsumer` and is installed through the internal `DeviceRegistry.SetConsumer` in `BeforeRun`, replacing the ring's own keyboard consumer for the run, which is the one-consumer-per-kind rule at work. `tests/Kernels/Cosmos.Kernel.Tests.Drivers.Library` holds every `[Driver]` class the suite drives and the state and identity types they need: a driver assembly (`<CosmosDriverAssembly>true</CosmosDriverAssembly>`, listed in `CosmosDriverAssemblyNames`) with no grant from any project, written over the public seam only, so its compiling is the proof that a third party can write each of those drivers. The kernel references the library, drives the library's drivers through the synthetic bus with no hardware behind any node and the shipped drivers on what the machine carries (the Hardware group below), and waits for the kit through `SyntheticBus.WaitForQueuedJobs`. It builds with `CosmosEnableMouse` off and with one `CosmosDriverExclude` and one `CosmosDriverInclude` item naming the library's types, so the manifest policy is under test too. Every assertion reads `DriverInfo` or the suite's own drivers and consumer, never the serial log.

Manifest order for the library's drivers follows the referenced-assembly rule: the generator sorts them by assembly name and then by full type name, both ordinal, after the kernel's own drivers. `Manifest_Order_ReferencedByTypeName` asserts that `TieFirstDriver` precedes `TieSecondDriver` on that rule alone (`F` sorts before `S`); where the two are declared plays no part. `Interrupt_WorkItemDeferredToWorker` proves deferral without asking the engine where it ran: the handler notes the work item's run count as it returns, with interrupts still disabled, so an unchanged count means the item did not run inside `RaiseInterrupt`, and the run seen after `WaitForQueuedJobs` is the worker's.

**Manifest (6 tests):**
- `Manifest_HighPriorityDriver_Present`, `Manifest_MouseFeatureDriver_Absent`, `Manifest_ExcludedDriver_Absent`
- `Manifest_OptInDriver_Present`, `Manifest_OptOutDriver_Absent`, `Manifest_Order_ReferencedByTypeName`

**Engine (2 tests):**
- `Engine_Started_WithWorker`, `BootPath_NodeFromConstructor_Bound`

**Arbitration (5 tests):**
- `Arbitration_ByPriority`, `Arbitration_BySpecificity`, `Arbitration_TieByManifestOrder`
- `Decline_UnwindsResources`, `ThrowingProbe_RecordedAsFailed`

**Keyboard device (7 tests):**
- `Publish_ReachesConsumer`, `WindowAndDma_Contents`
- `Interrupt_HandlerReadsWindow_ReportsKey`, `Interrupt_WorkItemDeferredToWorker`, `Interrupt_MaskUnmask`
- `Periodic_FiresAtLeastThreeTimes`, `BlockingHandler_FaultRecorded`

**Display (1 test):**
- `Publish_Display_ReachesDisplayManager`: the library's `DisplayDriver` publishes a `DisplayState`, a display in one fixed 64x32 mode with no framebuffer; `DisplayManager.Count` grows by one, the display is found by its node path in `DriverInfo` with `IsConsumed`, reports the driver's mode, is the primary (a driver display beats the firmware one) and yields the state as a facet but no `IDisplayModes`; after the retraction it has left the manager and the published list, has no facet, and the primary is the firmware display again, or none

**Block device (1 test):**
- `Publish_Block_ReachesStorageManager`: the library's `BlockDriver` publishes a `BlockState`, a blank 64-block disk named `synthetic-block` over a byte array; `StorageManager.DeviceCount` grows by one, `Devices` holds the state, `GetPartitions` finds nothing on it, it is the primary when nothing else was registered, and `DriverInfo` lists it under its node as a consumed, not withdrawn block device named by the driver; after the retraction the count, the list and the primary are what they were, the published entry is gone, and the driver recorded one probe and one detach. The UART carries `[StorageManager] synthetic-block registered by BlockDriver (primary)` and `[StorageManager] synthetic-block unregistered (no primary)`, read from the log, not asserted

**Retract (3 tests):**
- `Retract_DetachOrderAndAccounting`, `Retract_DriverThreadExited`, `Retract_SinkReportDiscarded`

**Children (3 tests):**
- `Children_PublishedFromProbe`, `UnmatchedNode_UnboundWithNoOffers`, `Children_RetractedWithParent`

**Diagnostics (1 test):**
- `DriverInfo_OutOfRange_ReturnsFalse`

**Hardware (5 tests):**
- `Hardware_PciHost_Bound`, `Hardware_E1000E_NodeBound`, `Hardware_E1000E_DeviceConsumed`
- `Hardware_E1000E_LinkUp`, `Hardware_E1000E_Transmit`

The hardware group runs the shipped drivers on real (emulated) hardware without a profile of its own: QEMU's q35 adds a default e1000e whenever a cell passes no `-netdev`, so the suite's bare x64 cell already carries the controller, while virt's default NIC is a virtio-net-pci function that the kit's `VirtioNetDriver` binds, so the four E1000E tests skip on arm64 with `no e1000e on this machine`. Their gate is a `DriverInfo` node with `BusName` `pci` whose `Description` starts with `8086:10d3`. `Hardware_PciHost_Bound` is unconditional: a `platform` node whose description contains `pci-host-` is `Bound` by `PciHostDriver`. `Hardware_E1000E_NodeBound` reads that the function is `Bound` by `E1000EDriver` with one published device and at least seven held resources (the register window, the four DMA buffers, the drain work item and its periodic registration, plus the interrupt handle when the line connected); `Hardware_E1000E_DeviceConsumed` finds a published device of kind `Network` at that node path with `IsConsumed` true and a non-zero `NetworkManager.MacAddress`; `Hardware_E1000E_LinkUp` polls `NetworkManager.LinkUp` for up to 2 s; and `Hardware_E1000E_Transmit` spends the kernel's HAL grant on reaching the node's `E1000EState` through `DriverEngine.Nodes`, sends a 60-byte broadcast frame through `NetworkManager.Send` and asserts `FramesTransmitted` grew.

#### Pci Tests

`tests/Kernels/Cosmos.Kernel.Tests.Pci` keeps its six configuration space tests over the legacy PCI manager and adds three that read the driver kit, on both architectures (arm64's EDK2 boot carries ACPI, so MCFG is present and the ECAM host node exists there): `Host_PlatformNode_BoundByPciHostDriver` (a `platform` node whose description contains `pci-host-` is `Bound` with `DriverName` `PciHostDriver`), `Host_PublishesPciNodes` (at least one `pci` node has the host's path as `ParentPath`, and every such node has `ResourceCount` 6 and an `InterruptCount` of its legacy line plus one source per described message: the node is resolved in `DriverEngine.Nodes` through the kernel's HAL grant, its `PciAccess` read, and the count checked against `1 + Math.Min(pci.MessageInterruptCount, PciHostAccess.MaxDescribedMessages)`, 32 at most, when `pci.IsMsiXCapable` and against 1 otherwise; the first source's `Describe()` starts with `line`, and on an MSI-X capable function the second's equals `message 0 of ` followed by the table size, compared ordinally) and `Host_NodeCount_MatchesLegacyScan` (the `pci` nodes number at least `PciManager.Count`). Both default cells carry an MSI-X capable function (q35's e1000e, virt's virtio-net-pci) and functions without the capability (the host bridge), so the per-node formula covers every mix; the project suppresses `COSMOS0003`, since `DeviceNode` and `PciAccess` are experimental.

#### Virtio Tests

`tests/Kernels/Cosmos.Kernel.Tests.Virtio` proves the virtio drivers in `Cosmos.Kernel.Drivers` over both transports with the same assertions. Its two profiles attach a virtio NIC, keyboard and mouse to every cell: `virtio-pci` on x64 and arm64 (the arm64 cell launches with `gic-version=3`, since the ITS is what routes the function's MSI-X messages), and `virtio-mmio` on arm64 alone, through the virt machine's virtio-mmio window (q35 has none). Which transport a cell presents is a property of the profile, not of the architecture, so the same kernel serves both arm64 cells and detects the transport at run time: `BeforeRun` captures the virtio-net node (a `DriverInfo` node with `BusName` `virtio` whose `Description` starts with `type 1 `), the virtio-input nodes (`type 18 `), and the PCI function node (`BusName` `pci`, `Description` starting with `1af4:1041` or `1af4:1000`), whose presence makes the cell a PCI cell. The kernel keeps a HAL grant for one purpose, reaching a node's binding state through `DriverEngine.Nodes` as the Drivers suite does, and suppresses `COSMOS0003`.

The twelve tests: `Net_DriverBound` (the net node is `Bound` by `VirtioNetDriver`), `Net_TransportMatchesCell` (its `Path` starts with `virtio:pci:` on the PCI cell and `virtio:mmio:` otherwise), `Net_DeviceReady` (a published `Network` device at the node's path with `IsConsumed` true, `NetworkManager.DeviceCount` at least 1 and `NetworkManager.Ready`), `Net_LinkUp` (`NetworkManager.LinkUp`: QEMU's user backend reports the link up at once), `Net_MacAddressProgrammed` (`NetworkManager.MacAddress` not null and not all zero), `Net_InterruptConnected` (the node's `VirtioNetState.HasInterrupt` is true and `IsPolling` false: every cell routes one source, MSI-X over PCI, the GIC line over MMIO), `Input_KeyboardBound` and `Input_MouseBound` (a `type 18` node `Bound` by `VirtioInputDriver` whose published `Keyboard`, respectively `Pointer`, device is consumed), then two per transport. On the PCI cell, `Pci_FunctionBoundByTransport` (the function node is `Bound` by `VirtioPciTransportDriver` with `ChildCount` 1) and `Pci_Version1Negotiated` (`VirtioNetState.Version1Negotiated`; QEMU's virtio-mmio is legacy by default, so the MMIO cell does not assert it), skipped elsewhere with `this cell presents virtio over MMIO`. On the MMIO cell, `Mmio_SlotBoundByTransport` (a `platform` node whose `Description` contains `virtio,mmio` is `Bound` by `VirtioMmioTransportDriver` with `ChildCount` 1, the assertion that catches a lost MMIO window) and `Mmio_AnyLayoutNegotiated` (`VirtioNetState.AnyLayoutNegotiated` on the legacy device, whose `any_layout` property QEMU defaults on), skipped elsewhere with `this cell presents virtio over PCI`. The Network suite's `virtio-net-pci` and `virtio-net-mmio` cells exercise the same driver's data path with no test change.

#### Graphic Tests

`tests/Kernels/Cosmos.Kernel.Tests.Graphic` runs on three profiles, `bare`, `vmware-svga` (x64 only, the adapter is programmed through port I/O) and `virtio-gpu` (a virtio-gpu-pci added beside the machine's default adapter, on both architectures), and registers the same 24 tests on every cell: a cell-specific test runs where its device is and skips elsewhere, `no virtio-gpu device on this cell, needs the virtio-gpu profile` or `VMware SVGA II adapter not present, needs the vmware-svga profile`. The cell is read in `BeforeRun` from the device tree, never from the profile name or from which driver bound, so a display driver that failed to bind fails the suite instead of skipping it: a `pci` node whose `Description` starts with `15ad:0405` marks the vmware-svga cell, a `pci` node starting with `1af4:1050` or a `virtio` node starting with `type 16 ` marks the virtio-gpu cell, and neither marks bare. The suite reaches the driver states in `Cosmos.Kernel.Drivers` through the facets of `DisplayManager.Primary` and holds no grant; the project suppresses `COSMOS0003` for the facets.

The 2D tests (8), every cell: `PCScreenFont_ChangeFont`, `Bitmap_Basic`, `Png_Decode`, `Ttf_Render`, `ColorClass_Basic`, `Canvas_Basic`, `VirtualCanvas_Basic`, `Canvas_CopyPixels_Overlap`. The display tests (4), every cell: `Display_PrimaryPresent` (a primary exists, the console's canvas has its width and a `Name` starting with its `DriverName`), `Display_PrimaryMatchesCell` (bare: the primary is the firmware display and it is the only one; virtio-gpu: `VirtioGpuDriver`'s display is primary beside the firmware one; vmware-svga: `VmwareSvgaDriver`'s display is the only one and `DriverInfo` lists no display without a node path, the firmware display having been retired), `Display_ListedInDriverInfo` (every display of the manager is a consumed `Display` device in `DriverInfo`, with a null `NodePath` exactly for the firmware one) and `Display_ModeRequest_FollowsFacet` (`Canvas.GetFullScreen(new Mode(640, 480, ColorDepth.ColorDepth32))` reports 640x480 when the primary offers `IDisplayModes` and the real size otherwise; it runs after the facet tests, with only `Canvas3D_Discovery` after it, since it switches the mode they measure). `VirtioGpu_DriverState` (1), on the virtio-gpu cells: the primary yields a `VirtioGpuState` that is not faulted, has an interrupt or polls, counts at least one scanout, matches the primary's size and sent the probe's four commands; one `Display()` of the console's canvas grows `FlushCount` by one and `CommandsSent` by two. The SVGA tests (9), on vmware-svga: `Svga_AdapterFacet` (`ISvgaAdapter` with capabilities, `FifoMin` below `FifoMax`, no 3D negotiated on QEMU, no `ICanvas3DFactory`, the scanout enabled since the console programmed a mode), `Svga_DisplayModesFacet` (`IDisplayModes` listing 1024x768x32, the default the console chose), `Svga_HardwareCursorFacet` (`IHardwareCursor`; `TryDefine` returns false on QEMU, which has no alpha cursor; `Set(1, 1, false)` does not throw), then the FIFO wire tests, each of which captures `NextCommand`, reads the commands its calls wrote back dword by dword, and rewinds: `Canvas3D_SceneSetup_Fifo` opens the block by re-programming the display's own mode (so the FIFO is back at its start), turning the scanout off through the facet (so the host consumes nothing) and creating the SVGA3D canvas through `CreateCanvas3D`, which the later tests share, and pins the scene setup (context, colour and depth targets sized to the canvas, the two render target binds, viewport, depth range, seven render states, the untextured stage); `Canvas3D_MeshUpload_Fifo` (per stream a `SURFACE_DEFINE` of a buffer and a `SURFACE_DMA` from the framebuffer region, surfaces 3, 4 and 5 for the cube), `Canvas3D_MeshValidation`, `Canvas3D_DrawCube_Fifo`, `Canvas3D_CameraCaching_Fifo` and `Canvas3D_DisposedTexture_Rejected` (`CreateTexture` defines and uploads an A8R8G8B8 surface, `Dispose` destroys it, and a mesh mapping it is rejected without a write) follow, and the last one restores the scanout. `Camera3D_Defaults` and `Canvas3D_Discovery` (2), every cell: `Canvas.GetFullScreen()` is not a `Canvas3D` on any CI cell. Per cell on x64: bare 14 passed and 10 skipped, vmware-svga 23 passed and 1 skipped, virtio-gpu 15 passed and 9 skipped; arm64 runs bare and virtio-gpu. The UART log holds every cell's boot in order, with the `[Display] primary` line of each.

#### Storage Tests

`tests/Kernels/Cosmos.Kernel.Tests.Storage` runs on the `ahci`, `nvme` and `usb` profiles, each attaching one disk, combined with the `acpi-off` modifier on x64 (6 cells) and with `gicv2`, `gicv3` and `acpi-off` on arm64 (18 cells), and registers the same 78 tests on every cell: 3 manager, 1 boot scan, 2 profile, 1 driver info, 12 device, 7 partition, 42 partition lifecycle (MBR mutation, the EBR chain, `PartitionManager`, the superfloppy), 2 bounds probes, 2 MMIO/PCI, 5 USB hot-plug and 1 reboot. `BeforeRun` takes `StorageManager.GetDevice(0)` as the cell's disk, and every test that needs one skips where none bound, with `no block device bound for this profile` (`no block device bound for partition-table tests` in the two partition groups), which on x64 is a failure of `Manager_ExactlyOneDevice` (PCI enumerates with or without ACPI there) and legitimate only on the arm64 acpi-off cells, which have no MCFG and so no PCI host. The ahci and nvme cells' disks are published by the kit's `AhciDriver` and `NvmeDriver` and registered by the storage manager's consumer during the driver stage; the usb cell's is registered by the HAL's USB stack. `Profile_DeviceKindMatches` checks the disk's name against the profile (`sata0`, `nvme0n1`, `usb0`); `Manager_DeviceListedInDriverInfo` finds it in `DriverInfo` as a consumed, not withdrawn block device with the `DriverName` the cell expects, and skips on usb with `the USB disk is registered by the HAL, not published through the kit`; `Profile_NvmeInterruptModeMatches` reads `HasInterrupt` off the `NvmeNamespace`'s `Controller` and pins it where the cell determines it (interrupt on plain x64 nvme and on arm64 gicv3, polling on gicv2), skipping elsewhere (`not an NVMe profile`, `acpi-off has no MSI routing to pin`, `interrupt mode not pinned by this cell`). The device and partition groups drive the disk through `IBlockDevice` and the ring's partition tables; `Boot_PartitionScanMatchesBootState` proves the scan the manager ran at boot, before `BeforeRun` (inside the publish on the ahci and nvme cells, in the System initializer's USB registration on the usb cells); the two MMIO/PCI probes relocate the NVMe controller's BAR above 4 GiB on x64 while no command is in flight; the hot-plug tests pull the stick out and back in through the engine's host requests; and `Boot_RebootAfterGptWrite`, last, stamps a GPT and reboots, so each cell with a disk boots twice and the second boot's scan finds the partition. On x64 the six cells report 433 passed, 35 skipped and 0 failed out of 468; on arm64 the eighteen report 671 passed, 733 skipped and 0 failed out of 1404. The Fat and File suites drive an in-memory block device and never name the manager, so the move of the storage drivers into the kit left them unchanged.

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
| Drivers | 60 s | 120 s |
| Pci | 60 s | 90 s |
| Graphic | 60 s | 90 s |
| Storage | 90 s | 180 s |

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
├── Cosmos.Tests.SourceGenerators/   # Unit tests: entry point and driver manifest generator
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

> `Assert` uses static failure state (no exceptions) for NativeAOT compatibility.
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
