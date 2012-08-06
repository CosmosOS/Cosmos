//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Microsoft.Samples.Debugging.Native;
using Microsoft.Samples.Debugging.Native.Private;

namespace Microsoft.Samples.Debugging.Native
{
    public class AMD64Context : INativeContext, IEquatable<INativeContext>, IDisposable
    {
        private int m_size;
        private Platform m_platform;
        private int m_imageFileMachine;
        private IntPtr m_rawPtr;

        ~AMD64Context()
        {
            this.Dispose(false);
        }

        // This is a private contstructor for cloning Context objects.  It should not be used outside of this class.
        private AMD64Context(INativeContext ctx)
        {
            this.m_size = ctx.Size;
            this.m_platform = ctx.Platform;
            this.m_imageFileMachine = ctx.ImageFileMachine;
            this.m_rawPtr = Marshal.AllocHGlobal(this.Size);

            using (IContextDirectAccessor w = ctx.OpenForDirectAccess())
            {
                // The fact that Marshal.Copy cannot copy between to native memory locations is stupid.
                for (int i = 0; i < this.m_size; i++)
                {
                    // In theory, we could make this faster by copying larger units (i.e. Int64).  We get our sizes in bytes
                    // so for now we'll just copy bytes to keep our units straight.
                    byte chunk = Marshal.ReadByte(w.RawBuffer, i);
                    Marshal.WriteByte(this.RawPtr, i, chunk);
                }
            }
        }

        public AMD64Context()
            : this(AgnosticContextFlags.ContextControl | AgnosticContextFlags.ContextInteger)
        {
        }

        public AMD64Context(AgnosticContextFlags aFlags)
        {
            InitializeContext();
            WriteContextFlagsToBuffer(GetPSFlags(aFlags));
            
        }

        public AMD64Context(ContextFlags flags)
        {
            InitializeContext();
            WriteContextFlagsToBuffer(flags);  
        }

        private void InitializeContext()
        {
            this.m_size = (int)ContextSize.AMD64;
            this.m_platform = Platform.AMD64;
            this.m_imageFileMachine = (int)Native.ImageFileMachine.AMD64;
            this.m_rawPtr = Marshal.AllocHGlobal(this.Size);

            return;
        }

        private void WriteContextFlagsToBuffer(ContextFlags flags)
        {
            Debug.Assert(this.RawPtr != IntPtr.Zero);
            Marshal.WriteInt32(this.RawPtr, (int)AMD64Offsets.ContextFlags, (Int32)flags);
        }

        public IContextDirectAccessor OpenForDirectAccess()
        {
            // we can return a copy of ourself so that it will be destroyed once it goes out of scope.
            return new ContextAccessor(this.RawPtr, this.Size);
        }

        public void ClearContext()
        {
            // make sure that we have access to the buffer
            using (IContextDirectAccessor w = this.OpenForDirectAccess())
            {
                for (int i = 0; i < w.Size; i++)
                {
                    Marshal.WriteByte(w.RawBuffer, i, (byte)0);
                }
            }
        }

        // returns the platform-specific Context Flags
        public ContextFlags GetPSFlags(AgnosticContextFlags flags)
        {
            
            // We know that we need an amd64 context
            this.m_platform = Platform.AMD64;
            ContextFlags cFlags = ContextFlags.AMD64Context;
               

            if ((flags & AgnosticContextFlags.ContextInteger) == AgnosticContextFlags.ContextInteger)
            {
                // ContextInteger is the same for all platforms, so we can do a blanket |=
                cFlags |= (ContextFlags)AgnosticContextFlags.ContextInteger;
            }
            if ((flags & AgnosticContextFlags.ContextControl) == AgnosticContextFlags.ContextControl)
            {
                cFlags |= (ContextFlags)AgnosticContextFlags.ContextControl;
            }
            if ((flags & AgnosticContextFlags.ContextFloatingPoint) == AgnosticContextFlags.ContextFloatingPoint)
            {
             
                cFlags |= (ContextFlags)AgnosticContextFlags.ContextFloatingPoint;
            }
            if ((flags & AgnosticContextFlags.ContextAll) == AgnosticContextFlags.ContextAll)
            {
                // ContextAll is the same for all platforms, so we can do a blanket |=
                cFlags |= (ContextFlags)AgnosticContextFlags.ContextAll;
            }

            return cFlags;
        }

