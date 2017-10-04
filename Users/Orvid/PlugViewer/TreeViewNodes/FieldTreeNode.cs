using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

namespace PlugViewer.TreeViewNodes
{
    internal class FieldTreeNode : OTreeNode
    {
        public FieldTreeNode(FieldInfo definition, Access mAccess, bool Constant) : base(TreeNodeType.Field)
        {
            this.def = definition;
            acc = mAccess;
            sconst = Constant;
            this.Text = definition.Name;
            if (!Constant)
            {
                switch (acc)
                {
                    case Access.Public:
                        this.SelectedImageIndex = Constants.Field_Public;
                        this.ImageIndex = Constants.Field_Public;
                        break;

                    case Access.Private:
                        this.SelectedImageIndex = Constants.Field_Private;
                        this.ImageIndex = Constants.Field_Private;
                        break;

                    case Access.Protected:
                        this.SelectedImageIndex = Constants.Field_Protected;
                        this.ImageIndex = Constants.Field_Protected;
                        break;

                    case Access.Internal:
                        this.SelectedImageIndex = Constants.Field_Internal;
                        this.ImageIndex = Constants.Field_Internal;
                        break;
                }
#if DebugTreeNodeLoading
                Log.WriteLine("Field '" + this.Text + "' was loaded.");
#endif
            }
            else
            {
                this.SelectedImageIndex = Constants.ConstantIcon;
                this.ImageIndex = Constants.ConstantIcon;
#if DebugTreeNodeLoading
                Log.WriteLine("Constant '" + this.Text + "' was loaded.");
#endif
            }
        }

        public override TreeNodeType Type
        {
            get { return TreeNodeType.Module; }
        }

        private FieldInfo def;
        private Access acc;
        private bool sconst;

        public bool IsConstant
        {
            get { return sconst; }
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
            if (!sconst)
            {
                sb.AppendLine("Field '" + def.Name + "'");
                sb.AppendLine("Has an access modifier of '" + acc.ToString() + "'");
            }
            else
            {
                sb.AppendLine("Constant '" + def.Name + "'");
            }
            sb.AppendLine();
            sb.AppendLine();

            rtb.Text = sb.ToString();
        }
    }
}
