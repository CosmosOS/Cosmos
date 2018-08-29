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
                    xString.Append(GenerateType(xCurrentType.TypeDefinition));
                    xString.AppendLine();
                }
            }

            Clipboard.SetText(xString.ToString());

            MessageBox.Show("Done", "Cosmos Plug tool");
        }

        public string GenerateType(TypeDefinition type)
        {
            var xString = new StringBuilder();
            xString.AppendFormat(
                type.IsPublic
                    ? "[Plug(Target = typeof(global::{0}))]"
                    : "[Plug(TargetName = \"{0}, {1}\")]", Utilities.GetCSharpTypeName(type), type.Module.Assembly.Name);
            xString.AppendLine();
            xString.AppendFormat("public static class {0}Impl", type.Name);
            xString.AppendLine();
            xString.AppendLine("{");
            xString.AppendLine("}");
            return xString.ToString();
        }
    }
}
