// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Xunit;

namespace Cosmos.Tests.SourceGenerators;

/// <summary>
/// The entry point and the manifest as generated from a kernel's own
/// sources: what is emitted, in what order, under which guards, and what the
/// policy lists and the diagnostics do.
/// </summary>
public sealed class CosmosEntryPointGeneratorTests
{
    private const string Prelude = "using Cosmos.Kernel.HAL.DriverKit;\n\n";

    [Fact]
    public Task WhenNoDriver_EmitsEntryPointAndEmptyManifest() =>
        new ManifestTest()
            .WithKernelClass(KitStubs.KernelClass)
            .ExpectEntryPoint(KitStubs.KernelClass)
            .ExpectManifest()
            .RunAsync();

    [Fact]
    public Task WhenKernelClassIsUnset_EmitsNothing() =>
        new ManifestTest()
            .WithSource("/k/Drivers.cs", Prelude + """
                namespace MyOS.Drivers
                {
                    [Driver]
                    internal sealed class BoardDriver : Driver
                    {
                        public override string Name => "board";
                    }
                }
                """)
            .RunAsync();

    [Fact]
    public Task WhenDriversSpanFiles_OrdersByFilePathThenPosition() =>
        new ManifestTest()
            .WithKernelClass(KitStubs.KernelClass)
            .WithSource("/k/Second.cs", Prelude + """
                namespace MyOS.Drivers
                {
                    [Driver]
                    internal sealed class Zeta : Driver
                    {
                        public override string Name => "zeta";
                    }

                    [Driver]
                    internal sealed class Alpha : Driver
                    {
                        public override string Name => "alpha";
                    }
                }
                """)
            .WithSource("/k/First.cs", Prelude + """
                namespace MyOS.Drivers
                {
                    internal static class Bus
                    {
                        [Driver]
                        internal sealed class Nested : Driver
                        {
                            public override string Name => "nested";
                        }
                    }

                    [Driver(Feature = DriverFeature.Network)]
                    internal sealed class Nic : Driver
                    {
                        public override string Name => "nic";
                    }
                }
                """)
            .ExpectEntryPoint(KitStubs.KernelClass)
            .ExpectManifest(
                GeneratedText.Registration("MyOS.Drivers.Bus.Nested"),
                GeneratedText.GuardedRegistration("Network", "MyOS.Drivers.Nic"),
                GeneratedText.Registration("MyOS.Drivers.Zeta"),
                GeneratedText.Registration("MyOS.Drivers.Alpha"))
            .RunAsync();

    [Fact]
    public Task WhenFeatureIsNone_EmitsNoGuard() =>
        new ManifestTest()
            .WithKernelClass(KitStubs.KernelClass)
            .WithSource("/k/Drivers.cs", Prelude + """
                namespace MyOS.Drivers
                {
                    [Driver(Feature = DriverFeature.None, Default = true)]
                    internal sealed class BoardDriver : Driver
                    {
                        public override string Name => "board";
                    }
                }
                """)
            .ExpectEntryPoint(KitStubs.KernelClass)
            .ExpectManifest(GeneratedText.Registration("MyOS.Drivers.BoardDriver"))
            .RunAsync();

    [Fact]
    public Task WhenDriverIsExcluded_LeavesItOut() =>
        new ManifestTest()
            .WithKernelClass(KitStubs.KernelClass)
            .WithExcludeList(" MyOS.Drivers.Second ,")
            .WithSource("/k/Drivers.cs", Prelude + """
                namespace MyOS.Drivers
                {
                    [Driver]
                    internal sealed class First : Driver
                    {
                        public override string Name => "first";
                    }

                    [Driver]
                    internal sealed class Second : Driver
                    {
                        public override string Name => "second";
                    }
                }
                """)
            .ExpectEntryPoint(KitStubs.KernelClass)
            .ExpectManifest(GeneratedText.Registration("MyOS.Drivers.First"))
            .RunAsync();

    [Fact]
    public Task WhenOptInDriverIsIncluded_RegistersIt() =>
        new ManifestTest()
            .WithKernelClass(KitStubs.KernelClass)
            .WithIncludeList("MyOS.Drivers.OptIn")
            .WithSource("/k/Drivers.cs", Prelude + """
                namespace MyOS.Drivers
                {
                    [Driver(Default = false, Feature = DriverFeature.Mouse)]
                    internal sealed class OptIn : Driver
                    {
                        public override string Name => "opt-in";
                    }
                }
                """)
            .ExpectEntryPoint(KitStubs.KernelClass)
            .ExpectManifest(GeneratedText.GuardedRegistration("Mouse", "MyOS.Drivers.OptIn"))
            .RunAsync();

    [Fact]
    public Task WhenOptInDriverIsNotIncluded_LeavesItOut() =>
        new ManifestTest()
            .WithKernelClass(KitStubs.KernelClass)
            .WithSource("/k/Drivers.cs", Prelude + """
                namespace MyOS.Drivers
                {
                    [Driver(Default = false)]
                    internal sealed class OptOut : Driver
                    {
                        public override string Name => "opt-out";
                    }

                    [Driver]
                    internal sealed class Always : Driver
                    {
                        public override string Name => "always";
                    }
                }
                """)
            .ExpectEntryPoint(KitStubs.KernelClass)
            .ExpectManifest(GeneratedText.Registration("MyOS.Drivers.Always"))
            .RunAsync();

