using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;
using System.Windows.Forms;

namespace PlugViewer.TreeViewNodes
{
    internal class PropertyTreeNode : OTreeNode
    {
        public PropertyTreeNode(PropertyDefinition definition, bool writable)
        {
            this.wrtble = writable;
            this.def = definition;
            if (this.wrtble)
            {
                this.SelectedImageIndex = Constants.PropertyIcon;
                this.ImageIndex = Constants.PropertyIcon;
            }
            else
            {
                this.SelectedImageIndex = Constants.ReadOnlyPropertyIcon;
                this.ImageIndex = Constants.ReadOnlyPropertyIcon;
            }
            this.Text = definition.Name;
            Log.WriteLine("Property '" + this.Text + "' was loaded.");
        }

        public override TreeNodeType Type
        {
            get { return TreeNodeType.Event; }
        }

        private PropertyDefinition def;
        private bool wrtble;

        public bool ReadOnly
        {
            get { return !wrtble; }
        }

        public override object Definition
        {
            get { return (object)def; }
        }

        public override void ShowNodeInfo(RichTextBox rtb)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Property '" + def.Name + "' is " + (wrtble ? "Writable" : "Read-Only"));
            sb.AppendLine();
            sb.AppendLine();

            rtb.Text = sb.ToString();
        }
    }
}
