// This code is licensed under the BSD 3-Clause license (see LICENSE for details)
using System.Collections.Immutable;
using Cosmos.Build.Analyzer.Patcher;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Cosmos.Tests.Build.Analyzer.Patcher;

public class DriverAssemblyAnalyzerTests
{
    private const string DriverAssemblyName = "DriverLib";

    private const string UnsafeAccessorCode = """
        using System.Runtime.CompilerServices;

        namespace DriverLib
        {
            public class Target
            {
                private int _count;
            }

            public static class Hatch
            {
                [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_count")]
                public static extern ref int GetCount(Target target);
            }
        }
        """;

    private const string UnsafeAccessorTypeCode = """
        using System.Runtime.CompilerServices;

        namespace DriverLib
        {
            public static class Hatch
            {
                [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "Reset")]
                public static extern void CallReset(
                    [UnsafeAccessorType("Cosmos.Kernel.Core.Hidden, Cosmos.Kernel.Core")] object target);
            }
        }
        """;

    private const string PlainCode = """
        namespace DriverLib
        {
            public class Driver
            {
                public string Name => "plain";
            }
        }
        """;

    private const string GrantingCode = """
        using System.Runtime.CompilerServices;

        [assembly: InternalsVisibleTo("DriverLib")]

        namespace Cosmos.Kernel.Fake
        {
            internal class Hidden
            {
            }
        }
        """;

    private const string NotGrantingCode = """
        namespace Cosmos.Kernel.Fake
        {
            internal class Hidden
            {
            }
        }
        """;

    private const string GrantingAnotherCode = """
        using System.Runtime.CompilerServices;

        [assembly: InternalsVisibleTo("OtherLib")]

        namespace Cosmos.Kernel.Fake
        {
            internal class Hidden
            {
            }
        }
        """;

    private const string PolyfilledUnsafeAccessorCode = """
        namespace System.Runtime.CompilerServices
        {
            internal enum UnsafeAccessorKind
            {
                Field,
            }

            internal sealed class UnsafeAccessorAttribute : System.Attribute
            {
                public UnsafeAccessorAttribute(UnsafeAccessorKind kind)
                {
                }

                public string? Name { get; set; }
            }
        }

        namespace DriverLib
        {
            public class Target
            {
                private int _count;
            }

            public static class Hatch
            {
                [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = "_count")]
                public static extern ref int GetCount(Target target);
            }
        }
        """;

    private static readonly MetadataReference s_corlibReference =
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