    [Fact]
    public Task WhenDriverIsAbstract_ReportsGen001AndLeavesItOut() =>
        new ManifestTest()
            .WithKernelClass(KitStubs.KernelClass)
            .WithSource("/k/Drivers.cs", Prelude + """
                namespace MyOS.Drivers
                {
                    [Driver]
                    internal abstract class {|#0:Base|} : Driver
                    {
                    }

                    [Driver]
                    internal sealed class Concrete : Base
                    {
                        public override string Name => "concrete";
                    }
                }
                """)
            .ExpectEntryPoint(KitStubs.KernelClass)
            .ExpectManifest(GeneratedText.Registration("MyOS.Drivers.Concrete"))
            .ExpectDiagnostic(Diagnostics.DriverSkipped(0, "MyOS.Drivers.Base", "the class is abstract"))
            .RunAsync();

    [Fact]
    public Task WhenDriverCannotBeConstructed_ReportsGen001ForEachReason() =>
        new ManifestTest()
            .WithKernelClass(KitStubs.KernelClass)
            .WithSource("/k/Drivers.cs", Prelude + """
                namespace MyOS.Drivers
                {
                    [Driver]
                    internal static class {|#0:Utility|}
                    {
                    }

                    [Driver]
                    internal sealed class {|#1:Generic|}<T> : Driver
                    {
                        public override string Name => "generic";
                    }

                    [Driver]
                    internal sealed class {|#2:NotADriver|}
                    {
                    }

                    [Driver]
                    internal sealed class {|#3:NeedsArgument|} : Driver
                    {
                        public NeedsArgument(int port)
                        {
                        }

                        public override string Name => "needs-argument";
                    }

                    internal sealed class Outer
                    {
                        [Driver]
                        private sealed class {|#4:Hidden|} : Driver
                        {
                            public override string Name => "hidden";
                        }
                    }
                }
                """)
            .ExpectEntryPoint(KitStubs.KernelClass)
            .ExpectManifest()
            .ExpectDiagnostic(Diagnostics.DriverSkipped(0, "MyOS.Drivers.Utility", "the class is static"))
            .ExpectDiagnostic(Diagnostics.DriverSkipped(1, "MyOS.Drivers.Generic<T>", "the class is generic, or nested in a generic type"))
            .ExpectDiagnostic(Diagnostics.DriverSkipped(2, "MyOS.Drivers.NotADriver", "the class does not derive from Cosmos.Kernel.HAL.DriverKit.Driver"))
            .ExpectDiagnostic(Diagnostics.DriverSkipped(3, "MyOS.Drivers.NeedsArgument", "the class has no parameterless constructor the kernel assembly can call"))
            .ExpectDiagnostic(Diagnostics.DriverSkipped(4, "MyOS.Drivers.Outer.Hidden", "the class is not accessible from the kernel assembly"))
            .RunAsync();

    [Fact]
    public Task WhenPolicyNamesNoDriver_ReportsGen002() =>
        new ManifestTest()
            .WithKernelClass(KitStubs.KernelClass)
            .WithExcludeList("MyOS.Drivers.Gone")
            .WithIncludeList("MyOS.Drivers.Board,MyOS.Drivers.Missing")
            .WithSource("/k/Drivers.cs", Prelude + """
                namespace MyOS.Drivers
                {
                    [Driver]
                    internal sealed class Board : Driver
                    {
                        public override string Name => "board";
                    }
                }
                """)
            .ExpectEntryPoint(KitStubs.KernelClass)
            .ExpectManifest(GeneratedText.Registration("MyOS.Drivers.Board"))
            .ExpectDiagnostic(Diagnostics.PolicyNameUnknown("CosmosDriverExclude", "MyOS.Drivers.Gone"))
            .ExpectDiagnostic(Diagnostics.PolicyNameUnknown("CosmosDriverInclude", "MyOS.Drivers.Missing"))
            .RunAsync();

    [Fact]
    public Task WhenFeatureHasNoKernelFeaturesProperty_ReportsGen003AndLeavesItOut() =>
        new ManifestTest(includeKitStubs: false)
            .WithKernelClass(KitStubs.KernelClass)
            .WithSource("/stubs/DriverKit.cs", KitStubs.Hal("internal", extraFeature: "Audio"))
            .WithSource("/k/Drivers.cs", Prelude + """
                namespace MyOS.Drivers
                {
                    [{|#0:Driver(Feature = DriverFeature.Audio)|}]
                    internal sealed class Sound : Driver
                    {
                        public override string Name => "sound";
                    }

                    [Driver(Feature = DriverFeature.Usb)]
                    internal sealed class Hub : Driver
                    {
                        public override string Name => "hub";
                    }
                }
                """)
            .ExpectEntryPoint(KitStubs.KernelClass)
            .ExpectManifest(GeneratedText.GuardedRegistration("Usb", "MyOS.Drivers.Hub"))
            .ExpectDiagnostic(Diagnostics.FeatureUnmapped(0, "MyOS.Drivers.Sound", "Audio"))
            .RunAsync();
}
