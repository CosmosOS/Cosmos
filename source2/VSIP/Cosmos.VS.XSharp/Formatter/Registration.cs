using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Cosmos.VS.XSharp {
  internal static class XSharpClassificationDefinition {
    /// Defines the "XSharp" classification type.
    [Export(typeof(ClassificationTypeDefinition))]
    [Name("XSharp")]
    internal static ClassificationTypeDefinition XSharpType;

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
