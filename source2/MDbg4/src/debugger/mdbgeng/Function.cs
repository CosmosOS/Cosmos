//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Text;


using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using Microsoft.Samples.Debugging.CorMetadata;


namespace Microsoft.Samples.Debugging.MdbgEngine
{
    internal class MDbgFunctionMgr : IDisposable
    {
        public MDbgFunction Get(CorFunction managedFunction)
        {
            int funcVersion;
            funcVersion = managedFunction.Version;

            // now get version from our cache.
            MDbgFunction mdbgFunction = RetrieveFromCache(managedFunction.Token, funcVersion);
            if (mdbgFunction == null)
            {
                mdbgFunction = new MDbgFunction(m_module, managedFunction);
                AddToCache(managedFunction.Token, funcVersion, mdbgFunction);
            }
            return mdbgFunction;
        }

        // Dispose all functions in our collection and then empty the collection.
        public void Dispose()
        {
            foreach (object o in m_functions.Values)
            {
                // items may be either a function, or an array of functions.
                MDbgFunction f1 = o as MDbgFunction;
                if (f1 != null)
                {
                    f1.Dispose();
                }
                else
                {
                    MDbgFunction[] a = (MDbgFunction[])o;
                    foreach (MDbgFunction f in a)
                    {
                        f.Dispose();
                    }
                }
            }

            Clear();
        }

        // Remove all functions from the collection.
        public void Clear()
        {
            m_functions.Clear();
        }

        internal MDbgFunctionMgr(MDbgModule module)
        {
            m_module = module;
        }


        private MDbgFunction RetrieveFromCache(int functionToken, int version)
        {
            Debug.Assert(version >= 0); // we don't accept default versions

            if (!m_functions.Contains(functionToken))
                return null;

            Object data = m_functions[functionToken];
            if (data is MDbgFunction)
            {
                // only 1 instance of function is saved
                if ((data as MDbgFunction).CorFunction.Version == version)
                    return (MDbgFunction)data;
                else
                    return null;
            }
            else
            {
                // data is array
                foreach (MDbgFunction f in (data as Array))
                    if (f.CorFunction.Version == version)
                        return f;
                return null;
            }
        }

        private void AddToCache(int functionToken, int version, MDbgFunction mdbgFunction)
        {
            Debug.Assert(mdbgFunction != null);
            Debug.Assert(functionToken == mdbgFunction.CorFunction.Token);
            Debug.Assert(version == mdbgFunction.CorFunction.Version);

            if (m_functions.Contains(functionToken))
            {
                // we already have some versions need to convert to array
                ArrayList al = new ArrayList();

                Object data = m_functions[functionToken];
                if (data is MDbgFunction)
                {
                    al.Add(data);
                }
                else
                {
                    // data is array
                    foreach (MDbgFunction f in (data as Array))
                        al.Add(f);
                }
                // now we add at the end newly added function
                al.Add(mdbgFunction);

                m_functions.Remove(functionToken);
                m_functions.Add(functionToken, al.ToArray(typeof(MDbgFunction)));
            }
            else
            {
                m_functions.Add(functionToken, mdbgFunction);
            }
        }

        private Hashtable m_functions = new Hashtable();
        private MDbgModule m_module;
    }

    /// <summary>
    /// The MDbgFunction class.
    /// This represents cached information about a function.  Since this is just a cache
    /// (and can be updated if symbols change), you should not hold onto instances
    /// of this across a Continue.  Instead use MDbgFunctionMgr.Get to get or create
    /// an up-to-date MDbgFunction object.
    /// </summary>
    public sealed class MDbgFunction : MarshalByRefObject, IDisposable
    {
        /// <summary>
        /// Gets the step ranges from an IP
        /// </summary>
        /// <param name="ip">The IP to get Step Ranges From.</param>
        /// <returns>The Step Ranges.</returns>
        [CLSCompliant(false)]
        public COR_DEBUG_STEP_RANGE[] GetStepRangesFromIP(int ip)
        {
            EnsureIsUpToDate();

            // we cannot use GetRange here, since this function doesn't work
            // in ENC mode.
            //
            // we will calculate step ranges manually from sequence points.
            //
            COR_DEBUG_STEP_RANGE[] ret = null;
            for (int j = 0; j < m_SPcount; j++)
                if (m_SPoffsets[j] > ip)
                {
                    ret = new COR_DEBUG_STEP_RANGE[1];
                    ret[0].endOffset = (uint)m_SPoffsets[j];
                    ret[0].startOffset = (uint)m_SPoffsets[j - 1];
                    break;
                }
            // let's handle correctly last step range from last sequence point till
            // end of the method.
            if (ret == null && m_SPcount > 0)
            {
                ret = new COR_DEBUG_STEP_RANGE[1];
                ret[0].startOffset = (uint)m_SPoffsets[m_SPcount - 1];
                ret[0].endOffset = (uint)CorFunction.ILCode.Size;
            }
            return ret;
        }

