using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cosmos.Build.Analyzer.Patcher
{
    /// <summary>
    /// Roslyn diagnostic analyzer that enforces Cosmos kernel layer dependencies.
    /// The layer is inferred purely from the assembly name — no MSBuild markers needed.
    /// Plug assemblies (any assembly named <c>*.Plugs</c>) are exempt and may reference
    /// all layers freely.
    /// <para>
    /// Allowed references (strict — no skipping layers, with one exception):
    /// <list type="bullet">
    ///   <item>User     → System, HAL</item>
    ///   <item>System   → HAL</item>
    ///   <item>HAL      → HAL, Core</item>
    ///   <item>Core     → Native</item>
    ///   <item>Native   → (nothing)</item>
    /// </list>
    /// User may skip System to reach the HAL because the driver kit, the seam a kernel
    /// writes its own PCI and USB drivers against, lives there
    /// (<c>Cosmos.Kernel.HAL.DriverKit</c>, experimental COSMOS0003), next to the device
    /// contracts the System API already hands out (<c>IBlockDevice</c>,
    /// <c>MACAddress</c> in <c>Cosmos.Kernel.HAL.Interfaces</c>).
    /// </para>
    /// <para>
    /// The check runs on the compilation's metadata references, not on the symbols the
    /// code uses: a user kernel is any assembly not named <c>Cosmos.*</c> that references
    /// a layer assembly, and every referenced layer assembly is checked whether the code
    /// touches it or not. <c>Cosmos.*</c> assemblies outside the layers (the aggregator,
    /// Plugs, Debug, Boot, the <c>Cosmos.Kernel.Tests.*</c> kernels) are not checked.
    /// </para>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LayerAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticMessages.LayerViolation);

        private enum KernelLayer
        {
            Native,
            Core,
            Hal,
            System,
            User
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationAction(AnalyzeCompilation);
        }

        /// <summary>
        /// Returns true for assemblies named *.Plugs — plug projects bridge all layers
        /// and are exempt from layer checks by convention.
        /// </summary>
        private static bool IsPlugAssembly(string assemblyName)
            => assemblyName.EndsWith(".Plugs", System.StringComparison.Ordinal)
            || assemblyName.Equals("Plugs", System.StringComparison.Ordinal);

        private static void AnalyzeCompilation(CompilationAnalysisContext context)
        {
            if (IsPlugAssembly(context.Compilation.Assembly.Name))
            {
                return;
            }

            KernelLayer? currentLayer = GetLayerFromAssemblyName(context.Compilation.Assembly.Name);

            // Non-Cosmos assemblies that reference at least one Cosmos layer assembly are user kernels.
            // Cosmos.* assemblies that are not a recognised layer (aggregator, Plugs, Debug, Boot…) are skipped.
            if (currentLayer == null)
            {
                bool isUserKernel =
                    !context.Compilation.Assembly.Name.StartsWith("Cosmos.", System.StringComparison.Ordinal)
                    && context.Compilation.ReferencedAssemblyNames
                        .Any(r => GetLayerFromAssemblyName(r.Name) != null);

                if (!isUserKernel)
                {
                    return;
                }

                currentLayer = KernelLayer.User;
            }

            foreach (AssemblyIdentity referenced in context.Compilation.ReferencedAssemblyNames)
            {
                KernelLayer? referencedLayer = GetLayerFromAssemblyName(referenced.Name);
                if (referencedLayer == null)
                {
                    continue;
                }

                if (!IsReferenceAllowed(currentLayer.Value, referencedLayer.Value))
                {
                    // Try to point at the first `using` directive that imports from the violating assembly.
                    // Assembly name doubles as namespace prefix (e.g. "Cosmos.Kernel.Core" → using Cosmos.Kernel.Core.*).
                    Location location = FindUsingLocation(context.Compilation, referenced.Name)
                        ?? Location.None;

                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticMessages.LayerViolation,
                        location,
                        referenced.Name,
                        referencedLayer.Value.ToString(),
                        currentLayer.Value.ToString()
                    ));
                }
            }
        }

        /// <summary>
        /// Scans every syntax tree for a <c>using</c> directive whose namespace starts with
        /// <paramref name="assemblyName"/>. Pure syntax — no semantic model needed.
        /// </summary>
        private static Location? FindUsingLocation(Compilation compilation, string assemblyName)
        {
            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                foreach (UsingDirectiveSyntax u in tree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>())
                {
                    string name = u.Name?.ToString() ?? string.Empty;
                    if (name == assemblyName ||
                        name.StartsWith(assemblyName + ".", System.StringComparison.Ordinal))
                    {
                        return u.GetLocation();
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Maps an assembly name to its Cosmos kernel layer.
        /// Returns null for assemblies that are not part of the kernel layer hierarchy.
        /// </summary>
        private static KernelLayer? GetLayerFromAssemblyName(string name)
        {
            // Native: Cosmos.Kernel.Native.*
            if (name.StartsWith("Cosmos.Kernel.Native", System.StringComparison.Ordinal))
            {
                return KernelLayer.Native;
            }

            // Core: Cosmos.Kernel.Core only (not HAL, not System)
            if (name == "Cosmos.Kernel.Core")
            {
                return KernelLayer.Core;
            }

            // HAL: Cosmos.Kernel.HAL, Cosmos.Kernel.HAL.X64, Cosmos.Kernel.HAL.ARM64, Cosmos.Kernel.HAL.Interfaces
            if (name.StartsWith("Cosmos.Kernel.HAL", System.StringComparison.Ordinal))
            {
                return KernelLayer.Hal;
            }

            // System: Cosmos.Kernel.System only
            if (name == "Cosmos.Kernel.System")
            {
                return KernelLayer.System;
            }

            // Cosmos.Kernel (aggregator), Cosmos.Kernel.Plugs, Cosmos.Kernel.Debug,
            // Cosmos.Kernel.Boot.*, Cosmos.Build.* etc. → not part of strict layer hierarchy
            return null;
        }

        private static bool IsReferenceAllowed(KernelLayer current, KernelLayer referenced)
        {
            return current switch
            {
                // The HAL is allowed for the driver kit (see the class summary).
                KernelLayer.User => referenced == KernelLayer.System || referenced == KernelLayer.Hal,
                KernelLayer.System => referenced == KernelLayer.Hal,
                KernelLayer.Hal => referenced == KernelLayer.Hal || referenced == KernelLayer.Core,
                KernelLayer.Core => referenced == KernelLayer.Native,
                KernelLayer.Native => false,
                _ => true
            };
        }

    }
}
