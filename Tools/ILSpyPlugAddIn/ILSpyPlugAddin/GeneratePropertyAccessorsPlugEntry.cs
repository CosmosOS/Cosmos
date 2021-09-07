using System.Text;
using System.Windows;
using ICSharpCode.ILSpy;
using ICSharpCode.ILSpy.TreeNodes;

namespace Cosmos.ILSpyPlugs.Plugin
{
    [ExportContextMenuEntry(Header = "Cosmos Plug: Generate method plug for all accessors")]
    public class GeneratePropertyAccessorsPlugEntry: BaseContextMenuEntry
    {
        public override bool IsVisible(TextViewContext context)
        {
            if (context?.SelectedTreeNodes != null)
            {
                foreach (var node in context.SelectedTreeNodes)
                {
                    var xCurrentProperty = node as PropertyTreeNode;
                    if (xCurrentProperty != null)
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
                var xCurrentProperty = node as PropertyTreeNode;
                if (node != null)
                {
                    xString.Append(Utilities.GeneratePropertyPlugEntry(xCurrentProperty.PropertyDefinition));
                    xString.AppendLine();
                }
            }

            Clipboard.SetText(xString.ToString().Trim());

            MessageBox.Show("Done", "Cosmos Plug tool");
        }
    }
}