    [Fact]
    public async Task UnsafeAccessor_InDriverAssembly_ReportsAtTheAttribute()
    {
        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(UnsafeAccessorCode, [], isDriverAssembly: true);

        Diagnostic diagnostic = Assert.Single(diagnostics, d => d.Id == DiagnosticMessages.DriverAssemblyUsesUnsafeAccessor.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("'GetCount'", diagnostic.GetMessage());
        Assert.Contains("UnsafeAccessorAttribute", diagnostic.GetMessage());
        Assert.NotEqual(Location.None, diagnostic.Location);
        Assert.Contains("UnsafeAccessor(", diagnostic.Location.SourceTree!.GetText().ToString(diagnostic.Location.SourceSpan));
    }

    [Fact]
    public async Task UnsafeAccessorType_InDriverAssembly_ReportsTheEnclosingMethod()
    {
        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(UnsafeAccessorTypeCode, [], isDriverAssembly: true);

        Assert.Contains(diagnostics,
            d => d.Id == DiagnosticMessages.DriverAssemblyUsesUnsafeAccessor.Id
                && d.GetMessage().Contains("'CallReset'")
                && d.GetMessage().Contains("UnsafeAccessorTypeAttribute"));
        Assert.Contains(diagnostics,
            d => d.Id == DiagnosticMessages.DriverAssemblyUsesUnsafeAccessor.Id
                && d.GetMessage().Contains("'CallReset'")
                && d.GetMessage().Contains("[UnsafeAccessorAttribute]"));
    }

    [Fact]
    public async Task UnsafeAccessor_NotADriverAssembly_IsSilent()
    {
        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(UnsafeAccessorCode, [], isDriverAssembly: false);

        Assert.DoesNotContain(diagnostics, d => d.Id == DiagnosticMessages.DriverAssemblyUsesUnsafeAccessor.Id);
    }

    [Fact]
    public async Task DriverAssembly_WithoutUnsafeAccessor_IsSilent()
    {
        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(PlainCode, [], isDriverAssembly: true);

        Assert.DoesNotContain(diagnostics, d => d.Id == DiagnosticMessages.DriverAssemblyUsesUnsafeAccessor.Id);
    }

    [Fact]
    public async Task InternalsGrant_ToDriverAssembly_NamesTheGrantingAssembly()
    {
        MetadataReference granting = FakeAssembly.Emit("Cosmos.Kernel.Fake", GrantingCode);

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(PlainCode, [granting], isDriverAssembly: true);

        Diagnostic diagnostic = Assert.Single(diagnostics, d => d.Id == DiagnosticMessages.DriverAssemblyGrantedInternals.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("'Cosmos.Kernel.Fake'", diagnostic.GetMessage());
        Assert.Equal(Location.None, diagnostic.Location);
    }

    [Fact]
    public async Task InternalsGrant_NotADriverAssembly_IsSilent()
    {
        MetadataReference granting = FakeAssembly.Emit("Cosmos.Kernel.Fake", GrantingCode);

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(PlainCode, [granting], isDriverAssembly: false);

        Assert.DoesNotContain(diagnostics, d => d.Id == DiagnosticMessages.DriverAssemblyGrantedInternals.Id);
    }

    [Fact]
    public async Task DriverAssembly_WithoutGrant_IsSilent()
    {
        MetadataReference notGranting = FakeAssembly.Emit("Cosmos.Kernel.Fake", NotGrantingCode);

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(PlainCode, [notGranting], isDriverAssembly: true);

        Assert.DoesNotContain(diagnostics, d => d.Id == DiagnosticMessages.DriverAssemblyGrantedInternals.Id);
    }

    [Fact]
    public async Task InternalsGrant_ToAnotherAssembly_IsSilent()
    {
        MetadataReference granting = FakeAssembly.Emit("Cosmos.Kernel.Fake", GrantingAnotherCode);

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(PlainCode, [granting], isDriverAssembly: true);

        Assert.DoesNotContain(diagnostics, d => d.Id == DiagnosticMessages.DriverAssemblyGrantedInternals.Id);
    }

    [Fact]
    public async Task UnsafeAccessor_PolyfilledInTheDriverAssembly_Reports()
    {
        // The attribute declared in source shadows the runtime's: caught by its full name all the same.
        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(PolyfilledUnsafeAccessorCode, [], isDriverAssembly: true);

        Diagnostic diagnostic = Assert.Single(diagnostics, d => d.Id == DiagnosticMessages.DriverAssemblyUsesUnsafeAccessor.Id);
        Assert.Contains("'GetCount'", diagnostic.GetMessage());
    }

    [Fact]
    public async Task InternalsGrant_FromANonCosmosAssembly_IsSilent()
    {
        MetadataReference granting = FakeAssembly.Emit("ThirdParty.Lib", GrantingCode);

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(PlainCode, [granting], isDriverAssembly: true);

        Assert.DoesNotContain(diagnostics, d => d.Id == DiagnosticMessages.DriverAssemblyGrantedInternals.Id);
    }

    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(
        string code,
        MetadataReference[] cosmosReferences,
        bool isDriverAssembly)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(DriverAssemblyName)
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(s_corlibReference)
            .AddReferences(cosmosReferences)
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));

        List<(string key, string value)> options = [("build_property.CosmosArch", "X64")];
        if (isDriverAssembly)
        {
            options.Add(("build_property.CosmosDriverAssembly", "true"));
        }

        DriverAssemblyAnalyzer analyzer = new();
        CompilationWithAnalyzers compilationWithAnalyzers = compilation.WithAnalyzers(
            [analyzer],
            new AnalyzerOptions([], new AnalyzerTestConfigOptionsProvider(new AnalyzerTestConfigOptions([.. options]))));
        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }
}
