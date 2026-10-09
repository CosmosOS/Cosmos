using Cosmos.Tools.Update;

namespace Cosmos.Tests.Tools;

public class ProjectPinUpdaterTests
{
    private const string NewVersion = "3.0.72";

    // Mirrors src/Cosmos.Build.Templates/templates/cosmos-kernel-csharp/KernelName.csproj
    // after pack-time token substitution — the exact shape every generated project has
    // (cosmos-kernel-vb/KernelName.vbproj carries the same three pins).
    private const string TemplateCsproj = """
        <Project Sdk="Cosmos.Sdk/3.0.70">

          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
          </PropertyGroup>

          <ItemGroup>
            <PackageReference Include="Cosmos.Kernel" Version="3.0.70" />
            <PackageReference Include="Cosmos.Kernel.System" Version="3.0.70" />
          </ItemGroup>

        </Project>
        """;

    [Fact]
    public void TemplateCsproj_MovesAllThreePinsAsOneSet()
    {
        ProjectPinUpdater.PinEdit edit = ProjectPinUpdater.ComputeEdit("Kernel.csproj", TemplateCsproj, NewVersion);

        Assert.Equal(3, edit.PinCount);
        Assert.Equal(3, edit.ChangedCount);
        Assert.Contains($"Sdk=\"Cosmos.Sdk/{NewVersion}\"", edit.NewContent);
        Assert.Contains($"<PackageReference Include=\"Cosmos.Kernel\" Version=\"{NewVersion}\" />", edit.NewContent);
        Assert.Contains($"<PackageReference Include=\"Cosmos.Kernel.System\" Version=\"{NewVersion}\" />", edit.NewContent);
        Assert.DoesNotContain("3.0.70", edit.NewContent);
        Assert.Equal(["3.0.70"], edit.PreviousVersions);
    }

    [Fact]
    public void SdkElementForm_IsUpdated()
    {
        string content = """
            <Project>
              <Sdk Name="Cosmos.Sdk" Version="3.0.70" />
            </Project>
            """;

        ProjectPinUpdater.PinEdit edit = ProjectPinUpdater.ComputeEdit("Kernel.csproj", content, NewVersion);

        Assert.Equal(1, edit.ChangedCount);
        Assert.Contains($"<Sdk Name=\"Cosmos.Sdk\" Version=\"{NewVersion}\" />", edit.NewContent);
    }

    [Fact]
    public void SdkElementForm_VersionBeforeName_IsUpdated()
    {
        string content = "<Project>\n  <Sdk Version=\"3.0.70\" Name=\"Cosmos.Sdk\" />\n</Project>";

        ProjectPinUpdater.PinEdit edit = ProjectPinUpdater.ComputeEdit("Kernel.csproj", content, NewVersion);

        Assert.Equal(1, edit.ChangedCount);
        Assert.Contains($"<Sdk Version=\"{NewVersion}\" Name=\"Cosmos.Sdk\" />", edit.NewContent);
    }

    [Fact]
    public void SdkElement_ForOtherSdk_IsUntouched()
    {
        string content = "<Project>\n  <Sdk Name=\"Other.Sdk\" Version=\"1.0.0\" />\n</Project>";

        ProjectPinUpdater.PinEdit edit = ProjectPinUpdater.ComputeEdit("Kernel.csproj", content, NewVersion);

        Assert.Equal(0, edit.PinCount);
        Assert.Equal(content, edit.NewContent);
    }

    [Fact]
    public void PackageReference_VersionBeforeInclude_IsUpdated()
    {
        string content = "<ItemGroup>\n  <PackageReference Version=\"3.0.70\" Include=\"Cosmos.Kernel\" />\n</ItemGroup>";

        ProjectPinUpdater.PinEdit edit = ProjectPinUpdater.ComputeEdit("Kernel.csproj", content, NewVersion);

        Assert.Equal(1, edit.ChangedCount);
        Assert.Contains($"Version=\"{NewVersion}\" Include=\"Cosmos.Kernel\"", edit.NewContent);
    }

    [Fact]
    public void PackageReference_MultiLineAttributes_IsUpdated()
    {
        string content = """
            <ItemGroup>
              <PackageReference Include="Cosmos.Kernel"
                                Version="3.0.70" />
            </ItemGroup>
            """;

        ProjectPinUpdater.PinEdit edit = ProjectPinUpdater.ComputeEdit("Kernel.csproj", content, NewVersion);

        Assert.Equal(1, edit.ChangedCount);
        Assert.Contains($"Version=\"{NewVersion}\"", edit.NewContent);
    }

