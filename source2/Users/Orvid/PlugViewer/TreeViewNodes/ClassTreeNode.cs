using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;
using System.Windows.Forms;

namespace PlugViewer.TreeViewNodes
{
    internal enum ClassType
    {
        Class,
        Enum,
        Interface,
        Struct,
        ImplementedInterface,
        Exception,
        Operator,
    }

    internal enum Access
    {
        Public,
        Private,
        Protected,
        Internal,
    }

    internal class ClassTreeNode : OTreeNode
    {
        public ClassTreeNode(TypeReference definition, ClassType type, Access access) : base(TreeNodeType.Class)
        {
            this.def = definition;
            this.acc = access;
            this.tp = type;
            this.Text = definition.Name;
            switch (type)
            {
                case ClassType.Class:
                    {
                        switch (access)
                        {
                            case Access.Public:
                                this.SelectedImageIndex = Constants.Class_Public;
                                this.ImageIndex = Constants.Class_Public;
                                break;
                            case Access.Private:
                                this.SelectedImageIndex = Constants.Class_Private;
                                this.ImageIndex = Constants.Class_Private;
                                break;
                            case Access.Protected:
                                this.SelectedImageIndex = Constants.Class_Protected;
                                this.ImageIndex = Constants.Class_Protected;
                                break;
                            case Access.Internal:
                                this.SelectedImageIndex = Constants.Class_Internal;
                                this.ImageIndex = Constants.Class_Internal;
                                break;
                        }
                        break;
                    }
                case ClassType.Struct:
                    {
                        switch (access)
                        {
                            case Access.Public:
                                this.SelectedImageIndex = Constants.Struct_Public;
                                this.ImageIndex = Constants.Struct_Public;
                                break;
                            case Access.Private:
                                this.SelectedImageIndex = Constants.Struct_Private;
                                this.ImageIndex = Constants.Struct_Private;
                                break;
                            case Access.Protected:
                                this.SelectedImageIndex = Constants.Struct_Protected;
                                this.ImageIndex = Constants.Struct_Protected;
                                break;
                            case Access.Internal:
                                this.SelectedImageIndex = Constants.Struct_Internal;
                                this.ImageIndex = Constants.Struct_Internal;
                                break;
                        }
                        break;
                    }
                case ClassType.Enum:
                    {
                        switch (access)
                        {
                            case Access.Public:
                                this.SelectedImageIndex = Constants.Enum_Public;
                                this.ImageIndex = Constants.Enum_Public;
                                break;
                            case Access.Private:
                                this.SelectedImageIndex = Constants.Enum_Private;
                                this.ImageIndex = Constants.Enum_Private;
                                break;
                            case Access.Protected:
                                this.SelectedImageIndex = Constants.Enum_Protected;
                                this.ImageIndex = Constants.Enum_Protected;
                                break;
                            case Access.Internal:
                                this.SelectedImageIndex = Constants.Enum_Internal;
                                this.ImageIndex = Constants.Enum_Internal;
                                break;
                        }
                        break;
                    }
                case ClassType.Interface:
                    {

                        switch (access)
                        {
                            case Access.Public:
                                this.SelectedImageIndex = Constants.Interface_Public;
                                this.ImageIndex = Constants.Interface_Public;
                                break;
                            case Access.Private:
                                this.SelectedImageIndex = Constants.Interface_Private;
                                this.ImageIndex = Constants.Interface_Private;
                                break;
                            case Access.Protected:
                                this.SelectedImageIndex = Constants.Interface_Protected;
                                this.ImageIndex = Constants.Interface_Protected;
                                break;
                            case Access.Internal:
                                this.SelectedImageIndex = Constants.Interface_Internal;
                                this.ImageIndex = Constants.Interface_Internal;
                                break;
                        }
                        break;
                    }
                case ClassType.ImplementedInterface:
                    {

                        switch (access)
                        {
                            case Access.Public:
                                this.SelectedImageIndex = Constants.InterfaceImpl_Public;
                                this.ImageIndex = Constants.InterfaceImpl_Public;
                                break;
                            case Access.Private:
                                this.SelectedImageIndex = Constants.InterfaceImpl_Private;
                                this.ImageIndex = Constants.InterfaceImpl_Private;
                                break;
                            case Access.Protected:
                                this.SelectedImageIndex = Constants.InterfaceImpl_Protected;
                                this.ImageIndex = Constants.InterfaceImpl_Protected;
                                break;
                            case Access.Internal:
                                this.SelectedImageIndex = Constants.InterfaceImpl_Internal;
                                this.ImageIndex = Constants.InterfaceImpl_Internal;
                                break;
                        }
                        break;
                    }

            }
#if DebugTreeNodeLoading
            Log.WriteLine("Type '" + this.Text + "' was loaded.");
#endif
        }

        public override TreeNodeType Type
        {
            get { return TreeNodeType.Class; }
        }

        private TypeReference def;
        private ClassType tp;
        private Access acc;

        public ClassType TypeOfClass
        {
            get { return tp; }
        }

