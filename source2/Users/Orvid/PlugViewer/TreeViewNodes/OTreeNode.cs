using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace PlugViewer.TreeViewNodes
{
    public abstract class OTreeNode : TreeNode
    {
        public List<Errors.BaseError> Errors = new List<Errors.BaseError>();
        public List<Warnings.BaseWarning> Warnings = new List<Warnings.BaseWarning>();
        public abstract TreeNodeType Type { get; }
        public abstract object Definition { get; }
        public abstract void ShowNodeInfo(RichTextBox itemPanel);
    }

    public enum TreeNodeType
    {
        Assembly,
        Module,
        Namespace,
        Class,
        Method,
        Event,
        Property,
        Field,
    }
}
