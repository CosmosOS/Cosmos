using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using PlugViewer.TreeViewNodes;

namespace PlugViewer
{
    class TreeViewSorter : IComparer<TreeNode>, System.Collections.IComparer
    {
        private StringComparer sc = StringComparer.CurrentCulture;

        public int Compare(TreeNode x, TreeNode y)
        {
            OTreeNode ox = (OTreeNode)x;
            OTreeNode oy = (OTreeNode)y;
            if (ox.Type == TreeNodeType.Assembly && oy.Type == TreeNodeType.Assembly)
            {
                return 0;
            }
            else if (ox.Type == TreeNodeType.Namespace && oy.Type == TreeNodeType.Namespace)
            {
                return sc.Compare(ox.Text, oy.Text);
            }
            else if (ox.Type == TreeNodeType.Module && oy.Type == TreeNodeType.Module)
            {
                return sc.Compare(ox.Text, oy.Text);
            }
            else
            {
                int xVal, yVal;

                #region Setup x
                if (ox.Type == TreeNodeType.Field)
                {
                    xVal = 10;
                }
                else if (ox.Type == TreeNodeType.Event)
                {
                    xVal = 20;
                }
                else if (ox.Type == TreeNodeType.Property)
                {
                    xVal = 30;
                }
                else if (ox.Type == TreeNodeType.Method)
                {
                    xVal = 40;
                }
                else if (ox.Type == TreeNodeType.Class)
                {
                    xVal = 50;
                }
                else if (ox.Type == TreeNodeType.Namespace)
                {
                    xVal = 60;
                }
                else if (ox.Type == TreeNodeType.Module)
                {
                    xVal = 70;
                }
                else if (ox.Type == TreeNodeType.Assembly)
                {
                    xVal = 80;
                }
                else
                {
                    xVal = 90;
                }
                #endregion

                #region Setup y
                if (oy.Type == TreeNodeType.Field)
                {
                    yVal = 10;
                }
                else if (oy.Type == TreeNodeType.Event)
                {
                    yVal = 20;
                }
                else if (oy.Type == TreeNodeType.Property)
                {
                    yVal = 30;
                }
                else if (oy.Type == TreeNodeType.Method)
                {
                    yVal = 40;
                }
                else if (oy.Type == TreeNodeType.Class)
                {
                    yVal = 50;
                }
                else if (oy.Type == TreeNodeType.Namespace)
                {
                    yVal = 60;
                }
                else if (oy.Type == TreeNodeType.Module)
                {
                    yVal = 70;
                }
                else if (oy.Type == TreeNodeType.Assembly)
                {
                    yVal = 80;
                }
                else
                {
                    yVal = 90;
                }
                #endregion

                if (xVal == yVal)
                {
                    return sc.Compare(ox.Text, oy.Text);
                }
                else
                {
                    return (xVal > yVal ? -1 : 1);
                }
            }
        }

        int System.Collections.IComparer.Compare(object x, object y)
        {
            return this.Compare((TreeNode)x, (TreeNode)y);
        }
    }
}
