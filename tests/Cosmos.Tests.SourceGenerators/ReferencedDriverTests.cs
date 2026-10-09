// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Cosmos.Tests.SourceGenerators;

/// <summary>
/// Drivers that reach the kernel through a referenced assembly: the kit
/// stubs are compiled into an assembly named <c>Cosmos.Kernel.HAL</c>, a
/// driver library referencing it is compiled next, and the kernel under
/// test references both. The kit is compiled public, as it will be once it
/// ships, and internal, as in stage 1, where a kernel or a library declares
/// a <c>[Driver]</c> class only under an <c>InternalsVisibleTo</c> grant
/// from the HAL. The harness names the kernel compilation
/// <c>TestProject</c>; the grants read the name from the test state rather
/// than repeat it.
/// </summary>
public sealed class ReferencedDriverTests
{
    private const string HalAssemblyName = "Cosmos.Kernel.HAL";
    private const string DriversAssemblyName = "Acme.Drivers";
    private const string InternalDriversAssemblyName = "Acme.Internal";

    private const string DriverLibrary = """
        using Cosmos.Kernel.HAL.DriverKit;

        namespace Acme.Drivers
        {
            [Driver]
            public sealed class ZDriver : Driver
            {
                public override string Name => "z";
            }

            [Driver(Feature = DriverFeature.Storage)]
            public sealed class ADriver : Driver
            {
                public override string Name => "a";
            }

            [Driver]
            internal sealed class HiddenDriver : Driver
            {
                public override string Name => "hidden";
            }

            public static class Bus
            {
                [Driver]
                public sealed class Child : Driver
                {
                    public override string Name => "child";
                }
            }
        }
        """;

    /// <summary>
    /// A library written against the internal kit: every driver is internal,
    /// since a public class cannot derive from an internal one. Declared out
    /// of name order, to show the manifest sorts them.
    /// </summary>
    private const string InternalDriverLibrary = """
        using Cosmos.Kernel.HAL.DriverKit;

        namespace Acme.Internal
        {
            [Driver(Feature = DriverFeature.Pci)]
            internal sealed class Slot : Driver
            {
                public override string Name => "slot";
            }

            [Driver]
            internal sealed class Bridge : Driver
            {
                public override string Name => "bridge";
            }
        }
        """;

    private const string KernelDriver = """
        using Cosmos.Kernel.HAL.DriverKit;

        namespace MyOS.Drivers
        {
            [Driver]
            internal sealed class Board : Driver
            {
                public override string Name => "board";
            }
        }
        """;

    [Fact]
    public async Task WhenReferencedAssemblySeesTheKit_RegistersItsVisibleDriversAfterTheKernelsOwn()
    {
        ImmutableArray<MetadataReference> framework = await FrameworkAsync();
        MetadataReference hal = Compile(HalAssemblyName, framework, KitStubs.Hal("public"));
        MetadataReference drivers = Compile(DriversAssemblyName, framework.Add(hal), DriverLibrary);

        ManifestTest test = new ManifestTest(includeKitStubs: false)
            .WithKernelClass(KitStubs.KernelClass)
            .WithSource("/k/Drivers.cs", KernelDriver)
            .ExpectEntryPoint(KitStubs.KernelClass)
            .ExpectManifest(
                GeneratedText.Registration("MyOS.Drivers.Board"),
                GeneratedText.GuardedRegistration("Storage", "Acme.Drivers.ADriver"),
                GeneratedText.Registration("Acme.Drivers.Bus.Child"),
                GeneratedText.Registration("Acme.Drivers.ZDriver"));
        test.TestState.AdditionalReferences.Add(hal);
        test.TestState.AdditionalReferences.Add(drivers);
        await test.RunAsync();
    }

    [Fact]
    public async Task WhenReferencedDriverIsExcluded_LeavesItOut()
    {
        ImmutableArray<MetadataReference> framework = await FrameworkAsync();
        MetadataReference hal = Compile(HalAssemblyName, framework, KitStubs.Hal("public"));
        MetadataReference drivers = Compile(DriversAssemblyName, framework.Add(hal), DriverLibrary);

        ManifestTest test = new ManifestTest(includeKitStubs: false)
            .WithKernelClass(KitStubs.KernelClass)
            .WithExcludeList("Acme.Drivers.ZDriver,Acme.Drivers.Bus.Child")
            .ExpectEntryPoint(KitStubs.KernelClass)
            .ExpectManifest(GeneratedText.GuardedRegistration("Storage", "Acme.Drivers.ADriver"));
        test.TestState.AdditionalReferences.Add(hal);
        test.TestState.AdditionalReferences.Add(drivers);
        await test.RunAsync();
    }

