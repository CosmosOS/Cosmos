using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Mono.Cecil;

namespace PlugViewer.TreeViewNodes
{
    internal class NamespaceTreeNode : OTreeNode
    {
        public NamespaceTreeNode(string name)
        {
            this.SelectedImageIndex = Constants.NamespaceIcon;
            this.ImageIndex = Constants.NamespaceIcon;
            this.Text = name;
            Log.WriteLine("Namespace '" + this.Text + "' was discovered.");
        }

        public override TreeNodeType Type
        {
            get { return TreeNodeType.Namespace; }
        }

        public override object Definition
        {
            get { return null; }
        }

        public override void ShowNodeInfo(RichTextBox rtb)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Namespace '" + this.Text + "'");
            rtb.Text = sb.ToString();
        }
    }
}
