using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Cosmos.VS.XSharp {
  /// Defines an editor format for the XSharp type that has a purple background
  /// and is underlined.
  [Export(typeof(EditorFormatDefinition))]
  [ClassificationType(ClassificationTypeNames = "XSharp")]
  [Name("XSharp")]
  [UserVisible(true)]
  [Order(Before = Priority.Default)] //set the priority to be after the default classifiers
  internal sealed class XSharpFormat : ClassificationFormatDefinition {
    /// Defines the visual format for the "Cosmos.VS.XSharp" classification type
    public XSharpFormat() {
      this.DisplayName = "XSharp"; //human readable version of the name
      this.BackgroundColor = Colors.BlueViolet;
      this.TextDecorations = System.Windows.TextDecorations.Underline;
    }
  }

  internal static class XSharpClassificationDefinition {
    /// Defines the "XSharp" classification type.
    [Export(typeof(ClassificationTypeDefinition))]
    [Name("XSharp")]
    internal static ClassificationTypeDefinition XSharpType = null;
  }

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
