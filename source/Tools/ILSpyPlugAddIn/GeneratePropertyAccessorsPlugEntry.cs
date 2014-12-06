using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if (context.SelectedTreeNodes.Length != 1)
            {
                return false;
            }
            var xCurrentProperty = context.SelectedTreeNodes[0] as PropertyTreeNode;
            if (xCurrentProperty == null)
            {
                return false;
            }
            return true;
        }

        public override void Execute(TextViewContext context)
        {
            if (context.SelectedTreeNodes.Length != 1)
            {
                throw new Exception("SelectedTreeNodes = " + context.SelectedTreeNodes.Length);
            }
            var xCurrentProperty = context.SelectedTreeNodes[0] as PropertyTreeNode;
            if (xCurrentProperty == null)
            {
                throw new Exception("Current TreeNode is not a Property!");
            }

            if (MessageBox.Show("Do you want to generate plug code to your clipboard?", "Cosmos Plug tool", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }

            var xProp = xCurrentProperty.PropertyDefinition;
            var xSB = new StringBuilder();
            if (xProp.GetMethod != null)
            {
                xSB.AppendLine(GenerateMethodPlugEntry.GenerateMethod(xProp.GetMethod));
                xSB.AppendLine();
            }
            if (xProp.SetMethod != null)
            {
                xSB.AppendLine(GenerateMethodPlugEntry.GenerateMethod(xProp.SetMethod));
                xSB.AppendLine();
            }

            Clipboard.SetText(xSB.ToString().Trim());

            MessageBox.Show("Done", "Cosmos Plug tool");
        }
    }
}