        /// <summary>
        /// Releases all resources used by the MDbgFunction.
        /// </summary>
        public void Dispose()
        {
            // Release unmanaged resources, especially symbol readers
            m_function = null;

            m_symMethod = null;
            m_SPoffsets = null;
            m_SPdocuments = null;
            m_SPstartLines = null;
            m_SPendLines = null;
            m_SPstartColumns = null;
            m_SPendColumns = null;
            m_SPcount = 0;
        }

        /// <summary>
        /// Gets the source position from a given Instruction Pointer
        /// </summary>
        /// <param name="ip">The Instruction Pointer.</param>
        /// <returns>The Source Position.</returns>
        public MDbgSourcePosition GetSourcePositionFromIP(int ip)
        {
            EnsureIsUpToDate();
            if (!m_haveSymbols)
            {
                return null;
            }

            if ((m_SPcount > 0) && (m_SPoffsets[0] <= ip))
            {
                int i;
                for (i = 0; i < m_SPcount; ++i)
                {
                    if (m_SPoffsets[i] >= ip)
                    {
                        break;
                    }
                }

                if ((i == m_SPcount) || (m_SPoffsets[i] != ip))
                {
                    --i;
                }

                MDbgSourcePosition sp = null;

                if (m_SPstartLines[i] == SpecialSequencePoint)
                {
                    int j = i;
                    // let's try to find a sequence point that is not special somewhere earlier in the code
                    // stream.
                    while (j > 0)
                    {
                        --j;
                        if (m_SPstartLines[j] != SpecialSequencePoint)
                        {
                            sp = new MDbgSourcePosition(true,
                                                        m_SPdocuments[j].URL,
                                                        m_SPstartLines[j],
                                                        m_SPendLines[j],
                                                        m_SPstartColumns[j],
                                                        m_SPendColumns[j]);
                            break;
                        }
                    }

                    if (sp == null)
                    {
                        // we didn't find any non-special seqeunce point before current one, let's try to search
                        // after.
                        j = i;
                        while (++j < m_SPcount)
                        {
                            if (m_SPstartLines[j] != SpecialSequencePoint)
                            {
                                sp = new MDbgSourcePosition(true,
                                                            m_SPdocuments[j].URL,
                                                            m_SPstartLines[j],
                                                            m_SPendLines[j],
                                                            m_SPstartColumns[j],
                                                            m_SPendColumns[j]);
                                break;
                            }
                        }
                    }

                    // Even if sp is null at this point, it's a valid scenario to have only special sequence 
                    // point in a function.  For example, we can have a compiler-generated default ctor which
                    // doesn't have any source.
                    if (sp == null)
                    {
                        return null;
                    }
                }
                else
                {
                    // non special sequence point.
                    sp = new MDbgSourcePosition(false,
                                                m_SPdocuments[i].URL,
                                                m_SPstartLines[i],
                                                m_SPendLines[i],
                                                m_SPstartColumns[i],
                                                m_SPendColumns[i]);
                }


                if (CorFunction.Version != 1) // function has been edited
                {
                    sp.m_fixedFile = Module.GetEditsSourceFile(CorFunction.Version - 1);
                }

                return sp;
            }

            return null;
        }

        /// <summary>
        /// Gets the MDbgSourcePosition from a given CorFrame.
        /// </summary>
        /// <param name="frame">The CorFrame.</param>
        /// <returns>The MDbgSourcePosition of the given frame.</returns>
        public MDbgSourcePosition GetSourcePositionFromFrame(CorFrame frame)
        {
            // EnsureIsUpToDate is called from GetSourcePositionFromIP

            // we only support this, when the frame is our function
            Debug.Assert(frame.FunctionToken == m_function.Token);

            uint ip;
            CorDebugMappingResult mappingResult;
            frame.GetIP(out ip, out mappingResult);

            // MAPPING_APPROXIMATE, MAPPING_EXACT, MAPPING_PROLOG, or MAPPING_EPILOG are all ok and we should show sources.
            // But these two indicate that something went wrong and nothing is available.
            if (mappingResult == CorDebugMappingResult.MAPPING_NO_INFO ||
               mappingResult == CorDebugMappingResult.MAPPING_UNMAPPED_ADDRESS)
                return null;

            return GetSourcePositionFromIP((int)ip);
        }

