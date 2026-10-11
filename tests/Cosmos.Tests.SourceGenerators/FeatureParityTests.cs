// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Cosmos.Tests.SourceGenerators;

/// <summary>
/// The generator turns a <c>DriverFeature</c> member into a read of the
/// <c>KernelFeatures</c> property of the same name, so the two lists in the
/// repository must agree, and the stubs the other tests compile against must
/// list what the repository lists. The repository files are read from the
/// checkout and parsed with Roslyn, like the stub texts, so an explicit
/// value, a missing trailing comma or a documentation comment cannot hide a
/// member from the comparison.
/// </summary>
public sealed class FeatureParityTests
{
    private const string DriverFeaturePath = "src/Cosmos.Kernel.HAL/DriverKit/DriverFeature.cs";
    private const string KernelFeaturesPath = "src/Cosmos.Kernel.System/KernelFeatures.cs";
    private const string RootMarker = "Directory.Build.props";
    private const string EnumName = "DriverFeature";
    private const string FeaturesClassName = "KernelFeatures";
    private const string NoFeature = "None";

    [Fact]
    public void WhenDriverFeatureIsParsed_DeclaresNoneAndAtLeastOneFeature()
    {
        List<string> features = DriverFeatureMembers(ParseRepositoryFile(DriverFeaturePath));

        Assert.Contains(NoFeature, features);
        Assert.True(features.Count >= 2, $"{EnumName} declares no feature besides {NoFeature}.");
    }

    [Fact]
    public void EveryDriverFeatureExceptNone_HasAKernelFeaturesProperty()
    {
        List<string> features = DriverFeatureMembers(ParseRepositoryFile(DriverFeaturePath));
        List<string> properties = FeatureProperties(ParseRepositoryFile(KernelFeaturesPath));

        foreach (string feature in features)
        {
            if (feature == NoFeature)
            {
                continue;
            }

            Assert.Contains(feature, properties);
        }
    }

    [Fact]
    public void WhenStubDriverFeatureIsParsed_ListsTheSameMembersAsTheKit()
    {
        List<string> kit = DriverFeatureMembers(ParseRepositoryFile(DriverFeaturePath));
        List<string> stub = DriverFeatureMembers(CSharpSyntaxTree.ParseText(KitStubs.Hal("internal")));

        Assert.Equal(kit, stub);
    }

    [Fact]
    public void WhenStubKernelFeaturesIsParsed_ListsTheSamePropertiesAsTheRing()
    {
        List<string> ring = FeatureProperties(ParseRepositoryFile(KernelFeaturesPath));
        List<string> stub = FeatureProperties(CSharpSyntaxTree.ParseText(KitStubs.System));

        Assert.Equal(ring, stub);
    }

    /// <summary>
    /// The member names of the <c>DriverFeature</c> enum in <paramref name="tree"/>,
    /// in declaration order: the generator resolves a feature by its constant
    /// value, so a stub that reorders the members would stand for a different enum.
    /// </summary>
    private static List<string> DriverFeatureMembers(SyntaxTree tree)
    {
        EnumDeclarationSyntax declaration = Assert.Single(
            tree.GetRoot().DescendantNodes().OfType<EnumDeclarationSyntax>(),
            candidate => candidate.Identifier.ValueText == EnumName);
        return declaration.Members.Select(member => member.Identifier.ValueText).ToList();
    }

    /// <summary>
    /// The names of the <c>KernelFeatures</c> properties in <paramref name="tree"/>
    /// that the generator accepts as a guard: public, static, of type
    /// <c>bool</c> and readable. Sorted ordinally, since the order of the
    /// properties carries no meaning.
    /// </summary>
    private static List<string> FeatureProperties(SyntaxTree tree)
    {
        ClassDeclarationSyntax declaration = Assert.Single(
            tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>(),
            candidate => candidate.Identifier.ValueText == FeaturesClassName);
        List<string> names = declaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .Where(IsReadablePublicStaticBool)
            .Select(property => property.Identifier.ValueText)
            .ToList();
        names.Sort(string.CompareOrdinal);
        return names;
    }

    private static bool IsReadablePublicStaticBool(PropertyDeclarationSyntax property) =>
        property.Modifiers.Any(SyntaxKind.PublicKeyword)
        && property.Modifiers.Any(SyntaxKind.StaticKeyword)
        && property.Type is PredefinedTypeSyntax predefined
        && predefined.Keyword.IsKind(SyntaxKind.BoolKeyword)
        && (property.ExpressionBody is not null || HasGetter(property));

    private static bool HasGetter(PropertyDeclarationSyntax property) =>
        property.AccessorList is not null
        && property.AccessorList.Accessors.Any(accessor => accessor.IsKind(SyntaxKind.GetAccessorDeclaration));

    private static SyntaxTree ParseRepositoryFile(string relativePath)
    {
        string path = Path.Combine(RepositoryRoot(), relativePath);
        return CSharpSyntaxTree.ParseText(File.ReadAllText(path), path: path);
    }

    private static string RepositoryRoot()
    {
        for (DirectoryInfo? directory = new(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, RootMarker)))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException($"No directory above {AppContext.BaseDirectory} contains {RootMarker}.");
    }
}
