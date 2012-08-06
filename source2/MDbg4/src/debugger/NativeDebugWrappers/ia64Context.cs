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
    public class IA64Context : INativeContext, IEquatable<INativeContext>, IDisposable
    {
        private int m_size;
        private Platform m_platform;
        private int m_imageFileMachine;
        private IntPtr m_rawPtr;

        ~IA64Context()
        {
            this.Dispose(false);
        }

        // This is a private contstructor for cloning Context objects.  It should not be used outside of this class.
        private IA64Context(INativeContext ctx)
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

        public IA64Context()
            : this(AgnosticContextFlags.ContextControl | AgnosticContextFlags.ContextInteger)
        {
        }

        public IA64Context(AgnosticContextFlags aFlags)
        {
            InitializeContext();
            WriteContextFlagsToBuffer(GetPSFlags(aFlags));
        }

        public IA64Context(ContextFlags flags)
        {
            InitializeContext();
            WriteContextFlagsToBuffer(flags); 
        }

        private void InitializeContext()
        {
            this.m_size = (int)ContextSize.IA64;
            this.m_platform = Platform.IA64;
            this.m_imageFileMachine = (int) Native.ImageFileMachine.IA64;
            this.m_rawPtr = Marshal.AllocHGlobal(this.Size);

            return;
        }

        private void WriteContextFlagsToBuffer(ContextFlags flags)
        {
            Debug.Assert(this.RawPtr != IntPtr.Zero);
            Marshal.WriteInt32(this.RawPtr, (int)IA64Offsets.ContextFlags, (Int32)flags);
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
            // We know that we need an ia64 context
            this.m_platform = Platform.IA64;
            ContextFlags cFlags = ContextFlags.IA64Context;
           
            if ((flags & AgnosticContextFlags.ContextInteger) == AgnosticContextFlags.ContextInteger)
            {
                // ContextInteger is the same for all platforms, so we can do a blanket |=
                cFlags |= (ContextFlags)AgnosticContextFlags.ContextInteger;
            }

            if ((flags & AgnosticContextFlags.ContextControl) == AgnosticContextFlags.ContextControl)
            {
                // IA64 has a different flag for ContextControl
                cFlags |= ContextFlags.IA64ContextControl;
            }

            if ((flags & AgnosticContextFlags.ContextFloatingPoint) == AgnosticContextFlags.ContextFloatingPoint)
            {
                // IA64 has a different flag for ContextFloatingPoint
                cFlags |= ContextFlags.IA64ContextFloatingPoint;
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
            return Platform.IA64;
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
                return (ContextFlags)Marshal.ReadInt32(this.RawPtr, (int)IA64Offsets.ContextFlags);
            }
            set
            {
                if ((value & ContextFlags.IA64Context) == 0)
                {
                    throw new ArgumentException("Process architecture flag must be set");
                }
                if ((value & ~ContextFlags.IA64ContextAll) != 0)
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

                return (IntPtr)Marshal.ReadInt64(this.m_rawPtr, (int)IA64Offsets.IntSp);
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
                    Int64 iip = Marshal.ReadInt64(w.RawBuffer, (int)IA64Offsets.StIIP);

                    Int64 StIPSR = Marshal.ReadInt64(w.RawBuffer, (int)IA64Offsets.StIPSR);
                    int PSR_RI = (int)IA64Flags.PSR_RI;
                    Int64 slot = (StIPSR >> PSR_RI) & 3;
                    iip |= slot << 2;

                    ip = (IntPtr)iip;
                }
                return ip;
            }
            set
            {
                using (IContextDirectAccessor w = this.OpenForDirectAccess())
                {
                    UInt64 ipsr = 0;
                    UInt64 iip = (UInt64)value.ToInt64();

                    // slot = iip & (IA64_BUNDLE_SIZE - 1) >> 2
                    UInt64 slot = ((UInt64)iip) & ((int)IA64Flags.IA64_BUNDLE_SIZE - 1) >> 2;

                    // StIIP = iip & ~(IA64_BUNDLE_SIZE - 1)
                    Marshal.WriteIntPtr(w.RawBuffer, (int)IA64Offsets.StIIP, (IntPtr)(iip & ~((UInt64)((int)IA64Flags.IA64_BUNDLE_SIZE - 1))));

                    // StIPSR &= ~(0x3 << PSR_RI)
                    ipsr = (UInt64)Marshal.ReadIntPtr(w.RawBuffer, (int)IA64Offsets.StIPSR) & ~(((UInt64)0x3) << (int)IA64Flags.PSR_RI);
                    Marshal.WriteIntPtr(w.RawBuffer, (int)IA64Offsets.StIPSR, (IntPtr)ipsr);

                    // StIPSR |= (slot << PSR_RI)
                    ipsr = (UInt64)Marshal.ReadIntPtr(w.RawBuffer, (int)IA64Offsets.StIPSR) | (slot << (int)IA64Flags.PSR_RI);
                    Marshal.WriteIntPtr(w.RawBuffer, (int)IA64Offsets.StIPSR, (IntPtr)ipsr);
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
                Int64 stipsr = Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.StIPSR) | (Int64)IA64Flags.SINGLE_STEP_FLAG;
                Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.StIPSR, stipsr);
            }
            else
            {
                Int64 stipsr = Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.StIPSR) & ~((Int64)IA64Flags.SINGLE_STEP_FLAG);
                Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.StIPSR, stipsr);
            }
        }

        public bool IsSingleStepFlagEnabled
        {
            get
            {
                return ((Marshal.ReadInt32(this.RawPtr, (int)IA64Offsets.StIPSR) & (long)IA64Flags.SINGLE_STEP_FLAG) != 0);
            }
        }

        public INativeContext Clone()
        {
            return new IA64Context(this);
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

            return CheckContexts(this, (IA64Context)other);
        }

        public bool CheckContexts(INativeContext a1, INativeContext a2)
        {
            return CheckIA64Contexts((IA64Context)a1, (IA64Context)a2);
        }

        // compare a chuck of the context record contained between the two offsets
        private bool CheckContextChunk(IA64Context a1, IA64Context a2, int startOffset, int endOffset)
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

        private bool CheckIA64Contexts(IA64Context a1, IA64Context a2)
        {
            // CONTEXT_CONTROL contains: ApUNAT through rest of CONTEXT
            if ((a1.Flags & ContextFlags.IA64ContextControl) == ContextFlags.IA64ContextControl)
            {
                if (!CheckContextChunk(a1, a2, (int)IA64Offsets.ApUNAT, (int)IA64Offsets.UNUSEDPACK))
                {
                    return false;
                }
            }

            // CONTEXT_CONTROL contains: IntGp through ApUNAT
            if ((a1.Flags & ContextFlags.IA64ContextInteger) == ContextFlags.IA64ContextInteger)
            {
                if (!CheckContextChunk(a1, a2, (int)IA64Offsets.IntGp, (int)IA64Offsets.ApUNAT))
                {
                    return false;
                }
            }

            // CONTEXT_DEBUG contains: DbI0 through FltS0
            if ((a1.Flags & ContextFlags.IA64ContextDebug) == ContextFlags.IA64ContextDebug)
            {
                if (!CheckContextChunk(a1, a2, (int)IA64Offsets.DbI0, (int)IA64Offsets.FltS0))
                {
                    return false;
                }
            }

            // CONTEXT_FLOATING_POINT contains: FltS0 through StFPSR
            if ((a1.Flags & ContextFlags.IA64ContextFloatingPoint) == ContextFlags.IA64ContextFloatingPoint)
            {
                if (!CheckContextChunk(a1, a2, (int)IA64Offsets.FltS0, (int)IA64Offsets.StFPSR))
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
            return EnumerateIA64Registers();
        }

        private IEnumerable<String> EnumerateIA64Registers()
        {
            List<String> list = new List<String>();

            // This includes the most commonly used flags. 
            if (HasFlags(ContextFlags.IA64ContextControl))
            {
                list.Add("ApUNAT");
                list.Add("ApLC");
                list.Add("ApEC");
                list.Add("ApCCV");
                list.Add("ApDCR");
                list.Add("RsPFS");
                list.Add("RsBSP");
                list.Add("RsBSPSTORE");
                list.Add("RsRSC");
                list.Add("RsRNAT");
                list.Add("StIPSR");
                list.Add("StIIP");
                list.Add("StIFS");
                list.Add("StFCR");
                list.Add("EFLAGS");
                list.Add("SegCSD");
                list.Add("SegSSD");
                list.Add("Cflag");
                list.Add("StFSR");
                list.Add("StFIR");
                list.Add("StFDR");
            }
            if (HasFlags(ContextFlags.IA64ContextInteger))
            {
                list.Add("IntGp");
                list.Add("IntT0");
                list.Add("IntT1");
                list.Add("IntS0");
                list.Add("IntS1");
                list.Add("IntS2");
                list.Add("IntS3");
                list.Add("IntV0");
                list.Add("IntT2");
                list.Add("IntT3");
                list.Add("IntT4");
                list.Add("IntSp");
                list.Add("IntTeb");
                list.Add("IntT5");
                list.Add("IntT6");
                list.Add("IntT7");
                list.Add("IntT8");
                list.Add("IntT9");
                list.Add("IntT10");
                list.Add("IntT11");
                list.Add("IntT12");
                list.Add("IntT13");
                list.Add("IntT14");
                list.Add("IntT15");
                list.Add("IntT16");
                list.Add("IntT17");
                list.Add("IntT18");
                list.Add("IntT19");
                list.Add("IntT20");
                list.Add("IntT21");
                list.Add("IntT22");
                list.Add("IntNats");
                list.Add("Preds");
                list.Add("BrRp");
                list.Add("BrS0");
                list.Add("BrS1");
                list.Add("BrS2");
                list.Add("BrS3");
                list.Add("BrS4");
                list.Add("BrT0");
                list.Add("BrT1");
            }

            if (HasFlags(ContextFlags.IA64ContextDebug))
            {
                list.Add("DbI0");
                list.Add("DbI1");
                list.Add("DbI2");
                list.Add("DbI3");
                list.Add("DbI4");
                list.Add("DbI5");
                list.Add("DbI6");
                list.Add("DbI7");
                list.Add("DbD0");
                list.Add("DbD1");
                list.Add("DbD2");
                list.Add("DbD3");
                list.Add("DbD4");
                list.Add("DbD5");
                list.Add("DbD6");
                list.Add("DbD7");
            }

            return list;
        }

        public object FindRegisterByName(String name)
        {
            return IA64FindRegisterByName(name);
        }

        private object IA64FindRegisterByName(String name)
        {
            name = name.ToUpperInvariant();

            if (HasFlags(ContextFlags.IA64ContextControl))
            {
                if (name == "APUNAT") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.ApUNAT);
                if (name == "APLC") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.ApLC);
                if (name == "APEC") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.ApEC);
                if (name == "APCCV") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.ApCCV);
                if (name == "APDCR") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.ApDCR);
                if (name == "RSPFS") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.RsPFS);
                if (name == "RSBSP") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.RsBSP);
                if (name == "RSBSPSTORE") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.RsBSPSTORE);
                if (name == "RSRSC") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.RsRSC);
                if (name == "RSRNAT") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.RsRNAT);
                if (name == "STIPSR") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.StIPSR);
                if (name == "STIIP") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.StIIP);
                if (name == "STIFS") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.StIFS);
                if (name == "STFCR") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.StFCR);
                if (name == "EFLAG") return Marshal.ReadInt32(this.RawPtr, (int)IA64Offsets.Eflag);
                if (name == "SEGCSD") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.SegCSD);
                if (name == "SEGSSD") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.SegSSD);
                if (name == "CFLAG") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.Cflag);
                if (name == "STFSR") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.StFSR);
                if (name == "STFIR") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.StFIR);
                if (name == "STFDR") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.StFDR);
            }
            if (HasFlags(ContextFlags.IA64ContextInteger))
            {
                if (name == "INTGP") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntGp);
                if (name == "INTT0") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT0);
                if (name == "INTT1") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT1);
                if (name == "INTS0") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntS0);
                if (name == "INTS1") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntS1);
                if (name == "INTS2") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntS2);
                if (name == "INTS3") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntS3);
                if (name == "INTV0") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntV0);
                if (name == "INTT2") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT2);
                if (name == "INTT3") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT3);
                if (name == "INTT4") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT4);
                if (name == "INTSP") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntSp);
                if (name == "INTTEB") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntTeb);
                if (name == "INTT5") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT5);
                if (name == "INTT6") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT6);
                if (name == "INTT7") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT7);
                if (name == "INTT8") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT8);
                if (name == "INTT9") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT9);
                if (name == "INTT10") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT10);
                if (name == "INTT11") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT11);
                if (name == "INTT12") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT11);
                if (name == "INTT13") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT13);
                if (name == "INTT14") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT14);
                if (name == "INTT15") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT15);
                if (name == "INTT16") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT16);
                if (name == "INTT17") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT17);
                if (name == "INTT18") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT18);
                if (name == "INTT19") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT19);
                if (name == "INTT20") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT20);
                if (name == "INTT21") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT21);
                if (name == "INTT22") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntT22);
                if (name == "INTNATS") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.IntNats);
                if (name == "PREDS") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.Preds);
                if (name == "BRRp") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.BrRp);
                if (name == "BRS0") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.BrS0);
                if (name == "BRS1") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.BrS1);
                if (name == "BRS2") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.BrS2);
                if (name == "BRS3") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.BrS3);
                if (name == "BRS4") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.BrS4);
                if (name == "BRT0") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.BrT0);
                if (name == "BRT1") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.BrT1);

            }
            if (HasFlags(ContextFlags.IA64ContextDebug))
            {
                if (name == "DBI0") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.DbI0);
                if (name == "DBI1") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.DbI1);
                if (name == "DBI2") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.DbI2);
                if (name == "DBI3") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.DbI3);
                if (name == "DBI4") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.DbI4);
                if (name == "DBI5") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.DbI5);
                if (name == "DBI6") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.DbI6);
                if (name == "DBI7") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.DbI7);
                if (name == "DBD0") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.DbD0);
                if (name == "DBD1") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.DbD1);
                if (name == "DBD2") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.DbD2);
                if (name == "DBD3") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.DbD3);
                if (name == "DBD4") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.DbD4);
                if (name == "DBD5") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.DbD5);
                if (name == "DBD6") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.DbD6);
                if (name == "DBD7") return Marshal.ReadInt64(this.RawPtr, (int)IA64Offsets.DbD7);
            }

            throw new InvalidOperationException(String.Format("Register '{0}' is not in the context", name));
        }

        public void SetRegisterByName(String name, object value)
        {
            IA64SetRegisterByName(name, value);
        }

        private void IA64SetRegisterByName(String name, object value)
        {
            name = name.ToUpperInvariant();

            if (HasFlags(ContextFlags.IA64ContextControl))
            {
                if (name == "APUNAT") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.ApUNAT, (Int64)value);
                    return;
                }
                if (name == "APLC") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.ApLC, (Int64)value);
                    return;
                }
                if (name == "APEC") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.ApEC, (Int64)value);
                    return;
                }
                if (name == "APCCV") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.ApCCV, (Int64)value);
                    return;
                }
                if (name == "APDCR") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.ApDCR, (Int64)value);
                    return;
                }
                if (name == "RSPFS") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.RsPFS, (Int64)value);
                    return;
                }
                if (name == "RSBSP") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.RsBSP, (Int64)value);
                    return;
                }
                if (name == "RSBSPSTORE")
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.RsBSPSTORE, (Int64)value);
                    return;
                }
                if (name == "RSRSC") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.RsRSC, (Int64)value);
                    return;
                }
                if (name == "RSRNAT") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.RsRNAT, (Int64)value);
                    return;
                }
                if (name == "STIPSR") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.StIPSR, (Int64)value);
                    return;
                }
                if (name == "STIIP") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.StIIP, (Int64)value);
                    return;
                }
                if (name == "STIFS") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.StIFS, (Int64)value);
                    return;
                }
                if (name == "STFCR") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.StFCR, (Int64)value);
                    return;
                }
                if (name == "EFLAG") 
                {
                    Marshal.WriteInt32(this.RawPtr, (int)IA64Offsets.Eflag, (Int32)value);
                    return;
                }
                if (name == "SEGCSD") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.SegCSD, (Int64)value);
                    return;
                }
                if (name == "SEGSSD") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.SegSSD, (Int64)value);
                    return;
                }
                if (name == "CFLAG") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.Cflag, (Int64)value);
                    return;
                }
                if (name == "STFSR") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.StFSR, (Int64)value);
                    return;
                }
                if (name == "STFIR") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.StFIR, (Int64)value);
                    return;
                }
                if (name == "STFDR") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.StFDR, (Int64)value);
                    return;
                }
            }
            if (HasFlags(ContextFlags.IA64ContextInteger))
            {
                if (name == "INTGP") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntGp, (Int64)value);
                    return;
                }
                if (name == "INTT0") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT0, (Int64)value);
                    return;
                }
                if (name == "INTT1") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT1, (Int64)value);
                    return;
                }
                if (name == "INTS0") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntS0, (Int64)value);
                    return;
                }
                if (name == "INTS1") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntS1, (Int64)value);
                    return;
                }
                if (name == "INTS2") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntS2, (Int64)value);
                    return;
                }
                if (name == "INTS3") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntS3, (Int64)value);
                    return;
                }
                if (name == "INTV0") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntV0, (Int64)value);
                    return;
                }
                if (name == "INTT2") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT2, (Int64)value);
                    return;
                }
                if (name == "INTT3") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT3, (Int64)value);
                    return;
                }
                if (name == "INTT4") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT4, (Int64)value);
                    return;
                }
                if (name == "INTSP") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntSp, (Int64)value);
                    return;
                }
                if (name == "INTTEB") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntTeb, (Int64)value);
                    return;
                }
                if (name == "INTT5") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT5, (Int64)value);
                    return;
                }
                if (name == "INTT6") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT6, (Int64)value);
                    return;
                }
                if (name == "INTT7") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT7, (Int64)value);
                    return;
                }
                if (name == "INTT8") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT8, (Int64)value);
                    return;
                }
                if (name == "INTT9") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT9, (Int64)value);
                    return;
                }
                if (name == "INTT10") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT10, (Int64)value);
                    return;
                }
                if (name == "INTT11") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT11, (Int64)value);
                    return;
                }
                if (name == "INTT12") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT11, (Int64)value);
                    return;
                }
                if (name == "INTT13") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT13, (Int64)value);
                    return;
                }
                if (name == "INTT14") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT14, (Int64)value);
                    return;
                }
                if (name == "INTT15") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT15, (Int64)value);
                    return;
                }
                if (name == "INTT16") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT16, (Int64)value);
                    return;
                }
                if (name == "INTT17") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT17, (Int64)value);
                    return;
                }
                if (name == "INTT18") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT18, (Int64)value);
                    return;
                }
                if (name == "INTT19") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT19, (Int64)value);
                    return;
                }
                if (name == "INTT20") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT20, (Int64)value);
                    return;
                }
                if (name == "INTT21") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT21, (Int64)value);
                    return;
                }
                if (name == "INTT22") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntT22, (Int64)value);
                    return;
                }
                if (name == "INTNATS") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.IntNats, (Int64)value);
                    return;
                }
                if (name == "PREDS") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.Preds, (Int64)value);
                    return;
                }
                if (name == "BRRp") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.BrRp, (Int64)value);
                    return;
                }
                if (name == "BRS0") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.BrS0, (Int64)value);
                    return;
                }
                if (name == "BRS1") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.BrS1, (Int64)value);
                    return;
                }
                if (name == "BRS2") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.BrS2, (Int64)value);
                    return;
                }
                if (name == "BRS3") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.BrS3, (Int64)value);
                    return;
                }
                if (name == "BRS4") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.BrS4, (Int64)value);
                    return;
                }
                if (name == "BRT0") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.BrT0, (Int64)value);
                    return;
                }
                if (name == "BRT1") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.BrT1, (Int64)value);
                    return;
                }
            }
            if (HasFlags(ContextFlags.IA64ContextDebug))
            {
                if (name == "DBI0")
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.DbI0, (Int64)value);
                    return;
                }
                if (name == "DBI1") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.DbI1, (Int64)value);
                    return;
                }
                if (name == "DBI2") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.DbI2, (Int64)value);
                    return;
                }
                if (name == "DBI3")
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.DbI3, (Int64)value);
                    return;
                }
                if (name == "DBI4") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.DbI4, (Int64)value);
                    return;
                }
                if (name == "DBI5") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.DbI5, (Int64)value);
                    return;
                }
                if (name == "DBI6") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.DbI6, (Int64)value);
                    return;
                }
                if (name == "DBI7") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.DbI7, (Int64)value);
                    return;
                }
                if (name == "DBD0") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.DbD0, (Int64)value);
                    return;
                }
                if (name == "DBD1") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.DbD1, (Int64)value);
                    return;
                }
                if (name == "DBD2") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.DbD2, (Int64)value);
                    return;
                }
                if (name == "DBD3") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.DbD3, (Int64)value);
                    return;
                }
                if (name == "DBD4") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.DbD4, (Int64)value);
                    return;
                }
                if (name == "DBD5") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.DbD5, (Int64)value);
                    return;
                }
                if (name == "DBD6") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.DbD6, (Int64)value);
                    return;
                }
                if (name == "DBD7") 
                {
                    Marshal.WriteInt64(this.RawPtr, (int)IA64Offsets.DbD7, (Int64)value);
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
