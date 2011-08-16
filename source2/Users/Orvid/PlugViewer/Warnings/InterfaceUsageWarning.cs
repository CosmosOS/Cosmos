using System;
using System.Collections.Generic;
using System.Text;
using PlugViewer.TreeViewNodes;
using Mono.Cecil;

namespace PlugViewer.Warnings
{
    internal class InterfaceUsageWarning : BaseWarning
    {
        public override TreeNodeType AppliesTo
        {
            get { return TreeNodeType.Method; }
        }

        public override void EvaluateNode(OTreeNode node)
        {
            MethodDefinition m = (MethodDefinition)node.Definition;
            foreach (Mono.Cecil.Cil.Instruction i in m.Body.Instructions)
            {
                if (i.OpCode.Code == Mono.Cecil.Cil.Code.Calli)
                {
                    Log.WriteLine("Warning: " + NameBuilder.BuildMethodName(m) + " Uses an interface");
                    node.Warnings.Add(this);
                    return;
                }
            }
        }

        public override string Name
        {
            get { return "Interface Usage Warning"; }
        }

        public override string Description
        {
            get { return "This method utalizes Interfaces. Interfaces are not currently supported by IL2CPU."; }
        }
    }
}
