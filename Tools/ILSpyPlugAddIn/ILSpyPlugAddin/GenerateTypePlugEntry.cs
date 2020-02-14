using System.Text;
using System.Windows;
using ICSharpCode.ILSpy;
using ICSharpCode.ILSpy.TreeNodes;

namespace Cosmos.ILSpyPlugs.Plugin
{
    [ExportContextMenuEntry(Header = "Cosmos Plug: Generate plug")]
    public class GenerateTypePlugEntry : BaseContextMenuEntry
    {
        public override bool IsVisible(TextViewContext context)
        {
            if (context?.SelectedTreeNodes != null)
            {
                foreach (var node in context.SelectedTreeNodes)
                {
                    var xCurrentType = node as TypeTreeNode;
                    if (xCurrentType != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void Execute(TextViewContext context)
        {
            if (MessageBox.Show("Do you want to generate plug code to your clipboard?", "Cosmos Plug tool", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }

            var xString = new StringBuilder();
            foreach (var node in context.SelectedTreeNodes)
            {
                var xCurrentType = node as TypeTreeNode;
                if (xCurrentType != null)
                {
                    xString.Append(Utilities.GenerateTypePlugEntry(xCurrentType.TypeDefinition));
                    xString.AppendLine();
                }
            }

            Clipboard.SetText(xString.ToString());

            MessageBox.Show("Done", "Cosmos Plug tool");
        }
    }
}
