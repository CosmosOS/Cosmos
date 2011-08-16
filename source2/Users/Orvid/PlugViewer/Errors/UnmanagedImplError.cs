using System;
using System.Collections.Generic;
using System.Text;
using PlugViewer.TreeViewNodes;
using Mono.Cecil;

namespace PlugViewer.Errors
{
    internal class UnmanagedImplError : BaseError
    {
        public override TreeNodeType AppliesTo
        {
            get { return TreeNodeType.Method; }
        }

        public override void EvaluateNode(OTreeNode node)
        {
            MethodDefinition m = (MethodDefinition)node.Definition;
            MethodImplAttributes xImplFlags = m.ImplAttributes;
            if ((xImplFlags & MethodImplAttributes.Unmanaged) != 0)
            {
#if DebugErrors
                Log.WriteLine(NameBuilder.BuildMethodName(m) + " ~ Method Implementation: Unmanaged");
#endif
                node.SelectedImageIndex = Constants.ErrorIcon;
                node.ImageIndex = Constants.ErrorIcon;
                node.Errors.Add(this);
            }
        }

        public override string Name
        {
            get { return "Unmanaged Implementation Error"; }
        }

        public override string Description
        {
            get { return "This method is Unmanaged, and can't be compiled by IL2CPU."; }
        }
    }
}