        public Access AccessModifier
        {
            get { return acc; }
        }

        public override object Definition
        {
            get { return (object)def; }
        }

        public override void ShowNodeInfo(RichTextBox rtb)
        {
            StringBuilder sb = new StringBuilder();
            switch (tp)
            {
                case ClassType.Class:
                    {
                        sb.AppendLine("Type '" + def.Name + "' contains:");
                        sb.AppendLine(def.Resolve().NestedTypes.Count.ToString() + " Nested Types,");
                        sb.AppendLine(def.Resolve().Properties.Count.ToString() + " Properties,");
                        sb.AppendLine(def.Resolve().Methods.Count.ToString() + " Methods,");
                        sb.AppendLine(def.Resolve().Fields.Count.ToString() + " Fields,");
                        sb.AppendLine(def.Resolve().Events.Count.ToString() + " Events,");
                        sb.AppendLine(def.Resolve().CustomAttributes.Count.ToString() + " Attributes,");
                        sb.AppendLine(def.Resolve().GenericParameters.Count.ToString() + " Generic Parameters,");
                        sb.AppendLine("Implements " + def.Resolve().Interfaces.Count.ToString() + " Interfaces,");
                        sb.AppendLine("and has an access modifier of '" + acc.ToString() + "'");
                        sb.AppendLine();
                        sb.AppendLine();
                    }
                    break;
                case ClassType.Enum:
                    {
                        sb.AppendLine("Enum '" + def.Name + "' contains:");
                        sb.AppendLine((def.Resolve().Fields.Count - 1).ToString() + " Values,");
                        sb.AppendLine(def.Resolve().CustomAttributes.Count.ToString() + " Attributes,");
                        sb.AppendLine("and has an access modifier of '" + acc.ToString() + "'");
                        sb.AppendLine();
                        sb.AppendLine();
                    }
                    break;
                case ClassType.Interface:
                    {
                        sb.AppendLine("Interface '" + def.Name + "' contains:");
                        sb.AppendLine(def.Resolve().NestedTypes.Count.ToString() + " Nested Types,");
                        sb.AppendLine(def.Resolve().Properties.Count.ToString() + " Properties,");
                        sb.AppendLine(def.Resolve().Methods.Count.ToString() + " Methods,");
                        sb.AppendLine(def.Resolve().Fields.Count.ToString() + " Fields,");
                        sb.AppendLine(def.Resolve().Events.Count.ToString() + " Events,");
                        sb.AppendLine(def.Resolve().CustomAttributes.Count.ToString() + " Attributes,");
                        sb.AppendLine(def.Resolve().GenericParameters.Count.ToString() + " Generic Parameters,");
                        sb.AppendLine("Implements " + def.Resolve().Interfaces.Count.ToString() + " Interfaces,");
                        sb.AppendLine("and has an access modifier of '" + acc.ToString() + "'");
                        sb.AppendLine();
                        sb.AppendLine();
                    }
                    break;
                case ClassType.ImplementedInterface:
                    {
                        sb.AppendLine("Implemented Interface '" + def.Name + "' contains:");
                        sb.AppendLine(def.Resolve().NestedTypes.Count.ToString() + " Nested Types,");
                        sb.AppendLine(def.Resolve().Properties.Count.ToString() + " Properties,");
                        sb.AppendLine(def.Resolve().Methods.Count.ToString() + " Methods,");
                        sb.AppendLine(def.Resolve().Fields.Count.ToString() + " Fields,");
                        sb.AppendLine(def.Resolve().Events.Count.ToString() + " Events,");
                        sb.AppendLine(def.Resolve().CustomAttributes.Count.ToString() + " Attributes,");
                        sb.AppendLine(def.Resolve().GenericParameters.Count.ToString() + " Generic Parameters,");
                        sb.AppendLine("and has an access modifier of '" + acc.ToString() + "'");
                        sb.AppendLine();
                        sb.AppendLine();
                    }
                    break;
                case ClassType.Struct:
                    {
                        sb.AppendLine("Struct '" + def.Name + "' contains:");
                        sb.AppendLine(def.Resolve().NestedTypes.Count.ToString() + " Nested Types,");
                        sb.AppendLine(def.Resolve().Properties.Count.ToString() + " Properties,");
                        sb.AppendLine(def.Resolve().Methods.Count.ToString() + " Methods,");
                        sb.AppendLine(def.Resolve().Fields.Count.ToString() + " Fields,");
                        sb.AppendLine(def.Resolve().Events.Count.ToString() + " Events,");
                        sb.AppendLine(def.Resolve().CustomAttributes.Count.ToString() + " Attributes,");
                        sb.AppendLine(def.Resolve().GenericParameters.Count.ToString() + " Generic Parameters,");
                        sb.AppendLine("Implements " + def.Resolve().Interfaces.Count.ToString() + " Interfaces,");
                        sb.AppendLine("and has an access modifier of '" + acc.ToString() + "'");
                        sb.AppendLine();
                        sb.AppendLine();
                    }
                    break;
            }

            rtb.Text = sb.ToString();
        }
    }
}
