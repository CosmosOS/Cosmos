using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

namespace PlugViewer.TreeViewNodes
{
    internal class ModuleTreeNode : OTreeNode
    {
        public Dictionary<string, TreeNode> Namespaces = new Dictionary<string, TreeNode>();

        public ModuleTreeNode(Module definition) : base(TreeNodeType.Module)
        {
            this.def = definition;
            this.SelectedImageIndex = Constants.ModuleIcon;
            this.ImageIndex = Constants.ModuleIcon;
            this.Text = definition.Name;
#if DebugTreeNodeLoading
            Log.WriteLine("Module '" + this.Text + "' was loaded.");
#endif
        }

        public override TreeNodeType Type
        {
            get { return TreeNodeType.Module; }
        }

        private Module def;

        public override object Definition
        {
            get { return (object)def; }
        }

        public override void ShowNodeInfo(RichTextBox rtb)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Module '" + def.Name + "' contains:");
            sb.AppendLine(def.GetTypes().Length.ToString() + " Types,");
            sb.AppendLine(Namespaces.Count.ToString() + " Namespaces,");
            //sb.AppendLine(def.Resources.Count.ToString() + " Resources,");
            //sb.AppendLine("and " + (def.EntryPoint != null ? "does " : "doesn't ") + "contain an entry point.");
            sb.AppendLine();
            sb.AppendLine();

            rtb.Text = sb.ToString();            
        }
    }
}
