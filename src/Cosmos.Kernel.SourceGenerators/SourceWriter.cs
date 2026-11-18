// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Microsoft.CodeAnalysis;

namespace Cosmos.Kernel.SourceGenerators;

/// <summary>
/// Writes the generated files in the kernel compilation's language: a C#
/// kernel gets <c>.g.cs</c> files, a Visual Basic kernel <c>.g.vb</c> files
/// that declare the same types in the same namespace. The generators decide
/// what the files hold; a writer only spells it.
/// </summary>
internal abstract class SourceWriter
{
    /// <summary>The namespace both generated types are declared in, whatever the kernel's own namespace.</summary>
    protected const string InternalNamespace = "Cosmos.Kernel.System.Internal";

    /// <summary>The registry the manifest registers each driver with.</summary>
    protected const string RegistryType = "Cosmos.Kernel.HAL.DriverKit.DriverRegistry";

    /// <summary>The ring's feature switches, one of which guards a driver that names a feature.</summary>
    protected const string FeaturesType = "Cosmos.Kernel.System.KernelFeatures";

    /// <summary>The ring's boot plumbing, which the entry point hands the kernel it constructs to.</summary>
    protected const string KernelEntryType = "Cosmos.Kernel.System.Internal.KernelEntry";

    /// <summary>The diagnostic the experimental driver kit seam reports, which the manifest acknowledges for itself.</summary>
    protected const string SeamDiagnosticId = "COSMOS0003";

    /// <summary>
    /// Why the manifest acknowledges the seam, one comment line each. The
    /// registry is on the experimental driver kit seam, and the drivers the
    /// kernel carries by default arrive with the aggregator package, so every
    /// kernel's manifest names it. The kernel author did not write the file,
    /// so it acknowledges the seam for itself alone: a driver the kernel
    /// declares still meets the diagnostic in the author's own code.
    /// </summary>
    protected static readonly string[] s_seamAcknowledgement =
    [
        $"The driver registry is on the experimental driver kit seam ({SeamDiagnosticId}).",
        "This file is generated, so it acknowledges the seam for itself alone;",
        "a driver this kernel declares still meets the diagnostic in its own code.",
    ];

    /// <summary>The summary on the generated entry point type.</summary>
    protected const string EntryPointSummary = "The kernel's entry point: registers the drivers in the manifest, then the kernel, and starts it.";

    /// <summary>The summary on the generated <c>Main</c>.</summary>
    protected const string MainSummary = "Called by the runtime startup code once the managed world is up.";

    /// <summary>The summary on the generated manifest type.</summary>
    protected const string ManifestSummary = "The drivers this kernel carries, registered in manifest order before the kernel starts.";

    /// <summary>The summary on the generated <c>Register</c>.</summary>
    protected const string RegisterSummary = "Registers every driver the manifest lists with the driver registry.";

    /// <summary>The extension of a generated file's hint name, <c>.g.cs</c> or <c>.g.vb</c>.</summary>
    public abstract string FileExtension { get; }

    /// <summary>The writer for a compilation in <paramref name="language"/>; C# for any language but Visual Basic.</summary>
    /// <param name="language">A <see cref="LanguageNames"/> value.</param>
    public static SourceWriter For(string language)
    {
        return language == LanguageNames.VisualBasic ? VisualBasicSourceWriter.Instance : CSharpSourceWriter.Instance;
    }

    /// <summary>
    /// The text of the <c>CosmosEntryPoint</c> file: a <c>Main</c> that
    /// registers the manifest's drivers, then constructs the kernel and
    /// starts it.
    /// </summary>
    /// <param name="kernelClass">The kernel type's full name, as <c>CosmosKernelClass</c> gives it.</param>
    public abstract string EntryPoint(string kernelClass);

    /// <summary>
    /// The text of the <c>DriverManifest</c> file: a <c>Register</c> that
    /// registers each driver in order, inside an <c>if</c> on its feature
    /// when it names one.
    /// </summary>
    /// <param name="registered">The drivers the manifest registers, in manifest order.</param>
    public abstract string Manifest(IReadOnlyList<DriverCandidate> registered);
}
