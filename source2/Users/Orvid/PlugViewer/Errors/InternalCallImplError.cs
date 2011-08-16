using System;
using System.Collections.Generic;
using System.Text;
using PlugViewer.TreeViewNodes;
using Mono.Cecil;

namespace PlugViewer.Errors
{
    internal class InternalCallImplError : BaseError
    {
        public override TreeNodeType AppliesTo
        {
            get { return TreeNodeType.Method; }
        }

        public override void EvaluateNode(OTreeNode node)
        {
            MethodDefinition m = (MethodDefinition)node.Definition;
            MethodImplAttributes xImplFlags = m.ImplAttributes;
            if ((xImplFlags & MethodImplAttributes.InternalCall) != 0)
            {
                Log.WriteLine(NameBuilder.BuildMethodName(m) + " ~ Method Implementation: Internal Call");
                node.Errors.Add(this);
            }
        }

        public override string Name
        {
            get { return "Internal Call Implementation Error"; }
        }

        public override string Description
        {
            get { return "This method is implemented internally, and can't be compiled by IL2CPU."; }
        }
    }
}
