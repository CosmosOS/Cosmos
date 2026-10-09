// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Microsoft.CodeAnalysis;
using Xunit;

namespace Cosmos.Tests.SourceGenerators;

/// <summary>
/// The entry point and the manifest of a Visual Basic kernel: the same files
/// as a C# kernel's, spelled in Visual Basic, declared in
/// <c>Cosmos.Kernel.System.Internal</c> whatever the project's root
/// namespace, and holding the kernel's own drivers and those of the
/// assemblies it references, which are written in C#.
/// </summary>
public sealed class VisualBasicTests
{
    private const string DriversAssemblyName = "Acme.Drivers";

    /// <summary>A driver library written in C#, as the shipped drivers are.</summary>
    private const string DriverLibrary = """
        using Cosmos.Kernel.HAL.DriverKit;

        namespace Acme.Drivers
        {
            [Driver]
            public sealed class ZDriver : Driver
            {
                public override string Name => "z";
            }
        }
        """;

    [Fact]
    public async Task WhenNoDriver_EmitsVisualBasicEntryPointAndEmptyManifest()
    {
        VisualBasicManifestTest test = await VisualBasicManifestTest.CreateAsync();
        await test
            .WithKernelClass(KitStubs.KernelClass)
            .ExpectEntryPoint(KitStubs.KernelClass)
            .ExpectManifest()
            .RunAsync();
    }

    [Fact]
    public async Task WhenKernelAndLibraryDeclareDrivers_RegistersTheKernelsOwnFirst_UnderTheirGuards()
    {
        VisualBasicManifestTest test = await VisualBasicManifestTest.CreateAsync();
        MetadataReference drivers = StubAssemblies.Compile(DriversAssemblyName, test.Framework.Add(test.Hal), DriverLibrary);
        await test
            .WithKernelClass(KitStubs.KernelClass)
            .WithReference(drivers)
            .WithSource("/k/Drivers.vb", """
                Imports Cosmos.Kernel.HAL.DriverKit

                Namespace Drivers
                    <Driver(Feature:=DriverFeature.Network)>
                    Friend NotInheritable Class Board
                        Inherits Driver

                        Public Overrides ReadOnly Property Name As String
                            Get
                                Return "board"
                            End Get
                        End Property
                    End Class
                End Namespace
                """)
            .ExpectEntryPoint(KitStubs.KernelClass)
            .ExpectManifest(
                GeneratedText.VisualBasicGuardedRegistration("Network", "MyOS.Drivers.Board"),
                GeneratedText.VisualBasicRegistration("Acme.Drivers.ZDriver"))
            .RunAsync();
    }

    [Fact]
    public async Task WhenDriverIsMustInherit_ReportsItAtTheClassName()
    {
        VisualBasicManifestTest test = await VisualBasicManifestTest.CreateAsync();
        await test
            .WithKernelClass(KitStubs.KernelClass)
            .WithSource("/k/Drivers.vb", """
                Imports Cosmos.Kernel.HAL.DriverKit

                Namespace Drivers
                    <Driver>
                    Public MustInherit Class {|#0:Base|}
                        Inherits Driver
                    End Class
                End Namespace
                """)
            .ExpectEntryPoint(KitStubs.KernelClass)
            .ExpectManifest()
            .ExpectDiagnostic(Diagnostics.DriverSkipped(0, "MyOS.Drivers.Base", "the class is abstract"))
            .RunAsync();
    }
}
