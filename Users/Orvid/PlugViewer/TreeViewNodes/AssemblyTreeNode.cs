using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace PlugViewer.TreeViewNodes
{
    internal class AssemblyTreeNode : OTreeNode
    {
        public AssemblyTreeNode(Assembly definition) : base(TreeNodeType.Assembly)
        {
            this.def = definition;
            this.SelectedImageIndex = Constants.AssemblyIcon;
            this.ImageIndex = Constants.AssemblyIcon;
            this.Text = definition.GetName().Name;
#if DebugTreeNodeLoading
            Log.WriteLine("Assembly '" + this.Text + "' Was loaded successfully.");
#endif
        }

        public override TreeNodeType Type
        {
            get { return TreeNodeType.Assembly; }
        }

        private Assembly def;

        public override object Definition
        {
            get { return (object)def; }
        }

        public override void ShowNodeInfo(RichTextBox rtb)
        {
            rtb.Text =
                "Assembly '" + def.GetName().Name + "' contains:\r\n" +
                def.GetModules().Length.ToString() + " Modules.\r\n";
        }
    }
}
