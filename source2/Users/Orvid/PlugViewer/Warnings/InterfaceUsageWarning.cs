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
            if (m.HasBody)
            {
                foreach (Mono.Cecil.Cil.VariableDefinition i in m.Body.Variables)
                {
                    if (!i.VariableType.IsGenericParameter && !i.VariableType.IsGenericInstance)
                    {
                        if (i.VariableType.IsArray)
                        {
                            if (((TypeSpecification)i.VariableType).ElementType.IsGenericParameter || ((TypeSpecification)i.VariableType).ElementType.IsGenericInstance)
                            {
                                continue;
                            }
                        }
                        if (i.VariableType.Resolve().IsInterface)
                        {
#if DebugWarnings
                            Log.WriteLine("Warning: " + NameBuilder.BuildMethodName(m) + " Uses an interface");
#endif
                            if (node.SelectedImageIndex != Constants.ErrorIcon)
                            {
                                node.SelectedImageIndex = Constants.WarningIcon;
                                node.ImageIndex = Constants.WarningIcon;
                            }
                            node.Warnings.Add(this);
                            return;
                        }
                    }
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
