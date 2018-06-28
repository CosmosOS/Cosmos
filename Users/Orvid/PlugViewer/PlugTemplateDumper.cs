using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace PlugViewer
{
    public static class PlugTemplateDumper
    {
        public static void Dump(Assembly asm)
        {
            const MethodImplAttributes BadFlags = MethodImplAttributes.InternalCall | MethodImplAttributes.Native | MethodImplAttributes.Unmanaged;
            bool TypeNeedsPlugs = false;
            bool firstParam = true;
            string str = "";
            string genParams = "";
            string curDir = Application.StartupPath + "\\PlugTemplates\\" + asm.GetName().Name + "\\";
            if (!Directory.Exists(Application.StartupPath + "\\PlugTemplates\\"))
            {
                Directory.CreateDirectory(Application.StartupPath + "\\PlugTemplates\\");
            }
            StreamWriter strm = null; // Only set to null to appease the compiler.
            foreach (Type t in asm.GetTypes())
            {
                if (t.BaseType != null && t.BaseType.FullName != "System.MulticastDelegate")
                {
                    foreach (MethodInfo m in t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly))
                    {
                        if ((m.GetMethodImplementationFlags() & BadFlags) != 0 || (m.Attributes & MethodAttributes.PinvokeImpl) != 0)
                        {
                            if (!TypeNeedsPlugs)
                            {
                                if (!Directory.Exists(curDir))
                                {
                                    Directory.CreateDirectory(curDir);
                                }
                                if (t.Namespace != null)
                                {
                                    if (!Directory.Exists(curDir + t.Namespace.Replace('.', '\\') + "\\"))
                                    {
                                        Directory.CreateDirectory(curDir + t.Namespace.Replace('.', '\\') + "\\");
                                    }
                                }
                                genParams = "";
                                str = "";
                                if (t.IsGenericTypeDefinition)
                                {
                                    genParams = "<";
                                    str = "<"; // This will be used for the typeof target.
                                    firstParam = true;
                                    foreach (Type T in t.GetGenericArguments())
                                    {
                                        if (!firstParam)
                                        {
                                            genParams += ", ";
                                            str += ", ";
                                        }
                                        genParams += T.Name;
                                        str += "object";
                                    }
                                    genParams += ">";
                                    str += ">";
                                }
                                if (t.Namespace != null)
                                {
                                    strm = new StreamWriter(curDir + t.Namespace.Replace('.', '\\') + "\\" + t.Name + "Impl.cs");
                                }
                                else
                                {
                                    strm = new StreamWriter(curDir + "\\" + t.Name + "Impl.cs");
                                }
                                strm.WriteLine("namespace Cosmos.Plugs");
                                strm.WriteLine("{");
                                strm.WriteLine("\t[IL2CPU.API.Plug(Target = typeof(" + t.Namespace + "." + t.Name + str + "), TargetFramework = IL2CPU.API.FrameworkVersion.v4_0)]");
                                strm.WriteLine("\tpublic static class " + t.FullName.Replace('.', '_') + "Impl" + genParams);
                                strm.WriteLine("\t{");
                                TypeNeedsPlugs = true;
                            }
                            strm.WriteLine();
                            str = "";
                            firstParam = true;
                            if (!m.IsStatic)
                            {
                                if (t.IsValueType && !t.IsEnum) // aka, a struct, instance members are references.
                                {
                                    str += "ref " + t.FullName.Replace('&', '*') + " aThis";
                                }
                                else // Something else, instance members aren't references.
                                {
                                    str += t.FullName.Replace('&', '*') + " aThis";
                                }
                                firstParam = false;
                            }
                            if (m.ContainsGenericParameters)
                            {
                                foreach (ParameterInfo p in m.GetParameters())
                                {
                                    if (!firstParam)
                                    {
                                        str += ", ";
                                    }
                                    if (p.ParameterType.IsGenericType || p.ParameterType.IsGenericParameter || p.ParameterType.IsGenericTypeDefinition)
                                    {
                                        str += p.ParameterType.Name + " " + p.Name;
                                    }
                                    else
                                    {
                                        if (p.ParameterType.FullName != null)
                                        {
                                            str += p.ParameterType.FullName.Replace('&', '*') + " " + p.Name;
                                        }
                                        else
                                        {
                                            str += p.ParameterType.Name.Replace('&', '*') + " " + p.Name;
                                        }
                                    }
                                    firstParam = false;
                                }
                            }
                            else
                            {
                                foreach (ParameterInfo p in m.GetParameters())
                                {
                                    if (!firstParam)
                                    {
                                        str += ", ";
                                    }
                                    str += p.ParameterType.FullName.Replace('&', '*') + " " + p.Name;
                                    firstParam = false;
                                }
                            }
                            if (m.ReturnType.IsGenericParameter || m.ReturnType.IsGenericType)
                            {
                                strm.WriteLine("\t\tpublic static " + m.ReturnType.Name + " " + m.Name + "(" + str + ")");
                            }
                            else
                            {
                                strm.WriteLine("\t\tpublic static " + m.ReturnType.FullName.Replace('&', '*') + " " + m.Name + "(" + str + ")");
                            }
                            strm.WriteLine("\t\t{");
                            strm.WriteLine("\t\t\tthrow new System.NotImplementedException(\"Method '" + t.FullName + "." + m.Name + "' has not been implemented!\");");
                            strm.WriteLine("\t\t}");
                        }
                    }
                    if (TypeNeedsPlugs)
                    {
                        strm.WriteLine("\t}");
                        strm.WriteLine("}");
                        strm.Flush();
                        strm.Close();
                    }
                    TypeNeedsPlugs = false;
                }
            }
        }

    }
}