    [Fact]
    public void NonCosmosPackages_AreUntouched()
    {
        string content = """
            <ItemGroup>
              <PackageReference Include="Spectre.Console" Version="0.49.1" />
              <PackageReference Include="Cosmos.Kernel" Version="3.0.70" />
            </ItemGroup>
            """;

        ProjectPinUpdater.PinEdit edit = ProjectPinUpdater.ComputeEdit("Kernel.csproj", content, NewVersion);

        Assert.Equal(1, edit.ChangedCount);
        Assert.Contains("<PackageReference Include=\"Spectre.Console\" Version=\"0.49.1\" />", edit.NewContent);
    }

    [Fact]
    public void UnderscorePrefixedInternalPackage_IsUntouched()
    {
        string content = "<ItemGroup>\n  <PackageReference Include=\"_Cosmos.Build.Analyzer.Patcher\" Version=\"3.0.70\" />\n</ItemGroup>";

        ProjectPinUpdater.PinEdit edit = ProjectPinUpdater.ComputeEdit("Kernel.csproj", content, NewVersion);

        Assert.Equal(0, edit.PinCount);
        Assert.Equal(content, edit.NewContent);
    }

    [Fact]
    public void CentralPackageManagementProps_UpdatesOnlyCosmosPins()
    {
        string content = """
            <Project>
              <ItemGroup>
                <PackageVersion Include="Cosmos.Kernel" Version="3.0.70" />
                <PackageVersion Include="Cosmos.Build.Ilc" Version="3.0.70" />
                <PackageVersion Include="xunit" Version="2.9.3" />
              </ItemGroup>
            </Project>
            """;

        ProjectPinUpdater.PinEdit edit = ProjectPinUpdater.ComputeEdit("Directory.Packages.props", content, NewVersion);

        Assert.Equal(2, edit.ChangedCount);
        Assert.Contains($"<PackageVersion Include=\"Cosmos.Kernel\" Version=\"{NewVersion}\" />", edit.NewContent);
        Assert.Contains($"<PackageVersion Include=\"Cosmos.Build.Ilc\" Version=\"{NewVersion}\" />", edit.NewContent);
        Assert.Contains("<PackageVersion Include=\"xunit\" Version=\"2.9.3\" />", edit.NewContent);
    }

    [Fact]
    public void GlobalJson_UpdatesOnlyTheCosmosSdkEntry()
    {
        string content = """
            {
              "msbuild-sdks": {
                "Cosmos.Sdk": "3.0.71.20260719",
                "Other.Sdk": "1.2.3"
              }
            }
            """;

        ProjectPinUpdater.PinEdit edit = ProjectPinUpdater.ComputeEdit("global.json", content, NewVersion);

        Assert.Equal(1, edit.ChangedCount);
        Assert.Contains($"\"Cosmos.Sdk\": \"{NewVersion}\"", edit.NewContent);
        Assert.Contains("\"Other.Sdk\": \"1.2.3\"", edit.NewContent);
        Assert.Equal(["3.0.71.20260719"], edit.PreviousVersions);
    }

    [Fact]
    public void GlobalJson_SdkSyntaxIsNotAppliedToCsproj()
    {
        // A csproj that merely mentions "Cosmos.Sdk": "..." in a comment/string must
        // not be edited by the JSON rule — file kind selects the rule set.
        string content = "<!-- \"Cosmos.Sdk\": \"3.0.70\" -->\n<Project></Project>";

        ProjectPinUpdater.PinEdit edit = ProjectPinUpdater.ComputeEdit("Kernel.csproj", content, NewVersion);

        Assert.Equal(0, edit.PinCount);
        Assert.Equal(content, edit.NewContent);
    }

    [Fact]
    public void MixedVersions_AreUnifiedToTheTarget()
    {
        string content = """
            <Project Sdk="Cosmos.Sdk/3.0.69">
              <ItemGroup>
                <PackageReference Include="Cosmos.Kernel" Version="3.0.70" />
              </ItemGroup>
            </Project>
            """;

        ProjectPinUpdater.PinEdit edit = ProjectPinUpdater.ComputeEdit("Kernel.csproj", content, NewVersion);

        Assert.Equal(2, edit.ChangedCount);
        Assert.DoesNotContain("3.0.69", edit.NewContent);
        Assert.DoesNotContain("3.0.70", edit.NewContent);
        Assert.Contains("3.0.69", edit.PreviousVersions);
        Assert.Contains("3.0.70", edit.PreviousVersions);
    }

