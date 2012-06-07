using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Cosmos.VS.XSharp {
  internal static class FileAndContentTypeDefinitions {
    [Export]
    [Name("XSharp")]
    [BaseDefinition("text")]
    internal static ContentTypeDefinition XSharpContentTypeDefinition;

    [Export]
    [FileExtension(".xs")]
    [ContentType("XSharp")]
    internal static FileExtensionToContentTypeDefinition XSharpFileExtensionDefinition;
  }
}
