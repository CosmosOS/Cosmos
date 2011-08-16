using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Mono.Cecil;
using System.IO;
using PlugViewer.TreeViewNodes;

namespace PlugViewer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void AddAssemblyToView(AssemblyDefinition a)
        {
            TreeNode nd = new AssemblyTreeNode(a);
            treeView1.Nodes.Add(nd);
            foreach (ModuleDefinition md in a.Modules)
            {
                LoadModule(nd, md);
            }
        }

        private void LoadModule(TreeNode parent, ModuleDefinition md)
        {
            ModuleTreeNode tn = new ModuleTreeNode(md);
            parent.Nodes.Add(tn);
            foreach (TypeDefinition t in md.Types)
            {
                if (!tn.Namespaces.ContainsKey(t.Namespace))
                {
                    TreeNode tnd = new NamespaceTreeNode(t.Namespace);
                    tn.Nodes.Add(tnd);
                    tn.Namespaces.Add(t.Namespace, tnd);
                }
                LoadType(tn.Namespaces[t.Namespace], t);
            }
        }

        private void LoadType(TreeNode parentNode, TypeDefinition type)
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
                else if (type.IsNestedFamily)
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
                foreach (TypeDefinition ntd in type.NestedTypes)
                {
                    LoadType(node, ntd);
                }
                foreach (MethodDefinition md in type.Methods)
                {
                    LoadMethod(node, md);
                }
                foreach (TypeReference ntd in type.Interfaces)
                {
                    LoadImplementedInterface(node, ntd);
                }
                foreach (EventDefinition ed in type.Events)
                {
                    LoadEvent(node, ed);
                }
                foreach (PropertyDefinition pd in type.Properties)
                {
                    LoadProperty(node, pd);
                }
                foreach (FieldDefinition fd in type.Fields)
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

        private void LoadMethod(TreeNode parentNode, MethodDefinition method)
        {
            string dispkey = NameBuilder.BuildMethodDisplayName(method);
            string fullkey = (method.FullName + NameBuilder.BuildMethodDisplayName(method).Substring(method.Name.Length));
            TreeNode tn;
            // Check that the method isn't part of a property or event definition.
            if (!method.IsGetter && !method.IsSetter)
            {
                if (method.IsVirtual)
                {
                    if (method.HasBody) // The method is an override
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
                        else if (method.IsInternalCall)
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
                        else if (method.IsInternalCall)
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
                    else if (method.IsInternalCall)
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

        private void LoadImplementedInterface(TreeNode parentNode, TypeReference intface)
        {
            TypeDefinition type = intface.Resolve();
            TreeNode node;
            if (intface.Resolve().IsPublic)
            {
                node = new ClassTreeNode(intface, ClassType.ImplementedInterface, Access.Public);
                parentNode.Nodes.Add(node);
            }
            else if (intface.Resolve().IsNestedPrivate)
            {
                node = new ClassTreeNode(intface, ClassType.ImplementedInterface, Access.Private);
                parentNode.Nodes.Add(node);
            }
            else if (intface.Resolve().IsNestedFamily)
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

            foreach (TypeDefinition ntd in type.NestedTypes)
            {
                LoadType(node, ntd);
            }
            foreach (MethodDefinition md in type.Methods)
            {
                LoadMethod(node, md);
            }
            foreach (TypeReference ntd in type.Interfaces)
            {
                LoadImplementedInterface(node, ntd);
            }
            foreach (EventDefinition ed in type.Events)
            {
                LoadEvent(node, ed);
            }
            foreach (PropertyDefinition pd in type.Properties)
            {
                LoadProperty(node, pd);
            }
            foreach (FieldDefinition fd in type.Fields)
            {
                LoadField(node, fd);
            }
        }

        private void LoadInterface(TreeNode parentNode, TypeReference intface)
        {
            TypeDefinition type = intface.Resolve();
            TreeNode node;
            if (intface.Resolve().IsPublic)
            {
                node = new ClassTreeNode(intface, ClassType.Interface, Access.Public);
                parentNode.Nodes.Add(node);
            }
            else if (intface.Resolve().IsNestedPrivate)
            {
                node = new ClassTreeNode(intface, ClassType.Interface, Access.Private);
                parentNode.Nodes.Add(node);
            }
            else if (intface.Resolve().IsNestedFamily)
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

            foreach (TypeDefinition ntd in type.NestedTypes)
            {
                LoadType(node, ntd);
            }
            foreach (MethodDefinition md in type.Methods)
            {
                LoadMethod(node, md);
            }
            foreach (TypeReference ntd in type.Interfaces)
            {
                LoadImplementedInterface(node, ntd);
            }
            foreach (EventDefinition ed in type.Events)
            {
                LoadEvent(node, ed);
            }
            foreach (PropertyDefinition pd in type.Properties)
            {
                LoadProperty(node, pd);
            }
            foreach (FieldDefinition fd in type.Fields)
            {
                LoadField(node, fd);
            }
        }

        private void LoadEvent(TreeNode parentNode, EventDefinition evnt)
        {
            TreeNode n = new EventTreeNode(evnt);
            parentNode.Nodes.Add(n);
        }

        private void LoadProperty(TreeNode parentNode, PropertyDefinition ptd)
        {
            TreeNode n;
            if (ptd.HasConstant)
                LoadConstant(parentNode, ptd);
            if (ptd.SetMethod != null)
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

        private void LoadConstant(TreeNode parentNode, PropertyDefinition ptd)
        {
            //TreeNode tr = new FieldTreeNode(fd, Access.Protected, false);
            //parentNode.Nodes.Add(tr);
            parentNode.Nodes.Add(ptd.FullName, ptd.Name, Constants.ConstantIcon, Constants.ConstantIcon);
        }

        private void LoadEnum(TreeNode parentNode, TypeDefinition td)
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
            else if (td.IsNestedFamily)
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
            foreach (FieldDefinition fd in td.Fields)
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

        private void LoadStruct(TreeNode parentNode, TypeDefinition td)
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
            else if (td.IsNestedFamily)
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


            foreach (TypeDefinition ntd in td.NestedTypes)
            {
                LoadType(node, ntd);
            }
            foreach (MethodDefinition md in td.Methods)
            {
                LoadMethod(node, md);
            }
            foreach (TypeReference ntd in td.Interfaces)
            {
                LoadImplementedInterface(node, ntd);
            }
            foreach (EventDefinition ed in td.Events)
            {
                LoadEvent(node, ed);
            }
            foreach (PropertyDefinition pd in td.Properties)
            {
                LoadProperty(node, pd);
            }
            foreach (FieldDefinition fd in td.Fields)
            {
                LoadField(node, fd);
            }
        }

        private void LoadField(TreeNode parentNode, FieldDefinition fd)
        {
            TreeNode tr;
            if (fd.HasConstant)
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
            else if (fd.IsFamily)
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

        private void OpenDll(string loc)
        {
            try
            {
                AssemblyDefinition asmb = AssemblyDefinition.ReadAssembly(loc);
                AddAssemblyToView(asmb);
            }
            catch { }
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