        // properties

        /// <summary>
        /// Gets the CorFunction encapsulated in the MDbgFunction.
        /// </summary>
        /// <value>The CorFunction.</value>
        public CorFunction CorFunction
        {
            get
            {
                return m_function;
            }
        }

        /// <summary>
        /// Gets the MethodInfo for the function.
        /// </summary>
        /// <value>The MethodInfo.</value>
        public MethodInfo MethodInfo
        {
            get
            {
                return m_module.Importer.GetMethodInfo(m_function.Token);
            }
        }

        /// <summary>
        /// Gets the Full Name for the Function.
        /// </summary>
        /// <value>The Full Name.</value>
        public string FullName
        {
            get
            {
                MethodInfo mi = MethodInfo;
                Type t = mi.DeclaringType;

                StringBuilder sb = new StringBuilder();
                sb.Append((t == null ? "" : t.FullName)).
                    Append(".").Append(mi.Name);

                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the Module that contains this function.
        /// </summary>
        /// <value>The MDbgModule.</value>
        public MDbgModule Module
        {
            get
            {
                return m_module;
            }
        }

        /// <summary>
        /// Gets the SymMethod.
        /// </summary>
        /// <value>The SymMethod.</value>
        public ISymbolMethod SymMethod
        {
            get
            {
                EnsureIsUpToDate();
                return m_symMethod;
            }
        }

        /// <summary>
        /// Function gets an IL offset that corresponds to the line number for the function.
        /// Function is needed for implementing SetIP command to implement setting an IP at some line number.
        /// This function only works if the function is defined in one file.
        /// </summary>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="ilOffset">The ILOffset.</param>
        /// <returns>true on success, else false</returns>
        public bool GetIPFromLine(int lineNumber, out int ilOffset)
        {
            ilOffset = -1;

            // first make sure that the function is defined in one document.
            if (m_SPcount == 0)
                return false;
            string d = m_SPdocuments[0].URL;
            for (int i = 1; i < m_SPcount; i++)
                if (d != m_SPdocuments[i].URL)
                    return false;                           // fail when multiple documents found
            ilOffset = GetIPFromPosition(m_SPdocuments[0], lineNumber);
            if (ilOffset == -1)
                return false;
            return true;
        }

        internal MDbgFunction(MDbgModule managedModule, CorFunction managedFunction)
        {
            Debug.Assert(managedModule != null);
            Debug.Assert(managedFunction != null);
            Debug.Assert(managedFunction.Version >= 0 && managedFunction.Version - 1 <= managedModule.EditsCounter); // version numbers starts with 1

            m_module = managedModule;
            m_function = managedFunction;
            EnsureIsUpToDate();
        }

        internal int GetIPFromPosition(ISymbolDocument document, int lineNumber)
        {
            EnsureIsUpToDate();

            for (int i = 0; i < m_SPcount; i++)
            {
                if (document.URL.Equals(m_SPdocuments[i].URL) &&
                    lineNumber == m_SPstartLines[i])
                    return m_SPoffsets[i];
            }
            return -1;

        }


        //////////////////////////////////////////////////////////////////////////////////
        //
        //  Support for printing local printing variables
        //
        //////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets an Array of MDbgValues for the Active Local Vars in the given frame.
        /// </summary>
        /// <param name="managedFrame">The Frame to look in.</param>
        /// <returns>The MDbgValue[] Active Local Valiables.</returns>
        public MDbgValue[] GetActiveLocalVars(MDbgFrame managedFrame)
        {
            Debug.Assert(managedFrame != null);
            if (managedFrame == null)
                throw new ArgumentException();

            CorFrame frame = managedFrame.CorFrame;

            // we only support this, when the frame is our function
            Debug.Assert(frame.FunctionToken == m_function.Token);
            if (!(frame.FunctionToken == m_function.Token))
                throw new ArgumentException();

            EnsureIsUpToDate();

            if (!m_haveSymbols)
            {
                // if we don't have symbols -- we'll print local variables as (loca1_0,local_1,local_2,...)
                // to give them names consistent with ILasm.
                int c = frame.GetLocalVariablesCount();
                if (c < 0)
                    c = 0;                                    // in case we cannot get locals,
                // we'll hide them.
                MDbgValue[] locals = new MDbgValue[c];
                for (int i = 0; i < c; ++i)
                {
                    CorValue arg = null;
                    try
                    {
                        arg = frame.GetLocalVariable(i);
                    }
                    catch (System.Runtime.InteropServices.COMException e)
                    {
                        if (e.ErrorCode != (int)Microsoft.Samples.Debugging.CorDebug.HResult.CORDBG_E_IL_VAR_NOT_AVAILABLE)
                            throw;
                    }
                    locals[i] = new MDbgValue(m_module.Process, "local_" + (i), arg);
                }
                return locals;
            }

            uint ip;
            CorDebugMappingResult mappingResult;
            frame.GetIP(out ip, out mappingResult);

            ArrayList al = new ArrayList();
            ISymbolScope scope = SymMethod.RootScope;
            AddLocalVariablesToList(frame, (int)ip, al, scope);

            return (MDbgValue[])al.ToArray(typeof(MDbgValue));
        }

        /// <summary>
        /// Gets an array of MDbgValues that are the Arguments to the Function in the given Frame.
        /// </summary>
        /// <param name="managedFrame">The Frame to use.</param>
        /// <returns>The MDbgValue[] Arguments.</returns>
        public MDbgValue[] GetArguments(MDbgFrame managedFrame)
        {
            Debug.Assert(managedFrame != null);
            if (managedFrame == null)
                throw new ArgumentException();

            CorFrame f = managedFrame.CorFrame;

            // we only support this, when the frame is our function
            Debug.Assert(f.FunctionToken == m_function.Token);
            if (!(f.FunctionToken == m_function.Token))
                throw new ArgumentException();

            EnsureIsUpToDate();

            ArrayList al = new ArrayList();
            int c = f.GetArgumentCount();
            if (c == -1)
                throw new MDbgException("Could not get metainformation. (Jit tracking information not turned on)");

            int i;
            for (i = 0; i < c; i++)
            {
                CorValue arg = null;
                try
                {
                    arg = f.GetArgument(i);
                }
                catch (System.Runtime.InteropServices.COMException e)
                {
                    if (e.ErrorCode != (int)Microsoft.Samples.Debugging.CorDebug.HResult.CORDBG_E_IL_VAR_NOT_AVAILABLE)
                        throw;
                }
                al.Add(new MDbgValue(m_module.Process, arg));
            }
            MDbgValue[] argArray = (MDbgValue[])al.ToArray(typeof(MDbgValue));

            MethodInfo mi = managedFrame.Function.MethodInfo;
            foreach (ParameterInfo pi in mi.GetParameters())
            {
                int pos = pi.Position;
                // ParameterInfo at Position 0 refers to the return type (eg. when it has an attribute applied)
                if (pos == 0)
                    continue;
                if (mi.IsStatic)
                    pos--;
                Debug.Assert(pos < c);
                argArray[pos].InternalSetName(pi.Name);
            }

            i = 0;
            foreach (MDbgValue v in argArray)
                if (v.Name == null)
                {
                    if (i == 0 && !mi.IsStatic)
                        v.InternalSetName("this");
                    else
                        v.InternalSetName("unnamed_param_" + i);
                    i++;
                }
            return argArray;
        }

        private void AddLocalVariablesToList(CorFrame frame, int ip, ArrayList listToAdd, ISymbolScope scope)
        {
            Debug.Assert(frame.FunctionToken == m_function.Token);

            foreach (ISymbolVariable isv in scope.GetLocals())
            {
                Debug.Assert(isv.AddressKind == SymAddressKind.ILOffset);
                CorValue v = null;
                try
                {
                    v = frame.GetLocalVariable(isv.AddressField1);
                }
                catch (System.Runtime.InteropServices.COMException e)
                {
                    if (e.ErrorCode != (int)Microsoft.Samples.Debugging.CorDebug.HResult.CORDBG_E_IL_VAR_NOT_AVAILABLE)
                        throw;
                }

                listToAdd.Add(new MDbgValue(m_module.Process, isv.Name, v));
            }

            foreach (ISymbolScope s in scope.GetChildren())
            {
                if (s.StartOffset <= ip && s.EndOffset >= ip)
                    AddLocalVariablesToList(frame, ip, listToAdd, s);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////
        //
        //  Initialization & locals
        //
        //////////////////////////////////////////////////////////////////////////////////

        private void EnsureIsUpToDate()
        {
            Debug.Assert(m_module != null);
            Debug.Assert(m_module.EditsCounter >= CorFunction.Version - 1); // version cannot be greater then # of edits; versions are 1 based

            if (m_isInitialized)
                return; // no need to do any refresh

            // perform refresh
            m_isInitialized = true;
            m_haveSymbols = m_module.SymReader != null;
            if (m_haveSymbols)
            {
                ISymbolMethod sm = null;
                sm = m_module.SymReader.GetMethod(new SymbolToken((int)m_function.Token), CorFunction.Version);
                if (sm == null)
                {
                    m_haveSymbols = false;
                    return;
                }

                m_symMethod = sm;
                m_SPcount = m_symMethod.SequencePointCount;
                m_SPoffsets = new int[m_SPcount];
                m_SPdocuments = new ISymbolDocument[m_SPcount];
                m_SPstartLines = new int[m_SPcount];
                m_SPendLines = new int[m_SPcount];
                m_SPstartColumns = new int[m_SPcount];
                m_SPendColumns = new int[m_SPcount];

                m_symMethod.GetSequencePoints(m_SPoffsets, m_SPdocuments, m_SPstartLines, m_SPstartColumns, m_SPendLines, m_SPendColumns);
            }
        }

        private CorFunction m_function;

        private bool m_isInitialized = false;
        private bool m_haveSymbols = false;

        private MDbgModule m_module;
        private ISymbolMethod m_symMethod;
        private int[] m_SPoffsets;
        private ISymbolDocument[] m_SPdocuments;
        private int[] m_SPstartLines, m_SPendLines, m_SPstartColumns, m_SPendColumns;
        private int m_SPcount;

        /// <summary>
        /// Constant to indicate if a Sequence Point is Special.
        /// </summary>
        public const int SpecialSequencePoint = 0xfeefee;
    }


    /// <summary>
    /// MDbgSourcePosition class.
    /// </summary>
    public sealed class MDbgSourcePosition : MarshalByRefObject
    {
        /// <summary>
        /// Gets if this source position is a special position.
        /// </summary>
        /// <value>true if special, else false.</value>
        public bool IsSpecial
        {
            get
            {
                return m_isSpecial;
            }
        }

        /// <summary>
        /// Same as StartLine.
        /// </summary>
        /// <value>StartLine.</value>
        public int Line
        {
            get
            {
                return m_startLine;
            }
        }

        /// <summary>
        /// Gets the start line of the location in the source file.
        /// </summary>
        /// <value>The start line.</value>
        public int StartLine
        {
            get
            {
                return m_startLine;
            }
        }

        /// <summary>
        /// Gets the start column of the location in the source file.
        /// </summary>
        /// <value>The start column.</value>
        public int StartColumn
        {
            get
            {
                return m_startColumn;
            }
        }

        /// <summary>
        /// Gets the final line of the location in the source file.
        /// </summary>
        /// <value>The final line.</value>
        public int EndLine
        {
            get
            {
                return m_endLine;
            }
        }

        /// <summary>
        /// Gets the end column of the location in the source file.
        /// </summary>
        /// <value>The End Column.</value>
        public int EndColumn
        {
            get
            {
                return m_endColumn;
            }
        }

        /// <summary>
        /// Gets the Path for the source file.
        /// </summary>
        /// <value>The Path.</value>
        public string Path
        {
            get
            {
                if (m_fixedFile != null)
                    //having new version of function in different file, therefore
                    //we have to pass in the file name manually.
                    return m_fixedFile;

                return m_path;
            }
        }


        /// <summary>
        /// Contructor of MDbgSourcePosition type.
        /// </summary>
        /// <param name="isSpecial">Indicates if this source position is a special position.</param>
        /// <param name="path">Path for the source file.</param>
        /// <param name="startLine">Start line of the location in the source file.</param>  
        /// <param name="endLine">Final line of the location in the source file.</param>                
        /// <param name="startColumn">Start column of the location in the source file.</param>          
        /// <param name="endColumn">Endcolumn of the location in the source file.</param>                  
        public MDbgSourcePosition(bool isSpecial,
                                    string path, int startLine, int endLine, int startColumn, int endColumn)
        {
            // special sequence points are handled elsewhere.
            Debug.Assert(m_startLine != MDbgFunction.SpecialSequencePoint);

            m_path = path;
            m_startLine = startLine;
            m_endLine = endLine;
            m_startColumn = startColumn;
            m_endColumn = endColumn;
            m_isSpecial = isSpecial;
        }

        internal string m_fixedFile = null;
        internal string m_path;
        bool m_isSpecial;
        private int m_startLine, m_endLine, m_startColumn, m_endColumn;

        /// <summary>
        /// Get a string representation of the Source position.
        /// </summary>
        /// <returns>a simple string representation</returns>
        public override string ToString()
        {
            if (m_path == null)
            {
                return String.Empty;
            }
            return m_path + ":" + m_startLine;
        }
    }
}
