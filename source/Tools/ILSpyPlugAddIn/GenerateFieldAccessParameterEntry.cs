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
    [ExportContextMenuEntry(Header = "Cosmos Plug: Generate FieldAccess parameter")]
    public class GenerateFieldAccessParameterEntry: BaseContextMenuEntry
    {
        public override bool IsVisible(TextViewContext context)
        {
            if (context?.SelectedTreeNodes != null)
            {
                foreach (var node in context.SelectedTreeNodes)
                {
                    var xCurrentField = node as FieldTreeNode;
                    if ((xCurrentField != null) && !xCurrentField.FieldDefinition.HasConstant)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void Execute(TextViewContext context)
        {
            if (MessageBox.Show("Do you want to generate FieldAccess code to your clipboard?", "Cosmos Plug tool", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }

            StringBuilder xString = new StringBuilder();
            foreach (var node in context.SelectedTreeNodes)
            {
                var xCurrentField = node as FieldTreeNode;
                if (xCurrentField != null)
                {
                    xString.Append(GenerateField(xCurrentField.FieldDefinition));
                    xString.AppendLine();
                }
            }

            Clipboard.SetText(xString.ToString());

            MessageBox.Show("Done", "Cosmos Plug tool");
        }

        public string GenerateField(FieldDefinition field)
        {
            StringBuilder xString = new StringBuilder();
            xString.Append($"[FieldAccess(Name = \"{field.FieldType.FullName} {field.DeclaringType.FullName}.{field.Name}\")] ref {Utilities.GetCSharpTypeName(field.FieldType)} field{field.Name}");
            return xString.ToString();
        }
    }
}
