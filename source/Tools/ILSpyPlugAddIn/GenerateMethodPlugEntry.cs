using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ICSharpCode.ILSpy;
using ICSharpCode.ILSpy.TreeNodes;

namespace Cosmos.ILSpyPlugs.Plugin
{
    [ExportContextMenuEntry(Header = "Cosmos Plug: Generate method plug")]
    public class GenerateMethodPlugEntry: BaseContextMenuEntry
    {
        public override bool IsVisible(TextViewContext context)
        {
            if (context.SelectedTreeNodes.Length != 1)
            {
                return false;
            }
            var xCurrentMethod = context.SelectedTreeNodes[0] as MethodTreeNode;
            if (xCurrentMethod == null)
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
            var xCurrentMethod = context.SelectedTreeNodes[0] as MethodTreeNode;
            if (xCurrentMethod == null)
            {
                throw new Exception("Current TreeNode is not a Method!");
            }

            if (MessageBox.Show("Do you want to generate plug code to your clipboard?", "Cosmos Plug tool", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }

            var xSB = new StringBuilder();
            xSB.Append("public static ");
            var xMethod = xCurrentMethod.MethodDefinition;
            xSB.Append(Utilities.GetCSharpTypeName(xMethod.ReturnType));
            xSB.Append(" ");
            xSB.Append(Utilities.GetMethodName(xMethod));
            xSB.Append("(");
            var xAddComma = false;
            
            if (!xMethod.IsStatic)
            {
                if (xMethod.DeclaringType.IsValueType)
                {
                    xSB.Append("ref ");
                }
                xSB.Append(Utilities.GetCSharpTypeName(xMethod.DeclaringType));
                xSB.Append(" ");
                xSB.Append("aThis");
                xAddComma = true;
            }

            foreach (var xParameter in xMethod.Parameters)
            {
                if (xAddComma)
                {
                    xSB.Append(", ");
                }
                xAddComma = true;
                xSB.Append(Utilities.GetCSharpTypeName(xParameter.ParameterType));
                xSB.Append(" ");
                xSB.Append(xParameter.Name);
            }

            xSB.AppendLine(")");
            xSB.AppendLine("{");
            xSB.AppendLine("}");
            Clipboard.SetText(xSB.ToString());

            MessageBox.Show("Done", "Cosmos Plug tool");
        }
    }
}