    [Fact]
    public async Task WhenKitIsInternal_AndKernelHoldsAGrant_RegistersTheKernelsInternalDriver()
    {
        ManifestTest test = new ManifestTest(includeKitStubs: false)
            .WithKernelClass(KitStubs.KernelClass)
            .WithSource("/k/Drivers.cs", KernelDriver)
            .ExpectEntryPoint(KitStubs.KernelClass)
            .ExpectManifest(GeneratedText.Registration("MyOS.Drivers.Board"));
        ImmutableArray<MetadataReference> framework = await FrameworkAsync();
        MetadataReference hal = Compile(HalAssemblyName, framework, KitStubs.Hal("internal"), InternalsVisibleTo(test.TestState.AssemblyName));

        test.TestState.AdditionalReferences.Add(hal);
        await test.RunAsync();
    }

    [Fact]
    public async Task WhenKitIsInternal_AndLibraryHoldsAndPassesOnAGrant_RegistersItsInternalDriversAfterTheKernelsOwn()
    {
        ManifestTest test = new ManifestTest(includeKitStubs: false)
            .WithKernelClass(KitStubs.KernelClass)
            .WithSource("/k/Drivers.cs", KernelDriver)
            .ExpectEntryPoint(KitStubs.KernelClass)
            .ExpectManifest(
                GeneratedText.Registration("MyOS.Drivers.Board"),
                GeneratedText.Registration("Acme.Internal.Bridge"),
                GeneratedText.GuardedRegistration("Pci", "Acme.Internal.Slot"));
        string kernelAssemblyName = test.TestState.AssemblyName;
        ImmutableArray<MetadataReference> framework = await FrameworkAsync();
        MetadataReference hal = Compile(HalAssemblyName, framework, KitStubs.Hal("internal"), InternalsVisibleTo(kernelAssemblyName, InternalDriversAssemblyName));
        MetadataReference drivers = Compile(InternalDriversAssemblyName, framework.Add(hal), InternalDriverLibrary, InternalsVisibleTo(kernelAssemblyName));

        test.TestState.AdditionalReferences.Add(hal);
        test.TestState.AdditionalReferences.Add(drivers);
        await test.RunAsync();
    }

    private static Task<ImmutableArray<MetadataReference>> FrameworkAsync() =>
        ReferenceAssemblies.Net.Net90.ResolveAsync(LanguageNames.CSharp, CancellationToken.None);

    /// <summary>A compilation unit that grants each of <paramref name="assemblyNames"/> access to the internals of the assembly it is compiled into.</summary>
    /// <param name="assemblyNames">The simple names of the assemblies granted access.</param>
    private static string InternalsVisibleTo(params string[] assemblyNames)
    {
        StringBuilder builder = new();
        foreach (string assemblyName in assemblyNames)
        {
            builder.Append("[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(\"").Append(assemblyName).Append("\")]\n");
        }

        return builder.ToString();
    }

    /// <summary>Compiles <paramref name="sources"/> into an in-memory library named <paramref name="assemblyName"/>, failing the test on any compile error.</summary>
    /// <param name="assemblyName">The library's simple name, which the generator and the grants match on.</param>
    /// <param name="references">The framework and the libraries it references.</param>
    /// <param name="sources">One compilation unit per string.</param>
    private static MetadataReference Compile(string assemblyName, ImmutableArray<MetadataReference> references, params string[] sources)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            sources.Select(source => CSharpSyntaxTree.ParseText(source)),
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using MemoryStream stream = new();
        Microsoft.CodeAnalysis.Emit.EmitResult result = compilation.Emit(stream);
        Assert.True(result.Success, string.Join("\n", result.Diagnostics));
        return MetadataReference.CreateFromImage(stream.ToArray());
    }
}
