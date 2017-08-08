using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.X86
{
    public enum RegisterGroup
    {
         REG_EA ,  /* 'normal' reg, qualifies as EA */
         RM_GPR ,   /* integer operand */
         REG_GPR,   /* integer register */
         REG8 ,   /*  8-bit GPR  */
         REG16 ,   /* 16-bit GPR */
         REG32,   /* 32-bit GPR */
         REG64 ,   /* 64-bit GPR */
         FPUREG ,   /* floating point stack registers */
         FPU0 ,   /* FPU stack register zero */
         RM_MMX ,   /* MMX operand */
         MMXREG ,   /* MMX register */
         RM_XMM ,   /* XMM (SSE) operand */
         XMMREG ,   /* XMM (SSE) register */
         XMM0 ,   /* XMM register zero */
         RM_YMM ,   /* YMM (AVX) operand */
         YMMREG ,   /* YMM (AVX) register */
         YMM0 ,   /* YMM register zero */
         REG_CDT ,   /* CRn, DRn and TRn */
         REG_CREG ,   /* CRn */
         REG_DREG ,   /* DRn */
         REG_TREG ,   /* TRn */
         REG_SREG ,   /* any segment register */
         REG_CS ,   /* CS */
         REG_DESS ,   /* DS, ES, SS */
         REG_FSGS,   /* FS, GS */
         REG_SEG67 ,   /* Unimplemented segment registers */

         REG_ACCUM,   /* accumulator: AL, AX, EAX, RAX */
         REG_COUNT,  /* counter: CL, CX, ECX, RCX */
         REG_HIGH, /* high regs: AH, CH, DH, BH */
         REG_NOTACC,  /* non-accumulator register */
         REG8NA, /*  8-bit non-acc GPR  */
         REG16NA,  /* 16-bit non-acc GPR */
         REG32NA,   /* 32-bit non-acc GPR */
         REG64NA,  /* 64-bit non-acc GPR */


         REG_RIP ,   /* RIP relative addressing */
         REG_EIP ,   /* EIP relative addressing */
    }
}
