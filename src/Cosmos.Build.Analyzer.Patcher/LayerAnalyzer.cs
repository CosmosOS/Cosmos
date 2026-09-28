// This code is licensed under the BSD 3-Clause license (see LICENSE for details)
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cosmos.Build.Analyzer.Patcher;

/// <summary>
/// Roslyn diagnostic analyzer that enforces Cosmos kernel layer dependencies.
/// The layer is inferred from the assembly name, with one MSBuild marker: a compilation
/// whose <c>build_property.CosmosDriverAssembly</c> is <c>true</c> is a driver assembly
/// and sits in the User layer whatever its name.
/// Plug assemblies (any assembly that declares a <c>[Plug]</c> class) are exempt and
/// may reference all layers freely.
/// <para>
/// A project is judged on the types and members its code names, not on the reference
/// list restore hands the compiler: a package or project reference drags its own
/// references in transitively, and a Core assembly nothing in the source names is not a
/// dependency. Each assembly used across a layer boundary is reported once, at its
/// first use in source order. Generated code is judged too: what a generator emits
/// names a lower layer exactly as hand-written code would.
/// </para>
/// <para>
/// Allowed references (strict, no skipping layers):
/// <list type="bullet">
///   <item>User     -> System, and the two HAL assemblies the public surface names: Cosmos.Kernel.HAL (the driver kit's host) and Cosmos.Kernel.HAL.Interfaces (the device contracts)</item>
///   <item>System   -> HAL</item>
///   <item>HAL      -> HAL, Core</item>
///   <item>Core     -> Native</item>
///   <item>Native   -> (nothing)</item>
/// </list>
/// </para>
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LayerAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The HAL assembly that carries the driver kit seam: a User layer project may name
    /// its types. The arch HAL assemblies are reached through it only.
    /// </summary>
    private const string DriverKitHostAssemblyName = "Cosmos.Kernel.HAL";

    /// <summary>
    /// The HAL assembly that holds the device contracts the ring and the driver kit name
    /// in their public surface (<c>IBlockDevice</c>, <c>MACAddress</c> and the rest): a
    /// User layer project names one of them whenever it touches such a member.
    /// </summary>
    private const string DeviceContractsAssemblyName = "Cosmos.Kernel.HAL.Interfaces";

    /// <summary>
    /// The analyzer config key behind <c>&lt;CosmosDriverAssembly&gt;</c>, made visible by
    /// the <c>CompilerVisibleProperty</c> the analyzer package's build props declare (an
    /// in-tree project that references the analyzer project declares it itself).
    /// </summary>
    private const string DriverAssemblyPropertyKey = "build_property.CosmosDriverAssembly";

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
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(AnalyzeCompilationStart);
    }

    /// <summary>
    /// Returns true for assemblies named *.Plugs: plug projects bridge all layers
    /// and are exempt from layer checks by convention.
    /// </summary>
    private static bool IsPlugAssembly(string assemblyName)
        => assemblyName.EndsWith(".Plugs", System.StringComparison.Ordinal)
        || assemblyName.Equals("Plugs", System.StringComparison.Ordinal);

    /// <summary>
    /// Returns true when the project declares <c>&lt;CosmosDriverAssembly&gt;true&lt;/CosmosDriverAssembly&gt;</c>.
    /// </summary>
    internal static bool IsDriverAssembly(AnalyzerOptions options)
    {
        return options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(DriverAssemblyPropertyKey, out string? value)
            && string.Equals(value, "true", System.StringComparison.OrdinalIgnoreCase);
    }

    private static void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
    {
        KernelLayer? currentLayer = GetCurrentLayer(context.Compilation, context.Options);
        if (currentLayer == null || !ReferencesAForbiddenLayer(context.Compilation, currentLayer.Value))
        {
            // A name can only reach an assembly on the reference list: a project whose
            // list holds nothing it may not name costs nothing to bind.
            return;
        }

        LayerUses uses = new(currentLayer.Value);
        context.RegisterSyntaxNodeAction(uses.RecordName, SyntaxKind.IdentifierName, SyntaxKind.GenericName);
        context.RegisterCompilationEndAction(uses.Report);
    }

    /// <summary>
    /// The layer the compilation is held to, or null when it is exempt: a plug assembly,
    /// a Cosmos.* assembly outside the layer hierarchy, or a non-Cosmos assembly that
    /// references no layer assembly at all.
    /// </summary>
    private static KernelLayer? GetCurrentLayer(Compilation compilation, AnalyzerOptions options)
    {
        if (IsDriverAssembly(options))
        {
            // A driver assembly is a User layer project whatever it is called: the
            // compiler must hold it to what a third-party driver library can reach.
            return KernelLayer.User;
        }

        if (IsPlugAssembly(compilation.Assembly.Name))
        {
            return null;
        }

        KernelLayer? currentLayer = GetLayerFromAssemblyName(compilation.Assembly.Name);
        if (currentLayer != null)
        {
            return currentLayer;
        }

        // Non-Cosmos assemblies that reference at least one Cosmos layer assembly are user kernels.
        // Cosmos.* assemblies that are not a recognised layer (aggregator, Plugs, Debug, Boot...) are skipped.
        bool isUserKernel =
            !compilation.Assembly.Name.StartsWith("Cosmos.", System.StringComparison.Ordinal)
            && compilation.ReferencedAssemblyNames.Any(r => GetLayerFromAssemblyName(r.Name) != null);

        return isUserKernel ? KernelLayer.User : null;
    }

    /// <summary>
    /// True when at least one referenced assembly sits in a layer <paramref name="currentLayer"/>
    /// may not name.
    /// </summary>
    private static bool ReferencesAForbiddenLayer(Compilation compilation, KernelLayer currentLayer)
    {
        foreach (AssemblyIdentity referenced in compilation.ReferencedAssemblyNames)
        {
            KernelLayer? referencedLayer = GetLayerFromAssemblyName(referenced.Name);
            if (referencedLayer != null && !IsReferenceAllowed(currentLayer, referencedLayer.Value, referenced.Name))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The assembly a bound name reaches into: the containing assembly of the type or
    /// member it names, through aliases, arrays and pointers. Null for a namespace, a
    /// type parameter and everything declared by the code itself (locals, parameters,
    /// labels, range variables). <c>var</c> binds to the inferred type on purpose: a
    /// local of a lower layer's type is a dependency the metadata records, whether or
    /// not the source spells the type out.
    /// </summary>
    private static IAssemblySymbol? ReferencedAssembly(ISymbol? symbol)
    {
        return symbol switch
        {
            null => null,
            IAliasSymbol alias => ReferencedAssembly(alias.Target),
            INamespaceSymbol => null,
            ITypeParameterSymbol => null,
            IDynamicTypeSymbol => null,
            IArrayTypeSymbol array => ReferencedAssembly(array.ElementType),
            IPointerTypeSymbol pointer => ReferencedAssembly(pointer.PointedAtType),
            ITypeSymbol type => type.ContainingAssembly,
            IMethodSymbol or IFieldSymbol or IPropertySymbol or IEventSymbol => symbol.ContainingAssembly,
            _ => null
        };
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
        // Cosmos.Kernel.Boot.*, Cosmos.Build.* etc. -> not part of strict layer hierarchy
        return null;
    }

    private static bool IsReferenceAllowed(KernelLayer current, KernelLayer referenced, string referencedName)
    {
        return current switch
        {
            KernelLayer.User => referenced == KernelLayer.System
                || referencedName == DriverKitHostAssemblyName
                || referencedName == DeviceContractsAssemblyName,
            KernelLayer.System => referenced == KernelLayer.Hal,
            KernelLayer.Hal => referenced == KernelLayer.Hal || referenced == KernelLayer.Core,
            KernelLayer.Core => referenced == KernelLayer.Native,
            KernelLayer.Native => false,
            _ => true
        };
    }

    /// <summary>
    /// One compilation's crossings: for every assembly the code names across a layer
    /// boundary, the earliest use in source order, reported once at compilation end.
    /// </summary>
    private sealed class LayerUses
    {
        private readonly KernelLayer _currentLayer;
        private readonly ConcurrentDictionary<string, Location> _firstUseByAssembly =
            new(System.StringComparer.Ordinal);

        public LayerUses(KernelLayer currentLayer)
        {
            _currentLayer = currentLayer;
        }

        public void RecordName(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.IsPartOfStructuredTrivia())
            {
                // A cref in a doc comment or a name in a #if directive emits nothing.
                return;
            }

            ISymbol? symbol = context.SemanticModel.GetSymbolInfo(context.Node, context.CancellationToken).Symbol;
            IAssemblySymbol? assembly = ReferencedAssembly(symbol);
            if (assembly == null || SymbolEqualityComparer.Default.Equals(assembly, context.Compilation.Assembly))
            {
                return;
            }

            KernelLayer? referencedLayer = GetLayerFromAssemblyName(assembly.Name);
            if (referencedLayer == null || IsReferenceAllowed(_currentLayer, referencedLayer.Value, assembly.Name))
            {
                return;
            }

            Location location = context.Node.GetLocation();
            _firstUseByAssembly.AddOrUpdate(
                assembly.Name,
                location,
                (_, existing) => IsBefore(location, existing) ? location : existing);
        }

        public void Report(CompilationAnalysisContext context)
        {
            foreach (KeyValuePair<string, Location> use in _firstUseByAssembly.OrderBy(p => p.Key, System.StringComparer.Ordinal))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticMessages.LayerViolation,
                    use.Value,
                    use.Key,
                    GetLayerFromAssemblyName(use.Key)!.Value.ToString(),
                    _currentLayer.ToString()
                ));
            }
        }

        /// <summary>Source order: by file path, then by position in the file.</summary>
        private static bool IsBefore(Location candidate, Location existing)
        {
            int byPath = string.CompareOrdinal(candidate.SourceTree?.FilePath, existing.SourceTree?.FilePath);
            return byPath < 0 || (byPath == 0 && candidate.SourceSpan.Start < existing.SourceSpan.Start);
        }
    }
}
