using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Cosmos.VS.XSharp {
  internal static class XSharpClassificationDefinition {
    /// Defines the "XSharp" classification type.
    [Export(typeof(ClassificationTypeDefinition))]
    [Name("XSharp")]
    internal static ClassificationTypeDefinition XSharpType = null;
  }
}
