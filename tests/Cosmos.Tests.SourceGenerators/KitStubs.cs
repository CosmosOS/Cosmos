// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Tests.SourceGenerators;

/// <summary>
/// Source text standing in for the kit and ring types the generator looks
/// up by name. The shapes are the minimum the generated code and the test
/// drivers compile against; the real types live in Cosmos.Kernel.HAL and
/// Cosmos.Kernel.System, which the test host does not load.
/// </summary>
internal static class KitStubs
{
    /// <summary>The kernel class every test compilation constructs, as <c>CosmosKernelClass</c> names it.</summary>
    public const string KernelClass = "MyOS.Kernel";

    /// <summary>A kernel deriving from the ring's base class.</summary>
    public const string Kernel = """
        namespace MyOS
        {
            public sealed class Kernel : Cosmos.Kernel.System.Kernel
            {
            }
        }
        """;

    /// <summary>
    /// <c>Cosmos.Kernel.HAL.DriverKit</c>: the driver base class, the attribute
    /// and enum, the registry and the types the base class signature names.
    /// </summary>
    /// <param name="visibility"><c>internal</c> when the stubs share the kernel's compilation, <c>public</c> when they are compiled into a referenced assembly.</param>
    /// <param name="extraFeature">An extra <c>DriverFeature</c> member, to test a feature the ring lacks; null for none.</param>
    public static string Hal(string visibility, string? extraFeature = null)
    {
        string extra = extraFeature is null ? string.Empty : $", {extraFeature}";
        return $$"""
            using System;

            namespace Cosmos.Kernel.HAL.DriverKit
            {
                {{visibility}} abstract class DeviceMatch
                {
                    public abstract int Specificity { get; }
                }

                {{visibility}} sealed class DeviceBinding
                {
                }

                {{visibility}} readonly struct ProbeResult
                {
                    public static ProbeResult Bound => default;
                }

                {{visibility}} abstract class Driver
                {
                    public abstract string Name { get; }
                    public virtual ReadOnlySpan<DeviceMatch> Matches => default;
                    public virtual int Priority => 0;
                    public virtual ProbeResult Probe(DeviceBinding binding) => ProbeResult.Bound;
                }

                {{visibility}} enum DriverFeature
                {
                    None, Interrupts, Uart, Pci, Timer, Keyboard, Mouse, Network, Storage, Fat, Graphics, Scheduler, Usb{{extra}}
                }

                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                {{visibility}} sealed class DriverAttribute : Attribute
                {
                    public DriverFeature Feature { get; set; }
                    public bool Default { get; set; } = true;
                }

                {{visibility}} static class DriverRegistry
                {
                    public static void Register(Driver driver)
                    {
                    }
                }
            }
            """;
    }

    /// <summary><c>Cosmos.Kernel.System</c>: the feature switches, the kernel base class and the entry point's target.</summary>
    public const string System = """
        namespace Cosmos.Kernel.System
        {
            public static class KernelFeatures
            {
                public static bool Interrupts => true;
                public static bool Uart => true;
                public static bool Pci => true;
                public static bool Timer => true;
                public static bool Keyboard => true;
                public static bool Mouse => true;
                public static bool Network => true;
                public static bool Storage => true;
                public static bool Fat => true;
                public static bool Graphics => true;
                public static bool Scheduler => true;
                public static bool Usb => true;
            }

            public abstract class Kernel
            {
                public virtual void Start()
                {
                }
            }

            public static class Global
            {
                public static void RegisterKernel(Kernel kernel)
                {
                }

                public static void StartKernel()
                {
                }
            }
        }
        """;
}
