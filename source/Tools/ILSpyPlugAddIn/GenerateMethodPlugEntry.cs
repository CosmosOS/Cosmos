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
            if (context?.SelectedTreeNodes != null)
            {
                foreach (var node in context.SelectedTreeNodes)
                {
                    var xCurrentMethod = node as MethodTreeNode;
                    if (xCurrentMethod != null)
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

            StringBuilder xString = new StringBuilder();
            foreach (var node in context.SelectedTreeNodes)
            {
                var xCurrentMethod = node as MethodTreeNode;
                xString.Append(GenerateMethod(xCurrentMethod.MethodDefinition));
                xString.AppendLine();
            }

            Clipboard.SetText(xString.ToString());

            MessageBox.Show("Done", "Cosmos Plug tool");
        }

        public static string GenerateMethod(MethodDefinition method)
        {
            var xSB = new StringBuilder();
            
            xSB.Append($"public static {Utilities.GetCSharpTypeName(method.ReturnType)} {Utilities.GetMethodName(method)}(");
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