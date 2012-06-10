using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

// http://msdn.microsoft.com/en-us/library/dd885244.aspx
// http://msdn.microsoft.com/en-us/library/dd885240.aspx
// http://buildstarted.com/2010/09/09/managed-extensibility-framework-and-visual-studio-2010-editor-classifiers/
// http://dotneteers.net/blogs/divedeeper/archive/2008/11/04/LearnVSXNowPart38.aspx
// http://social.msdn.microsoft.com/Forums/en-US/vsx/thread/9875a130-ec8b-4026-94e6-e53b23481b6f/

// TODO:
// http://msdn.microsoft.com/en-us/library/bb166173.aspx
// This seems easier, but requires changing to MPF project. Can use one of our existing MPF projects?
// http://stackoverflow.com/questions/5211018/implementing-a-language-service-by-using-the-managed-package-framework
// http://channel9.msdn.com/blogs/vsipmarketing/vsx212-adding-a-language-service-into-visual-studio-2010
// Language Services
//  http://msdn.microsoft.com/en-us/library/bb165099
// Language Service using MPF
//  http://msdn.microsoft.com/en-us/library/bb166533

namespace Cosmos.VS.XSharp {
  internal static class XSharpClassificationDefinition {
    /// Defines the XSharp classification type.
    [Export(typeof(ClassificationTypeDefinition))]
    [Name("XSharp")]
    [BaseDefinition("formal language")]
    internal static ClassificationTypeDefinition XSharpType;

    // Define XSharp content type.
    [Export]
    [Name("XSharp")]
    [DisplayName("X#")]
    [BaseDefinition("code")]
    internal static ContentTypeDefinition XSharpContentTypeDefinition;

    // Associate XSharp content type to *.xs.
    [Export]
    [FileExtension(".xs")]
    [ContentType("XSharp")]
    internal static FileExtensionToContentTypeDefinition XSharpFileExtensionDefinition;
  }
}
