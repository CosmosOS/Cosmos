using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Mono.Cecil;

namespace PlugViewer.TreeViewNodes
{
    internal class EventTreeNode : OTreeNode
    {
        public EventTreeNode(EventDefinition definition) : base(TreeNodeType.Event)
        {
            this.def = definition;
            this.SelectedImageIndex = Constants.EventIcon;
            this.ImageIndex = Constants.EventIcon;
            this.Text = definition.Name;
#if DebugTreeNodeLoading
            Log.WriteLine("Event '" + this.Text + "' was loaded.");
#endif
        }

        public override TreeNodeType Type
        {
            get { return TreeNodeType.Event; }
        }

        private EventDefinition def;

        public override object Definition
        {
            get { return (object)def; }
        }

        public override void ShowNodeInfo(RichTextBox rtb)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Event '" + def.Name + "'");
            sb.AppendLine();
            sb.AppendLine();

            rtb.Text = sb.ToString();
        }
    }
}
