using System;
using System.Collections.Generic;
using System.Text;
using PlugViewer.TreeViewNodes;
using System.Reflection;

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
            MethodInfo m = (MethodInfo)node.Definition;
            if (m.GetMethodBody() != null)
            {
                foreach (LocalVariableInfo i in m.GetMethodBody().LocalVariables)
                {
                    if (!i.LocalType.IsGenericParameter && !i.LocalType.IsGenericType)
                    {
                        if (i.LocalType.IsArray)
                        {
                            if (i.LocalType.GetElementType().IsGenericParameter || i.LocalType.GetElementType().IsGenericType)
                            {
                                continue;
                            }
                        }
                        if (i.LocalType.IsInterface)
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
