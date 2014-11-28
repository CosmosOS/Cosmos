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
    public class X86Context : INativeContext, IEquatable<INativeContext>, IDisposable
    {
        private int m_size;
        private Platform m_platform;
        private int m_imageFileMachine;
        private IntPtr m_rawPtr;

        ~X86Context()
        {
            this.Dispose(false);
        }

        // This is a private contstructor for cloning Context objects.  It should not be used outside of this class.
        private X86Context(INativeContext ctx)
        {
            this.m_size = ctx.Size;
            this.m_platform = ctx.Platform;
            this.m_imageFileMachine = ctx.ImageFileMachine;
            this.m_rawPtr = Marshal.AllocHGlobal(this.Size);

            using (IContextDirectAccessor w = ctx.OpenForDirectAccess())
            {
                // The fact that Marshal.Copy cannot copy between to native memory locations is stupid.
                for (int i = 0; i < this.Size; i++)
                {
                    // In theory, we could make this faster by copying larger units (i.e. Int64).  We get our sizes in bytes
                    // so for now we'll just copy bytes to keep our units straight.
                    byte chunk = Marshal.ReadByte(w.RawBuffer, i);
                    Marshal.WriteByte(this.RawPtr, i, chunk);
                }
            }
        }

        public X86Context()
            : this(AgnosticContextFlags.ContextControl | AgnosticContextFlags.ContextInteger)
        {
        }

        public X86Context(AgnosticContextFlags aFlags)
        {
            InitializeContext();
            WriteContextFlagsToBuffer(GetPSFlags(aFlags));
        }

        public X86Context(ContextFlags flags)
        {
            InitializeContext();
            WriteContextFlagsToBuffer(flags);  
        }

        private void InitializeContext()
        {
            this.m_size = (int)ContextSize.X86;
            this.m_platform = Platform.X86;
            this.m_imageFileMachine = (int)Native.ImageFileMachine.X86;
            this.m_rawPtr = Marshal.AllocHGlobal(this.Size);

            ClearContext();

            return;
        }

        private void WriteContextFlagsToBuffer(ContextFlags flags)
        {
            Debug.Assert(this.RawPtr != IntPtr.Zero);
            Marshal.WriteInt32(this.RawPtr, (int)X86Offsets.ContextFlags, (Int32)flags);
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
            // We know that we need an x86 context
            ContextFlags cFlags = ContextFlags.X86Context;
   
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
                return (ContextFlags)Marshal.ReadInt32(this.RawPtr, (int)X86Offsets.ContextFlags);
            }
            set
            {
                if ((value & ContextFlags.X86Context) == 0)
                {
                    throw new ArgumentException("Process architecture flag must be set");
                }
                if ((value & ~ContextFlags.X86ContextAll) != 0)
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

                return (IntPtr)Marshal.ReadInt32(this.m_rawPtr, (int)X86Offsets.Esp);    
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
                    ip = (IntPtr)Marshal.ReadInt32(w.RawBuffer, (int)X86Offsets.Eip);
                }
                
                return ip;
            }
            set
            {
                using (IContextDirectAccessor w = this.OpenForDirectAccess())
                {
                    Marshal.WriteInt32(w.RawBuffer, (int)X86Offsets.Eip, (Int32)value);
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
                int eflags = Marshal.ReadInt32(this.RawPtr, (int)X86Offsets.EFlags) | (int)X86Flags.SINGLE_STEP_FLAG;
                Marshal.WriteInt32(this.RawPtr, (int)X86Offsets.EFlags, eflags);
            }
            else
            {
                
                int eflags = Marshal.ReadInt32(this.RawPtr, (int)X86Offsets.EFlags) & ~((int)X86Flags.SINGLE_STEP_FLAG);
                Marshal.WriteInt32(this.RawPtr, (int)X86Offsets.EFlags, eflags);
            }
        }

        public bool IsSingleStepFlagEnabled
        {
            get
            {
                return ((Marshal.ReadInt32(this.RawPtr, (int)X86Offsets.EFlags) & (int)X86Flags.SINGLE_STEP_FLAG) != 0);  
            }
        }

        public INativeContext Clone()
        {
            return new X86Context(this);
        }

        public bool Equals(INativeContext other)
        {
            if (this.Platform != other.Platform)
            {
                return false;
            }

            if (this.Flags != other.Flags)
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

            return CheckContexts(this, (X86Context)other);
        }

        public bool CheckContexts(INativeContext a1, INativeContext a2)
        {
            return CheckX86Contexts((X86Context)a1, (X86Context)a2);
        }

        // compare a chuck of the context record contained between the two offsets
        private bool CheckContextChunk(X86Context a1, X86Context a2, int startOffset, int endOffset)
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

        private bool CheckX86Contexts(X86Context a1, X86Context a2)
        {
            bool retVal = false;
            if ((a1.Flags & ContextFlags.X86ContextControl) == ContextFlags.X86ContextControl)
            {
                // Due to runtime limitations, we can't check EFLAGS and SegSs, see DevDiv 186645
                retVal = CheckContextChunk(a1, a2, (int)X86Offsets.Ebp, (int)X86Offsets.EFlags);

                if (!retVal)
                {
                    return retVal;
                }

                retVal = CheckContextChunk(a1, a2, (int)X86Offsets.Esp, (int)X86Offsets.SegSs);

                if (!retVal)
                {
                    return retVal;
                }

            }

            if ((a1.Flags & ContextFlags.X86ContextInteger) == ContextFlags.X86ContextInteger)
            {
                // check range Edi to Eax
                retVal = CheckContextChunk(a1, a2, (int)X86Offsets.Edi, (int)X86Offsets.Ebp);

                if (!retVal)
                {
                    return retVal;
                }
            }
            if ((a1.Flags & ContextFlags.X86ContextFloatingPoint) == ContextFlags.X86ContextFloatingPoint)
            {

                // check range FLOAT_SAVE
                retVal = CheckContextChunk(a1, a2, (int)X86Offsets.FloatSave, (int)X86Offsets.SegGs);

                if (!retVal)
                {
                    return retVal;
                }
            }
            return true;
        }

        private bool HasFlags(ContextFlags flags)
        {
            return ((flags & this.Flags) == flags);
        }

        public IEnumerable<String> EnumerateRegisters()
        {
            return EnumerateX86Registers();
        }

        private IEnumerable<String> EnumerateX86Registers()
        {
            List<String> list = new List<String>();

            // This includes the most commonly used flags. 
            if (HasFlags(ContextFlags.X86ContextDebugRegisters))
            {
                list.Add("Dr0");
                list.Add("Dr1");
                list.Add("Dr2");
                list.Add("Dr3");
                list.Add("Dr6");
                list.Add("Dr7");
            }

            if (HasFlags(ContextFlags.X86ContextFloatingPoint))
            {
                list.Add("FloatSave");
            }

            if (HasFlags(ContextFlags.X86ContextSegments))
            {
                list.Add("SegDs");
                list.Add("SegEs");
                list.Add("SegFs");
                list.Add("SegGs");
            }

            if (HasFlags(ContextFlags.X86ContextInteger))
            {
                list.Add("Eax");
                list.Add("Bbx");
                list.Add("Ccx");
                list.Add("Edx");
                list.Add("Esi");
                list.Add("edi");
            }

            if (HasFlags(ContextFlags.X86ContextControl))
            {
                list.Add("Ebp");
                list.Add("Esp");
                list.Add("Eip");
                list.Add("EFlags");
                list.Add("SegSs");
            }

            if (HasFlags(ContextFlags.X86ContextExtendedRegisters))
            {
                list.Add("ExtendedRegisters");
            }

            return list;
        }

        public object FindRegisterByName(String name)
        {
            return X86FindRegisterByName(name);
        }

        private object X86FindRegisterByName(String name)
        {
            name = name.ToUpperInvariant();

            if (HasFlags(ContextFlags.X86ContextControl))
            {
                if (name == "EBP") return Marshal.ReadInt32(this.RawPtr, (int)X86Offsets.Ebp);
                if (name == "ESP") return Marshal.ReadInt32(this.RawPtr, (int)X86Offsets.Esp);
                if (name == "EIP") return Marshal.ReadInt32(this.RawPtr, (int)X86Offsets.Eip);
                if (name == "EFLAGS") return Marshal.ReadInt32(this.RawPtr, (int)X86Offsets.EFlags);
                if (name == "SEGSS") return Marshal.ReadInt32(this.RawPtr, (int)X86Offsets.SegSs);
            }
            if (HasFlags(ContextFlags.X86ContextInteger))
            {
                if (name == "EAX") return Marshal.ReadInt32(this.RawPtr, (int)X86Offsets.Eax);
                if (name == "EBX") return Marshal.ReadInt32(this.RawPtr, (int)X86Offsets.Ebx);
                if (name == "ECX") return Marshal.ReadInt32(this.RawPtr, (int)X86Offsets.Ecx);
                if (name == "EDX") return Marshal.ReadInt32(this.RawPtr, (int)X86Offsets.Edx);
                if (name == "ESI") return Marshal.ReadInt32(this.RawPtr, (int)X86Offsets.Esi);
                if (name == "EDI") return Marshal.ReadInt32(this.RawPtr, (int)X86Offsets.Edi);
            }
            if (HasFlags(ContextFlags.X86ContextSegments))
            {
                if (name == "SEGDS") return Marshal.ReadInt32(this.RawPtr, (int)X86Offsets.SegDs);
                if (name == "SEGES") return Marshal.ReadInt32(this.RawPtr, (int)X86Offsets.SegEs);
                if (name == "SEGFS") return Marshal.ReadInt32(this.RawPtr, (int)X86Offsets.SegFs);
                if (name == "SEGGS") return Marshal.ReadInt32(this.RawPtr, (int)X86Offsets.SegGs);
                
            }

            throw new InvalidOperationException(String.Format("Register '{0}' is not in the context", name));
        }

        public void SetRegisterByName(String name, object value)
        {
            X86SetRegisterByName(name, value);
        }

        private void X86SetRegisterByName(String name, object value)
        {
            name = name.ToUpperInvariant();

            if (HasFlags(ContextFlags.X86ContextControl))
            {
                if (name == "EBP")
                {
                    Marshal.WriteInt32(this.RawPtr, (int)X86Offsets.Ebp, (Int32)value);
                    return;
                }
                if (name == "ESP")
                {
                    Marshal.WriteInt32(this.RawPtr, (int)X86Offsets.Esp, (Int32)value);
                    return;
                }
                if (name == "EIP")
                {
                    Marshal.WriteInt32(this.RawPtr, (int)X86Offsets.Eip, (Int32)value);
                    return;
                }
                if (name == "EFLAGS")
                {
                    Marshal.WriteInt32(this.RawPtr, (int)X86Offsets.EFlags, (Int32)value);
                    return;
                }
                if (name == "SEGSS")
                {
                    Marshal.WriteInt32(this.RawPtr, (int)X86Offsets.SegSs, (Int32)value);
                    return;
                }
            }
            if (HasFlags(ContextFlags.X86ContextInteger))
            {
                if (name == "EAX")
                {
                    Marshal.WriteInt32(this.RawPtr, (int)X86Offsets.Eax, (Int32)value);
                    return;
                }
                if (name == "EBX")
                {
                    Marshal.WriteInt32(this.RawPtr, (int)X86Offsets.Ebx, (Int32)value);
                    return;
                }
                if (name == "ECX") 
                {
                    Marshal.WriteInt32(this.RawPtr, (int)X86Offsets.Ecx, (Int32)value);
                    return;
                }
                if (name == "EDX") 
                {
                    Marshal.WriteInt32(this.RawPtr, (int)X86Offsets.Edx, (Int32)value);
                    return;
                }
                if (name == "ESI") 
                {
                    Marshal.WriteInt32(this.RawPtr, (int)X86Offsets.Esi, (Int32)value);
                    return;
                }
                if (name == "EDI") 
                {
                    Marshal.WriteInt32(this.RawPtr, (int)X86Offsets.Edi, (Int32)value);
                    return;
                }
            }
            if (HasFlags(ContextFlags.X86ContextSegments))
            {
                if (name == "SEGDS") 
                {
                    Marshal.WriteInt32(this.RawPtr, (int)X86Offsets.SegDs, (Int32)value);
                    return;
                }
                if (name == "SEGES") 
                {
                    Marshal.WriteInt32(this.RawPtr, (int)X86Offsets.SegEs, (Int32)value);
                    return;
                }
                if (name == "SEGFS") 
                {
                    Marshal.WriteInt32(this.RawPtr, (int)X86Offsets.SegFs, (Int32)value);
                    return;
                }
                if (name == "SEGGS")
                {
                    Marshal.WriteInt32(this.RawPtr, (int)X86Offsets.SegGs, (Int32)value);
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