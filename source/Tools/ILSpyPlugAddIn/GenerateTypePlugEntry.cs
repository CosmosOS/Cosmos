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
    [ExportContextMenuEntry(Header = "Cosmos Plug: Generate plug")]
    public class GenerateTypePlugEntry: BaseContextMenuEntry
    {
        public override bool IsVisible(TextViewContext context)
        {
            if (context.SelectedTreeNodes.Length != 1)
            {
                return false;
            }
            var xCurrentType = context.SelectedTreeNodes[0] as TypeTreeNode;
            if (xCurrentType == null)
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
            var xCurrentType = context.SelectedTreeNodes[0] as TypeTreeNode;
            if (xCurrentType == null)
            {
                throw new Exception("Current TreeNode is not a Type!");
            }

            if (MessageBox.Show("Do you want to generate plug code to your clipboard?", "Cosmos Plug tool", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }

            var xSB = new StringBuilder();
            if (xCurrentType.TypeDefinition.IsPublic)
            {
                xSB.AppendFormat("[Plug(Target = typeof(global::{0}))]", Utilities.GetCSharpTypeName(xCurrentType.TypeDefinition));
            }
            else
            {
                xSB.AppendFormat("[Plug(TargetName = \"{0}\")]", Utilities.GetCSharpTypeName(xCurrentType.TypeDefinition));
            }
            xSB.AppendLine();
            xSB.AppendFormat("public static class {0}Plug", xCurrentType.Name);
            xSB.AppendLine();
            xSB.AppendLine("{");
            xSB.AppendLine("}");

            Clipboard.SetText(xSB.ToString());

            MessageBox.Show("Done", "Cosmos Plug tool");
        }
    }
}