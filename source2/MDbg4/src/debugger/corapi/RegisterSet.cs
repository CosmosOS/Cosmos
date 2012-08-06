//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Diagnostics;

using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using Microsoft.Samples.Debugging.Native;

namespace Microsoft.Samples.Debugging.CorDebug
{
    // This code needs to be kept in sync with CorDebug.idl
    public enum CorCorDebugRegister
    {
        Eip = 0,                           // REGISTER_X86_EIP = 0,
        Esp,                               // REGISTER_X86_ESP,
        Ebp,                               // REGISTER_X86_EBP,

        Eax,                               // REGISTER_X86_EAX,
        Ecx,                               // REGISTER_X86_ECX,
        Edx,                               // REGISTER_X86_EDX,
        Ebx,                               // REGISTER_X86_EBX,

        Esi,                               // REGISTER_X86_ESI,
        Edi,                               // REGISTER_X86_EDI,

        FPstack0,                          // REGISTER_X86_FPSTACK_0,
        FPstack1,                          // REGISTER_X86_FPSTACK_1,
        FPstack2,                          // REGISTER_X86_FPSTACK_2,
        FPstack3,                          // REGISTER_X86_FPSTACK_3,
        FPstack4,                          // REGISTER_X86_FPSTACK_4,
        FPstack5,                          // REGISTER_X86_FPSTACK_5,
        FPstack6,                          // REGISTER_X86_FPSTACK_6,
        FPstack7,                          // REGISTER_X86_FPSTACK_7,

        RegisterMax // this needs to be last enum!
    };


    public sealed class CorRegisterSet : WrapperBase
    {
        internal CorRegisterSet(ICorDebugRegisterSet registerSet)
            : base(registerSet)
        {
            m_rs = registerSet;
        }

        [CLSCompliant(false)]
        public ICorDebugRegisterSet Raw
        {
            get 
            { 
                return m_rs;
            }
        }

        /*
         * GetRegistersAvailable returns a mask indicating which registers
         * are available in the given register set.  Registers may be unavailable
         * if their value is undeterminable for the given situation.  The returned
         * word contains a bit for each register (1 << register index), which will
         * be 1 if the register is available or 0 if it is not.
         */
        //HRESULT GetRegistersAvailable([out] ULONG64 *pAvailable);

        [CLSCompliant(false)]
        public UInt64 RegistersAvailable
        {
            get
            {
                UInt64 availableReg;
                m_rs.GetRegistersAvailable(out availableReg);
                return availableReg;
            }
        }

        /* 
         * GetRegisters returns an array of register values corresponding
         * to the given mask.  The registers which have their bit set in
         * the mask will be packed into the resulting array.  (No room is
         * assigned in the array for registers whose mask bit is not set.)
         * Thus, the size of the array should be equal to the number of
         * 1's in the mask.
         *
         * If an unavailable register is indicated by the mask, an indeterminate
         * value will be returned for the corresponding register.
         *
         * registerBufferCount should indicate number of elements in the
         * buffer to receive the register values.  If it is too small for
         * the number of registers indicated by the mask, the higher
         * numbered registers will be truncated from the set.  Or, if it
         * is too large, the unused registerBuffer elements will be
         * unmodified.  */

        //HRESULT GetRegisters([in] ULONG64 mask, [in] ULONG32 regCount,
        //                [out, size_is(regCount), length_is(regCount)] 
        //                 CORDB_REGISTER regBuffer[]);
        [CLSCompliant(false)]
        public UInt64[] GetRegisters(UInt64 mask)
        {
            int regsToGet = 0;
            for (UInt64 m = mask; m != 0; m >>= 1)
                if ((m & 1) != 0)
                    regsToGet++;

            UInt64[] regs = new UInt64[regsToGet];
            m_rs.GetRegisters(mask, (uint)regs.Length, regs);
            return regs;
        }

        [CLSCompliant(false)]
        public UInt64 GetRegister(CorCorDebugRegister register)
        {
            UInt64 mask = 1;
            mask <<= (int)register;
            UInt64[] regs = GetRegisters(mask);
            Debug.Assert(regs != null && regs.Length == 1);
            return regs[0];
        }

        /* 
         * SetRegisters sets the value of the set registers corresponding
         * to the given mask.  For each bit set in the mask, the
         * corresponding register will be set from the corresponding
         * element in the registerBuffer. (Note that the correlation is by
         * sequence, not by the position of the bit.  That is,
         * registerBuffer is "packed"; there are no elements corresponding
         * to registers whose bit is not set.
         *
         * If an unavailable register is indicated by the mask, the
         * register will not be set (although a value for that register is
         * recognized from the registerBuffer.)
         *
         * registerBufferCount should indicate number of elements in the
         * buffer to be the register values.  If it is too small for the
         * number of registers indicated by the mask, the higher numbered
         * registers will not be set.  If it is too large, the extra
         * values will be ignored.
         *
         * Note that setting registers this way is inherently dangerous.
         * CorDebug makes no attempt to ensure that the runtime remains in
         * a valid state when register values are changed. (For example,
         * if the IP were set to point to non-managed code, who knows what
         * would happen.)  
         *
         * Not Implemented In-Proc
         */

        //     HRESULT SetRegisters([in] ULONG64 mask, 
        //                          [in] ULONG32 regCount, 
        //                          [in, size_is(regCount)] CORDB_REGISTER regBuffer[]);
        [CLSCompliant(false)]
        public void SetRegisters(UInt64 mask, UInt64[] registerValues)
        {
            m_rs.SetRegisters(mask, (uint)registerValues.Length, registerValues);
        }

        [CLSCompliant(false)]
        public void SetRegister(CorCorDebugRegister register, UInt64 value)
        {
            UInt64 mask = 1;
            mask <<= (int)register;
            SetRegisters(mask, new UInt64[] { value });
        }
   
        public void GetThreadContext(IntPtr contextPtr, int contextSize)
        {
            m_rs.GetThreadContext((uint)contextSize, contextPtr);
        }

        /// <summary>
        /// Helper to get a self-descibing wrapper around a native OS context for the register set.
        /// Throws if unavailable or if NativeContextAllocator is not set.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] // Too expensive to eagerly browse in debugger.
        public INativeContext GetContext
        {
            get
            {
                // Note:  We're using the non-Wow mode GenerateContext() since we don't have the PID
                INativeContext c = ContextAllocator.GenerateContext();

                using (IContextDirectAccessor w = c.OpenForDirectAccess())
                {
                    GetThreadContext(w.RawBuffer, w.Size);
                }
                return c;
            }
        }
        public void SetThreadContext(IntPtr contextPtr, int contextSize)
        {
            m_rs.SetThreadContext((uint)contextSize, contextPtr);
        }

        private ICorDebugRegisterSet m_rs;
    }
}


