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
    [ExportContextMenuEntry(Header = "Cosmos Plug: Generate FieldAccess parameter")]
    public class GenerateFieldAccessParameterEntry: BaseContextMenuEntry
    {
        public override bool IsVisible(TextViewContext context)
        {
            if (context.SelectedTreeNodes.Length != 1)
            {
                return false;
            }
            var xCurrentField = context.SelectedTreeNodes[0] as FieldTreeNode;
            if (xCurrentField == null)
            {
                return false;
            }
            if (xCurrentField.FieldDefinition.HasConstant)
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
            var xCurrentField = context.SelectedTreeNodes[0] as FieldTreeNode;
            if (xCurrentField == null)
            {
                throw new Exception("Current TreeNode is not a Field!");
            }

            if (MessageBox.Show("Do you want to generate FieldAccess code to your clipboard?", "Cosmos Plug tool", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }


            Clipboard.SetText(String.Format("[FieldAccess(Name = \"{0} {1}.{2}\")] ref {3} field{2}",
                                            xCurrentField.FieldDefinition.FieldType.FullName,
                                            xCurrentField.FieldDefinition.DeclaringType.FullName,
                                            xCurrentField.FieldDefinition.Name,
                                            Utilities.GetCSharpTypeName(xCurrentField.FieldDefinition.FieldType)));
            MessageBox.Show("Done", "Cosmos Plug tool");
        }
    }
}
