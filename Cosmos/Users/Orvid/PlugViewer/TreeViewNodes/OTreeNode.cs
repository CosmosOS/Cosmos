using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace PlugViewer.TreeViewNodes
{
    public abstract class OTreeNode : TreeNode
    {
        public static List<OTreeNode>[] TreeNodes;
        static OTreeNode()
        {
            TreeNodes = new List<OTreeNode>[8];
            for (byte b = 0; b < 8; b++)
            {
                TreeNodes[b] = new List<OTreeNode>();
            }
        }
        public OTreeNode(TreeNodeType t)
        {
            TreeNodes[(byte)t].Add(this);
        }
        public List<Errors.BaseError> Errors = new List<Errors.BaseError>();
        public List<Warnings.BaseWarning> Warnings = new List<Warnings.BaseWarning>();
        public abstract TreeNodeType Type { get; }
        public abstract object Definition { get; }
        public abstract void ShowNodeInfo(RichTextBox itemPanel);
    }

    public enum TreeNodeType : byte
    {
        Assembly = 0,
        Module = 1,
        Namespace = 2,
        Class = 3,
        Method = 4,
        Event = 5,
        Property = 6,
        Field = 7,
    }
}
