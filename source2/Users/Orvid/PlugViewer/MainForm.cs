using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Mono.Cecil;
using System.IO;

namespace PlugViewer
{
    public partial class MainForm : Form
    {
        private Dictionary<string, AssemblyDefinition> LoadedAssemblies = new Dictionary<string, AssemblyDefinition>();
        private Dictionary<string, TreeNode> Namespaces = new Dictionary<string, TreeNode>();
        private List<string> MethodsNeedingPlugging = new List<string>();
        private const int WarningIconIndex = 156;
        private const int ErrorIconIndex = 157;


        public MainForm()
        {
            InitializeComponent();
        }

        private void AddAssemblyToView(AssemblyDefinition a)
        {
            TreeNode nd = treeView1.Nodes.Add(a.FullName, a.Name.Name, 0, 0);
            MethodsNeedingPlugging.Add("Assembly: " + a.Name.Name);
            foreach (ModuleDefinition md in a.Modules)
            {
                LoadModule(nd, md);
            }
            LoadedAssemblies.Add(a.Name.Name, a);
            MethodsNeedingPlugging.Add("");
            MethodsNeedingPlugging.Add("");
            MethodsNeedingPlugging.Add("");
        }

        private void LoadModule(TreeNode parent, ModuleDefinition md)
        {
            TreeNode tn = parent.Nodes.Add(md.FullyQualifiedName, md.Name, 89, 89);
            foreach (TypeDefinition t in md.Types)
            {
                if (!Namespaces.ContainsKey(t.Namespace))
                {
                    TreeNode tnd = tn.Nodes.Add(t.Namespace, t.Namespace, 95, 95);
                    Namespaces.Add(t.Namespace, tnd);
                }
                LoadType(Namespaces[t.Namespace], t);
            }
            Namespaces = new Dictionary<string, TreeNode>();
        }

