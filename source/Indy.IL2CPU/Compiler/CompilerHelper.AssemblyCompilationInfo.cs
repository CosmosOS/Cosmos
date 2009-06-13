using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Indy.IL2CPU.IL;

namespace Indy.IL2CPU.Compiler
{
    partial class CompilerHelper
    {
        public class AssemblyCompilationInfo: IComparable<AssemblyCompilationInfo>
        {
            private Assembly mAssembly;

            public AssemblyCompilationInfo()
            {
                Methods = new  Dictionary<string, MethodBase>();
                StaticFields=new Dictionary<string, FieldInfo>();
            }

            public Assembly Assembly
            {
                get { return mAssembly; }
                set { 
                    if(value == null)
                    {
                        throw new ArgumentException("Assembly cannot be null!");
                    }
                    mAssembly = value;
                    mAssemblyName = value.FullName;
                }
            }

            private string mAssemblyName;

            public List<string> Externals = new List<string>();
            public List<string> IDLabels = new List<string>();

            /// <summary>
            /// Adds the specified method to the Methods dictionary, skipping that action if it already is in.
            /// </summary>
            /// <param name="aMethod"></param>
            public void AddMethod(MethodBase aMethod)
            {
                var xName = aMethod.GetFullName();
                if(!Methods.ContainsKey(xName))
                {
                    Methods.Add(xName, aMethod);
                }
            }

            public Dictionary<string, MethodBase> Methods
            {
                get; private set;
            }

            /// <summary>
            /// Adds the specified method to the Methods dictionary, skipping that action if it already is in.
            /// </summary>
            /// <param name="aMethod"></param>
            public void AddStaticField(FieldInfo aField)
            {
                if(!aField.IsStatic)
                {
                    throw new Exception("Can only add static fields!");
                }
                var xName = aField.GetFullName();
                if (!StaticFields.ContainsKey(xName))
                {
                    StaticFields.Add(xName, aField);
                }
            }

            public Dictionary<string, FieldInfo> StaticFields
            {
                get;
                private set;
            }

            #region Implementation of IComparable<AssemblyCompilationInfo>

            public int CompareTo(AssemblyCompilationInfo other)
            {
                return String.Compare(mAssemblyName, other.mAssemblyName);
            }

            #endregion
        }
    }
}