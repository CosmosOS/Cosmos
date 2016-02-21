using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ICSharpCode.ILSpy;
using ICSharpCode.ILSpy.TreeNodes;
using Mono.Cecil;

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
                    xString.Append(GenerateProperty(xCurrentProperty.PropertyDefinition));
                    xString.AppendLine();
                }
            }

            Clipboard.SetText(xString.ToString().Trim());

            MessageBox.Show("Done", "Cosmos Plug tool");
        }

        public string GenerateProperty(PropertyDefinition property)
        {
            StringBuilder xString = new StringBuilder();
            if (property.GetMethod != null)
            {
                xString.AppendLine(GenerateMethodPlugEntry.GenerateMethod(property.GetMethod));
                xString.AppendLine();
            }
            if (property.SetMethod != null)
            {
                xString.AppendLine(GenerateMethodPlugEntry.GenerateMethod(property.SetMethod));
                xString.AppendLine();
            }
            return xString.ToString();
        }
    }
}