    [Fact]
    public void NonLiteralPins_PropertiesAndTokens_AreUntouched()
    {
        // The repo's own raw template and any project driving versions from an
        // MSBuild property must never have those replaced with a literal.
        string content = """
            <Project Sdk="Cosmos.Sdk/@CosmosPackageVersion@">
              <ItemGroup>
                <PackageReference Include="Cosmos.Kernel" Version="$(CosmosVersion)" />
                <PackageReference Include="Cosmos.Kernel.System" Version="@CosmosPackageVersion@" />
              </ItemGroup>
            </Project>
            """;

        ProjectPinUpdater.PinEdit edit = ProjectPinUpdater.ComputeEdit("Kernel.csproj", content, NewVersion);

        Assert.Equal(0, edit.PinCount);
        Assert.Equal(0, edit.ChangedCount);
        Assert.Equal(content, edit.NewContent);
    }

    [Fact]
    public void PinsInsideXmlComments_AreUntouched()
    {
        string content = """
            <Project Sdk="Cosmos.Sdk/3.0.70">
              <ItemGroup>
                <!-- <PackageReference Include="Cosmos.Kernel" Version="1.0.0" /> -->
                <PackageReference Include="Cosmos.Kernel" Version="3.0.70" />
              </ItemGroup>
            </Project>
            """;

        ProjectPinUpdater.PinEdit edit = ProjectPinUpdater.ComputeEdit("Kernel.csproj", content, NewVersion);

        Assert.Equal(2, edit.ChangedCount);
        Assert.Contains("<!-- <PackageReference Include=\"Cosmos.Kernel\" Version=\"1.0.0\" /> -->", edit.NewContent);
        Assert.Contains($"<PackageReference Include=\"Cosmos.Kernel\" Version=\"{NewVersion}\" />", edit.NewContent);
    }

    [Fact]
    public void SecondPass_IsIdempotent()
    {
        ProjectPinUpdater.PinEdit first = ProjectPinUpdater.ComputeEdit("Kernel.csproj", TemplateCsproj, NewVersion);
        ProjectPinUpdater.PinEdit second = ProjectPinUpdater.ComputeEdit("Kernel.csproj", first.NewContent, NewVersion);

        Assert.Equal(3, second.PinCount);
        Assert.Equal(0, second.ChangedCount);
        Assert.Equal(first.NewContent, second.NewContent);
    }

    [Fact]
    public void FindPinFiles_FindsProjectFilesAndSkipsBuildOutput()
    {
        string root = Path.Combine(Path.GetTempPath(), $"cosmos-pin-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "bin"));
            Directory.CreateDirectory(Path.Combine(root, "obj"));
            Directory.CreateDirectory(Path.Combine(root, "output-x64"));
            Directory.CreateDirectory(Path.Combine(root, "VbKernel", "bin"));

            File.WriteAllText(Path.Combine(root, "Kernel.csproj"), TemplateCsproj);
            File.WriteAllText(Path.Combine(root, "VbKernel", "VbKernel.vbproj"), TemplateCsproj);
            File.WriteAllText(Path.Combine(root, "VbKernel", "bin", "VbKernel.vbproj"), TemplateCsproj);
            File.WriteAllText(Path.Combine(root, "bin", "Kernel.csproj"), TemplateCsproj);
            File.WriteAllText(Path.Combine(root, "obj", "Kernel.csproj"), TemplateCsproj);
            File.WriteAllText(Path.Combine(root, "output-x64", "Kernel.csproj"), TemplateCsproj);
            File.WriteAllText(Path.Combine(root, "global.json"), "{ \"msbuild-sdks\": { \"Cosmos.Sdk\": \"3.0.70\" } }");
            File.WriteAllText(Path.Combine(root, "Unrelated.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");
            File.WriteAllText(Path.Combine(root, "Directory.Packages.props"),
                "<Project><ItemGroup><PackageVersion Include=\"Cosmos.Kernel\" Version=\"3.0.70\" /></ItemGroup></Project>");

            List<string> files = ProjectPinUpdater.FindPinFiles(root);
            List<string> names = files.Select(f => Path.GetRelativePath(root, f)).ToList();

            Assert.Equal(4, files.Count);
            Assert.Contains("Kernel.csproj", names);
            Assert.Contains(Path.Combine("VbKernel", "VbKernel.vbproj"), names);
            Assert.Contains("global.json", names);
            Assert.Contains("Directory.Packages.props", names);
        }
        finally
        {
            try
            {
                Directory.Delete(root, recursive: true);
            }
            catch
            {
            }
        }
    }
}
