//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Diagnostics;

using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace Microsoft.Samples.Debugging.CorDebug
{
    public struct CorActiveFunction
    {
        public int ILoffset
        {
            get
            {
                return m_ilOffset;
            }
        }
        private int m_ilOffset;

        public CorFunction Function
        {
            get
            {
                return m_function;
            }
        }
        private CorFunction m_function;

        public CorModule Module
        {
            get
            {
                return m_module;
            }
        }
        private CorModule m_module;

        internal CorActiveFunction(int ilOffset, CorFunction managedFunction, CorModule managedModule)
        {
            m_ilOffset = ilOffset;
            m_function = managedFunction;
            m_module = managedModule;
        }
    }

    public enum CorStackWalkType
    {
        PureV3StackWalk,        // true representation of the V3 ICorDebugStackWalk API
        ExtendedV3StackWalk     // V3 ICorDebugStackWalk API with internal frames interleaved
    }

    /// <summary>
    /// An object which a thread is blocked on
    /// </summary>
    public struct CorBlockingObject
    {
        public CorValue BlockingObject;
        public CorDebugBlockingReason BlockingReason;
        public TimeSpan Timeout;
    }

    /** A thread in the debugged process. */
    public sealed class CorThread : WrapperBase
    {
        internal CorThread(ICorDebugThread thread)
            : base(thread)
        {
            m_th = thread;
        }

        internal ICorDebugThread GetInterface()
        {
            return m_th;
        }

        [CLSCompliant(false)]
        public ICorDebugThread Raw
        {
            get 
            { 
                return m_th;
            }
        }

        /** The process that this thread is in. */
        public CorProcess Process
        {
            get
            {
                ICorDebugProcess p = null;
                m_th.GetProcess(out p);
                return CorProcess.GetCorProcess(p);
            }
        }

        /** the OS id of the thread. */
        public int Id
        {
            get
            {
                uint id = 0;
                m_th.GetID(out id);
                return (int)id;
            }
        }

        /** The handle of the active part of the thread. */
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IntPtr Handle
        {
            get
            {
                IntPtr h = IntPtr.Zero;
                m_th.GetHandle(out h);
                return h;
            }
        }

        /** The AppDomain that owns the thread. */
        public CorAppDomain AppDomain
        {
            get
            {
                ICorDebugAppDomain ad = null;
                m_th.GetAppDomain(out ad);
                return new CorAppDomain(ad);
            }
        }

        /** Set the current debug state of the thread. */
        [CLSCompliant(false)]
        public CorDebugThreadState DebugState
        {
            get
            {
                CorDebugThreadState s = CorDebugThreadState.THREAD_RUN;
                m_th.GetDebugState(out s);
                return s;
            }
            set
            {
                m_th.SetDebugState(value);
            }
        }

        /** the user state. */
        [CLSCompliant(false)]
        public CorDebugUserState UserState
        {
            get
            {
                CorDebugUserState s = CorDebugUserState.USER_STOP_REQUESTED;
                m_th.GetUserState(out s);
                return s;
            }
        }

        /** the exception object which is currently being thrown by the thread. */
        public CorValue CurrentException
        {
            get
            {
                ICorDebugValue v = null;
                m_th.GetCurrentException(out v);
                return (v == null) ? null : new CorValue(v);
            }
        }

        /** gets the current custom notification object on the thread or null if
         * no such object exists.
         * */
        public CorValue CurrentNotification
        {
            get
            {
                ICorDebugThread4 th4 = (ICorDebugThread4)m_th;

                ICorDebugValue v = null;
                th4.GetCurrentCustomDebuggerNotification(out v);
                return (v == null) ? null : new CorValue(v);
            }
        }

        /// <summary>
        /// Returns true if this thread has an unhandled managed exception
        /// </summary>
        public bool HasUnhandledException
        {
            get
            {
                // This is only supported on ICorDebugThread4
                ICorDebugThread4 th4 = m_th as ICorDebugThread4;
                if (th4 == null)
                    throw new NotSupportedException();
                else
                {
                    int ret = th4.HasUnhandledException();
                    if (ret == (int)HResult.S_OK)
                        return true;
                    else if (ret == (int)HResult.S_FALSE)
                        return false;
                    else
                        Marshal.ThrowExceptionForHR(ret);
                    // unreachable
                    throw new Exception();
                }
            }
        }

        /** 
         * Clear the current exception object, preventing it from being thrown.
         */
        public void ClearCurrentException()
        {
            m_th.ClearCurrentException();
        }

        /** 
         * Intercept the current exception.
         */
        public void InterceptCurrentException(CorFrame frame)
        {
            ICorDebugThread2 m_th2 = (ICorDebugThread2)m_th;
            m_th2.InterceptCurrentException(frame.m_frame);
        }

        /** 
         * create a stepper object relative to the active frame in this thread.
         */
        public CorStepper CreateStepper()
        {
            ICorDebugStepper s = null;
            m_th.CreateStepper(out s);
            return new CorStepper(s);
        }

        /** All stack chains in the thread. */
        public IEnumerable Chains
        {
            get
            {
                ICorDebugChainEnum ec = null;
                m_th.EnumerateChains(out ec);
                return (ec == null) ? null : new CorChainEnumerator(ec);
            }
        }

        /** The most recent chain in the thread, if any. */
        public CorChain ActiveChain
        {
            get
            {
                ICorDebugChain ch = null;
                m_th.GetActiveChain(out ch);
                return ch == null ? null : new CorChain(ch);
            }
        }

        /** Get the active frame. */
        public CorFrame ActiveFrame
        {
            get
            {
                ICorDebugFrame f = null;
                m_th.GetActiveFrame(out f);
                return f == null ? null : new CorFrame(f);
            }
        }

        /** Get the register set for the active part of the thread. */
        public CorRegisterSet RegisterSet
        {
            get
            {
                ICorDebugRegisterSet r = null;
                m_th.GetRegisterSet(out r);
                return r == null ? null : new CorRegisterSet(r);
            }
        }

        /** Creates an evaluation object. */
        public CorEval CreateEval()
        {
            ICorDebugEval e = null;
            m_th.CreateEval(out e);
            return e == null ? null : new CorEval(e);
        }

        /** Get the runtime thread object. */
        public CorValue ThreadVariable
        {
            get
            {
                ICorDebugValue v = null;
                m_th.GetObject(out v);
                return new CorValue(v);
            }
        }

        public CorActiveFunction[] GetActiveFunctions()
        {
            ICorDebugThread2 m_th2 = (ICorDebugThread2)m_th;
            UInt32 pcFunctions;
            m_th2.GetActiveFunctions(0, out pcFunctions, null);
            COR_ACTIVE_FUNCTION[] afunctions = new COR_ACTIVE_FUNCTION[pcFunctions];
            m_th2.GetActiveFunctions(pcFunctions, out pcFunctions, afunctions);
            CorActiveFunction[] caf = new CorActiveFunction[pcFunctions];
            for (int i = 0; i < pcFunctions; ++i)
            {
                caf[i] = new CorActiveFunction((int)afunctions[i].ilOffset,
                                               new CorFunction((ICorDebugFunction)afunctions[i].pFunction),
                                               afunctions[i].pModule == null ? null : new CorModule(afunctions[i].pModule)
                                               );
            }
            return caf;
        }
     
        public bool IsV3
        {
            get
            {
                ICorDebugThread3 th3 = m_th as ICorDebugThread3;
                if (th3 == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /** 
         * If PureV3StackWalk is specified, then this method returns a CorStackWalk, which does not expose
         * ICorDebugInternalFrames.  If ExtendedV3StackWalk is specified, then this method returns a 
         * CorStackWalkEx, which derives from CorStackWalk and interleaves ICorDebugInternalFrames.
         */
        public CorStackWalk CreateStackWalk (CorStackWalkType type)
        {
            ICorDebugThread3 th3 = m_th as ICorDebugThread3;
            if (th3 == null)
            {
                return null;
            }

            ICorDebugStackWalk s = null;
            th3.CreateStackWalk (out s);
            if (type == CorStackWalkType.PureV3StackWalk)
            {
                return new CorStackWalk(s, this);
            }
            else
            {
                return new CorStackWalkEx(s, this);
            }
        }
        public CorStackWalk CreateStackWalk()
        {
            return CreateStackWalk(CorStackWalkType.PureV3StackWalk);
        }

        public CorFrame[] GetActiveInternalFrames()
        {
            ICorDebugThread3 th3 = (ICorDebugThread3)m_th;

            UInt32 cInternalFrames = 0;
            th3.GetActiveInternalFrames(0, out cInternalFrames, null);

            ICorDebugInternalFrame2[] ppInternalFrames = new ICorDebugInternalFrame2[cInternalFrames];
            th3.GetActiveInternalFrames(cInternalFrames, out cInternalFrames, ppInternalFrames);

            CorFrame[] corFrames = new CorFrame[cInternalFrames];
            for (int i = 0; i < cInternalFrames; i++)
            {
                corFrames[i] = new CorFrame(ppInternalFrames[i] as ICorDebugFrame);
            }
            return corFrames;
        }

        ///<summary>
        ///Returns an array of objects which this thread is blocked on
        ///</summary>
        public CorBlockingObject[] GetBlockingObjects()
        {
            ICorDebugThread4 th4 = m_th as ICorDebugThread4;
            if (th4 == null)
                throw new NotSupportedException();
            ICorDebugEnumBlockingObject blockingObjectEnumerator;
            th4.GetBlockingObjects(out blockingObjectEnumerator);
            uint countBlockingObjects;
            blockingObjectEnumerator.GetCount(out countBlockingObjects);
            CorDebugBlockingObject[] rawBlockingObjects = new CorDebugBlockingObject[countBlockingObjects];
            uint countFetched;
            blockingObjectEnumerator.Next(countBlockingObjects, rawBlockingObjects, out countFetched);
            Debug.Assert(countFetched == countBlockingObjects);
            CorBlockingObject[] blockingObjects = new CorBlockingObject[countBlockingObjects];
            for(int i = 0; i < countBlockingObjects; i++)
            {
                blockingObjects[i].BlockingObject = new CorValue(rawBlockingObjects[i].BlockingObject);
                if(rawBlockingObjects[i].Timeout == uint.MaxValue)
                {
                    blockingObjects[i].Timeout = TimeSpan.MaxValue;
                }
                else
                {
                    blockingObjects[i].Timeout = TimeSpan.FromMilliseconds(rawBlockingObjects[i].Timeout);
                }
                blockingObjects[i].BlockingReason = rawBlockingObjects[i].BlockingReason;
            }
            return blockingObjects;
        }

        private ICorDebugThread m_th;

    } /* class Thread */



    public enum CorFrameType
    {
        ILFrame, NativeFrame, InternalFrame,          
            RuntimeUnwindableFrame
    }


    public sealed class CorFrame : WrapperBase
    {
        internal CorFrame(ICorDebugFrame frame)
            : base(frame)
        {
            m_frame = frame;
        }

        [CLSCompliant(false)]
        public ICorDebugFrame Raw
        {
            get 
            { 
                return m_frame;
            }
        }

        public CorStepper CreateStepper()
        {
            ICorDebugStepper istepper;
            m_frame.CreateStepper(out istepper);
            return (istepper == null ? null : new CorStepper(istepper));
        }

        public CorFrame Callee
        {
            get
            {
                ICorDebugFrame iframe;
                m_frame.GetCallee(out iframe);
                return (iframe == null ? null : new CorFrame(iframe));
            }
        }

        public CorFrame Caller
        {
            get
            {
                ICorDebugFrame iframe;
                m_frame.GetCaller(out iframe);
                return (iframe == null ? null : new CorFrame(iframe));
            }
        }

        public CorChain Chain
        {
            get
            {
                ICorDebugChain ichain;
                m_frame.GetChain(out ichain);
                return (ichain == null ? null : new CorChain(ichain));
            }
        }

        public CorCode Code
        {
            get
            {
                ICorDebugCode icode;
                m_frame.GetCode(out icode);
                return (icode == null ? null : new CorCode(icode));
            }
        }

        public CorFunction Function
        {
            get
            {
                ICorDebugFunction ifunction;
                try
                {
                    m_frame.GetFunction(out ifunction);
                }
                catch (System.Runtime.InteropServices.COMException e)
                {
                    if (e.ErrorCode == (int)HResult.CORDBG_E_CODE_NOT_AVAILABLE)
                    {
                        return null;
                    }
                    else
                    {
                        throw;
                    }
                }

                return (ifunction == null ? null : new CorFunction(ifunction));
            }
        }

        public int FunctionToken
        {
            get
            {
                uint token;
                m_frame.GetFunctionToken(out token);
                return (int)token;
            }
        }

        public CorFrameType FrameType
        {
            get
            {
                ICorDebugILFrame ilframe = GetILFrame();
                if (ilframe != null)
                    return CorFrameType.ILFrame;

                ICorDebugInternalFrame iframe = GetInternalFrame();
                if (iframe != null)
                    return CorFrameType.InternalFrame;

                ICorDebugRuntimeUnwindableFrame ruf = GetRuntimeUnwindableFrame();
                if (ruf != null)
                    return CorFrameType.RuntimeUnwindableFrame;
                return CorFrameType.NativeFrame;
            }
        }

        [CLSCompliant(false)]
        public CorDebugInternalFrameType InternalFrameType
        {
            get
            {
                ICorDebugInternalFrame iframe = GetInternalFrame();
                CorDebugInternalFrameType ft;

                if (iframe == null)
                    throw new CorException("Cannot get frame type on non-internal frame");

                iframe.GetFrameType(out ft);
                return ft;
            }
        }

        [CLSCompliant(false)]
        public ulong Address
        {
            get 
            {
                ICorDebugInternalFrame iframe = GetInternalFrame();
                if (iframe == null)
                {
                    throw new CorException("Cannot get the frame address on non-internal frame");
                }

                ulong address = 0;
                ICorDebugInternalFrame2 iframe2 = (ICorDebugInternalFrame2)iframe;
                iframe2.GetAddress(out address);
                return address;
            }
        }

        public bool IsCloserToLeaf(CorFrame frameToCompare)
        {
            ICorDebugInternalFrame2 iFrame2 = m_frame as ICorDebugInternalFrame2;
            if (iFrame2 == null)
            {
                throw new ArgumentException("The this object is not an ICorDebugInternalFrame");
            }

            int isCloser = 0;
            iFrame2.IsCloserToLeaf(frameToCompare.m_frame, out isCloser);
            return (isCloser == 0 ? false : true);
        }

        [CLSCompliant(false)]
        public void GetStackRange(out UInt64 startOffset, out UInt64 endOffset)
        {
            m_frame.GetStackRange(out startOffset, out endOffset);
        }

        [CLSCompliant(false)]
        public void GetIP(out uint offset, out CorDebugMappingResult mappingResult)
        {
            ICorDebugILFrame ilframe = GetILFrame();
            if (ilframe == null)
            {
                offset = 0;
                mappingResult = CorDebugMappingResult.MAPPING_NO_INFO;
            }
            else
                ilframe.GetIP(out offset, out mappingResult);
        }

        public void SetIP(int offset)
        {
            ICorDebugILFrame ilframe = GetILFrame();
            if (ilframe == null)
                throw new CorException("Cannot set an IP on non-il frame");
            ilframe.SetIP((uint)offset);
        }

        public bool CanSetIP(int offset)
        {
            ICorDebugILFrame ilframe = GetILFrame();
            if (ilframe == null)
                return false;
            return (ilframe.CanSetIP((uint)offset) == (int)HResult.S_OK);
        }

        public bool CanSetIP(int offset, out int hresult)
        {
            ICorDebugILFrame ilframe = GetILFrame();
            if (ilframe == null)
            {
                hresult = (int)HResult.E_FAIL;
                return false;
            }
            hresult = ilframe.CanSetIP((uint)offset);
            return (hresult == (int)HResult.S_OK);
        }

        [CLSCompliant(false)]
        public void GetNativeIP(out uint offset)
        {
            ICorDebugNativeFrame nativeFrame = m_frame as ICorDebugNativeFrame;
            Debug.Assert(nativeFrame != null);
            nativeFrame.GetIP(out offset);
        }
        public bool IsChild
        {
            get
            {
                ICorDebugNativeFrame2 nativeFrame2 = m_frame as ICorDebugNativeFrame2;
                if (nativeFrame2 == null)
                {
                    return false;
                }

                int isChild = 0;
                nativeFrame2.IsChild(out isChild);
                return (isChild == 0 ? false : true);
            }
        }

        [CLSCompliant(false)]
        public bool IsMatchingParentFrame(CorFrame parentFrame)
        {
            if (!this.IsChild)
            {
                return false;
            }
            ICorDebugNativeFrame2 nativeFrame2 = m_frame as ICorDebugNativeFrame2;
            Debug.Assert(nativeFrame2 != null);

            ICorDebugNativeFrame2 nativeParentFrame2 = parentFrame.m_frame as ICorDebugNativeFrame2;
            if (nativeParentFrame2 == null)
            {
                return false;
            }

            int isParent = 0;
            nativeFrame2.IsMatchingParentFrame(nativeParentFrame2, out isParent);
            return (isParent == 0 ? false : true);
        }

        [CLSCompliant(false)]
        public uint CalleeStackParameterSize
        {
            get
            {
                ICorDebugNativeFrame2 nativeFrame2 = m_frame as ICorDebugNativeFrame2;
                Debug.Assert(nativeFrame2 != null);

                uint paramSize = 0;
                nativeFrame2.GetCalleeStackParameterSize(out paramSize);
                return paramSize;
            }
        }

        public CorValue GetLocalVariable(int index)
        {
            ICorDebugILFrame ilframe = GetILFrame();
            if (ilframe == null)
                return null;

            ICorDebugValue value;
            try
            {
                ilframe.GetLocalVariable((uint)index, out value);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                // If you are stopped in the Prolog, the variable may not be available.
                // CORDBG_E_IL_VAR_NOT_AVAILABLE is returned after dubugee triggers StackOverflowException
                if (e.ErrorCode == (int)HResult.CORDBG_E_IL_VAR_NOT_AVAILABLE)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
            return (value == null) ? null : new CorValue(value);
        }

        public int GetLocalVariablesCount()
        {
            ICorDebugILFrame ilframe = GetILFrame();
            if (ilframe == null)
                return -1;

            ICorDebugValueEnum ve;
            ilframe.EnumerateLocalVariables(out ve);
            uint count;
            ve.GetCount(out count);
            return (int)count;
        }

        public CorValue GetArgument(int index)
        {
            ICorDebugILFrame ilframe = GetILFrame();
            if (ilframe == null)
                return null;


            ICorDebugValue value;
            ilframe.GetArgument((uint)index, out value);
            return (value == null) ? null : new CorValue(value);
        }

        public int GetArgumentCount()
        {
            ICorDebugILFrame ilframe = GetILFrame();
            if (ilframe == null)
                return -1;

            ICorDebugValueEnum ve;
            ilframe.EnumerateArguments(out ve);
            uint count;
            ve.GetCount(out count);
            return (int)count;
        }

        public void RemapFunction(int newILOffset)
        {
            ICorDebugILFrame ilframe = GetILFrame();
            if (ilframe == null)
                throw new CorException("Cannot remap on non-il frame.");
            ICorDebugILFrame2 ilframe2 = (ICorDebugILFrame2)ilframe;
            ilframe2.RemapFunction((uint)newILOffset);
        }

        private ICorDebugILFrame GetILFrame()
        {
            if (!m_ilFrameCached)
            {
                m_ilFrameCached = true;
                m_ilFrame = m_frame as ICorDebugILFrame;

            }
            return m_ilFrame;
        }

        private ICorDebugInternalFrame GetInternalFrame()
        {
            if (!m_iFrameCached)
            {
                m_iFrameCached = true;

                m_iFrame = m_frame as ICorDebugInternalFrame;
            }
            return m_iFrame;
        }

        private ICorDebugRuntimeUnwindableFrame GetRuntimeUnwindableFrame()
        {
            if(!m_ruFrameCached) 
            {
                m_ruFrameCached = true;
                
                m_ruFrame = m_frame as ICorDebugRuntimeUnwindableFrame;
            }
            return m_ruFrame;
        }
        // 'TypeParameters' returns an enumerator that goes yields generic args from
        // both the class and the method. To enumerate just the generic args on the 
        // method, we need to skip past the class args. We have to get that skip value
        // from the metadata. This is a helper function to efficiently get an enumerator that skips
        // to a given spot (likely past the class generic args). 
        public IEnumerable GetTypeParamEnumWithSkip(int skip)
        {
            if (skip < 0)
            {
                throw new ArgumentException("Skip parameter must be positive");
            }
            IEnumerable e = this.TypeParameters;
            Debug.Assert(e is CorTypeEnumerator);

            // Skip will throw if we try to skip the whole collection
            int total = (e as CorTypeEnumerator).Count;
            if (skip >= total)
            {
                return new CorTypeEnumerator(null); // empty.
            }

            (e as CorTypeEnumerator).Skip(skip);
            return e;
        }

        public IEnumerable TypeParameters
        {
            get
            {
                ICorDebugTypeEnum icdte = null;
                ICorDebugILFrame ilf = GetILFrame();

                (ilf as ICorDebugILFrame2).EnumerateTypeParameters(out icdte);
                return new CorTypeEnumerator(icdte);        // icdte can be null, is handled by enumerator
            }
        }



        private ICorDebugILFrame m_ilFrame = null;
        private bool m_ilFrameCached = false;

        private ICorDebugInternalFrame m_iFrame = null;
        private bool m_iFrameCached = false;
        private ICorDebugRuntimeUnwindableFrame m_ruFrame = null;
        private bool m_ruFrameCached = false;

        internal ICorDebugFrame m_frame;
    }

    public sealed class CorChain : WrapperBase
    {
        internal CorChain(ICorDebugChain chain)
            : base(chain)
        {
            m_chain = chain;
        }

        [CLSCompliant(false)]
        public ICorDebugChain Raw
        {
            get 
            { 
                return m_chain;
            }
        }

        public CorFrame ActiveFrame
        {
            get
            {
                ICorDebugFrame iframe;
                m_chain.GetActiveFrame(out iframe);
                return (iframe == null ? null : new CorFrame(iframe));
            }
        }

        public CorChain Callee
        {
            get
            {
                ICorDebugChain ichain;
                m_chain.GetCallee(out ichain);
                return (ichain == null ? null : new CorChain(ichain));
            }
        }

        public CorChain Caller
        {
            get
            {
                ICorDebugChain ichain;
                m_chain.GetCaller(out ichain);
                return (ichain == null ? null : new CorChain(ichain));
            }
        }

        public CorContext Context
        {
            get
            {
                ICorDebugContext icontext;
                m_chain.GetContext(out icontext);
                return (icontext == null ? null : new CorContext(icontext));
            }
        }

        public CorChain Next
        {
            get
            {
                ICorDebugChain ichain;
                m_chain.GetNext(out ichain);
                return (ichain == null ? null : new CorChain(ichain));
            }
        }

        public CorChain Previous
        {
            get
            {
                ICorDebugChain ichain;
                m_chain.GetPrevious(out ichain);
                return (ichain == null ? null : new CorChain(ichain));
            }
        }

        [CLSCompliant(false)]
        public CorDebugChainReason Reason
        {
            get
            {
                CorDebugChainReason reason;
                m_chain.GetReason(out reason);
                return reason;
            }
        }

        public CorRegisterSet RegisterSet
        {
            get
            {
                ICorDebugRegisterSet r = null;
                m_chain.GetRegisterSet(out r);
                return r == null ? null : new CorRegisterSet(r);
            }
        }

        public void GetStackRange(out Int64 pStart, out Int64 pEnd)
        {
            UInt64 start = 0;
            UInt64 end = 0;
            m_chain.GetStackRange(out start, out end);
            pStart = (Int64)start;
            pEnd = (Int64)end;
        }

        public CorThread Thread
        {
            get
            {
                ICorDebugThread ithread;
                m_chain.GetThread(out ithread);
                return (ithread == null ? null : new CorThread(ithread));
            }
        }

        public bool IsManaged
        {
            get
            {
                int managed;
                m_chain.IsManaged(out managed);
                return (managed != 0 ? true : false);
            }
        }

        public IEnumerable Frames
        {
            get
            {
                ICorDebugFrameEnum ef = null;
                m_chain.EnumerateFrames(out ef);
                return (ef == null) ? null : new CorFrameEnumerator(ef);
            }
        }

        private ICorDebugChain m_chain;
    }

    internal class CorFrameEnumerator : IEnumerable, IEnumerator, ICloneable
    {
        internal CorFrameEnumerator(ICorDebugFrameEnum frameEnumerator)
        {
            m_enum = frameEnumerator;
        }

        //
        // ICloneable interface
        //
        public Object Clone()
        {
            ICorDebugEnum clone = null;
            m_enum.Clone(out clone);
            return new CorFrameEnumerator((ICorDebugFrameEnum)clone);
        }

        //
        // IEnumerable interface
        //
        public IEnumerator GetEnumerator()
        {
            return this;
        }

        //
        // IEnumerator interface
        //
        public bool MoveNext()
        {
            ICorDebugFrame[] a = new ICorDebugFrame[1];
            uint c = 0;
            int r = m_enum.Next((uint)a.Length, a, out c);
            if (r == 0 && c == 1) // S_OK && we got 1 new element
                m_frame = new CorFrame(a[0]);
            else
                m_frame = null;
            return m_frame != null;
        }

        public void Reset()
        {
            m_enum.Reset();
            m_frame = null;
        }

        public Object Current
        {
            get
            {
                return m_frame;
            }
        }

        private ICorDebugFrameEnum m_enum;
        private CorFrame m_frame;
    }


    public struct IL2NativeMap
    {
        public int IlOffset
        {
            get
            {
                return m_ilOffset;
            }
        }
        private int m_ilOffset;

        public int NativeStartOffset
        {
            get
            {
                return m_nativeStartOffset;
            }
        }
        private int m_nativeStartOffset;

        public int NativeEndOffset
        {
            get
            {
                return m_nativeEndOffset;
            }
        }
        private int m_nativeEndOffset;

        internal IL2NativeMap(int ilOffset, int nativeStartOffset, int nativeEndOffset)
        {
            m_ilOffset = ilOffset;
            m_nativeStartOffset = nativeStartOffset;
            m_nativeEndOffset = nativeEndOffset;
        }
    }


    public sealed class CorCode : WrapperBase
    {
        internal CorCode(ICorDebugCode code)
            : base(code)
        {
            m_code = code;
        }

        [CLSCompliant(false)]
        public ICorDebugCode Raw
        {
            get 
            { 
                return m_code;
            }
        }

        public CorFunctionBreakpoint CreateBreakpoint(int offset)
        {
            ICorDebugFunctionBreakpoint ibreakpoint;
            m_code.CreateBreakpoint((uint)offset, out ibreakpoint);
            return (ibreakpoint == null ? null : new CorFunctionBreakpoint(ibreakpoint));
        }

        [CLSCompliant(false)]
        public ulong Address
        {
            get
            {
                UInt64 start;
                m_code.GetAddress(out start);
                return start;
            }
        }

        public CorDebugJITCompilerFlags CompilerFlags
        {
            get
            {
                uint dwFlags;
                (m_code as ICorDebugCode2).GetCompilerFlags(out dwFlags);
                return (CorDebugJITCompilerFlags)dwFlags;
            }
        }

        public byte[] GetCode()
        {
            uint codeSize = (uint)this.Size;

            byte[] code = new byte[codeSize];
            uint returnedCode;
            m_code.GetCode(0, codeSize, codeSize, code, out returnedCode);
            Debug.Assert(returnedCode == codeSize);
            return code;
        }

        [CLSCompliant(false)]
        public _CodeChunkInfo[] GetCodeChunks()
        {
            UInt32 pcnumChunks;
            (m_code as ICorDebugCode2).GetCodeChunks(0, out pcnumChunks, null);
            if (pcnumChunks == 0)
                return new _CodeChunkInfo[0];

            _CodeChunkInfo[] chunks = new _CodeChunkInfo[pcnumChunks];
            (m_code as ICorDebugCode2).GetCodeChunks((uint)chunks.Length, out pcnumChunks, chunks);
            return chunks;
        }

        public CorFunction GetFunction()
        {
            ICorDebugFunction ifunction;
            m_code.GetFunction(out ifunction);
            return (ifunction == null ? null : new CorFunction(ifunction));
        }

        public IL2NativeMap[] GetILToNativeMapping()
        {
            UInt32 pcMap;
            m_code.GetILToNativeMapping(0, out pcMap, null);
            if (pcMap == 0)
                return new IL2NativeMap[0];

            COR_DEBUG_IL_TO_NATIVE_MAP[] map = new COR_DEBUG_IL_TO_NATIVE_MAP[pcMap];
            m_code.GetILToNativeMapping((uint)map.Length, out pcMap, map);

            IL2NativeMap[] ret = new IL2NativeMap[map.Length];
            for (int i = 0; i < map.Length; i++)
            {
                ret[i] = new IL2NativeMap((int)map[i].ilOffset,
                                          (int)map[i].nativeStartOffset,
                                          (int)map[i].nativeEndOffset
                                          );
            }
            return ret;
        }

        [CLSCompliant(false)]
        public int Size
        {
            get
            {
                UInt32 pcBytes;
                m_code.GetSize(out pcBytes);
                return (int)pcBytes;
            }
        }

        public int VersionNumber
        {
            get
            {
                UInt32 nVersion;
                m_code.GetVersionNumber(out nVersion);
                return (int)nVersion;
            }
        }

        public bool IsIL
        {
            get
            {
                Int32 pbIL;
                m_code.IsIL(out pbIL);
                return (pbIL != 0 ? true : false);
            }
        }

        private ICorDebugCode m_code;
    }

    /** Exposes an enumerator for CodeEnum. */
    internal class CorCodeEnumerator : IEnumerable, IEnumerator, ICloneable
    {
        private ICorDebugCodeEnum m_enum;
        private CorCode m_c;

        internal CorCodeEnumerator(ICorDebugCodeEnum codeEnumerator)
        {
            m_enum = codeEnumerator;
        }

        //
        // ICloneable interface
        //
        public Object Clone()
        {
            ICorDebugEnum clone = null;
            m_enum.Clone(out clone);
            return new CorCodeEnumerator((ICorDebugCodeEnum)clone);
        }

        //
        // IEnumerable interface
        //
        public IEnumerator GetEnumerator()
        {
            return this;
        }

        //
        // IEnumerator interface
        //
        public bool MoveNext()
        {
            ICorDebugCode[] a = new ICorDebugCode[1];
            uint c = 0;
            int r = m_enum.Next((uint)a.Length, a, out c);
            if (r == 0 && c == 1) // S_OK && we got 1 new element
                m_c = new CorCode(a[0]);
            else
                m_c = null;
            return m_c != null;
        }

        public void Skip(uint celt)
        {
            m_enum.Skip(celt);
            m_c = null;
        }

        public void Reset()
        {
            m_enum.Reset();
            m_c = null;
        }

        public Object Current
        {
            get
            {
                return m_c;
            }
        }
    } /* class CodeEnumerator */

    public sealed class CorFunction : WrapperBase
    {
        internal CorFunction(ICorDebugFunction managedFunction)
            : base(managedFunction)
        {
            m_function = managedFunction;
        }

        [CLSCompliant(false)]
        public ICorDebugFunction Raw
        {
            get 
            { 
                return m_function;
            }
        }

        public CorFunctionBreakpoint CreateBreakpoint()
        {
            ICorDebugFunctionBreakpoint ifuncbreakpoint;
            m_function.CreateBreakpoint(out ifuncbreakpoint);
            return (ifuncbreakpoint == null ? null : new CorFunctionBreakpoint(ifuncbreakpoint));
        }

        public CorClass Class
        {
            get
            {
                ICorDebugClass iclass;
                m_function.GetClass(out iclass);
                return (iclass == null ? null : new CorClass(iclass));
            }
        }

        public CorCode ILCode
        {
            get
            {
                ICorDebugCode icode;
                m_function.GetILCode(out icode);
                return (icode == null ? null : new CorCode(icode));
            }
        }

        public CorCode NativeCode
        {
            get
            {
                ICorDebugCode icode;
                m_function.GetNativeCode(out icode);
                return (icode == null ? null : new CorCode(icode));
            }
        }


        public CorModule Module
        {
            get
            {
                ICorDebugModule imodule;
                m_function.GetModule(out imodule);
                return (imodule == null ? null : new CorModule(imodule));
            }
        }

        public int Token
        {
            get
            {
                UInt32 pMethodDef;
                m_function.GetToken(out pMethodDef);
                return (int)pMethodDef;
            }
        }

        public int Version
        {
            get
            {
                UInt32 pVersion;
                (m_function as ICorDebugFunction2).GetVersionNumber(out pVersion);
                return (int)pVersion;
            }
        }

        public bool JMCStatus
        {
            get
            {
                int status;
                (m_function as ICorDebugFunction2).GetJMCStatus(out status);
                return status != 0;
            }
            set
            {
                (m_function as ICorDebugFunction2).SetJMCStatus(value ? 1 : 0);
            }
        }
        internal ICorDebugFunction m_function;
    }

    public sealed class CorContext : WrapperBase
    {
        internal CorContext(ICorDebugContext context)
            : base(context)
        {
            m_context = context;
        }

        [CLSCompliant(false)]
        public ICorDebugContext Raw
        {
            get 
            { 
                return m_context;
            }
        }

        private ICorDebugContext m_context;
    }

    [Serializable]
    public class CorException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the CorException.
        /// </summary>
        public CorException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the CorException with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CorException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CorException with the specified error message and inner Exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public CorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CorException class with serialized data. 
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected CorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

} /* namespace */
