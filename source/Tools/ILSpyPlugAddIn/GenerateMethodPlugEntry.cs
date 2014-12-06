using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ICSharpCode.ILSpy;
using ICSharpCode.ILSpy.TreeNodes;
using Mono.Cecil;

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

            var xSB = GenerateMethod(xCurrentMethod.MethodDefinition);
            Clipboard.SetText(xSB);

            MessageBox.Show("Done", "Cosmos Plug tool");
        }

        public static string GenerateMethod(MethodDefinition method)
        {
            var xSB = new StringBuilder();
            
            xSB.Append("public static ");
            xSB.Append(Utilities.GetCSharpTypeName(method.ReturnType));
            xSB.Append(" ");
            xSB.Append(Utilities.GetMethodName(method));
            xSB.Append("(");
            var xAddComma = false;

            if (!method.IsStatic)
            {
                if (method.DeclaringType.IsValueType)
                {
                    xSB.Append("ref ");
                }
                if (method.DeclaringType.IsPublic)
                {
                    xSB.Append(Utilities.GetCSharpTypeName(method.DeclaringType));
                }
                else
                {
                    xSB.Append("object");
                }
                xSB.Append(" ");
                xSB.Append("aThis");
                xAddComma = true;
            }

            foreach (var xParameter in method.Parameters)
            {
                if (xAddComma)
                {
                    xSB.Append(", ");
                }
                xAddComma = true;
                var xParameterTypeDef = xParameter.ParameterType as TypeDefinition;
                if (xParameterTypeDef != null
                    && xParameterTypeDef.IsPublic)
                {
                    xSB.Append(Utilities.GetCSharpTypeName(xParameter.ParameterType));
                }
                else
                {
                    xSB.Append("object");
                }
                xSB.Append(" ");
                xSB.Append(xParameter.Name);
            }

            xSB.AppendLine(")");
            xSB.AppendLine("{");
            xSB.AppendLine("}");
            return xSB.ToString();
        }
    }
}