        // Sets m_platform the the platform represented by the context flags
        private Platform GetPlatform(ContextFlags flags)
        { 
            return Platform.AMD64;
        }

        // returns the size of the Context
        public int Size
        {
            get
            {
                return this.m_size;
            }
        }

        // returns a pointer to the Context
        private IntPtr RawPtr
        {
            get
            {
                return this.m_rawPtr;
            }
        }

        // returns the ContextFlags for the Context
        public ContextFlags Flags
        {
            get
            {
                Debug.Assert(this.RawPtr != IntPtr.Zero);
                return (ContextFlags)Marshal.ReadInt32(this.RawPtr, (int)AMD64Offsets.ContextFlags);
            }
            set
            {
                if ((value & ContextFlags.AMD64Context) == 0)
                {
                    throw new ArgumentException("Process architecture flag must be set");
                }
                if ((value & ~ContextFlags.AMD64ContextAll) != 0)
                {
                    throw new ArgumentException("Unsupported context flags for this architecture");
                }
                WriteContextFlagsToBuffer(value);
            }
        }

        // returns the StackPointer for the Context
        public IntPtr StackPointer
        {
            get
            {
                Debug.Assert(this.m_rawPtr != IntPtr.Zero, "The context has an invalid context pointer");
                
                return (IntPtr)Marshal.ReadInt64(this.m_rawPtr, (int)AMD64Offsets.Rsp); 
            }
        }

        // returns a Platform for the Context
        public Platform Platform
        {
            get
            {
                return this.m_platform;
            }
        }

        // returns the IP for the Context
        public IntPtr InstructionPointer
        {
            get
            {
                IntPtr ip = IntPtr.Zero;
                using (IContextDirectAccessor w = this.OpenForDirectAccess())
                {
                    ip = (IntPtr)Marshal.ReadInt64(w.RawBuffer, (int)AMD64Offsets.Rip);         
                }

                return ip;
            }
            set
            {
                using (IContextDirectAccessor w = this.OpenForDirectAccess())
                {
                    Marshal.WriteInt64(w.RawBuffer, (int)AMD64Offsets.Rip, (Int64)value);
                }
            }
        }

        // returns the ImageFileMachine for the Context
        public int ImageFileMachine
        {
            get
            {
                return this.m_imageFileMachine;
            }
        }

        public void SetSingleStepFlag(bool fEnable)
        {
            if (fEnable)
            {
                int eflags = Marshal.ReadInt32(this.RawPtr, (int)AMD64Offsets.EFlags) | (int)AMD64Flags.SINGLE_STEP_FLAG;
                Marshal.WriteInt32(this.RawPtr, (int)AMD64Offsets.EFlags, eflags);
            }
            else
            {
                int eflags = Marshal.ReadInt32(this.RawPtr, (int)AMD64Offsets.EFlags) & ~((int)AMD64Flags.SINGLE_STEP_FLAG);
                Marshal.WriteInt32(this.RawPtr, (int)AMD64Offsets.EFlags, eflags);
            }
        }

        public bool IsSingleStepFlagEnabled
        {
            get
            {
                return ((Marshal.ReadInt32(this.RawPtr, (int)AMD64Offsets.EFlags) & (int)AMD64Flags.SINGLE_STEP_FLAG) != 0);
            }
        }

        public INativeContext Clone()
        {
            return new AMD64Context(this);
        }

