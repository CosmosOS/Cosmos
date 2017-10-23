using System;
using System.Collections.Generic;
using System.Text;
using PlugViewer.TreeViewNodes;
using System.Reflection;

namespace PlugViewer.Errors
{
    internal class PInvokeError : BaseError
    {
        public override TreeNodeType AppliesTo
        {
            get { return TreeNodeType.Method; }
        }

        public override void EvaluateNode(OTreeNode node)
        {
            MethodInfo m = (MethodInfo)node.Definition;
            if ((m.Attributes & MethodAttributes.PinvokeImpl) != 0)
            {
#if DebugErrors
                Log.WriteLine(NameBuilder.BuildMethodName(m) + " ~ PInvoke Impl");
#endif
                node.SelectedImageIndex = Constants.ErrorIcon;
                node.ImageIndex = Constants.ErrorIcon;
                node.Errors.Add(this);
            }
        }

        public override string Name
        {
            get { return "PInvoke Error"; }
        }

        public override string Description
        {
            get { return "This method is a PInvoke Implementation."; }
        }
    }
}
