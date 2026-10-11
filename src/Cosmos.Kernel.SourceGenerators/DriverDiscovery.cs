// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cosmos.Kernel.SourceGenerators;

/// <summary>
/// Finds the classes marked <c>[Driver]</c> a kernel can register: its own,
/// from the compilation's syntax, and those of the assemblies it references
/// that themselves reference <c>Cosmos.Kernel.HAL</c>. Each class is
/// inspected once, here, and reduced to a <see cref="DriverCandidate"/>.
/// </summary>
internal static class DriverDiscovery
{
    /// <summary>Metadata name of the attribute that marks a driver class.</summary>
    public const string DriverAttributeMetadataName = "Cosmos.Kernel.HAL.DriverKit.DriverAttribute";

    private const string HalAssemblyName = "Cosmos.Kernel.HAL";
    private const string DriverBaseTypeName = "Cosmos.Kernel.HAL.DriverKit.Driver";
    private const string KernelFeaturesMetadataName = "Cosmos.Kernel.System.KernelFeatures";
    private const string FeatureArgumentName = "Feature";
    private const string DefaultArgumentName = "Default";
    private const string NoFeatureMemberName = "None";

    /// <summary>Full names without <c>global::</c>, nested types joined with dots: the form policy items use.</summary>
    private static readonly SymbolDisplayFormat s_fullNameFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);

    /// <summary>
    /// Reduces one attributed class of the kernel's own source to a candidate.
    /// Called by <c>ForAttributeWithMetadataName</c> for each declaration
    /// carrying the attribute.
    /// </summary>
    /// <param name="context">The declaration, its symbol and the attribute.</param>
    /// <param name="cancellationToken">Cancels the inspection.</param>
    public static DriverCandidate FromSource(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        INamedTypeSymbol type = (INamedTypeSymbol)context.TargetSymbol;
        AttributeData attribute = context.Attributes[0];
        SyntaxNode node = context.TargetNode;
        Location classLocation = node is BaseTypeDeclarationSyntax declaration ? declaration.Identifier.GetLocation() : node.GetLocation();
        Location? attributeLocation = attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation();
        return Inspect(
            type,
            attribute,
            context.SemanticModel.Compilation,
            node.SyntaxTree.FilePath,
            node.SpanStart,
            SourceLocationInfo.From(classLocation),
            SourceLocationInfo.From(attributeLocation));
    }

    /// <summary>
    /// Finds the attributed classes of every referenced assembly that
    /// references <c>Cosmos.Kernel.HAL</c> (or is it), keeping those the
    /// kernel assembly can see: public ones, and internal ones under an
    /// <c>InternalsVisibleTo</c> grant. Sorted by assembly name, then full
    /// type name, both ordinal.
    /// </summary>
    /// <param name="compilation">The kernel compilation.</param>
    /// <param name="cancellationToken">Cancels the walk.</param>
    public static EquatableArray<DriverCandidate> FromReferences(Compilation compilation, CancellationToken cancellationToken)
    {
        ImmutableArray<DriverCandidate>.Builder found = ImmutableArray.CreateBuilder<DriverCandidate>();
        foreach (IAssemblySymbol assembly in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!SeesDriverKit(assembly))
            {
                continue;
            }

            CollectNamespace(assembly.GlobalNamespace, assembly, compilation, found, cancellationToken);
        }

        found.Sort(CompareReferenced);
        return new EquatableArray<DriverCandidate>(found.ToImmutable());
    }

    private static int CompareReferenced(DriverCandidate left, DriverCandidate right)
    {
        int byAssembly = string.CompareOrdinal(left.AssemblyName, right.AssemblyName);
        return byAssembly != 0 ? byAssembly : string.CompareOrdinal(left.FullName, right.FullName);
    }

    private static bool SeesDriverKit(IAssemblySymbol assembly)
    {
        if (assembly.Name == HalAssemblyName)
        {
            return true;
        }

        foreach (IModuleSymbol module in assembly.Modules)
        {
            foreach (AssemblyIdentity referenced in module.ReferencedAssemblies)
            {
                if (referenced.Name == HalAssemblyName)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void CollectNamespace(
        INamespaceSymbol namespaceSymbol,
        IAssemblySymbol assembly,
        Compilation compilation,
        ImmutableArray<DriverCandidate>.Builder found,
        CancellationToken cancellationToken)
    {
        foreach (INamespaceOrTypeSymbol member in namespaceSymbol.GetMembers())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (member is INamespaceSymbol nested)
            {
                CollectNamespace(nested, assembly, compilation, found, cancellationToken);
            }
            else if (member is INamedTypeSymbol type)
            {
                CollectType(type, assembly, compilation, found);
            }
        }
    }

    private static void CollectType(INamedTypeSymbol type, IAssemblySymbol assembly, Compilation compilation, ImmutableArray<DriverCandidate>.Builder found)
    {
        if (type.TypeKind == TypeKind.Class && compilation.IsSymbolAccessibleWithin(type, compilation.Assembly))
        {
            AttributeData? attribute = FindDriverAttribute(type);
            if (attribute is not null)
            {
                found.Add(Inspect(type, attribute, compilation, assembly.Name, 0, null, null));
            }
        }

        foreach (INamedTypeSymbol nested in type.GetTypeMembers())
        {
            CollectType(nested, assembly, compilation, found);
        }
    }

    /// <summary>The <c>[Driver]</c> attribute on <paramref name="type"/>, or null when it carries none.</summary>
    private static AttributeData? FindDriverAttribute(INamedTypeSymbol type)
    {
        foreach (AttributeData candidate in type.GetAttributes())
        {
            if (candidate.AttributeClass is not null && candidate.AttributeClass.ToDisplayString(s_fullNameFormat) == DriverAttributeMetadataName)
            {
                return candidate;
            }
        }

        return null;
    }

    private static DriverCandidate Inspect(
        INamedTypeSymbol type,
        AttributeData attribute,
        Compilation compilation,
        string orderPath,
        int orderPosition,
        SourceLocationInfo? classLocation,
        SourceLocationInfo? attributeLocation)
    {
        bool isDefault = true;
        string? featureGuard = null;
        string? unmappedFeature = null;
        foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
        {
            switch (argument.Key)
            {
                case DefaultArgumentName:
                    if (argument.Value.Value is bool value)
                    {
                        isDefault = value;
                    }

                    break;
                case FeatureArgumentName:
                    ResolveFeature(argument.Value, compilation, out featureGuard, out unmappedFeature);
                    break;
                default:
                    break;
            }
        }

        return new DriverCandidate(
            type.ToDisplayString(s_fullNameFormat),
            type.ContainingAssembly.Name,
            isDefault,
            featureGuard,
            unmappedFeature,
            SkipReasonOf(type, compilation),
            orderPath,
            orderPosition,
            classLocation,
            attributeLocation);
    }

    private static void ResolveFeature(TypedConstant constant, Compilation compilation, out string? featureGuard, out string? unmappedFeature)
    {
        featureGuard = null;
        unmappedFeature = null;
        if (constant.Kind == TypedConstantKind.Error || constant.Value is null)
        {
            // The compiler already reports the bad argument.
            return;
        }

        string? memberName = EnumMemberName(constant);
        if (memberName is null)
        {
            unmappedFeature = constant.Value.ToString();
            return;
        }

        if (memberName == NoFeatureMemberName)
        {
            return;
        }

        if (HasFeatureProperty(compilation, memberName))
        {
            featureGuard = memberName;
            return;
        }

        unmappedFeature = memberName;
    }

    private static string? EnumMemberName(TypedConstant constant)
    {
        if (constant.Type is not INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType)
        {
            return null;
        }

        foreach (ISymbol member in enumType.GetMembers())
        {
            if (member is IFieldSymbol { HasConstantValue: true } field && Equals(field.ConstantValue, constant.Value))
            {
                return field.Name;
            }
        }

        return null;
    }

    private static bool HasFeatureProperty(Compilation compilation, string memberName)
    {
        INamedTypeSymbol? features = compilation.GetTypeByMetadataName(KernelFeaturesMetadataName);
        if (features is null)
        {
            return false;
        }

        foreach (ISymbol member in features.GetMembers(memberName))
        {
            if (member is IPropertySymbol { IsStatic: true, DeclaredAccessibility: Accessibility.Public, GetMethod: not null } property
                && property.Type.SpecialType == SpecialType.System_Boolean)
            {
                return true;
            }
        }

        return false;
    }

    private static DriverSkipReason SkipReasonOf(INamedTypeSymbol type, Compilation compilation)
    {
        if (type.IsStatic)
        {
            return DriverSkipReason.Static;
        }

        if (type.IsAbstract)
        {
            return DriverSkipReason.Abstract;
        }

        if (IsGeneric(type))
        {
            return DriverSkipReason.Generic;
        }

        if (!DerivesFromDriver(type))
        {
            return DriverSkipReason.NotADriver;
        }

        if (!compilation.IsSymbolAccessibleWithin(type, compilation.Assembly))
        {
            return DriverSkipReason.Inaccessible;
        }

        if (!HasAccessibleParameterlessConstructor(type, compilation))
        {
            return DriverSkipReason.NoConstructor;
        }

        return DriverSkipReason.None;
    }

    private static bool IsGeneric(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
        {
            if (current.IsGenericType)
            {
                return true;
            }
        }

        return false;
    }

    private static bool DerivesFromDriver(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.ToDisplayString(s_fullNameFormat) == DriverBaseTypeName)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasAccessibleParameterlessConstructor(INamedTypeSymbol type, Compilation compilation)
    {
        foreach (IMethodSymbol constructor in type.InstanceConstructors)
        {
            if (constructor.Parameters.Length == 0 && compilation.IsSymbolAccessibleWithin(constructor, compilation.Assembly))
            {
                return true;
            }
        }

        return false;
    }
}
