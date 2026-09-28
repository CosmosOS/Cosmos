using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Cosmos.Build.Analyzer.Patcher.Extensions;
using Cosmos.Build.Analyzer.Patcher.Models;
using Cosmos.Build.API.Enum;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cosmos.Build.Analyzer.Patcher
{
    /// <summary>
    /// Roslyn diagnostic analyzer for Cosmos NativeAOT patching.
    /// Analyzes plug attributes and member accesses to ensure correct plug implementations
    /// and reports diagnostics for missing plugs, mismatches, and other issues.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PatcherAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The diagnostic ID used by this analyzer.
        /// </summary>
        public const string DiagnosticId = "NAOT";

        /// <summary>
        /// Gets the supported diagnostics for this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            DiagnosticMessages.SupportedDiagnostics;

        /// <summary>
        /// Cache for external types that have already been validated.
        /// </summary>
        private static readonly HashSet<string> s_ignoredExternalTypes = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>
        /// Initializes the analyzer and registers analysis actions.
        /// </summary>
        /// <param name="context">The analysis context.</param>
        public override void Initialize(AnalysisContext context)
        {
            DebugLog("Initializing PatcherAnalyzer");
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                string assemblyName = compilationContext.Compilation?.AssemblyName ?? "<unknown>";
                DebugLog($"Starting compilation analysis for {assemblyName}");
                PlatformArchitecture currentArchitecture = PlatformArchitecture.None;

                if (compilationContext.Options?.AnalyzerConfigOptionsProvider?.GlobalOptions != null)
                {
                    _ = compilationContext.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(
                        "build_property.CosmosArch", out string? archValue) &&
                        Enum.TryParse(archValue, true, out currentArchitecture);
                }

                DebugLog($"Current architecture: {currentArchitecture}");
                var pluggedClasses = new ConcurrentDictionary<string, PlugInfo>();

                compilationContext.RegisterSyntaxNodeAction(
                    nodeContext => AnalyzePlugAttribute(nodeContext, pluggedClasses),
                    SyntaxKind.Attribute);

                // NAOT0002 is guidance for user kernel developers only.
                // Framework assemblies (Cosmos.*) call native methods directly and don't use plugs.
                if (!assemblyName.StartsWith("Cosmos.", StringComparison.Ordinal))
                {
                    compilationContext.RegisterSyntaxNodeAction(
                        nodeContext => AnalyzeAccessedMember(nodeContext, pluggedClasses, currentArchitecture),
                        SyntaxKind.SimpleMemberAccessExpression);

                    compilationContext.RegisterSyntaxTreeAction(
                        treeContext => AnalyzeSyntaxTree(treeContext, pluggedClasses));
                }

                DebugLog($"Registered syntax node actions for compilation {assemblyName}");
            });
        }

        /// <summary>
        /// Analyzes the syntax tree for plugged classes and removes entries
        /// that no longer have syntax references in the current tree.
        /// </summary>
        private void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context,
            ConcurrentDictionary<string, PlugInfo> pluggedClasses)
        {
            DebugLog($"AnalyzeSyntaxTree started for {context.Tree.FilePath}");
            var syntaxRoot = context.Tree.GetRoot(context.CancellationToken);

            foreach (KeyValuePair<string, PlugInfo> kvp in pluggedClasses.ToList())
            {
                if (kvp.Value.PlugSymbol == null || !kvp.Value.PlugSymbol.DeclaringSyntaxReferences.Any(r => r.SyntaxTree == context.Tree))
                {
                    pluggedClasses.TryRemove(kvp.Key, out _);
                    DebugLog($"Removed {kvp.Key} from pluggedClasses due to missing syntax reference");
                }
            }
        }

        /// <summary>
        /// Analyzes member access expressions to check for missing plugs.
        /// </summary>
        private void AnalyzeAccessedMember(
            SyntaxNodeAnalysisContext context,
            ConcurrentDictionary<string, PlugInfo> pluggedClasses,
            PlatformArchitecture currentArchitecture)
        {
            DebugLog($"AnalyzeAccessedMember started at {context.Node.GetLocation().GetMappedLineSpan()}");
            if (context.Node is not MemberAccessExpressionSyntax memberAccess)
            {
                DebugLog("Node is not a MemberAccessExpressionSyntax");
                return;
            }

            // Skip member accesses inside plug class implementations — plug code is kernel code
            // and is free to call any native/platform methods without needing its own plugs.
            ClassDeclarationSyntax? containingClass = memberAccess
                .Ancestors()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();
            if (containingClass != null &&
                containingClass.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Any(a => a.Name.ToString() is "Plug" or "PlugAttribute"))
            {
                DebugLog($"Skipping member access inside plug class {containingClass.Identifier.Text}");
                return;
            }

            if (context.SemanticModel.GetSymbolInfo(memberAccess.Expression).Symbol is not INamedTypeSymbol classSymbol)
            {
                DebugLog("Symbol is not a named type symbol");
                return;
            }

            ISymbol? accessedMemberSymbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol;
            DebugLog($"Checking accessed method {accessedMemberSymbol?.Name} in class {classSymbol.Name}");

            if (accessedMemberSymbol != null && accessedMemberSymbol.HasAttribute("PlatformSpecificAttribute"))
            {
                DebugLog($"Found PlatformSpecificAttribute on {accessedMemberSymbol.Name}");
                AttributeData? platformSpecificAttr = accessedMemberSymbol.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.Name == "PlatformSpecificAttribute");

                PlatformArchitecture? targetPlatform =
                    platformSpecificAttr?.GetArgument<PlatformArchitecture>(named: "Architecture");
                DebugLog($"Found target platform {targetPlatform}");

                if (targetPlatform == null)
                {
                    DebugLog("PlatformSpecificAttribute does not specify an architecture");
                    return;
                }

                if (targetPlatform.Value.HasFlag(currentArchitecture))
                {
                    DebugLog($"Current architecture {currentArchitecture} matches target platform {targetPlatform}");
                    return;
                }

                ReportDiagnostic(context, DiagnosticMessages.MemberCanNotBeUsed,
                    memberAccess.Expression.GetLocation(),
                    null,
                    accessedMemberSymbol.Name,
                    currentArchitecture,
                    targetPlatform,
                   classSymbol.Name
                );
            }

            if (s_ignoredExternalTypes.Contains(classSymbol.Name))
            {
                DebugLog($"Class {classSymbol.Name} is in ignored external types");
                return;
            }

            bool isMethod = accessedMemberSymbol is IMethodSymbol { MethodKind: MethodKind.Ordinary };
            if (!pluggedClasses.TryGetValue(classSymbol.Name, out PlugInfo plugInfo))
            {
                DebugLog($"No plug info found for class {classSymbol.Name}");
                if (!isMethod || accessedMemberSymbol == null || !CheckIfNeedsPlug(accessedMemberSymbol, null))
                {
                    return;
                }

                DebugLog($"Reporting MethodNeedsPlug for {accessedMemberSymbol.Name}");
                ReportDiagnostic(context, DiagnosticMessages.MemberNeedsPlug,
                    memberAccess.Expression.GetLocation(),
                    properties: null,
                    accessedMemberSymbol.Name,
                    classSymbol.Name
                );
                return;
            }

            DebugLog($"Found plugged class {classSymbol.Name}");
            if (!isMethod || accessedMemberSymbol == null || !CheckIfNeedsPlug(accessedMemberSymbol, null))
            {
                return;
            }

            ImmutableDictionary<string, string?> properties = ImmutableDictionary.CreateRange(new[]
            {
                new KeyValuePair<string, string?>("PlugClass", plugInfo.PlugSymbol?.Name)
            });
            DebugLog($"Reporting MethodNeedsPlug for {accessedMemberSymbol.Name}");

            ReportDiagnostic(context, DiagnosticMessages.MemberNeedsPlug,
                memberAccess.Expression.GetLocation(),
                properties,
                accessedMemberSymbol.Name,
                classSymbol.Name
            );
        }

        /// <summary>
        /// Analyzes Plug attributes on classes.
        /// </summary>
        private void AnalyzePlugAttribute(SyntaxNodeAnalysisContext context,
            ConcurrentDictionary<string, PlugInfo> pluggedClasses)
        {
            DebugLog($"AnalyzePlugAttribute started at {context.Node.GetLocation().GetMappedLineSpan()}");
            if (context.Node is not AttributeSyntax attribute || attribute.Name.ToString() != "Plug")
            {
                DebugLog("Node is not Plug attribute");
                return;
            }

            ClassDeclarationSyntax? plugClass = attribute
                .Ancestors()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();

            if (plugClass == null)
            {
                DebugLog("No containing class found for Plug attribute");
                return;
            }

            DebugLog($"Found Plug attribute on class {plugClass.Identifier.Text}");
            _ = attribute.GetArgument(context, "TargetName", 0, out string? targetName) ||
                                attribute.GetArgument(context, "Target", 0, out targetName);


            DebugLog($"Resolved targetName: {targetName}");
            if (string.IsNullOrEmpty(targetName))
            {
                DebugLog("targetName is null or empty");
                return;
            }

            // Non-null guaranteed by IsNullOrEmpty guard above.
            string name = targetName!;

            // Strip assembly qualifier if present ("Full.Type.Name, AssemblyName")
            if (name.Contains(','))
            {
                name = name.Split(',')[0].Trim();
            }

            // Derive simple class name from the fully-qualified target name.
            // Handles dot-separated namespaces and '/' nested-type separators.
            string simpleClassName = name.Replace('/', '.').Split('.')[^1];

            DebugLog($"Checking plug class name for target {name} (simple: {simpleClassName})");
            AnalyzePlugClass(plugClass, simpleClassName, context);

            // Resolve the target type and analyse plug members (NAOT0005, NAOT0006, …)
            string metadataName = name.Replace('/', '+');
            INamedTypeSymbol? targetSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(metadataName);
            AnalyzePluggedClass(targetSymbol, plugClass, context, pluggedClasses);
        }

        /// <summary>
        /// Checks if the plug class name matches the expected convention.
        /// </summary>
        private void AnalyzePlugClass(ClassDeclarationSyntax classDeclarationSyntax, string pluggedClassName,
            SyntaxNodeAnalysisContext context)
        {
            DebugLog($"AnalyzePlugClass for {classDeclarationSyntax.Identifier.Text}");
            if (string.IsNullOrEmpty(pluggedClassName))
            {
                return;
            }

            string expectedName = $"{pluggedClassName}Impl";
            DebugLog($"Expected plug class name: {expectedName}");
            if (classDeclarationSyntax.Identifier.Text == expectedName)
            {
                return;
            }

            DebugLog($"Reporting PlugNameMismatch for {classDeclarationSyntax.Identifier.Text}");
            ReportDiagnostic(
                context,
                DiagnosticMessages.PlugNameDoesNotMatch,
                classDeclarationSyntax.GetLocation(),
                ImmutableDictionary.CreateRange(new[]
                {
                    new KeyValuePair<string, string?>("ExpectedName", expectedName)
                }),
                classDeclarationSyntax.Identifier.Text,
                expectedName);
        }

        /// <summary>
        /// Adds the plug info to the dictionary and analyzes its members.
        /// </summary>
        private void AnalyzePluggedClass(INamedTypeSymbol? symbol, ClassDeclarationSyntax? plugClass,
            SyntaxNodeAnalysisContext context, ConcurrentDictionary<string, PlugInfo> pluggedClasses)
        {
            DebugLog($"AnalyzePluggedClass for {symbol?.Name}");
            if (plugClass == null || symbol == null)
            {
                DebugLog("plugClass or symbol is null");
                return;
            }

            bool targetExternal = symbol.ContainingAssembly.Name != context.Compilation.Assembly.Name;
            INamedTypeSymbol? plugSymbol = context.SemanticModel.GetDeclaredSymbol(plugClass);

            if (plugSymbol != null)
            {
                AnalyzePluggedClassMembers(plugClass, plugSymbol, symbol, context, pluggedClasses);
            }
            if (!pluggedClasses.TryAdd(symbol.Name, new PlugInfo(targetExternal, plugSymbol)))
            {
                DebugLog($"Plug info for {symbol.Name} already exists in pluggedClasses");
                return;
            }

            DebugLog($"Added {symbol.Name} to pluggedClasses");
        }

        /// <summary>
        /// Analyzes the members of the plugged class for missing implementations.
        /// </summary>
        private void AnalyzePluggedClassMembers(ClassDeclarationSyntax plugClass, INamedTypeSymbol plugSymbol,
            INamedTypeSymbol symbol, SyntaxNodeAnalysisContext context,
            ConcurrentDictionary<string, PlugInfo> pluggedClasses)
        {
            DebugLog($"AnalyzePluggedClassMembers for {symbol.Name}");
            if (plugSymbol == null || s_ignoredExternalTypes.Contains(symbol.Name))
            {
                return;
            }

            List<IMethodSymbol> methods = symbol.GetMembers().OfType<IMethodSymbol>().ToList();
            bool targetInvalid = false;

            DebugLog($"Checking {methods.Count()} methods");
            foreach (IMethodSymbol method in methods)
            {
                DebugLog($"Checking method {method.Name}");
                if (method is IMethodSymbol { MethodKind: not MethodKind.Ordinary })
                {
                    DebugLog($"Skipping non-ordinary method {method.Name}");
                    continue;
                }

                if (!CheckIfNeedsPlug(method, plugSymbol))
                {
                    continue;
                }

                targetInvalid = true;
                DebugLog($"Reporting MethodNeedsPlug for {method.Name}");

                ReportDiagnostic(
                    context,
                    DiagnosticMessages.MemberNeedsPlug,
                    plugClass.GetLocation(),
                    ImmutableDictionary.CreateRange(new[]
                {
                        new KeyValuePair<string, string?>("ClassName", plugClass.Identifier.Text),
                        new KeyValuePair<string, string?>("MethodName", method.Name)
                    }),
                    method.Name,
                    symbol.Name);


                if (pluggedClasses.TryGetValue(symbol.Name, out PlugInfo plugInfo) && plugInfo.TargetExternal && !targetInvalid)
                {
                    s_ignoredExternalTypes.Add(symbol.Name); // Cache valid external types
                }
            }

            AnalyzePluggedClassCtors(plugClass, symbol, methods, context);
            foreach (MemberDeclarationSyntax member in plugClass.Members)
            {
                string? name = member.GetName();
                if (name is "Ctor" or "CCtor" || member.HasAttribute("PlugMemberAttribute") ||
                    methods.Any(x => x is IMethodSymbol method
                        ? method.MethodKind == MethodKind.Ordinary && method.Name == name
                        : x.Name == name))
                {
                    continue;
                }

                DebugLog($"Reporting MethodNotImplemented for {name}");
                ReportDiagnostic(
                    context,
                    DiagnosticMessages.MethodNotImplemented,
                    plugClass.GetLocation(),
                    null,
                    name,
                    symbol.Name);
            }
        }

        /// <summary>
        /// Analyzes constructors in the plugged class.
        /// </summary>
        private void AnalyzePluggedClassCtors(ClassDeclarationSyntax plugClass, INamedTypeSymbol symbol,
            List<IMethodSymbol> methods, SyntaxNodeAnalysisContext context)
        {
            DebugLog($"AnalyzePluggedClassCtors for {symbol.Name}");
            if (plugClass.TryGetMemberByName("CCtor", out MethodDeclarationSyntax? cctor))
            {
                if (methods.All(x => x.MethodKind != MethodKind.StaticConstructor))
                {
                    DebugLog("Reporting missing static constructor");
                    ReportDiagnostic(
                        context,
                        DiagnosticMessages.MethodNotImplemented,
                        plugClass.GetLocation(),
                        null,
                        "CCtor(.cctor)",
                        symbol.Name);
                    return;
                }
                DebugLog("Found CCtor method");
                DebugLog($"CCtor has {cctor!.ParameterList.Parameters.Count} parameters");
                if (cctor!.ParameterList.Parameters.Count <= 1)
                {
                    return;
                }

                DebugLog("Reporting static constructor too many params");
                ReportDiagnostic(
                    context,
                    DiagnosticMessages.StaticConstructorTooManyParams,
                    plugClass.GetLocation(),
                    null,
                    cctor?.Identifier.Text);
            }
            else if (plugClass.TryGetMemberByName("Ctor", out ClassDeclarationSyntax? ctor) &&
                     methods.All(x => x.Name != ".ctor"))
            {
                DebugLog("Reporting missing instance constructor");
                ReportDiagnostic(
                    context,
                    DiagnosticMessages.MethodNotImplemented,
                    ctor?.Identifier.GetLocation(),
                    null,
                    ".ctor",
                    symbol.Name);
            }
        }

        /// <summary>
        /// Checks if a symbol needs a plug implementation.
        /// </summary>
        private static bool CheckIfNeedsPlug(ISymbol symbol, INamedTypeSymbol? plugSymbol)
        {
            DebugLog($"CheckIfNeedsPlug for {symbol.Name}");

            bool hasMethod = plugSymbol?.GetMembers(symbol.Name)
                                 .Any(m => m.HasAttribute("PlugMemberAttribute")) ??
                             false;

            if (hasMethod)
            {
                return false;
            }

            AttributeData? pInvoke = symbol.GetAttributes().FirstOrDefault(a =>
                a.AttributeClass?.Name is "DllImportAttribute" or "LibraryImportAttribute");

            if (pInvoke != null)
            {
                // Module "*" is a direct P/Invoke resolved by the kernel link step
                // (GCCProject objects, native libraries) — no plug involved. Imports
                // from a real library file can never load on bare metal, so those
                // still need a plug.
                bool isStaticallyLinked = pInvoke.ConstructorArguments.FirstOrDefault().Value is "*";
                DebugLog($"[DEBUG] P/Invoke on {symbol.Name}, staticallyLinked: {isStaticallyLinked}");
                return !isStaticallyLinked;
            }

            // [MethodImpl] marks a plug candidate only for runtime-provided icalls;
            // flags like AggressiveInlining sit on ordinary managed bodies.
            AttributeData? methodImpl = symbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "MethodImplAttribute");

            return methodImpl != null && IsInternalCall(methodImpl);
        }

        /// <summary>
        /// Checks if a [MethodImpl] attribute carries MethodImplOptions.InternalCall.
        /// </summary>
        private static bool IsInternalCall(AttributeData methodImpl)
        {
            int options = methodImpl.ConstructorArguments.FirstOrDefault().Value switch
            {
                short shortOptions => shortOptions,
                int intOptions => intOptions,
                _ => 0
            };

            return (options & (int)MethodImplOptions.InternalCall) != 0;
        }

        /// <summary>
        /// Writes debug log messages if DEBUG is defined. The trace listener, not the
        /// console: an analyzer runs inside the compiler server, whose console is not its own.
        /// </summary>
        private static void DebugLog(string message, [CallerMemberName] string memberName = "")
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[DEBUG] [{memberName}]: {message}");
#endif
        }

        /// <summary>
        /// Reports a diagnostic to the Roslyn context.
        /// </summary>
        private static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            DiagnosticDescriptor descriptor,
            Location? location,
            ImmutableDictionary<string, string?>? properties,
            params object?[] messageArgs)
        {
            try
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    location,
                    properties,
                    messageArgs));
            }
            catch (Exception ex)
            {
                DebugLog($"Exception while reporting diagnostic: {ex}");
            }
        }

        private static void SafeInvoke(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                DebugLog($"Exception in SafeInvoke: {ex}");
            }
        }
    }
}