        public bool Equals(INativeContext other)
        {
            if (this.Platform != other.Platform)
            {
                return false;
            }

            if (this.ImageFileMachine != other.ImageFileMachine)
            {
                return false;
            }

            if (this.Size != other.Size)
            {
                return false;
            }

            return CheckContexts(this, (AMD64Context)other);
        }

        public bool CheckContexts(INativeContext a1, INativeContext a2)
        {
            return CheckAMD64Contexts((AMD64Context)a1, (AMD64Context)a2);
        }

        // compare a chuck of the context record contained between the two offsets
        private bool CheckContextChunk(AMD64Context a1, AMD64Context a2, int startOffset, int endOffset)
        {
            using (IContextDirectAccessor c1 = a1.OpenForDirectAccess())
            {
                using (IContextDirectAccessor c2 = a2.OpenForDirectAccess())
                {
                    for (int i = startOffset; i < endOffset; i++)
                    {
                        if (Marshal.ReadByte(c1.RawBuffer, i) != Marshal.ReadByte(c2.RawBuffer, i))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private bool CheckAMD64Contexts(AMD64Context a1, AMD64Context a2)
        {
            //CONTEXT_CONTROL contains: SegCs, SegSs, EFlags, Rsp, Rip
            if ((a1.Flags & ContextFlags.AMD64ContextControl) == ContextFlags.AMD64ContextControl)
            {

                if (!CheckContextChunk(a1, a2, (int)AMD64Offsets.SegCs, (int)AMD64Offsets.SegDs) ||
                    !CheckContextChunk(a1, a2, (int)AMD64Offsets.SegSs, (int)AMD64Offsets.Dr0) ||
                    !CheckContextChunk(a1, a2, (int)AMD64Offsets.Rsp, (int)AMD64Offsets.Rbp) ||
                    !CheckContextChunk(a1, a2, (int)AMD64Offsets.Rip, (int)AMD64Offsets.FltSave))
                {
                    return false;
                }
            }

            // CONTEXT_INTEGER contains: Rax, Rcx, Rdx, Rbx, Rbp, Rsi, Rdi, R8-R15
            if ((a1.Flags & ContextFlags.AMD64ContextInteger) == ContextFlags.AMD64ContextInteger)
            {
                if (!CheckContextChunk(a1, a2, (int)AMD64Offsets.Rax, (int)AMD64Offsets.Rsp) ||
                   !CheckContextChunk(a1, a2, (int)AMD64Offsets.Rbp, (int)AMD64Offsets.Rip))
                {
                    return false;
                }
            }

            // CONTEXT_SEGMENTS contains: SegDs, SegEs, SegFs, SegGs
            if ((a1.Flags & ContextFlags.AMD64ContextSegments) == ContextFlags.AMD64ContextSegments)
            {
                if (!CheckContextChunk(a1, a2, (int)AMD64Offsets.SegDs, (int)AMD64Offsets.SegSs))
                {
                    return false;
                }
            }
            //CONTEXT_DEBUG_REGISTERS contains: Dr0-Dr3, Dr6-Dr7 (these are continuous in winnt.h)
            if ((a1.Flags & ContextFlags.AMD64ContextDebugRegisters) == ContextFlags.AMD64ContextDebugRegisters)
            {
                if (!CheckContextChunk(a1, a2, (int)AMD64Offsets.Dr0, (int)AMD64Offsets.Rax))
                {
                    return false;
                }
            }

            //CONTEXT_FLOATING_POINT contains: Mm0/St0-Mm7/St7, Xmm0-Xmm15
            if ((a1.Flags & ContextFlags.AMD64ContextFloatingPoint) == ContextFlags.AMD64ContextFloatingPoint)
            {
                if (!CheckContextChunk(a1, a2, (int)AMD64Offsets.FltSave, (int)AMD64Offsets.VectorRegister))
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasFlags(ContextFlags flags)
        {
            return ((this.Flags & flags) == flags);
        }

        public IEnumerable<String> EnumerateRegisters()
        {
            return EnumerateAMD64Registers();
        }

        private IEnumerable<String> EnumerateAMD64Registers()
        {
            List<String> list = new List<String>();

            // This includes the most commonly used flags. 
            if (HasFlags(ContextFlags.AMD64ContextControl))
            {
                list.Add("Rsp");
                list.Add("Rip");
                list.Add("EFlags");
            }

            if (HasFlags(ContextFlags.AMD64ContextInteger))
            {
                list.Add("Rax");
                list.Add("Rbx");
                list.Add("Rcx");
                list.Add("Rdx");
                list.Add("Rsi");
                list.Add("Rdi");
                list.Add("Rbp");
                list.Add("Rsi");
                list.Add("Rdi");
                list.Add("R8");
                list.Add("R9");
                list.Add("R10");
                list.Add("R11");
                list.Add("R12");
                list.Add("R13");
                list.Add("R14");
                list.Add("R15");
            }

            if (HasFlags(ContextFlags.AMD64ContextSegments))
            {
                list.Add("SegDs");
                list.Add("SegEs");
                list.Add("SegFs");
                list.Add("SegGs");
            }

            if (HasFlags(ContextFlags.AMD64ContextFloatingPoint))
            {
                list.Add("FltSave");
                list.Add("Legacy");
                list.Add("Xmm0");
                list.Add("Xmm1");
                list.Add("Xmm2");
                list.Add("Xmm3");
                list.Add("Xmm4");
                list.Add("Xmm5");
                list.Add("Xmm6");
                list.Add("Xmm7");
                list.Add("Xmm8");
                list.Add("Xmm9");
                list.Add("Xmm10");
                list.Add("Xmm11");
                list.Add("Xmm12");
                list.Add("Xmm13");
                list.Add("Xmm14");
                list.Add("Xmm15");
            }

            if (HasFlags(ContextFlags.AMD64ContextDebugRegisters))
            {
                list.Add("Dr0");
                list.Add("Dr1");
                list.Add("Dr2");
                list.Add("Dr3");
                list.Add("Dr6");
                list.Add("Dr7");
            }

            return list;
        }

        public object FindRegisterByName(String name)
        {
            return AMD64FindRegisterByName(name);
        }

        private object AMD64FindRegisterByName(String name)
        {
            name = name.ToUpperInvariant();

            if (HasFlags(ContextFlags.AMD64ContextControl))
            {
                if (name == "RSP") return Marshal.ReadInt64(this.RawPtr, (int)AMD64Offsets.Rsp);
                if (name == "RIP") return Marshal.ReadInt64(this.RawPtr, (int)AMD64Offsets.Rip);
                if (name == "EFLAGS") return Marshal.ReadInt32(this.RawPtr, (int)AMD64Offsets.EFlags);
            }
            if (HasFlags(ContextFlags.AMD64ContextInteger))
            {
                if (name == "RAX") return Marshal.ReadInt64(this.RawPtr, (int)AMD64Offsets.Rax);
                if (name == "RBX") return Marshal.ReadInt64(this.RawPtr, (int)AMD64Offsets.Rbx);
                if (name == "RCX") return Marshal.ReadInt64(this.RawPtr, (int)AMD64Offsets.Rcx);
                if (name == "RDX") return Marshal.ReadInt64(this.RawPtr, (int)AMD64Offsets.Rdx);
                if (name == "RSI") return Marshal.ReadInt64(this.RawPtr, (int)AMD64Offsets.Rsi);
                if (name == "RDI") return Marshal.ReadInt64(this.RawPtr, (int)AMD64Offsets.Rdi);
                if (name == "RBP") return Marshal.ReadInt64(this.RawPtr, (int)AMD64Offsets.Rbp);

            }
            if (HasFlags(ContextFlags.AMD64ContextSegments))
            {
                if (name == "SEGDS") return Marshal.ReadInt32(this.RawPtr, (int)AMD64Offsets.SegDs);
                if (name == "SEGES") return Marshal.ReadInt32(this.RawPtr, (int)AMD64Offsets.SegEs);
                if (name == "SEGFS") return Marshal.ReadInt32(this.RawPtr, (int)AMD64Offsets.SegFs);
                if (name == "SEGGS") return Marshal.ReadInt32(this.RawPtr, (int)AMD64Offsets.SegGs);
            }

            throw new InvalidOperationException(String.Format("Register '{0}' is not in the context", name));
        }

        public void SetRegisterByName(String name, object value)
        {
            AMD64SetRegisterByName(name, value);
        }

        private void AMD64SetRegisterByName(String name, object value)
        {
            name = name.ToUpperInvariant();

            if (HasFlags(ContextFlags.AMD64ContextControl))
            {
                if (name == "RSP") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)AMD64Offsets.Rsp, (Int64)value);
                    return;
                }
                if (name == "RIP") 
                {   
                    Marshal.WriteInt64(this.RawPtr, (int)AMD64Offsets.Rip, (Int64)value);
                    return;
                }
                if (name == "EFLAGS")
                {
                    Marshal.WriteInt32(this.RawPtr, (int)AMD64Offsets.EFlags, (Int32)value);
                    return;
                }
            }
            if (HasFlags(ContextFlags.AMD64ContextInteger))
            {
                if (name == "RAX")
                {
                    Marshal.WriteInt64(this.RawPtr, (int)AMD64Offsets.Rax, (Int64)value);
                    return;
                }
                if (name == "RBX")
                {
                    Marshal.WriteInt64(this.RawPtr, (int)AMD64Offsets.Rbx, (Int64)value);
                    return;
                }
                if (name == "RCX")
                {
                    Marshal.WriteInt64(this.RawPtr, (int)AMD64Offsets.Rcx, (Int64)value);
                    return;
                }
                if (name == "RDX")
                {
                    Marshal.WriteInt64(this.RawPtr, (int)AMD64Offsets.Rdx, (Int64)value);
                    return;
                }
                if (name == "RSI")
                {
                    Marshal.WriteInt64(this.RawPtr, (int)AMD64Offsets.Rsi, (Int64)value);
                    return;
                }
                if (name == "RDI")
                {
                    Marshal.WriteInt64(this.RawPtr, (int)AMD64Offsets.Rdi, (Int64)value);
                    return;
                }
                if (name == "RBP")
                {
                    Marshal.WriteInt64(this.RawPtr, (int)AMD64Offsets.Rbp, (Int64)value);
                    return;
                }
            }
            if (HasFlags(ContextFlags.AMD64ContextSegments))
            {
                if (name == "SEGDS")
                {
                    Marshal.WriteInt32(this.RawPtr, (int)AMD64Offsets.SegDs, (Int32)value);
                    return;
                }
                if (name == "SEGES")
                {
                    Marshal.WriteInt32(this.RawPtr, (int)AMD64Offsets.SegEs, (Int32)value);
                    return;
                }
                if (name == "SEGFS")
                {
                    Marshal.WriteInt32(this.RawPtr, (int)AMD64Offsets.SegFs, (Int32)value);
                    return;
                }
                if (name == "SEGGS")
                {
                    Marshal.WriteInt32(this.RawPtr, (int)AMD64Offsets.SegGs, (Int32)value);
                    return;
                }
            }

            throw new InvalidOperationException(String.Format("Register '{0}' is not in the context", name));
        }

        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose(true);
        }

        public void Dispose(bool supressPendingFinalizer)
        {
            this.m_imageFileMachine = 0;
            this.m_platform = Platform.None;

            Marshal.FreeHGlobal(this.m_rawPtr);
            this.m_size = 0;
            this.m_rawPtr = IntPtr.Zero;

            if (supressPendingFinalizer)
            {
                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }
}