        private void LoadType(TreeNode parentNode, TypeDefinition type)
        {
            if (!type.IsEnum && !type.IsInterface && !type.IsValueType && type.IsClass)
            {
                TreeNode node;
                if (type.IsPublic)
                {
                    // It must be public
                    node = parentNode.Nodes.Add(type.FullName, type.Name, 3, 3);
                }
                else if (type.IsNestedPrivate)
                {
                    // It must be private.
                    node = parentNode.Nodes.Add(type.FullName, type.Name, 5, 5);
                }
                else if (type.IsNestedFamily)
                {
                    node = parentNode.Nodes.Add(type.FullName, type.Name, 4, 4);
                }
                else
                {
                    // It must be NestedProtected.
                    node = parentNode.Nodes.Add(type.FullName, type.Name, 6, 6);
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
                    LoadInterface(node, ntd);
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
            else if (type.IsValueType)
            {
                LoadStruct(parentNode, type);
            }
            else if (type.IsEnum)
            {
                LoadEnum(parentNode, type);
            }
            else if (type.IsInterface)
            {
                LoadInterface(parentNode, type.GetElementType());
            }
            else
            {
                throw new Exception();
            }

        }

        uint i = 0;
        private void LoadMethod(TreeNode parentNode, MethodDefinition method)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(method.Name);
            sb.Append("(");
            int cnt = 0;
            while (cnt < method.Parameters.Count)
            {
                if (cnt > 0)
                {
                    sb.Append(", ");
                }
                ParameterDefinition pd = method.Parameters[cnt];
                //foreach (CustomAttribute c in pd.CustomAttributes)
                //{
                //    if (c.AttributeType.Name == "OutAttribute")
                //    {
                //        sb.Append("out ");
                //    }
                //    else if (c.AttributeType.Name == "InAttribute")
                //    {
                //        sb.Append("in ");
                //    }
                //}
                sb.Append(pd.ParameterType.Name);
                sb.Append(" ");
                sb.Append(pd.Name);
                cnt++;
            }
            sb.Append(")");
            string dispkey = sb.ToString();
            string fullkey = (method.FullName + sb.ToString().Substring(method.Name.Length));

            #region Check if plug needed
            if ((method.Attributes & MethodAttributes.PInvokeImpl) != 0)
            {
                // pinvoke methods dont have an embedded implementation
                parentNode.Nodes.Add(fullkey, dispkey, ErrorIconIndex, ErrorIconIndex);
                i++;
                MethodsNeedingPlugging.Add(i.ToString().PadLeft(7, ' ') + ". " + method.FullName + " ~ PInvoke Impl");
                return;
            }
            else
            {
                MethodImplAttributes xImplFlags = method.ImplAttributes;
                // todo: prob even more
                if ((xImplFlags & MethodImplAttributes.Native) != 0)
                {
                    // native implementations cannot be compiled
                    parentNode.Nodes.Add(fullkey, dispkey, ErrorIconIndex, ErrorIconIndex);
                    i++;
                    MethodsNeedingPlugging.Add(i.ToString().PadLeft(7, ' ') + ". " + method.FullName + " ~ Method Implementation: Native");
                    return;
                }
                else if ((xImplFlags & MethodImplAttributes.InternalCall) != 0)
                {
                    parentNode.Nodes.Add(fullkey, dispkey, ErrorIconIndex, ErrorIconIndex);
                    i++;
                    MethodsNeedingPlugging.Add(i.ToString().PadLeft(7, ' ') + ". " + method.FullName + " ~ Method Implementation: Internal Call");
                    return;
                }
                else if ((xImplFlags & MethodImplAttributes.Unmanaged) != 0)
                {
                    parentNode.Nodes.Add(fullkey, dispkey, ErrorIconIndex, ErrorIconIndex);
                    i++;
                    MethodsNeedingPlugging.Add(i.ToString().PadLeft(7, ' ') + ". " + method.FullName + " ~ Method Implementation: Unmanaged");
                    return;
                }
            }
            #endregion

            // Check that the method isn't part of a property or event definition.
            if (!method.IsGetter && !method.IsSetter)
            {
                if (method.IsVirtual)
                {
                    // The method is an override (or at least overridable)
                    if (method.IsPublic)
                    {
                        parentNode.Nodes.Add(fullkey, dispkey, 83, 83);
                    }
                    else if (method.IsPrivate)
                    {
                        parentNode.Nodes.Add(fullkey, dispkey, 85, 85);
                    }
                    else if (method.IsInternalCall)
                    {
                        parentNode.Nodes.Add(fullkey, dispkey, 84, 84);
                    }
                    else
                    {
                        // It must be protected.
                        parentNode.Nodes.Add(fullkey, dispkey, 86, 86);
                    }
                }
                else
                {
                    if (method.IsPublic)
                    {
                        parentNode.Nodes.Add(fullkey, dispkey, 77, 77);
                    }
                    else if (method.IsPrivate)
                    {
                        parentNode.Nodes.Add(fullkey, dispkey, 79, 79);
                    }
                    else if (method.IsInternalCall)
                    {
                        // It's internal.
                        parentNode.Nodes.Add(fullkey, dispkey, 78, 78);
                    }
                    else
                    {
                        // It must be protected.
                        parentNode.Nodes.Add(fullkey, dispkey, 80, 80);
                    }
                }
            }
        }

        private void LoadInterface(TreeNode parentNode, TypeReference intface)
        {
            TypeDefinition type = intface.Resolve();
            TreeNode node;
            if (intface.Resolve().IsPublic)
            {
                node = parentNode.Nodes.Add(intface.FullName, intface.Name, 52, 52);
            }
            else if (intface.Resolve().IsNestedPrivate)
            {
                node = parentNode.Nodes.Add(intface.FullName, intface.Name, 54, 54);
            }
            else if (intface.Resolve().IsNestedFamily)
            {
                node = parentNode.Nodes.Add(intface.FullName, intface.Name, 53, 53);
            }
            else if (intface.Resolve().IsSealed)
            {
                node = parentNode.Nodes.Add(intface.FullName, intface.Name, 56, 56);
            }
            else
            {
                // It must be protected.
                node = parentNode.Nodes.Add(intface.FullName, intface.Name, 55, 55);
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
                LoadInterface(node, ntd);
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
            parentNode.Nodes.Add(evnt.FullName, evnt.Name, 34, 34);
        }

        private void LoadProperty(TreeNode parentNode, PropertyDefinition ptd)
        {
            parentNode.Nodes.Add(ptd.FullName, ptd.Name, 113, 113);
        }

        private void LoadEnum(TreeNode parentNode, TypeDefinition td)
        {
            TreeNode enode;
            if (td.IsPublic)
            {
                enode = parentNode.Nodes.Add(td.FullName, td.Name, 22, 22);
            }
            else if (td.IsNestedPrivate)
            {
                // It must be private.
                enode = parentNode.Nodes.Add(td.FullName, td.Name, 24, 24);
            }
            else if (td.IsNestedFamily)
            {
                enode = parentNode.Nodes.Add(td.FullName, td.Name, 23, 23);
            }
            else if (td.IsSealed)
            {
                enode = parentNode.Nodes.Add(td.FullName, td.Name, 25, 25);
            }
            else
            {
                // It must be protected.
                enode = parentNode.Nodes.Add(td.FullName, td.Name, 24, 24);
            }
            // Now we get to load it's values.
            foreach (FieldDefinition fd in td.Fields)
            {
                // We don't want to show "value__" because it's automatically added 
                // by the compiler.
                if (fd.Name != "value__")
                {
                    enode.Nodes.Add(fd.FullName, fd.Name, 9, 9);
                }
            }
        }

        private void LoadStruct(TreeNode parentNode, TypeDefinition td)
        {
            if (td.IsPublic)
            {
                parentNode.Nodes.Add(td.FullName, td.Name, 120, 120);
            }
            else if (td.IsNestedPrivate)
            {
                // It must be private.
                parentNode.Nodes.Add(td.FullName, td.Name, 122, 122);
            }
            else if (td.IsNestedFamily)
            {
                parentNode.Nodes.Add(td.FullName, td.Name, 121, 121);
            }
            else if (td.IsSealed)
            {
                parentNode.Nodes.Add(td.FullName, td.Name, 124, 124);
            }
            else
            {
                // It must be protected.
                parentNode.Nodes.Add(td.FullName, td.Name, 123, 123);
            }
        }

        private void LoadField(TreeNode parentNode, FieldDefinition fd)
        {
            if (fd.IsPublic)
            {
                parentNode.Nodes.Add(fd.FullName, fd.Name, 46, 46);
            }
            else if (fd.IsPrivate)
            {
                parentNode.Nodes.Add(fd.FullName, fd.Name, 48, 48);
            }
            else if (fd.IsFamily)
            {
                // This means internal
                parentNode.Nodes.Add(fd.FullName, fd.Name, 47, 47);
            }
            else
            {
                // It must be protected.
                parentNode.Nodes.Add(fd.FullName, fd.Name, 49, 49);
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
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StreamWriter s = new StreamWriter(Application.StartupPath + "\\analysis.txt");
            string mnpl = MethodsNeedingPlugging.Count.ToString();
            foreach (string str in MethodsNeedingPlugging)
            {
                s.WriteLine(str);
            }
            s.Flush();
            s.Close();
            s.Dispose();
        }
    }
}
