using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

// http://msdn.microsoft.com/en-us/library/dd885244.aspx
// http://buildstarted.com/2010/09/09/managed-extensibility-framework-and-visual-studio-2010-editor-classifiers/
// http://dotneteers.net/blogs/divedeeper/archive/2008/11/04/LearnVSXNowPart38.aspx

namespace Cosmos.VS.XSharp {
  internal static class XSharpClassificationDefinition {
    /// Defines the "XSharp" classification type.
    [Export(typeof(ClassificationTypeDefinition))]
    [Name("XSharp")]
    [BaseDefinition("formal language")]
    internal static ClassificationTypeDefinition XSharpType;

    // Define XSharp content type.
    [Export]
    [Name("XSharp")]
    [BaseDefinition("code")]
    internal static ContentTypeDefinition XSharpContentTypeDefinition;

    // Associate XSharp content type to *.xs.
    [Export]
    [FileExtension(".xs")]
    [ContentType("XSharp")]
    internal static FileExtensionToContentTypeDefinition XSharpFileExtensionDefinition;
  }
}
