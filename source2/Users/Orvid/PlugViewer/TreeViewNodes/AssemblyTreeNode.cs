using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;
using System.Windows.Forms;

namespace PlugViewer.TreeViewNodes
{
    internal class AssemblyTreeNode : OTreeNode
    {
        public AssemblyTreeNode(AssemblyDefinition definition)
        {
            this.def = definition;
            this.SelectedImageIndex = Constants.AssemblyIcon;
            this.ImageIndex = Constants.AssemblyIcon;
            this.Text = definition.Name.Name;
            Log.WriteLine("Assembly '" + this.Text + "' Was loaded successfully.");
        }

        public override TreeNodeType Type
        {
            get { return TreeNodeType.Assembly; }
        }

        private AssemblyDefinition def;

        public override object Definition
        {
            get { return (object)def; }
        }

        public override void ShowNodeInfo(RichTextBox rtb)
        {
            rtb.Text =
                "Assembly '" + def.Name.Name + "' contains:\r\n" +
                def.Modules.Count.ToString() + " Modules.\r\n";
        }
    }
}
