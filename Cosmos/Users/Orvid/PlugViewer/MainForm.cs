using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using PlugViewer.TreeViewNodes;

namespace PlugViewer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            treeView1.TreeViewNodeSorter = new TreeViewSorter();
        }

        private void AddAssemblyToView(Assembly a)
        {
            if (a != null)
            {
                PlugTemplateDumper.Dump(a);
                try
                {
                    TreeNode nd = new AssemblyTreeNode(a);
                    treeView1.Nodes.Add(nd);
                    foreach (Module md in a.GetModules())
                    {
                        LoadModule(nd, md);
                    }
                    treeView1.Sort();
                }
                catch {}
            }
        }

        #region LoadModule
        private void LoadModule(TreeNode parent, Module md)
        {
            ModuleTreeNode tn = new ModuleTreeNode(md);
            parent.Nodes.Add(tn);
            foreach (Type t in md.GetTypes())
            {
                if (!tn.Namespaces.ContainsKey((t.Namespace == null ? "-" : t.Namespace)))
                {
                    TreeNode tnd = new NamespaceTreeNode((t.Namespace == null ? "-" : t.Namespace));
                    tn.Nodes.Add(tnd);
                    tn.Namespaces.Add((t.Namespace == null ? "-" : t.Namespace), tnd);
                }
                LoadType(tn.Namespaces[(t.Namespace == null ? "-" : t.Namespace)], t);
            }
        }
        #endregion

        #region LoadType
        private void LoadType(TreeNode parentNode, Type type)
        {
            if (!type.IsEnum && !type.IsInterface && !type.IsValueType && type.IsClass)
            {
                TreeNode node;
                if (type.IsPublic)
                {
                    node = new ClassTreeNode(type, ClassType.Class, Access.Public);
                    parentNode.Nodes.Add(node);
                }
                else if (type.IsNestedPrivate)
                {
                    node = new ClassTreeNode(type, ClassType.Class, Access.Private);
                    parentNode.Nodes.Add(node);
                }
                else if (type.IsNestedFamORAssem)
                {
                    node = new ClassTreeNode(type, ClassType.Class, Access.Internal);
                    parentNode.Nodes.Add(node);
                }
                else
                {
                    // It must be Protected.
                    node = new ClassTreeNode(type, ClassType.Class, Access.Protected);
                    parentNode.Nodes.Add(node);
                }


                //TreeNode node = (treeView1.Nodes.Find(type.FullName, true))[0];
                foreach (Type ntd in type.GetNestedTypes())
                {
                    LoadType(node, ntd);
                }
                foreach (MethodInfo md in type.GetMethods())
                {
                    LoadMethod(node, md);
                }
                foreach (Type ntd in type.GetInterfaces())
                {
                    LoadImplementedInterface(node, ntd);
                }
                foreach (EventInfo ed in type.GetEvents())
                {
                    LoadEvent(node, ed);
                }
                foreach (PropertyInfo pd in type.GetProperties())
                {
                    LoadProperty(node, pd);
                }
                foreach (FieldInfo fd in type.GetFields())
                {
                    LoadField(node, fd);
                }
            }
            else if (type.IsEnum)
            {
                LoadEnum(parentNode, type);
            }
            else if (type.IsInterface)
            {
                LoadInterface(parentNode, type.GetElementType());
            }
            else if (type.IsValueType)
            {
                LoadStruct(parentNode, type);
            }
            else
            {
                throw new Exception();
            }
        }
        #endregion

        /// <summary>
        /// Checks if the method is part of an event or property definition.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private bool ShouldLoadMethod(MethodInfo method)
        {
            string s;
            if (method.Name.Length <= 4) // Too short to be one of things being tested
            {
                return true;
            }
            else if (method.Name.Length < 7) // only check add, get, and set
            {
                s = method.Name.Substring(0,4);
                if (s == "add_" || s == "get_" || s == "set_")
                {
                    return false;
                }
                return true;
            }
            else // Check add, remove, get, and set
            {
                s = method.Name.Substring(0, 4);
                if (s == "add_" || s == "get_" || s == "set_")
                {
                    return false;
                }
                else if (method.Name.Substring(0, 7) == "remove_")
                {
                    return false;
                }
                return true;
            }

        }

        private void LoadMethod(TreeNode parentNode, MethodInfo method)
        {
            string dispkey = NameBuilder.BuildMethodDisplayName(method);
            TreeNode tn;
            // Check that the method isn't part of a property or event definition.
            if (ShouldLoadMethod(method))
            {
                if (method.IsVirtual)
                {
                    if (method.GetMethodBody() != null) // The method is an override
                    {
                        if (method.IsPublic)
                        {
                            tn = new MethodTreeNode(method, MethodType.OverrideMethod, Access.Public);
                            parentNode.Nodes.Add(tn);
                        }
                        else if (method.IsPrivate)
                        {
                            tn = new MethodTreeNode(method, MethodType.OverrideMethod, Access.Private);
                            parentNode.Nodes.Add(tn);
                        }
                        else if (method.IsFamilyOrAssembly)
                        {
                            tn = new MethodTreeNode(method, MethodType.OverrideMethod, Access.Internal);
                            parentNode.Nodes.Add(tn);
                        }
                        else
                        {
                            // It must be protected.
                            tn = new MethodTreeNode(method, MethodType.OverrideMethod, Access.Protected);
                            parentNode.Nodes.Add(tn);
                        }
                    }
                    else // The method is virtual, but not an override.
                    {
                        if (method.IsPublic)
                        {
                            tn = new MethodTreeNode(method, MethodType.VirtualMethod, Access.Public);
                            parentNode.Nodes.Add(tn);
                        }
                        else if (method.IsPrivate)
                        {
                            tn = new MethodTreeNode(method, MethodType.VirtualMethod, Access.Private);
                            parentNode.Nodes.Add(tn);
                        }
                        else if (method.IsFamilyOrAssembly)
                        {
                            tn = new MethodTreeNode(method, MethodType.VirtualMethod, Access.Internal);
                            parentNode.Nodes.Add(tn);
                        }
                        else
                        {
                            // It must be protected.
                            tn = new MethodTreeNode(method, MethodType.VirtualMethod, Access.Protected);
                            parentNode.Nodes.Add(tn);
                        }
                    }
                }
                else // The method is a regular method
                {
                    if (method.IsPublic)
                    {
                        tn = new MethodTreeNode(method, MethodType.BasicMethod, Access.Public);
                        parentNode.Nodes.Add(tn);
                    }
                    else if (method.IsPrivate)
                    {
                        tn = new MethodTreeNode(method, MethodType.BasicMethod, Access.Private);
                        parentNode.Nodes.Add(tn);
                    }
                    else if (method.IsFamilyOrAssembly)
                    {
                        tn = new MethodTreeNode(method, MethodType.BasicMethod, Access.Internal);
                        parentNode.Nodes.Add(tn);
                    }
                    else
                    {
                        // It must be protected.
                        tn = new MethodTreeNode(method, MethodType.BasicMethod, Access.Protected);
                        parentNode.Nodes.Add(tn);
                    }
                }
            }
        }

        #region LoadImplementedInterface
        private void LoadImplementedInterface(TreeNode parentNode, Type intface)
        {
            TreeNode node;
            if (intface.IsPublic)
            {
                node = new ClassTreeNode(intface, ClassType.ImplementedInterface, Access.Public);
                parentNode.Nodes.Add(node);
            }
            else if (intface.IsNestedPrivate)
            {
                node = new ClassTreeNode(intface, ClassType.ImplementedInterface, Access.Private);
                parentNode.Nodes.Add(node);
            }
            else if (intface.IsNestedFamORAssem)
            {
                node = new ClassTreeNode(intface, ClassType.ImplementedInterface, Access.Internal);
                parentNode.Nodes.Add(node);
            }
            else
            {
                // It must be protected.
                node = new ClassTreeNode(intface, ClassType.ImplementedInterface, Access.Protected);
                parentNode.Nodes.Add(node);
            }

            foreach (Type ntd in intface.GetNestedTypes())
            {
                LoadType(node, ntd);
            }
            foreach (MethodInfo md in intface.GetMethods())
            {
                LoadMethod(node, md);
            }
            foreach (Type ntd in intface.GetInterfaces())
            {
                LoadImplementedInterface(node, ntd);
            }
            foreach (EventInfo ed in intface.GetEvents())
            {
                LoadEvent(node, ed);
            }
            foreach (PropertyInfo pd in intface.GetProperties())
            {
                LoadProperty(node, pd);
            }
            foreach (FieldInfo fd in intface.GetFields())
            {
                LoadField(node, fd);
            }
        }
        #endregion

        #region LoadInterface
        private void LoadInterface(TreeNode parentNode, Type intface)
        {
            TreeNode node;
            if (intface.IsPublic)
            {
                node = new ClassTreeNode(intface, ClassType.Interface, Access.Public);
                parentNode.Nodes.Add(node);
            }
            else if (intface.IsNestedPrivate)
            {
                node = new ClassTreeNode(intface, ClassType.Interface, Access.Private);
                parentNode.Nodes.Add(node);
            }
            else if (intface.IsNestedFamORAssem)
            {
                node = new ClassTreeNode(intface, ClassType.Interface, Access.Internal);
                parentNode.Nodes.Add(node);
            }
            else
            {
                // It must be protected.
                node = new ClassTreeNode(intface, ClassType.Interface, Access.Protected);
                parentNode.Nodes.Add(node);
            }

            foreach (Type ntd in intface.GetNestedTypes())
            {
                LoadType(node, ntd);
            }
            foreach (MethodInfo md in intface.GetMethods())
            {
                LoadMethod(node, md);
            }
            foreach (Type ntd in intface.GetInterfaces())
            {
                LoadImplementedInterface(node, ntd);
            }
            foreach (EventInfo ed in intface.GetEvents())
            {
                LoadEvent(node, ed);
            }
            foreach (PropertyInfo pd in intface.GetProperties())
            {
                LoadProperty(node, pd);
            }
            foreach (FieldInfo fd in intface.GetFields())
            {
                LoadField(node, fd);
            }
        }
        #endregion

        #region LoadEvent
        private void LoadEvent(TreeNode parentNode, EventInfo evnt)
        {
            TreeNode n = new EventTreeNode(evnt);
            parentNode.Nodes.Add(n);
        }
        #endregion

        #region LoadProperty
        private void LoadProperty(TreeNode parentNode, PropertyInfo ptd)
        {
            TreeNode n;
            if (ptd.CanWrite)
            {
                n = new PropertyTreeNode(ptd, true);
                parentNode.Nodes.Add(n);
            }
            else
            {
                n = new PropertyTreeNode(ptd, false);
                parentNode.Nodes.Add(n);
            }
        }
        #endregion

        //private void LoadConstant(TreeNode parentNode, PropertyInfo ptd)
        //{
        //    //TreeNode tr = new FieldTreeNode(fd, Access.Protected, false);
        //    //parentNode.Nodes.Add(tr);
        //    parentNode.Nodes.Add(ptd.FullName, ptd.Name, Constants.ConstantIcon, Constants.ConstantIcon);
        //}

        #region LoadEnum
        private void LoadEnum(TreeNode parentNode, Type td)
        {
            TreeNode enode;
            TreeNode tr;
            if (td.IsPublic)
            {
                enode = new ClassTreeNode(td, ClassType.Enum, Access.Public);
                parentNode.Nodes.Add(enode);
            }
            else if (td.IsNestedPrivate)
            {
                enode = new ClassTreeNode(td, ClassType.Enum, Access.Private);
                parentNode.Nodes.Add(enode);
            }
            else if (td.IsNestedFamORAssem)
            {
                enode = new ClassTreeNode(td, ClassType.Enum, Access.Internal);
                parentNode.Nodes.Add(enode);
            }
            else
            {
                // It must be protected.
                enode = new ClassTreeNode(td, ClassType.Enum, Access.Protected);
                parentNode.Nodes.Add(enode);
            }
            // Now we get to load it's values.
            foreach (FieldInfo fd in td.GetFields())
            {
                // We don't want to show "value__" because it's automatically added 
                // by the compiler.
                if (fd.Name != "value__")
                {
                    tr = new FieldTreeNode(fd, Access.Public, true);
                    enode.Nodes.Add(tr);
                }
            }
        }
        #endregion

        #region LoadStruct
        private void LoadStruct(TreeNode parentNode, Type td)
        {
            TreeNode node;
            if (td.IsPublic)
            {
                node = new ClassTreeNode(td, ClassType.Struct, Access.Public);
                parentNode.Nodes.Add(node);
            }
            else if (td.IsNestedPrivate)
            {
                node = new ClassTreeNode(td, ClassType.Struct, Access.Private);
                parentNode.Nodes.Add(node);
            }
            else if (td.IsNestedFamORAssem)
            {
                node = new ClassTreeNode(td, ClassType.Struct, Access.Internal);
                parentNode.Nodes.Add(node);
            }
            else
            {
                // It must be protected.
                node = new ClassTreeNode(td, ClassType.Struct, Access.Protected);
                parentNode.Nodes.Add(node);
            }


            foreach (Type ntd in td.GetNestedTypes())
            {
                LoadType(node, ntd);
            }
            foreach (MethodInfo md in td.GetMethods())
            {
                LoadMethod(node, md);
            }
            foreach (Type ntd in td.GetInterfaces())
            {
                LoadImplementedInterface(node, ntd);
            }
            foreach (EventInfo ed in td.GetEvents())
            {
                LoadEvent(node, ed);
            }
            foreach (PropertyInfo pd in td.GetProperties())
            {
                LoadProperty(node, pd);
            }
            foreach (FieldInfo fd in td.GetFields())
            {
                LoadField(node, fd);
            }
        }
        #endregion

        #region LoadField
        private void LoadField(TreeNode parentNode, FieldInfo fd)
        {
            TreeNode tr;
            if (fd.IsLiteral)
            {
                tr = new FieldTreeNode(fd, Access.Public, true);
                parentNode.Nodes.Add(tr);
            }
            else if (fd.IsPublic)
            {
                tr = new FieldTreeNode(fd, Access.Public, false);
                parentNode.Nodes.Add(tr);
            }
            else if (fd.IsPrivate)
            {
                tr = new FieldTreeNode(fd, Access.Private, false);
                parentNode.Nodes.Add(tr);
            }
            else if (fd.IsFamilyOrAssembly)
            {
                tr = new FieldTreeNode(fd, Access.Internal, false);
                parentNode.Nodes.Add(tr);
            }
            else
            {
                // It must be protected.
                tr = new FieldTreeNode(fd, Access.Protected, false);
                parentNode.Nodes.Add(tr);
            }
        }
        #endregion

        private void OpenDll(string loc)
        {
            Assembly asmb = null;
            try
            {
                asmb = Assembly.LoadFrom(loc);
            }
            catch { }
            AddAssemblyToView(asmb);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string str in openFileDialog1.FileNames)
                {
                    OpenDll(str);
                }
                TestRunner.RunTests();
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            OTreeNode n = (OTreeNode)e.Node;
            n.ShowNodeInfo(Rtb);

            bool hErr = false;
            bool hWarn = false;
            int ErrSelStart = 0;
            int WarnSelStart = 0;
            if (n.Errors.Count > 0)
            {
                ErrSelStart = Rtb.Text.Length - 1;
                foreach (Errors.BaseError er in n.Errors)
                {
                    Rtb.Text += er.Name + ": " + er.Description + "\r\n";
                }
                hErr = true;
            }
            if (n.Warnings.Count > 0)
            {
                WarnSelStart = Rtb.Text.Length - 1;
                foreach (Warnings.BaseWarning b in n.Warnings)
                {
                    Rtb.Text += b.Name + ": " + b.Description + "\r\n";
                }
                hWarn = true;
            }
            if (hErr)
            {
                Rtb.SelectionStart = ErrSelStart;
                Rtb.SelectionLength = Rtb.Text.Length - Rtb.SelectionStart;
                Rtb.SelectionColor = System.Drawing.Color.FromArgb(0xff, 0x00, 0x00);
                Rtb.SelectionLength = 0;
            }
            if (hWarn)
            {
                Rtb.SelectionStart = WarnSelStart;
                Rtb.SelectionLength = Rtb.Text.Length - Rtb.SelectionStart;
                Rtb.SelectionColor = System.Drawing.Color.FromArgb(0xff, 0x99, 0x00);
                Rtb.SelectionLength = 0;
            }
        }
    }
}
