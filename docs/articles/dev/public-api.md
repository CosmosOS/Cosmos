# Public API Tracking

The public surface of the kernel packages is declared in text files checked into the repository, enforced by Roslyn analyzers at build time, guarded against breaking changes at release time, and published as one frozen documentation site per release. This page states the policy that decides what is public, then the three mechanisms and the contributor workflow they impose.

---

## The policy

Three rules decide what is public:

1. **One supported ring.** `Cosmos.Kernel.System` is the API kernels program against: tracked, documented, frozen at release, with deprecation cycles applying there and only there. A contract type from another assembly rides along as public only when a kernel can obtain one or meaningfully supply one through the ring. `IBlockDevice` in `Cosmos.Kernel.HAL.Interfaces` is the one device contract that qualifies, and it qualifies on both halves: `StorageManager.PrimaryDevice`, `Devices` and `GetDevice` hand a kernel a device, DevKernel reads and writes blocks straight through it, 31 public members take one as a parameter, and the Fat and File suites each implement one of their own and drive it, holding no grant to `Cosmos.Kernel.HAL.Interfaces` (the Fat suite's grant is to the ring, which is a different question). `MACAddress` follows it because the packet seam and `NetworkManager.MacAddress` both hand one out. The VFS contracts in `Cosmos.Kernel.HAL.Vfs` qualify through `VfsManager.RegisterFilesystem` and the types the handle API returns. Everything else in those assemblies is internal, including contracts that a `Register` method happens to name: the input, timer and network devices are registered only by this assembly's own `LibraryInitializer` wiring up HAL enumeration, and a kernel reads `KeyboardManager.ReadKey`, `MouseManager.X` and `NetworkManager.LinkUp`, never the device. Nor do the platform contracts (`IPortIO`, `ICpuOps`, `IPowerOps`, `IInterruptController`, `IRQContext`), the boot contract `IPlatformInitializer` or `IGraphicDevice`, since `PlatformHAL` is internal and no kernel can register or obtain one.

   Beware the weaker version of this test, "a ring signature names it". It passes anything somebody once wrote a `Register` overload for and fails an identical contract nobody did, which is how `IKeyboardDevice` and `IGraphicDevice` ended up on opposite sides of the same question.

   `INetworkDevice` shows how far that weaker test can carry a type. Thirteen ring members took one, which reads as decisive until you count what a kernel does with the handle: it reads `Name`, `MacAddress`, `LinkUp` and `Ready`, and null-checks it. Across DevKernel and the network suite nothing ever called `Enable`, `Disable`, `Initialize` or `Send` on it, nothing asked for `GetDevice(1)`, and `RegisterDevice` had exactly one caller, this assembly's own `LibraryInitializer`. `NetworkManager` was already fronting `LinkUp` and `Send` off the primary device, so the facade was half built and callers were reaching past it. Adding the four missing facts finished it and the contract went internal, which is what `KeyboardManager` and `MouseManager` had done all along.

   Hiding a contract must not hide a capability with it. `INetworkDevice` was the only way to name a device, so the ring grew `NetworkAdapter`, a handle carrying the registration index, with `NetworkManager.GetAdapter`, a settable `NetworkManager.Primary`, an adapter overload of `IPConfig.Enable`, and `NetworkAdapter.IPConfig` for the configuration in force on one. The ring owns the handle, a kernel genuinely obtains one, and the members a kernel never called stay behind it.

   The test then runs again member by member, because a type earning its place does not carry its whole class with it. `SoftwareTimer` is public: `TimerManager.Schedule` returns one and `Cancel` takes it back. Its constructor, `SetActive`, `Tick` and `Invoke` are not, because every call site is the timer device that owns the registry, and a kernel calling them advances another timer's countdown, runs an interrupt-context callback from thread context, or deactivates an entry without unregistering it. What is left public, `TimeoutNs`, `Recurring` and `IsActive`, is what a caller can read off a handle without breaking the thing behind it. `SoftwareTimer` keeps its declaration in `Cosmos.Kernel.HAL.Interfaces` for a layering reason rather than a policy one: `TimerManager` constructs it, but `TimerDevice` holds the registry and ticks it from the interrupt path, and `HAL.Interfaces` cannot reference the ring.

   A corollary of that same rule: the ring must not document a capability it does not offer. `TimerManager.Schedule` and `ScheduleRecurring` both told kernels to use `AlarmSystem` for callbacks that need thread context, and `AlarmSystem` is internal to `Cosmos.Kernel.Core`. So the ring handed out the interrupt-context half, where a callback must not block, allocate or take a lock, and pointed at the safe half through a wall. `AlarmManager` is the ring route to it, forwarding to the internal implementation; it is a separate manager rather than three more methods on `TimerManager` because calling the wrong one deadlocks, and the manager name is what makes the context visible at the call site. Two managers that a kernel picks between by execution context must not also differ in shape, so `Schedule` and `ScheduleRecurring` take `(Action callback, TimeSpan delay)` on both and `Cancel` returns `true` on both only when the callback was still pending. Converging the shape makes the one thing that cannot converge worth stating on the ring: an alarm's own scheduling calls park on a mutex, so they may be made from thread context only, while the timer's mask interrupts and may be made from anywhere. What stays different is what each one hands back: `AlarmSystem` owns its alarms and is internal to `Cosmos.Kernel.Core`, so the ring can only return a `ulong` id for one, while the other returns the `SoftwareTimer` the device registry already holds.

   The same rerun deleted `NetworkConfigManager`. It read as the manager for `IPConfig`, but it held only half of it: configuring one device wrote the same config to `IPConfig`'s routing list, to the manager's device-keyed list and to its current pointer, three stores written together and cleared apart, with the two `Remove` methods that would desynchronize them both dead. The device-keyed lookup moved onto `NetworkAdapter.IPConfig`, where the ring already keeps every other per-device fact, and the routing list became the only store. `DnsConfig` keeps its own list rather than gaining a manager to match, because a nameserver list is global to the resolver while an IPv4 config belongs to one device.
   A member that cannot answer anything but one value is not a member. `Address.IsIpv4` read `Parts.Length == 4` on a type whose every constructor produces exactly four bytes, so it was a constant `true`, `IsIpv6` a constant `false`, and `CompareTo`'s "not the same family" throw tested the constant against itself. `MACAddress.IsValid` went the same way for the same reason. Delete these rather than rename them: a rename keeps the ring slot and proves nothing, and the same rerun deleted `Address.IsLoopbackAddress`, `IsAPIPA` and `CIDRToAddress` because nothing called them, while `IsBroadcastAddress`, `ToUInt32` and `ToSpan` went internal because something did. The ring must also not offer a third spelling of a question it already answers twice: `NetworkManager.HasDevice` was `DeviceCount > 0` and `Primary.IsValid` under a third name.

   Acronym casing follows the .NET rule, which is worth stating because it points one way only: an acronym of two letters stays capitalised (`IPPacket`, `SourceIP`), one of three or more is Pascal-cased (`IcmpType`, `UdpDataLength`, `MacAddress`). So `NetworkManager.MacAddress` was already right and `EthernetPacket.DestinationMAC` was not. One rename in that sweep does not compile: `TcpPacket.TCPFlags` cannot become `TcpFlags`, because the enum `TcpFlags` lives in the same namespace and the class body evaluates `(byte)TcpFlags.SYN`, which a `byte`-typed property of that name shadows (CS1061). It is `FlagBits`. Wire-format field names that mirror a header keep their spelling, which is why `TcpFlags.ACK` and `DnsPacket.AnswerRRs` stay as they are.

2. **Chosen experimental seams.** Extension points are opened deliberately and marked `[Experimental]`: public and usable now, no compatibility promise, promoted to stable by removing the attribute once proven. Referencing one is a build error until the consuming project suppresses its diagnostic ID, which is the consumer's acknowledgement of that contract. The scheduler policy seam in Core is the first, the packet seam in System is the second; a driver kit in the HAL is the planned third.
3. **Everything else is internal.** Visibility is never the extension mechanism; seams are. First-party assemblies and the white-box test kernels reach internals through `InternalsVisibleTo`; anyone else goes through `[UnsafeAccessor]`/`[UnsafeAccessorType]` ([Accessing internals](accessing-internals.md)) and accepts that internals change without notice.

The enforcement test is mechanical: `examples/DevKernel` must compile with no `InternalsVisibleTo` grant. If DevKernel needs a symbol, the symbol becomes public or gets a `Cosmos.Kernel.System` facade; if it does not, the symbol stays internal.

| Assembly | Surface |
|----------|---------|
| `Cosmos.Kernel.System` | The supported ring |
| `Cosmos.Kernel.HAL.Interfaces` | `IBlockDevice`, `MACAddress` and `SoftwareTimer` as a read-only handle, tracked; the boot, graphics, input, timer and network contracts internal |
| `Cosmos.Kernel.HAL` | VFS contracts only, tracked; drivers, PCI, ports internal |
| `Cosmos.Kernel.Core` | The scheduler seam (`[Experimental]`) and nothing else, tracked |
| Arch assemblies, Native, Plugs, Debug, Boot.Limine, `Cosmos.Kernel` | Internal, `InternalsVisibleTo` for first-party |

The last row is policy rather than tracked enforcement: those assemblies are untracked, and their remaining public types shrink as they are touched.

Experimental seams carry diagnostic IDs:

| ID | Seam |
|----|------|
| `COSMOS0001` | The scheduler policy seam: `IScheduler`, `SchedulerManager`, `SchedulerThread`, `PerCpuState`, `SchedulerExtensible`, `InterruptMaskScope`, `SchedulerThreadState`, `SchedulerThreadFlags` ([Scheduler - Writing a Scheduler](scheduler-plugging.md)) |
| `COSMOS0002` | The packet seam: the protocol packet types (`EthernetPacket`, ARP, `IPPacket`, ICMP, `UdpPacket`, DHCP, DNS, `TcpPacket`), `NetworkStack.Send`/`HandlePacket`, and the client members that take or return packets ([Network - Crafting packets](../user/network.md#crafting-packets)) |

---

## Behaviour when a feature is compiled out

Every feature switch (`CosmosEnableStorage`, `CosmosEnableMouse` and the rest) can leave a ring manager with no subsystem behind it. One rule decides what its members do then:

| Kind of member | Behaviour with the feature off |
|----------------|--------------------------------|
| A read that can express "nothing here" | Returns the honest answer: `0`, `null`, `false`, an empty list, an invalid handle |
| A `Try` method, or any member whose `bool` already means "it did not happen" | Returns `false` |
| Anything else | Throws `InvalidOperationException` naming the switch to set |

The reads answer so a kernel can branch on them, and `KernelFeatures` answers the compile-time question directly. The actions throw because the alternative is a silent no-op, which reads as a bug in the kernel rather than a switch left off in its `.csproj`. The middle row is keyed on shape rather than on the `Try` prefix: `TimerManager.Cancel` and `AlarmManager.Cancel` already return `false` for a callback that was not pending, a compiled-out subsystem is one more way it was not pending, and a cleanup path should not have to check the switch before it can run.

Two members cannot follow the read half and say so in their XML docs: `KeyboardManager.Peek` and `ReadKey` return a non-nullable `KeyEvent`, so they have no value for "no key", and `ReadKey` would otherwise block on an interrupt no keyboard will raise.

---

## How a member reports failure

The compiled-out table is one case of a wider rule. A ring member has exactly one channel for saying it did not work, and which channel it is follows from the member's shape.

| The member is | It says so with |
|---------------|-----------------|
| A read that has an empty value of its own | that value: `0`, `null`, `false`, an empty list, an invalid handle |
| An operation that fails for reasons a caller can act on | a `bool`, spelled `Try` when it also has an `out` |
| An operation whose failures a caller must tell apart | an outcome enum, as `SchedulerInfo.RequestKill` returns `ThreadKillResult` |
| Anything reached in a state a correct kernel cannot produce | an exception, documented with `<exception>` and never a bare `Exception` |

Four rules make that table usable.

**A `Try` does not throw for the failure its own bool carries.** `IVfsFileHandle` splits on exactly this: `TrySeek`, `TryFlush`, `TrySetAttr` and `TryStat` answer `false` for a handle that has been disposed, while `Read` and `Write` throw, because a byte count has no value that means "the handle is gone" rather than "no bytes moved". A `Try` that reaches a `Dictionary` lookup, an indexer or a `Dequeue` on the caller's behalf owns the argument and emptiness checks that would otherwise throw through it: `VfsManager.TryMount` guards the driver name the way `RegisterFilesystem` does, and `KeyboardManager.TryReadKey` takes the key with `TryDequeue`, one call whose own bool is the answer, rather than a `Count` test followed by a `Dequeue` that throws when the test was wrong.

**The shape decides, not the prefix.** The middle row of the compiled-out table already says this, and it applies here the same way. `IInodeOperations` spells the driver side of the VFS as plain verbs, `Lookup`, `Create`, `Mkdir`, `Rename`, next to an `IVfsDirectoryHandle` that spells the consumer side as `TryLookup`, `TryCreateFile`, `TryCreateDirectory`, `TryRename`. Both are `Try` members for every rule on this page. The naming split is worth keeping, because it marks which side of the VFS a reader is on, but it buys no exemption.

**Every nullable-reference `out` of a `Try` carries `[NotNullWhen(true)]`, and only where every true path assigns.** Without it each call site pays a `!` or a redundant `x != null`, and the redundancy is invisible: the compiler cannot tell a clause that guards a real hole from one that guards nothing. The attribute is checked at the declaring method's return points, but flow analysis is not interprocedural, so a method that forwards another's `out` needs that one annotated too: `FatInodeOperations.Create` cannot prove its own postcondition until `FatSuperblock.AllocateDirectoryEntry` is annotated. Never put the attribute on an input parameter. It is legal and Roslyn honours it there, the way `string.IsNullOrEmpty` uses it, so it stops meaning "the out is populated" and starts asserting that the caller's argument was not null. `IVfsFilesystemType.TryFormat` shipped exactly that, four lines under a doc sentence saying `null` selects the driver's defaults.

**A sentinel that collides with a real value needs a second member that separates them.** `SchedulerInfo.GetRunQueueCount` returns `0` for an empty queue and for a CPU id past the end, and that is honest because `CpuCount` states the range; adding a `TryGetRunQueueCount` beside it would be a third spelling of a question the ring already answers. `MACAddress.None` and a null `MACAddress` are two sentinels for one question, so the members that can return either say which and point at `NetworkAdapter.Ready`.

---

## Property or method

A value the ring holds is a property. An operation is a method. The surface already splits that way by a wide margin: 268 properties, 223 of them read-only, 34 carrying a setter and 11 an `init`, against three `Set` methods and two `Get` methods that hand back a stored value. The rule is worth writing down mostly for the things that look like they should force the method form and do not.

Work does not. `KernelConsole.Font`'s setter masks interrupts, takes the console lock, reallocates the cell grid, homes the cursor and repaints the canvas, and it is a property. Reaching the device does not either: `Canvas3D.Camera`'s setter calls a virtual hook that the VMware backend overrides to invalidate the device state derived from it. Nor does validation: `NetworkManager.Primary` throws `ArgumentException` for a handle naming no device, `TimerManager.Frequency` throws `ArgumentOutOfRangeException` for a tick the device cannot divide to, and both are properties. Nor does a feature-switch guard, which is why `MouseManager.Sensitivity` throws from its setter for the same reason `TimerManager.Wait` throws and stays a property.

Three members earn the method form on the write side, each for something an assignment cannot express.

| Member | Why it is not a property |
|--------|--------------------------|
| `MouseManager.SetPosition(x, y)` | Two coordinates that must land together, then clamp against each other |
| `MouseManager.SetScreenSize(width, height)` | The same, and it re-clamps the pointer into the new bounds |
| `KeyboardManager.SetKeyLayout(scanMap)` | The two halves cannot share a type. The read is honestly `ScanMapBase?`, null until a keyboard registers and null for good with the feature compiled out, while a null layout would leave the interrupt path with nothing to decode with. Both forms refuse null at run time; only a non-nullable parameter also diagnoses it at compile time |

A get-only property does not oblige a `Set` twin, and the pairing of the two is the shape to look for. `TimerManager` held the surface's only one: a property to read the tick and a method to change it, which reads as an oversight rather than a decision, and which hid that the write was never checked. The device rejects a frequency it cannot divide to, `ITimerDevice.SetFrequency` returned `void`, so `SetFrequency(5)` returned normally having done nothing. It is one property now, and the device says whether it took the value. `KeyboardManager` is the one pair left, and it is a method on both halves rather than half of each: since the write cannot become a setter, the read stays `GetKeyLayout()` so the two spellings match.

The read side takes the same test. A property hands back a value the type already holds. A method is for a read that computes it (`TcpPacket.GetFlags` builds a string, `TrueTypeFont.GetMaxAdvance` measures every printable character), allocates on every call (`IcmpPacket.GetIcmpData` and `UdpPacket.GetUdpData` each copy the payload out of the frame), or takes a parameter that selects which value (`NetworkManager.GetAdapter(index)`, `SchedulerInfo.GetRunQueueCount(cpuId)`, `TrueTypeFont.GetAscent(sizePx)`).

Those two tests cut across the array-returning reads, which is worth saying because "it returns an array" is not the test. `Font.Data`, `Image.RawData`, `EthernetPacket.RawData`, `TcpOption.Data`, `DhcpOption.Data` and `DnsAnswer.Address` hand out an array the type stores, and they are properties. `IcmpPacket.GetIcmpData` and `UdpPacket.GetUdpData` build a fresh copy per call, and they are methods; `UdpData` was a property doing exactly that until it was renamed, so `packet.UdpData[i]` in a loop copied the whole payload once per iteration. `Canvas.GetBuffer` is the single array read that is a method for a value the type stores, and it is one because assigning `Canvas.Mode` replaces that array under a caller already holding it, so the call form says to ask again after a mode change. `.editorconfig` sets CA1819 to `warning`, which would move all of them; this rule is the narrower one and the six properties stay.

An `out` parameter is not a third option. `MemoryInfo.GetGcStats` handed back two collector counters that way, among five properties, and splitting them cost nothing: they are two fields the collector writes at different moments with no lock across them, so the out-method never gave a caller a consistent pair either. `TrueTypeFont.GetLineMetrics(sizePx, out ascent, out descent, out lineGap)` keeps its group because the three values come out of one scaled read of the font's vertical metrics, and a caller wanting two of them would otherwise pay for it twice. Every other `out` on the surface belongs to a `Try` member, which the failure table above already owns.

Two rules on overload sets, both about the ring not making the obvious spelling the wrong one.

**A `bool` that switches what a member produces is a second spelling, not an option.** `Log.WriteNumber(x, hex: true)` printed exactly what `Log.WriteHex(x)` prints, no call site anywhere passed it, and the parameter is gone. `WriteHex` and `WriteHexWithPrefix` are the ring's one answer for hex, split on the only distinction that is a real question.

**A `params` overload takes a `ReadOnlySpan`, not an array.** `Log` promises allocation-free writes for strings and numbers, and `Log.Write("text")` bound to `Write(params object?[])`, which allocates an array for its single argument; the plug layer paid that on every `Monitor` acquire and release, which is every `lock` in BCL code. `params ReadOnlySpan<object?>` is the same call at every site and the compiler builds the list on the stack, so the allocation is gone for every argument count rather than for the one a typed twin would have covered. What remains is the box around a value type, and the fix for that is the typed overload at the call site: `Log.WriteNumber(n)`, not `Log.Write(n)`.

---

## Naming

Two rules carry weight; the rest of this section records what they decided and what they deliberately left alone.

**A ring or seam type must be nameable without a using-alias.** `ImplicitUsings` is on repo-wide, so `System`, `System.Collections.Generic`, `System.IO`, `System.Linq`, `System.Net.Http`, `System.Threading` and `System.Threading.Tasks` are in scope in every kernel file whether or not the author asked for them. A public Cosmos type whose simple name matches one of theirs is not a naming preference, it is a compile error in the reader's file. The scheduler seam shipped two: `Thread` and `ThreadState`. The sample under [Attaching state](scheduler-plugging.md#attaching-state) is three lines of hook signatures, and compiled against the ring it produced three `CS0104: 'Thread' is an ambiguous reference between 'Cosmos.Kernel.Core.Scheduler.Thread' and 'System.Threading.Thread'`. The tree had already voted with its hands: eleven first-party files carried twelve alias lines, eight writing `using SchedThread =`, two `using SchedulerThread =` and one `using SchedThreadState =`, and the in-tree example policy that [the plugging guide](scheduler-plugging.md#round-robin) points at as "exactly as a user policy would" needed `using Thread =` on its third line to compile. They are `SchedulerThread` and `SchedulerThreadState` now, with `ThreadFlags` following them so the family stays one family, and all twelve aliases are deleted rather than rewritten.

The name the obvious fix reaches for was the wrong one. `KernelThread` collides with nothing the compiler can see, and that is the problem: the ring already ships `KernelThreadInfo` and `KernelThreadState` in `Cosmos.Kernel.System.Diagnostics` as the projection of the seam type, so `KernelThread.State` would return `Core.Scheduler.ThreadState` while `KernelThreadInfo.State` returned `Diagnostics.KernelThreadState`, and a reader pairing the two would be wrong with nothing to tell them so. A silent confusion is worse than the loud one it replaces. `SchedulerThread` matches its namespace, matches what nine files chose unprompted, and leaves the `KernelThread*` pair unambiguously the ring's half.

The other three colliding simple names on the seam are `Monitor`, `Mutex` and `SpinLock`. All three are internal, so they cost a first-party alias and nothing on the ring.

**A member does not repeat its declaring type.** `Image.Depth` was the graphics namespace's fourth spelling of one concept, beside `Mode.ColorDepth`, `Canvas`'s `colorDepth` parameter and `Image`'s own constructor parameter `color`, which named a depth in a namespace where `color` otherwise means a `System.Drawing.Color`. Both are `ColorDepth` now. Three members keep a repetition because it earns its place:

| Member | Why the repetition stays |
|--------|--------------------------|
| `Canvas3D.DrawLine3D` | `Canvas3D` inherits `Canvas.DrawLine(Color, int, int, int, int)`, so both are callable on one object. The suffix says which coordinate space the call is in; dropping it leaves two same-named line operations separated only by argument types |
| `ColorDepth.ColorDepth4` .. `ColorDepth32` | The repetition only shows in qualified position, where it is unambiguous. `Bpp4`..`Bpp32` would drop the word "depth", abbreviate against the .NET guidelines, and collide in the reader's head with the `bytesPerPixel` in six files and the `BitsPerPixel` in three. Gen2 spells them the same way |
| `PCScreenFont.DefaultFont` | `PCScreenFont.Default` is taken by the nested constant holder, and the word is a contract rather than a spelling: `Sdk.props` emits `Cosmos.Kernel.System.Graphics.Fonts.DefaultFont` as a runtime host configuration option keyed to the `CosmosDefaultFont` property, and the embedded resource is `DefaultFont.psf` |

**One value gets one word.** Storage says `Sector`: `StartSector`, `SectorCount`, `newStartSector` across `Gpt`, `Mbr`, `Partition` and `PartitionManager`. `Ebr` said `Lba`, and section 07 sharpened it into a single signature carrying both, `TryAddLogical(device, extendedStartLba, extendedSectorCount, systemId, sectorCount, out startSector)`, whose own `<param>` for the out described it as "Absolute LBA". The parameters are `extendedStartSector` and `newStartSector` now. FAT and GPT keep `Lba` where it names a field the on-disk format names that way (`FatStartLba`, `PrimaryHeaderLba`): the rule is one word per value, not one word per assembly.

**A member does not take the BCL's name for a different type.** `IPConfig.IPAddress` returned a `Cosmos.Kernel.System.Network.IPv4.Address` in a file where `System.Net.IPAddress` is in scope and used, and [Network](../user/network.md) showed both meanings fifteen lines apart. It is `IPConfig.Address` now, matching `EndPoint.Address` and reading as one triple beside `SubnetMask` and `DefaultGateway`.

**`Initialize`, not `Init`.** 63 members across the four assemblies spelled it in full, and one member on the tracked surface abbreviated it. `ScanMapBase.InitKeys` is a protected abstract with seven overrides, implemented by any kernel writing its own keyboard layout, so it is `InitializeKeys`; the abbreviation survives on six internal helpers in `Cosmos.Kernel.Core` that the ring does not govern. An abstract member cannot be bridged with `[Obsolete]`, which makes the rename free while `Shipped` is empty and unfixable afterwards.

**A tracked type declares a namespace its own assembly owns.** `Cosmos.Kernel.HAL.Interfaces` has a root namespace of the same name, and eight of its nine files declare `Cosmos.Kernel.HAL.Interfaces` or `Cosmos.Kernel.HAL.Interfaces.Devices`. `MACAddress` declared `Cosmos.Kernel.HAL.Devices.Network`, which belongs to `Cosmos.Kernel.HAL`, where its only two neighbours are the internal `NetworkDevice` and `VirtioNet`. Nothing failed to compile, because a namespace is not a reference boundary. What it produced is a published API tree carrying a `Cosmos.Kernel.HAL.Devices.Network` branch for one type, while not one of `Cosmos.Kernel.HAL`'s own 114 surface lines sits outside `Cosmos.Kernel.HAL.Vfs`. It is `Cosmos.Kernel.HAL.Interfaces.Devices` now, so the assembly's three tracked types, `IBlockDevice`, `MACAddress` and `SoftwareTimer`, share one namespace and one `using`: nine files swap that using, six drop it, and the four that keep the old one keep it for `VirtioNet`. A namespace is part of what a release freezes, so the move is free while `PublicAPI.Shipped.txt` is still empty and a breaking change afterwards.

Two things this section looked at and left alone. `Manager` means the ring's entry point for a subsystem, not that the type holds the subsystem's state: `AlarmManager` forwards to an implementation internal to `Cosmos.Kernel.Core` and is defended above on exactly that ground, so `PartitionManager` being stateless beside `Gpt`, `Mbr` and `Ebr` is the suffix working, not failing. And three ring types are nested in the type that produces them, `KeyEvent.KeyEventType`, `PartitionManager.PartitionLocation` and `VfsManager.VfsMount`; rule 1 governs whether a type is public, not where it is declared, and a name that is meaningless apart from its producer is where nesting belongs.

---

## The declared surface

Projects opt in with `<CosmosTrackPublicApi>true</CosmosTrackPublicApi>` in their `.csproj` (wired in `Directory.Build.props`). That covers `Cosmos.Kernel.System`, `Cosmos.Kernel.Core`, `Cosmos.Kernel.HAL`, and `Cosmos.Kernel.HAL.Interfaces`.

The API site publishes those same four projects and no others: `docs/docfx.json` lists them, and an assembly that is not tracked is not published. The two lists drifted apart once already, which put `Cosmos.Kernel.Boot.Limine`'s 28 public types and `Cosmos.Kernel.Debug`'s one on the site under a policy that calls both internal, alongside two entries that emitted nothing only because they happen to declare no public type at all.

The rule points one way. An untracked assembly with public types left in it is not a reason to publish it; it is the backlog the last row of the policy table describes.

That backlog is not only what `Cosmos.Kernel` drags in. `Cosmos.Sdk` gives every kernel a direct `PackageReference` to `Cosmos.Kernel.Core.X64` or `Cosmos.Kernel.Core.ARM64` by architecture, so a kernel's completion list carries 13 public types on x64 (`ApicManager`, `Idt`, `IoApic`, `LocalApic`, `DeviceMapper` and the ACPI MADT records) and 7 on arm64 (the `GIC` family plus `AcpiGic` and `DeviceMapper`), reaching the user without passing through the aggregator at all. `Cosmos.Kernel.HAL.X64` and `Cosmos.Kernel.HAL.ARM64` have none, so the arch tier is half clean already.

An opted-in project references [Microsoft.CodeAnalysis.PublicApiAnalyzers](https://github.com/dotnet/roslyn-analyzers/blob/main/src/PublicApiAnalyzers/PublicApiAnalyzers.Help.md), which requires every `public` symbol to appear in one of two files next to the `.csproj`:

| File | Contents |
|------|----------|
| `PublicAPI.Shipped.txt` | The surface of the last release. Frozen: it only changes when a release is cut. |
| `PublicAPI.Unshipped.txt` | Surface added, changed, or removed since then. Emptied into `Shipped` at release time. |

Two diagnostics enforce the contract, both raised as build errors:

| Rule | Meaning |
|------|---------|
| `RS0016` | A public symbol exists in code but is not declared in either file. |
| `RS0017` | A declared symbol no longer exists in code. |

The effect is that any change to the public surface, deliberate or accidental, must land in the same commit as a diff of `PublicAPI.Unshipped.txt`, where the review can see it.

Three categories stay out of the files, the first two through `.editorconfig` overrides that set `RS0016`/`RS0017` to `none` under their paths:

- The vendored directories (`SharpZipLib`, the BigGustave PNG decoder, the TrueType fonts). The exemption is what let that cleanup happen without churning the declared surface; all three trees are `internal` throughout now, so it currently exempts nothing and stays as the standing rule for the next vendored tree.
- The generated `KernelVersion.g.cs` that carries `Kernel.VersionString`: the declared-API format records constant values, and this one changes with every version stamp.
- `internal` symbols, including everything exposed to the test kernels through `InternalsVisibleTo`.

### Changing the public API

1. Make the code change. The build now fails with `RS0016` or `RS0017`.
2. Run `make api` from the repository root. It rebuilds the tracked projects and applies the analyzer's code fixes to `PublicAPI.Unshipped.txt`. The IDE quick fix ("Add to public API") on each diagnostic is equivalent.
3. Commit the txt diff together with the code.

Removing or changing a symbol that already shipped is recorded as a `*REMOVED*` line in `Unshipped`, which is exactly what it is: a breaking change, visible as such in the PR. Before the first release, while `Shipped` is empty, a removal simply drops the line.

The code fix inserts a new line in sorted position and never touches the lines already there, so the file's order is whatever the last hand that edited it left behind. Its comparer is `OrdinalIgnoreCase` with an `Ordinal` tie-break, which is not what any shell sort does: a `LC_ALL=C sort` once ran over `Cosmos.Kernel.System/PublicAPI.Unshipped.txt` and left 19 lines the code fix would never have placed there, including the six `abstract` and four `const` entries that belong at the top. No analyzer reads line position, so nothing reported it and nothing will. Sort with the analyzer's own comparer, or leave the file to the code fix:

```
python3 -c "p='src/Cosmos.Kernel.System/PublicAPI.Unshipped.txt'; L=open(p).read().split(chr(10)); L=[l for l in L if l]; open(p,'w').write(chr(10).join([L[0]]+sorted(L[1:],key=lambda s:(s.upper(),s)))+chr(10))"
```

---

## Package validation

The same `CosmosTrackPublicApi` flag enables [NuGet package validation](https://learn.microsoft.com/en-us/dotnet/fundamentals/package-validation/overview) on the project, and `Directory.Build.props` holds the baseline knob:

```xml
<CosmosApiBaselineVersion></CosmosApiBaselineVersion>
```

The property is empty until the first Gen3 release is published on NuGet.org. Once it is pinned to that version, two guards activate on their own:

- `PackageValidationBaselineVersion` makes every pack compare the package against the baseline release and fail on binary breaking changes.
- The `API compatibility guard` step in `release.yml` downloads the baseline package from NuGet.org and runs [Microsoft.DotNet.ApiCompat.Tool](https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/global-tool) against the freshly built one, as a final gate before publishing.

An intentional breaking change is recorded in a `CompatibilitySuppressions.xml` next to the project (`dotnet pack /p:ApiCompatGenerateSuppressionFile=true` generates it), so it too becomes a reviewable diff.

---

## Versioned docs

The docs site at the root of `gh-pages` follows the main branch: it is the dev documentation, rebuilt by `build-docs.yml` on every docs push. Releases get frozen copies next to it, published by the `publish-docs` job of `release.yml` when a `v*` tag is pushed:

| Path | Contents |
|------|----------|
| `/` | Dev docs, follows main. |
| `/vX.Y.Z/` | The docs as built from tag `vX.Y.Z`, never rebuilt. |
| `/latest/` | Alias of the newest release, stable URL for external links. |
| `/versions.json` | The release list, newest first, plus the `latest` marker. |

The version dropdown in the site navbar comes from `docs/templates/custom/public/main.js`. It reads `versions.json`, lists `dev` plus every release, and keeps the reader on the same page across versions when the page exists there. It only appears once `versions.json` exists, i.e. after the first tagged release; until then the site behaves exactly as before.

The dev deploys use `keep_files: true` so they never wipe the `v*/` folders, at the cost of deleted dev pages lingering on the branch until overwritten.

---

## Release checklist

What cutting a release changes in this system:

1. Tag `vX.Y.Z`. CI builds the packages, runs the API compatibility guard, publishes to NuGet.org, and freezes `/vX.Y.Z/` docs.
2. Move the contents of `PublicAPI.Unshipped.txt` into `PublicAPI.Shipped.txt` (drop the `*REMOVED*` pairs), leaving `Unshipped` empty.
3. Set `CosmosApiBaselineVersion` in `Directory.Build.props` to `X.Y.Z` so the next cycle validates against this release.
