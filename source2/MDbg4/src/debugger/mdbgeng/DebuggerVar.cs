//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------

using System;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Reflection;
using System.IO;

using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using Microsoft.Samples.Debugging.CorMetadata;

using System.Runtime.Remoting;
namespace Microsoft.Samples.Debugging.MdbgEngine
{
    /// <summary>
    /// This represents a Collection of MDbg Debugger Variables.
    /// </summary>
    public sealed class MDbgDebuggerVarCollection : MarshalByRefObject,IEnumerable
    {
        internal MDbgDebuggerVarCollection(MDbgProcess process)
        {
            m_process = process;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            MDbgDebuggerVar[] ret = new MDbgDebuggerVar[m_debuggerVars.Count+g_specialVars.Length];
            m_debuggerVars.Values.CopyTo(ret,0);
            for(int i=0;i<g_specialVars.Length;++i)
            {
                MDbgDebuggerVar d = RetrieveSpecialVar(g_specialVars[i]);
                Debug.Assert(d!=null); // RetrieveSpecialVar needs to succedd for every var in g_specialVars
                ret[m_debuggerVars.Count+i] = d;
            }

            Array.Sort(ret);
            return ret.GetEnumerator();
        }

        /// <summary>
        /// Indexer allowing the Variable Collection to be accessed using array syntax.
        /// </summary>
        /// <param name="varName">The variable name that you want to access.</param>
        /// <returns>The variable.</returns>
        public MDbgDebuggerVar this[string varName]
        {
            get
            {
                Debug.Assert(varName!=null && varName.StartsWith("$"));
                if( varName==null || !varName.StartsWith("$") )
                    throw new ArgumentException("Wrong name of debugger variable.");

                // handle special read-only debugger vars
                MDbgDebuggerVar dv;
                dv = RetrieveSpecialVar(varName);
                if( dv!=null )
                    return dv;
                
                string istr =String.Intern(varName);
                if(m_debuggerVars.Contains(istr))
                    return (MDbgDebuggerVar) m_debuggerVars[istr];
                else
                {
                    dv = new MDbgDebuggerVar(varName);
                    m_debuggerVars.Add(istr,dv);
                    return dv;
                }
            }
        }

        /// <summary>
        /// Indicates if a variable name is in the collection.
        /// </summary>
        /// <param name="variableName">The variable name to look up.</param>
        /// <returns>Returns true if the variable is in the collection and false if it is not.</returns>
        public bool HaveVariable(string variableName)
        {
            return ( RetrieveSpecialVar(variableName)!=null
                     ||m_debuggerVars.Contains(String.Intern(variableName)) );
        }

        private static string[] g_specialVars = new string[]{"$ex","$thread"};
        
        private MDbgDebuggerVar RetrieveSpecialVar(string variableName)
        {
            switch(variableName)
            {
            case "$ex":
            case "$exception":
                return new MDbgDebuggerVar("$ex",m_process.Threads.Active.CurrentException.CorValue);
            case "$thread":
                return new MDbgDebuggerVar("$thread", m_process.Threads.Active.CorThread.ThreadVariable);
            default:
                return null;                                // we don't know this special variable
            }
        }

        /// <summary>
        /// Removes a variable from the collection.
        /// </summary>
        /// <param name="variableName">The name of the variable to remove.</param>
        public void DeleteVariable(string variableName)
        {
            Debug.Assert(variableName!=null);
            m_debuggerVars.Remove(String.Intern(variableName));
        }

        /// <summary>
        /// Sets the EvalResult to the specified CorValue
        /// </summary>
        /// <param name="evalValue">What value to set the Result to.</param>
        /// <returns>Returns true on success else false.</returns>
        public bool SetEvalResult(CorValue evalValue)
        {
            Debug.Assert(evalValue!=null);
            if( evalValue==null )
                throw new ArgumentException();
            this["$result"].Value = evalValue;
            return true;
        }

        private Hashtable m_debuggerVars = new Hashtable();
        private MDbgProcess m_process;
    }

    /// <summary>
    /// MDbg Debugger Variable class.
    /// </summary>
    public sealed class MDbgDebuggerVar : MarshalByRefObject, IComparable
    {
        internal MDbgDebuggerVar(string variableName)
        {
            Debug.Assert(variableName!=null);
            m_name = variableName;
        }

        internal MDbgDebuggerVar(string variableName, CorValue corValue)
        {
            // corValue can be null meaning that the variable is not available
            Debug.Assert(variableName!=null);
            m_name = variableName;
            m_corValue = corValue;
        }

        /// <summary>
        /// Gets or sets the CorValue.  Can be either CorReferenceValue or CorHeapValue
        /// </summary>
        /// <value></value>
        public CorValue Value
        {
            get
            {
                return m_corValue;
            }
            set
            {
                Debug.Assert(value!=null);
                if(value!=null)
                {
                    CorHeapValue hv = value.CastToHeapValue();
                    if( hv==null )
                    {
                        // Check if it's a reference value.
                        CorReferenceValue cv = value.CastToReferenceValue();
                        if ((cv != null) && !cv.IsNull)
                        {
                            CorValue v2 = null;
                            try
                            {
                                v2 = cv.Dereference();

                                Debug.Assert(v2 != null, "Should always be able to dereference a reference value.");
                                hv = v2.CastToHeapValue();                                
                            }
                            catch(System.Runtime.InteropServices.COMException)
                            {                                
                                // If dereference failed because it was a bad value, then we won't try to make a strong
                                // reference to it.
                            }
                        }
                    }
                    if( hv!=null )
                    {
                        m_corValue = hv.CreateHandle(CorDebugHandleType.HANDLE_STRONG);
                    }
                    else
                    {
                        // this is not a GC collectable object, we might store the reference directly
                        m_corValue = value;
                    }
                }
                else
                    m_corValue = null;
            }

        }

        /// <summary>
        /// Get the CorValue for Variable.
        /// </summary>
        /// <value>The CorValue.</value>
        public CorValue CorValue
        {
            get
            {
                return m_corValue;
            }
        }

        /// <summary>
        /// Get the Name for the Variable.
        /// </summary>
        /// <value>The Name.</value>
        public string Name 
        {
            get
            {
                return m_name;
            }
        }


        int IComparable.CompareTo(object obj)
        {
            MDbgDebuggerVar other= obj as MDbgDebuggerVar;
            return String.Compare( this.Name, other.Name );
        }
            
        private CorValue m_corValue=null;
        private string m_name;
    }
}
