using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

namespace PlugViewer.TreeViewNodes
{
    public enum MethodType
    {
        BasicMethod,
        VirtualMethod,
        OverrideMethod,
    }

    internal class MethodTreeNode : OTreeNode
    {
        public MethodTreeNode(MethodInfo definition, MethodType mType, Access mAccess) : base(TreeNodeType.Method)
        {
            this.def = definition;
            this.Text = NameBuilder.BuildMethodDisplayName(definition);
            this.mType = mType;
            this.acc = mAccess;
            switch (mType)
            {
                case MethodType.BasicMethod:
                    {
                        switch (mAccess)
                        {
                            case Access.Public:
                                this.SelectedImageIndex = Constants.Method_Public;
                                this.ImageIndex = Constants.Method_Public;
                                break;
                            case Access.Private:
                                this.SelectedImageIndex = Constants.Method_Private;
                                this.ImageIndex = Constants.Method_Private;
                                break;
                            case Access.Protected:
                                this.SelectedImageIndex = Constants.Method_Protected;
                                this.ImageIndex = Constants.Method_Protected;
                                break;
                            case Access.Internal:
                                this.SelectedImageIndex = Constants.Method_Internal;
                                this.ImageIndex = Constants.Method_Internal;
                                break;
                        }
                    }
                    break;
                case MethodType.OverrideMethod:
                    {

                        switch (mAccess)
                        {
                            case Access.Public:
                                this.SelectedImageIndex = Constants.MethodOverride_Public;
                                this.ImageIndex = Constants.MethodOverride_Public;
                                break;
                            case Access.Private:
                                this.SelectedImageIndex = Constants.MethodOverride_Private;
                                this.ImageIndex = Constants.MethodOverride_Private;
                                break;
                            case Access.Protected:
                                this.SelectedImageIndex = Constants.MethodOverride_Protected;
                                this.ImageIndex = Constants.MethodOverride_Protected;
                                break;
                            case Access.Internal:
                                this.SelectedImageIndex = Constants.MethodOverride_Internal;
                                this.ImageIndex = Constants.MethodOverride_Internal;
                                break;
                        }
                    }
                    break;
                case MethodType.VirtualMethod:
                    {

                        switch (mAccess)
                        {
                            case Access.Public:
                                this.SelectedImageIndex = Constants.MethodVirtual_Public;
                                this.ImageIndex = Constants.MethodVirtual_Public;
                                break;
                            case Access.Private:
                                this.SelectedImageIndex = Constants.MethodVirtual_Private;
                                this.ImageIndex = Constants.MethodVirtual_Private;
                                break;
                            case Access.Protected:
                                this.SelectedImageIndex = Constants.MethodVirtual_Protected;
                                this.ImageIndex = Constants.MethodVirtual_Protected;
                                break;
                            case Access.Internal:
                                this.SelectedImageIndex = Constants.MethodVirtual_Internal;
                                this.ImageIndex = Constants.MethodVirtual_Internal;
                                break;
                        }
                    }
                    break;
            }
#if DebugTreeNodeLoading
            Log.WriteLine("Method '" + this.Text + "' was loaded.");
#endif
        }

        public override TreeNodeType Type
        {
            get { return TreeNodeType.Method; }
        }

        private MethodInfo def;
        private MethodType mType;
        private Access acc;

        public Access AccessModifier
        {
            get { return acc; }
        }

        public MethodType TypeOfMethod
        {
            get { return mType; }
        }

        public override object Definition
        {
            get { return (object)def; }
        }

        public override void ShowNodeInfo(RichTextBox rtb)
        {
            StringBuilder sb = new StringBuilder();
            switch (mType)
            {
                case MethodType.BasicMethod:
                    {
                        sb.AppendLine("Basic Method '" + this.Text + "' contains:");
                        sb.AppendLine(def.GetGenericArguments().Length.ToString() + " Generic Parameters,");
                        sb.AppendLine(def.GetParameters().Length.ToString() + " Parameters,");
                        //sb.AppendLine(def.SecurityDeclarations.Count.ToString() + " Security Declarations,");
                        sb.AppendLine(def.GetCustomAttributes(true).Length.ToString() + " Custom Attributes,");
                        if (def.GetMethodBody() != null)
                        {
                            sb.AppendLine(def.GetMethodBody().GetILAsByteArray().Length.ToString() + " Instructions,");
                            sb.AppendLine(def.GetMethodBody().ExceptionHandlingClauses.Count.ToString() + " Exception Handlers,");
                            sb.AppendLine(def.GetMethodBody().LocalVariables.Count.ToString() + " Variables,");
                        }
                        else
                        {
                            sb.AppendLine("Doesn't have a body.");
                        }
                        sb.AppendLine("Has a calling convention of '" + def.CallingConvention.ToString() + "'");
                        sb.AppendLine();
                        sb.AppendLine();
                    }
                    break;
                case MethodType.OverrideMethod:
                    {
                        sb.AppendLine("Override Method '" + this.Text + "' contains:");
                        sb.AppendLine(def.GetGenericArguments().Length.ToString() + " Generic Parameters,");
                        sb.AppendLine(def.GetParameters().Length.ToString() + " Parameters,");
                        //sb.AppendLine(def.SecurityDeclarations.Count.ToString() + " Security Declarations,");
                        sb.AppendLine(def.GetCustomAttributes(true).Length.ToString() + " Custom Attributes,");
                        if (def.GetMethodBody() != null)
                        {
                            sb.AppendLine(def.GetMethodBody().GetILAsByteArray().Length.ToString() + " Instructions,");
                            sb.AppendLine(def.GetMethodBody().ExceptionHandlingClauses.Count.ToString() + " Exception Handlers,");
                            sb.AppendLine(def.GetMethodBody().LocalVariables.Count.ToString() + " Variables,");
                        }
                        else
                        {
                            sb.AppendLine("Doesn't have a body.");
                        }
                        //sb.AppendLine("Overrides " + def..Resolve().Overrides.Count.ToString() + " methods,");
                        sb.AppendLine("Has a calling convention of '" + def.CallingConvention.ToString() + "'");
                        sb.AppendLine();
                        sb.AppendLine();
                    }
                    break;
                case MethodType.VirtualMethod:
                    {
                        sb.AppendLine("Virtual Method '" + this.Text + "' contains:");
                        sb.AppendLine(def.GetGenericArguments().Length.ToString() + " Generic Parameters,");
                        sb.AppendLine(def.GetParameters().Length.ToString() + " Parameters,");
                        //sb.AppendLine(def.SecurityDeclarations.Count.ToString() + " Security Declarations,");
                        sb.AppendLine(def.GetCustomAttributes(true).Length.ToString() + " Custom Attributes,");
                        if (def.GetMethodBody() != null)
                        {
                            sb.AppendLine(def.GetMethodBody().GetILAsByteArray().Length.ToString() + " Instructions,");
                            sb.AppendLine(def.GetMethodBody().ExceptionHandlingClauses.Count.ToString() + " Exception Handlers,");
                            sb.AppendLine(def.GetMethodBody().LocalVariables.Count.ToString() + " Variables,");
                        }
                        else
                        {
                            sb.AppendLine("Doesn't have a body.");
                        }
                        sb.AppendLine("Has a calling convention of '" + def.CallingConvention.ToString() + "'");
                        sb.AppendLine();
                        sb.AppendLine();
                    }
                    break;
            }

            rtb.Text = sb.ToString();
        }
    }
}
