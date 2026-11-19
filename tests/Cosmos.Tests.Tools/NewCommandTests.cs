// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Tools.Commands;

namespace Cosmos.Tests.Tools;

public class NewCommandTests
{
    [Theory]
    [InlineData("csharp", "cosmos-kernel-csharp")]
    [InlineData("vb", "cosmos-kernel-vb")]
    [InlineData("VB", "cosmos-kernel-vb")]
    public void TemplateFor_KnownLanguage_NamesItsTemplate(string language, string template)
    {
        Assert.Equal(template, NewCommand.TemplateFor(language));
    }

    [Theory]
    [InlineData("fsharp")]
    [InlineData("c#")]
    [InlineData("")]
    public void TemplateFor_OtherLanguage_IsNull(string language)
    {
        Assert.Null(NewCommand.TemplateFor(language));
    }

    [Fact]
    public void Validate_RejectsALanguageWithNoTemplate()
    {
        NewSettings settings = new NewSettings { Name = "MyKernel", Language = "fsharp" };

        Assert.False(settings.Validate().Successful);
    }

    [Fact]
    public void Validate_DefaultsToCSharp()
    {
        NewSettings settings = new NewSettings { Name = "MyKernel" };

        Assert.True(settings.Validate().Successful);
        Assert.Equal("cosmos-kernel-csharp", NewCommand.TemplateFor(settings.Language));
    }
}
