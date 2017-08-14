using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Assembler.X86;

using OCE = Cosmos.Assembler.X86.InstructionEnum;
using OF = Cosmos.Assembler.X86.Generator.OperandFlag;
using Cosmos.Assembler;
namespace Cosmos.Assembler.X86.Generator
{
    public enum OperandFlag
    {
        IF_SM     , /* size match */
        IF_SM2    , /* size match first two operands */
        IF_SB     , /* unsized operands can't be non-byte */
        IF_SW     , /* unsized operands can't be non-word */
        IF_SD     , /* unsized operands can't be non-dword */
        IF_SQ     , /* unsized operands can't be non-qword */
        IF_SO     , /* unsized operands can't be non-oword */
        IF_SY     , /* unsized operands can't be non-yword */
        IF_SZ     , /* unsized operands must match the bitsize */
        IF_SMASK  , /* mask for unsized argument size */
        IF_AR0    , /* SB, SW, SD applies to argument 0 */
        IF_AR1    , /* SB, SW, SD applies to argument 1 */
        IF_AR2    , /* SB, SW, SD applies to argument 2 */
        IF_AR3    , /* SB, SW, SD applies to argument 3 */
        IF_ARMASK , /* mask for unsized argument spec */
        IF_ARSHFT , /* LSB in IF_ARMASK */
        IF_PRIV   , /* it's a privileged instruction */
        IF_SMM    , /* it's only valid in SMM */
        IF_PROT   , /* it's protected mode only */
        IF_NOLONG , /* it's not available in long mode */
        IF_UNDOC  , /* it's an undocumented instruction */
        IF_FPU    , /* it's an FPU instruction */
        IF_MMX    , /* it's an MMX instruction */
        IF_3DNOW  , /* it's a 3DNow! instruction */
        IF_SSE    , /* it's a SSE (KNI, MMX2) instruction */
        IF_SSE2   , /* it's a SSE2 instruction */
        IF_SSE3   , /* it's a SSE3 (PNI) instruction */
        IF_VMX    , /* it's a VMX instruction */
        IF_LONG   , /* long mode instruction */
        IF_SSSE3  , /* it's an SSSE3 instruction */
        IF_SSE4A  , /* AMD SSE4a */
        IF_SSE41  , /* it's an SSE4.1 instruction */
        IF_SSE42  , /* HACK NEED TO REORGANIZE THESE BITS */
        IF_SSE5    , /* HACK NEED TO REORGANIZE THESE BITS */
        IF_AVX    , /* HACK NEED TO REORGANIZE THESE BITS */
        IF_FMA    , /* HACK NEED TO REORGANIZE THESE BITS */

        IF_8086      , /* 8086 instruction */
        IF_186       , /* 186+ instruction */
        IF_286         , /* 286+ instruction */
        IF_386        , /* 386+ instruction */
        IF_486        , /* 486+ instruction */
        IF_PENT        , /* Pentium instruction */
        IF_P6          , /* P6 instruction */
        IF_KATMAI      , /* Katmai instructions */
        IF_WILLAMETTE      , /* Willamette instructions */
        IF_PRESCOTT         , /* Prescott instructions */
        IF_X86_64  ,    	/* x86-64 instruction (long or legacy mode) */
        IF_NEHALEM      , /* Nehalem instruction */
        IF_WESTMERE      , /* Westmere instruction */
        IF_SANDYBRIDGE  , /* Sandy Bridge instruction */
        IF_FUTURE,       	/* Future processor (not yet disclosed) */
        IF_X64,          
        IF_IA64         , /* IA64 instructions (in x86 mode) */
        IF_CYRIX         , /* Cyrix-specific instruction */
        IF_AMD          , /* AMD-specific instruction */
        NONE,
}
    public class OpCodeData
    {
        /* Size, and other attributes, of the operand */
        public const ulong BITS8           = 0x00000001U;
        public const ulong BITS16 = 0x00000002U;
        public const ulong BITS32 = 0x00000004U;
        public const ulong BITS64 = 0x00000008U;   /* x64 and FPU only */
        public const ulong BITS80 = 0x00000010U;   /* FPU only */
        public const ulong BITS128 = 0x20000000U;
        public const ulong BITS256 = 0x00800000U;
        public const ulong FAR = 0x00000020U;   /* grotty: this means 16:16 or */
        /* 16:32, like in CALL/JMP */
        public const ulong NEAR = 0x00000040U;
        public const ulong SHORT = 0x00000080U;   /* and this means what it says :) */

        public const ulong SIZE_MASK = 0x208000FFU;   /* all the size attributes */

        /* Modifiers */
        public const ulong MODIFIER_MASK = 0x00000f00U;
        public const ulong TO = 0x00000100U;   /* reverse effect in FADD, FSUB &c */
        public const ulong COLON = 0x00000200U;   /* operand is followed by a colon */
        public const ulong STRICT = 0x00000400U;   /* do not optimize this operand */

        /* Type of operand: memory reference, register, etc. */
        public const ulong OPTYPE_MASK = 0x0000f000U;
        public const ulong REGISTER = 0x00001000U;   /* register number in 'basereg' */
        public const ulong IMMEDIATE = 0x00002000U;
        public const ulong MEMORY = 0x0000c000U;
        public const ulong REGMEM = 0x00008000U;   /* for r/m, ie EA, operands */

        /* Register classes */
        public const ulong REG_EA = 0x00009000U;  /* 'normal' reg, qualifies as EA */
        public const ulong RM_GPR = 0x00208000U;   /* integer operand */
        public const ulong REG_GPR = 0x00209000U;   /* integer register */
        public const ulong REG8 = 0x00209001U;   /*  8-bit GPR  */
        public const ulong REG16 = 0x00209002U;   /* 16-bit GPR */
        public const ulong REG32 = 0x00209004U;   /* 32-bit GPR */
        public const ulong REG64 = 0x00209008U;   /* 64-bit GPR */
        public const ulong FPUREG = 0x01001000U;   /* floating point stack registers */
        public const ulong FPU0 = 0x01011000U;   /* FPU stack register zero */
        public const ulong RM_MMX = 0x02008000U;   /* MMX operand */
        public const ulong MMXREG = 0x02009000U;   /* MMX register */
        public const ulong RM_XMM = 0x04008000U;   /* XMM (SSE) operand */
        public const ulong XMMREG = 0x04009000U;   /* XMM (SSE) register */
        public const ulong XMM0 = 0x04019000U;   /* XMM register zero */
        public const ulong RM_YMM = 0x08008000U;   /* YMM (AVX) operand */
        public const ulong YMMREG = 0x08009000U;   /* YMM (AVX) register */
        public const ulong YMM0 = 0x08019000U;   /* YMM register zero */
        public const ulong REG_CDT = 0x00101004U;   /* CRn, DRn and TRn */
        public const ulong REG_CREG = 0x00111004U;   /* CRn */
        public const ulong REG_DREG = 0x00121004U;   /* DRn */
        public const ulong REG_TREG = 0x00141004U;   /* TRn */
        public const ulong REG_SREG = 0x00401002U;   /* any segment register */
        public const ulong REG_CS = 0x00411002U;   /* CS */
        public const ulong REG_DESS = 0x00421002U;   /* DS, ES, SS */
        public const ulong REG_FSGS = 0x00441002U;   /* FS, GS */
        public const ulong REG_SEG67 = 0x00481002U;   /* Unimplemented segment registers */

        public const ulong REG_RIP = 0x00801008U;   /* RIP relative addressing */
        public const ulong REG_EIP = 0x00801004U;   /* EIP relative addressing */

        /* Special GPRs */
        public const ulong REG_SMASK = 0x100f0000U;   /* a mask for the following */
        public const ulong REG_ACCUM = 0x00219000U;   /* accumulator: AL, AX, EAX, RAX */
        public const ulong REG_AL = 0x00219001U;
        public const ulong REG_AX = 0x00219002U;
        public const ulong REG_EAX = 0x00219004U;
        public const ulong REG_RAX = 0x00219008U;
        public const ulong REG_COUNT = 0x10229000U;  /* counter: CL, CX, ECX, RCX */
        public const ulong REG_CL = 0x10229001U;
        public const ulong REG_CX = 0x10229002U;
        public const ulong REG_ECX = 0x10229004U;
        public const ulong REG_RCX = 0x10229008U;
        public const ulong REG_DL = 0x10249001U;  /* data: DL, DX, EDX, RDX */
        public const ulong REG_DX = 0x10249002U;
        public const ulong REG_EDX = 0x10249004U;
        public const ulong REG_RDX = 0x10249008U;
        public const ulong REG_HIGH = 0x10289001U; /* high regs: AH, CH, DH, BH */
        public const ulong REG_NOTACC = 0x10000000U;  /* non-accumulator register */
        public const ulong REG8NA = 0x10209001U; /*  8-bit non-acc GPR  */
        public const ulong REG16NA = 0x10209002U;  /* 16-bit non-acc GPR */
        public const ulong REG32NA = 0x10209004U;   /* 32-bit non-acc GPR */
        public const ulong REG64NA = 0x10209008U;  /* 64-bit non-acc GPR */

        /* special types of EAs */
        public const ulong MEM_OFFS = 0x0001c000U; /* simple [address] offset - absolute! */
        public const ulong IP_REL = 0x0002c000U;   /* IP-relative offset */

        /* memory which matches any type of r/m operand */
        public const ulong MEMORY_ANY = ( MEMORY | RM_GPR | RM_MMX | RM_XMM | RM_YMM );

        /* special type of immediate operand */
        public const ulong UNITY = 0x00012000U; /* for shift/rotate instructions */
        public const ulong SBYTE16 = 0x00022000U; /* for op r16,immediate instrs. */
        public const ulong SBYTE32 = 0x00042000U;  /* for op r32,immediate instrs. */
        public const ulong SBYTE64 = 0x00082000U;  /* for op r64,immediate instrs. */
        public const ulong BYTENESS = 0x000E0000U;  /* for testing for byteness */

        public const ulong SAME_AS = 0x40000000U;
        public struct itemplate
        {
            public OCE opcode;		/* the token, passed from "parser.c" */
            public int operands;		/* number of operands */
            public ulong[] opd; /* bit flags for operand types */
            public int code;	/* the code it assembles to */
            public OperandFlag flag1;		/* some flags */
            public OperandFlag flag2;		/* some flags */
            public OperandFlag flag3;		/* some flags */
            public OperandFlag flag4;
            public OperandFlag flag5;

            public itemplate( OCE aOpCode, int aOperandCount, ulong[] aOperands, int OpCodeAdress, OperandFlag aFlags )
            {
                opcode = aOpCode;
                operands = aOperandCount;
                opd = aOperands;
                code = OpCodeAdress;
                flag1 = aFlags;
                flag2 = OF.NONE;
                flag3 = OF.NONE;
                flag4 = OF.NONE;
                flag5 = OF.NONE;
            }
            public itemplate( OCE aOpCode, int aOperandCount, ulong[] aOperands, int OpCodeAdress, OperandFlag aFlag1, OperandFlag aFlag2 )
            {
                opcode = aOpCode;
                operands = aOperandCount;
                opd = aOperands;
                code = OpCodeAdress;
                flag1 = aFlag1;
                flag2 = aFlag2;
                flag3 = OF.NONE;
                flag4 = OF.NONE;
                flag5 = OF.NONE;
            }
            public itemplate( OCE aOpCode, int aOperandCount, ulong[] aOperands, int OpCodeAdress, OperandFlag aFlag1, OperandFlag aFlag2, OperandFlag aFlag3 )
            {
                opcode = aOpCode;
                operands = aOperandCount;
                opd = aOperands;
                code = OpCodeAdress;
                flag1 = aFlag1;
                flag2 = aFlag2;
                flag3 = aFlag3;
                flag4 = OF.NONE;
                flag5 = OF.NONE;
            }
            public itemplate( OCE aOpCode, int aOperandCount, ulong[] aOperands, int OpCodeAdress, OperandFlag aFlag1, OperandFlag aFlag2, OperandFlag aFlag3, OperandFlag aFlag4 )
            {
                opcode = aOpCode;
                operands = aOperandCount;
                opd = aOperands;
                code = OpCodeAdress;
                flag1 = aFlag1;
                flag2 = aFlag2;
                flag3 = aFlag3;
                flag4 = aFlag4;
                flag5 = OF.NONE;
            }

            public itemplate( OCE aOpCode, int aOperandCount, ulong[] aOperands, int OpCodeAdress, OperandFlag aFlag1, OperandFlag aFlag2, OperandFlag aFlag3, OperandFlag aFlag4, OperandFlag aFlag5 )
            {
                opcode = aOpCode;
                operands = aOperandCount;
                opd = aOperands;
                code = OpCodeAdress;
                flag1 = aFlag1;
                flag2 = aFlag2;
                flag3 = aFlag3;
                flag4 = aFlag4;
                flag5 = aFlag5;
            }

        };

        static itemplate[] instrux_AAA = new[]
        {
            new itemplate(OCE.AAA, 0, new ulong[]{0,0,0,0,0} ,22095, OF.IF_8086, OF.IF_NOLONG),
        };

        static itemplate[] instrux_AAD = new[]
        {
            new itemplate(OCE.AAD, 0, new ulong[] {0,0,0,0,0} ,21047, OF.IF_8086, OF.IF_NOLONG),
            new itemplate(OCE.AAD, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,21051, OF.IF_8086, OF.IF_SB, OF.IF_NOLONG),
        };

        static itemplate[] instrux_AAM = new[] 
        {
            new itemplate(OCE.AAM, 0, new ulong[] {0,0,0,0,0} ,21055, OF.IF_8086, OF.IF_NOLONG),
            new itemplate(OCE.AAM, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,21059, OF.IF_8086, OF.IF_SB, OF.IF_NOLONG),
        };

        static itemplate[] instrux_AAS = new[] 
        {
            new itemplate(OCE.AAS, 0, new ulong[] {0,0,0,0,0} ,22098, OF.IF_8086, OF.IF_NOLONG),
    
        };

        static itemplate[] instrux_ADC = new[] 
        {
            new itemplate(OCE.ADC, 2, new ulong[] {MEMORY,REG8,0,0,0} ,21063, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADC, 2, new ulong[] {REG8,REG8,0,0,0} ,21063, OF.IF_8086),
            new itemplate(OCE.ADC, 2, new ulong[] {MEMORY,REG16,0,0,0} ,19167, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADC, 2, new ulong[] {REG16,REG16,0,0,0} ,19167, OF.IF_8086),
            new itemplate(OCE.ADC, 2, new ulong[] {MEMORY,REG32,0,0,0} ,19172, OF.IF_386, OF.IF_SM),
            new itemplate(OCE.ADC, 2, new ulong[] {REG32,REG32,0,0,0} ,19172, OF.IF_386),
            new itemplate(OCE.ADC, 2, new ulong[] {MEMORY,REG64,0,0,0} ,19177, OF.IF_X64, OF.IF_SM),
            new itemplate(OCE.ADC, 2, new ulong[] {REG64,REG64,0,0,0} ,19177, OF.IF_X64),
            new itemplate(OCE.ADC, 2, new ulong[] {REG8,MEMORY,0,0,0} ,12405, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADC, 2, new ulong[] {REG8,REG8,0,0,0} ,12405, OF.IF_8086),
            new itemplate(OCE.ADC, 2, new ulong[] {REG16,MEMORY,0,0,0} ,19182, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADC, 2, new ulong[] {REG16,REG16,0,0,0} ,19182, OF.IF_8086),
            new itemplate(OCE.ADC, 2, new ulong[] {REG32,MEMORY,0,0,0} ,19187, OF.IF_386, OF.IF_SM),
            new itemplate(OCE.ADC, 2, new ulong[] {REG32,REG32,0,0,0} ,19187, OF.IF_386),
            new itemplate(OCE.ADC, 2, new ulong[] {REG64,MEMORY,0,0,0} ,19192, OF.IF_X64, OF.IF_SM),
            new itemplate(OCE.ADC, 2, new ulong[] {REG64,REG64,0,0,0} ,19192, OF.IF_X64),
            new itemplate(OCE.ADC, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE, BITS8,0,0,0} ,15657, OF.IF_8086),
            new itemplate(OCE.ADC, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE, BITS8,0,0,0} ,15663, OF.IF_386),
            new itemplate(OCE.ADC, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE, BITS8,0,0,0} ,15669, OF.IF_X64),
            new itemplate(OCE.ADC, 2, new ulong[] {REG_AL,IMMEDIATE,0,0,0} ,21067, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADC, 2, new ulong[] {REG_AX,SBYTE16,0,0,0} ,15657, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADC, 2, new ulong[] {REG_AX,IMMEDIATE,0,0,0} ,19197, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADC, 2, new ulong[] {REG_EAX,SBYTE32,0,0,0} ,15663, OF.IF_386, OF.IF_SM),
            new itemplate(OCE.ADC, 2, new ulong[] {REG_EAX,IMMEDIATE,0,0,0} ,19202, OF.IF_386, OF.IF_SM),
            new itemplate(OCE.ADC, 2, new ulong[] {REG_RAX,SBYTE64,0,0,0} ,15669, OF.IF_X64, OF.IF_SM),
            new itemplate(OCE.ADC, 2, new ulong[] {REG_RAX,IMMEDIATE,0,0,0} ,19207, OF.IF_X64, OF.IF_SM),
            new itemplate(OCE.ADC, 2, new ulong[] {RM_GPR, BITS8,IMMEDIATE,0,0,0} ,19212, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADC, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,15675, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADC, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,15681, OF.IF_386, OF.IF_SM),
            new itemplate(OCE.ADC, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,15687, OF.IF_X64, OF.IF_SM),
            new itemplate(OCE.ADC, 2, new ulong[] {MEMORY,IMMEDIATE, BITS8,0,0,0} ,19212, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADC, 2, new ulong[] {MEMORY,IMMEDIATE, BITS16,0,0,0} ,15675, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADC, 2, new ulong[] {MEMORY,IMMEDIATE, BITS32,0,0,0} ,15681, OF.IF_386, OF.IF_SM),
        };

        static itemplate[] instrux_ADD = new[] 
        {
            new itemplate(OCE.ADD, 2, new ulong[] {MEMORY,REG8,0,0,0} ,21701, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADD, 2, new ulong[] {REG8,REG8,0,0,0} ,21071, OF.IF_8086),
            new itemplate(OCE.ADD, 2, new ulong[] {MEMORY,REG16,0,0,0} ,19217, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADD, 2, new ulong[] {REG16,REG16,0,0,0} ,19217, OF.IF_8086),
            new itemplate(OCE.ADD, 2, new ulong[] {MEMORY,REG32,0,0,0} ,19222, OF.IF_386, OF.IF_SM),
            new itemplate(OCE.ADD, 2, new ulong[] {REG32,REG32,0,0,0} ,19222, OF.IF_386),
            new itemplate(OCE.ADD, 2, new ulong[] {MEMORY,REG64,0,0,0} ,19227, OF.IF_X64, OF.IF_SM),
            new itemplate(OCE.ADD, 2, new ulong[] {REG64,REG64,0,0,0} ,19227, OF.IF_X64),
            new itemplate(OCE.ADD, 2, new ulong[] {REG8,MEMORY,0,0,0} ,13056, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADD, 2, new ulong[] {REG8,REG8,0,0,0} ,13056, OF.IF_8086),
            new itemplate(OCE.ADD, 2, new ulong[] {REG16,MEMORY,0,0,0} ,19232, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADD, 2, new ulong[] {REG16,REG16,0,0,0} ,19232, OF.IF_8086),
            new itemplate(OCE.ADD, 2, new ulong[] {REG32,MEMORY,0,0,0} ,19237, OF.IF_386, OF.IF_SM),
            new itemplate(OCE.ADD, 2, new ulong[] {REG32,REG32,0,0,0} ,19237, OF.IF_386),
            new itemplate(OCE.ADD, 2, new ulong[] {REG64,MEMORY,0,0,0} ,19242, OF.IF_X64, OF.IF_SM),
            new itemplate(OCE.ADD, 2, new ulong[] {REG64,REG64,0,0,0} ,19242, OF.IF_X64),
            new itemplate(OCE.ADD, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE, BITS8,0,0,0} ,15693, OF.IF_8086),
            new itemplate(OCE.ADD, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE, BITS8,0,0,0} ,15699, OF.IF_386),
            new itemplate(OCE.ADD, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE, BITS8,0,0,0} ,15705, OF.IF_X64),
            new itemplate(OCE.ADD, 2, new ulong[] {REG_AL,IMMEDIATE,0,0,0} ,21075, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADD, 2, new ulong[] {REG_AX,SBYTE16,0,0,0} ,15693, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADD, 2, new ulong[] {REG_AX,IMMEDIATE,0,0,0} ,19247, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADD, 2, new ulong[] {REG_EAX,SBYTE32,0,0,0} ,15699, OF.IF_386, OF.IF_SM),
            new itemplate(OCE.ADD, 2, new ulong[] {REG_EAX,IMMEDIATE,0,0,0} ,19252, OF.IF_386, OF.IF_SM),
            new itemplate(OCE.ADD, 2, new ulong[] {REG_RAX,SBYTE64,0,0,0} ,15705, OF.IF_X64, OF.IF_SM),
            new itemplate(OCE.ADD, 2, new ulong[] {REG_RAX,IMMEDIATE,0,0,0} ,19257, OF.IF_X64, OF.IF_SM),
            new itemplate(OCE.ADD, 2, new ulong[] {RM_GPR, BITS8,IMMEDIATE,0,0,0} ,19262, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADD, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,15711, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADD, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,15717, OF.IF_386, OF.IF_SM),
            new itemplate(OCE.ADD, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,15723, OF.IF_X64, OF.IF_SM),
            new itemplate(OCE.ADD, 2, new ulong[] {MEMORY,IMMEDIATE, BITS8,0,0,0} ,19262, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADD, 2, new ulong[] {MEMORY,IMMEDIATE, BITS16,0,0,0} ,15711, OF.IF_8086, OF.IF_SM),
            new itemplate(OCE.ADD, 2, new ulong[] {MEMORY,IMMEDIATE, BITS32,0,0,0} ,15717, OF.IF_386, OF.IF_SM),
        };

        static itemplate[] instrux_ADDPD = new[] {
    new itemplate(OCE.ADDPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17601, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_ADDPS = new[] {
    new itemplate(OCE.ADDPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,16887, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_ADDSD = new[] {
    new itemplate(OCE.ADDSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17607, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SQ),
    
};

        static itemplate[] instrux_ADDSS = new[] {
    new itemplate(OCE.ADDSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,16893, OF.IF_KATMAI, OF.IF_SSE, OF.IF_SD),
    
};

        static itemplate[] instrux_ADDSUBPD = new[] {
    new itemplate(OCE.ADDSUBPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17877, OF.IF_PRESCOTT, OF.IF_SSE3, OF.IF_SO),
    
};

        static itemplate[] instrux_ADDSUBPS = new[] {
    new itemplate(OCE.ADDSUBPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17883, OF.IF_PRESCOTT, OF.IF_SSE3, OF.IF_SO),
    
};

        static itemplate[] instrux_AESDEC = new[] {
    new itemplate(OCE.AESDEC, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10904, OF.IF_SSE, OF.IF_WESTMERE, OF.IF_SO),
    
};

        static itemplate[] instrux_AESDECLAST = new[] {
    new itemplate(OCE.AESDECLAST, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10911, OF.IF_SSE, OF.IF_WESTMERE, OF.IF_SO),
    
};

        static itemplate[] instrux_AESENC = new[] {
    new itemplate(OCE.AESENC, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10890, OF.IF_SSE, OF.IF_WESTMERE, OF.IF_SO),
    
};

        static itemplate[] instrux_AESENCLAST = new[] {
    new itemplate(OCE.AESENCLAST, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10897, OF.IF_SSE, OF.IF_WESTMERE, OF.IF_SO),
    
};

        static itemplate[] instrux_AESIMC = new[] {
    new itemplate(OCE.AESIMC, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10918, OF.IF_SSE, OF.IF_WESTMERE, OF.IF_SO),
    
};

        static itemplate[] instrux_AESKEYGENASSIST = new[] {
    new itemplate(OCE.AESKEYGENASSIST, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7388, OF.IF_SSE, OF.IF_WESTMERE, OF.IF_SO),
    
};

        static itemplate[] instrux_AND = new[] {
    new itemplate(OCE.AND, 2, new ulong[] {MEMORY,REG8,0,0,0} ,21079, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.AND, 2, new ulong[] {REG8,REG8,0,0,0} ,21079, OF.IF_8086),
    new itemplate(OCE.AND, 2, new ulong[] {MEMORY,REG16,0,0,0} ,19267, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.AND, 2, new ulong[] {REG16,REG16,0,0,0} ,19267, OF.IF_8086),
    new itemplate(OCE.AND, 2, new ulong[] {MEMORY,REG32,0,0,0} ,19272, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.AND, 2, new ulong[] {REG32,REG32,0,0,0} ,19272, OF.IF_386),
    new itemplate(OCE.AND, 2, new ulong[] {MEMORY,REG64,0,0,0} ,19277, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.AND, 2, new ulong[] {REG64,REG64,0,0,0} ,19277, OF.IF_X64),
    new itemplate(OCE.AND, 2, new ulong[] {REG8,MEMORY,0,0,0} ,13343, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.AND, 2, new ulong[] {REG8,REG8,0,0,0} ,13343, OF.IF_8086),
    new itemplate(OCE.AND, 2, new ulong[] {REG16,MEMORY,0,0,0} ,19282, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.AND, 2, new ulong[] {REG16,REG16,0,0,0} ,19282, OF.IF_8086),
    new itemplate(OCE.AND, 2, new ulong[] {REG32,MEMORY,0,0,0} ,19287, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.AND, 2, new ulong[] {REG32,REG32,0,0,0} ,19287, OF.IF_386),
    new itemplate(OCE.AND, 2, new ulong[] {REG64,MEMORY,0,0,0} ,19292, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.AND, 2, new ulong[] {REG64,REG64,0,0,0} ,19292, OF.IF_X64),
    new itemplate(OCE.AND, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE, BITS8,0,0,0} ,15729, OF.IF_8086),
    new itemplate(OCE.AND, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE, BITS8,0,0,0} ,15735, OF.IF_386),
    new itemplate(OCE.AND, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE, BITS8,0,0,0} ,15741, OF.IF_X64),
    new itemplate(OCE.AND, 2, new ulong[] {REG_AL,IMMEDIATE,0,0,0} ,21083, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.AND, 2, new ulong[] {REG_AX,SBYTE16,0,0,0} ,15729, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.AND, 2, new ulong[] {REG_AX,IMMEDIATE,0,0,0} ,19297, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.AND, 2, new ulong[] {REG_EAX,SBYTE32,0,0,0} ,15735, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.AND, 2, new ulong[] {REG_EAX,IMMEDIATE,0,0,0} ,19302, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.AND, 2, new ulong[] {REG_RAX,SBYTE64,0,0,0} ,15741, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.AND, 2, new ulong[] {REG_RAX,IMMEDIATE,0,0,0} ,19307, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.AND, 2, new ulong[] {RM_GPR, BITS8,IMMEDIATE,0,0,0} ,19312, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.AND, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,15747, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.AND, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,15753, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.AND, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,15759, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.AND, 2, new ulong[] {MEMORY,IMMEDIATE, BITS8,0,0,0} ,19312, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.AND, 2, new ulong[] {MEMORY,IMMEDIATE, BITS16,0,0,0} ,15747, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.AND, 2, new ulong[] {MEMORY,IMMEDIATE, BITS32,0,0,0} ,15753, OF.IF_386, OF.IF_SM),
    
};

        static itemplate[] instrux_ANDNPD = new[] {
    new itemplate(OCE.ANDNPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17613, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_ANDNPS = new[] {
    new itemplate(OCE.ANDNPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,16899, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_ANDPD = new[] {
    new itemplate(OCE.ANDPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17619, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_ANDPS = new[] {
    new itemplate(OCE.ANDPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,16905, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_ARPL = new[] {
    new itemplate(OCE.ARPL, 2, new ulong[] {MEMORY,REG16,0,0,0} ,21087, OF.IF_286, OF.IF_PROT, OF.IF_SM, OF.IF_NOLONG),
    new itemplate(OCE.ARPL, 2, new ulong[] {REG16,REG16,0,0,0} ,21087, OF.IF_286, OF.IF_PROT, OF.IF_NOLONG),
    
};

        static itemplate[] instrux_BB0_RESET = new[] {
    new itemplate(OCE.BB0_RESET, 0, new ulong[] {0,0,0,0,0} ,21091, OF.IF_PENT, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_BB1_RESET = new[] {
    new itemplate(OCE.BB1_RESET, 0, new ulong[] {0,0,0,0,0} ,21095, OF.IF_PENT, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_BLENDPD = new[] {
    new itemplate(OCE.BLENDPD, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7092, OF.IF_SSE41),
    
};

        static itemplate[] instrux_BLENDPS = new[] {
    new itemplate(OCE.BLENDPS, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7100, OF.IF_SSE41),
    
};

        static itemplate[] instrux_BLENDVPD = new[] {
    new itemplate(OCE.BLENDVPD, 3, new ulong[] {XMMREG,RM_XMM,XMM0,0,0} ,9651, OF.IF_SSE41),
    
};

        static itemplate[] instrux_BLENDVPS = new[] {
    new itemplate(OCE.BLENDVPS, 3, new ulong[] {XMMREG,RM_XMM,XMM0,0,0} ,9658, OF.IF_SSE41),
    
};

        static itemplate[] instrux_BOUND = new[] {
    new itemplate(OCE.BOUND, 2, new ulong[] {REG16,MEMORY,0,0,0} ,19317, OF.IF_186, OF.IF_NOLONG),
    new itemplate(OCE.BOUND, 2, new ulong[] {REG32,MEMORY,0,0,0} ,19322, OF.IF_386, OF.IF_NOLONG),
    
};

        static itemplate[] instrux_BSF = new[] {
    new itemplate(OCE.BSF, 2, new ulong[] {REG16,MEMORY,0,0,0} ,15765, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.BSF, 2, new ulong[] {REG16,REG16,0,0,0} ,15765, OF.IF_386),
    new itemplate(OCE.BSF, 2, new ulong[] {REG32,MEMORY,0,0,0} ,15771, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.BSF, 2, new ulong[] {REG32,REG32,0,0,0} ,15771, OF.IF_386),
    new itemplate(OCE.BSF, 2, new ulong[] {REG64,MEMORY,0,0,0} ,15777, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.BSF, 2, new ulong[] {REG64,REG64,0,0,0} ,15777, OF.IF_X64),
    
};

        static itemplate[] instrux_BSR = new[] {
    new itemplate(OCE.BSR, 2, new ulong[] {REG16,MEMORY,0,0,0} ,15783, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.BSR, 2, new ulong[] {REG16,REG16,0,0,0} ,15783, OF.IF_386),
    new itemplate(OCE.BSR, 2, new ulong[] {REG32,MEMORY,0,0,0} ,15789, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.BSR, 2, new ulong[] {REG32,REG32,0,0,0} ,15789, OF.IF_386),
    new itemplate(OCE.BSR, 2, new ulong[] {REG64,MEMORY,0,0,0} ,15795, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.BSR, 2, new ulong[] {REG64,REG64,0,0,0} ,15795, OF.IF_X64),
    
};

        static itemplate[] instrux_BSWAP = new[] {
    new itemplate(OCE.BSWAP, 1, new ulong[] {REG32,0,0,0,0} ,15801, OF.IF_486),
    new itemplate(OCE.BSWAP, 1, new ulong[] {REG64,0,0,0,0} ,15807, OF.IF_X64),
    
};

        static itemplate[] instrux_BT = new[] {
    new itemplate(OCE.BT, 2, new ulong[] {MEMORY,REG16,0,0,0} ,15813, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.BT, 2, new ulong[] {REG16,REG16,0,0,0} ,15813, OF.IF_386),
    new itemplate(OCE.BT, 2, new ulong[] {MEMORY,REG32,0,0,0} ,15819, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.BT, 2, new ulong[] {REG32,REG32,0,0,0} ,15819, OF.IF_386),
    new itemplate(OCE.BT, 2, new ulong[] {MEMORY,REG64,0,0,0} ,15825, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.BT, 2, new ulong[] {REG64,REG64,0,0,0} ,15825, OF.IF_X64),
    new itemplate(OCE.BT, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,8468, OF.IF_386, OF.IF_SB),
    new itemplate(OCE.BT, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,8475, OF.IF_386, OF.IF_SB),
    new itemplate(OCE.BT, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,8482, OF.IF_X64, OF.IF_SB),
    
};

        static itemplate[] instrux_BTC = new[] {
    new itemplate(OCE.BTC, 2, new ulong[] {MEMORY,REG16,0,0,0} ,15831, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.BTC, 2, new ulong[] {REG16,REG16,0,0,0} ,15831, OF.IF_386),
    new itemplate(OCE.BTC, 2, new ulong[] {MEMORY,REG32,0,0,0} ,15837, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.BTC, 2, new ulong[] {REG32,REG32,0,0,0} ,15837, OF.IF_386),
    new itemplate(OCE.BTC, 2, new ulong[] {MEMORY,REG64,0,0,0} ,15843, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.BTC, 2, new ulong[] {REG64,REG64,0,0,0} ,15843, OF.IF_X64),
    new itemplate(OCE.BTC, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,8489, OF.IF_386, OF.IF_SB),
    new itemplate(OCE.BTC, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,8496, OF.IF_386, OF.IF_SB),
    new itemplate(OCE.BTC, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,8503, OF.IF_X64, OF.IF_SB),
    
};

        static itemplate[] instrux_BTR = new[] {
    new itemplate(OCE.BTR, 2, new ulong[] {MEMORY,REG16,0,0,0} ,15849, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.BTR, 2, new ulong[] {REG16,REG16,0,0,0} ,15849, OF.IF_386),
    new itemplate(OCE.BTR, 2, new ulong[] {MEMORY,REG32,0,0,0} ,15855, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.BTR, 2, new ulong[] {REG32,REG32,0,0,0} ,15855, OF.IF_386),
    new itemplate(OCE.BTR, 2, new ulong[] {MEMORY,REG64,0,0,0} ,15861, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.BTR, 2, new ulong[] {REG64,REG64,0,0,0} ,15861, OF.IF_X64),
    new itemplate(OCE.BTR, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,8510, OF.IF_386, OF.IF_SB),
    new itemplate(OCE.BTR, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,8517, OF.IF_386, OF.IF_SB),
    new itemplate(OCE.BTR, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,8524, OF.IF_X64, OF.IF_SB),
    
};

        static itemplate[] instrux_BTS = new[] {
    new itemplate(OCE.BTS, 2, new ulong[] {MEMORY,REG16,0,0,0} ,15867, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.BTS, 2, new ulong[] {REG16,REG16,0,0,0} ,15867, OF.IF_386),
    new itemplate(OCE.BTS, 2, new ulong[] {MEMORY,REG32,0,0,0} ,15873, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.BTS, 2, new ulong[] {REG32,REG32,0,0,0} ,15873, OF.IF_386),
    new itemplate(OCE.BTS, 2, new ulong[] {MEMORY,REG64,0,0,0} ,15879, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.BTS, 2, new ulong[] {REG64,REG64,0,0,0} ,15879, OF.IF_X64),
    new itemplate(OCE.BTS, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,8531, OF.IF_386, OF.IF_SB),
    new itemplate(OCE.BTS, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,8538, OF.IF_386, OF.IF_SB),
    new itemplate(OCE.BTS, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,8545, OF.IF_X64, OF.IF_SB),
    
};

        static itemplate[] instrux_CALL = new[] {
    new itemplate(OCE.CALL, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,19327, OF.IF_8086),
    new itemplate(OCE.CALL, 1, new ulong[] {IMMEDIATE, NEAR,0,0,0,0} ,19327, OF.IF_8086),
    new itemplate(OCE.CALL, 1, new ulong[] {IMMEDIATE, FAR,0,0,0,0} ,15885, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.CALL, 1, new ulong[] {IMMEDIATE, BITS16,0,0,0,0} ,19332, OF.IF_8086),
    new itemplate(OCE.CALL, 1, new ulong[] {IMMEDIATE, BITS16, NEAR,0,0,0,0} ,19332, OF.IF_8086),
    new itemplate(OCE.CALL, 1, new ulong[] {IMMEDIATE, BITS16, FAR,0,0,0,0} ,15891, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.CALL, 1, new ulong[] {IMMEDIATE, BITS32,0,0,0,0} ,19337, OF.IF_386),
    new itemplate(OCE.CALL, 1, new ulong[] {IMMEDIATE, BITS32, NEAR,0,0,0,0} ,19337, OF.IF_386),
    new itemplate(OCE.CALL, 1, new ulong[] {IMMEDIATE, BITS32, FAR,0,0,0,0} ,15897, OF.IF_386, OF.IF_NOLONG),
    new itemplate(OCE.CALL, 2, new ulong[] {IMMEDIATE, COLON,IMMEDIATE,0,0,0} ,15903, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.CALL, 2, new ulong[] {IMMEDIATE, BITS16, COLON,IMMEDIATE,0,0,0} ,15909, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.CALL, 2, new ulong[] {IMMEDIATE, COLON,IMMEDIATE, BITS16,0,0,0} ,15909, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.CALL, 2, new ulong[] {IMMEDIATE, BITS32, COLON,IMMEDIATE,0,0,0} ,15915, OF.IF_386, OF.IF_NOLONG),
    new itemplate(OCE.CALL, 2, new ulong[] {IMMEDIATE, COLON,IMMEDIATE, BITS32,0,0,0} ,15915, OF.IF_386, OF.IF_NOLONG),
    new itemplate(OCE.CALL, 1, new ulong[] {MEMORY, FAR,0,0,0,0} ,19342, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.CALL, 1, new ulong[] {MEMORY, FAR,0,0,0,0} ,19347, OF.IF_X64),
    new itemplate(OCE.CALL, 1, new ulong[] {MEMORY, BITS16, FAR,0,0,0,0} ,19352, OF.IF_8086),
    new itemplate(OCE.CALL, 1, new ulong[] {MEMORY, BITS32, FAR,0,0,0,0} ,19357, OF.IF_386),
    new itemplate(OCE.CALL, 1, new ulong[] {MEMORY, BITS64, FAR,0,0,0,0} ,19347, OF.IF_X64),
    new itemplate(OCE.CALL, 1, new ulong[] {MEMORY, NEAR,0,0,0,0} ,19362, OF.IF_8086),
    new itemplate(OCE.CALL, 1, new ulong[] {MEMORY, BITS16, NEAR,0,0,0,0} ,19367, OF.IF_8086),
    new itemplate(OCE.CALL, 1, new ulong[] {MEMORY, BITS32, NEAR,0,0,0,0} ,19372, OF.IF_386, OF.IF_NOLONG),
    new itemplate(OCE.CALL, 1, new ulong[] {MEMORY, BITS64, NEAR,0,0,0,0} ,19377, OF.IF_X64),
    new itemplate(OCE.CALL, 1, new ulong[] {REG16,0,0,0,0} ,19367, OF.IF_8086),
    new itemplate(OCE.CALL, 1, new ulong[] {REG32,0,0,0,0} ,19372, OF.IF_386, OF.IF_NOLONG),
    new itemplate(OCE.CALL, 1, new ulong[] {REG64,0,0,0,0} ,19382, OF.IF_X64),
    new itemplate(OCE.CALL, 1, new ulong[] {MEMORY,0,0,0,0} ,19362, OF.IF_8086),
    new itemplate(OCE.CALL, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,19367, OF.IF_8086),
    new itemplate(OCE.CALL, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,19372, OF.IF_386, OF.IF_NOLONG),
    new itemplate(OCE.CALL, 1, new ulong[] {MEMORY, BITS64,0,0,0,0} ,19382, OF.IF_X64),
    
};

        static itemplate[] instrux_CBW = new[] {
    new itemplate(OCE.CBW, 0, new ulong[] {0,0,0,0,0} ,21099, OF.IF_8086),
    
};

        static itemplate[] instrux_CDQ = new[] {
    new itemplate(OCE.CDQ, 0, new ulong[] {0,0,0,0,0} ,21103, OF.IF_386),
    
};

        static itemplate[] instrux_CDQE = new[] {
    new itemplate(OCE.CDQE, 0, new ulong[] {0,0,0,0,0} ,21107, OF.IF_X64),
    
};

        static itemplate[] instrux_CLC = new[] {
    new itemplate(OCE.CLC, 0, new ulong[] {0,0,0,0,0} ,20819, OF.IF_8086),
    
};

        static itemplate[] instrux_CLD = new[] {
    new itemplate(OCE.CLD, 0, new ulong[] {0,0,0,0,0} ,22101, OF.IF_8086),
    
};

        static itemplate[] instrux_CLFLUSH = new[] {
    new itemplate(OCE.CLFLUSH, 1, new ulong[] {MEMORY,0,0,0,0} ,20992, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_CLGI = new[] {
    new itemplate(OCE.CLGI, 0, new ulong[] {0,0,0,0,0} ,19387, OF.IF_X64, OF.IF_AMD),
    
};

        static itemplate[] instrux_CLI = new[] {
    new itemplate(OCE.CLI, 0, new ulong[] {0,0,0,0,0} ,22104, OF.IF_8086),
    
};

        static itemplate[] instrux_CLTS = new[] {
    new itemplate(OCE.CLTS, 0, new ulong[] {0,0,0,0,0} ,21111, OF.IF_286, OF.IF_PRIV),
    
};

        static itemplate[] instrux_CMC = new[] {
    new itemplate(OCE.CMC, 0, new ulong[] {0,0,0,0,0} ,22107, OF.IF_8086),
    
};

        static itemplate[] instrux_CMP = new[] {
    new itemplate(OCE.CMP, 2, new ulong[] {MEMORY,REG8,0,0,0} ,21115, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.CMP, 2, new ulong[] {REG8,REG8,0,0,0} ,21115, OF.IF_8086),
    new itemplate(OCE.CMP, 2, new ulong[] {MEMORY,REG16,0,0,0} ,19392, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.CMP, 2, new ulong[] {REG16,REG16,0,0,0} ,19392, OF.IF_8086),
    new itemplate(OCE.CMP, 2, new ulong[] {MEMORY,REG32,0,0,0} ,19397, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.CMP, 2, new ulong[] {REG32,REG32,0,0,0} ,19397, OF.IF_386),
    new itemplate(OCE.CMP, 2, new ulong[] {MEMORY,REG64,0,0,0} ,19402, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.CMP, 2, new ulong[] {REG64,REG64,0,0,0} ,19402, OF.IF_X64),
    new itemplate(OCE.CMP, 2, new ulong[] {REG8,MEMORY,0,0,0} ,13301, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.CMP, 2, new ulong[] {REG8,REG8,0,0,0} ,13301, OF.IF_8086),
    new itemplate(OCE.CMP, 2, new ulong[] {REG16,MEMORY,0,0,0} ,19407, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.CMP, 2, new ulong[] {REG16,REG16,0,0,0} ,19407, OF.IF_8086),
    new itemplate(OCE.CMP, 2, new ulong[] {REG32,MEMORY,0,0,0} ,19412, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.CMP, 2, new ulong[] {REG32,REG32,0,0,0} ,19412, OF.IF_386),
    new itemplate(OCE.CMP, 2, new ulong[] {REG64,MEMORY,0,0,0} ,19417, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.CMP, 2, new ulong[] {REG64,REG64,0,0,0} ,19417, OF.IF_X64),
    new itemplate(OCE.CMP, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE, BITS8,0,0,0} ,15921, OF.IF_8086),
    new itemplate(OCE.CMP, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE, BITS8,0,0,0} ,15927, OF.IF_386),
    new itemplate(OCE.CMP, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE, BITS8,0,0,0} ,15933, OF.IF_X64),
    new itemplate(OCE.CMP, 2, new ulong[] {REG_AL,IMMEDIATE,0,0,0} ,21119, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.CMP, 2, new ulong[] {REG_AX,SBYTE16,0,0,0} ,15921, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.CMP, 2, new ulong[] {REG_AX,IMMEDIATE,0,0,0} ,19422, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.CMP, 2, new ulong[] {REG_EAX,SBYTE32,0,0,0} ,15927, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.CMP, 2, new ulong[] {REG_EAX,IMMEDIATE,0,0,0} ,19427, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.CMP, 2, new ulong[] {REG_RAX,SBYTE64,0,0,0} ,15933, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.CMP, 2, new ulong[] {REG_RAX,IMMEDIATE,0,0,0} ,19432, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.CMP, 2, new ulong[] {RM_GPR, BITS8,IMMEDIATE,0,0,0} ,19437, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.CMP, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,15939, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.CMP, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,15945, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.CMP, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,15951, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.CMP, 2, new ulong[] {MEMORY,IMMEDIATE, BITS8,0,0,0} ,19437, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.CMP, 2, new ulong[] {MEMORY,IMMEDIATE, BITS16,0,0,0} ,15939, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.CMP, 2, new ulong[] {MEMORY,IMMEDIATE, BITS32,0,0,0} ,15945, OF.IF_386, OF.IF_SM),
    
};

        static itemplate[] instrux_CMPEQPD = new[] {
    new itemplate(OCE.CMPEQPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6916, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_CMPEQPS = new[] {
    new itemplate(OCE.CMPEQPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6740, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_CMPEQSD = new[] {
    new itemplate(OCE.CMPEQSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6924, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_CMPEQSS = new[] {
    new itemplate(OCE.CMPEQSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6748, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_CMPLEPD = new[] {
    new itemplate(OCE.CMPLEPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6932, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_CMPLEPS = new[] {
    new itemplate(OCE.CMPLEPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6756, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_CMPLESD = new[] {
    new itemplate(OCE.CMPLESD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6940, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_CMPLESS = new[] {
    new itemplate(OCE.CMPLESS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6764, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_CMPLTPD = new[] {
    new itemplate(OCE.CMPLTPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6948, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_CMPLTPS = new[] {
    new itemplate(OCE.CMPLTPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6772, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_CMPLTSD = new[] {
    new itemplate(OCE.CMPLTSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6956, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_CMPLTSS = new[] {
    new itemplate(OCE.CMPLTSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6780, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_CMPNEQPD = new[] {
    new itemplate(OCE.CMPNEQPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6964, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_CMPNEQPS = new[] {
    new itemplate(OCE.CMPNEQPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6788, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_CMPNEQSD = new[] {
    new itemplate(OCE.CMPNEQSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6972, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_CMPNEQSS = new[] {
    new itemplate(OCE.CMPNEQSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6796, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_CMPNLEPD = new[] {
    new itemplate(OCE.CMPNLEPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6980, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_CMPNLEPS = new[] {
    new itemplate(OCE.CMPNLEPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6804, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_CMPNLESD = new[] {
    new itemplate(OCE.CMPNLESD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6988, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_CMPNLESS = new[] {
    new itemplate(OCE.CMPNLESS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6812, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_CMPNLTPD = new[] {
    new itemplate(OCE.CMPNLTPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6996, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_CMPNLTPS = new[] {
    new itemplate(OCE.CMPNLTPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6820, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_CMPNLTSD = new[] {
    new itemplate(OCE.CMPNLTSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,7004, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_CMPNLTSS = new[] {
    new itemplate(OCE.CMPNLTSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6828, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_CMPORDPD = new[] {
    new itemplate(OCE.CMPORDPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,7012, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_CMPORDPS = new[] {
    new itemplate(OCE.CMPORDPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6836, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_CMPORDSD = new[] {
    new itemplate(OCE.CMPORDSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,7020, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_CMPORDSS = new[] {
    new itemplate(OCE.CMPORDSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6844, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_CMPPD = new[] {
    new itemplate(OCE.CMPPD, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,9357, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SM2, OF.IF_SB, OF.IF_AR2),
    
};

        static itemplate[] instrux_CMPPS = new[] {
    new itemplate(OCE.CMPPS, 3, new ulong[] {XMMREG,MEMORY,IMMEDIATE,0,0} ,9098, OF.IF_KATMAI, OF.IF_SSE, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.CMPPS, 3, new ulong[] {XMMREG,XMMREG,IMMEDIATE,0,0} ,9098, OF.IF_KATMAI, OF.IF_SSE, OF.IF_SB, OF.IF_AR2),
    
};

        static itemplate[] instrux_CMPSB = new[] {
    new itemplate(OCE.CMPSB, 0, new ulong[] {0,0,0,0,0} ,21123, OF.IF_8086),
    
};

        static itemplate[] instrux_CMPSD = new[] {
    new itemplate(OCE.CMPSD, 0, new ulong[] {0,0,0,0,0} ,19442, OF.IF_386),
    new itemplate(OCE.CMPSD, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,9364, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SB, OF.IF_AR2),
    
};

        static itemplate[] instrux_CMPSQ = new[] {
    new itemplate(OCE.CMPSQ, 0, new ulong[] {0,0,0,0,0} ,19447, OF.IF_X64),
    
};

        static itemplate[] instrux_CMPSS = new[] {
    new itemplate(OCE.CMPSS, 3, new ulong[] {XMMREG,MEMORY,IMMEDIATE,0,0} ,9105, OF.IF_KATMAI, OF.IF_SSE, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.CMPSS, 3, new ulong[] {XMMREG,XMMREG,IMMEDIATE,0,0} ,9105, OF.IF_KATMAI, OF.IF_SSE, OF.IF_SB, OF.IF_AR2),
    
};

        static itemplate[] instrux_CMPSW = new[] {
    new itemplate(OCE.CMPSW, 0, new ulong[] {0,0,0,0,0} ,19452, OF.IF_8086),
    
};

        static itemplate[] instrux_CMPUNORDPD = new[] {
    new itemplate(OCE.CMPUNORDPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,7028, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_CMPUNORDPS = new[] {
    new itemplate(OCE.CMPUNORDPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6852, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_CMPUNORDSD = new[] {
    new itemplate(OCE.CMPUNORDSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,7036, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_CMPUNORDSS = new[] {
    new itemplate(OCE.CMPUNORDSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,6860, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_CMPXCHG = new[] {
    new itemplate(OCE.CMPXCHG, 2, new ulong[] {MEMORY,REG8,0,0,0} ,19457, OF.IF_PENT, OF.IF_SM),
    new itemplate(OCE.CMPXCHG, 2, new ulong[] {REG8,REG8,0,0,0} ,19457, OF.IF_PENT),
    new itemplate(OCE.CMPXCHG, 2, new ulong[] {MEMORY,REG16,0,0,0} ,15957, OF.IF_PENT, OF.IF_SM),
    new itemplate(OCE.CMPXCHG, 2, new ulong[] {REG16,REG16,0,0,0} ,15957, OF.IF_PENT),
    new itemplate(OCE.CMPXCHG, 2, new ulong[] {MEMORY,REG32,0,0,0} ,15963, OF.IF_PENT, OF.IF_SM),
    new itemplate(OCE.CMPXCHG, 2, new ulong[] {REG32,REG32,0,0,0} ,15963, OF.IF_PENT),
    new itemplate(OCE.CMPXCHG, 2, new ulong[] {MEMORY,REG64,0,0,0} ,15969, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.CMPXCHG, 2, new ulong[] {REG64,REG64,0,0,0} ,15969, OF.IF_X64),
    
};

        static itemplate[] instrux_CMPXCHG16B = new[] {
    new itemplate(OCE.CMPXCHG16B, 1, new ulong[] {MEMORY,0,0,0,0} ,15987, OF.IF_X64),
    
};

        static itemplate[] instrux_CMPXCHG486 = new[] {
    new itemplate(OCE.CMPXCHG486, 2, new ulong[] {MEMORY,REG8,0,0,0} ,19462, OF.IF_486, OF.IF_SM, OF.IF_UNDOC),
    new itemplate(OCE.CMPXCHG486, 2, new ulong[] {REG8,REG8,0,0,0} ,19462, OF.IF_486, OF.IF_UNDOC),
    new itemplate(OCE.CMPXCHG486, 2, new ulong[] {MEMORY,REG16,0,0,0} ,15975, OF.IF_486, OF.IF_SM, OF.IF_UNDOC),
    new itemplate(OCE.CMPXCHG486, 2, new ulong[] {REG16,REG16,0,0,0} ,15975, OF.IF_486, OF.IF_UNDOC),
    new itemplate(OCE.CMPXCHG486, 2, new ulong[] {MEMORY,REG32,0,0,0} ,15981, OF.IF_486, OF.IF_SM, OF.IF_UNDOC),
    new itemplate(OCE.CMPXCHG486, 2, new ulong[] {REG32,REG32,0,0,0} ,15981, OF.IF_486, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_CMPXCHG8B = new[] {
    new itemplate(OCE.CMPXCHG8B, 1, new ulong[] {MEMORY,0,0,0,0} ,15988, OF.IF_PENT),
    
};

        static itemplate[] instrux_COMEQPD = new[] {
    new itemplate(OCE.COMEQPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,198, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMEQPS = new[] {
    new itemplate(OCE.COMEQPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,54, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMEQSD = new[] {
    new itemplate(OCE.COMEQSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,486, OF.IF_SSE5, OF.IF_AMD, OF.IF_SQ),
    
};

        static itemplate[] instrux_COMEQSS = new[] {
    new itemplate(OCE.COMEQSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,342, OF.IF_SSE5, OF.IF_AMD, OF.IF_SD),
    
};

        static itemplate[] instrux_COMFALSEPD = new[] {
    new itemplate(OCE.COMFALSEPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,297, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMFALSEPS = new[] {
    new itemplate(OCE.COMFALSEPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,153, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMFALSESD = new[] {
    new itemplate(OCE.COMFALSESD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,585, OF.IF_SSE5, OF.IF_AMD, OF.IF_SQ),
    
};

        static itemplate[] instrux_COMFALSESS = new[] {
    new itemplate(OCE.COMFALSESS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,441, OF.IF_SSE5, OF.IF_AMD, OF.IF_SD),
    
};

        static itemplate[] instrux_COMISD = new[] {
    new itemplate(OCE.COMISD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17625, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_COMISS = new[] {
    new itemplate(OCE.COMISS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,16911, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_COMLEPD = new[] {
    new itemplate(OCE.COMLEPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,216, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMLEPS = new[] {
    new itemplate(OCE.COMLEPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,72, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMLESD = new[] {
    new itemplate(OCE.COMLESD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,504, OF.IF_SSE5, OF.IF_AMD, OF.IF_SQ),
    
};

        static itemplate[] instrux_COMLESS = new[] {
    new itemplate(OCE.COMLESS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,360, OF.IF_SSE5, OF.IF_AMD, OF.IF_SD),
    
};

        static itemplate[] instrux_COMLTPD = new[] {
    new itemplate(OCE.COMLTPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,207, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMLTPS = new[] {
    new itemplate(OCE.COMLTPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,63, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMLTSD = new[] {
    new itemplate(OCE.COMLTSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,495, OF.IF_SSE5, OF.IF_AMD, OF.IF_SQ),
    
};

        static itemplate[] instrux_COMLTSS = new[] {
    new itemplate(OCE.COMLTSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,351, OF.IF_SSE5, OF.IF_AMD, OF.IF_SD),
    
};

        static itemplate[] instrux_COMNEQPD = new[] {
    new itemplate(OCE.COMNEQPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,306, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMNEQPS = new[] {
    new itemplate(OCE.COMNEQPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,162, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMNEQSD = new[] {
    new itemplate(OCE.COMNEQSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,594, OF.IF_SSE5, OF.IF_AMD, OF.IF_SQ),
    
};

        static itemplate[] instrux_COMNEQSS = new[] {
    new itemplate(OCE.COMNEQSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,450, OF.IF_SSE5, OF.IF_AMD, OF.IF_SD),
    
};

        static itemplate[] instrux_COMNLEPD = new[] {
    new itemplate(OCE.COMNLEPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,324, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMNLEPS = new[] {
    new itemplate(OCE.COMNLEPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,180, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMNLESD = new[] {
    new itemplate(OCE.COMNLESD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,612, OF.IF_SSE5, OF.IF_AMD, OF.IF_SQ),
    
};

        static itemplate[] instrux_COMNLESS = new[] {
    new itemplate(OCE.COMNLESS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,468, OF.IF_SSE5, OF.IF_AMD, OF.IF_SD),
    
};

        static itemplate[] instrux_COMNLTPD = new[] {
    new itemplate(OCE.COMNLTPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,315, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMNLTPS = new[] {
    new itemplate(OCE.COMNLTPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,171, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMNLTSD = new[] {
    new itemplate(OCE.COMNLTSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,603, OF.IF_SSE5, OF.IF_AMD, OF.IF_SQ),
    
};

        static itemplate[] instrux_COMNLTSS = new[] {
    new itemplate(OCE.COMNLTSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,459, OF.IF_SSE5, OF.IF_AMD, OF.IF_SD),
    
};

        static itemplate[] instrux_COMORDPD = new[] {
    new itemplate(OCE.COMORDPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,261, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMORDPS = new[] {
    new itemplate(OCE.COMORDPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,117, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMORDSD = new[] {
    new itemplate(OCE.COMORDSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,549, OF.IF_SSE5, OF.IF_AMD, OF.IF_SQ),
    
};

        static itemplate[] instrux_COMORDSS = new[] {
    new itemplate(OCE.COMORDSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,405, OF.IF_SSE5, OF.IF_AMD, OF.IF_SD),
    
};

        static itemplate[] instrux_COMPD = new[] {
    new itemplate(OCE.COMPD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7252, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMPS = new[] {
    new itemplate(OCE.COMPS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7244, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMSD = new[] {
    new itemplate(OCE.COMSD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7268, OF.IF_SSE5, OF.IF_AMD, OF.IF_SQ),
    
};

        static itemplate[] instrux_COMSS = new[] {
    new itemplate(OCE.COMSS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7260, OF.IF_SSE5, OF.IF_AMD, OF.IF_SD),
    
};

        static itemplate[] instrux_COMTRUEPD = new[] {
    new itemplate(OCE.COMTRUEPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,333, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMTRUEPS = new[] {
    new itemplate(OCE.COMTRUEPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,189, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMTRUESD = new[] {
    new itemplate(OCE.COMTRUESD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,621, OF.IF_SSE5, OF.IF_AMD, OF.IF_SQ),
    
};

        static itemplate[] instrux_COMTRUESS = new[] {
    new itemplate(OCE.COMTRUESS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,477, OF.IF_SSE5, OF.IF_AMD, OF.IF_SD),
    
};

        static itemplate[] instrux_COMUEQPD = new[] {
    new itemplate(OCE.COMUEQPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,270, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMUEQPS = new[] {
    new itemplate(OCE.COMUEQPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,126, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMUEQSD = new[] {
    new itemplate(OCE.COMUEQSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,558, OF.IF_SSE5, OF.IF_AMD, OF.IF_SQ),
    
};

        static itemplate[] instrux_COMUEQSS = new[] {
    new itemplate(OCE.COMUEQSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,414, OF.IF_SSE5, OF.IF_AMD, OF.IF_SD),
    
};

        static itemplate[] instrux_COMULEPD = new[] {
    new itemplate(OCE.COMULEPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,288, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMULEPS = new[] {
    new itemplate(OCE.COMULEPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,144, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMULESD = new[] {
    new itemplate(OCE.COMULESD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,576, OF.IF_SSE5, OF.IF_AMD, OF.IF_SQ),
    
};

        static itemplate[] instrux_COMULESS = new[] {
    new itemplate(OCE.COMULESS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,432, OF.IF_SSE5, OF.IF_AMD, OF.IF_SD),
    
};

        static itemplate[] instrux_COMULTPD = new[] {
    new itemplate(OCE.COMULTPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,279, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMULTPS = new[] {
    new itemplate(OCE.COMULTPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,135, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMULTSD = new[] {
    new itemplate(OCE.COMULTSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,567, OF.IF_SSE5, OF.IF_AMD, OF.IF_SQ),
    
};

        static itemplate[] instrux_COMULTSS = new[] {
    new itemplate(OCE.COMULTSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,423, OF.IF_SSE5, OF.IF_AMD, OF.IF_SD),
    
};

        static itemplate[] instrux_COMUNEQPD = new[] {
    new itemplate(OCE.COMUNEQPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,234, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMUNEQPS = new[] {
    new itemplate(OCE.COMUNEQPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,90, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMUNEQSD = new[] {
    new itemplate(OCE.COMUNEQSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,522, OF.IF_SSE5, OF.IF_AMD, OF.IF_SQ),
    
};

        static itemplate[] instrux_COMUNEQSS = new[] {
    new itemplate(OCE.COMUNEQSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,378, OF.IF_SSE5, OF.IF_AMD, OF.IF_SD),
    
};

        static itemplate[] instrux_COMUNLEPD = new[] {
    new itemplate(OCE.COMUNLEPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,252, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMUNLEPS = new[] {
    new itemplate(OCE.COMUNLEPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,108, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMUNLESD = new[] {
    new itemplate(OCE.COMUNLESD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,540, OF.IF_SSE5, OF.IF_AMD, OF.IF_SQ),
    
};

        static itemplate[] instrux_COMUNLESS = new[] {
    new itemplate(OCE.COMUNLESS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,396, OF.IF_SSE5, OF.IF_AMD, OF.IF_SD),
    
};

        static itemplate[] instrux_COMUNLTPD = new[] {
    new itemplate(OCE.COMUNLTPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,243, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMUNLTPS = new[] {
    new itemplate(OCE.COMUNLTPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,99, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMUNLTSD = new[] {
    new itemplate(OCE.COMUNLTSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,531, OF.IF_SSE5, OF.IF_AMD, OF.IF_SQ),
    
};

        static itemplate[] instrux_COMUNLTSS = new[] {
    new itemplate(OCE.COMUNLTSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,387, OF.IF_SSE5, OF.IF_AMD, OF.IF_SD),
    
};

        static itemplate[] instrux_COMUNORDPD = new[] {
    new itemplate(OCE.COMUNORDPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,225, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMUNORDPS = new[] {
    new itemplate(OCE.COMUNORDPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,81, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_COMUNORDSD = new[] {
    new itemplate(OCE.COMUNORDSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,513, OF.IF_SSE5, OF.IF_AMD, OF.IF_SQ),
    
};

        static itemplate[] instrux_COMUNORDSS = new[] {
    new itemplate(OCE.COMUNORDSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,369, OF.IF_SSE5, OF.IF_AMD, OF.IF_SD),
    
};

        static itemplate[] instrux_CPUID = new[] {
    new itemplate(OCE.CPUID, 0, new ulong[] {0,0,0,0,0} ,21127, OF.IF_PENT),
    
};

        static itemplate[] instrux_CPU_READ = new[] {
    new itemplate(OCE.CPU_READ, 0, new ulong[] {0,0,0,0,0} ,21131, OF.IF_PENT, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_CPU_WRITE = new[] {
    new itemplate(OCE.CPU_WRITE, 0, new ulong[] {0,0,0,0,0} ,21135, OF.IF_PENT, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_CQO = new[] {
    new itemplate(OCE.CQO, 0, new ulong[] {0,0,0,0,0} ,21139, OF.IF_X64),
    
};

        static itemplate[] instrux_CRC32 = new[] {
    new itemplate(OCE.CRC32, 2, new ulong[] {REG32,RM_GPR, BITS8,0,0,0} ,7197, OF.IF_SSE42),
    new itemplate(OCE.CRC32, 2, new ulong[] {REG32,RM_GPR, BITS16,0,0,0} ,7180, OF.IF_SSE42),
    new itemplate(OCE.CRC32, 2, new ulong[] {REG32,RM_GPR, BITS32,0,0,0} ,7188, OF.IF_SSE42),
    new itemplate(OCE.CRC32, 2, new ulong[] {REG64,RM_GPR, BITS8,0,0,0} ,7196, OF.IF_SSE42, OF.IF_X64),
    new itemplate(OCE.CRC32, 2, new ulong[] {REG64,RM_GPR, BITS64,0,0,0} ,7204, OF.IF_SSE42, OF.IF_X64),
    
};

        static itemplate[] instrux_CVTDQ2PD = new[] {
    new itemplate(OCE.CVTDQ2PD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17631, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SQ),
    
};

        static itemplate[] instrux_CVTDQ2PS = new[] {
    new itemplate(OCE.CVTDQ2PS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17637, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_CVTPD2DQ = new[] {
    new itemplate(OCE.CVTPD2DQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17643, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_CVTPD2PI = new[] {
    new itemplate(OCE.CVTPD2PI, 2, new ulong[] {MMXREG,RM_XMM,0,0,0} ,17649, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_CVTPD2PS = new[] {
    new itemplate(OCE.CVTPD2PS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17655, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_CVTPH2PS = new[] {
    new itemplate(OCE.CVTPH2PS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10729, OF.IF_SSE5, OF.IF_AMD, OF.IF_SQ),
    
};

        static itemplate[] instrux_CVTPI2PD = new[] {
    new itemplate(OCE.CVTPI2PD, 2, new ulong[] {XMMREG,RM_MMX,0,0,0} ,17661, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SQ),
    
};

        static itemplate[] instrux_CVTPI2PS = new[] {
    new itemplate(OCE.CVTPI2PS, 2, new ulong[] {XMMREG,RM_MMX,0,0,0} ,16917, OF.IF_KATMAI, OF.IF_SSE, OF.IF_MMX, OF.IF_SQ),
    
};

        static itemplate[] instrux_CVTPS2DQ = new[] {
    new itemplate(OCE.CVTPS2DQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17667, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_CVTPS2PD = new[] {
    new itemplate(OCE.CVTPS2PD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17673, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SQ),
    
};

        static itemplate[] instrux_CVTPS2PH = new[] {
    new itemplate(OCE.CVTPS2PH, 2, new ulong[] {RM_XMM,XMMREG,0,0,0} ,10736, OF.IF_SSE5, OF.IF_AMD, OF.IF_SQ),
    
};

        static itemplate[] instrux_CVTPS2PI = new[] {
    new itemplate(OCE.CVTPS2PI, 2, new ulong[] {MMXREG,RM_XMM,0,0,0} ,16923, OF.IF_KATMAI, OF.IF_SSE, OF.IF_MMX, OF.IF_SQ),
    
};

        static itemplate[] instrux_CVTSD2SI = new[] {
    new itemplate(OCE.CVTSD2SI, 2, new ulong[] {REG32,XMMREG,0,0,0} ,9372, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SQ, OF.IF_AR1),
    new itemplate(OCE.CVTSD2SI, 2, new ulong[] {REG32,MEMORY,0,0,0} ,9372, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SQ, OF.IF_AR1),
    new itemplate(OCE.CVTSD2SI, 2, new ulong[] {REG64,XMMREG,0,0,0} ,9371, OF.IF_X64, OF.IF_SSE2, OF.IF_SQ, OF.IF_AR1),
    new itemplate(OCE.CVTSD2SI, 2, new ulong[] {REG64,MEMORY,0,0,0} ,9371, OF.IF_X64, OF.IF_SSE2, OF.IF_SQ, OF.IF_AR1),
    
};

        static itemplate[] instrux_CVTSD2SS = new[] {
    new itemplate(OCE.CVTSD2SS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17679, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SQ),
    
};

        static itemplate[] instrux_CVTSI2SD = new[] {
    new itemplate(OCE.CVTSI2SD, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,9379, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SD, OF.IF_AR1),
    new itemplate(OCE.CVTSI2SD, 2, new ulong[] {XMMREG,RM_GPR, BITS32,0,0,0} ,9379, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SD, OF.IF_AR1),
    new itemplate(OCE.CVTSI2SD, 2, new ulong[] {XMMREG,RM_GPR, BITS64,0,0,0} ,9378, OF.IF_X64, OF.IF_SSE2, OF.IF_SQ, OF.IF_AR1),
    
};

        static itemplate[] instrux_CVTSI2SS = new[] {
    new itemplate(OCE.CVTSI2SS, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,9113, OF.IF_KATMAI, OF.IF_SSE, OF.IF_SD, OF.IF_AR1),
    new itemplate(OCE.CVTSI2SS, 2, new ulong[] {XMMREG,RM_GPR, BITS32,0,0,0} ,9113, OF.IF_KATMAI, OF.IF_SSE, OF.IF_SD, OF.IF_AR1),
    new itemplate(OCE.CVTSI2SS, 2, new ulong[] {XMMREG,RM_GPR, BITS64,0,0,0} ,9112, OF.IF_X64, OF.IF_SSE, OF.IF_SQ, OF.IF_AR1),
    
};

        static itemplate[] instrux_CVTSS2SD = new[] {
    new itemplate(OCE.CVTSS2SD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17685, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SD),
    
};

        static itemplate[] instrux_CVTSS2SI = new[] {
    new itemplate(OCE.CVTSS2SI, 2, new ulong[] {REG32,XMMREG,0,0,0} ,9120, OF.IF_KATMAI, OF.IF_SSE, OF.IF_SD, OF.IF_AR1),
    new itemplate(OCE.CVTSS2SI, 2, new ulong[] {REG32,MEMORY,0,0,0} ,9120, OF.IF_KATMAI, OF.IF_SSE, OF.IF_SD, OF.IF_AR1),
    new itemplate(OCE.CVTSS2SI, 2, new ulong[] {REG64,XMMREG,0,0,0} ,9119, OF.IF_X64, OF.IF_SSE, OF.IF_SD, OF.IF_AR1),
    new itemplate(OCE.CVTSS2SI, 2, new ulong[] {REG64,MEMORY,0,0,0} ,9119, OF.IF_X64, OF.IF_SSE, OF.IF_SD, OF.IF_AR1),
    
};

        static itemplate[] instrux_CVTTPD2DQ = new[] {
    new itemplate(OCE.CVTTPD2DQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17697, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_CVTTPD2PI = new[] {
    new itemplate(OCE.CVTTPD2PI, 2, new ulong[] {MMXREG,RM_XMM,0,0,0} ,17691, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_CVTTPS2DQ = new[] {
    new itemplate(OCE.CVTTPS2DQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17703, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_CVTTPS2PI = new[] {
    new itemplate(OCE.CVTTPS2PI, 2, new ulong[] {MMXREG,RM_XMM,0,0,0} ,16929, OF.IF_KATMAI, OF.IF_SSE, OF.IF_MMX, OF.IF_SQ),
    
};

        static itemplate[] instrux_CVTTSD2SI = new[] {
    new itemplate(OCE.CVTTSD2SI, 2, new ulong[] {REG32,XMMREG,0,0,0} ,9386, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SQ, OF.IF_AR1),
    new itemplate(OCE.CVTTSD2SI, 2, new ulong[] {REG32,MEMORY,0,0,0} ,9386, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SQ, OF.IF_AR1),
    new itemplate(OCE.CVTTSD2SI, 2, new ulong[] {REG64,XMMREG,0,0,0} ,9385, OF.IF_X64, OF.IF_SSE2, OF.IF_SQ, OF.IF_AR1),
    new itemplate(OCE.CVTTSD2SI, 2, new ulong[] {REG64,MEMORY,0,0,0} ,9385, OF.IF_X64, OF.IF_SSE2, OF.IF_SQ, OF.IF_AR1),
    
};

        static itemplate[] instrux_CVTTSS2SI = new[] {
    new itemplate(OCE.CVTTSS2SI, 2, new ulong[] {REG32,RM_XMM,0,0,0} ,9127, OF.IF_KATMAI, OF.IF_SSE, OF.IF_SD, OF.IF_AR1),
    new itemplate(OCE.CVTTSS2SI, 2, new ulong[] {REG64,RM_XMM,0,0,0} ,9126, OF.IF_X64, OF.IF_SSE, OF.IF_SD, OF.IF_AR1),
    
};

        static itemplate[] instrux_CWD = new[] {
    new itemplate(OCE.CWD, 0, new ulong[] {0,0,0,0,0} ,21143, OF.IF_8086),
    
};

        static itemplate[] instrux_CWDE = new[] {
    new itemplate(OCE.CWDE, 0, new ulong[] {0,0,0,0,0} ,21147, OF.IF_386),
    
};

        static itemplate[] instrux_DAA = new[] {
    new itemplate(OCE.DAA, 0, new ulong[] {0,0,0,0,0} ,22110, OF.IF_8086, OF.IF_NOLONG),
    
};

        static itemplate[] instrux_DAS = new[] {
    new itemplate(OCE.DAS, 0, new ulong[] {0,0,0,0,0} ,22113, OF.IF_8086, OF.IF_NOLONG),
    
};

        static itemplate[] instrux_DB;// = new[] {};

        static itemplate[] instrux_DD;// = new[] {};

        static itemplate[] instrux_DEC = new[] {
    new itemplate(OCE.DEC, 1, new ulong[] {REG16,0,0,0,0} ,21151, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.DEC, 1, new ulong[] {REG32,0,0,0,0} ,21155, OF.IF_386, OF.IF_NOLONG),
    new itemplate(OCE.DEC, 1, new ulong[] {RM_GPR, BITS8,0,0,0,0} ,21159, OF.IF_8086),
    new itemplate(OCE.DEC, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,19467, OF.IF_8086),
    new itemplate(OCE.DEC, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,19472, OF.IF_386),
    new itemplate(OCE.DEC, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,19477, OF.IF_X64),
    
};

        static itemplate[] instrux_DIV = new[] {
    new itemplate(OCE.DIV, 1, new ulong[] {RM_GPR, BITS8,0,0,0,0} ,21163, OF.IF_8086),
    new itemplate(OCE.DIV, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,19482, OF.IF_8086),
    new itemplate(OCE.DIV, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,19487, OF.IF_386),
    new itemplate(OCE.DIV, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,19492, OF.IF_X64),
    
};

        static itemplate[] instrux_DIVPD = new[] {
    new itemplate(OCE.DIVPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17709, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_DIVPS = new[] {
    new itemplate(OCE.DIVPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,16935, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_DIVSD = new[] {
    new itemplate(OCE.DIVSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17715, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_DIVSS = new[] {
    new itemplate(OCE.DIVSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,16941, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_DMINT = new[] {
    new itemplate(OCE.DMINT, 0, new ulong[] {0,0,0,0,0} ,21167, OF.IF_P6, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_DO;// = new[] {};

        static itemplate[] instrux_DPPD = new[] {
    new itemplate(OCE.DPPD, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7108, OF.IF_SSE41),
    
};

        static itemplate[] instrux_DPPS = new[] {
    new itemplate(OCE.DPPS, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7116, OF.IF_SSE41),
    
};

        static itemplate[] instrux_DQ;// = new[] {};

        static itemplate[] instrux_DT;// = new[] {};

        static itemplate[] instrux_DW;// = new[] {};

        static itemplate[] instrux_DY;// = new[] {};

        static itemplate[] instrux_EMMS = new[] {
    new itemplate(OCE.EMMS, 0, new ulong[] {0,0,0,0,0} ,21171, OF.IF_PENT, OF.IF_MMX),
    
};

        static itemplate[] instrux_ENTER = new[] {
    new itemplate(OCE.ENTER, 2, new ulong[] {IMMEDIATE,IMMEDIATE,0,0,0} ,19497, OF.IF_186),
    
};

        static itemplate[] instrux_EQU = new[] {
    new itemplate(OCE.EQU, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,6930, OF.IF_8086),
    new itemplate(OCE.EQU, 2, new ulong[] {IMMEDIATE, COLON,IMMEDIATE,0,0,0} ,6930, OF.IF_8086),
    
};

        static itemplate[] instrux_EXTRACTPS = new[] {
    new itemplate(OCE.EXTRACTPS, 3, new ulong[] {RM_GPR, BITS32,XMMREG,IMMEDIATE,0,0} ,1, OF.IF_SSE41),
    new itemplate(OCE.EXTRACTPS, 3, new ulong[] {REG64,XMMREG,IMMEDIATE,0,0} ,0, OF.IF_SSE41, OF.IF_X64),
    
};

        static itemplate[] instrux_EXTRQ = new[] {
    new itemplate(OCE.EXTRQ, 3, new ulong[] {XMMREG,IMMEDIATE,IMMEDIATE,0,0} ,7076, OF.IF_SSE4A, OF.IF_AMD),
    new itemplate(OCE.EXTRQ, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,17949, OF.IF_SSE4A, OF.IF_AMD),
    
};

        static itemplate[] instrux_F2XM1 = new[] {
    new itemplate(OCE.F2XM1, 0, new ulong[] {0,0,0,0,0} ,21175, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FABS = new[] {
    new itemplate(OCE.FABS, 0, new ulong[] {0,0,0,0,0} ,21179, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FADD = new[] {
    new itemplate(OCE.FADD, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21183, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FADD, 1, new ulong[] {MEMORY, BITS64,0,0,0,0} ,21187, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FADD, 1, new ulong[] {FPUREG, TO,0,0,0,0} ,19502, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FADD, 1, new ulong[] {FPUREG,0,0,0,0} ,19507, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FADD, 2, new ulong[] {FPUREG,FPU0,0,0,0} ,19502, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FADD, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19512, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FADD, 0, new ulong[] {0,0,0,0,0} ,21191, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FADDP = new[] {
    new itemplate(OCE.FADDP, 1, new ulong[] {FPUREG,0,0,0,0} ,19517, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FADDP, 2, new ulong[] {FPUREG,FPU0,0,0,0} ,19517, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FADDP, 0, new ulong[] {0,0,0,0,0} ,21191, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FBLD = new[] {
    new itemplate(OCE.FBLD, 1, new ulong[] {MEMORY, BITS80,0,0,0,0} ,21195, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FBLD, 1, new ulong[] {MEMORY,0,0,0,0} ,21195, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FBSTP = new[] {
    new itemplate(OCE.FBSTP, 1, new ulong[] {MEMORY, BITS80,0,0,0,0} ,21199, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FBSTP, 1, new ulong[] {MEMORY,0,0,0,0} ,21199, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FCHS = new[] {
    new itemplate(OCE.FCHS, 0, new ulong[] {0,0,0,0,0} ,21203, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FCLEX = new[] {
    new itemplate(OCE.FCLEX, 0, new ulong[] {0,0,0,0,0} ,19522, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FCMOVB = new[] {
    new itemplate(OCE.FCMOVB, 1, new ulong[] {FPUREG,0,0,0,0} ,19527, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FCMOVB, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19532, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FCMOVB, 0, new ulong[] {0,0,0,0,0} ,21207, OF.IF_P6, OF.IF_FPU),
    
};

        static itemplate[] instrux_FCMOVBE = new[] {
    new itemplate(OCE.FCMOVBE, 1, new ulong[] {FPUREG,0,0,0,0} ,19537, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FCMOVBE, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19542, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FCMOVBE, 0, new ulong[] {0,0,0,0,0} ,21211, OF.IF_P6, OF.IF_FPU),
    
};

        static itemplate[] instrux_FCMOVE = new[] {
    new itemplate(OCE.FCMOVE, 1, new ulong[] {FPUREG,0,0,0,0} ,19547, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FCMOVE, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19552, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FCMOVE, 0, new ulong[] {0,0,0,0,0} ,21215, OF.IF_P6, OF.IF_FPU),
    
};

        static itemplate[] instrux_FCMOVNB = new[] {
    new itemplate(OCE.FCMOVNB, 1, new ulong[] {FPUREG,0,0,0,0} ,19557, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FCMOVNB, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19562, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FCMOVNB, 0, new ulong[] {0,0,0,0,0} ,21219, OF.IF_P6, OF.IF_FPU),
    
};

        static itemplate[] instrux_FCMOVNBE = new[] {
    new itemplate(OCE.FCMOVNBE, 1, new ulong[] {FPUREG,0,0,0,0} ,19567, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FCMOVNBE, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19572, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FCMOVNBE, 0, new ulong[] {0,0,0,0,0} ,21223, OF.IF_P6, OF.IF_FPU),
    
};

        static itemplate[] instrux_FCMOVNE = new[] {
    new itemplate(OCE.FCMOVNE, 1, new ulong[] {FPUREG,0,0,0,0} ,19577, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FCMOVNE, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19582, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FCMOVNE, 0, new ulong[] {0,0,0,0,0} ,21227, OF.IF_P6, OF.IF_FPU),
    
};

        static itemplate[] instrux_FCMOVNU = new[] {
    new itemplate(OCE.FCMOVNU, 1, new ulong[] {FPUREG,0,0,0,0} ,19587, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FCMOVNU, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19592, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FCMOVNU, 0, new ulong[] {0,0,0,0,0} ,21231, OF.IF_P6, OF.IF_FPU),
    
};

        static itemplate[] instrux_FCMOVU = new[] {
    new itemplate(OCE.FCMOVU, 1, new ulong[] {FPUREG,0,0,0,0} ,19597, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FCMOVU, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19602, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FCMOVU, 0, new ulong[] {0,0,0,0,0} ,21235, OF.IF_P6, OF.IF_FPU),
    
};

        static itemplate[] instrux_FCOM = new[] {
    new itemplate(OCE.FCOM, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21239, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FCOM, 1, new ulong[] {MEMORY, BITS64,0,0,0,0} ,21243, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FCOM, 1, new ulong[] {FPUREG,0,0,0,0} ,19607, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FCOM, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19612, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FCOM, 0, new ulong[] {0,0,0,0,0} ,21247, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FCOMI = new[] {
    new itemplate(OCE.FCOMI, 1, new ulong[] {FPUREG,0,0,0,0} ,19617, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FCOMI, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19622, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FCOMI, 0, new ulong[] {0,0,0,0,0} ,21251, OF.IF_P6, OF.IF_FPU),
    
};

        static itemplate[] instrux_FCOMIP = new[] {
    new itemplate(OCE.FCOMIP, 1, new ulong[] {FPUREG,0,0,0,0} ,19627, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FCOMIP, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19632, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FCOMIP, 0, new ulong[] {0,0,0,0,0} ,21255, OF.IF_P6, OF.IF_FPU),
    
};

        static itemplate[] instrux_FCOMP = new[] {
    new itemplate(OCE.FCOMP, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21259, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FCOMP, 1, new ulong[] {MEMORY, BITS64,0,0,0,0} ,21263, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FCOMP, 1, new ulong[] {FPUREG,0,0,0,0} ,19637, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FCOMP, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19642, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FCOMP, 0, new ulong[] {0,0,0,0,0} ,21267, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FCOMPP = new[] {
    new itemplate(OCE.FCOMPP, 0, new ulong[] {0,0,0,0,0} ,21271, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FCOS = new[] {
    new itemplate(OCE.FCOS, 0, new ulong[] {0,0,0,0,0} ,21275, OF.IF_386, OF.IF_FPU),
    
};

        static itemplate[] instrux_FDECSTP = new[] {
    new itemplate(OCE.FDECSTP, 0, new ulong[] {0,0,0,0,0} ,21279, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FDISI = new[] {
    new itemplate(OCE.FDISI, 0, new ulong[] {0,0,0,0,0} ,19647, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FDIV = new[] {
    new itemplate(OCE.FDIV, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21283, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FDIV, 1, new ulong[] {MEMORY, BITS64,0,0,0,0} ,21287, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FDIV, 1, new ulong[] {FPUREG, TO,0,0,0,0} ,19652, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FDIV, 1, new ulong[] {FPUREG,0,0,0,0} ,19657, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FDIV, 2, new ulong[] {FPUREG,FPU0,0,0,0} ,19652, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FDIV, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19662, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FDIV, 0, new ulong[] {0,0,0,0,0} ,21291, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FDIVP = new[] {
    new itemplate(OCE.FDIVP, 1, new ulong[] {FPUREG,0,0,0,0} ,19667, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FDIVP, 2, new ulong[] {FPUREG,FPU0,0,0,0} ,19667, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FDIVP, 0, new ulong[] {0,0,0,0,0} ,21291, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FDIVR = new[] {
    new itemplate(OCE.FDIVR, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21295, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FDIVR, 1, new ulong[] {MEMORY, BITS64,0,0,0,0} ,21299, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FDIVR, 1, new ulong[] {FPUREG, TO,0,0,0,0} ,19672, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FDIVR, 2, new ulong[] {FPUREG,FPU0,0,0,0} ,19672, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FDIVR, 1, new ulong[] {FPUREG,0,0,0,0} ,19677, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FDIVR, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19682, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FDIVR, 0, new ulong[] {0,0,0,0,0} ,21303, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FDIVRP = new[] {
    new itemplate(OCE.FDIVRP, 1, new ulong[] {FPUREG,0,0,0,0} ,19687, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FDIVRP, 2, new ulong[] {FPUREG,FPU0,0,0,0} ,19687, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FDIVRP, 0, new ulong[] {0,0,0,0,0} ,21303, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FEMMS = new[] {
    new itemplate(OCE.FEMMS, 0, new ulong[] {0,0,0,0,0} ,21307, OF.IF_PENT, OF.IF_3DNOW),
    
};

        static itemplate[] instrux_FENI = new[] {
    new itemplate(OCE.FENI, 0, new ulong[] {0,0,0,0,0} ,19692, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FFREE = new[] {
    new itemplate(OCE.FFREE, 1, new ulong[] {FPUREG,0,0,0,0} ,19697, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FFREE, 0, new ulong[] {0,0,0,0,0} ,21311, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FFREEP = new[] {
    new itemplate(OCE.FFREEP, 1, new ulong[] {FPUREG,0,0,0,0} ,19702, OF.IF_286, OF.IF_FPU, OF.IF_UNDOC),
    new itemplate(OCE.FFREEP, 0, new ulong[] {0,0,0,0,0} ,21315, OF.IF_286, OF.IF_FPU, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_FIADD = new[] {
    new itemplate(OCE.FIADD, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21319, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FIADD, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,21323, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FICOM = new[] {
    new itemplate(OCE.FICOM, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21327, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FICOM, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,21331, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FICOMP = new[] {
    new itemplate(OCE.FICOMP, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21335, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FICOMP, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,21339, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FIDIV = new[] {
    new itemplate(OCE.FIDIV, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21343, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FIDIV, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,21347, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FIDIVR = new[] {
    new itemplate(OCE.FIDIVR, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21351, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FIDIVR, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,21355, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FILD = new[] {
    new itemplate(OCE.FILD, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21359, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FILD, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,21363, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FILD, 1, new ulong[] {MEMORY, BITS64,0,0,0,0} ,21367, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FIMUL = new[] {
    new itemplate(OCE.FIMUL, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21371, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FIMUL, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,21375, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FINCSTP = new[] {
    new itemplate(OCE.FINCSTP, 0, new ulong[] {0,0,0,0,0} ,21379, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FINIT = new[] {
    new itemplate(OCE.FINIT, 0, new ulong[] {0,0,0,0,0} ,19707, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FIST = new[] {
    new itemplate(OCE.FIST, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21383, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FIST, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,21387, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FISTP = new[] {
    new itemplate(OCE.FISTP, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21391, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FISTP, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,21395, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FISTP, 1, new ulong[] {MEMORY, BITS64,0,0,0,0} ,21399, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FISTTP = new[] {
    new itemplate(OCE.FISTTP, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,21403, OF.IF_PRESCOTT, OF.IF_FPU),
    new itemplate(OCE.FISTTP, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21407, OF.IF_PRESCOTT, OF.IF_FPU),
    new itemplate(OCE.FISTTP, 1, new ulong[] {MEMORY, BITS64,0,0,0,0} ,21411, OF.IF_PRESCOTT, OF.IF_FPU),
    
};

        static itemplate[] instrux_FISUB = new[] {
    new itemplate(OCE.FISUB, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21415, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FISUB, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,21419, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FISUBR = new[] {
    new itemplate(OCE.FISUBR, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21423, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FISUBR, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,21427, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FLD = new[] {
    new itemplate(OCE.FLD, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21431, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FLD, 1, new ulong[] {MEMORY, BITS64,0,0,0,0} ,21435, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FLD, 1, new ulong[] {MEMORY, BITS80,0,0,0,0} ,21439, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FLD, 1, new ulong[] {FPUREG,0,0,0,0} ,19712, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FLD, 0, new ulong[] {0,0,0,0,0} ,21443, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FLD1 = new[] {
    new itemplate(OCE.FLD1, 0, new ulong[] {0,0,0,0,0} ,21447, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FLDCW = new[] {
    new itemplate(OCE.FLDCW, 1, new ulong[] {MEMORY,0,0,0,0} ,21451, OF.IF_8086, OF.IF_FPU, OF.IF_SW),
    
};

        static itemplate[] instrux_FLDENV = new[] {
    new itemplate(OCE.FLDENV, 1, new ulong[] {MEMORY,0,0,0,0} ,21455, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FLDL2E = new[] {
    new itemplate(OCE.FLDL2E, 0, new ulong[] {0,0,0,0,0} ,21459, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FLDL2T = new[] {
    new itemplate(OCE.FLDL2T, 0, new ulong[] {0,0,0,0,0} ,21463, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FLDLG2 = new[] {
    new itemplate(OCE.FLDLG2, 0, new ulong[] {0,0,0,0,0} ,21467, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FLDLN2 = new[] {
    new itemplate(OCE.FLDLN2, 0, new ulong[] {0,0,0,0,0} ,21471, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FLDPI = new[] {
    new itemplate(OCE.FLDPI, 0, new ulong[] {0,0,0,0,0} ,21475, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FLDZ = new[] {
    new itemplate(OCE.FLDZ, 0, new ulong[] {0,0,0,0,0} ,21479, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FMADDPD = new[] {
    new itemplate(OCE.FMADDPD, 4, new ulong[] {XMMREG,SAME_AS, 0,XMMREG,RM_XMM,0} ,9917, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMADDPD, 4, new ulong[] {XMMREG,SAME_AS, 0,RM_XMM,XMMREG,0} ,9924, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMADDPD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,9931, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMADDPD, 4, new ulong[] {XMMREG,RM_XMM,XMMREG,SAME_AS, 0,0} ,9938, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_FMADDPS = new[] {
    new itemplate(OCE.FMADDPS, 4, new ulong[] {XMMREG,SAME_AS, 0,XMMREG,RM_XMM,0} ,9889, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMADDPS, 4, new ulong[] {XMMREG,SAME_AS, 0,RM_XMM,XMMREG,0} ,9896, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMADDPS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,9903, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMADDPS, 4, new ulong[] {XMMREG,RM_XMM,XMMREG,SAME_AS, 0,0} ,9910, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_FMADDSD = new[] {
    new itemplate(OCE.FMADDSD, 4, new ulong[] {XMMREG,SAME_AS, 0,XMMREG,RM_XMM,0} ,9973, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMADDSD, 4, new ulong[] {XMMREG,SAME_AS, 0,RM_XMM,XMMREG,0} ,9980, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMADDSD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,9987, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMADDSD, 4, new ulong[] {XMMREG,RM_XMM,XMMREG,SAME_AS, 0,0} ,9994, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_FMADDSS = new[] {
    new itemplate(OCE.FMADDSS, 4, new ulong[] {XMMREG,SAME_AS, 0,XMMREG,RM_XMM,0} ,9945, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMADDSS, 4, new ulong[] {XMMREG,SAME_AS, 0,RM_XMM,XMMREG,0} ,9952, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMADDSS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,9959, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMADDSS, 4, new ulong[] {XMMREG,RM_XMM,XMMREG,SAME_AS, 0,0} ,9966, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_FMSUBPD = new[] {
    new itemplate(OCE.FMSUBPD, 4, new ulong[] {XMMREG,SAME_AS, 0,XMMREG,RM_XMM,0} ,10029, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMSUBPD, 4, new ulong[] {XMMREG,SAME_AS, 0,RM_XMM,XMMREG,0} ,10036, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMSUBPD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10043, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMSUBPD, 4, new ulong[] {XMMREG,RM_XMM,XMMREG,SAME_AS, 0,0} ,10050, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_FMSUBPS = new[] {
    new itemplate(OCE.FMSUBPS, 4, new ulong[] {XMMREG,SAME_AS, 0,XMMREG,RM_XMM,0} ,10001, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMSUBPS, 4, new ulong[] {XMMREG,SAME_AS, 0,RM_XMM,XMMREG,0} ,10008, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMSUBPS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10015, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMSUBPS, 4, new ulong[] {XMMREG,RM_XMM,XMMREG,SAME_AS, 0,0} ,10022, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_FMSUBSD = new[] {
    new itemplate(OCE.FMSUBSD, 4, new ulong[] {XMMREG,SAME_AS, 0,XMMREG,RM_XMM,0} ,10085, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMSUBSD, 4, new ulong[] {XMMREG,SAME_AS, 0,RM_XMM,XMMREG,0} ,10092, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMSUBSD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10099, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMSUBSD, 4, new ulong[] {XMMREG,RM_XMM,XMMREG,SAME_AS, 0,0} ,10106, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_FMSUBSS = new[] {
    new itemplate(OCE.FMSUBSS, 4, new ulong[] {XMMREG,SAME_AS, 0,XMMREG,RM_XMM,0} ,10057, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMSUBSS, 4, new ulong[] {XMMREG,SAME_AS, 0,RM_XMM,XMMREG,0} ,10064, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMSUBSS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10071, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FMSUBSS, 4, new ulong[] {XMMREG,RM_XMM,XMMREG,SAME_AS, 0,0} ,10078, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_FMUL = new[] {
    new itemplate(OCE.FMUL, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21483, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FMUL, 1, new ulong[] {MEMORY, BITS64,0,0,0,0} ,21487, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FMUL, 1, new ulong[] {FPUREG, TO,0,0,0,0} ,19717, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FMUL, 2, new ulong[] {FPUREG,FPU0,0,0,0} ,19717, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FMUL, 1, new ulong[] {FPUREG,0,0,0,0} ,19722, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FMUL, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19727, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FMUL, 0, new ulong[] {0,0,0,0,0} ,21491, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FMULP = new[] {
    new itemplate(OCE.FMULP, 1, new ulong[] {FPUREG,0,0,0,0} ,19732, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FMULP, 2, new ulong[] {FPUREG,FPU0,0,0,0} ,19732, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FMULP, 0, new ulong[] {0,0,0,0,0} ,21491, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FNCLEX = new[] {
    new itemplate(OCE.FNCLEX, 0, new ulong[] {0,0,0,0,0} ,19523, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FNDISI = new[] {
    new itemplate(OCE.FNDISI, 0, new ulong[] {0,0,0,0,0} ,19648, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FNENI = new[] {
    new itemplate(OCE.FNENI, 0, new ulong[] {0,0,0,0,0} ,19693, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FNINIT = new[] {
    new itemplate(OCE.FNINIT, 0, new ulong[] {0,0,0,0,0} ,19708, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FNMADDPD = new[] {
    new itemplate(OCE.FNMADDPD, 4, new ulong[] {XMMREG,SAME_AS, 0,XMMREG,RM_XMM,0} ,10141, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMADDPD, 4, new ulong[] {XMMREG,SAME_AS, 0,RM_XMM,XMMREG,0} ,10148, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMADDPD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10155, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMADDPD, 4, new ulong[] {XMMREG,RM_XMM,XMMREG,SAME_AS, 0,0} ,10162, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_FNMADDPS = new[] {
    new itemplate(OCE.FNMADDPS, 4, new ulong[] {XMMREG,SAME_AS, 0,XMMREG,RM_XMM,0} ,10113, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMADDPS, 4, new ulong[] {XMMREG,SAME_AS, 0,RM_XMM,XMMREG,0} ,10120, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMADDPS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10127, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMADDPS, 4, new ulong[] {XMMREG,RM_XMM,XMMREG,SAME_AS, 0,0} ,10134, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_FNMADDSD = new[] {
    new itemplate(OCE.FNMADDSD, 4, new ulong[] {XMMREG,SAME_AS, 0,XMMREG,RM_XMM,0} ,10197, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMADDSD, 4, new ulong[] {XMMREG,SAME_AS, 0,RM_XMM,XMMREG,0} ,10204, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMADDSD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10211, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMADDSD, 4, new ulong[] {XMMREG,RM_XMM,XMMREG,SAME_AS, 0,0} ,10218, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_FNMADDSS = new[] {
    new itemplate(OCE.FNMADDSS, 4, new ulong[] {XMMREG,SAME_AS, 0,XMMREG,RM_XMM,0} ,10169, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMADDSS, 4, new ulong[] {XMMREG,SAME_AS, 0,RM_XMM,XMMREG,0} ,10176, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMADDSS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10183, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMADDSS, 4, new ulong[] {XMMREG,RM_XMM,XMMREG,SAME_AS, 0,0} ,10190, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_FNMSUBPD = new[] {
    new itemplate(OCE.FNMSUBPD, 4, new ulong[] {XMMREG,SAME_AS, 0,XMMREG,RM_XMM,0} ,10253, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMSUBPD, 4, new ulong[] {XMMREG,SAME_AS, 0,RM_XMM,XMMREG,0} ,10260, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMSUBPD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10267, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMSUBPD, 4, new ulong[] {XMMREG,RM_XMM,XMMREG,SAME_AS, 0,0} ,10274, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_FNMSUBPS = new[] {
    new itemplate(OCE.FNMSUBPS, 4, new ulong[] {XMMREG,SAME_AS, 0,XMMREG,RM_XMM,0} ,10225, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMSUBPS, 4, new ulong[] {XMMREG,SAME_AS, 0,RM_XMM,XMMREG,0} ,10232, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMSUBPS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10239, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMSUBPS, 4, new ulong[] {XMMREG,RM_XMM,XMMREG,SAME_AS, 0,0} ,10246, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_FNMSUBSD = new[] {
    new itemplate(OCE.FNMSUBSD, 4, new ulong[] {XMMREG,SAME_AS, 0,XMMREG,RM_XMM,0} ,10309, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMSUBSD, 4, new ulong[] {XMMREG,SAME_AS, 0,RM_XMM,XMMREG,0} ,10316, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMSUBSD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10323, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMSUBSD, 4, new ulong[] {XMMREG,RM_XMM,XMMREG,SAME_AS, 0,0} ,10330, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_FNMSUBSS = new[] {
    new itemplate(OCE.FNMSUBSS, 4, new ulong[] {XMMREG,SAME_AS, 0,XMMREG,RM_XMM,0} ,10281, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMSUBSS, 4, new ulong[] {XMMREG,SAME_AS, 0,RM_XMM,XMMREG,0} ,10288, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMSUBSS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10295, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.FNMSUBSS, 4, new ulong[] {XMMREG,RM_XMM,XMMREG,SAME_AS, 0,0} ,10302, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_FNOP = new[] {
    new itemplate(OCE.FNOP, 0, new ulong[] {0,0,0,0,0} ,21495, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FNSAVE = new[] {
    new itemplate(OCE.FNSAVE, 1, new ulong[] {MEMORY,0,0,0,0} ,19738, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FNSTCW = new[] {
    new itemplate(OCE.FNSTCW, 1, new ulong[] {MEMORY,0,0,0,0} ,19748, OF.IF_8086, OF.IF_FPU, OF.IF_SW),
    
};

        static itemplate[] instrux_FNSTENV = new[] {
    new itemplate(OCE.FNSTENV, 1, new ulong[] {MEMORY,0,0,0,0} ,19753, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FNSTSW = new[] {
    new itemplate(OCE.FNSTSW, 1, new ulong[] {MEMORY,0,0,0,0} ,19763, OF.IF_8086, OF.IF_FPU, OF.IF_SW),
    new itemplate(OCE.FNSTSW, 1, new ulong[] {REG_AX,0,0,0,0} ,19768, OF.IF_286, OF.IF_FPU),
    
};

        static itemplate[] instrux_FPATAN = new[] {
    new itemplate(OCE.FPATAN, 0, new ulong[] {0,0,0,0,0} ,21499, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FPREM = new[] {
    new itemplate(OCE.FPREM, 0, new ulong[] {0,0,0,0,0} ,21503, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FPREM1 = new[] {
    new itemplate(OCE.FPREM1, 0, new ulong[] {0,0,0,0,0} ,21507, OF.IF_386, OF.IF_FPU),
    
};

        static itemplate[] instrux_FPTAN = new[] {
    new itemplate(OCE.FPTAN, 0, new ulong[] {0,0,0,0,0} ,21511, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FRCZPD = new[] {
    new itemplate(OCE.FRCZPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10708, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_FRCZPS = new[] {
    new itemplate(OCE.FRCZPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10701, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_FRCZSD = new[] {
    new itemplate(OCE.FRCZSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10722, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_FRCZSS = new[] {
    new itemplate(OCE.FRCZSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10715, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_FRNDINT = new[] {
    new itemplate(OCE.FRNDINT, 0, new ulong[] {0,0,0,0,0} ,21515, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FRSTOR = new[] {
    new itemplate(OCE.FRSTOR, 1, new ulong[] {MEMORY,0,0,0,0} ,21519, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FSAVE = new[] {
    new itemplate(OCE.FSAVE, 1, new ulong[] {MEMORY,0,0,0,0} ,19737, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FSCALE = new[] {
    new itemplate(OCE.FSCALE, 0, new ulong[] {0,0,0,0,0} ,21523, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FSETPM = new[] {
    new itemplate(OCE.FSETPM, 0, new ulong[] {0,0,0,0,0} ,21527, OF.IF_286, OF.IF_FPU),
    
};

        static itemplate[] instrux_FSIN = new[] {
    new itemplate(OCE.FSIN, 0, new ulong[] {0,0,0,0,0} ,21531, OF.IF_386, OF.IF_FPU),
    
};

        static itemplate[] instrux_FSINCOS = new[] {
    new itemplate(OCE.FSINCOS, 0, new ulong[] {0,0,0,0,0} ,21535, OF.IF_386, OF.IF_FPU),
    
};

        static itemplate[] instrux_FSQRT = new[] {
    new itemplate(OCE.FSQRT, 0, new ulong[] {0,0,0,0,0} ,21539, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FST = new[] {
    new itemplate(OCE.FST, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21543, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FST, 1, new ulong[] {MEMORY, BITS64,0,0,0,0} ,21547, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FST, 1, new ulong[] {FPUREG,0,0,0,0} ,19742, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FST, 0, new ulong[] {0,0,0,0,0} ,21551, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FSTCW = new[] {
    new itemplate(OCE.FSTCW, 1, new ulong[] {MEMORY,0,0,0,0} ,19747, OF.IF_8086, OF.IF_FPU, OF.IF_SW),
    
};

        static itemplate[] instrux_FSTENV = new[] {
    new itemplate(OCE.FSTENV, 1, new ulong[] {MEMORY,0,0,0,0} ,19752, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FSTP = new[] {
    new itemplate(OCE.FSTP, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21555, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FSTP, 1, new ulong[] {MEMORY, BITS64,0,0,0,0} ,21559, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FSTP, 1, new ulong[] {MEMORY, BITS80,0,0,0,0} ,21563, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FSTP, 1, new ulong[] {FPUREG,0,0,0,0} ,19757, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FSTP, 0, new ulong[] {0,0,0,0,0} ,21567, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FSTSW = new[] {
    new itemplate(OCE.FSTSW, 1, new ulong[] {MEMORY,0,0,0,0} ,19762, OF.IF_8086, OF.IF_FPU, OF.IF_SW),
    new itemplate(OCE.FSTSW, 1, new ulong[] {REG_AX,0,0,0,0} ,19767, OF.IF_286, OF.IF_FPU),
    
};

        static itemplate[] instrux_FSUB = new[] {
    new itemplate(OCE.FSUB, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21571, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FSUB, 1, new ulong[] {MEMORY, BITS64,0,0,0,0} ,21575, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FSUB, 1, new ulong[] {FPUREG, TO,0,0,0,0} ,19772, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FSUB, 2, new ulong[] {FPUREG,FPU0,0,0,0} ,19772, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FSUB, 1, new ulong[] {FPUREG,0,0,0,0} ,19777, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FSUB, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19782, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FSUB, 0, new ulong[] {0,0,0,0,0} ,21579, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FSUBP = new[] {
    new itemplate(OCE.FSUBP, 1, new ulong[] {FPUREG,0,0,0,0} ,19787, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FSUBP, 2, new ulong[] {FPUREG,FPU0,0,0,0} ,19787, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FSUBP, 0, new ulong[] {0,0,0,0,0} ,21579, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FSUBR = new[] {
    new itemplate(OCE.FSUBR, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,21583, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FSUBR, 1, new ulong[] {MEMORY, BITS64,0,0,0,0} ,21587, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FSUBR, 1, new ulong[] {FPUREG, TO,0,0,0,0} ,19792, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FSUBR, 2, new ulong[] {FPUREG,FPU0,0,0,0} ,19792, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FSUBR, 1, new ulong[] {FPUREG,0,0,0,0} ,19797, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FSUBR, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19802, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FSUBR, 0, new ulong[] {0,0,0,0,0} ,21591, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FSUBRP = new[] {
    new itemplate(OCE.FSUBRP, 1, new ulong[] {FPUREG,0,0,0,0} ,19807, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FSUBRP, 2, new ulong[] {FPUREG,FPU0,0,0,0} ,19807, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FSUBRP, 0, new ulong[] {0,0,0,0,0} ,21591, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FTST = new[] {
    new itemplate(OCE.FTST, 0, new ulong[] {0,0,0,0,0} ,21595, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FUCOM = new[] {
    new itemplate(OCE.FUCOM, 1, new ulong[] {FPUREG,0,0,0,0} ,19812, OF.IF_386, OF.IF_FPU),
    new itemplate(OCE.FUCOM, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19817, OF.IF_386, OF.IF_FPU),
    new itemplate(OCE.FUCOM, 0, new ulong[] {0,0,0,0,0} ,21599, OF.IF_386, OF.IF_FPU),
    
};

        static itemplate[] instrux_FUCOMI = new[] {
    new itemplate(OCE.FUCOMI, 1, new ulong[] {FPUREG,0,0,0,0} ,19822, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FUCOMI, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19827, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FUCOMI, 0, new ulong[] {0,0,0,0,0} ,21603, OF.IF_P6, OF.IF_FPU),
    
};

        static itemplate[] instrux_FUCOMIP = new[] {
    new itemplate(OCE.FUCOMIP, 1, new ulong[] {FPUREG,0,0,0,0} ,19832, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FUCOMIP, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19837, OF.IF_P6, OF.IF_FPU),
    new itemplate(OCE.FUCOMIP, 0, new ulong[] {0,0,0,0,0} ,21607, OF.IF_P6, OF.IF_FPU),
    
};

        static itemplate[] instrux_FUCOMP = new[] {
    new itemplate(OCE.FUCOMP, 1, new ulong[] {FPUREG,0,0,0,0} ,19842, OF.IF_386, OF.IF_FPU),
    new itemplate(OCE.FUCOMP, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19847, OF.IF_386, OF.IF_FPU),
    new itemplate(OCE.FUCOMP, 0, new ulong[] {0,0,0,0,0} ,21611, OF.IF_386, OF.IF_FPU),
    
};

        static itemplate[] instrux_FUCOMPP = new[] {
    new itemplate(OCE.FUCOMPP, 0, new ulong[] {0,0,0,0,0} ,21615, OF.IF_386, OF.IF_FPU),
    
};

        static itemplate[] instrux_FWAIT = new[] {
    new itemplate(OCE.FWAIT, 0, new ulong[] {0,0,0,0,0} ,21601, OF.IF_8086),
    
};

        static itemplate[] instrux_FXAM = new[] {
    new itemplate(OCE.FXAM, 0, new ulong[] {0,0,0,0,0} ,21619, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FXCH = new[] {
    new itemplate(OCE.FXCH, 1, new ulong[] {FPUREG,0,0,0,0} ,19852, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FXCH, 2, new ulong[] {FPUREG,FPU0,0,0,0} ,19852, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FXCH, 2, new ulong[] {FPU0,FPUREG,0,0,0} ,19857, OF.IF_8086, OF.IF_FPU),
    new itemplate(OCE.FXCH, 0, new ulong[] {0,0,0,0,0} ,21623, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FXRSTOR = new[] {
    new itemplate(OCE.FXRSTOR, 1, new ulong[] {MEMORY,0,0,0,0} ,20982, OF.IF_P6, OF.IF_SSE, OF.IF_FPU),
    
};

        static itemplate[] instrux_FXSAVE = new[] {
    new itemplate(OCE.FXSAVE, 1, new ulong[] {MEMORY,0,0,0,0} ,20987, OF.IF_P6, OF.IF_SSE, OF.IF_FPU),
    
};

        static itemplate[] instrux_FXTRACT = new[] {
    new itemplate(OCE.FXTRACT, 0, new ulong[] {0,0,0,0,0} ,21627, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FYL2X = new[] {
    new itemplate(OCE.FYL2X, 0, new ulong[] {0,0,0,0,0} ,21631, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_FYL2XP1 = new[] {
    new itemplate(OCE.FYL2XP1, 0, new ulong[] {0,0,0,0,0} ,21635, OF.IF_8086, OF.IF_FPU),
    
};

        static itemplate[] instrux_GETSEC = new[] {
    new itemplate(OCE.GETSEC, 0, new ulong[] {0,0,0,0,0} ,22091, OF.IF_KATMAI),
    
};

        static itemplate[] instrux_HADDPD = new[] {
    new itemplate(OCE.HADDPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17889, OF.IF_PRESCOTT, OF.IF_SSE3, OF.IF_SO),
    
};

        static itemplate[] instrux_HADDPS = new[] {
    new itemplate(OCE.HADDPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17895, OF.IF_PRESCOTT, OF.IF_SSE3, OF.IF_SO),
    
};

        static itemplate[] instrux_HINT_NOP0 = new[] {
    new itemplate(OCE.HINT_NOP0, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18033, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP0, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18039, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP0, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18045, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP1 = new[] {
    new itemplate(OCE.HINT_NOP1, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18051, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP1, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18057, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP1, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18063, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP10 = new[] {
    new itemplate(OCE.HINT_NOP10, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18213, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP10, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18219, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP10, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18225, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP11 = new[] {
    new itemplate(OCE.HINT_NOP11, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18231, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP11, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18237, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP11, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18243, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP12 = new[] {
    new itemplate(OCE.HINT_NOP12, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18249, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP12, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18255, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP12, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18261, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP13 = new[] {
    new itemplate(OCE.HINT_NOP13, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18267, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP13, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18273, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP13, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18279, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP14 = new[] {
    new itemplate(OCE.HINT_NOP14, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18285, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP14, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18291, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP14, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18297, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP15 = new[] {
    new itemplate(OCE.HINT_NOP15, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18303, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP15, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18309, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP15, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18315, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP16 = new[] {
    new itemplate(OCE.HINT_NOP16, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18321, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP16, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18327, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP16, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18333, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP17 = new[] {
    new itemplate(OCE.HINT_NOP17, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18339, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP17, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18345, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP17, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18351, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP18 = new[] {
    new itemplate(OCE.HINT_NOP18, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18357, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP18, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18363, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP18, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18369, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP19 = new[] {
    new itemplate(OCE.HINT_NOP19, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18375, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP19, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18381, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP19, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18387, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP2 = new[] {
    new itemplate(OCE.HINT_NOP2, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18069, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP2, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18075, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP2, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18081, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP20 = new[] {
    new itemplate(OCE.HINT_NOP20, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18393, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP20, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18399, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP20, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18405, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP21 = new[] {
    new itemplate(OCE.HINT_NOP21, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18411, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP21, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18417, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP21, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18423, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP22 = new[] {
    new itemplate(OCE.HINT_NOP22, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18429, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP22, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18435, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP22, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18441, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP23 = new[] {
    new itemplate(OCE.HINT_NOP23, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18447, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP23, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18453, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP23, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18459, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP24 = new[] {
    new itemplate(OCE.HINT_NOP24, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18465, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP24, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18471, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP24, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18477, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP25 = new[] {
    new itemplate(OCE.HINT_NOP25, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18483, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP25, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18489, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP25, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18495, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP26 = new[] {
    new itemplate(OCE.HINT_NOP26, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18501, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP26, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18507, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP26, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18513, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP27 = new[] {
    new itemplate(OCE.HINT_NOP27, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18519, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP27, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18525, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP27, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18531, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP28 = new[] {
    new itemplate(OCE.HINT_NOP28, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18537, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP28, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18543, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP28, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18549, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP29 = new[] {
    new itemplate(OCE.HINT_NOP29, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18555, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP29, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18561, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP29, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18567, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP3 = new[] {
    new itemplate(OCE.HINT_NOP3, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18087, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP3, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18093, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP3, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18099, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP30 = new[] {
    new itemplate(OCE.HINT_NOP30, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18573, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP30, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18579, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP30, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18585, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP31 = new[] {
    new itemplate(OCE.HINT_NOP31, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18591, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP31, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18597, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP31, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18603, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP32 = new[] {
    new itemplate(OCE.HINT_NOP32, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18609, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP32, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18615, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP32, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18621, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP33 = new[] {
    new itemplate(OCE.HINT_NOP33, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18627, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP33, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18633, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP33, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18639, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP34 = new[] {
    new itemplate(OCE.HINT_NOP34, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18645, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP34, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18651, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP34, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18657, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP35 = new[] {
    new itemplate(OCE.HINT_NOP35, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18663, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP35, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18669, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP35, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18675, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP36 = new[] {
    new itemplate(OCE.HINT_NOP36, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18681, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP36, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18687, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP36, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18693, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP37 = new[] {
    new itemplate(OCE.HINT_NOP37, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18699, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP37, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18705, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP37, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18711, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP38 = new[] {
    new itemplate(OCE.HINT_NOP38, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18717, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP38, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18723, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP38, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18729, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP39 = new[] {
    new itemplate(OCE.HINT_NOP39, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18735, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP39, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18741, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP39, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18747, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP4 = new[] {
    new itemplate(OCE.HINT_NOP4, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18105, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP4, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18111, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP4, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18117, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP40 = new[] {
    new itemplate(OCE.HINT_NOP40, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18753, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP40, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18759, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP40, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18765, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP41 = new[] {
    new itemplate(OCE.HINT_NOP41, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18771, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP41, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18777, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP41, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18783, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP42 = new[] {
    new itemplate(OCE.HINT_NOP42, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18789, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP42, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18795, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP42, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18801, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP43 = new[] {
    new itemplate(OCE.HINT_NOP43, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18807, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP43, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18813, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP43, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18819, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP44 = new[] {
    new itemplate(OCE.HINT_NOP44, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18825, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP44, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18831, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP44, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18837, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP45 = new[] {
    new itemplate(OCE.HINT_NOP45, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18843, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP45, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18849, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP45, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18855, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP46 = new[] {
    new itemplate(OCE.HINT_NOP46, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18861, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP46, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18867, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP46, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18873, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP47 = new[] {
    new itemplate(OCE.HINT_NOP47, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18879, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP47, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18885, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP47, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18891, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP48 = new[] {
    new itemplate(OCE.HINT_NOP48, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18897, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP48, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18903, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP48, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18909, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP49 = new[] {
    new itemplate(OCE.HINT_NOP49, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18915, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP49, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18921, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP49, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18927, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP5 = new[] {
    new itemplate(OCE.HINT_NOP5, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18123, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP5, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18129, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP5, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18135, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP50 = new[] {
    new itemplate(OCE.HINT_NOP50, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18933, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP50, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18939, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP50, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18945, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP51 = new[] {
    new itemplate(OCE.HINT_NOP51, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18951, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP51, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18957, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP51, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18963, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP52 = new[] {
    new itemplate(OCE.HINT_NOP52, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18969, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP52, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18975, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP52, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18981, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP53 = new[] {
    new itemplate(OCE.HINT_NOP53, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18987, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP53, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18993, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP53, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18999, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP54 = new[] {
    new itemplate(OCE.HINT_NOP54, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,19005, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP54, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,19011, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP54, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,19017, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP55 = new[] {
    new itemplate(OCE.HINT_NOP55, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,19023, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP55, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,19029, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP55, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,19035, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP56 = new[] {
    new itemplate(OCE.HINT_NOP56, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,16395, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP56, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,16401, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP56, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,16407, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP57 = new[] {
    new itemplate(OCE.HINT_NOP57, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,19041, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP57, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,19047, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP57, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,19053, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP58 = new[] {
    new itemplate(OCE.HINT_NOP58, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,19059, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP58, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,19065, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP58, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,19071, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP59 = new[] {
    new itemplate(OCE.HINT_NOP59, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,19077, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP59, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,19083, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP59, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,19089, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP6 = new[] {
    new itemplate(OCE.HINT_NOP6, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18141, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP6, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18147, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP6, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18153, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP60 = new[] {
    new itemplate(OCE.HINT_NOP60, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,19095, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP60, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,19101, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP60, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,19107, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP61 = new[] {
    new itemplate(OCE.HINT_NOP61, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,19113, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP61, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,19119, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP61, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,19125, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP62 = new[] {
    new itemplate(OCE.HINT_NOP62, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,19131, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP62, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,19137, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP62, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,19143, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP63 = new[] {
    new itemplate(OCE.HINT_NOP63, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,19149, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP63, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,19155, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP63, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,19161, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP7 = new[] {
    new itemplate(OCE.HINT_NOP7, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18159, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP7, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18165, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP7, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18171, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP8 = new[] {
    new itemplate(OCE.HINT_NOP8, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18177, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP8, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18183, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP8, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18189, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HINT_NOP9 = new[] {
    new itemplate(OCE.HINT_NOP9, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,18195, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP9, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,18201, OF.IF_P6, OF.IF_UNDOC),
    new itemplate(OCE.HINT_NOP9, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,18207, OF.IF_X64, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_HLT = new[] {
    new itemplate(OCE.HLT, 0, new ulong[] {0,0,0,0,0} ,22116, OF.IF_8086, OF.IF_PRIV),
    
};

        static itemplate[] instrux_HSUBPD = new[] {
    new itemplate(OCE.HSUBPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17901, OF.IF_PRESCOTT, OF.IF_SSE3, OF.IF_SO),
    
};

        static itemplate[] instrux_HSUBPS = new[] {
    new itemplate(OCE.HSUBPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17907, OF.IF_PRESCOTT, OF.IF_SSE3, OF.IF_SO),
    
};

        static itemplate[] instrux_IBTS = new[] {
    new itemplate(OCE.IBTS, 2, new ulong[] {MEMORY,REG16,0,0,0} ,15975, OF.IF_386, OF.IF_SW, OF.IF_UNDOC),
    new itemplate(OCE.IBTS, 2, new ulong[] {REG16,REG16,0,0,0} ,15975, OF.IF_386, OF.IF_UNDOC),
    new itemplate(OCE.IBTS, 2, new ulong[] {MEMORY,REG32,0,0,0} ,15981, OF.IF_386, OF.IF_SD, OF.IF_UNDOC),
    new itemplate(OCE.IBTS, 2, new ulong[] {REG32,REG32,0,0,0} ,15981, OF.IF_386, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_ICEBP = new[] {
    new itemplate(OCE.ICEBP, 0, new ulong[] {0,0,0,0,0} ,22119, OF.IF_386),
    
};

        static itemplate[] instrux_IDIV = new[] {
    new itemplate(OCE.IDIV, 1, new ulong[] {RM_GPR, BITS8,0,0,0,0} ,21639, OF.IF_8086),
    new itemplate(OCE.IDIV, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,19862, OF.IF_8086),
    new itemplate(OCE.IDIV, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,19867, OF.IF_386),
    new itemplate(OCE.IDIV, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,19872, OF.IF_X64),
    
};

        static itemplate[] instrux_IMUL = new[] {
    new itemplate(OCE.IMUL, 1, new ulong[] {RM_GPR, BITS8,0,0,0,0} ,21643, OF.IF_8086),
    new itemplate(OCE.IMUL, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,19877, OF.IF_8086),
    new itemplate(OCE.IMUL, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,19882, OF.IF_386),
    new itemplate(OCE.IMUL, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,19887, OF.IF_X64),
    new itemplate(OCE.IMUL, 2, new ulong[] {REG16,MEMORY,0,0,0} ,15993, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.IMUL, 2, new ulong[] {REG16,REG16,0,0,0} ,15993, OF.IF_386),
    new itemplate(OCE.IMUL, 2, new ulong[] {REG32,MEMORY,0,0,0} ,15999, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.IMUL, 2, new ulong[] {REG32,REG32,0,0,0} ,15999, OF.IF_386),
    new itemplate(OCE.IMUL, 2, new ulong[] {REG64,MEMORY,0,0,0} ,16005, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.IMUL, 2, new ulong[] {REG64,REG64,0,0,0} ,16005, OF.IF_X64),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG16,MEMORY,IMMEDIATE, BITS8,0,0} ,16011, OF.IF_186, OF.IF_SM),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG16,MEMORY,SBYTE16,0,0} ,16011, OF.IF_186, OF.IF_SM),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG16,MEMORY,IMMEDIATE, BITS16,0,0} ,16017, OF.IF_186, OF.IF_SM),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG16,MEMORY,IMMEDIATE,0,0} ,16023, OF.IF_186, OF.IF_SM),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG16,REG16,IMMEDIATE, BITS8,0,0} ,16011, OF.IF_186),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG16,REG16,SBYTE32,0,0} ,16011, OF.IF_186, OF.IF_SM),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG16,REG16,IMMEDIATE, BITS16,0,0} ,16017, OF.IF_186),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG16,REG16,IMMEDIATE,0,0} ,16023, OF.IF_186, OF.IF_SM),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG32,MEMORY,IMMEDIATE, BITS8,0,0} ,16029, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG32,MEMORY,SBYTE64,0,0} ,16029, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG32,MEMORY,IMMEDIATE, BITS32,0,0} ,16035, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG32,MEMORY,IMMEDIATE,0,0} ,16041, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG32,REG32,IMMEDIATE, BITS8,0,0} ,16029, OF.IF_386),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG32,REG32,SBYTE16,0,0} ,16029, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG32,REG32,IMMEDIATE, BITS32,0,0} ,16035, OF.IF_386),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG32,REG32,IMMEDIATE,0,0} ,16041, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG64,MEMORY,IMMEDIATE, BITS8,0,0} ,16047, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG64,MEMORY,SBYTE32,0,0} ,16047, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG64,MEMORY,IMMEDIATE, BITS32,0,0} ,16053, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG64,MEMORY,IMMEDIATE,0,0} ,16059, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG64,REG64,IMMEDIATE, BITS8,0,0} ,16047, OF.IF_X64),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG64,REG64,SBYTE64,0,0} ,16047, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG64,REG64,IMMEDIATE, BITS32,0,0} ,16053, OF.IF_X64),
    new itemplate(OCE.IMUL, 3, new ulong[] {REG64,REG64,IMMEDIATE,0,0} ,16059, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.IMUL, 2, new ulong[] {REG16,IMMEDIATE, BITS8,0,0,0} ,16065, OF.IF_186),
    new itemplate(OCE.IMUL, 2, new ulong[] {REG16,SBYTE16,0,0,0} ,16065, OF.IF_186, OF.IF_SM),
    new itemplate(OCE.IMUL, 2, new ulong[] {REG16,IMMEDIATE, BITS16,0,0,0} ,16071, OF.IF_186),
    new itemplate(OCE.IMUL, 2, new ulong[] {REG16,IMMEDIATE,0,0,0} ,16077, OF.IF_186, OF.IF_SM),
    new itemplate(OCE.IMUL, 2, new ulong[] {REG32,IMMEDIATE, BITS8,0,0,0} ,16083, OF.IF_386),
    new itemplate(OCE.IMUL, 2, new ulong[] {REG32,SBYTE32,0,0,0} ,16083, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.IMUL, 2, new ulong[] {REG32,IMMEDIATE, BITS32,0,0,0} ,16089, OF.IF_386),
    new itemplate(OCE.IMUL, 2, new ulong[] {REG32,IMMEDIATE,0,0,0} ,16095, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.IMUL, 2, new ulong[] {REG64,IMMEDIATE, BITS8,0,0,0} ,16101, OF.IF_X64),
    new itemplate(OCE.IMUL, 2, new ulong[] {REG64,SBYTE64,0,0,0} ,16101, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.IMUL, 2, new ulong[] {REG64,IMMEDIATE, BITS32,0,0,0} ,16107, OF.IF_X64),
    new itemplate(OCE.IMUL, 2, new ulong[] {REG64,IMMEDIATE,0,0,0} ,16113, OF.IF_X64, OF.IF_SM),
    
};

        static itemplate[] instrux_IN = new[] {
    new itemplate(OCE.IN, 2, new ulong[] {REG_AL,IMMEDIATE,0,0,0} ,21647, OF.IF_8086, OF.IF_SB),
    new itemplate(OCE.IN, 2, new ulong[] {REG_AX,IMMEDIATE,0,0,0} ,19892, OF.IF_8086, OF.IF_SB),
    new itemplate(OCE.IN, 2, new ulong[] {REG_EAX,IMMEDIATE,0,0,0} ,19897, OF.IF_386, OF.IF_SB),
    new itemplate(OCE.IN, 2, new ulong[] {REG_AL,REG_DX,0,0,0} ,22122, OF.IF_8086),
    new itemplate(OCE.IN, 2, new ulong[] {REG_AX,REG_DX,0,0,0} ,21651, OF.IF_8086),
    new itemplate(OCE.IN, 2, new ulong[] {REG_EAX,REG_DX,0,0,0} ,21655, OF.IF_386),
    
};

        static itemplate[] instrux_INC = new[] {
    new itemplate(OCE.INC, 1, new ulong[] {REG16,0,0,0,0} ,21659, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.INC, 1, new ulong[] {REG32,0,0,0,0} ,21663, OF.IF_386, OF.IF_NOLONG),
    new itemplate(OCE.INC, 1, new ulong[] {RM_GPR, BITS8,0,0,0,0} ,21667, OF.IF_8086),
    new itemplate(OCE.INC, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,19902, OF.IF_8086),
    new itemplate(OCE.INC, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,19907, OF.IF_386),
    new itemplate(OCE.INC, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,19912, OF.IF_X64),
    
};

        static itemplate[] instrux_INCBIN;// = new[] {};

        static itemplate[] instrux_INSB = new[] {
    new itemplate(OCE.INSB, 0, new ulong[] {0,0,0,0,0} ,22125, OF.IF_186),
    
};

        static itemplate[] instrux_INSD = new[] {
    new itemplate(OCE.INSD, 0, new ulong[] {0,0,0,0,0} ,21671, OF.IF_386),
    
};

        static itemplate[] instrux_INSERTPS = new[] {
    new itemplate(OCE.INSERTPS, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7124, OF.IF_SSE41, OF.IF_SD),
    
};

        static itemplate[] instrux_INSERTQ = new[] {
    new itemplate(OCE.INSERTQ, 4, new ulong[] {XMMREG,XMMREG,IMMEDIATE,IMMEDIATE,0} ,7084, OF.IF_SSE4A, OF.IF_AMD),
    new itemplate(OCE.INSERTQ, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,17955, OF.IF_SSE4A, OF.IF_AMD),
    
};

        static itemplate[] instrux_INSW = new[] {
    new itemplate(OCE.INSW, 0, new ulong[] {0,0,0,0,0} ,21675, OF.IF_186),
    
};

        static itemplate[] instrux_INT = new[] {
    new itemplate(OCE.INT, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,21679, OF.IF_8086, OF.IF_SB),
    
};

        static itemplate[] instrux_INT01 = new[] {
    new itemplate(OCE.INT01, 0, new ulong[] {0,0,0,0,0} ,22119, OF.IF_386),
    
};

        static itemplate[] instrux_INT03 = new[] {
    new itemplate(OCE.INT03, 0, new ulong[] {0,0,0,0,0} ,22128, OF.IF_8086),
    
};

        static itemplate[] instrux_INT1 = new[] {
    new itemplate(OCE.INT1, 0, new ulong[] {0,0,0,0,0} ,22119, OF.IF_386),
    
};

        static itemplate[] instrux_INT3 = new[] {
    new itemplate(OCE.INT3, 0, new ulong[] {0,0,0,0,0} ,22128, OF.IF_8086),
    
};

        static itemplate[] instrux_INTO = new[] {
    new itemplate(OCE.INTO, 0, new ulong[] {0,0,0,0,0} ,22131, OF.IF_8086, OF.IF_NOLONG),
    
};

        static itemplate[] instrux_INVD = new[] {
    new itemplate(OCE.INVD, 0, new ulong[] {0,0,0,0,0} ,21683, OF.IF_486, OF.IF_PRIV),
    
};

        static itemplate[] instrux_INVEPT = new[] {
    new itemplate(OCE.INVEPT, 2, new ulong[] {REG32,MEMORY,0,0,0} ,7045, OF.IF_VMX, OF.IF_SO, OF.IF_NOLONG),
    new itemplate(OCE.INVEPT, 2, new ulong[] {REG64,MEMORY,0,0,0} ,7044, OF.IF_VMX, OF.IF_SO, OF.IF_LONG),
    
};

        static itemplate[] instrux_INVLPG = new[] {
    new itemplate(OCE.INVLPG, 1, new ulong[] {MEMORY,0,0,0,0} ,19917, OF.IF_486, OF.IF_PRIV),
    
};

        static itemplate[] instrux_INVLPGA = new[] {
    new itemplate(OCE.INVLPGA, 2, new ulong[] {REG_AX,REG_ECX,0,0,0} ,16119, OF.IF_X86_64, OF.IF_AMD, OF.IF_NOLONG),
    new itemplate(OCE.INVLPGA, 2, new ulong[] {REG_EAX,REG_ECX,0,0,0} ,16125, OF.IF_X86_64, OF.IF_AMD),
    new itemplate(OCE.INVLPGA, 2, new ulong[] {REG_RAX,REG_ECX,0,0,0} ,8552, OF.IF_X64, OF.IF_AMD),
    new itemplate(OCE.INVLPGA, 0, new ulong[] {0,0,0,0,0} ,16126, OF.IF_X86_64, OF.IF_AMD),
    
};

        static itemplate[] instrux_INVVPID = new[] {
    new itemplate(OCE.INVVPID, 2, new ulong[] {REG32,MEMORY,0,0,0} ,7053, OF.IF_VMX, OF.IF_SO, OF.IF_NOLONG),
    new itemplate(OCE.INVVPID, 2, new ulong[] {REG64,MEMORY,0,0,0} ,7052, OF.IF_VMX, OF.IF_SO, OF.IF_LONG),
    
};

        static itemplate[] instrux_IRET = new[] {
    new itemplate(OCE.IRET, 0, new ulong[] {0,0,0,0,0} ,21687, OF.IF_8086),
    
};

        static itemplate[] instrux_IRETD = new[] {
    new itemplate(OCE.IRETD, 0, new ulong[] {0,0,0,0,0} ,21691, OF.IF_386),
    
};

        static itemplate[] instrux_IRETQ = new[] {
    new itemplate(OCE.IRETQ, 0, new ulong[] {0,0,0,0,0} ,21695, OF.IF_X64),
    
};

        static itemplate[] instrux_IRETW = new[] {
    new itemplate(OCE.IRETW, 0, new ulong[] {0,0,0,0,0} ,21699, OF.IF_8086),
    
};

        static itemplate[] instrux_JCXZ = new[] {
    new itemplate(OCE.JCXZ, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,19922, OF.IF_8086, OF.IF_NOLONG),
    
};

        static itemplate[] instrux_JECXZ = new[] {
    new itemplate(OCE.JECXZ, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,19927, OF.IF_386),
    
};

        static itemplate[] instrux_JMP = new[] {
    new itemplate(OCE.JMP, 1, new ulong[] {IMMEDIATE, SHORT,0,0,0,0} ,19938, OF.IF_8086),
    new itemplate(OCE.JMP, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,19937, OF.IF_8086),
    new itemplate(OCE.JMP, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,19942, OF.IF_8086),
    new itemplate(OCE.JMP, 1, new ulong[] {IMMEDIATE, NEAR,0,0,0,0} ,19942, OF.IF_8086),
    new itemplate(OCE.JMP, 1, new ulong[] {IMMEDIATE, FAR,0,0,0,0} ,16131, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.JMP, 1, new ulong[] {IMMEDIATE, BITS16,0,0,0,0} ,19947, OF.IF_8086),
    new itemplate(OCE.JMP, 1, new ulong[] {IMMEDIATE, BITS16, NEAR,0,0,0,0} ,19947, OF.IF_8086),
    new itemplate(OCE.JMP, 1, new ulong[] {IMMEDIATE, BITS16, FAR,0,0,0,0} ,16137, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.JMP, 1, new ulong[] {IMMEDIATE, BITS32,0,0,0,0} ,19952, OF.IF_386),
    new itemplate(OCE.JMP, 1, new ulong[] {IMMEDIATE, BITS32, NEAR,0,0,0,0} ,19952, OF.IF_386),
    new itemplate(OCE.JMP, 1, new ulong[] {IMMEDIATE, BITS32, FAR,0,0,0,0} ,16143, OF.IF_386, OF.IF_NOLONG),
    new itemplate(OCE.JMP, 2, new ulong[] {IMMEDIATE, COLON,IMMEDIATE,0,0,0} ,16149, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.JMP, 2, new ulong[] {IMMEDIATE, BITS16, COLON,IMMEDIATE,0,0,0} ,16155, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.JMP, 2, new ulong[] {IMMEDIATE, COLON,IMMEDIATE, BITS16,0,0,0} ,16155, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.JMP, 2, new ulong[] {IMMEDIATE, BITS32, COLON,IMMEDIATE,0,0,0} ,16161, OF.IF_386, OF.IF_NOLONG),
    new itemplate(OCE.JMP, 2, new ulong[] {IMMEDIATE, COLON,IMMEDIATE, BITS32,0,0,0} ,16161, OF.IF_386, OF.IF_NOLONG),
    new itemplate(OCE.JMP, 1, new ulong[] {MEMORY, FAR,0,0,0,0} ,19957, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.JMP, 1, new ulong[] {MEMORY, FAR,0,0,0,0} ,19962, OF.IF_X64),
    new itemplate(OCE.JMP, 1, new ulong[] {MEMORY, BITS16, FAR,0,0,0,0} ,19967, OF.IF_8086),
    new itemplate(OCE.JMP, 1, new ulong[] {MEMORY, BITS32, FAR,0,0,0,0} ,19972, OF.IF_386),
    new itemplate(OCE.JMP, 1, new ulong[] {MEMORY, BITS64, FAR,0,0,0,0} ,19962, OF.IF_X64),
    new itemplate(OCE.JMP, 1, new ulong[] {MEMORY, NEAR,0,0,0,0} ,19977, OF.IF_8086),
    new itemplate(OCE.JMP, 1, new ulong[] {MEMORY, BITS16, NEAR,0,0,0,0} ,19982, OF.IF_8086),
    new itemplate(OCE.JMP, 1, new ulong[] {MEMORY, BITS32, NEAR,0,0,0,0} ,19987, OF.IF_386, OF.IF_NOLONG),
    new itemplate(OCE.JMP, 1, new ulong[] {MEMORY, BITS64, NEAR,0,0,0,0} ,19992, OF.IF_X64),
    new itemplate(OCE.JMP, 1, new ulong[] {REG16,0,0,0,0} ,19982, OF.IF_8086),
    new itemplate(OCE.JMP, 1, new ulong[] {REG32,0,0,0,0} ,19987, OF.IF_386, OF.IF_NOLONG),
    new itemplate(OCE.JMP, 1, new ulong[] {REG64,0,0,0,0} ,19992, OF.IF_X64),
    new itemplate(OCE.JMP, 1, new ulong[] {MEMORY,0,0,0,0} ,19977, OF.IF_8086),
    new itemplate(OCE.JMP, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,19982, OF.IF_8086),
    new itemplate(OCE.JMP, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,19987, OF.IF_386, OF.IF_NOLONG),
    new itemplate(OCE.JMP, 1, new ulong[] {MEMORY, BITS64,0,0,0,0} ,19992, OF.IF_X64),
    
};

        static itemplate[] instrux_JMPE = new[] {
    new itemplate(OCE.JMPE, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,16167, OF.IF_IA64),
    new itemplate(OCE.JMPE, 1, new ulong[] {IMMEDIATE, BITS16,0,0,0,0} ,16173, OF.IF_IA64),
    new itemplate(OCE.JMPE, 1, new ulong[] {IMMEDIATE, BITS32,0,0,0,0} ,16179, OF.IF_IA64),
    new itemplate(OCE.JMPE, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,16185, OF.IF_IA64),
    new itemplate(OCE.JMPE, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,16191, OF.IF_IA64),
    
};

        static itemplate[] instrux_JRCXZ = new[] {
    new itemplate(OCE.JRCXZ, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,19932, OF.IF_X64),
    
};

        static itemplate[] instrux_LAHF = new[] {
    new itemplate(OCE.LAHF, 0, new ulong[] {0,0,0,0,0} ,22134, OF.IF_8086),
    
};

        static itemplate[] instrux_LAR = new[] {
    new itemplate(OCE.LAR, 2, new ulong[] {REG16,MEMORY,0,0,0} ,16197, OF.IF_286, OF.IF_PROT, OF.IF_SW),
    new itemplate(OCE.LAR, 2, new ulong[] {REG16,REG16,0,0,0} ,16197, OF.IF_286, OF.IF_PROT),
    new itemplate(OCE.LAR, 2, new ulong[] {REG16,REG32,0,0,0} ,16197, OF.IF_386, OF.IF_PROT),
    new itemplate(OCE.LAR, 2, new ulong[] {REG16,REG64,0,0,0} ,8559, OF.IF_X64, OF.IF_PROT),
    new itemplate(OCE.LAR, 2, new ulong[] {REG32,MEMORY,0,0,0} ,16203, OF.IF_386, OF.IF_PROT, OF.IF_SW),
    new itemplate(OCE.LAR, 2, new ulong[] {REG32,REG16,0,0,0} ,16203, OF.IF_386, OF.IF_PROT),
    new itemplate(OCE.LAR, 2, new ulong[] {REG32,REG32,0,0,0} ,16203, OF.IF_386, OF.IF_PROT),
    new itemplate(OCE.LAR, 2, new ulong[] {REG32,REG64,0,0,0} ,8566, OF.IF_X64, OF.IF_PROT),
    new itemplate(OCE.LAR, 2, new ulong[] {REG64,MEMORY,0,0,0} ,16209, OF.IF_X64, OF.IF_PROT, OF.IF_SW),
    new itemplate(OCE.LAR, 2, new ulong[] {REG64,REG16,0,0,0} ,16209, OF.IF_X64, OF.IF_PROT),
    new itemplate(OCE.LAR, 2, new ulong[] {REG64,REG32,0,0,0} ,16209, OF.IF_X64, OF.IF_PROT),
    new itemplate(OCE.LAR, 2, new ulong[] {REG64,REG64,0,0,0} ,16209, OF.IF_X64, OF.IF_PROT),
    
};

        static itemplate[] instrux_LDDQU = new[] {
    new itemplate(OCE.LDDQU, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,17913, OF.IF_PRESCOTT, OF.IF_SSE3, OF.IF_SO),
    
};

        static itemplate[] instrux_LDMXCSR = new[] {
    new itemplate(OCE.LDMXCSR, 1, new ulong[] {MEMORY,0,0,0,0} ,20972, OF.IF_KATMAI, OF.IF_SSE, OF.IF_SD),
    
};

        static itemplate[] instrux_LDS = new[] {
    new itemplate(OCE.LDS, 2, new ulong[] {REG16,MEMORY,0,0,0} ,19997, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.LDS, 2, new ulong[] {REG32,MEMORY,0,0,0} ,20002, OF.IF_386, OF.IF_NOLONG),
    
};

        static itemplate[] instrux_LEA = new[] {
    new itemplate(OCE.LEA, 2, new ulong[] {REG16,MEMORY,0,0,0} ,20007, OF.IF_8086),
    new itemplate(OCE.LEA, 2, new ulong[] {REG32,MEMORY,0,0,0} ,20012, OF.IF_386),
    new itemplate(OCE.LEA, 2, new ulong[] {REG64,MEMORY,0,0,0} ,20017, OF.IF_X64),
    
};

        static itemplate[] instrux_LEAVE = new[] {
    new itemplate(OCE.LEAVE, 0, new ulong[] {0,0,0,0,0} ,20254, OF.IF_186),
    
};

        static itemplate[] instrux_LES = new[] {
    new itemplate(OCE.LES, 2, new ulong[] {REG16,MEMORY,0,0,0} ,20022, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.LES, 2, new ulong[] {REG32,MEMORY,0,0,0} ,20027, OF.IF_386, OF.IF_NOLONG),
    
};

        static itemplate[] instrux_LFENCE = new[] {
    new itemplate(OCE.LFENCE, 0, new ulong[] {0,0,0,0,0} ,20032, OF.IF_X64, OF.IF_AMD),
    new itemplate(OCE.LFENCE, 0, new ulong[] {0,0,0,0,0} ,20032, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_LFS = new[] {
    new itemplate(OCE.LFS, 2, new ulong[] {REG16,MEMORY,0,0,0} ,16215, OF.IF_386),
    new itemplate(OCE.LFS, 2, new ulong[] {REG32,MEMORY,0,0,0} ,16221, OF.IF_386),
    
};

        static itemplate[] instrux_LGDT = new[] {
    new itemplate(OCE.LGDT, 1, new ulong[] {MEMORY,0,0,0,0} ,20037, OF.IF_286, OF.IF_PRIV),
    
};

        static itemplate[] instrux_LGS = new[] {
    new itemplate(OCE.LGS, 2, new ulong[] {REG16,MEMORY,0,0,0} ,16227, OF.IF_386),
    new itemplate(OCE.LGS, 2, new ulong[] {REG32,MEMORY,0,0,0} ,16233, OF.IF_386),
    
};

        static itemplate[] instrux_LIDT = new[] {
    new itemplate(OCE.LIDT, 1, new ulong[] {MEMORY,0,0,0,0} ,20042, OF.IF_286, OF.IF_PRIV),
    
};

        static itemplate[] instrux_LLDT = new[] {
    new itemplate(OCE.LLDT, 1, new ulong[] {MEMORY,0,0,0,0} ,20047, OF.IF_286, OF.IF_PROT, OF.IF_PRIV),
    new itemplate(OCE.LLDT, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,20047, OF.IF_286, OF.IF_PROT, OF.IF_PRIV),
    new itemplate(OCE.LLDT, 1, new ulong[] {REG16,0,0,0,0} ,20047, OF.IF_286, OF.IF_PROT, OF.IF_PRIV),
    
};

        static itemplate[] instrux_LMSW = new[] {
    new itemplate(OCE.LMSW, 1, new ulong[] {MEMORY,0,0,0,0} ,20052, OF.IF_286, OF.IF_PRIV),
    new itemplate(OCE.LMSW, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,20052, OF.IF_286, OF.IF_PRIV),
    new itemplate(OCE.LMSW, 1, new ulong[] {REG16,0,0,0,0} ,20052, OF.IF_286, OF.IF_PRIV),
    
};

        static itemplate[] instrux_LOADALL = new[] {
    new itemplate(OCE.LOADALL, 0, new ulong[] {0,0,0,0,0} ,21703, OF.IF_386, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_LOADALL286 = new[] {
    new itemplate(OCE.LOADALL286, 0, new ulong[] {0,0,0,0,0} ,21707, OF.IF_286, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_LODSB = new[] {
    new itemplate(OCE.LODSB, 0, new ulong[] {0,0,0,0,0} ,22137, OF.IF_8086),
    
};

        static itemplate[] instrux_LODSD = new[] {
    new itemplate(OCE.LODSD, 0, new ulong[] {0,0,0,0,0} ,21711, OF.IF_386),
    
};

        static itemplate[] instrux_LODSQ = new[] {
    new itemplate(OCE.LODSQ, 0, new ulong[] {0,0,0,0,0} ,21715, OF.IF_X64),
    
};

        static itemplate[] instrux_LODSW = new[] {
    new itemplate(OCE.LODSW, 0, new ulong[] {0,0,0,0,0} ,21719, OF.IF_8086),
    
};

        static itemplate[] instrux_LOOP = new[] {
    new itemplate(OCE.LOOP, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,20057, OF.IF_8086),
    new itemplate(OCE.LOOP, 2, new ulong[] {IMMEDIATE,REG_CX,0,0,0} ,20062, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.LOOP, 2, new ulong[] {IMMEDIATE,REG_ECX,0,0,0} ,20067, OF.IF_386),
    new itemplate(OCE.LOOP, 2, new ulong[] {IMMEDIATE,REG_RCX,0,0,0} ,20072, OF.IF_X64),
    
};

        static itemplate[] instrux_LOOPE = new[] {
    new itemplate(OCE.LOOPE, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,20077, OF.IF_8086),
    new itemplate(OCE.LOOPE, 2, new ulong[] {IMMEDIATE,REG_CX,0,0,0} ,20082, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.LOOPE, 2, new ulong[] {IMMEDIATE,REG_ECX,0,0,0} ,20087, OF.IF_386),
    new itemplate(OCE.LOOPE, 2, new ulong[] {IMMEDIATE,REG_RCX,0,0,0} ,20092, OF.IF_X64),
    
};

        static itemplate[] instrux_LOOPNE = new[] {
    new itemplate(OCE.LOOPNE, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,20097, OF.IF_8086),
    new itemplate(OCE.LOOPNE, 2, new ulong[] {IMMEDIATE,REG_CX,0,0,0} ,20102, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.LOOPNE, 2, new ulong[] {IMMEDIATE,REG_ECX,0,0,0} ,20107, OF.IF_386),
    new itemplate(OCE.LOOPNE, 2, new ulong[] {IMMEDIATE,REG_RCX,0,0,0} ,20112, OF.IF_X64),
    
};

        static itemplate[] instrux_LOOPNZ = new[] {
    new itemplate(OCE.LOOPNZ, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,20097, OF.IF_8086),
    new itemplate(OCE.LOOPNZ, 2, new ulong[] {IMMEDIATE,REG_CX,0,0,0} ,20102, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.LOOPNZ, 2, new ulong[] {IMMEDIATE,REG_ECX,0,0,0} ,20107, OF.IF_386),
    new itemplate(OCE.LOOPNZ, 2, new ulong[] {IMMEDIATE,REG_RCX,0,0,0} ,20112, OF.IF_X64),
    
};

        static itemplate[] instrux_LOOPZ = new[] {
    new itemplate(OCE.LOOPZ, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,20077, OF.IF_8086),
    new itemplate(OCE.LOOPZ, 2, new ulong[] {IMMEDIATE,REG_CX,0,0,0} ,20082, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.LOOPZ, 2, new ulong[] {IMMEDIATE,REG_ECX,0,0,0} ,20087, OF.IF_386),
    new itemplate(OCE.LOOPZ, 2, new ulong[] {IMMEDIATE,REG_RCX,0,0,0} ,20092, OF.IF_X64),
    
};

        static itemplate[] instrux_LSL = new[] {
    new itemplate(OCE.LSL, 2, new ulong[] {REG16,MEMORY,0,0,0} ,16239, OF.IF_286, OF.IF_PROT, OF.IF_SW),
    new itemplate(OCE.LSL, 2, new ulong[] {REG16,REG16,0,0,0} ,16239, OF.IF_286, OF.IF_PROT),
    new itemplate(OCE.LSL, 2, new ulong[] {REG16,REG32,0,0,0} ,16239, OF.IF_386, OF.IF_PROT),
    new itemplate(OCE.LSL, 2, new ulong[] {REG16,REG64,0,0,0} ,8573, OF.IF_X64, OF.IF_PROT),
    new itemplate(OCE.LSL, 2, new ulong[] {REG32,MEMORY,0,0,0} ,16245, OF.IF_386, OF.IF_PROT, OF.IF_SW),
    new itemplate(OCE.LSL, 2, new ulong[] {REG32,REG16,0,0,0} ,16245, OF.IF_386, OF.IF_PROT),
    new itemplate(OCE.LSL, 2, new ulong[] {REG32,REG32,0,0,0} ,16245, OF.IF_386, OF.IF_PROT),
    new itemplate(OCE.LSL, 2, new ulong[] {REG32,REG64,0,0,0} ,8580, OF.IF_X64, OF.IF_PROT),
    new itemplate(OCE.LSL, 2, new ulong[] {REG64,MEMORY,0,0,0} ,16251, OF.IF_X64, OF.IF_PROT, OF.IF_SW),
    new itemplate(OCE.LSL, 2, new ulong[] {REG64,REG16,0,0,0} ,16251, OF.IF_X64, OF.IF_PROT),
    new itemplate(OCE.LSL, 2, new ulong[] {REG64,REG32,0,0,0} ,16251, OF.IF_X64, OF.IF_PROT),
    new itemplate(OCE.LSL, 2, new ulong[] {REG64,REG64,0,0,0} ,16251, OF.IF_X64, OF.IF_PROT),
    
};

        static itemplate[] instrux_LSS = new[] {
    new itemplate(OCE.LSS, 2, new ulong[] {REG16,MEMORY,0,0,0} ,16257, OF.IF_386),
    new itemplate(OCE.LSS, 2, new ulong[] {REG32,MEMORY,0,0,0} ,16263, OF.IF_386),
    
};

        static itemplate[] instrux_LTR = new[] {
    new itemplate(OCE.LTR, 1, new ulong[] {MEMORY,0,0,0,0} ,20117, OF.IF_286, OF.IF_PROT, OF.IF_PRIV),
    new itemplate(OCE.LTR, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,20117, OF.IF_286, OF.IF_PROT, OF.IF_PRIV),
    new itemplate(OCE.LTR, 1, new ulong[] {REG16,0,0,0,0} ,20117, OF.IF_286, OF.IF_PROT, OF.IF_PRIV),
    
};

        static itemplate[] instrux_LZCNT = new[] {
    new itemplate(OCE.LZCNT, 2, new ulong[] {REG16,RM_GPR, BITS16,0,0,0} ,9630, OF.IF_P6, OF.IF_AMD),
    new itemplate(OCE.LZCNT, 2, new ulong[] {REG32,RM_GPR, BITS32,0,0,0} ,9637, OF.IF_P6, OF.IF_AMD),
    new itemplate(OCE.LZCNT, 2, new ulong[] {REG64,RM_GPR, BITS64,0,0,0} ,9644, OF.IF_X64, OF.IF_AMD),
    
};

        static itemplate[] instrux_MASKMOVDQU = new[] {
    new itemplate(OCE.MASKMOVDQU, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,17169, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_MASKMOVQ = new[] {
    new itemplate(OCE.MASKMOVQ, 2, new ulong[] {MMXREG,MMXREG,0,0,0} ,17151, OF.IF_KATMAI, OF.IF_MMX),
    
};

        static itemplate[] instrux_MAXPD = new[] {
    new itemplate(OCE.MAXPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17721, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_MAXPS = new[] {
    new itemplate(OCE.MAXPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,16947, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_MAXSD = new[] {
    new itemplate(OCE.MAXSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17727, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_MAXSS = new[] {
    new itemplate(OCE.MAXSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,16953, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_MFENCE = new[] {
    new itemplate(OCE.MFENCE, 0, new ulong[] {0,0,0,0,0} ,20122, OF.IF_X64, OF.IF_AMD),
    new itemplate(OCE.MFENCE, 0, new ulong[] {0,0,0,0,0} ,20122, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_MINPD = new[] {
    new itemplate(OCE.MINPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17733, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_MINPS = new[] {
    new itemplate(OCE.MINPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,16959, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_MINSD = new[] {
    new itemplate(OCE.MINSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17739, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_MINSS = new[] {
    new itemplate(OCE.MINSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,16965, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_MONITOR = new[] {
    new itemplate(OCE.MONITOR, 0, new ulong[] {0,0,0,0,0} ,20127, OF.IF_PRESCOTT),
    new itemplate(OCE.MONITOR, 3, new ulong[] {REG_EAX,REG_ECX,REG_EDX,0,0} ,20127, OF.IF_PRESCOTT),
    
};

        static itemplate[] instrux_MONTMUL = new[] {
    new itemplate(OCE.MONTMUL, 0, new ulong[] {0,0,0,0,0} ,18015, OF.IF_PENT, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_MOV = new[] {
    new itemplate(OCE.MOV, 2, new ulong[] {MEMORY,REG_SREG,0,0,0} ,20138, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {REG16,REG_SREG,0,0,0} ,20132, OF.IF_8086),
    new itemplate(OCE.MOV, 2, new ulong[] {REG32,REG_SREG,0,0,0} ,20137, OF.IF_386),
    new itemplate(OCE.MOV, 2, new ulong[] {REG_SREG,MEMORY,0,0,0} ,21723, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {REG_SREG,REG16,0,0,0} ,21723, OF.IF_8086),
    new itemplate(OCE.MOV, 2, new ulong[] {REG_SREG,REG32,0,0,0} ,21723, OF.IF_386),
    new itemplate(OCE.MOV, 2, new ulong[] {REG_AL,MEM_OFFS,0,0,0} ,21727, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {REG_AX,MEM_OFFS,0,0,0} ,20142, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {REG_EAX,MEM_OFFS,0,0,0} ,20147, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {REG_RAX,MEM_OFFS,0,0,0} ,20152, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {MEM_OFFS,REG_AL,0,0,0} ,21731, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {MEM_OFFS,REG_AX,0,0,0} ,20157, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {MEM_OFFS,REG_EAX,0,0,0} ,20162, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {MEM_OFFS,REG_RAX,0,0,0} ,20167, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {REG32,REG_CREG,0,0,0} ,16269, OF.IF_386, OF.IF_PRIV, OF.IF_NOLONG),
    new itemplate(OCE.MOV, 2, new ulong[] {REG64,REG_CREG,0,0,0} ,16275, OF.IF_X64, OF.IF_PRIV),
    new itemplate(OCE.MOV, 2, new ulong[] {REG_CREG,REG32,0,0,0} ,16281, OF.IF_386, OF.IF_PRIV, OF.IF_NOLONG),
    new itemplate(OCE.MOV, 2, new ulong[] {REG_CREG,REG64,0,0,0} ,16287, OF.IF_X64, OF.IF_PRIV),
    new itemplate(OCE.MOV, 2, new ulong[] {REG32,REG_DREG,0,0,0} ,16294, OF.IF_386, OF.IF_PRIV, OF.IF_NOLONG),
    new itemplate(OCE.MOV, 2, new ulong[] {REG64,REG_DREG,0,0,0} ,16293, OF.IF_X64, OF.IF_PRIV),
    new itemplate(OCE.MOV, 2, new ulong[] {REG_DREG,REG32,0,0,0} ,16300, OF.IF_386, OF.IF_PRIV, OF.IF_NOLONG),
    new itemplate(OCE.MOV, 2, new ulong[] {REG_DREG,REG64,0,0,0} ,16299, OF.IF_X64, OF.IF_PRIV),
    new itemplate(OCE.MOV, 2, new ulong[] {REG32,REG_TREG,0,0,0} ,20172, OF.IF_386, OF.IF_NOLONG),
    new itemplate(OCE.MOV, 2, new ulong[] {REG_TREG,REG32,0,0,0} ,20177, OF.IF_386, OF.IF_NOLONG),
    new itemplate(OCE.MOV, 2, new ulong[] {MEMORY,REG8,0,0,0} ,21735, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {REG8,REG8,0,0,0} ,21735, OF.IF_8086),
    new itemplate(OCE.MOV, 2, new ulong[] {MEMORY,REG16,0,0,0} ,20182, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {REG16,REG16,0,0,0} ,20182, OF.IF_8086),
    new itemplate(OCE.MOV, 2, new ulong[] {MEMORY,REG32,0,0,0} ,20187, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {REG32,REG32,0,0,0} ,20187, OF.IF_386),
    new itemplate(OCE.MOV, 2, new ulong[] {MEMORY,REG64,0,0,0} ,20192, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {REG64,REG64,0,0,0} ,20192, OF.IF_X64),
    new itemplate(OCE.MOV, 2, new ulong[] {REG8,MEMORY,0,0,0} ,21739, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {REG8,REG8,0,0,0} ,21739, OF.IF_8086),
    new itemplate(OCE.MOV, 2, new ulong[] {REG16,MEMORY,0,0,0} ,20197, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {REG16,REG16,0,0,0} ,20197, OF.IF_8086),
    new itemplate(OCE.MOV, 2, new ulong[] {REG32,MEMORY,0,0,0} ,20202, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {REG32,REG32,0,0,0} ,20202, OF.IF_386),
    new itemplate(OCE.MOV, 2, new ulong[] {REG64,MEMORY,0,0,0} ,20207, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {REG64,REG64,0,0,0} ,20207, OF.IF_X64),
    new itemplate(OCE.MOV, 2, new ulong[] {REG8,IMMEDIATE,0,0,0} ,21743, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {REG16,IMMEDIATE,0,0,0} ,20212, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {REG32,IMMEDIATE,0,0,0} ,20217, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {REG64,IMMEDIATE,0,0,0} ,20222, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {REG64,IMMEDIATE, BITS32,0,0,0} ,16305, OF.IF_X64),
    new itemplate(OCE.MOV, 2, new ulong[] {RM_GPR, BITS8,IMMEDIATE,0,0,0} ,20227, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,16311, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,16317, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,16305, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {MEMORY,IMMEDIATE, BITS8,0,0,0} ,20227, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {MEMORY,IMMEDIATE, BITS16,0,0,0} ,16311, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.MOV, 2, new ulong[] {MEMORY,IMMEDIATE, BITS32,0,0,0} ,16317, OF.IF_386, OF.IF_SM),
    
};

        static itemplate[] instrux_MOVAPD = new[] {
    new itemplate(OCE.MOVAPD, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,17745, OF.IF_WILLAMETTE, OF.IF_SSE2),
    new itemplate(OCE.MOVAPD, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,17751, OF.IF_WILLAMETTE, OF.IF_SSE2),
    new itemplate(OCE.MOVAPD, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,17751, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    new itemplate(OCE.MOVAPD, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,17745, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_MOVAPS = new[] {
    new itemplate(OCE.MOVAPS, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,16971, OF.IF_KATMAI, OF.IF_SSE),
    new itemplate(OCE.MOVAPS, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,16977, OF.IF_KATMAI, OF.IF_SSE),
    new itemplate(OCE.MOVAPS, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,16971, OF.IF_KATMAI, OF.IF_SSE),
    new itemplate(OCE.MOVAPS, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,16977, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_MOVBE = new[] {
    new itemplate(OCE.MOVBE, 2, new ulong[] {REG16,MEMORY, BITS16,0,0,0} ,10848, OF.IF_NEHALEM, OF.IF_SM),
    new itemplate(OCE.MOVBE, 2, new ulong[] {REG32,MEMORY, BITS32,0,0,0} ,10855, OF.IF_NEHALEM, OF.IF_SM),
    new itemplate(OCE.MOVBE, 2, new ulong[] {REG64,MEMORY, BITS64,0,0,0} ,10862, OF.IF_NEHALEM, OF.IF_SM),
    new itemplate(OCE.MOVBE, 2, new ulong[] {MEMORY, BITS16,REG16,0,0,0} ,10869, OF.IF_NEHALEM, OF.IF_SM),
    new itemplate(OCE.MOVBE, 2, new ulong[] {MEMORY, BITS32,REG32,0,0,0} ,10876, OF.IF_NEHALEM, OF.IF_SM),
    new itemplate(OCE.MOVBE, 2, new ulong[] {MEMORY, BITS64,REG64,0,0,0} ,10883, OF.IF_NEHALEM, OF.IF_SM),
    
};

        static itemplate[] instrux_MOVD = new[] {
    new itemplate(OCE.MOVD, 2, new ulong[] {MMXREG,MEMORY,0,0,0} ,16323, OF.IF_PENT, OF.IF_MMX, OF.IF_SD),
    new itemplate(OCE.MOVD, 2, new ulong[] {MMXREG,REG32,0,0,0} ,16323, OF.IF_PENT, OF.IF_MMX),
    new itemplate(OCE.MOVD, 2, new ulong[] {MEMORY,MMXREG,0,0,0} ,16329, OF.IF_PENT, OF.IF_MMX, OF.IF_SD),
    new itemplate(OCE.MOVD, 2, new ulong[] {REG32,MMXREG,0,0,0} ,16329, OF.IF_PENT, OF.IF_MMX),
    new itemplate(OCE.MOVD, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,8587, OF.IF_X64, OF.IF_SD),
    new itemplate(OCE.MOVD, 2, new ulong[] {XMMREG,REG32,0,0,0} ,8587, OF.IF_X64),
    new itemplate(OCE.MOVD, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,8594, OF.IF_X64, OF.IF_SD),
    new itemplate(OCE.MOVD, 2, new ulong[] {REG32,XMMREG,0,0,0} ,8594, OF.IF_X64, OF.IF_SSE),
    new itemplate(OCE.MOVD, 2, new ulong[] {XMMREG,REG32,0,0,0} ,17187, OF.IF_WILLAMETTE, OF.IF_SSE2),
    new itemplate(OCE.MOVD, 2, new ulong[] {REG32,XMMREG,0,0,0} ,17193, OF.IF_WILLAMETTE, OF.IF_SSE2),
    new itemplate(OCE.MOVD, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,17193, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SD),
    new itemplate(OCE.MOVD, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,17187, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SD),
    
};

        static itemplate[] instrux_MOVDDUP = new[] {
    new itemplate(OCE.MOVDDUP, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17919, OF.IF_PRESCOTT, OF.IF_SSE3),
    
};

        static itemplate[] instrux_MOVDQ2Q = new[] {
    new itemplate(OCE.MOVDQ2Q, 2, new ulong[] {MMXREG,XMMREG,0,0,0} ,17223, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_MOVDQA = new[] {
    new itemplate(OCE.MOVDQA, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,17199, OF.IF_WILLAMETTE, OF.IF_SSE2),
    new itemplate(OCE.MOVDQA, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,17205, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    new itemplate(OCE.MOVDQA, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,17199, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    new itemplate(OCE.MOVDQA, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,17205, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_MOVDQU = new[] {
    new itemplate(OCE.MOVDQU, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,17211, OF.IF_WILLAMETTE, OF.IF_SSE2),
    new itemplate(OCE.MOVDQU, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,17217, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    new itemplate(OCE.MOVDQU, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,17211, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    new itemplate(OCE.MOVDQU, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,17217, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_MOVHLPS = new[] {
    new itemplate(OCE.MOVHLPS, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,16803, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_MOVHPD = new[] {
    new itemplate(OCE.MOVHPD, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,17757, OF.IF_WILLAMETTE, OF.IF_SSE2),
    new itemplate(OCE.MOVHPD, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,17763, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_MOVHPS = new[] {
    new itemplate(OCE.MOVHPS, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,16983, OF.IF_KATMAI, OF.IF_SSE),
    new itemplate(OCE.MOVHPS, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,16989, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_MOVLHPS = new[] {
    new itemplate(OCE.MOVLHPS, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,16983, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_MOVLPD = new[] {
    new itemplate(OCE.MOVLPD, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,17769, OF.IF_WILLAMETTE, OF.IF_SSE2),
    new itemplate(OCE.MOVLPD, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,17775, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_MOVLPS = new[] {
    new itemplate(OCE.MOVLPS, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,16803, OF.IF_KATMAI, OF.IF_SSE),
    new itemplate(OCE.MOVLPS, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,16995, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_MOVMSKPD = new[] {
    new itemplate(OCE.MOVMSKPD, 2, new ulong[] {REG32,XMMREG,0,0,0} ,17781, OF.IF_WILLAMETTE, OF.IF_SSE2),
    new itemplate(OCE.MOVMSKPD, 2, new ulong[] {REG64,XMMREG,0,0,0} ,9392, OF.IF_X64, OF.IF_SSE2),
    
};

        static itemplate[] instrux_MOVMSKPS = new[] {
    new itemplate(OCE.MOVMSKPS, 2, new ulong[] {REG32,XMMREG,0,0,0} ,17001, OF.IF_KATMAI, OF.IF_SSE),
    new itemplate(OCE.MOVMSKPS, 2, new ulong[] {REG64,XMMREG,0,0,0} ,9133, OF.IF_X64, OF.IF_SSE),
    
};

        static itemplate[] instrux_MOVNTDQ = new[] {
    new itemplate(OCE.MOVNTDQ, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,17175, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_MOVNTDQA = new[] {
    new itemplate(OCE.MOVNTDQA, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,9665, OF.IF_SSE41),
    
};

        static itemplate[] instrux_MOVNTI = new[] {
    new itemplate(OCE.MOVNTI, 2, new ulong[] {MEMORY,REG32,0,0,0} ,9218, OF.IF_WILLAMETTE, OF.IF_SD),
    new itemplate(OCE.MOVNTI, 2, new ulong[] {MEMORY,REG64,0,0,0} ,9217, OF.IF_X64, OF.IF_SQ),
    
};

        static itemplate[] instrux_MOVNTPD = new[] {
    new itemplate(OCE.MOVNTPD, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,17181, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_MOVNTPS = new[] {
    new itemplate(OCE.MOVNTPS, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,17007, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_MOVNTQ = new[] {
    new itemplate(OCE.MOVNTQ, 2, new ulong[] {MEMORY,MMXREG,0,0,0} ,17157, OF.IF_KATMAI, OF.IF_MMX, OF.IF_SQ),
    
};

        static itemplate[] instrux_MOVNTSD = new[] {
    new itemplate(OCE.MOVNTSD, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,17961, OF.IF_SSE4A, OF.IF_AMD, OF.IF_SQ),
    
};

        static itemplate[] instrux_MOVNTSS = new[] {
    new itemplate(OCE.MOVNTSS, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,17967, OF.IF_SSE4A, OF.IF_AMD, OF.IF_SD),
    
};

        static itemplate[] instrux_MOVQ = new[] {
    new itemplate(OCE.MOVQ, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8601, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.MOVQ, 2, new ulong[] {RM_MMX,MMXREG,0,0,0} ,8608, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.MOVQ, 2, new ulong[] {MMXREG,RM_GPR, BITS64,0,0,0} ,16323, OF.IF_X64, OF.IF_MMX),
    new itemplate(OCE.MOVQ, 2, new ulong[] {RM_GPR, BITS64,MMXREG,0,0,0} ,16329, OF.IF_X64, OF.IF_MMX),
    new itemplate(OCE.MOVQ, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,17229, OF.IF_WILLAMETTE, OF.IF_SSE2),
    new itemplate(OCE.MOVQ, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,17235, OF.IF_WILLAMETTE, OF.IF_SSE2),
    new itemplate(OCE.MOVQ, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,17235, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SQ),
    new itemplate(OCE.MOVQ, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,17229, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SQ),
    new itemplate(OCE.MOVQ, 2, new ulong[] {XMMREG,RM_GPR, BITS64,0,0,0} ,9224, OF.IF_X64, OF.IF_SSE2),
    new itemplate(OCE.MOVQ, 2, new ulong[] {RM_GPR, BITS64,XMMREG,0,0,0} ,9231, OF.IF_X64, OF.IF_SSE2),
    
};

        static itemplate[] instrux_MOVQ2DQ = new[] {
    new itemplate(OCE.MOVQ2DQ, 2, new ulong[] {XMMREG,MMXREG,0,0,0} ,17241, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_MOVSB = new[] {
    new itemplate(OCE.MOVSB, 0, new ulong[] {0,0,0,0,0} ,6649, OF.IF_8086),
    
};

        static itemplate[] instrux_MOVSD = new[] {
    new itemplate(OCE.MOVSD, 0, new ulong[] {0,0,0,0,0} ,21747, OF.IF_386),
    new itemplate(OCE.MOVSD, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,17787, OF.IF_WILLAMETTE, OF.IF_SSE2),
    new itemplate(OCE.MOVSD, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,17793, OF.IF_WILLAMETTE, OF.IF_SSE2),
    new itemplate(OCE.MOVSD, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,17793, OF.IF_WILLAMETTE, OF.IF_SSE2),
    new itemplate(OCE.MOVSD, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,17787, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_MOVSHDUP = new[] {
    new itemplate(OCE.MOVSHDUP, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17925, OF.IF_PRESCOTT, OF.IF_SSE3),
    
};

        static itemplate[] instrux_MOVSLDUP = new[] {
    new itemplate(OCE.MOVSLDUP, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17931, OF.IF_PRESCOTT, OF.IF_SSE3),
    
};

        static itemplate[] instrux_MOVSQ = new[] {
    new itemplate(OCE.MOVSQ, 0, new ulong[] {0,0,0,0,0} ,21751, OF.IF_X64),
    
};

        static itemplate[] instrux_MOVSS = new[] {
    new itemplate(OCE.MOVSS, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,17013, OF.IF_KATMAI, OF.IF_SSE),
    new itemplate(OCE.MOVSS, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,17019, OF.IF_KATMAI, OF.IF_SSE),
    new itemplate(OCE.MOVSS, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,17013, OF.IF_KATMAI, OF.IF_SSE),
    new itemplate(OCE.MOVSS, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,17019, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_MOVSW = new[] {
    new itemplate(OCE.MOVSW, 0, new ulong[] {0,0,0,0,0} ,21755, OF.IF_8086),
    
};

        static itemplate[] instrux_MOVSX = new[] {
    new itemplate(OCE.MOVSX, 2, new ulong[] {REG16,MEMORY,0,0,0} ,16335, OF.IF_386, OF.IF_SB),
    new itemplate(OCE.MOVSX, 2, new ulong[] {REG16,REG8,0,0,0} ,16335, OF.IF_386),
    new itemplate(OCE.MOVSX, 2, new ulong[] {REG32,RM_GPR, BITS8,0,0,0} ,16341, OF.IF_386),
    new itemplate(OCE.MOVSX, 2, new ulong[] {REG32,RM_GPR, BITS16,0,0,0} ,16347, OF.IF_386),
    new itemplate(OCE.MOVSX, 2, new ulong[] {REG64,RM_GPR, BITS8,0,0,0} ,16353, OF.IF_X64),
    new itemplate(OCE.MOVSX, 2, new ulong[] {REG64,RM_GPR, BITS16,0,0,0} ,16359, OF.IF_X64),
    new itemplate(OCE.MOVSX, 2, new ulong[] {REG64,RM_GPR, BITS32,0,0,0} ,20232, OF.IF_X64),
    
};

        static itemplate[] instrux_MOVSXD = new[] {
    new itemplate(OCE.MOVSXD, 2, new ulong[] {REG64,RM_GPR, BITS32,0,0,0} ,20232, OF.IF_X64),
    
};

        static itemplate[] instrux_MOVUPD = new[] {
    new itemplate(OCE.MOVUPD, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,17799, OF.IF_WILLAMETTE, OF.IF_SSE2),
    new itemplate(OCE.MOVUPD, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,17805, OF.IF_WILLAMETTE, OF.IF_SSE2),
    new itemplate(OCE.MOVUPD, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,17805, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    new itemplate(OCE.MOVUPD, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,17799, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_MOVUPS = new[] {
    new itemplate(OCE.MOVUPS, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,17025, OF.IF_KATMAI, OF.IF_SSE),
    new itemplate(OCE.MOVUPS, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,17031, OF.IF_KATMAI, OF.IF_SSE),
    new itemplate(OCE.MOVUPS, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,17025, OF.IF_KATMAI, OF.IF_SSE),
    new itemplate(OCE.MOVUPS, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,17031, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_MOVZX = new[] {
    new itemplate(OCE.MOVZX, 2, new ulong[] {REG16,MEMORY,0,0,0} ,16365, OF.IF_386, OF.IF_SB),
    new itemplate(OCE.MOVZX, 2, new ulong[] {REG16,REG8,0,0,0} ,16365, OF.IF_386),
    new itemplate(OCE.MOVZX, 2, new ulong[] {REG32,RM_GPR, BITS8,0,0,0} ,16371, OF.IF_386),
    new itemplate(OCE.MOVZX, 2, new ulong[] {REG32,RM_GPR, BITS16,0,0,0} ,16377, OF.IF_386),
    new itemplate(OCE.MOVZX, 2, new ulong[] {REG64,RM_GPR, BITS8,0,0,0} ,16383, OF.IF_X64),
    new itemplate(OCE.MOVZX, 2, new ulong[] {REG64,RM_GPR, BITS16,0,0,0} ,16389, OF.IF_X64),
    
};

        static itemplate[] instrux_MPSADBW = new[] {
    new itemplate(OCE.MPSADBW, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7132, OF.IF_SSE41),
    
};

        static itemplate[] instrux_MUL = new[] {
    new itemplate(OCE.MUL, 1, new ulong[] {RM_GPR, BITS8,0,0,0,0} ,21759, OF.IF_8086),
    new itemplate(OCE.MUL, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,20237, OF.IF_8086),
    new itemplate(OCE.MUL, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,20242, OF.IF_386),
    new itemplate(OCE.MUL, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,20247, OF.IF_X64),
    
};

        static itemplate[] instrux_MULPD = new[] {
    new itemplate(OCE.MULPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17811, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_MULPS = new[] {
    new itemplate(OCE.MULPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17037, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_MULSD = new[] {
    new itemplate(OCE.MULSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17817, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_MULSS = new[] {
    new itemplate(OCE.MULSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17043, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_MWAIT = new[] {
    new itemplate(OCE.MWAIT, 0, new ulong[] {0,0,0,0,0} ,20252, OF.IF_PRESCOTT),
    new itemplate(OCE.MWAIT, 2, new ulong[] {REG_EAX,REG_ECX,0,0,0} ,20252, OF.IF_PRESCOTT),
    
};

        static itemplate[] instrux_NEG = new[] {
    new itemplate(OCE.NEG, 1, new ulong[] {RM_GPR, BITS8,0,0,0,0} ,21763, OF.IF_8086),
    new itemplate(OCE.NEG, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,20257, OF.IF_8086),
    new itemplate(OCE.NEG, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,20262, OF.IF_386),
    new itemplate(OCE.NEG, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,20267, OF.IF_X64),
    
};

        static itemplate[] instrux_NOP = new[] {
    new itemplate(OCE.NOP, 0, new ulong[] {0,0,0,0,0} ,21767, OF.IF_8086),
    new itemplate(OCE.NOP, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,16395, OF.IF_P6),
    new itemplate(OCE.NOP, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,16401, OF.IF_P6),
    new itemplate(OCE.NOP, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,16407, OF.IF_X64),
    
};

        static itemplate[] instrux_NOT = new[] {
    new itemplate(OCE.NOT, 1, new ulong[] {RM_GPR, BITS8,0,0,0,0} ,21771, OF.IF_8086),
    new itemplate(OCE.NOT, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,20272, OF.IF_8086),
    new itemplate(OCE.NOT, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,20277, OF.IF_386),
    new itemplate(OCE.NOT, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,20282, OF.IF_X64),
    
};

        static itemplate[] instrux_OR = new[] {
    new itemplate(OCE.OR, 2, new ulong[] {MEMORY,REG8,0,0,0} ,21775, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.OR, 2, new ulong[] {REG8,REG8,0,0,0} ,21775, OF.IF_8086),
    new itemplate(OCE.OR, 2, new ulong[] {MEMORY,REG16,0,0,0} ,20287, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.OR, 2, new ulong[] {REG16,REG16,0,0,0} ,20287, OF.IF_8086),
    new itemplate(OCE.OR, 2, new ulong[] {MEMORY,REG32,0,0,0} ,20292, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.OR, 2, new ulong[] {REG32,REG32,0,0,0} ,20292, OF.IF_386),
    new itemplate(OCE.OR, 2, new ulong[] {MEMORY,REG64,0,0,0} ,20297, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.OR, 2, new ulong[] {REG64,REG64,0,0,0} ,20297, OF.IF_X64),
    new itemplate(OCE.OR, 2, new ulong[] {REG8,MEMORY,0,0,0} ,13588, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.OR, 2, new ulong[] {REG8,REG8,0,0,0} ,13588, OF.IF_8086),
    new itemplate(OCE.OR, 2, new ulong[] {REG16,MEMORY,0,0,0} ,20302, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.OR, 2, new ulong[] {REG16,REG16,0,0,0} ,20302, OF.IF_8086),
    new itemplate(OCE.OR, 2, new ulong[] {REG32,MEMORY,0,0,0} ,20307, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.OR, 2, new ulong[] {REG32,REG32,0,0,0} ,20307, OF.IF_386),
    new itemplate(OCE.OR, 2, new ulong[] {REG64,MEMORY,0,0,0} ,20312, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.OR, 2, new ulong[] {REG64,REG64,0,0,0} ,20312, OF.IF_X64),
    new itemplate(OCE.OR, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE, BITS8,0,0,0} ,16413, OF.IF_8086),
    new itemplate(OCE.OR, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE, BITS8,0,0,0} ,16419, OF.IF_386),
    new itemplate(OCE.OR, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE, BITS8,0,0,0} ,16425, OF.IF_X64),
    new itemplate(OCE.OR, 2, new ulong[] {REG_AL,IMMEDIATE,0,0,0} ,21779, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.OR, 2, new ulong[] {REG_AX,SBYTE16,0,0,0} ,16413, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.OR, 2, new ulong[] {REG_AX,IMMEDIATE,0,0,0} ,20317, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.OR, 2, new ulong[] {REG_EAX,SBYTE32,0,0,0} ,16419, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.OR, 2, new ulong[] {REG_EAX,IMMEDIATE,0,0,0} ,20322, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.OR, 2, new ulong[] {REG_RAX,SBYTE64,0,0,0} ,16425, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.OR, 2, new ulong[] {REG_RAX,IMMEDIATE,0,0,0} ,20327, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.OR, 2, new ulong[] {RM_GPR, BITS8,IMMEDIATE,0,0,0} ,20332, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.OR, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,16431, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.OR, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,16437, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.OR, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,16443, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.OR, 2, new ulong[] {MEMORY,IMMEDIATE, BITS8,0,0,0} ,20332, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.OR, 2, new ulong[] {MEMORY,IMMEDIATE, BITS16,0,0,0} ,16431, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.OR, 2, new ulong[] {MEMORY,IMMEDIATE, BITS32,0,0,0} ,16437, OF.IF_386, OF.IF_SM),
    
};

        static itemplate[] instrux_ORPD = new[] {
    new itemplate(OCE.ORPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17823, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_ORPS = new[] {
    new itemplate(OCE.ORPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17049, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_OUT = new[] {
    new itemplate(OCE.OUT, 2, new ulong[] {IMMEDIATE,REG_AL,0,0,0} ,21783, OF.IF_8086, OF.IF_SB),
    new itemplate(OCE.OUT, 2, new ulong[] {IMMEDIATE,REG_AX,0,0,0} ,20337, OF.IF_8086, OF.IF_SB),
    new itemplate(OCE.OUT, 2, new ulong[] {IMMEDIATE,REG_EAX,0,0,0} ,20342, OF.IF_386, OF.IF_SB),
    new itemplate(OCE.OUT, 2, new ulong[] {REG_DX,REG_AL,0,0,0} ,22140, OF.IF_8086),
    new itemplate(OCE.OUT, 2, new ulong[] {REG_DX,REG_AX,0,0,0} ,21787, OF.IF_8086),
    new itemplate(OCE.OUT, 2, new ulong[] {REG_DX,REG_EAX,0,0,0} ,21791, OF.IF_386),
    
};

        static itemplate[] instrux_OUTSB = new[] {
    new itemplate(OCE.OUTSB, 0, new ulong[] {0,0,0,0,0} ,22143, OF.IF_186),
    
};

        static itemplate[] instrux_OUTSD = new[] {
    new itemplate(OCE.OUTSD, 0, new ulong[] {0,0,0,0,0} ,21795, OF.IF_386),
    
};

        static itemplate[] instrux_OUTSW = new[] {
    new itemplate(OCE.OUTSW, 0, new ulong[] {0,0,0,0,0} ,21799, OF.IF_186),
    
};

        static itemplate[] instrux_PABSB = new[] {
    new itemplate(OCE.PABSB, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9420, OF.IF_SSSE3, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PABSB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9427, OF.IF_SSSE3),
    
};

        static itemplate[] instrux_PABSD = new[] {
    new itemplate(OCE.PABSD, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9448, OF.IF_SSSE3, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PABSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9455, OF.IF_SSSE3),
    
};

        static itemplate[] instrux_PABSW = new[] {
    new itemplate(OCE.PABSW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9434, OF.IF_SSSE3, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PABSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9441, OF.IF_SSSE3),
    
};

        static itemplate[] instrux_PACKSSDW = new[] {
    new itemplate(OCE.PACKSSDW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8615, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PACKSSDW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17253, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PACKSSWB = new[] {
    new itemplate(OCE.PACKSSWB, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8622, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PACKSSWB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17247, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PACKUSDW = new[] {
    new itemplate(OCE.PACKUSDW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9672, OF.IF_SSE41),
    
};

        static itemplate[] instrux_PACKUSWB = new[] {
    new itemplate(OCE.PACKUSWB, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8629, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PACKUSWB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17259, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PADDB = new[] {
    new itemplate(OCE.PADDB, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8636, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PADDB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17265, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PADDD = new[] {
    new itemplate(OCE.PADDD, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8643, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PADDD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17277, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PADDQ = new[] {
    new itemplate(OCE.PADDQ, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,17283, OF.IF_WILLAMETTE, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PADDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17289, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PADDSB = new[] {
    new itemplate(OCE.PADDSB, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8650, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PADDSB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17295, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PADDSIW = new[] {
    new itemplate(OCE.PADDSIW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,16449, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_PADDSW = new[] {
    new itemplate(OCE.PADDSW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8657, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PADDSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17301, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PADDUSB = new[] {
    new itemplate(OCE.PADDUSB, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8664, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PADDUSB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17307, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PADDUSW = new[] {
    new itemplate(OCE.PADDUSW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8671, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PADDUSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17313, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PADDW = new[] {
    new itemplate(OCE.PADDW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8678, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PADDW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17271, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PALIGNR = new[] {
    new itemplate(OCE.PALIGNR, 3, new ulong[] {MMXREG,RM_MMX,IMMEDIATE,0,0} ,7060, OF.IF_SSSE3, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PALIGNR, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7068, OF.IF_SSSE3),
    
};

        static itemplate[] instrux_PAND = new[] {
    new itemplate(OCE.PAND, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8685, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PAND, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17319, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PANDN = new[] {
    new itemplate(OCE.PANDN, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8692, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PANDN, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17325, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PAUSE = new[] {
    new itemplate(OCE.PAUSE, 0, new ulong[] {0,0,0,0,0} ,20347, OF.IF_8086),
    
};

        static itemplate[] instrux_PAVEB = new[] {
    new itemplate(OCE.PAVEB, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,16455, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_PAVGB = new[] {
    new itemplate(OCE.PAVGB, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9147, OF.IF_KATMAI, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PAVGB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17331, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PAVGUSB = new[] {
    new itemplate(OCE.PAVGUSB, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6588, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PAVGW = new[] {
    new itemplate(OCE.PAVGW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9154, OF.IF_KATMAI, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PAVGW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17337, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PBLENDVB = new[] {
    new itemplate(OCE.PBLENDVB, 3, new ulong[] {XMMREG,RM_XMM,XMM0,0,0} ,9679, OF.IF_SSE41),
    
};

        static itemplate[] instrux_PBLENDW = new[] {
    new itemplate(OCE.PBLENDW, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7140, OF.IF_SSE41),
    
};

        static itemplate[] instrux_PCLMULHQHQDQ = new[] {
    new itemplate(OCE.PCLMULHQHQDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,5031, OF.IF_SSE, OF.IF_WESTMERE, OF.IF_SO),
    
};

        static itemplate[] instrux_PCLMULHQLQDQ = new[] {
    new itemplate(OCE.PCLMULHQLQDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,5013, OF.IF_SSE, OF.IF_WESTMERE, OF.IF_SO),
    
};

        static itemplate[] instrux_PCLMULLQHQDQ = new[] {
    new itemplate(OCE.PCLMULLQHQDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,5022, OF.IF_SSE, OF.IF_WESTMERE, OF.IF_SO),
    
};

        static itemplate[] instrux_PCLMULLQLQDQ = new[] {
    new itemplate(OCE.PCLMULLQLQDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,5004, OF.IF_SSE, OF.IF_WESTMERE, OF.IF_SO),
    
};

        static itemplate[] instrux_PCLMULQDQ = new[] {
    new itemplate(OCE.PCLMULQDQ, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,8188, OF.IF_SSE, OF.IF_WESTMERE, OF.IF_SO),
    
};

        static itemplate[] instrux_PCMOV = new[] {
    new itemplate(OCE.PCMOV, 4, new ulong[] {XMMREG,SAME_AS, 0,XMMREG,RM_XMM,0} ,10393, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PCMOV, 4, new ulong[] {XMMREG,SAME_AS, 0,RM_XMM,XMMREG,0} ,10400, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PCMOV, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10407, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PCMOV, 4, new ulong[] {XMMREG,RM_XMM,XMMREG,SAME_AS, 0,0} ,10414, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PCMPEQB = new[] {
    new itemplate(OCE.PCMPEQB, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8699, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PCMPEQB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17343, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PCMPEQD = new[] {
    new itemplate(OCE.PCMPEQD, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8706, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PCMPEQD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17355, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PCMPEQQ = new[] {
    new itemplate(OCE.PCMPEQQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9686, OF.IF_SSE41),
    
};

        static itemplate[] instrux_PCMPEQW = new[] {
    new itemplate(OCE.PCMPEQW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8713, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PCMPEQW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17349, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PCMPESTRI = new[] {
    new itemplate(OCE.PCMPESTRI, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7212, OF.IF_SSE42),
    
};

        static itemplate[] instrux_PCMPESTRM = new[] {
    new itemplate(OCE.PCMPESTRM, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7220, OF.IF_SSE42),
    
};

        static itemplate[] instrux_PCMPGTB = new[] {
    new itemplate(OCE.PCMPGTB, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8720, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PCMPGTB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17361, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PCMPGTD = new[] {
    new itemplate(OCE.PCMPGTD, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8727, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PCMPGTD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17373, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PCMPGTQ = new[] {
    new itemplate(OCE.PCMPGTQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9861, OF.IF_SSE42),
    
};

        static itemplate[] instrux_PCMPGTW = new[] {
    new itemplate(OCE.PCMPGTW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8734, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PCMPGTW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17367, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PCMPISTRI = new[] {
    new itemplate(OCE.PCMPISTRI, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7228, OF.IF_SSE42),
    
};

        static itemplate[] instrux_PCMPISTRM = new[] {
    new itemplate(OCE.PCMPISTRM, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7236, OF.IF_SSE42),
    
};

        static itemplate[] instrux_PCOMB = new[] {
    new itemplate(OCE.PCOMB, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7276, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMD = new[] {
    new itemplate(OCE.PCOMD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7292, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMEQB = new[] {
    new itemplate(OCE.PCOMEQB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,666, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMEQD = new[] {
    new itemplate(OCE.PCOMEQD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,810, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMEQQ = new[] {
    new itemplate(OCE.PCOMEQQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,882, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMEQUB = new[] {
    new itemplate(OCE.PCOMEQUB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,954, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMEQUD = new[] {
    new itemplate(OCE.PCOMEQUD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1098, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMEQUQ = new[] {
    new itemplate(OCE.PCOMEQUQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1170, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMEQUW = new[] {
    new itemplate(OCE.PCOMEQUW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1026, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMEQW = new[] {
    new itemplate(OCE.PCOMEQW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,738, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMFALSEB = new[] {
    new itemplate(OCE.PCOMFALSEB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,684, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMFALSED = new[] {
    new itemplate(OCE.PCOMFALSED, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,828, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMFALSEQ = new[] {
    new itemplate(OCE.PCOMFALSEQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,900, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMFALSEUB = new[] {
    new itemplate(OCE.PCOMFALSEUB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,972, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMFALSEUD = new[] {
    new itemplate(OCE.PCOMFALSEUD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1116, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMFALSEUQ = new[] {
    new itemplate(OCE.PCOMFALSEUQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1188, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMFALSEUW = new[] {
    new itemplate(OCE.PCOMFALSEUW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1044, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMFALSEW = new[] {
    new itemplate(OCE.PCOMFALSEW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,756, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMGEB = new[] {
    new itemplate(OCE.PCOMGEB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,657, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMGED = new[] {
    new itemplate(OCE.PCOMGED, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,801, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMGEQ = new[] {
    new itemplate(OCE.PCOMGEQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,873, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMGEUB = new[] {
    new itemplate(OCE.PCOMGEUB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,945, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMGEUD = new[] {
    new itemplate(OCE.PCOMGEUD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1089, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMGEUQ = new[] {
    new itemplate(OCE.PCOMGEUQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1161, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMGEUW = new[] {
    new itemplate(OCE.PCOMGEUW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1017, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMGEW = new[] {
    new itemplate(OCE.PCOMGEW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,729, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMGTB = new[] {
    new itemplate(OCE.PCOMGTB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,648, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMGTD = new[] {
    new itemplate(OCE.PCOMGTD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,792, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMGTQ = new[] {
    new itemplate(OCE.PCOMGTQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,864, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMGTUB = new[] {
    new itemplate(OCE.PCOMGTUB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,936, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMGTUD = new[] {
    new itemplate(OCE.PCOMGTUD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1080, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMGTUQ = new[] {
    new itemplate(OCE.PCOMGTUQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1152, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMGTUW = new[] {
    new itemplate(OCE.PCOMGTUW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1008, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMGTW = new[] {
    new itemplate(OCE.PCOMGTW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,720, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMLEB = new[] {
    new itemplate(OCE.PCOMLEB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,639, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMLED = new[] {
    new itemplate(OCE.PCOMLED, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,783, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMLEQ = new[] {
    new itemplate(OCE.PCOMLEQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,855, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMLEUB = new[] {
    new itemplate(OCE.PCOMLEUB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,927, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMLEUD = new[] {
    new itemplate(OCE.PCOMLEUD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1071, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMLEUQ = new[] {
    new itemplate(OCE.PCOMLEUQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1143, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMLEUW = new[] {
    new itemplate(OCE.PCOMLEUW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,999, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMLEW = new[] {
    new itemplate(OCE.PCOMLEW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,711, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMLTB = new[] {
    new itemplate(OCE.PCOMLTB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,630, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMLTD = new[] {
    new itemplate(OCE.PCOMLTD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,774, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMLTQ = new[] {
    new itemplate(OCE.PCOMLTQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,846, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMLTUB = new[] {
    new itemplate(OCE.PCOMLTUB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,918, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMLTUD = new[] {
    new itemplate(OCE.PCOMLTUD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1062, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMLTUQ = new[] {
    new itemplate(OCE.PCOMLTUQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1134, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMLTUW = new[] {
    new itemplate(OCE.PCOMLTUW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,990, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMLTW = new[] {
    new itemplate(OCE.PCOMLTW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,702, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMNEQB = new[] {
    new itemplate(OCE.PCOMNEQB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,675, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMNEQD = new[] {
    new itemplate(OCE.PCOMNEQD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,819, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMNEQQ = new[] {
    new itemplate(OCE.PCOMNEQQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,891, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMNEQUB = new[] {
    new itemplate(OCE.PCOMNEQUB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,963, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMNEQUD = new[] {
    new itemplate(OCE.PCOMNEQUD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1107, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMNEQUQ = new[] {
    new itemplate(OCE.PCOMNEQUQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1179, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMNEQUW = new[] {
    new itemplate(OCE.PCOMNEQUW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1035, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMNEQW = new[] {
    new itemplate(OCE.PCOMNEQW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,747, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMQ = new[] {
    new itemplate(OCE.PCOMQ, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7300, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMTRUEB = new[] {
    new itemplate(OCE.PCOMTRUEB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,693, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMTRUED = new[] {
    new itemplate(OCE.PCOMTRUED, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,837, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMTRUEQ = new[] {
    new itemplate(OCE.PCOMTRUEQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,909, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMTRUEUB = new[] {
    new itemplate(OCE.PCOMTRUEUB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,981, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMTRUEUD = new[] {
    new itemplate(OCE.PCOMTRUEUD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1125, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMTRUEUQ = new[] {
    new itemplate(OCE.PCOMTRUEUQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1197, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMTRUEUW = new[] {
    new itemplate(OCE.PCOMTRUEUW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1053, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMTRUEW = new[] {
    new itemplate(OCE.PCOMTRUEW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,765, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMUB = new[] {
    new itemplate(OCE.PCOMUB, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7308, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMUD = new[] {
    new itemplate(OCE.PCOMUD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7324, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMUQ = new[] {
    new itemplate(OCE.PCOMUQ, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7332, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMUW = new[] {
    new itemplate(OCE.PCOMUW, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7316, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PCOMW = new[] {
    new itemplate(OCE.PCOMW, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7284, OF.IF_SSE5, OF.IF_AMD, OF.IF_SO),
    
};

        static itemplate[] instrux_PDISTIB = new[] {
    new itemplate(OCE.PDISTIB, 2, new ulong[] {MMXREG,MEMORY,0,0,0} ,17620, OF.IF_PENT, OF.IF_MMX, OF.IF_SM, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_PERMPD = new[] {
    new itemplate(OCE.PERMPD, 4, new ulong[] {XMMREG,SAME_AS, 0,XMMREG,RM_XMM,0} ,10365, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PERMPD, 4, new ulong[] {XMMREG,SAME_AS, 0,RM_XMM,XMMREG,0} ,10372, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PERMPD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10379, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PERMPD, 4, new ulong[] {XMMREG,RM_XMM,XMMREG,SAME_AS, 0,0} ,10386, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PERMPS = new[] {
    new itemplate(OCE.PERMPS, 4, new ulong[] {XMMREG,SAME_AS, 0,XMMREG,RM_XMM,0} ,10337, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PERMPS, 4, new ulong[] {XMMREG,SAME_AS, 0,RM_XMM,XMMREG,0} ,10344, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PERMPS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10351, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PERMPS, 4, new ulong[] {XMMREG,RM_XMM,XMMREG,SAME_AS, 0,0} ,10358, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PEXTRB = new[] {
    new itemplate(OCE.PEXTRB, 3, new ulong[] {REG32,XMMREG,IMMEDIATE,0,0} ,10, OF.IF_SSE41),
    new itemplate(OCE.PEXTRB, 3, new ulong[] {MEMORY, BITS8,XMMREG,IMMEDIATE,0,0} ,10, OF.IF_SSE41),
    new itemplate(OCE.PEXTRB, 3, new ulong[] {REG64,XMMREG,IMMEDIATE,0,0} ,9, OF.IF_SSE41, OF.IF_X64),
    
};

        static itemplate[] instrux_PEXTRD = new[] {
    new itemplate(OCE.PEXTRD, 3, new ulong[] {RM_GPR, BITS32,XMMREG,IMMEDIATE,0,0} ,19, OF.IF_SSE41),
    
};

        static itemplate[] instrux_PEXTRQ = new[] {
    new itemplate(OCE.PEXTRQ, 3, new ulong[] {RM_GPR, BITS64,XMMREG,IMMEDIATE,0,0} ,18, OF.IF_SSE41, OF.IF_X64),
    
};

        static itemplate[] instrux_PEXTRW = new[] {
    new itemplate(OCE.PEXTRW, 3, new ulong[] {REG32,MMXREG,IMMEDIATE,0,0} ,9161, OF.IF_KATMAI, OF.IF_MMX, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.PEXTRW, 3, new ulong[] {REG32,XMMREG,IMMEDIATE,0,0} ,9238, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.PEXTRW, 3, new ulong[] {REG32,XMMREG,IMMEDIATE,0,0} ,28, OF.IF_SSE41),
    new itemplate(OCE.PEXTRW, 3, new ulong[] {MEMORY, BITS16,XMMREG,IMMEDIATE,0,0} ,28, OF.IF_SSE41),
    new itemplate(OCE.PEXTRW, 3, new ulong[] {REG64,XMMREG,IMMEDIATE,0,0} ,27, OF.IF_SSE41, OF.IF_X64),
    
};

        static itemplate[] instrux_PF2ID = new[] {
    new itemplate(OCE.PF2ID, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6596, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PF2IW = new[] {
    new itemplate(OCE.PF2IW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6876, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PFACC = new[] {
    new itemplate(OCE.PFACC, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6604, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PFADD = new[] {
    new itemplate(OCE.PFADD, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6612, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PFCMPEQ = new[] {
    new itemplate(OCE.PFCMPEQ, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6620, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PFCMPGE = new[] {
    new itemplate(OCE.PFCMPGE, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6628, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PFCMPGT = new[] {
    new itemplate(OCE.PFCMPGT, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6636, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PFMAX = new[] {
    new itemplate(OCE.PFMAX, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6644, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PFMIN = new[] {
    new itemplate(OCE.PFMIN, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6652, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PFMUL = new[] {
    new itemplate(OCE.PFMUL, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6660, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PFNACC = new[] {
    new itemplate(OCE.PFNACC, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6884, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PFPNACC = new[] {
    new itemplate(OCE.PFPNACC, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6892, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PFRCP = new[] {
    new itemplate(OCE.PFRCP, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6668, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PFRCPIT1 = new[] {
    new itemplate(OCE.PFRCPIT1, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6676, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PFRCPIT2 = new[] {
    new itemplate(OCE.PFRCPIT2, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6684, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PFRCPV = new[] {
    new itemplate(OCE.PFRCPV, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,7372, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_PFRSQIT1 = new[] {
    new itemplate(OCE.PFRSQIT1, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6692, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PFRSQRT = new[] {
    new itemplate(OCE.PFRSQRT, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6700, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PFRSQRTV = new[] {
    new itemplate(OCE.PFRSQRTV, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,7380, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_PFSUB = new[] {
    new itemplate(OCE.PFSUB, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6708, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PFSUBR = new[] {
    new itemplate(OCE.PFSUBR, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6716, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PHADDBD = new[] {
    new itemplate(OCE.PHADDBD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10750, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PHADDBQ = new[] {
    new itemplate(OCE.PHADDBQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10757, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PHADDBW = new[] {
    new itemplate(OCE.PHADDBW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10743, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PHADDD = new[] {
    new itemplate(OCE.PHADDD, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9476, OF.IF_SSSE3, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PHADDD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9483, OF.IF_SSSE3),
    
};

        static itemplate[] instrux_PHADDDQ = new[] {
    new itemplate(OCE.PHADDDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10778, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PHADDSW = new[] {
    new itemplate(OCE.PHADDSW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9490, OF.IF_SSSE3, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PHADDSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9497, OF.IF_SSSE3),
    
};

        static itemplate[] instrux_PHADDUBD = new[] {
    new itemplate(OCE.PHADDUBD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10792, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PHADDUBQ = new[] {
    new itemplate(OCE.PHADDUBQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10799, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PHADDUBW = new[] {
    new itemplate(OCE.PHADDUBW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10785, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PHADDUDQ = new[] {
    new itemplate(OCE.PHADDUDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10820, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PHADDUWD = new[] {
    new itemplate(OCE.PHADDUWD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10806, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PHADDUWQ = new[] {
    new itemplate(OCE.PHADDUWQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10813, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PHADDW = new[] {
    new itemplate(OCE.PHADDW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9462, OF.IF_SSSE3, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PHADDW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9469, OF.IF_SSSE3),
    
};

        static itemplate[] instrux_PHADDWD = new[] {
    new itemplate(OCE.PHADDWD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10764, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PHADDWQ = new[] {
    new itemplate(OCE.PHADDWQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10771, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PHMINPOSUW = new[] {
    new itemplate(OCE.PHMINPOSUW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9693, OF.IF_SSE41),
    
};

        static itemplate[] instrux_PHSUBBW = new[] {
    new itemplate(OCE.PHSUBBW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10827, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PHSUBD = new[] {
    new itemplate(OCE.PHSUBD, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9518, OF.IF_SSSE3, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PHSUBD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9525, OF.IF_SSSE3),
    
};

        static itemplate[] instrux_PHSUBDQ = new[] {
    new itemplate(OCE.PHSUBDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10841, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PHSUBSW = new[] {
    new itemplate(OCE.PHSUBSW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9532, OF.IF_SSSE3, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PHSUBSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9539, OF.IF_SSSE3),
    
};

        static itemplate[] instrux_PHSUBW = new[] {
    new itemplate(OCE.PHSUBW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9504, OF.IF_SSSE3, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PHSUBW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9511, OF.IF_SSSE3),
    
};

        static itemplate[] instrux_PHSUBWD = new[] {
    new itemplate(OCE.PHSUBWD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10834, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PI2FD = new[] {
    new itemplate(OCE.PI2FD, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6724, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PI2FW = new[] {
    new itemplate(OCE.PI2FW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6900, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PINSRB = new[] {
    new itemplate(OCE.PINSRB, 3, new ulong[] {XMMREG,MEMORY,IMMEDIATE,0,0} ,37, OF.IF_SSE41, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.PINSRB, 3, new ulong[] {XMMREG,RM_GPR, BITS8,IMMEDIATE,0,0} ,36, OF.IF_SSE41, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.PINSRB, 3, new ulong[] {XMMREG,REG32,IMMEDIATE,0,0} ,37, OF.IF_SSE41, OF.IF_SB, OF.IF_AR2),
    
};

        static itemplate[] instrux_PINSRD = new[] {
    new itemplate(OCE.PINSRD, 3, new ulong[] {XMMREG,MEMORY,IMMEDIATE,0,0} ,46, OF.IF_SSE41, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.PINSRD, 3, new ulong[] {XMMREG,RM_GPR, BITS32,IMMEDIATE,0,0} ,46, OF.IF_SSE41, OF.IF_SB, OF.IF_AR2),
    
};

        static itemplate[] instrux_PINSRQ = new[] {
    new itemplate(OCE.PINSRQ, 3, new ulong[] {XMMREG,MEMORY,IMMEDIATE,0,0} ,45, OF.IF_SSE41, OF.IF_X64, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.PINSRQ, 3, new ulong[] {XMMREG,RM_GPR, BITS64,IMMEDIATE,0,0} ,45, OF.IF_SSE41, OF.IF_X64, OF.IF_SB, OF.IF_AR2),
    
};

        static itemplate[] instrux_PINSRW = new[] {
    new itemplate(OCE.PINSRW, 3, new ulong[] {MMXREG,MEMORY,IMMEDIATE,0,0} ,9168, OF.IF_KATMAI, OF.IF_MMX, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.PINSRW, 3, new ulong[] {MMXREG,RM_GPR, BITS16,IMMEDIATE,0,0} ,9168, OF.IF_KATMAI, OF.IF_MMX, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.PINSRW, 3, new ulong[] {MMXREG,REG32,IMMEDIATE,0,0} ,9168, OF.IF_KATMAI, OF.IF_MMX, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.PINSRW, 3, new ulong[] {XMMREG,REG16,IMMEDIATE,0,0} ,9245, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.PINSRW, 3, new ulong[] {XMMREG,REG32,IMMEDIATE,0,0} ,9245, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.PINSRW, 3, new ulong[] {XMMREG,MEMORY,IMMEDIATE,0,0} ,9245, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.PINSRW, 3, new ulong[] {XMMREG,MEMORY, BITS16,IMMEDIATE,0,0} ,9245, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SB, OF.IF_AR2),
    
};

        static itemplate[] instrux_PMACHRIW = new[] {
    new itemplate(OCE.PMACHRIW, 2, new ulong[] {MMXREG,MEMORY,0,0,0} ,17716, OF.IF_PENT, OF.IF_MMX, OF.IF_SM, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_PMACSDD = new[] {
    new itemplate(OCE.PMACSDD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10484, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PMACSDQH = new[] {
    new itemplate(OCE.PMACSDQH, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10512, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PMACSDQL = new[] {
    new itemplate(OCE.PMACSDQL, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10498, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PMACSSDD = new[] {
    new itemplate(OCE.PMACSSDD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10477, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PMACSSDQH = new[] {
    new itemplate(OCE.PMACSSDQH, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10505, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PMACSSDQL = new[] {
    new itemplate(OCE.PMACSSDQL, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10491, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PMACSSWD = new[] {
    new itemplate(OCE.PMACSSWD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10463, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PMACSSWW = new[] {
    new itemplate(OCE.PMACSSWW, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10449, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PMACSWD = new[] {
    new itemplate(OCE.PMACSWD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10470, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PMACSWW = new[] {
    new itemplate(OCE.PMACSWW, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10456, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PMADCSSWD = new[] {
    new itemplate(OCE.PMADCSSWD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10519, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PMADCSWD = new[] {
    new itemplate(OCE.PMADCSWD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10526, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PMADDUBSW = new[] {
    new itemplate(OCE.PMADDUBSW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9546, OF.IF_SSSE3, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PMADDUBSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9553, OF.IF_SSSE3),
    
};

        static itemplate[] instrux_PMADDWD = new[] {
    new itemplate(OCE.PMADDWD, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8741, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PMADDWD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17379, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PMAGW = new[] {
    new itemplate(OCE.PMAGW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,16461, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_PMAXSB = new[] {
    new itemplate(OCE.PMAXSB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9700, OF.IF_SSE41),
    
};

        static itemplate[] instrux_PMAXSD = new[] {
    new itemplate(OCE.PMAXSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9707, OF.IF_SSE41),
    
};

        static itemplate[] instrux_PMAXSW = new[] {
    new itemplate(OCE.PMAXSW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9175, OF.IF_KATMAI, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PMAXSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17385, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PMAXUB = new[] {
    new itemplate(OCE.PMAXUB, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9182, OF.IF_KATMAI, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PMAXUB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17391, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PMAXUD = new[] {
    new itemplate(OCE.PMAXUD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9714, OF.IF_SSE41),
    
};

        static itemplate[] instrux_PMAXUW = new[] {
    new itemplate(OCE.PMAXUW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9721, OF.IF_SSE41),
    
};

        static itemplate[] instrux_PMINSB = new[] {
    new itemplate(OCE.PMINSB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9728, OF.IF_SSE41),
    
};

        static itemplate[] instrux_PMINSD = new[] {
    new itemplate(OCE.PMINSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9735, OF.IF_SSE41),
    
};

        static itemplate[] instrux_PMINSW = new[] {
    new itemplate(OCE.PMINSW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9189, OF.IF_KATMAI, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PMINSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17397, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PMINUB = new[] {
    new itemplate(OCE.PMINUB, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9196, OF.IF_KATMAI, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PMINUB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17403, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PMINUD = new[] {
    new itemplate(OCE.PMINUD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9742, OF.IF_SSE41),
    
};

        static itemplate[] instrux_PMINUW = new[] {
    new itemplate(OCE.PMINUW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9749, OF.IF_SSE41),
    
};

        static itemplate[] instrux_PMOVMSKB = new[] {
    new itemplate(OCE.PMOVMSKB, 2, new ulong[] {REG32,MMXREG,0,0,0} ,17163, OF.IF_KATMAI, OF.IF_MMX),
    new itemplate(OCE.PMOVMSKB, 2, new ulong[] {REG32,XMMREG,0,0,0} ,17409, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_PMOVSXBD = new[] {
    new itemplate(OCE.PMOVSXBD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9763, OF.IF_SSE41, OF.IF_SD),
    
};

        static itemplate[] instrux_PMOVSXBQ = new[] {
    new itemplate(OCE.PMOVSXBQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9770, OF.IF_SSE41, OF.IF_SW),
    
};

        static itemplate[] instrux_PMOVSXBW = new[] {
    new itemplate(OCE.PMOVSXBW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9756, OF.IF_SSE41, OF.IF_SQ),
    
};

        static itemplate[] instrux_PMOVSXDQ = new[] {
    new itemplate(OCE.PMOVSXDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9791, OF.IF_SSE41, OF.IF_SQ),
    
};

        static itemplate[] instrux_PMOVSXWD = new[] {
    new itemplate(OCE.PMOVSXWD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9777, OF.IF_SSE41, OF.IF_SQ),
    
};

        static itemplate[] instrux_PMOVSXWQ = new[] {
    new itemplate(OCE.PMOVSXWQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9784, OF.IF_SSE41, OF.IF_SD),
    
};

        static itemplate[] instrux_PMOVZXBD = new[] {
    new itemplate(OCE.PMOVZXBD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9805, OF.IF_SSE41, OF.IF_SD),
    
};

        static itemplate[] instrux_PMOVZXBQ = new[] {
    new itemplate(OCE.PMOVZXBQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9812, OF.IF_SSE41, OF.IF_SW),
    
};

        static itemplate[] instrux_PMOVZXBW = new[] {
    new itemplate(OCE.PMOVZXBW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9798, OF.IF_SSE41, OF.IF_SQ),
    
};

        static itemplate[] instrux_PMOVZXDQ = new[] {
    new itemplate(OCE.PMOVZXDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9833, OF.IF_SSE41, OF.IF_SQ),
    
};

        static itemplate[] instrux_PMOVZXWD = new[] {
    new itemplate(OCE.PMOVZXWD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9819, OF.IF_SSE41, OF.IF_SQ),
    
};

        static itemplate[] instrux_PMOVZXWQ = new[] {
    new itemplate(OCE.PMOVZXWQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9826, OF.IF_SSE41, OF.IF_SD),
    
};

        static itemplate[] instrux_PMULDQ = new[] {
    new itemplate(OCE.PMULDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9840, OF.IF_SSE41),
    
};

        static itemplate[] instrux_PMULHRIW = new[] {
    new itemplate(OCE.PMULHRIW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,16467, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_PMULHRSW = new[] {
    new itemplate(OCE.PMULHRSW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9560, OF.IF_SSSE3, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PMULHRSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9567, OF.IF_SSSE3),
    
};

        static itemplate[] instrux_PMULHRWA = new[] {
    new itemplate(OCE.PMULHRWA, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6732, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PMULHRWC = new[] {
    new itemplate(OCE.PMULHRWC, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,16473, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_PMULHUW = new[] {
    new itemplate(OCE.PMULHUW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9203, OF.IF_KATMAI, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PMULHUW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17415, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PMULHW = new[] {
    new itemplate(OCE.PMULHW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8748, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PMULHW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17421, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PMULLD = new[] {
    new itemplate(OCE.PMULLD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9847, OF.IF_SSE41),
    
};

        static itemplate[] instrux_PMULLW = new[] {
    new itemplate(OCE.PMULLW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8755, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PMULLW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17427, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PMULUDQ = new[] {
    new itemplate(OCE.PMULUDQ, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9252, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    new itemplate(OCE.PMULUDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17433, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PMVGEZB = new[] {
    new itemplate(OCE.PMVGEZB, 2, new ulong[] {MMXREG,MEMORY,0,0,0} ,17848, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_PMVLZB = new[] {
    new itemplate(OCE.PMVLZB, 2, new ulong[] {MMXREG,MEMORY,0,0,0} ,17704, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_PMVNZB = new[] {
    new itemplate(OCE.PMVNZB, 2, new ulong[] {MMXREG,MEMORY,0,0,0} ,17686, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_PMVZB = new[] {
    new itemplate(OCE.PMVZB, 2, new ulong[] {MMXREG,MEMORY,0,0,0} ,17608, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_POP = new[] {
    new itemplate(OCE.POP, 1, new ulong[] {REG16,0,0,0,0} ,21803, OF.IF_8086),
    new itemplate(OCE.POP, 1, new ulong[] {REG32,0,0,0,0} ,21807, OF.IF_386, OF.IF_NOLONG),
    new itemplate(OCE.POP, 1, new ulong[] {REG64,0,0,0,0} ,21811, OF.IF_X64),
    new itemplate(OCE.POP, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,20352, OF.IF_8086),
    new itemplate(OCE.POP, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,20357, OF.IF_386, OF.IF_NOLONG),
    new itemplate(OCE.POP, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,20362, OF.IF_X64),
    new itemplate(OCE.POP, 1, new ulong[] {REG_CS,0,0,0,0} ,4407, OF.IF_8086, OF.IF_UNDOC),
    new itemplate(OCE.POP, 1, new ulong[] {REG_DESS,0,0,0,0} ,21621, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.POP, 1, new ulong[] {REG_FSGS,0,0,0,0} ,21815, OF.IF_386),
    
};

        static itemplate[] instrux_POPA = new[] {
    new itemplate(OCE.POPA, 0, new ulong[] {0,0,0,0,0} ,21819, OF.IF_186, OF.IF_NOLONG),
    
};

        static itemplate[] instrux_POPAD = new[] {
    new itemplate(OCE.POPAD, 0, new ulong[] {0,0,0,0,0} ,21823, OF.IF_386, OF.IF_NOLONG),
    
};

        static itemplate[] instrux_POPAW = new[] {
    new itemplate(OCE.POPAW, 0, new ulong[] {0,0,0,0,0} ,21827, OF.IF_186, OF.IF_NOLONG),
    
};

        static itemplate[] instrux_POPCNT = new[] {
    new itemplate(OCE.POPCNT, 2, new ulong[] {REG16,RM_GPR, BITS16,0,0,0} ,9868, OF.IF_NEHALEM, OF.IF_SW),
    new itemplate(OCE.POPCNT, 2, new ulong[] {REG32,RM_GPR, BITS32,0,0,0} ,9875, OF.IF_NEHALEM, OF.IF_SD),
    new itemplate(OCE.POPCNT, 2, new ulong[] {REG64,RM_GPR, BITS64,0,0,0} ,9882, OF.IF_NEHALEM, OF.IF_SQ, OF.IF_X64),
    
};

        static itemplate[] instrux_POPF = new[] {
    new itemplate(OCE.POPF, 0, new ulong[] {0,0,0,0,0} ,21831, OF.IF_8086),
    
};

        static itemplate[] instrux_POPFD = new[] {
    new itemplate(OCE.POPFD, 0, new ulong[] {0,0,0,0,0} ,21835, OF.IF_386, OF.IF_NOLONG),
    
};

        static itemplate[] instrux_POPFQ = new[] {
    new itemplate(OCE.POPFQ, 0, new ulong[] {0,0,0,0,0} ,21835, OF.IF_X64),
    
};

        static itemplate[] instrux_POPFW = new[] {
    new itemplate(OCE.POPFW, 0, new ulong[] {0,0,0,0,0} ,21839, OF.IF_8086),
    
};

        static itemplate[] instrux_POR = new[] {
    new itemplate(OCE.POR, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8762, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.POR, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17439, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PPERM = new[] {
    new itemplate(OCE.PPERM, 4, new ulong[] {XMMREG,SAME_AS, 0,XMMREG,RM_XMM,0} ,10421, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PPERM, 4, new ulong[] {XMMREG,SAME_AS, 0,RM_XMM,XMMREG,0} ,10428, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PPERM, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,SAME_AS, 0,0} ,10435, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PPERM, 4, new ulong[] {XMMREG,RM_XMM,XMMREG,SAME_AS, 0,0} ,10442, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PREFETCH = new[] {
    new itemplate(OCE.PREFETCH, 1, new ulong[] {MEMORY,0,0,0,0} ,20367, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PREFETCHNTA = new[] {
    new itemplate(OCE.PREFETCHNTA, 1, new ulong[] {MEMORY,0,0,0,0} ,18046, OF.IF_KATMAI),
    
};

        static itemplate[] instrux_PREFETCHT0 = new[] {
    new itemplate(OCE.PREFETCHT0, 1, new ulong[] {MEMORY,0,0,0,0} ,18064, OF.IF_KATMAI),
    
};

        static itemplate[] instrux_PREFETCHT1 = new[] {
    new itemplate(OCE.PREFETCHT1, 1, new ulong[] {MEMORY,0,0,0,0} ,18082, OF.IF_KATMAI),
    
};

        static itemplate[] instrux_PREFETCHT2 = new[] {
    new itemplate(OCE.PREFETCHT2, 1, new ulong[] {MEMORY,0,0,0,0} ,18100, OF.IF_KATMAI),
    
};

        static itemplate[] instrux_PREFETCHW = new[] {
    new itemplate(OCE.PREFETCHW, 1, new ulong[] {MEMORY,0,0,0,0} ,20372, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PROTB = new[] {
    new itemplate(OCE.PROTB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,10533, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PROTB, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,10540, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PROTB, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7340, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PROTD = new[] {
    new itemplate(OCE.PROTD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,10561, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PROTD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,10568, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PROTD, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7356, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PROTQ = new[] {
    new itemplate(OCE.PROTQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,10575, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PROTQ, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,10582, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PROTQ, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7364, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PROTW = new[] {
    new itemplate(OCE.PROTW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,10547, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PROTW, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,10554, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PROTW, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7348, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PSADBW = new[] {
    new itemplate(OCE.PSADBW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9210, OF.IF_KATMAI, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PSADBW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17445, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PSHAB = new[] {
    new itemplate(OCE.PSHAB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,10645, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PSHAB, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,10652, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PSHAD = new[] {
    new itemplate(OCE.PSHAD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,10673, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PSHAD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,10680, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PSHAQ = new[] {
    new itemplate(OCE.PSHAQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,10687, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PSHAQ, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,10694, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PSHAW = new[] {
    new itemplate(OCE.PSHAW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,10659, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PSHAW, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,10666, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PSHLB = new[] {
    new itemplate(OCE.PSHLB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,10589, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PSHLB, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,10596, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PSHLD = new[] {
    new itemplate(OCE.PSHLD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,10617, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PSHLD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,10624, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PSHLQ = new[] {
    new itemplate(OCE.PSHLQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,10631, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PSHLQ, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,10638, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PSHLW = new[] {
    new itemplate(OCE.PSHLW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,10603, OF.IF_SSE5, OF.IF_AMD),
    new itemplate(OCE.PSHLW, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,10610, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_PSHUFB = new[] {
    new itemplate(OCE.PSHUFB, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9574, OF.IF_SSSE3, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PSHUFB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9581, OF.IF_SSSE3),
    
};

        static itemplate[] instrux_PSHUFD = new[] {
    new itemplate(OCE.PSHUFD, 3, new ulong[] {XMMREG,XMMREG,IMMEDIATE,0,0} ,9259, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.PSHUFD, 3, new ulong[] {XMMREG,MEMORY,IMMEDIATE,0,0} ,9259, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SM2, OF.IF_SB, OF.IF_AR2),
    
};

        static itemplate[] instrux_PSHUFHW = new[] {
    new itemplate(OCE.PSHUFHW, 3, new ulong[] {XMMREG,XMMREG,IMMEDIATE,0,0} ,9266, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.PSHUFHW, 3, new ulong[] {XMMREG,MEMORY,IMMEDIATE,0,0} ,9266, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SM2, OF.IF_SB, OF.IF_AR2),
    
};

        static itemplate[] instrux_PSHUFLW = new[] {
    new itemplate(OCE.PSHUFLW, 3, new ulong[] {XMMREG,XMMREG,IMMEDIATE,0,0} ,9273, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.PSHUFLW, 3, new ulong[] {XMMREG,MEMORY,IMMEDIATE,0,0} ,9273, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SM2, OF.IF_SB, OF.IF_AR2),
    
};

        static itemplate[] instrux_PSHUFW = new[] {
    new itemplate(OCE.PSHUFW, 3, new ulong[] {MMXREG,RM_MMX,IMMEDIATE,0,0} ,6868, OF.IF_KATMAI, OF.IF_MMX, OF.IF_SM2, OF.IF_SB, OF.IF_AR2),
    
};

        static itemplate[] instrux_PSIGNB = new[] {
    new itemplate(OCE.PSIGNB, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9588, OF.IF_SSSE3, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PSIGNB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9595, OF.IF_SSSE3),
    
};

        static itemplate[] instrux_PSIGND = new[] {
    new itemplate(OCE.PSIGND, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9616, OF.IF_SSSE3, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PSIGND, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9623, OF.IF_SSSE3),
    
};

        static itemplate[] instrux_PSIGNW = new[] {
    new itemplate(OCE.PSIGNW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9602, OF.IF_SSSE3, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PSIGNW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9609, OF.IF_SSSE3),
    
};

        static itemplate[] instrux_PSLLD = new[] {
    new itemplate(OCE.PSLLD, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8769, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PSLLD, 2, new ulong[] {MMXREG,IMMEDIATE,0,0,0} ,8776, OF.IF_PENT, OF.IF_MMX),
    new itemplate(OCE.PSLLD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17457, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    new itemplate(OCE.PSLLD, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,9294, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SB, OF.IF_AR1),
    
};

        static itemplate[] instrux_PSLLDQ = new[] {
    new itemplate(OCE.PSLLDQ, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,9280, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SB, OF.IF_AR1),
    
};

        static itemplate[] instrux_PSLLQ = new[] {
    new itemplate(OCE.PSLLQ, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8783, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PSLLQ, 2, new ulong[] {MMXREG,IMMEDIATE,0,0,0} ,8790, OF.IF_PENT, OF.IF_MMX),
    new itemplate(OCE.PSLLQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17463, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    new itemplate(OCE.PSLLQ, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,9301, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SB, OF.IF_AR1),
    
};

        static itemplate[] instrux_PSLLW = new[] {
    new itemplate(OCE.PSLLW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8797, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PSLLW, 2, new ulong[] {MMXREG,IMMEDIATE,0,0,0} ,8804, OF.IF_PENT, OF.IF_MMX),
    new itemplate(OCE.PSLLW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17451, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    new itemplate(OCE.PSLLW, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,9287, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SB, OF.IF_AR1),
    
};

        static itemplate[] instrux_PSRAD = new[] {
    new itemplate(OCE.PSRAD, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8811, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PSRAD, 2, new ulong[] {MMXREG,IMMEDIATE,0,0,0} ,8818, OF.IF_PENT, OF.IF_MMX),
    new itemplate(OCE.PSRAD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17475, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    new itemplate(OCE.PSRAD, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,9315, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SB, OF.IF_AR1),
    
};

        static itemplate[] instrux_PSRAW = new[] {
    new itemplate(OCE.PSRAW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8825, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PSRAW, 2, new ulong[] {MMXREG,IMMEDIATE,0,0,0} ,8832, OF.IF_PENT, OF.IF_MMX),
    new itemplate(OCE.PSRAW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17469, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    new itemplate(OCE.PSRAW, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,9308, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SB, OF.IF_AR1),
    
};

        static itemplate[] instrux_PSRLD = new[] {
    new itemplate(OCE.PSRLD, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8839, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PSRLD, 2, new ulong[] {MMXREG,IMMEDIATE,0,0,0} ,8846, OF.IF_PENT, OF.IF_MMX),
    new itemplate(OCE.PSRLD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17487, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    new itemplate(OCE.PSRLD, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,9336, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SB, OF.IF_AR1),
    
};

        static itemplate[] instrux_PSRLDQ = new[] {
    new itemplate(OCE.PSRLDQ, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,9322, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SB, OF.IF_AR1),
    
};

        static itemplate[] instrux_PSRLQ = new[] {
    new itemplate(OCE.PSRLQ, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8853, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PSRLQ, 2, new ulong[] {MMXREG,IMMEDIATE,0,0,0} ,8860, OF.IF_PENT, OF.IF_MMX),
    new itemplate(OCE.PSRLQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17493, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    new itemplate(OCE.PSRLQ, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,9343, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SB, OF.IF_AR1),
    
};

        static itemplate[] instrux_PSRLW = new[] {
    new itemplate(OCE.PSRLW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8867, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PSRLW, 2, new ulong[] {MMXREG,IMMEDIATE,0,0,0} ,8874, OF.IF_PENT, OF.IF_MMX),
    new itemplate(OCE.PSRLW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17481, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    new itemplate(OCE.PSRLW, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,9329, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SB, OF.IF_AR1),
    
};

        static itemplate[] instrux_PSUBB = new[] {
    new itemplate(OCE.PSUBB, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8881, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PSUBB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17499, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PSUBD = new[] {
    new itemplate(OCE.PSUBD, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8888, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PSUBD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17511, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PSUBQ = new[] {
    new itemplate(OCE.PSUBQ, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,9350, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    new itemplate(OCE.PSUBQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17517, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PSUBSB = new[] {
    new itemplate(OCE.PSUBSB, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8895, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PSUBSB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17523, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PSUBSIW = new[] {
    new itemplate(OCE.PSUBSIW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,16479, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_PSUBSW = new[] {
    new itemplate(OCE.PSUBSW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8902, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PSUBSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17529, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PSUBUSB = new[] {
    new itemplate(OCE.PSUBUSB, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8909, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PSUBUSB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17535, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PSUBUSW = new[] {
    new itemplate(OCE.PSUBUSW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8916, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PSUBUSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17541, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PSUBW = new[] {
    new itemplate(OCE.PSUBW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8923, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PSUBW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17505, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PSWAPD = new[] {
    new itemplate(OCE.PSWAPD, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,6908, OF.IF_PENT, OF.IF_3DNOW, OF.IF_SQ),
    
};

        static itemplate[] instrux_PTEST = new[] {
    new itemplate(OCE.PTEST, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,9854, OF.IF_SSE41),
    
};

        static itemplate[] instrux_PUNPCKHBW = new[] {
    new itemplate(OCE.PUNPCKHBW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8930, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PUNPCKHBW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17547, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PUNPCKHDQ = new[] {
    new itemplate(OCE.PUNPCKHDQ, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8937, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PUNPCKHDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17559, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PUNPCKHQDQ = new[] {
    new itemplate(OCE.PUNPCKHQDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17565, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PUNPCKHWD = new[] {
    new itemplate(OCE.PUNPCKHWD, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8944, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PUNPCKHWD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17553, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PUNPCKLBW = new[] {
    new itemplate(OCE.PUNPCKLBW, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8951, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PUNPCKLBW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17571, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PUNPCKLDQ = new[] {
    new itemplate(OCE.PUNPCKLDQ, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8958, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PUNPCKLDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17583, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PUNPCKLQDQ = new[] {
    new itemplate(OCE.PUNPCKLQDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17589, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PUNPCKLWD = new[] {
    new itemplate(OCE.PUNPCKLWD, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8965, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PUNPCKLWD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17577, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_PUSH = new[] {
    new itemplate(OCE.PUSH, 1, new ulong[] {REG16,0,0,0,0} ,21843, OF.IF_8086),
    new itemplate(OCE.PUSH, 1, new ulong[] {REG32,0,0,0,0} ,21847, OF.IF_386, OF.IF_NOLONG),
    new itemplate(OCE.PUSH, 1, new ulong[] {REG64,0,0,0,0} ,21851, OF.IF_X64),
    new itemplate(OCE.PUSH, 1, new ulong[] {RM_GPR, BITS16,0,0,0,0} ,20377, OF.IF_8086),
    new itemplate(OCE.PUSH, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,20382, OF.IF_386, OF.IF_NOLONG),
    new itemplate(OCE.PUSH, 1, new ulong[] {RM_GPR, BITS64,0,0,0,0} ,20387, OF.IF_X64),
    new itemplate(OCE.PUSH, 1, new ulong[] {REG_CS,0,0,0,0} ,21597, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.PUSH, 1, new ulong[] {REG_DESS,0,0,0,0} ,21597, OF.IF_8086, OF.IF_NOLONG),
    new itemplate(OCE.PUSH, 1, new ulong[] {REG_FSGS,0,0,0,0} ,21855, OF.IF_386),
    new itemplate(OCE.PUSH, 1, new ulong[] {IMMEDIATE, BITS8,0,0,0,0} ,21859, OF.IF_186),
    new itemplate(OCE.PUSH, 1, new ulong[] {IMMEDIATE, BITS16,0,0,0,0} ,20392, OF.IF_186, OF.IF_AR0, OF.IF_SZ),
    new itemplate(OCE.PUSH, 1, new ulong[] {IMMEDIATE, BITS32,0,0,0,0} ,20397, OF.IF_386, OF.IF_NOLONG, OF.IF_AR0, OF.IF_SZ),
    new itemplate(OCE.PUSH, 1, new ulong[] {IMMEDIATE, BITS32,0,0,0,0} ,20397, OF.IF_386, OF.IF_NOLONG, OF.IF_SD),
    new itemplate(OCE.PUSH, 1, new ulong[] {IMMEDIATE, BITS64,0,0,0,0} ,20402, OF.IF_X64, OF.IF_AR0, OF.IF_SZ),
    
};

        static itemplate[] instrux_PUSHA = new[] {
    new itemplate(OCE.PUSHA, 0, new ulong[] {0,0,0,0,0} ,21863, OF.IF_186, OF.IF_NOLONG),
    
};

        static itemplate[] instrux_PUSHAD = new[] {
    new itemplate(OCE.PUSHAD, 0, new ulong[] {0,0,0,0,0} ,21867, OF.IF_386, OF.IF_NOLONG),
    
};

        static itemplate[] instrux_PUSHAW = new[] {
    new itemplate(OCE.PUSHAW, 0, new ulong[] {0,0,0,0,0} ,21871, OF.IF_186, OF.IF_NOLONG),
    
};

        static itemplate[] instrux_PUSHF = new[] {
    new itemplate(OCE.PUSHF, 0, new ulong[] {0,0,0,0,0} ,21875, OF.IF_8086),
    
};

        static itemplate[] instrux_PUSHFD = new[] {
    new itemplate(OCE.PUSHFD, 0, new ulong[] {0,0,0,0,0} ,21879, OF.IF_386, OF.IF_NOLONG),
    
};

        static itemplate[] instrux_PUSHFQ = new[] {
    new itemplate(OCE.PUSHFQ, 0, new ulong[] {0,0,0,0,0} ,21879, OF.IF_X64),
    
};

        static itemplate[] instrux_PUSHFW = new[] {
    new itemplate(OCE.PUSHFW, 0, new ulong[] {0,0,0,0,0} ,21883, OF.IF_8086),
    
};

        static itemplate[] instrux_PXOR = new[] {
    new itemplate(OCE.PXOR, 2, new ulong[] {MMXREG,RM_MMX,0,0,0} ,8972, OF.IF_PENT, OF.IF_MMX, OF.IF_SQ),
    new itemplate(OCE.PXOR, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17595, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_RCL = new[] {
    new itemplate(OCE.RCL, 2, new ulong[] {RM_GPR, BITS8,UNITY,0,0,0} ,21887, OF.IF_8086),
    new itemplate(OCE.RCL, 2, new ulong[] {RM_GPR, BITS8,REG_CL,0,0,0} ,21891, OF.IF_8086),
    new itemplate(OCE.RCL, 2, new ulong[] {RM_GPR, BITS8,IMMEDIATE,0,0,0} ,20407, OF.IF_186, OF.IF_SB),
    new itemplate(OCE.RCL, 2, new ulong[] {RM_GPR, BITS16,UNITY,0,0,0} ,20412, OF.IF_8086),
    new itemplate(OCE.RCL, 2, new ulong[] {RM_GPR, BITS16,REG_CL,0,0,0} ,20417, OF.IF_8086),
    new itemplate(OCE.RCL, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,16485, OF.IF_186, OF.IF_SB),
    new itemplate(OCE.RCL, 2, new ulong[] {RM_GPR, BITS32,UNITY,0,0,0} ,20422, OF.IF_386),
    new itemplate(OCE.RCL, 2, new ulong[] {RM_GPR, BITS32,REG_CL,0,0,0} ,20427, OF.IF_386),
    new itemplate(OCE.RCL, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,16491, OF.IF_386, OF.IF_SB),
    new itemplate(OCE.RCL, 2, new ulong[] {RM_GPR, BITS64,UNITY,0,0,0} ,20432, OF.IF_X64),
    new itemplate(OCE.RCL, 2, new ulong[] {RM_GPR, BITS64,REG_CL,0,0,0} ,20437, OF.IF_X64),
    new itemplate(OCE.RCL, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,16497, OF.IF_X64, OF.IF_SB),
    
};

        static itemplate[] instrux_RCPPS = new[] {
    new itemplate(OCE.RCPPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17055, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_RCPSS = new[] {
    new itemplate(OCE.RCPSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17061, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_RCR = new[] {
    new itemplate(OCE.RCR, 2, new ulong[] {RM_GPR, BITS8,UNITY,0,0,0} ,21895, OF.IF_8086),
    new itemplate(OCE.RCR, 2, new ulong[] {RM_GPR, BITS8,REG_CL,0,0,0} ,21899, OF.IF_8086),
    new itemplate(OCE.RCR, 2, new ulong[] {RM_GPR, BITS8,IMMEDIATE,0,0,0} ,20442, OF.IF_186, OF.IF_SB),
    new itemplate(OCE.RCR, 2, new ulong[] {RM_GPR, BITS16,UNITY,0,0,0} ,20447, OF.IF_8086),
    new itemplate(OCE.RCR, 2, new ulong[] {RM_GPR, BITS16,REG_CL,0,0,0} ,20452, OF.IF_8086),
    new itemplate(OCE.RCR, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,16503, OF.IF_186, OF.IF_SB),
    new itemplate(OCE.RCR, 2, new ulong[] {RM_GPR, BITS32,UNITY,0,0,0} ,20457, OF.IF_386),
    new itemplate(OCE.RCR, 2, new ulong[] {RM_GPR, BITS32,REG_CL,0,0,0} ,20462, OF.IF_386),
    new itemplate(OCE.RCR, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,16509, OF.IF_386, OF.IF_SB),
    new itemplate(OCE.RCR, 2, new ulong[] {RM_GPR, BITS64,UNITY,0,0,0} ,20467, OF.IF_X64),
    new itemplate(OCE.RCR, 2, new ulong[] {RM_GPR, BITS64,REG_CL,0,0,0} ,20472, OF.IF_X64),
    new itemplate(OCE.RCR, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,16515, OF.IF_X64, OF.IF_SB),
    
};

        static itemplate[] instrux_RDM = new[] {
    new itemplate(OCE.RDM, 0, new ulong[] {0,0,0,0,0} ,21091, OF.IF_P6, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_RDMSR = new[] {
    new itemplate(OCE.RDMSR, 0, new ulong[] {0,0,0,0,0} ,21903, OF.IF_PENT, OF.IF_PRIV),
    
};

        static itemplate[] instrux_RDPMC = new[] {
    new itemplate(OCE.RDPMC, 0, new ulong[] {0,0,0,0,0} ,21907, OF.IF_P6),
    
};

        static itemplate[] instrux_RDSHR = new[] {
    new itemplate(OCE.RDSHR, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,16521, OF.IF_P6, OF.IF_CYRIX, OF.IF_SMM),
    
};

        static itemplate[] instrux_RDTSC = new[] {
    new itemplate(OCE.RDTSC, 0, new ulong[] {0,0,0,0,0} ,21911, OF.IF_PENT),
    
};

        static itemplate[] instrux_RDTSCP = new[] {
    new itemplate(OCE.RDTSCP, 0, new ulong[] {0,0,0,0,0} ,20477, OF.IF_X86_64),
    
};

        static itemplate[] instrux_RESB = new[] {
    new itemplate(OCE.RESB, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,21205, OF.IF_8086),
    
};

        static itemplate[] instrux_RESD;// = new[] {};

        static itemplate[] instrux_RESO;// = new[] {};

        static itemplate[] instrux_RESQ;// = new[] {};

        static itemplate[] instrux_REST;// = new[] {};

        static itemplate[] instrux_RESW;// = new[] {};

        static itemplate[] instrux_RESY;// = new[] {};

        static itemplate[] instrux_RET = new[] {
    new itemplate(OCE.RET, 0, new ulong[] {0,0,0,0,0} ,21024, OF.IF_8086),
    new itemplate(OCE.RET, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,21915, OF.IF_8086, OF.IF_SW),
    
};

        static itemplate[] instrux_RETF = new[] {
    new itemplate(OCE.RETF, 0, new ulong[] {0,0,0,0,0} ,22146, OF.IF_8086),
    new itemplate(OCE.RETF, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,21919, OF.IF_8086, OF.IF_SW),
    
};

        static itemplate[] instrux_RETN = new[] {
    new itemplate(OCE.RETN, 0, new ulong[] {0,0,0,0,0} ,21024, OF.IF_8086),
    new itemplate(OCE.RETN, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,21915, OF.IF_8086, OF.IF_SW),
    
};

        static itemplate[] instrux_ROL = new[] {
    new itemplate(OCE.ROL, 2, new ulong[] {RM_GPR, BITS8,UNITY,0,0,0} ,21923, OF.IF_8086),
    new itemplate(OCE.ROL, 2, new ulong[] {RM_GPR, BITS8,REG_CL,0,0,0} ,21927, OF.IF_8086),
    new itemplate(OCE.ROL, 2, new ulong[] {RM_GPR, BITS8,IMMEDIATE,0,0,0} ,20482, OF.IF_186, OF.IF_SB),
    new itemplate(OCE.ROL, 2, new ulong[] {RM_GPR, BITS16,UNITY,0,0,0} ,20487, OF.IF_8086),
    new itemplate(OCE.ROL, 2, new ulong[] {RM_GPR, BITS16,REG_CL,0,0,0} ,20492, OF.IF_8086),
    new itemplate(OCE.ROL, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,16527, OF.IF_186, OF.IF_SB),
    new itemplate(OCE.ROL, 2, new ulong[] {RM_GPR, BITS32,UNITY,0,0,0} ,20497, OF.IF_386),
    new itemplate(OCE.ROL, 2, new ulong[] {RM_GPR, BITS32,REG_CL,0,0,0} ,20502, OF.IF_386),
    new itemplate(OCE.ROL, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,16533, OF.IF_386, OF.IF_SB),
    new itemplate(OCE.ROL, 2, new ulong[] {RM_GPR, BITS64,UNITY,0,0,0} ,20507, OF.IF_X64),
    new itemplate(OCE.ROL, 2, new ulong[] {RM_GPR, BITS64,REG_CL,0,0,0} ,20512, OF.IF_X64),
    new itemplate(OCE.ROL, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,16539, OF.IF_X64, OF.IF_SB),
    
};

        static itemplate[] instrux_ROR = new[] {
    new itemplate(OCE.ROR, 2, new ulong[] {RM_GPR, BITS8,UNITY,0,0,0} ,21931, OF.IF_8086),
    new itemplate(OCE.ROR, 2, new ulong[] {RM_GPR, BITS8,REG_CL,0,0,0} ,21935, OF.IF_8086),
    new itemplate(OCE.ROR, 2, new ulong[] {RM_GPR, BITS8,IMMEDIATE,0,0,0} ,20517, OF.IF_186, OF.IF_SB),
    new itemplate(OCE.ROR, 2, new ulong[] {RM_GPR, BITS16,UNITY,0,0,0} ,20522, OF.IF_8086),
    new itemplate(OCE.ROR, 2, new ulong[] {RM_GPR, BITS16,REG_CL,0,0,0} ,20527, OF.IF_8086),
    new itemplate(OCE.ROR, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,16545, OF.IF_186, OF.IF_SB),
    new itemplate(OCE.ROR, 2, new ulong[] {RM_GPR, BITS32,UNITY,0,0,0} ,20532, OF.IF_386),
    new itemplate(OCE.ROR, 2, new ulong[] {RM_GPR, BITS32,REG_CL,0,0,0} ,20537, OF.IF_386),
    new itemplate(OCE.ROR, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,16551, OF.IF_386, OF.IF_SB),
    new itemplate(OCE.ROR, 2, new ulong[] {RM_GPR, BITS64,UNITY,0,0,0} ,20542, OF.IF_X64),
    new itemplate(OCE.ROR, 2, new ulong[] {RM_GPR, BITS64,REG_CL,0,0,0} ,20547, OF.IF_X64),
    new itemplate(OCE.ROR, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,16557, OF.IF_X64, OF.IF_SB),
    
};

        static itemplate[] instrux_ROUNDPD = new[] {
    new itemplate(OCE.ROUNDPD, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7148, OF.IF_SSE41),
    new itemplate(OCE.ROUNDPD, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7156, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_ROUNDPS = new[] {
    new itemplate(OCE.ROUNDPS, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7156, OF.IF_SSE41),
    new itemplate(OCE.ROUNDPS, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7156, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_ROUNDSD = new[] {
    new itemplate(OCE.ROUNDSD, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7164, OF.IF_SSE41),
    new itemplate(OCE.ROUNDSD, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7156, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_ROUNDSS = new[] {
    new itemplate(OCE.ROUNDSS, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7172, OF.IF_SSE41),
    new itemplate(OCE.ROUNDSS, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7156, OF.IF_SSE5, OF.IF_AMD),
    
};

        static itemplate[] instrux_RSDC = new[] {
    new itemplate(OCE.RSDC, 2, new ulong[] {REG_SREG,MEMORY, BITS80,0,0,0} ,17956, OF.IF_486, OF.IF_CYRIX, OF.IF_SMM),
    
};

        static itemplate[] instrux_RSLDT = new[] {
    new itemplate(OCE.RSLDT, 1, new ulong[] {MEMORY, BITS80,0,0,0,0} ,20552, OF.IF_486, OF.IF_CYRIX, OF.IF_SMM),
    
};

        static itemplate[] instrux_RSM = new[] {
    new itemplate(OCE.RSM, 0, new ulong[] {0,0,0,0,0} ,21939, OF.IF_PENT, OF.IF_SMM),
    
};

        static itemplate[] instrux_RSQRTPS = new[] {
    new itemplate(OCE.RSQRTPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17067, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_RSQRTSS = new[] {
    new itemplate(OCE.RSQRTSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17073, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_RSTS = new[] {
    new itemplate(OCE.RSTS, 1, new ulong[] {MEMORY, BITS80,0,0,0,0} ,20557, OF.IF_486, OF.IF_CYRIX, OF.IF_SMM),
    
};

        static itemplate[] instrux_SAHF = new[] {
    new itemplate(OCE.SAHF, 0, new ulong[] {0,0,0,0,0} ,6617, OF.IF_8086),
    
};

        static itemplate[] instrux_SAL = new[] {
    new itemplate(OCE.SAL, 2, new ulong[] {RM_GPR, BITS8,UNITY,0,0,0} ,21943, OF.IF_8086),
    new itemplate(OCE.SAL, 2, new ulong[] {RM_GPR, BITS8,REG_CL,0,0,0} ,21947, OF.IF_8086),
    new itemplate(OCE.SAL, 2, new ulong[] {RM_GPR, BITS8,IMMEDIATE,0,0,0} ,20562, OF.IF_186, OF.IF_SB),
    new itemplate(OCE.SAL, 2, new ulong[] {RM_GPR, BITS16,UNITY,0,0,0} ,20567, OF.IF_8086),
    new itemplate(OCE.SAL, 2, new ulong[] {RM_GPR, BITS16,REG_CL,0,0,0} ,20572, OF.IF_8086),
    new itemplate(OCE.SAL, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,16563, OF.IF_186, OF.IF_SB),
    new itemplate(OCE.SAL, 2, new ulong[] {RM_GPR, BITS32,UNITY,0,0,0} ,20577, OF.IF_386),
    new itemplate(OCE.SAL, 2, new ulong[] {RM_GPR, BITS32,REG_CL,0,0,0} ,20582, OF.IF_386),
    new itemplate(OCE.SAL, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,16569, OF.IF_386, OF.IF_SB),
    new itemplate(OCE.SAL, 2, new ulong[] {RM_GPR, BITS64,UNITY,0,0,0} ,20587, OF.IF_X64),
    new itemplate(OCE.SAL, 2, new ulong[] {RM_GPR, BITS64,REG_CL,0,0,0} ,20592, OF.IF_X64),
    new itemplate(OCE.SAL, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,16575, OF.IF_X64, OF.IF_SB),
    
};

        static itemplate[] instrux_SALC = new[] {
    new itemplate(OCE.SALC, 0, new ulong[] {0,0,0,0,0} ,22149, OF.IF_8086, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_SAR = new[] {
    new itemplate(OCE.SAR, 2, new ulong[] {RM_GPR, BITS8,UNITY,0,0,0} ,21951, OF.IF_8086),
    new itemplate(OCE.SAR, 2, new ulong[] {RM_GPR, BITS8,REG_CL,0,0,0} ,21955, OF.IF_8086),
    new itemplate(OCE.SAR, 2, new ulong[] {RM_GPR, BITS8,IMMEDIATE,0,0,0} ,20597, OF.IF_186, OF.IF_SB),
    new itemplate(OCE.SAR, 2, new ulong[] {RM_GPR, BITS16,UNITY,0,0,0} ,20602, OF.IF_8086),
    new itemplate(OCE.SAR, 2, new ulong[] {RM_GPR, BITS16,REG_CL,0,0,0} ,20607, OF.IF_8086),
    new itemplate(OCE.SAR, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,16581, OF.IF_186, OF.IF_SB),
    new itemplate(OCE.SAR, 2, new ulong[] {RM_GPR, BITS32,UNITY,0,0,0} ,20612, OF.IF_386),
    new itemplate(OCE.SAR, 2, new ulong[] {RM_GPR, BITS32,REG_CL,0,0,0} ,20617, OF.IF_386),
    new itemplate(OCE.SAR, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,16587, OF.IF_386, OF.IF_SB),
    new itemplate(OCE.SAR, 2, new ulong[] {RM_GPR, BITS64,UNITY,0,0,0} ,20622, OF.IF_X64),
    new itemplate(OCE.SAR, 2, new ulong[] {RM_GPR, BITS64,REG_CL,0,0,0} ,20627, OF.IF_X64),
    new itemplate(OCE.SAR, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,16593, OF.IF_X64, OF.IF_SB),
    
};

        static itemplate[] instrux_SBB = new[] {
    new itemplate(OCE.SBB, 2, new ulong[] {MEMORY,REG8,0,0,0} ,21959, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SBB, 2, new ulong[] {REG8,REG8,0,0,0} ,21959, OF.IF_8086),
    new itemplate(OCE.SBB, 2, new ulong[] {MEMORY,REG16,0,0,0} ,20632, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SBB, 2, new ulong[] {REG16,REG16,0,0,0} ,20632, OF.IF_8086),
    new itemplate(OCE.SBB, 2, new ulong[] {MEMORY,REG32,0,0,0} ,20637, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.SBB, 2, new ulong[] {REG32,REG32,0,0,0} ,20637, OF.IF_386),
    new itemplate(OCE.SBB, 2, new ulong[] {MEMORY,REG64,0,0,0} ,20642, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.SBB, 2, new ulong[] {REG64,REG64,0,0,0} ,20642, OF.IF_X64),
    new itemplate(OCE.SBB, 2, new ulong[] {REG8,MEMORY,0,0,0} ,11292, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SBB, 2, new ulong[] {REG8,REG8,0,0,0} ,11292, OF.IF_8086),
    new itemplate(OCE.SBB, 2, new ulong[] {REG16,MEMORY,0,0,0} ,20647, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SBB, 2, new ulong[] {REG16,REG16,0,0,0} ,20647, OF.IF_8086),
    new itemplate(OCE.SBB, 2, new ulong[] {REG32,MEMORY,0,0,0} ,20652, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.SBB, 2, new ulong[] {REG32,REG32,0,0,0} ,20652, OF.IF_386),
    new itemplate(OCE.SBB, 2, new ulong[] {REG64,MEMORY,0,0,0} ,20657, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.SBB, 2, new ulong[] {REG64,REG64,0,0,0} ,20657, OF.IF_X64),
    new itemplate(OCE.SBB, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE, BITS8,0,0,0} ,16599, OF.IF_8086),
    new itemplate(OCE.SBB, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE, BITS8,0,0,0} ,16605, OF.IF_386),
    new itemplate(OCE.SBB, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE, BITS8,0,0,0} ,16611, OF.IF_X64),
    new itemplate(OCE.SBB, 2, new ulong[] {REG_AL,IMMEDIATE,0,0,0} ,21963, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SBB, 2, new ulong[] {REG_AX,SBYTE16,0,0,0} ,16599, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SBB, 2, new ulong[] {REG_AX,IMMEDIATE,0,0,0} ,20662, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SBB, 2, new ulong[] {REG_EAX,SBYTE32,0,0,0} ,16605, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.SBB, 2, new ulong[] {REG_EAX,IMMEDIATE,0,0,0} ,20667, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.SBB, 2, new ulong[] {REG_RAX,SBYTE64,0,0,0} ,16611, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.SBB, 2, new ulong[] {REG_RAX,IMMEDIATE,0,0,0} ,20672, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.SBB, 2, new ulong[] {RM_GPR, BITS8,IMMEDIATE,0,0,0} ,20677, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SBB, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,16617, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SBB, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,16623, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.SBB, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,16629, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.SBB, 2, new ulong[] {MEMORY,IMMEDIATE, BITS8,0,0,0} ,20677, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SBB, 2, new ulong[] {MEMORY,IMMEDIATE, BITS16,0,0,0} ,16617, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SBB, 2, new ulong[] {MEMORY,IMMEDIATE, BITS32,0,0,0} ,16623, OF.IF_386, OF.IF_SM),
    
};

        static itemplate[] instrux_SCASB = new[] {
    new itemplate(OCE.SCASB, 0, new ulong[] {0,0,0,0,0} ,21967, OF.IF_8086),
    
};

        static itemplate[] instrux_SCASD = new[] {
    new itemplate(OCE.SCASD, 0, new ulong[] {0,0,0,0,0} ,20682, OF.IF_386),
    
};

        static itemplate[] instrux_SCASQ = new[] {
    new itemplate(OCE.SCASQ, 0, new ulong[] {0,0,0,0,0} ,20687, OF.IF_X64),
    
};

        static itemplate[] instrux_SCASW = new[] {
    new itemplate(OCE.SCASW, 0, new ulong[] {0,0,0,0,0} ,20692, OF.IF_8086),
    
};

        static itemplate[] instrux_SFENCE = new[] {
    new itemplate(OCE.SFENCE, 0, new ulong[] {0,0,0,0,0} ,20697, OF.IF_X64, OF.IF_AMD),
    new itemplate(OCE.SFENCE, 0, new ulong[] {0,0,0,0,0} ,20697, OF.IF_KATMAI),
    
};

        static itemplate[] instrux_SGDT = new[] {
    new itemplate(OCE.SGDT, 1, new ulong[] {MEMORY,0,0,0,0} ,20702, OF.IF_286),
    
};

        static itemplate[] instrux_SHL = new[] {
    new itemplate(OCE.SHL, 2, new ulong[] {RM_GPR, BITS8,UNITY,0,0,0} ,21943, OF.IF_8086),
    new itemplate(OCE.SHL, 2, new ulong[] {RM_GPR, BITS8,REG_CL,0,0,0} ,21947, OF.IF_8086),
    new itemplate(OCE.SHL, 2, new ulong[] {RM_GPR, BITS8,IMMEDIATE,0,0,0} ,20562, OF.IF_186, OF.IF_SB),
    new itemplate(OCE.SHL, 2, new ulong[] {RM_GPR, BITS16,UNITY,0,0,0} ,20567, OF.IF_8086),
    new itemplate(OCE.SHL, 2, new ulong[] {RM_GPR, BITS16,REG_CL,0,0,0} ,20572, OF.IF_8086),
    new itemplate(OCE.SHL, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,16563, OF.IF_186, OF.IF_SB),
    new itemplate(OCE.SHL, 2, new ulong[] {RM_GPR, BITS32,UNITY,0,0,0} ,20577, OF.IF_386),
    new itemplate(OCE.SHL, 2, new ulong[] {RM_GPR, BITS32,REG_CL,0,0,0} ,20582, OF.IF_386),
    new itemplate(OCE.SHL, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,16569, OF.IF_386, OF.IF_SB),
    new itemplate(OCE.SHL, 2, new ulong[] {RM_GPR, BITS64,UNITY,0,0,0} ,20587, OF.IF_X64),
    new itemplate(OCE.SHL, 2, new ulong[] {RM_GPR, BITS64,REG_CL,0,0,0} ,20592, OF.IF_X64),
    new itemplate(OCE.SHL, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,16575, OF.IF_X64, OF.IF_SB),
    
};

        static itemplate[] instrux_SHLD = new[] {
    new itemplate(OCE.SHLD, 3, new ulong[] {MEMORY,REG16,IMMEDIATE,0,0} ,8979, OF.IF_386, OF.IF_SM2, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.SHLD, 3, new ulong[] {REG16,REG16,IMMEDIATE,0,0} ,8979, OF.IF_386, OF.IF_SM2, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.SHLD, 3, new ulong[] {MEMORY,REG32,IMMEDIATE,0,0} ,8986, OF.IF_386, OF.IF_SM2, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.SHLD, 3, new ulong[] {REG32,REG32,IMMEDIATE,0,0} ,8986, OF.IF_386, OF.IF_SM2, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.SHLD, 3, new ulong[] {MEMORY,REG64,IMMEDIATE,0,0} ,8993, OF.IF_X64, OF.IF_SM2, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.SHLD, 3, new ulong[] {REG64,REG64,IMMEDIATE,0,0} ,8993, OF.IF_X64, OF.IF_SM2, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.SHLD, 3, new ulong[] {MEMORY,REG16,REG_CL,0,0} ,16635, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.SHLD, 3, new ulong[] {REG16,REG16,REG_CL,0,0} ,16635, OF.IF_386),
    new itemplate(OCE.SHLD, 3, new ulong[] {MEMORY,REG32,REG_CL,0,0} ,16641, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.SHLD, 3, new ulong[] {REG32,REG32,REG_CL,0,0} ,16641, OF.IF_386),
    new itemplate(OCE.SHLD, 3, new ulong[] {MEMORY,REG64,REG_CL,0,0} ,16647, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.SHLD, 3, new ulong[] {REG64,REG64,REG_CL,0,0} ,16647, OF.IF_X64),
    
};

        static itemplate[] instrux_SHR = new[] {
    new itemplate(OCE.SHR, 2, new ulong[] {RM_GPR, BITS8,UNITY,0,0,0} ,21971, OF.IF_8086),
    new itemplate(OCE.SHR, 2, new ulong[] {RM_GPR, BITS8,REG_CL,0,0,0} ,21975, OF.IF_8086),
    new itemplate(OCE.SHR, 2, new ulong[] {RM_GPR, BITS8,IMMEDIATE,0,0,0} ,20707, OF.IF_186, OF.IF_SB),
    new itemplate(OCE.SHR, 2, new ulong[] {RM_GPR, BITS16,UNITY,0,0,0} ,20712, OF.IF_8086),
    new itemplate(OCE.SHR, 2, new ulong[] {RM_GPR, BITS16,REG_CL,0,0,0} ,20717, OF.IF_8086),
    new itemplate(OCE.SHR, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,16653, OF.IF_186, OF.IF_SB),
    new itemplate(OCE.SHR, 2, new ulong[] {RM_GPR, BITS32,UNITY,0,0,0} ,20722, OF.IF_386),
    new itemplate(OCE.SHR, 2, new ulong[] {RM_GPR, BITS32,REG_CL,0,0,0} ,20727, OF.IF_386),
    new itemplate(OCE.SHR, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,16659, OF.IF_386, OF.IF_SB),
    new itemplate(OCE.SHR, 2, new ulong[] {RM_GPR, BITS64,UNITY,0,0,0} ,20732, OF.IF_X64),
    new itemplate(OCE.SHR, 2, new ulong[] {RM_GPR, BITS64,REG_CL,0,0,0} ,20737, OF.IF_X64),
    new itemplate(OCE.SHR, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,16665, OF.IF_X64, OF.IF_SB),
    
};

        static itemplate[] instrux_SHRD = new[] {
    new itemplate(OCE.SHRD, 3, new ulong[] {MEMORY,REG16,IMMEDIATE,0,0} ,9000, OF.IF_386, OF.IF_SM2, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.SHRD, 3, new ulong[] {REG16,REG16,IMMEDIATE,0,0} ,9000, OF.IF_386, OF.IF_SM2, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.SHRD, 3, new ulong[] {MEMORY,REG32,IMMEDIATE,0,0} ,9007, OF.IF_386, OF.IF_SM2, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.SHRD, 3, new ulong[] {REG32,REG32,IMMEDIATE,0,0} ,9007, OF.IF_386, OF.IF_SM2, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.SHRD, 3, new ulong[] {MEMORY,REG64,IMMEDIATE,0,0} ,9014, OF.IF_X64, OF.IF_SM2, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.SHRD, 3, new ulong[] {REG64,REG64,IMMEDIATE,0,0} ,9014, OF.IF_X64, OF.IF_SM2, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.SHRD, 3, new ulong[] {MEMORY,REG16,REG_CL,0,0} ,16671, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.SHRD, 3, new ulong[] {REG16,REG16,REG_CL,0,0} ,16671, OF.IF_386),
    new itemplate(OCE.SHRD, 3, new ulong[] {MEMORY,REG32,REG_CL,0,0} ,16677, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.SHRD, 3, new ulong[] {REG32,REG32,REG_CL,0,0} ,16677, OF.IF_386),
    new itemplate(OCE.SHRD, 3, new ulong[] {MEMORY,REG64,REG_CL,0,0} ,16683, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.SHRD, 3, new ulong[] {REG64,REG64,REG_CL,0,0} ,16683, OF.IF_X64),
    
};

        static itemplate[] instrux_SHUFPD = new[] {
    new itemplate(OCE.SHUFPD, 3, new ulong[] {XMMREG,XMMREG,IMMEDIATE,0,0} ,9399, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.SHUFPD, 3, new ulong[] {XMMREG,MEMORY,IMMEDIATE,0,0} ,9399, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SM, OF.IF_SB, OF.IF_AR2),
    
};

        static itemplate[] instrux_SHUFPS = new[] {
    new itemplate(OCE.SHUFPS, 3, new ulong[] {XMMREG,MEMORY,IMMEDIATE,0,0} ,9140, OF.IF_KATMAI, OF.IF_SSE, OF.IF_SB, OF.IF_AR2),
    new itemplate(OCE.SHUFPS, 3, new ulong[] {XMMREG,XMMREG,IMMEDIATE,0,0} ,9140, OF.IF_KATMAI, OF.IF_SSE, OF.IF_SB, OF.IF_AR2),
    
};

        static itemplate[] instrux_SIDT = new[] {
    new itemplate(OCE.SIDT, 1, new ulong[] {MEMORY,0,0,0,0} ,20742, OF.IF_286),
    
};

        static itemplate[] instrux_SKINIT = new[] {
    new itemplate(OCE.SKINIT, 0, new ulong[] {0,0,0,0,0} ,20747, OF.IF_X64),
    
};

        static itemplate[] instrux_SLDT = new[] {
    new itemplate(OCE.SLDT, 1, new ulong[] {MEMORY,0,0,0,0} ,16708, OF.IF_286),
    new itemplate(OCE.SLDT, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,16708, OF.IF_286),
    new itemplate(OCE.SLDT, 1, new ulong[] {REG16,0,0,0,0} ,16689, OF.IF_286),
    new itemplate(OCE.SLDT, 1, new ulong[] {REG32,0,0,0,0} ,16695, OF.IF_386),
    new itemplate(OCE.SLDT, 1, new ulong[] {REG64,0,0,0,0} ,16701, OF.IF_X64),
    new itemplate(OCE.SLDT, 1, new ulong[] {REG64,0,0,0,0} ,16707, OF.IF_X64),
    
};

        static itemplate[] instrux_SMI = new[] {
    new itemplate(OCE.SMI, 0, new ulong[] {0,0,0,0,0} ,22119, OF.IF_386, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_SMINT = new[] {
    new itemplate(OCE.SMINT, 0, new ulong[] {0,0,0,0,0} ,21979, OF.IF_P6, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_SMINTOLD = new[] {
    new itemplate(OCE.SMINTOLD, 0, new ulong[] {0,0,0,0,0} ,21983, OF.IF_486, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_SMSW = new[] {
    new itemplate(OCE.SMSW, 1, new ulong[] {MEMORY,0,0,0,0} ,16720, OF.IF_286),
    new itemplate(OCE.SMSW, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,16720, OF.IF_286),
    new itemplate(OCE.SMSW, 1, new ulong[] {REG16,0,0,0,0} ,16713, OF.IF_286),
    new itemplate(OCE.SMSW, 1, new ulong[] {REG32,0,0,0,0} ,16719, OF.IF_386),
    
};

        static itemplate[] instrux_SQRTPD = new[] {
    new itemplate(OCE.SQRTPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17829, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_SQRTPS = new[] {
    new itemplate(OCE.SQRTPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17079, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_SQRTSD = new[] {
    new itemplate(OCE.SQRTSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17835, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_SQRTSS = new[] {
    new itemplate(OCE.SQRTSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17085, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_STC = new[] {
    new itemplate(OCE.STC, 0, new ulong[] {0,0,0,0,0} ,20479, OF.IF_8086),
    
};

        static itemplate[] instrux_STD = new[] {
    new itemplate(OCE.STD, 0, new ulong[] {0,0,0,0,0} ,22152, OF.IF_8086),
    
};

        static itemplate[] instrux_STGI = new[] {
    new itemplate(OCE.STGI, 0, new ulong[] {0,0,0,0,0} ,20752, OF.IF_X64),
    
};

        static itemplate[] instrux_STI = new[] {
    new itemplate(OCE.STI, 0, new ulong[] {0,0,0,0,0} ,22155, OF.IF_8086),
    
};

        static itemplate[] instrux_STMXCSR = new[] {
    new itemplate(OCE.STMXCSR, 1, new ulong[] {MEMORY,0,0,0,0} ,20977, OF.IF_KATMAI, OF.IF_SSE, OF.IF_SD),
    
};

        static itemplate[] instrux_STOSB = new[] {
    new itemplate(OCE.STOSB, 0, new ulong[] {0,0,0,0,0} ,6721, OF.IF_8086),
    
};

        static itemplate[] instrux_STOSD = new[] {
    new itemplate(OCE.STOSD, 0, new ulong[] {0,0,0,0,0} ,21987, OF.IF_386),
    
};

        static itemplate[] instrux_STOSQ = new[] {
    new itemplate(OCE.STOSQ, 0, new ulong[] {0,0,0,0,0} ,21991, OF.IF_X64),
    
};

        static itemplate[] instrux_STOSW = new[] {
    new itemplate(OCE.STOSW, 0, new ulong[] {0,0,0,0,0} ,21995, OF.IF_8086),
    
};

        static itemplate[] instrux_STR = new[] {
    new itemplate(OCE.STR, 1, new ulong[] {MEMORY,0,0,0,0} ,16738, OF.IF_286, OF.IF_PROT),
    new itemplate(OCE.STR, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,16738, OF.IF_286, OF.IF_PROT),
    new itemplate(OCE.STR, 1, new ulong[] {REG16,0,0,0,0} ,16725, OF.IF_286, OF.IF_PROT),
    new itemplate(OCE.STR, 1, new ulong[] {REG32,0,0,0,0} ,16731, OF.IF_386, OF.IF_PROT),
    new itemplate(OCE.STR, 1, new ulong[] {REG64,0,0,0,0} ,16737, OF.IF_X64),
    
};

        static itemplate[] instrux_SUB = new[] {
    new itemplate(OCE.SUB, 2, new ulong[] {MEMORY,REG8,0,0,0} ,21999, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SUB, 2, new ulong[] {REG8,REG8,0,0,0} ,21999, OF.IF_8086),
    new itemplate(OCE.SUB, 2, new ulong[] {MEMORY,REG16,0,0,0} ,20757, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SUB, 2, new ulong[] {REG16,REG16,0,0,0} ,20757, OF.IF_8086),
    new itemplate(OCE.SUB, 2, new ulong[] {MEMORY,REG32,0,0,0} ,20762, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.SUB, 2, new ulong[] {REG32,REG32,0,0,0} ,20762, OF.IF_386),
    new itemplate(OCE.SUB, 2, new ulong[] {MEMORY,REG64,0,0,0} ,20767, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.SUB, 2, new ulong[] {REG64,REG64,0,0,0} ,20767, OF.IF_X64),
    new itemplate(OCE.SUB, 2, new ulong[] {REG8,MEMORY,0,0,0} ,12307, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SUB, 2, new ulong[] {REG8,REG8,0,0,0} ,12307, OF.IF_8086),
    new itemplate(OCE.SUB, 2, new ulong[] {REG16,MEMORY,0,0,0} ,20772, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SUB, 2, new ulong[] {REG16,REG16,0,0,0} ,20772, OF.IF_8086),
    new itemplate(OCE.SUB, 2, new ulong[] {REG32,MEMORY,0,0,0} ,20777, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.SUB, 2, new ulong[] {REG32,REG32,0,0,0} ,20777, OF.IF_386),
    new itemplate(OCE.SUB, 2, new ulong[] {REG64,MEMORY,0,0,0} ,20782, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.SUB, 2, new ulong[] {REG64,REG64,0,0,0} ,20782, OF.IF_X64),
    new itemplate(OCE.SUB, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE, BITS8,0,0,0} ,16743, OF.IF_8086),
    new itemplate(OCE.SUB, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE, BITS8,0,0,0} ,16749, OF.IF_386),
    new itemplate(OCE.SUB, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE, BITS8,0,0,0} ,16755, OF.IF_X64),
    new itemplate(OCE.SUB, 2, new ulong[] {REG_AL,IMMEDIATE,0,0,0} ,22003, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SUB, 2, new ulong[] {REG_AX,SBYTE16,0,0,0} ,16743, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SUB, 2, new ulong[] {REG_AX,IMMEDIATE,0,0,0} ,20787, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SUB, 2, new ulong[] {REG_EAX,SBYTE32,0,0,0} ,16749, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.SUB, 2, new ulong[] {REG_EAX,IMMEDIATE,0,0,0} ,20792, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.SUB, 2, new ulong[] {REG_RAX,SBYTE64,0,0,0} ,16755, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.SUB, 2, new ulong[] {REG_RAX,IMMEDIATE,0,0,0} ,20797, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.SUB, 2, new ulong[] {RM_GPR, BITS8,IMMEDIATE,0,0,0} ,20802, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SUB, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,16761, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SUB, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,16767, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.SUB, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,16773, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.SUB, 2, new ulong[] {MEMORY,IMMEDIATE, BITS8,0,0,0} ,20802, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SUB, 2, new ulong[] {MEMORY,IMMEDIATE, BITS16,0,0,0} ,16761, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.SUB, 2, new ulong[] {MEMORY,IMMEDIATE, BITS32,0,0,0} ,16767, OF.IF_386, OF.IF_SM),
    
};

        static itemplate[] instrux_SUBPD = new[] {
    new itemplate(OCE.SUBPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17841, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_SUBPS = new[] {
    new itemplate(OCE.SUBPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17091, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_SUBSD = new[] {
    new itemplate(OCE.SUBSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17847, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_SUBSS = new[] {
    new itemplate(OCE.SUBSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17097, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_SVDC = new[] {
    new itemplate(OCE.SVDC, 2, new ulong[] {MEMORY, BITS80,REG_SREG,0,0,0} ,9408, OF.IF_486, OF.IF_CYRIX, OF.IF_SMM),
    
};

        static itemplate[] instrux_SVLDT = new[] {
    new itemplate(OCE.SVLDT, 1, new ulong[] {MEMORY, BITS80,0,0,0,0} ,20807, OF.IF_486, OF.IF_CYRIX, OF.IF_SMM),
    
};

        static itemplate[] instrux_SVTS = new[] {
    new itemplate(OCE.SVTS, 1, new ulong[] {MEMORY, BITS80,0,0,0,0} ,20812, OF.IF_486, OF.IF_CYRIX, OF.IF_SMM),
    
};

        static itemplate[] instrux_SWAPGS = new[] {
    new itemplate(OCE.SWAPGS, 0, new ulong[] {0,0,0,0,0} ,20817, OF.IF_X64),
    
};

        static itemplate[] instrux_SYSCALL = new[] {
    new itemplate(OCE.SYSCALL, 0, new ulong[] {0,0,0,0,0} ,21707, OF.IF_P6, OF.IF_AMD),
    
};

        static itemplate[] instrux_SYSENTER = new[] {
    new itemplate(OCE.SYSENTER, 0, new ulong[] {0,0,0,0,0} ,22007, OF.IF_P6),
    
};

        static itemplate[] instrux_SYSEXIT = new[] {
    new itemplate(OCE.SYSEXIT, 0, new ulong[] {0,0,0,0,0} ,22011, OF.IF_P6, OF.IF_PRIV),
    
};

        static itemplate[] instrux_SYSRET = new[] {
    new itemplate(OCE.SYSRET, 0, new ulong[] {0,0,0,0,0} ,21703, OF.IF_P6, OF.IF_PRIV, OF.IF_AMD),
    
};

        static itemplate[] instrux_TEST = new[] {
    new itemplate(OCE.TEST, 2, new ulong[] {MEMORY,REG8,0,0,0} ,22015, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.TEST, 2, new ulong[] {REG8,REG8,0,0,0} ,22015, OF.IF_8086),
    new itemplate(OCE.TEST, 2, new ulong[] {MEMORY,REG16,0,0,0} ,20822, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.TEST, 2, new ulong[] {REG16,REG16,0,0,0} ,20822, OF.IF_8086),
    new itemplate(OCE.TEST, 2, new ulong[] {MEMORY,REG32,0,0,0} ,20827, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.TEST, 2, new ulong[] {REG32,REG32,0,0,0} ,20827, OF.IF_386),
    new itemplate(OCE.TEST, 2, new ulong[] {MEMORY,REG64,0,0,0} ,20832, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.TEST, 2, new ulong[] {REG64,REG64,0,0,0} ,20832, OF.IF_X64),
    new itemplate(OCE.TEST, 2, new ulong[] {REG8,MEMORY,0,0,0} ,22019, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.TEST, 2, new ulong[] {REG16,MEMORY,0,0,0} ,20837, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.TEST, 2, new ulong[] {REG32,MEMORY,0,0,0} ,20842, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.TEST, 2, new ulong[] {REG64,MEMORY,0,0,0} ,20847, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.TEST, 2, new ulong[] {REG_AL,IMMEDIATE,0,0,0} ,22023, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.TEST, 2, new ulong[] {REG_AX,IMMEDIATE,0,0,0} ,20852, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.TEST, 2, new ulong[] {REG_EAX,IMMEDIATE,0,0,0} ,20857, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.TEST, 2, new ulong[] {REG_RAX,IMMEDIATE,0,0,0} ,20862, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.TEST, 2, new ulong[] {RM_GPR, BITS8,IMMEDIATE,0,0,0} ,20867, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.TEST, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,16779, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.TEST, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,16785, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.TEST, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,16791, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.TEST, 2, new ulong[] {MEMORY,IMMEDIATE, BITS8,0,0,0} ,20867, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.TEST, 2, new ulong[] {MEMORY,IMMEDIATE, BITS16,0,0,0} ,16779, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.TEST, 2, new ulong[] {MEMORY,IMMEDIATE, BITS32,0,0,0} ,16785, OF.IF_386, OF.IF_SM),
    
};

        static itemplate[] instrux_UCOMISD = new[] {
    new itemplate(OCE.UCOMISD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17853, OF.IF_WILLAMETTE, OF.IF_SSE2),
    
};

        static itemplate[] instrux_UCOMISS = new[] {
    new itemplate(OCE.UCOMISS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17103, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_UD0 = new[] {
    new itemplate(OCE.UD0, 0, new ulong[] {0,0,0,0,0} ,22027, OF.IF_186, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_UD1 = new[] {
    new itemplate(OCE.UD1, 0, new ulong[] {0,0,0,0,0} ,22031, OF.IF_186, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_UD2 = new[] {
    new itemplate(OCE.UD2, 0, new ulong[] {0,0,0,0,0} ,22035, OF.IF_186),
    
};

        static itemplate[] instrux_UD2A = new[] {
    new itemplate(OCE.UD2A, 0, new ulong[] {0,0,0,0,0} ,22035, OF.IF_186),
    
};

        static itemplate[] instrux_UD2B = new[] {
    new itemplate(OCE.UD2B, 0, new ulong[] {0,0,0,0,0} ,22031, OF.IF_186, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_UMOV = new[] {
    new itemplate(OCE.UMOV, 2, new ulong[] {MEMORY,REG8,0,0,0} ,16797, OF.IF_386, OF.IF_UNDOC, OF.IF_SM),
    new itemplate(OCE.UMOV, 2, new ulong[] {REG8,REG8,0,0,0} ,16797, OF.IF_386, OF.IF_UNDOC),
    new itemplate(OCE.UMOV, 2, new ulong[] {MEMORY,REG16,0,0,0} ,9021, OF.IF_386, OF.IF_UNDOC, OF.IF_SM),
    new itemplate(OCE.UMOV, 2, new ulong[] {REG16,REG16,0,0,0} ,9021, OF.IF_386, OF.IF_UNDOC),
    new itemplate(OCE.UMOV, 2, new ulong[] {MEMORY,REG32,0,0,0} ,9028, OF.IF_386, OF.IF_UNDOC, OF.IF_SM),
    new itemplate(OCE.UMOV, 2, new ulong[] {REG32,REG32,0,0,0} ,9028, OF.IF_386, OF.IF_UNDOC),
    new itemplate(OCE.UMOV, 2, new ulong[] {REG8,MEMORY,0,0,0} ,16803, OF.IF_386, OF.IF_UNDOC, OF.IF_SM),
    new itemplate(OCE.UMOV, 2, new ulong[] {REG8,REG8,0,0,0} ,16803, OF.IF_386, OF.IF_UNDOC),
    new itemplate(OCE.UMOV, 2, new ulong[] {REG16,MEMORY,0,0,0} ,9035, OF.IF_386, OF.IF_UNDOC, OF.IF_SM),
    new itemplate(OCE.UMOV, 2, new ulong[] {REG16,REG16,0,0,0} ,9035, OF.IF_386, OF.IF_UNDOC),
    new itemplate(OCE.UMOV, 2, new ulong[] {REG32,MEMORY,0,0,0} ,9042, OF.IF_386, OF.IF_UNDOC, OF.IF_SM),
    new itemplate(OCE.UMOV, 2, new ulong[] {REG32,REG32,0,0,0} ,9042, OF.IF_386, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_UNPCKHPD = new[] {
    new itemplate(OCE.UNPCKHPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17859, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_UNPCKHPS = new[] {
    new itemplate(OCE.UNPCKHPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17109, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_UNPCKLPD = new[] {
    new itemplate(OCE.UNPCKLPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17865, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_UNPCKLPS = new[] {
    new itemplate(OCE.UNPCKLPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17115, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_VADDPD = new[] {
    new itemplate(OCE.VADDPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,10988, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VADDPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10995, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VADDPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,11002, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VADDPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,11009, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VADDPS = new[] {
    new itemplate(OCE.VADDPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11016, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VADDPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11023, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VADDPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,11030, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VADDPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,11037, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VADDSD = new[] {
    new itemplate(OCE.VADDSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11044, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VADDSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11051, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VADDSS = new[] {
    new itemplate(OCE.VADDSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11058, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VADDSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11065, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VADDSUBPD = new[] {
    new itemplate(OCE.VADDSUBPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11072, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VADDSUBPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11079, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VADDSUBPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,11086, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VADDSUBPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,11093, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VADDSUBPS = new[] {
    new itemplate(OCE.VADDSUBPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11100, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VADDSUBPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11107, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VADDSUBPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,11114, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VADDSUBPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,11121, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VAESDEC = new[] {
    new itemplate(OCE.VAESDEC, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,10953, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VAESDEC, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10960, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VAESDECLAST = new[] {
    new itemplate(OCE.VAESDECLAST, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,10967, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VAESDECLAST, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10974, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VAESENC = new[] {
    new itemplate(OCE.VAESENC, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,10925, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VAESENC, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10932, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VAESENCLAST = new[] {
    new itemplate(OCE.VAESENCLAST, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,10939, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VAESENCLAST, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10946, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VAESIMC = new[] {
    new itemplate(OCE.VAESIMC, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,10981, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VAESKEYGENASSIST = new[] {
    new itemplate(OCE.VAESKEYGENASSIST, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7396, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VANDNPD = new[] {
    new itemplate(OCE.VANDNPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11184, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VANDNPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11191, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VANDNPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,11198, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VANDNPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,11205, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VANDNPS = new[] {
    new itemplate(OCE.VANDNPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11212, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VANDNPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11219, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VANDNPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,11226, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VANDNPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,11233, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VANDPD = new[] {
    new itemplate(OCE.VANDPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11128, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VANDPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11135, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VANDPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,11142, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VANDPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,11149, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VANDPS = new[] {
    new itemplate(OCE.VANDPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11156, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VANDPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11163, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VANDPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,11170, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VANDPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,11177, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VBLENDPD = new[] {
    new itemplate(OCE.VBLENDPD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7404, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VBLENDPD, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7412, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VBLENDPD, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,IMMEDIATE,0} ,7420, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VBLENDPD, 3, new ulong[] {YMMREG,RM_YMM,IMMEDIATE,0,0} ,7428, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VBLENDPS = new[] {
    new itemplate(OCE.VBLENDPS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7436, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VBLENDPS, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7444, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VBLENDPS, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,IMMEDIATE,0} ,7452, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VBLENDPS, 3, new ulong[] {YMMREG,RM_YMM,IMMEDIATE,0,0} ,7460, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VBLENDVPD = new[] {
    new itemplate(OCE.VBLENDVPD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,1206, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VBLENDVPD, 3, new ulong[] {XMMREG,RM_XMM,XMM0,0,0} ,11240, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VBLENDVPD, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,0} ,1215, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VBLENDVPD, 3, new ulong[] {YMMREG,RM_YMM,YMM0,0,0} ,11247, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VBLENDVPD, 3, new ulong[] {YMMREG,RM_YMM,YMM0,0,0} ,11261, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VBLENDVPS = new[] {
    new itemplate(OCE.VBLENDVPS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,1224, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VBLENDVPS, 3, new ulong[] {XMMREG,RM_XMM,XMM0,0,0} ,11254, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VBLENDVPS, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,0} ,1233, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VBROADCASTF128 = new[] {
    new itemplate(OCE.VBROADCASTF128, 2, new ulong[] {YMMREG,MEMORY,0,0,0} ,11289, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VBROADCASTSD = new[] {
    new itemplate(OCE.VBROADCASTSD, 2, new ulong[] {YMMREG,MEMORY,0,0,0} ,11282, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VBROADCASTSS = new[] {
    new itemplate(OCE.VBROADCASTSS, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,11268, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VBROADCASTSS, 2, new ulong[] {YMMREG,MEMORY,0,0,0} ,11275, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPEQPD = new[] {
    new itemplate(OCE.VCMPEQPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1242, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPEQPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,1251, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPEQPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,1260, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPEQPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,1269, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPEQPS = new[] {
    new itemplate(OCE.VCMPEQPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2394, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPEQPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2403, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPEQPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2412, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPEQPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2421, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPEQSD = new[] {
    new itemplate(OCE.VCMPEQSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3546, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPEQSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3555, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPEQSS = new[] {
    new itemplate(OCE.VCMPEQSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4122, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPEQSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4131, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPEQ_OSPD = new[] {
    new itemplate(OCE.VCMPEQ_OSPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1818, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPEQ_OSPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,1827, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPEQ_OSPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,1836, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPEQ_OSPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,1845, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPEQ_OSPS = new[] {
    new itemplate(OCE.VCMPEQ_OSPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2970, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPEQ_OSPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2979, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPEQ_OSPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2988, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPEQ_OSPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2997, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPEQ_OSSD = new[] {
    new itemplate(OCE.VCMPEQ_OSSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3834, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPEQ_OSSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3843, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPEQ_OSSS = new[] {
    new itemplate(OCE.VCMPEQ_OSSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4410, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPEQ_OSSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4419, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPEQ_UQPD = new[] {
    new itemplate(OCE.VCMPEQ_UQPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1530, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPEQ_UQPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,1539, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPEQ_UQPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,1548, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPEQ_UQPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,1557, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPEQ_UQPS = new[] {
    new itemplate(OCE.VCMPEQ_UQPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2682, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPEQ_UQPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2691, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPEQ_UQPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2700, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPEQ_UQPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2709, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPEQ_UQSD = new[] {
    new itemplate(OCE.VCMPEQ_UQSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3690, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPEQ_UQSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3699, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPEQ_UQSS = new[] {
    new itemplate(OCE.VCMPEQ_UQSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4266, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPEQ_UQSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4275, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPEQ_USPD = new[] {
    new itemplate(OCE.VCMPEQ_USPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2106, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPEQ_USPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2115, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPEQ_USPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2124, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPEQ_USPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2133, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPEQ_USPS = new[] {
    new itemplate(OCE.VCMPEQ_USPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3258, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPEQ_USPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3267, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPEQ_USPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,3276, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPEQ_USPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,3285, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPEQ_USSD = new[] {
    new itemplate(OCE.VCMPEQ_USSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3978, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPEQ_USSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3987, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPEQ_USSS = new[] {
    new itemplate(OCE.VCMPEQ_USSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4554, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPEQ_USSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4563, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPFALSEPD = new[] {
    new itemplate(OCE.VCMPFALSEPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1638, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPFALSEPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,1647, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPFALSEPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,1656, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPFALSEPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,1665, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPFALSEPS = new[] {
    new itemplate(OCE.VCMPFALSEPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2790, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPFALSEPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2799, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPFALSEPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2808, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPFALSEPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2817, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPFALSESD = new[] {
    new itemplate(OCE.VCMPFALSESD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3744, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPFALSESD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3753, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPFALSESS = new[] {
    new itemplate(OCE.VCMPFALSESS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4320, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPFALSESS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4329, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPFALSE_OSPD = new[] {
    new itemplate(OCE.VCMPFALSE_OSPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2214, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPFALSE_OSPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2223, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPFALSE_OSPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2232, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPFALSE_OSPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2241, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPFALSE_OSPS = new[] {
    new itemplate(OCE.VCMPFALSE_OSPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3366, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPFALSE_OSPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3375, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPFALSE_OSPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,3384, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPFALSE_OSPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,3393, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPFALSE_OSSD = new[] {
    new itemplate(OCE.VCMPFALSE_OSSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4032, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPFALSE_OSSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4041, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPFALSE_OSSS = new[] {
    new itemplate(OCE.VCMPFALSE_OSSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4608, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPFALSE_OSSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4617, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPGEPD = new[] {
    new itemplate(OCE.VCMPGEPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1710, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPGEPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,1719, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPGEPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,1728, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPGEPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,1737, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPGEPS = new[] {
    new itemplate(OCE.VCMPGEPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2862, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPGEPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2871, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPGEPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2880, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPGEPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2889, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPGESD = new[] {
    new itemplate(OCE.VCMPGESD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3780, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPGESD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3789, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPGESS = new[] {
    new itemplate(OCE.VCMPGESS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4356, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPGESS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4365, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPGE_OQPD = new[] {
    new itemplate(OCE.VCMPGE_OQPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2286, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPGE_OQPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2295, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPGE_OQPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2304, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPGE_OQPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2313, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPGE_OQPS = new[] {
    new itemplate(OCE.VCMPGE_OQPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3438, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPGE_OQPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3447, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPGE_OQPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,3456, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPGE_OQPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,3465, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPGE_OQSD = new[] {
    new itemplate(OCE.VCMPGE_OQSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4068, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPGE_OQSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4077, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPGE_OQSS = new[] {
    new itemplate(OCE.VCMPGE_OQSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4644, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPGE_OQSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4653, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPGTPD = new[] {
    new itemplate(OCE.VCMPGTPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1746, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPGTPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,1755, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPGTPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,1764, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPGTPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,1773, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPGTPS = new[] {
    new itemplate(OCE.VCMPGTPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2898, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPGTPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2907, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPGTPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2916, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPGTPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2925, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPGTSD = new[] {
    new itemplate(OCE.VCMPGTSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3798, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPGTSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3807, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPGTSS = new[] {
    new itemplate(OCE.VCMPGTSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4374, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPGTSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4383, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPGT_OQPD = new[] {
    new itemplate(OCE.VCMPGT_OQPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2322, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPGT_OQPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2331, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPGT_OQPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2340, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPGT_OQPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2349, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPGT_OQPS = new[] {
    new itemplate(OCE.VCMPGT_OQPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3474, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPGT_OQPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3483, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPGT_OQPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,3492, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPGT_OQPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,3501, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPGT_OQSD = new[] {
    new itemplate(OCE.VCMPGT_OQSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4086, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPGT_OQSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4095, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPGT_OQSS = new[] {
    new itemplate(OCE.VCMPGT_OQSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4662, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPGT_OQSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4671, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPLEPD = new[] {
    new itemplate(OCE.VCMPLEPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1314, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPLEPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,1323, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPLEPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,1332, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPLEPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,1341, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPLEPS = new[] {
    new itemplate(OCE.VCMPLEPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2466, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPLEPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2475, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPLEPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2484, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPLEPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2493, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPLESD = new[] {
    new itemplate(OCE.VCMPLESD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3582, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPLESD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3591, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPLESS = new[] {
    new itemplate(OCE.VCMPLESS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4158, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPLESS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4167, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPLE_OQPD = new[] {
    new itemplate(OCE.VCMPLE_OQPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1890, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPLE_OQPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,1899, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPLE_OQPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,1908, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPLE_OQPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,1917, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPLE_OQPS = new[] {
    new itemplate(OCE.VCMPLE_OQPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3042, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPLE_OQPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3051, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPLE_OQPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,3060, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPLE_OQPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,3069, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPLE_OQSD = new[] {
    new itemplate(OCE.VCMPLE_OQSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3870, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPLE_OQSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3879, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPLE_OQSS = new[] {
    new itemplate(OCE.VCMPLE_OQSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4446, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPLE_OQSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4455, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPLTPD = new[] {
    new itemplate(OCE.VCMPLTPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1278, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPLTPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,1287, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPLTPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,1296, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPLTPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,1305, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPLTPS = new[] {
    new itemplate(OCE.VCMPLTPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2430, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPLTPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2439, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPLTPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2448, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPLTPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2457, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPLTSD = new[] {
    new itemplate(OCE.VCMPLTSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3564, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPLTSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3573, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPLTSS = new[] {
    new itemplate(OCE.VCMPLTSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4140, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPLTSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4149, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPLT_OQPD = new[] {
    new itemplate(OCE.VCMPLT_OQPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1854, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPLT_OQPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,1863, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPLT_OQPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,1872, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPLT_OQPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,1881, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPLT_OQPS = new[] {
    new itemplate(OCE.VCMPLT_OQPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3006, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPLT_OQPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3015, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPLT_OQPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,3024, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPLT_OQPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,3033, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPLT_OQSD = new[] {
    new itemplate(OCE.VCMPLT_OQSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3852, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPLT_OQSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3861, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPLT_OQSS = new[] {
    new itemplate(OCE.VCMPLT_OQSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4428, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPLT_OQSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4437, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPNEQPD = new[] {
    new itemplate(OCE.VCMPNEQPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1386, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNEQPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,1395, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNEQPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,1404, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNEQPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,1413, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNEQPS = new[] {
    new itemplate(OCE.VCMPNEQPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2538, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNEQPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2547, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNEQPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2556, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNEQPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2565, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNEQSD = new[] {
    new itemplate(OCE.VCMPNEQSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3618, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPNEQSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3627, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPNEQSS = new[] {
    new itemplate(OCE.VCMPNEQSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4194, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPNEQSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4203, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPNEQ_OQPD = new[] {
    new itemplate(OCE.VCMPNEQ_OQPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1674, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNEQ_OQPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,1683, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNEQ_OQPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,1692, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNEQ_OQPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,1701, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNEQ_OQPS = new[] {
    new itemplate(OCE.VCMPNEQ_OQPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2826, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNEQ_OQPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2835, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNEQ_OQPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2844, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNEQ_OQPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2853, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNEQ_OQSD = new[] {
    new itemplate(OCE.VCMPNEQ_OQSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3762, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPNEQ_OQSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3771, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPNEQ_OQSS = new[] {
    new itemplate(OCE.VCMPNEQ_OQSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4338, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPNEQ_OQSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4347, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPNEQ_OSPD = new[] {
    new itemplate(OCE.VCMPNEQ_OSPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2250, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNEQ_OSPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2259, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNEQ_OSPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2268, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNEQ_OSPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2277, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNEQ_OSPS = new[] {
    new itemplate(OCE.VCMPNEQ_OSPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3402, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNEQ_OSPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3411, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNEQ_OSPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,3420, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNEQ_OSPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,3429, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNEQ_OSSD = new[] {
    new itemplate(OCE.VCMPNEQ_OSSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4050, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPNEQ_OSSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4059, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPNEQ_OSSS = new[] {
    new itemplate(OCE.VCMPNEQ_OSSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4626, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPNEQ_OSSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4635, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPNEQ_USPD = new[] {
    new itemplate(OCE.VCMPNEQ_USPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1962, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNEQ_USPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,1971, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNEQ_USPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,1980, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNEQ_USPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,1989, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNEQ_USPS = new[] {
    new itemplate(OCE.VCMPNEQ_USPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3114, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNEQ_USPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3123, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNEQ_USPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,3132, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNEQ_USPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,3141, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNEQ_USSD = new[] {
    new itemplate(OCE.VCMPNEQ_USSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3906, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPNEQ_USSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3915, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPNEQ_USSS = new[] {
    new itemplate(OCE.VCMPNEQ_USSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4482, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPNEQ_USSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4491, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPNGEPD = new[] {
    new itemplate(OCE.VCMPNGEPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1566, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNGEPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,1575, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNGEPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,1584, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNGEPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,1593, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNGEPS = new[] {
    new itemplate(OCE.VCMPNGEPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2718, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNGEPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2727, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNGEPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2736, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNGEPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2745, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNGESD = new[] {
    new itemplate(OCE.VCMPNGESD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3708, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPNGESD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3717, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPNGESS = new[] {
    new itemplate(OCE.VCMPNGESS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4284, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPNGESS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4293, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPNGE_UQPD = new[] {
    new itemplate(OCE.VCMPNGE_UQPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2142, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNGE_UQPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2151, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNGE_UQPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2160, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNGE_UQPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2169, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNGE_UQPS = new[] {
    new itemplate(OCE.VCMPNGE_UQPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3294, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNGE_UQPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3303, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNGE_UQPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,3312, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNGE_UQPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,3321, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNGE_UQSD = new[] {
    new itemplate(OCE.VCMPNGE_UQSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3996, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPNGE_UQSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4005, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPNGE_UQSS = new[] {
    new itemplate(OCE.VCMPNGE_UQSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4572, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPNGE_UQSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4581, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPNGTPD = new[] {
    new itemplate(OCE.VCMPNGTPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1602, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNGTPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,1611, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNGTPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,1620, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNGTPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,1629, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNGTPS = new[] {
    new itemplate(OCE.VCMPNGTPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2754, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNGTPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2763, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNGTPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2772, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNGTPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2781, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNGTSD = new[] {
    new itemplate(OCE.VCMPNGTSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3726, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPNGTSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3735, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPNGTSS = new[] {
    new itemplate(OCE.VCMPNGTSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4302, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPNGTSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4311, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPNGT_UQPD = new[] {
    new itemplate(OCE.VCMPNGT_UQPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2178, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNGT_UQPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2187, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNGT_UQPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2196, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNGT_UQPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2205, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNGT_UQPS = new[] {
    new itemplate(OCE.VCMPNGT_UQPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3330, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNGT_UQPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3339, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNGT_UQPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,3348, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNGT_UQPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,3357, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNGT_UQSD = new[] {
    new itemplate(OCE.VCMPNGT_UQSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4014, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPNGT_UQSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4023, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPNGT_UQSS = new[] {
    new itemplate(OCE.VCMPNGT_UQSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4590, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPNGT_UQSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4599, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPNLEPD = new[] {
    new itemplate(OCE.VCMPNLEPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1458, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNLEPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,1467, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNLEPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,1476, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNLEPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,1485, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNLEPS = new[] {
    new itemplate(OCE.VCMPNLEPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2610, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNLEPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2619, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNLEPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2628, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNLEPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2637, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNLESD = new[] {
    new itemplate(OCE.VCMPNLESD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3654, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPNLESD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3663, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPNLESS = new[] {
    new itemplate(OCE.VCMPNLESS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4230, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPNLESS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4239, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPNLE_UQPD = new[] {
    new itemplate(OCE.VCMPNLE_UQPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2034, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNLE_UQPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2043, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNLE_UQPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2052, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNLE_UQPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2061, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNLE_UQPS = new[] {
    new itemplate(OCE.VCMPNLE_UQPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3186, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNLE_UQPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3195, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNLE_UQPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,3204, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNLE_UQPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,3213, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNLE_UQSD = new[] {
    new itemplate(OCE.VCMPNLE_UQSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3942, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPNLE_UQSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3951, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPNLE_UQSS = new[] {
    new itemplate(OCE.VCMPNLE_UQSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4518, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPNLE_UQSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4527, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPNLTPD = new[] {
    new itemplate(OCE.VCMPNLTPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1422, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNLTPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,1431, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNLTPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,1440, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNLTPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,1449, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNLTPS = new[] {
    new itemplate(OCE.VCMPNLTPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2574, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNLTPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2583, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNLTPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2592, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNLTPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2601, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNLTSD = new[] {
    new itemplate(OCE.VCMPNLTSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3636, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPNLTSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3645, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPNLTSS = new[] {
    new itemplate(OCE.VCMPNLTSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4212, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPNLTSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4221, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPNLT_UQPD = new[] {
    new itemplate(OCE.VCMPNLT_UQPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1998, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNLT_UQPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2007, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNLT_UQPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2016, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNLT_UQPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2025, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNLT_UQPS = new[] {
    new itemplate(OCE.VCMPNLT_UQPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3150, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNLT_UQPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3159, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPNLT_UQPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,3168, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPNLT_UQPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,3177, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPNLT_UQSD = new[] {
    new itemplate(OCE.VCMPNLT_UQSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3924, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPNLT_UQSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3933, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPNLT_UQSS = new[] {
    new itemplate(OCE.VCMPNLT_UQSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4500, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPNLT_UQSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4509, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPORDPD = new[] {
    new itemplate(OCE.VCMPORDPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1494, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPORDPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,1503, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPORDPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,1512, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPORDPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,1521, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPORDPS = new[] {
    new itemplate(OCE.VCMPORDPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2646, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPORDPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2655, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPORDPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2664, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPORDPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2673, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPORDSD = new[] {
    new itemplate(OCE.VCMPORDSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3672, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPORDSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3681, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPORDSS = new[] {
    new itemplate(OCE.VCMPORDSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4248, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPORDSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4257, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPORD_SPD = new[] {
    new itemplate(OCE.VCMPORD_SPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2070, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPORD_SPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2079, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPORD_SPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2088, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPORD_SPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2097, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPORD_SPS = new[] {
    new itemplate(OCE.VCMPORD_SPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3222, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPORD_SPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3231, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPORD_SPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,3240, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPORD_SPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,3249, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPORD_SSD = new[] {
    new itemplate(OCE.VCMPORD_SSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3960, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPORD_SSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3969, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPORD_SSS = new[] {
    new itemplate(OCE.VCMPORD_SSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4536, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPORD_SSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4545, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPPD = new[] {
    new itemplate(OCE.VCMPPD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7468, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPPD, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7476, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPPD, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,IMMEDIATE,0} ,7484, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPPD, 3, new ulong[] {YMMREG,RM_YMM,IMMEDIATE,0,0} ,7492, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPPS = new[] {
    new itemplate(OCE.VCMPPS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7500, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPPS, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7508, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPPS, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,IMMEDIATE,0} ,7516, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPPS, 3, new ulong[] {YMMREG,RM_YMM,IMMEDIATE,0,0} ,7524, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPSD = new[] {
    new itemplate(OCE.VCMPSD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7532, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPSD, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7540, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPSS = new[] {
    new itemplate(OCE.VCMPSS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7548, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPSS, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7556, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPTRUEPD = new[] {
    new itemplate(OCE.VCMPTRUEPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1782, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPTRUEPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,1791, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPTRUEPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,1800, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPTRUEPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,1809, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPTRUEPS = new[] {
    new itemplate(OCE.VCMPTRUEPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2934, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPTRUEPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2943, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPTRUEPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2952, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPTRUEPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2961, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPTRUESD = new[] {
    new itemplate(OCE.VCMPTRUESD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3816, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPTRUESD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3825, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPTRUESS = new[] {
    new itemplate(OCE.VCMPTRUESS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4392, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPTRUESS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4401, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPTRUE_USPD = new[] {
    new itemplate(OCE.VCMPTRUE_USPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2358, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPTRUE_USPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2367, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPTRUE_USPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2376, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPTRUE_USPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2385, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPTRUE_USPS = new[] {
    new itemplate(OCE.VCMPTRUE_USPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3510, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPTRUE_USPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3519, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPTRUE_USPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,3528, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPTRUE_USPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,3537, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPTRUE_USSD = new[] {
    new itemplate(OCE.VCMPTRUE_USSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4104, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPTRUE_USSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4113, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPTRUE_USSS = new[] {
    new itemplate(OCE.VCMPTRUE_USSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4680, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPTRUE_USSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4689, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPUNORDPD = new[] {
    new itemplate(OCE.VCMPUNORDPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1350, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPUNORDPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,1359, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPUNORDPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,1368, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPUNORDPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,1377, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPUNORDPS = new[] {
    new itemplate(OCE.VCMPUNORDPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,2502, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPUNORDPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,2511, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPUNORDPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,2520, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPUNORDPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,2529, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPUNORDSD = new[] {
    new itemplate(OCE.VCMPUNORDSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3600, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPUNORDSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3609, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPUNORDSS = new[] {
    new itemplate(OCE.VCMPUNORDSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4176, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPUNORDSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4185, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCMPUNORD_SPD = new[] {
    new itemplate(OCE.VCMPUNORD_SPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,1926, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPUNORD_SPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,1935, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPUNORD_SPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,1944, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPUNORD_SPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,1953, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPUNORD_SPS = new[] {
    new itemplate(OCE.VCMPUNORD_SPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3078, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPUNORD_SPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3087, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCMPUNORD_SPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,3096, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VCMPUNORD_SPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,3105, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCMPUNORD_SSD = new[] {
    new itemplate(OCE.VCMPUNORD_SSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,3888, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCMPUNORD_SSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,3897, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCMPUNORD_SSS = new[] {
    new itemplate(OCE.VCMPUNORD_SSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,4464, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCMPUNORD_SSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,4473, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCOMISD = new[] {
    new itemplate(OCE.VCOMISD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11296, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCOMISS = new[] {
    new itemplate(OCE.VCOMISS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11303, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCVTDQ2PD = new[] {
    new itemplate(OCE.VCVTDQ2PD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11310, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCVTDQ2PD, 2, new ulong[] {YMMREG,RM_XMM,0,0,0} ,11317, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VCVTDQ2PS = new[] {
    new itemplate(OCE.VCVTDQ2PS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11324, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCVTDQ2PS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,11331, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCVTPD2DQ = new[] {
    new itemplate(OCE.VCVTPD2DQ, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,11338, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VCVTPD2DQ, 2, new ulong[] {XMMREG,MEMORY, BITS128,0,0,0} ,11338, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VCVTPD2DQ, 2, new ulong[] {XMMREG,YMMREG,0,0,0} ,11345, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VCVTPD2DQ, 2, new ulong[] {XMMREG,MEMORY, BITS256,0,0,0} ,11345, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    
};

        static itemplate[] instrux_VCVTPD2PS = new[] {
    new itemplate(OCE.VCVTPD2PS, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,11352, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VCVTPD2PS, 2, new ulong[] {XMMREG,MEMORY, BITS128,0,0,0} ,11352, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VCVTPD2PS, 2, new ulong[] {XMMREG,YMMREG,0,0,0} ,11359, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VCVTPD2PS, 2, new ulong[] {XMMREG,MEMORY, BITS256,0,0,0} ,11359, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    
};

        static itemplate[] instrux_VCVTPH2PS = new[] {
    new itemplate(OCE.VCVTPH2PS, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,8212, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    new itemplate(OCE.VCVTPH2PS, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,8220, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    new itemplate(OCE.VCVTPH2PS, 3, new ulong[] {YMMREG,RM_XMM,IMMEDIATE,0,0} ,8228, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VCVTPH2PS, 3, new ulong[] {YMMREG,RM_YMM,IMMEDIATE,0,0} ,8228, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VCVTPH2PS, 2, new ulong[] {YMMREG,IMMEDIATE,0,0,0} ,8236, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VCVTPS2DQ = new[] {
    new itemplate(OCE.VCVTPS2DQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11366, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCVTPS2DQ, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,11373, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCVTPS2PD = new[] {
    new itemplate(OCE.VCVTPS2PD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11380, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCVTPS2PD, 2, new ulong[] {YMMREG,RM_XMM,0,0,0} ,11387, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VCVTPS2PH = new[] {
    new itemplate(OCE.VCVTPS2PH, 3, new ulong[] {RM_XMM,XMMREG,IMMEDIATE,0,0} ,8244, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    new itemplate(OCE.VCVTPS2PH, 2, new ulong[] {RM_XMM,IMMEDIATE,0,0,0} ,8252, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    new itemplate(OCE.VCVTPS2PH, 3, new ulong[] {RM_XMM,YMMREG,IMMEDIATE,0,0} ,8260, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VCVTPS2PH, 3, new ulong[] {RM_YMM,YMMREG,IMMEDIATE,0,0} ,8260, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VCVTPS2PH, 2, new ulong[] {RM_YMM,IMMEDIATE,0,0,0} ,8268, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VCVTSD2SI = new[] {
    new itemplate(OCE.VCVTSD2SI, 2, new ulong[] {REG32,RM_XMM,0,0,0} ,11394, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCVTSD2SI, 2, new ulong[] {REG64,RM_XMM,0,0,0} ,11401, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ, OF.IF_LONG),
    
};

        static itemplate[] instrux_VCVTSD2SS = new[] {
    new itemplate(OCE.VCVTSD2SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11408, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCVTSD2SS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11415, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VCVTSI2SD = new[] {
    new itemplate(OCE.VCVTSI2SD, 3, new ulong[] {XMMREG,XMMREG,RM_GPR, BITS32,0,0} ,11422, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VCVTSI2SD, 2, new ulong[] {XMMREG,RM_GPR, BITS32,0,0,0} ,11429, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VCVTSI2SD, 3, new ulong[] {XMMREG,XMMREG,MEMORY,0,0} ,11422, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD, OF.IF_AR2),
    new itemplate(OCE.VCVTSI2SD, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,11429, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD, OF.IF_AR2),
    new itemplate(OCE.VCVTSI2SD, 3, new ulong[] {XMMREG,XMMREG,RM_GPR, BITS64,0,0} ,11436, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_LONG),
    new itemplate(OCE.VCVTSI2SD, 2, new ulong[] {XMMREG,RM_GPR, BITS64,0,0,0} ,11443, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_LONG),
    
};

        static itemplate[] instrux_VCVTSI2SS = new[] {
    new itemplate(OCE.VCVTSI2SS, 3, new ulong[] {XMMREG,XMMREG,RM_GPR, BITS32,0,0} ,11450, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VCVTSI2SS, 2, new ulong[] {XMMREG,RM_GPR, BITS32,0,0,0} ,11457, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VCVTSI2SS, 3, new ulong[] {XMMREG,XMMREG,MEMORY,0,0} ,11450, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD, OF.IF_AR2),
    new itemplate(OCE.VCVTSI2SS, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,11457, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD, OF.IF_AR2),
    new itemplate(OCE.VCVTSI2SS, 3, new ulong[] {XMMREG,XMMREG,RM_GPR, BITS64,0,0} ,11464, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_LONG),
    new itemplate(OCE.VCVTSI2SS, 2, new ulong[] {XMMREG,RM_GPR, BITS64,0,0,0} ,11471, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_LONG),
    
};

        static itemplate[] instrux_VCVTSS2SD = new[] {
    new itemplate(OCE.VCVTSS2SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11478, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCVTSS2SD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11485, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VCVTSS2SI = new[] {
    new itemplate(OCE.VCVTSS2SI, 2, new ulong[] {REG32,RM_XMM,0,0,0} ,11492, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCVTSS2SI, 2, new ulong[] {REG64,RM_XMM,0,0,0} ,11499, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD, OF.IF_LONG),
    
};

        static itemplate[] instrux_VCVTTPD2DQ = new[] {
    new itemplate(OCE.VCVTTPD2DQ, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,11506, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VCVTTPD2DQ, 2, new ulong[] {XMMREG,MEMORY, BITS128,0,0,0} ,11506, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VCVTTPD2DQ, 2, new ulong[] {XMMREG,YMMREG,0,0,0} ,11513, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VCVTTPD2DQ, 2, new ulong[] {XMMREG,MEMORY, BITS256,0,0,0} ,11513, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    
};

        static itemplate[] instrux_VCVTTPS2DQ = new[] {
    new itemplate(OCE.VCVTTPS2DQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11520, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VCVTTPS2DQ, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,11527, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VCVTTSD2SI = new[] {
    new itemplate(OCE.VCVTTSD2SI, 2, new ulong[] {REG32,RM_XMM,0,0,0} ,11534, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VCVTTSD2SI, 2, new ulong[] {REG64,RM_XMM,0,0,0} ,11541, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ, OF.IF_LONG),
    
};

        static itemplate[] instrux_VCVTTSS2SI = new[] {
    new itemplate(OCE.VCVTTSS2SI, 2, new ulong[] {REG32,RM_XMM,0,0,0} ,11548, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VCVTTSS2SI, 2, new ulong[] {REG64,RM_XMM,0,0,0} ,11555, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD, OF.IF_LONG),
    
};

        static itemplate[] instrux_VDIVPD = new[] {
    new itemplate(OCE.VDIVPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11562, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VDIVPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11569, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VDIVPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,11576, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VDIVPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,11583, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VDIVPS = new[] {
    new itemplate(OCE.VDIVPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11590, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VDIVPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11597, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VDIVPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,11604, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VDIVPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,11611, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VDIVSD = new[] {
    new itemplate(OCE.VDIVSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11618, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VDIVSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11625, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VDIVSS = new[] {
    new itemplate(OCE.VDIVSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11632, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VDIVSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11639, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VDPPD = new[] {
    new itemplate(OCE.VDPPD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7564, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VDPPD, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7572, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VDPPS = new[] {
    new itemplate(OCE.VDPPS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7580, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VDPPS, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7588, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VDPPS, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,IMMEDIATE,0} ,7596, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VDPPS, 3, new ulong[] {YMMREG,RM_YMM,IMMEDIATE,0,0} ,7604, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VERR = new[] {
    new itemplate(OCE.VERR, 1, new ulong[] {MEMORY,0,0,0,0} ,20872, OF.IF_286, OF.IF_PROT),
    new itemplate(OCE.VERR, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,20872, OF.IF_286, OF.IF_PROT),
    new itemplate(OCE.VERR, 1, new ulong[] {REG16,0,0,0,0} ,20872, OF.IF_286, OF.IF_PROT),
    
};

        static itemplate[] instrux_VERW = new[] {
    new itemplate(OCE.VERW, 1, new ulong[] {MEMORY,0,0,0,0} ,20877, OF.IF_286, OF.IF_PROT),
    new itemplate(OCE.VERW, 1, new ulong[] {MEMORY, BITS16,0,0,0,0} ,20877, OF.IF_286, OF.IF_PROT),
    new itemplate(OCE.VERW, 1, new ulong[] {REG16,0,0,0,0} ,20877, OF.IF_286, OF.IF_PROT),
    
};

        static itemplate[] instrux_VEXTRACTF128 = new[] {
    new itemplate(OCE.VEXTRACTF128, 3, new ulong[] {RM_XMM,XMMREG,IMMEDIATE,0,0} ,7612, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VEXTRACTPS = new[] {
    new itemplate(OCE.VEXTRACTPS, 3, new ulong[] {RM_GPR, BITS32,XMMREG,IMMEDIATE,0,0} ,7620, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFMADD123PD = new[] {
    new itemplate(OCE.VFMADD123PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14411, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADD123PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14418, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADD123PS = new[] {
    new itemplate(OCE.VFMADD123PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14397, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADD123PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14404, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADD123SD = new[] {
    new itemplate(OCE.VFMADD123SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14894, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFMADD123SS = new[] {
    new itemplate(OCE.VFMADD123SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14887, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFMADD132PD = new[] {
    new itemplate(OCE.VFMADD132PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14383, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADD132PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14390, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADD132PS = new[] {
    new itemplate(OCE.VFMADD132PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14369, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADD132PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14376, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADD132SD = new[] {
    new itemplate(OCE.VFMADD132SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14880, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFMADD132SS = new[] {
    new itemplate(OCE.VFMADD132SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14873, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFMADD213PD = new[] {
    new itemplate(OCE.VFMADD213PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14411, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADD213PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14418, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADD213PS = new[] {
    new itemplate(OCE.VFMADD213PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14397, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADD213PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14404, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADD213SD = new[] {
    new itemplate(OCE.VFMADD213SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14894, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFMADD213SS = new[] {
    new itemplate(OCE.VFMADD213SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14887, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFMADD231PD = new[] {
    new itemplate(OCE.VFMADD231PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14439, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADD231PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14446, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADD231PS = new[] {
    new itemplate(OCE.VFMADD231PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14425, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADD231PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14432, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADD231SD = new[] {
    new itemplate(OCE.VFMADD231SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14908, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFMADD231SS = new[] {
    new itemplate(OCE.VFMADD231SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14901, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFMADD312PD = new[] {
    new itemplate(OCE.VFMADD312PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14383, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADD312PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14390, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADD312PS = new[] {
    new itemplate(OCE.VFMADD312PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14369, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADD312PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14376, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADD312SD = new[] {
    new itemplate(OCE.VFMADD312SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14880, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFMADD312SS = new[] {
    new itemplate(OCE.VFMADD312SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14873, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFMADD321PD = new[] {
    new itemplate(OCE.VFMADD321PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14439, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADD321PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14446, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADD321PS = new[] {
    new itemplate(OCE.VFMADD321PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14425, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADD321PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14432, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADD321SD = new[] {
    new itemplate(OCE.VFMADD321SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14908, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFMADD321SS = new[] {
    new itemplate(OCE.VFMADD321SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14901, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFMADDPD = new[] {
    new itemplate(OCE.VFMADDPD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,5112, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMADDPD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,5121, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMADDPD, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,0} ,5130, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMADDPD, 3, new ulong[] {YMMREG,RM_YMM,YMMREG,0,0} ,5139, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMADDPD, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,5148, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMADDPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,5157, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMADDPD, 4, new ulong[] {YMMREG,YMMREG,YMMREG,RM_YMM,0} ,5166, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMADDPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,5175, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADDPS = new[] {
    new itemplate(OCE.VFMADDPS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,5184, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMADDPS, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,5193, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMADDPS, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,0} ,5202, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMADDPS, 3, new ulong[] {YMMREG,RM_YMM,YMMREG,0,0} ,5211, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMADDPS, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,5220, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMADDPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,5229, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMADDPS, 4, new ulong[] {YMMREG,YMMREG,YMMREG,RM_YMM,0} ,5238, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMADDPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,5247, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADDSD = new[] {
    new itemplate(OCE.VFMADDSD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,5256, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    new itemplate(OCE.VFMADDSD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,5265, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    new itemplate(OCE.VFMADDSD, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,5274, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    new itemplate(OCE.VFMADDSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,5283, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFMADDSS = new[] {
    new itemplate(OCE.VFMADDSS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,5292, OF.IF_AMD, OF.IF_SSE5, OF.IF_SD),
    new itemplate(OCE.VFMADDSS, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,5301, OF.IF_AMD, OF.IF_SSE5, OF.IF_SD),
    new itemplate(OCE.VFMADDSS, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,5310, OF.IF_AMD, OF.IF_SSE5, OF.IF_SD),
    new itemplate(OCE.VFMADDSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,5319, OF.IF_AMD, OF.IF_SSE5, OF.IF_SD),
    
};

        static itemplate[] instrux_VFMADDSUB123PD = new[] {
    new itemplate(OCE.VFMADDSUB123PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14495, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADDSUB123PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14502, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADDSUB123PS = new[] {
    new itemplate(OCE.VFMADDSUB123PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14481, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADDSUB123PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14488, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADDSUB132PD = new[] {
    new itemplate(OCE.VFMADDSUB132PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14467, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADDSUB132PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14474, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADDSUB132PS = new[] {
    new itemplate(OCE.VFMADDSUB132PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14453, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADDSUB132PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14460, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADDSUB213PD = new[] {
    new itemplate(OCE.VFMADDSUB213PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14495, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADDSUB213PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14502, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADDSUB213PS = new[] {
    new itemplate(OCE.VFMADDSUB213PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14481, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADDSUB213PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14488, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADDSUB231PD = new[] {
    new itemplate(OCE.VFMADDSUB231PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14523, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADDSUB231PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14530, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADDSUB231PS = new[] {
    new itemplate(OCE.VFMADDSUB231PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14509, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADDSUB231PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14516, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADDSUB312PD = new[] {
    new itemplate(OCE.VFMADDSUB312PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14467, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADDSUB312PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14474, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADDSUB312PS = new[] {
    new itemplate(OCE.VFMADDSUB312PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14453, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADDSUB312PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14460, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADDSUB321PD = new[] {
    new itemplate(OCE.VFMADDSUB321PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14523, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADDSUB321PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14530, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADDSUB321PS = new[] {
    new itemplate(OCE.VFMADDSUB321PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14509, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMADDSUB321PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14516, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADDSUBPD = new[] {
    new itemplate(OCE.VFMADDSUBPD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,5328, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMADDSUBPD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,5337, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMADDSUBPD, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,0} ,5346, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMADDSUBPD, 3, new ulong[] {YMMREG,RM_YMM,YMMREG,0,0} ,5355, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMADDSUBPD, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,5364, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMADDSUBPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,5373, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMADDSUBPD, 4, new ulong[] {YMMREG,YMMREG,YMMREG,RM_YMM,0} ,5382, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMADDSUBPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,5391, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMADDSUBPS = new[] {
    new itemplate(OCE.VFMADDSUBPS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,5400, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMADDSUBPS, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,5409, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMADDSUBPS, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,0} ,5418, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMADDSUBPS, 3, new ulong[] {YMMREG,RM_YMM,YMMREG,0,0} ,5427, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMADDSUBPS, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,5436, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMADDSUBPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,5445, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMADDSUBPS, 4, new ulong[] {YMMREG,YMMREG,YMMREG,RM_YMM,0} ,5454, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMADDSUBPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,5463, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUB123PD = new[] {
    new itemplate(OCE.VFMSUB123PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14579, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUB123PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14586, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUB123PS = new[] {
    new itemplate(OCE.VFMSUB123PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14565, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUB123PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14572, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUB123SD = new[] {
    new itemplate(OCE.VFMSUB123SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14936, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFMSUB123SS = new[] {
    new itemplate(OCE.VFMSUB123SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14929, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFMSUB132PD = new[] {
    new itemplate(OCE.VFMSUB132PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14551, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUB132PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14558, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUB132PS = new[] {
    new itemplate(OCE.VFMSUB132PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14537, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUB132PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14544, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUB132SD = new[] {
    new itemplate(OCE.VFMSUB132SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14922, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFMSUB132SS = new[] {
    new itemplate(OCE.VFMSUB132SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14915, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFMSUB213PD = new[] {
    new itemplate(OCE.VFMSUB213PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14579, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUB213PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14586, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUB213PS = new[] {
    new itemplate(OCE.VFMSUB213PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14565, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUB213PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14572, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUB213SD = new[] {
    new itemplate(OCE.VFMSUB213SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14936, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFMSUB213SS = new[] {
    new itemplate(OCE.VFMSUB213SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14929, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFMSUB231PD = new[] {
    new itemplate(OCE.VFMSUB231PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14607, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUB231PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14614, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUB231PS = new[] {
    new itemplate(OCE.VFMSUB231PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14593, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUB231PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14600, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUB231SD = new[] {
    new itemplate(OCE.VFMSUB231SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14950, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFMSUB231SS = new[] {
    new itemplate(OCE.VFMSUB231SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14943, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFMSUB312PD = new[] {
    new itemplate(OCE.VFMSUB312PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14551, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUB312PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14558, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUB312PS = new[] {
    new itemplate(OCE.VFMSUB312PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14537, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUB312PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14544, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUB312SD = new[] {
    new itemplate(OCE.VFMSUB312SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14922, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFMSUB312SS = new[] {
    new itemplate(OCE.VFMSUB312SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14915, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFMSUB321PD = new[] {
    new itemplate(OCE.VFMSUB321PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14607, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUB321PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14614, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUB321PS = new[] {
    new itemplate(OCE.VFMSUB321PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14593, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUB321PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14600, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUB321SD = new[] {
    new itemplate(OCE.VFMSUB321SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14950, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFMSUB321SS = new[] {
    new itemplate(OCE.VFMSUB321SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14943, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFMSUBADD123PD = new[] {
    new itemplate(OCE.VFMSUBADD123PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14663, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUBADD123PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14670, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUBADD123PS = new[] {
    new itemplate(OCE.VFMSUBADD123PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14649, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUBADD123PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14656, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUBADD132PD = new[] {
    new itemplate(OCE.VFMSUBADD132PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14635, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUBADD132PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14642, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUBADD132PS = new[] {
    new itemplate(OCE.VFMSUBADD132PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14621, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUBADD132PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14628, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUBADD213PD = new[] {
    new itemplate(OCE.VFMSUBADD213PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14663, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUBADD213PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14670, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUBADD213PS = new[] {
    new itemplate(OCE.VFMSUBADD213PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14649, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUBADD213PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14656, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUBADD231PD = new[] {
    new itemplate(OCE.VFMSUBADD231PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14691, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUBADD231PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14698, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUBADD231PS = new[] {
    new itemplate(OCE.VFMSUBADD231PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14677, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUBADD231PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14684, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUBADD312PD = new[] {
    new itemplate(OCE.VFMSUBADD312PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14635, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUBADD312PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14642, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUBADD312PS = new[] {
    new itemplate(OCE.VFMSUBADD312PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14621, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUBADD312PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14628, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUBADD321PD = new[] {
    new itemplate(OCE.VFMSUBADD321PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14691, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUBADD321PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14698, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUBADD321PS = new[] {
    new itemplate(OCE.VFMSUBADD321PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14677, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFMSUBADD321PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14684, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUBADDPD = new[] {
    new itemplate(OCE.VFMSUBADDPD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,5472, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMSUBADDPD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,5481, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMSUBADDPD, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,0} ,5490, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMSUBADDPD, 3, new ulong[] {YMMREG,RM_YMM,YMMREG,0,0} ,5499, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMSUBADDPD, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,5508, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMSUBADDPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,5517, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMSUBADDPD, 4, new ulong[] {YMMREG,YMMREG,YMMREG,RM_YMM,0} ,5526, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMSUBADDPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,5535, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUBADDPS = new[] {
    new itemplate(OCE.VFMSUBADDPS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,5544, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMSUBADDPS, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,5553, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMSUBADDPS, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,0} ,5562, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMSUBADDPS, 3, new ulong[] {YMMREG,RM_YMM,YMMREG,0,0} ,5571, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMSUBADDPS, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,5580, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMSUBADDPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,5589, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMSUBADDPS, 4, new ulong[] {YMMREG,YMMREG,YMMREG,RM_YMM,0} ,5598, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMSUBADDPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,5607, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUBPD = new[] {
    new itemplate(OCE.VFMSUBPD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,5616, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMSUBPD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,5625, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMSUBPD, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,0} ,5634, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMSUBPD, 3, new ulong[] {YMMREG,RM_YMM,YMMREG,0,0} ,5643, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMSUBPD, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,5652, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMSUBPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,5661, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMSUBPD, 4, new ulong[] {YMMREG,YMMREG,YMMREG,RM_YMM,0} ,5670, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMSUBPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,5679, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUBPS = new[] {
    new itemplate(OCE.VFMSUBPS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,5688, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMSUBPS, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,5697, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMSUBPS, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,0} ,5706, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMSUBPS, 3, new ulong[] {YMMREG,RM_YMM,YMMREG,0,0} ,5715, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMSUBPS, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,5724, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMSUBPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,5733, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFMSUBPS, 4, new ulong[] {YMMREG,YMMREG,YMMREG,RM_YMM,0} ,5742, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFMSUBPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,5751, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    
};

        static itemplate[] instrux_VFMSUBSD = new[] {
    new itemplate(OCE.VFMSUBSD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,5760, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    new itemplate(OCE.VFMSUBSD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,5769, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    new itemplate(OCE.VFMSUBSD, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,5778, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    new itemplate(OCE.VFMSUBSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,5787, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFMSUBSS = new[] {
    new itemplate(OCE.VFMSUBSS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,5796, OF.IF_AMD, OF.IF_SSE5, OF.IF_SD),
    new itemplate(OCE.VFMSUBSS, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,5805, OF.IF_AMD, OF.IF_SSE5, OF.IF_SD),
    new itemplate(OCE.VFMSUBSS, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,5814, OF.IF_AMD, OF.IF_SSE5, OF.IF_SD),
    new itemplate(OCE.VFMSUBSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,5823, OF.IF_AMD, OF.IF_SSE5, OF.IF_SD),
    
};

        static itemplate[] instrux_VFNMADD123PD = new[] {
    new itemplate(OCE.VFNMADD123PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14747, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMADD123PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14754, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMADD123PS = new[] {
    new itemplate(OCE.VFNMADD123PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14733, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMADD123PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14740, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMADD123SD = new[] {
    new itemplate(OCE.VFNMADD123SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14978, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFNMADD123SS = new[] {
    new itemplate(OCE.VFNMADD123SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14971, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFNMADD132PD = new[] {
    new itemplate(OCE.VFNMADD132PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14719, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMADD132PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14726, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMADD132PS = new[] {
    new itemplate(OCE.VFNMADD132PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14705, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMADD132PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14712, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMADD132SD = new[] {
    new itemplate(OCE.VFNMADD132SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14964, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFNMADD132SS = new[] {
    new itemplate(OCE.VFNMADD132SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14957, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFNMADD213PD = new[] {
    new itemplate(OCE.VFNMADD213PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14747, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMADD213PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14754, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMADD213PS = new[] {
    new itemplate(OCE.VFNMADD213PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14733, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMADD213PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14740, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMADD213SD = new[] {
    new itemplate(OCE.VFNMADD213SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14978, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFNMADD213SS = new[] {
    new itemplate(OCE.VFNMADD213SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14971, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFNMADD231PD = new[] {
    new itemplate(OCE.VFNMADD231PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14775, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMADD231PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14782, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMADD231PS = new[] {
    new itemplate(OCE.VFNMADD231PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14761, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMADD231PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14768, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMADD231SD = new[] {
    new itemplate(OCE.VFNMADD231SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14992, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFNMADD231SS = new[] {
    new itemplate(OCE.VFNMADD231SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14985, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFNMADD312PD = new[] {
    new itemplate(OCE.VFNMADD312PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14719, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMADD312PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14726, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMADD312PS = new[] {
    new itemplate(OCE.VFNMADD312PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14705, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMADD312PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14712, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMADD312SD = new[] {
    new itemplate(OCE.VFNMADD312SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14964, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFNMADD312SS = new[] {
    new itemplate(OCE.VFNMADD312SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14957, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFNMADD321PD = new[] {
    new itemplate(OCE.VFNMADD321PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14775, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMADD321PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14782, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMADD321PS = new[] {
    new itemplate(OCE.VFNMADD321PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14761, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMADD321PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14768, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMADD321SD = new[] {
    new itemplate(OCE.VFNMADD321SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14992, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFNMADD321SS = new[] {
    new itemplate(OCE.VFNMADD321SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14985, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFNMADDPD = new[] {
    new itemplate(OCE.VFNMADDPD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,5832, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFNMADDPD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,5841, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFNMADDPD, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,0} ,5850, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFNMADDPD, 3, new ulong[] {YMMREG,RM_YMM,YMMREG,0,0} ,5859, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFNMADDPD, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,5868, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFNMADDPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,5877, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFNMADDPD, 4, new ulong[] {YMMREG,YMMREG,YMMREG,RM_YMM,0} ,5886, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFNMADDPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,5895, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMADDPS = new[] {
    new itemplate(OCE.VFNMADDPS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,5904, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFNMADDPS, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,5913, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFNMADDPS, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,0} ,5922, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFNMADDPS, 3, new ulong[] {YMMREG,RM_YMM,YMMREG,0,0} ,5931, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFNMADDPS, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,5940, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFNMADDPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,5949, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFNMADDPS, 4, new ulong[] {YMMREG,YMMREG,YMMREG,RM_YMM,0} ,5958, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFNMADDPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,5967, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMADDSD = new[] {
    new itemplate(OCE.VFNMADDSD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,5976, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    new itemplate(OCE.VFNMADDSD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,5985, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    new itemplate(OCE.VFNMADDSD, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,5994, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    new itemplate(OCE.VFNMADDSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,6003, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFNMADDSS = new[] {
    new itemplate(OCE.VFNMADDSS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,6012, OF.IF_AMD, OF.IF_SSE5, OF.IF_SD),
    new itemplate(OCE.VFNMADDSS, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,6021, OF.IF_AMD, OF.IF_SSE5, OF.IF_SD),
    new itemplate(OCE.VFNMADDSS, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,6030, OF.IF_AMD, OF.IF_SSE5, OF.IF_SD),
    new itemplate(OCE.VFNMADDSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,6039, OF.IF_AMD, OF.IF_SSE5, OF.IF_SD),
    
};

        static itemplate[] instrux_VFNMSUB123PD = new[] {
    new itemplate(OCE.VFNMSUB123PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14831, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMSUB123PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14838, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMSUB123PS = new[] {
    new itemplate(OCE.VFNMSUB123PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14817, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMSUB123PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14824, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMSUB123SD = new[] {
    new itemplate(OCE.VFNMSUB123SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15020, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFNMSUB123SS = new[] {
    new itemplate(OCE.VFNMSUB123SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15013, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFNMSUB132PD = new[] {
    new itemplate(OCE.VFNMSUB132PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14803, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMSUB132PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14810, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMSUB132PS = new[] {
    new itemplate(OCE.VFNMSUB132PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14789, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMSUB132PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14796, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMSUB132SD = new[] {
    new itemplate(OCE.VFNMSUB132SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15006, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFNMSUB132SS = new[] {
    new itemplate(OCE.VFNMSUB132SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14999, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFNMSUB213PD = new[] {
    new itemplate(OCE.VFNMSUB213PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14831, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMSUB213PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14838, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMSUB213PS = new[] {
    new itemplate(OCE.VFNMSUB213PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14817, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMSUB213PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14824, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMSUB213SD = new[] {
    new itemplate(OCE.VFNMSUB213SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15020, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFNMSUB213SS = new[] {
    new itemplate(OCE.VFNMSUB213SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15013, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFNMSUB231PD = new[] {
    new itemplate(OCE.VFNMSUB231PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14859, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMSUB231PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14866, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMSUB231PS = new[] {
    new itemplate(OCE.VFNMSUB231PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14845, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMSUB231PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14852, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMSUB231SD = new[] {
    new itemplate(OCE.VFNMSUB231SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15034, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFNMSUB231SS = new[] {
    new itemplate(OCE.VFNMSUB231SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15027, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFNMSUB312PD = new[] {
    new itemplate(OCE.VFNMSUB312PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14803, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMSUB312PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14810, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMSUB312PS = new[] {
    new itemplate(OCE.VFNMSUB312PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14789, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMSUB312PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14796, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMSUB312SD = new[] {
    new itemplate(OCE.VFNMSUB312SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15006, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFNMSUB312SS = new[] {
    new itemplate(OCE.VFNMSUB312SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14999, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFNMSUB321PD = new[] {
    new itemplate(OCE.VFNMSUB321PD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14859, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMSUB321PD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14866, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMSUB321PS = new[] {
    new itemplate(OCE.VFNMSUB321PS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14845, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SO),
    new itemplate(OCE.VFNMSUB321PS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14852, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMSUB321SD = new[] {
    new itemplate(OCE.VFNMSUB321SD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15034, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFNMSUB321SS = new[] {
    new itemplate(OCE.VFNMSUB321SS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15027, OF.IF_FMA, OF.IF_FUTURE, OF.IF_SD),
    
};

        static itemplate[] instrux_VFNMSUBPD = new[] {
    new itemplate(OCE.VFNMSUBPD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,6048, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFNMSUBPD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,6057, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFNMSUBPD, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,0} ,6066, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFNMSUBPD, 3, new ulong[] {YMMREG,RM_YMM,YMMREG,0,0} ,6075, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFNMSUBPD, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,6084, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFNMSUBPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,6093, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFNMSUBPD, 4, new ulong[] {YMMREG,YMMREG,YMMREG,RM_YMM,0} ,6102, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFNMSUBPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,6111, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMSUBPS = new[] {
    new itemplate(OCE.VFNMSUBPS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,6120, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFNMSUBPS, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,6129, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFNMSUBPS, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,0} ,6138, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFNMSUBPS, 3, new ulong[] {YMMREG,RM_YMM,YMMREG,0,0} ,6147, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFNMSUBPS, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,6156, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFNMSUBPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,6165, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFNMSUBPS, 4, new ulong[] {YMMREG,YMMREG,YMMREG,RM_YMM,0} ,6174, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFNMSUBPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,6183, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    
};

        static itemplate[] instrux_VFNMSUBSD = new[] {
    new itemplate(OCE.VFNMSUBSD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,6192, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    new itemplate(OCE.VFNMSUBSD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,6201, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    new itemplate(OCE.VFNMSUBSD, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,6210, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    new itemplate(OCE.VFNMSUBSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,6219, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFNMSUBSS = new[] {
    new itemplate(OCE.VFNMSUBSS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,6228, OF.IF_AMD, OF.IF_SSE5, OF.IF_SD),
    new itemplate(OCE.VFNMSUBSS, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,6237, OF.IF_AMD, OF.IF_SSE5, OF.IF_SD),
    new itemplate(OCE.VFNMSUBSS, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,6246, OF.IF_AMD, OF.IF_SSE5, OF.IF_SD),
    new itemplate(OCE.VFNMSUBSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,6255, OF.IF_AMD, OF.IF_SSE5, OF.IF_SD),
    
};

        static itemplate[] instrux_VFRCZPD = new[] {
    new itemplate(OCE.VFRCZPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15041, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFRCZPD, 1, new ulong[] {XMMREG,0,0,0,0} ,15048, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFRCZPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,15055, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFRCZPD, 1, new ulong[] {YMMREG,0,0,0,0} ,15062, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    
};

        static itemplate[] instrux_VFRCZPS = new[] {
    new itemplate(OCE.VFRCZPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15069, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFRCZPS, 1, new ulong[] {XMMREG,0,0,0,0} ,15076, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VFRCZPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,15083, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VFRCZPS, 1, new ulong[] {YMMREG,0,0,0,0} ,15090, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    
};

        static itemplate[] instrux_VFRCZSD = new[] {
    new itemplate(OCE.VFRCZSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15097, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    new itemplate(OCE.VFRCZSD, 1, new ulong[] {XMMREG,0,0,0,0} ,15104, OF.IF_AMD, OF.IF_SSE5, OF.IF_SQ),
    
};

        static itemplate[] instrux_VFRCZSS = new[] {
    new itemplate(OCE.VFRCZSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15111, OF.IF_AMD, OF.IF_SSE5, OF.IF_SD),
    new itemplate(OCE.VFRCZSS, 1, new ulong[] {XMMREG,0,0,0,0} ,15118, OF.IF_AMD, OF.IF_SSE5, OF.IF_SD),
    
};

        static itemplate[] instrux_VHADDPD = new[] {
    new itemplate(OCE.VHADDPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11646, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VHADDPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11653, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VHADDPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,11660, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VHADDPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,11667, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VHADDPS = new[] {
    new itemplate(OCE.VHADDPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11674, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VHADDPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11681, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VHADDPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,11688, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VHADDPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,11695, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VHSUBPD = new[] {
    new itemplate(OCE.VHSUBPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11702, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VHSUBPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11709, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VHSUBPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,11716, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VHSUBPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,11723, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VHSUBPS = new[] {
    new itemplate(OCE.VHSUBPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11730, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VHSUBPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11737, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VHSUBPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,11744, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VHSUBPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,11751, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VINSERTF128 = new[] {
    new itemplate(OCE.VINSERTF128, 4, new ulong[] {YMMREG,YMMREG,RM_XMM,IMMEDIATE,0} ,7628, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VINSERTPS = new[] {
    new itemplate(OCE.VINSERTPS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7636, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VINSERTPS, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7644, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VLDDQU = new[] {
    new itemplate(OCE.VLDDQU, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,11758, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VLDDQU, 2, new ulong[] {YMMREG,MEMORY,0,0,0} ,11765, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VLDMXCSR = new[] {
    new itemplate(OCE.VLDMXCSR, 1, new ulong[] {MEMORY, BITS32,0,0,0,0} ,11772, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VLDQQU = new[] {
    new itemplate(OCE.VLDQQU, 2, new ulong[] {YMMREG,MEMORY,0,0,0} ,11765, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMASKMOVDQU = new[] {
    new itemplate(OCE.VMASKMOVDQU, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,11779, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    
};

        static itemplate[] instrux_VMASKMOVPD = new[] {
    new itemplate(OCE.VMASKMOVPD, 3, new ulong[] {XMMREG,XMMREG,MEMORY,0,0} ,11814, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMASKMOVPD, 3, new ulong[] {YMMREG,YMMREG,MEMORY,0,0} ,11821, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VMASKMOVPD, 3, new ulong[] {MEMORY,XMMREG,XMMREG,0,0} ,11828, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMASKMOVPD, 3, new ulong[] {MEMORY,YMMREG,YMMREG,0,0} ,11835, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMASKMOVPS = new[] {
    new itemplate(OCE.VMASKMOVPS, 3, new ulong[] {XMMREG,XMMREG,MEMORY,0,0} ,11786, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMASKMOVPS, 3, new ulong[] {YMMREG,YMMREG,MEMORY,0,0} ,11793, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VMASKMOVPS, 3, new ulong[] {MEMORY,XMMREG,XMMREG,0,0} ,11800, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMASKMOVPS, 3, new ulong[] {MEMORY,XMMREG,XMMREG,0,0} ,11807, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMAXPD = new[] {
    new itemplate(OCE.VMAXPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11842, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMAXPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11849, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMAXPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,11856, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VMAXPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,11863, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMAXPS = new[] {
    new itemplate(OCE.VMAXPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11870, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMAXPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11877, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMAXPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,11884, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VMAXPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,11891, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMAXSD = new[] {
    new itemplate(OCE.VMAXSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11898, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VMAXSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11905, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VMAXSS = new[] {
    new itemplate(OCE.VMAXSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11912, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VMAXSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11919, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VMCALL = new[] {
    new itemplate(OCE.VMCALL, 0, new ulong[] {0,0,0,0,0} ,20997, OF.IF_VMX),
    
};

        static itemplate[] instrux_VMCLEAR = new[] {
    new itemplate(OCE.VMCLEAR, 1, new ulong[] {MEMORY,0,0,0,0} ,17937, OF.IF_VMX),
    
};

        static itemplate[] instrux_VMINPD = new[] {
    new itemplate(OCE.VMINPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11926, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMINPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11933, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMINPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,11940, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VMINPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,11947, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMINPS = new[] {
    new itemplate(OCE.VMINPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11954, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMINPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11961, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMINPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,11968, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VMINPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,11975, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMINSD = new[] {
    new itemplate(OCE.VMINSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11982, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VMINSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,11989, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VMINSS = new[] {
    new itemplate(OCE.VMINSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,11996, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VMINSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12003, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VMLAUNCH = new[] {
    new itemplate(OCE.VMLAUNCH, 0, new ulong[] {0,0,0,0,0} ,21002, OF.IF_VMX),
    
};

        static itemplate[] instrux_VMLOAD = new[] {
    new itemplate(OCE.VMLOAD, 0, new ulong[] {0,0,0,0,0} ,21007, OF.IF_X64, OF.IF_VMX),
    
};

        static itemplate[] instrux_VMMCALL = new[] {
    new itemplate(OCE.VMMCALL, 0, new ulong[] {0,0,0,0,0} ,21012, OF.IF_X64, OF.IF_VMX),
    
};

        static itemplate[] instrux_VMOVAPD = new[] {
    new itemplate(OCE.VMOVAPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12010, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMOVAPD, 2, new ulong[] {RM_XMM,XMMREG,0,0,0} ,12017, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMOVAPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,12024, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VMOVAPD, 2, new ulong[] {RM_YMM,YMMREG,0,0,0} ,12031, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMOVAPS = new[] {
    new itemplate(OCE.VMOVAPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12038, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMOVAPS, 2, new ulong[] {RM_XMM,XMMREG,0,0,0} ,12045, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMOVAPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,12052, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VMOVAPS, 2, new ulong[] {RM_YMM,YMMREG,0,0,0} ,12059, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMOVD = new[] {
    new itemplate(OCE.VMOVD, 2, new ulong[] {XMMREG,RM_GPR, BITS32,0,0,0} ,12080, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VMOVD, 2, new ulong[] {RM_GPR, BITS32,XMMREG,0,0,0} ,12094, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VMOVDDUP = new[] {
    new itemplate(OCE.VMOVDDUP, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12108, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VMOVDDUP, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,12115, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMOVDQA = new[] {
    new itemplate(OCE.VMOVDQA, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12122, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMOVDQA, 2, new ulong[] {RM_XMM,XMMREG,0,0,0} ,12129, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMOVDQA, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,12136, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VMOVDQA, 2, new ulong[] {RM_YMM,YMMREG,0,0,0} ,12143, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMOVDQU = new[] {
    new itemplate(OCE.VMOVDQU, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12150, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMOVDQU, 2, new ulong[] {RM_XMM,XMMREG,0,0,0} ,12157, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMOVDQU, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,12164, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VMOVDQU, 2, new ulong[] {RM_YMM,YMMREG,0,0,0} ,12171, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMOVHLPS = new[] {
    new itemplate(OCE.VMOVHLPS, 3, new ulong[] {XMMREG,XMMREG,XMMREG,0,0} ,12178, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VMOVHLPS, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,12185, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    
};

        static itemplate[] instrux_VMOVHPD = new[] {
    new itemplate(OCE.VMOVHPD, 3, new ulong[] {XMMREG,XMMREG,MEMORY,0,0} ,12192, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VMOVHPD, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,12199, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VMOVHPD, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,12206, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VMOVHPS = new[] {
    new itemplate(OCE.VMOVHPS, 3, new ulong[] {XMMREG,XMMREG,MEMORY,0,0} ,12213, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VMOVHPS, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,12220, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VMOVHPS, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,12227, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VMOVLHPS = new[] {
    new itemplate(OCE.VMOVLHPS, 3, new ulong[] {XMMREG,XMMREG,XMMREG,0,0} ,12213, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VMOVLHPS, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,12220, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    
};

        static itemplate[] instrux_VMOVLPD = new[] {
    new itemplate(OCE.VMOVLPD, 3, new ulong[] {XMMREG,XMMREG,MEMORY,0,0} ,12234, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VMOVLPD, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,12241, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VMOVLPD, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,12248, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VMOVLPS = new[] {
    new itemplate(OCE.VMOVLPS, 3, new ulong[] {XMMREG,XMMREG,MEMORY,0,0} ,12178, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VMOVLPS, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,12185, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VMOVLPS, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,12255, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VMOVMSKPD = new[] {
    new itemplate(OCE.VMOVMSKPD, 2, new ulong[] {REG64,XMMREG,0,0,0} ,12262, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_LONG),
    new itemplate(OCE.VMOVMSKPD, 2, new ulong[] {REG32,XMMREG,0,0,0} ,12262, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VMOVMSKPD, 2, new ulong[] {REG64,YMMREG,0,0,0} ,12269, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_LONG),
    new itemplate(OCE.VMOVMSKPD, 2, new ulong[] {REG32,YMMREG,0,0,0} ,12269, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    
};

        static itemplate[] instrux_VMOVMSKPS = new[] {
    new itemplate(OCE.VMOVMSKPS, 2, new ulong[] {REG64,XMMREG,0,0,0} ,12276, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_LONG),
    new itemplate(OCE.VMOVMSKPS, 2, new ulong[] {REG32,XMMREG,0,0,0} ,12276, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VMOVMSKPS, 2, new ulong[] {REG64,YMMREG,0,0,0} ,12283, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_LONG),
    new itemplate(OCE.VMOVMSKPS, 2, new ulong[] {REG32,YMMREG,0,0,0} ,12283, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    
};

        static itemplate[] instrux_VMOVNTDQ = new[] {
    new itemplate(OCE.VMOVNTDQ, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,12290, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMOVNTDQ, 2, new ulong[] {MEMORY,YMMREG,0,0,0} ,12297, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMOVNTDQA = new[] {
    new itemplate(OCE.VMOVNTDQA, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,12304, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VMOVNTPD = new[] {
    new itemplate(OCE.VMOVNTPD, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,12311, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMOVNTPD, 2, new ulong[] {MEMORY,YMMREG,0,0,0} ,12318, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMOVNTPS = new[] {
    new itemplate(OCE.VMOVNTPS, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,12325, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMOVNTPS, 2, new ulong[] {MEMORY,YMMREG,0,0,0} ,12332, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VMOVNTQQ = new[] {
    new itemplate(OCE.VMOVNTQQ, 2, new ulong[] {MEMORY,YMMREG,0,0,0} ,12297, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMOVQ = new[] {
    new itemplate(OCE.VMOVQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12066, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VMOVQ, 2, new ulong[] {RM_XMM,XMMREG,0,0,0} ,12073, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VMOVQ, 2, new ulong[] {XMMREG,RM_GPR, BITS64,0,0,0} ,12087, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ, OF.IF_LONG),
    new itemplate(OCE.VMOVQ, 2, new ulong[] {RM_GPR, BITS64,XMMREG,0,0,0} ,12101, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ, OF.IF_LONG),
    
};

        static itemplate[] instrux_VMOVQQA = new[] {
    new itemplate(OCE.VMOVQQA, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,12136, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VMOVQQA, 2, new ulong[] {RM_YMM,YMMREG,0,0,0} ,12143, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMOVQQU = new[] {
    new itemplate(OCE.VMOVQQU, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,12164, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VMOVQQU, 2, new ulong[] {RM_YMM,YMMREG,0,0,0} ,12171, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMOVSD = new[] {
    new itemplate(OCE.VMOVSD, 3, new ulong[] {XMMREG,XMMREG,XMMREG,0,0} ,12339, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VMOVSD, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,12346, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VMOVSD, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,12353, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VMOVSD, 3, new ulong[] {XMMREG,XMMREG,XMMREG,0,0} ,12360, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VMOVSD, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,12367, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VMOVSD, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,12374, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VMOVSHDUP = new[] {
    new itemplate(OCE.VMOVSHDUP, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12381, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMOVSHDUP, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,12388, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMOVSLDUP = new[] {
    new itemplate(OCE.VMOVSLDUP, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12395, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMOVSLDUP, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,12402, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMOVSS = new[] {
    new itemplate(OCE.VMOVSS, 3, new ulong[] {XMMREG,XMMREG,XMMREG,0,0} ,12409, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VMOVSS, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,12416, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VMOVSS, 2, new ulong[] {XMMREG,MEMORY,0,0,0} ,12423, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VMOVSS, 3, new ulong[] {XMMREG,XMMREG,XMMREG,0,0} ,12430, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VMOVSS, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,12437, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VMOVSS, 2, new ulong[] {MEMORY,XMMREG,0,0,0} ,12444, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VMOVUPD = new[] {
    new itemplate(OCE.VMOVUPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12451, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMOVUPD, 2, new ulong[] {RM_XMM,XMMREG,0,0,0} ,12458, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMOVUPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,12465, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VMOVUPD, 2, new ulong[] {RM_YMM,YMMREG,0,0,0} ,12472, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMOVUPS = new[] {
    new itemplate(OCE.VMOVUPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12479, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMOVUPS, 2, new ulong[] {RM_XMM,XMMREG,0,0,0} ,12486, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMOVUPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,12493, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VMOVUPS, 2, new ulong[] {RM_YMM,YMMREG,0,0,0} ,12500, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMPSADBW = new[] {
    new itemplate(OCE.VMPSADBW, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7652, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMPSADBW, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7660, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VMPTRLD = new[] {
    new itemplate(OCE.VMPTRLD, 1, new ulong[] {MEMORY,0,0,0,0} ,17944, OF.IF_VMX),
    
};

        static itemplate[] instrux_VMPTRST = new[] {
    new itemplate(OCE.VMPTRST, 1, new ulong[] {MEMORY,0,0,0,0} ,21017, OF.IF_VMX),
    
};

        static itemplate[] instrux_VMREAD = new[] {
    new itemplate(OCE.VMREAD, 2, new ulong[] {RM_GPR, BITS32,REG32,0,0,0} ,9407, OF.IF_VMX, OF.IF_NOLONG, OF.IF_SD),
    new itemplate(OCE.VMREAD, 2, new ulong[] {RM_GPR, BITS64,REG64,0,0,0} ,9406, OF.IF_X64, OF.IF_VMX, OF.IF_SQ),
    
};

        static itemplate[] instrux_VMRESUME = new[] {
    new itemplate(OCE.VMRESUME, 0, new ulong[] {0,0,0,0,0} ,21022, OF.IF_VMX),
    
};

        static itemplate[] instrux_VMRUN = new[] {
    new itemplate(OCE.VMRUN, 0, new ulong[] {0,0,0,0,0} ,21027, OF.IF_X64, OF.IF_VMX),
    
};

        static itemplate[] instrux_VMSAVE = new[] {
    new itemplate(OCE.VMSAVE, 0, new ulong[] {0,0,0,0,0} ,21032, OF.IF_X64, OF.IF_VMX),
    
};

        static itemplate[] instrux_VMULPD = new[] {
    new itemplate(OCE.VMULPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12507, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMULPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12514, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMULPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,12521, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VMULPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,12528, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMULPS = new[] {
    new itemplate(OCE.VMULPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12535, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMULPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12542, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VMULPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,12549, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VMULPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,12556, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VMULSD = new[] {
    new itemplate(OCE.VMULSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12563, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VMULSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12570, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VMULSS = new[] {
    new itemplate(OCE.VMULSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12577, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VMULSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12584, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VMWRITE = new[] {
    new itemplate(OCE.VMWRITE, 2, new ulong[] {REG32,RM_GPR, BITS32,0,0,0} ,9414, OF.IF_VMX, OF.IF_NOLONG, OF.IF_SD),
    new itemplate(OCE.VMWRITE, 2, new ulong[] {REG64,RM_GPR, BITS64,0,0,0} ,9413, OF.IF_X64, OF.IF_VMX, OF.IF_SQ),
    
};

        static itemplate[] instrux_VMXOFF = new[] {
    new itemplate(OCE.VMXOFF, 0, new ulong[] {0,0,0,0,0} ,21037, OF.IF_VMX),
    
};

        static itemplate[] instrux_VMXON = new[] {
    new itemplate(OCE.VMXON, 1, new ulong[] {MEMORY,0,0,0,0} ,17943, OF.IF_VMX),
    
};

        static itemplate[] instrux_VORPD = new[] {
    new itemplate(OCE.VORPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12591, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VORPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12598, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VORPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,12605, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VORPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,12612, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VORPS = new[] {
    new itemplate(OCE.VORPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12619, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VORPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12626, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VORPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,12633, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VORPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,12640, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VPABSB = new[] {
    new itemplate(OCE.VPABSB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12647, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPABSD = new[] {
    new itemplate(OCE.VPABSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12661, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPABSW = new[] {
    new itemplate(OCE.VPABSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12654, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPACKSSDW = new[] {
    new itemplate(OCE.VPACKSSDW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12682, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPACKSSDW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12689, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPACKSSWB = new[] {
    new itemplate(OCE.VPACKSSWB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12668, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPACKSSWB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12675, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPACKUSDW = new[] {
    new itemplate(OCE.VPACKUSDW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12710, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPACKUSDW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12717, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPACKUSWB = new[] {
    new itemplate(OCE.VPACKUSWB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12696, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPACKUSWB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12703, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPADDB = new[] {
    new itemplate(OCE.VPADDB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12724, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPADDB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12731, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPADDD = new[] {
    new itemplate(OCE.VPADDD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12752, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPADDD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12759, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPADDQ = new[] {
    new itemplate(OCE.VPADDQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12766, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPADDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12773, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPADDSB = new[] {
    new itemplate(OCE.VPADDSB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12780, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPADDSB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12787, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPADDSW = new[] {
    new itemplate(OCE.VPADDSW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12794, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPADDSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12801, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPADDUSB = new[] {
    new itemplate(OCE.VPADDUSB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12808, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPADDUSB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12815, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPADDUSW = new[] {
    new itemplate(OCE.VPADDUSW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12822, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPADDUSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12829, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPADDW = new[] {
    new itemplate(OCE.VPADDW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12738, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPADDW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12745, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPALIGNR = new[] {
    new itemplate(OCE.VPALIGNR, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7668, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPALIGNR, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7676, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPAND = new[] {
    new itemplate(OCE.VPAND, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12836, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPAND, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12843, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPANDN = new[] {
    new itemplate(OCE.VPANDN, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12850, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPANDN, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12857, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPAVGB = new[] {
    new itemplate(OCE.VPAVGB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12864, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPAVGB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12871, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPAVGW = new[] {
    new itemplate(OCE.VPAVGW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12878, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPAVGW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12885, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPBLENDVB = new[] {
    new itemplate(OCE.VPBLENDVB, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,4698, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPBLENDVB, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,4707, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPBLENDW = new[] {
    new itemplate(OCE.VPBLENDW, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,7684, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPBLENDW, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7692, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCLMULHQHQDQ = new[] {
    new itemplate(OCE.VPCLMULHQHQDQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,5094, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPCLMULHQHQDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,5103, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCLMULHQLQDQ = new[] {
    new itemplate(OCE.VPCLMULHQLQDQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,5058, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPCLMULHQLQDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,5067, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCLMULLQHQDQ = new[] {
    new itemplate(OCE.VPCLMULLQHQDQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,5076, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPCLMULLQHQDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,5085, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCLMULLQLQDQ = new[] {
    new itemplate(OCE.VPCLMULLQLQDQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,5040, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPCLMULLQLQDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,5049, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCLMULQDQ = new[] {
    new itemplate(OCE.VPCLMULQDQ, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,8196, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPCLMULQDQ, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,8204, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCMOV = new[] {
    new itemplate(OCE.VPCMOV, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,6264, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPCMOV, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,6273, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPCMOV, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,0} ,6282, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VPCMOV, 3, new ulong[] {YMMREG,RM_YMM,YMMREG,0,0} ,6291, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VPCMOV, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,6300, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPCMOV, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,6309, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPCMOV, 4, new ulong[] {YMMREG,YMMREG,YMMREG,RM_YMM,0} ,6318, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    new itemplate(OCE.VPCMOV, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,6327, OF.IF_AMD, OF.IF_SSE5, OF.IF_SY),
    
};

        static itemplate[] instrux_VPCMPEQB = new[] {
    new itemplate(OCE.VPCMPEQB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12892, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPCMPEQB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12899, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCMPEQD = new[] {
    new itemplate(OCE.VPCMPEQD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12920, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPCMPEQD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12927, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCMPEQQ = new[] {
    new itemplate(OCE.VPCMPEQQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12934, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPCMPEQQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12941, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCMPEQW = new[] {
    new itemplate(OCE.VPCMPEQW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12906, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPCMPEQW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12913, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCMPESTRI = new[] {
    new itemplate(OCE.VPCMPESTRI, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7700, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCMPESTRM = new[] {
    new itemplate(OCE.VPCMPESTRM, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7708, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCMPGTB = new[] {
    new itemplate(OCE.VPCMPGTB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12948, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPCMPGTB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12955, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCMPGTD = new[] {
    new itemplate(OCE.VPCMPGTD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12976, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPCMPGTD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12983, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCMPGTQ = new[] {
    new itemplate(OCE.VPCMPGTQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12990, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPCMPGTQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12997, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCMPGTW = new[] {
    new itemplate(OCE.VPCMPGTW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,12962, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPCMPGTW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,12969, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCMPISTRI = new[] {
    new itemplate(OCE.VPCMPISTRI, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7716, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCMPISTRM = new[] {
    new itemplate(OCE.VPCMPISTRM, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7724, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCOMB = new[] {
    new itemplate(OCE.VPCOMB, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,8276, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPCOMB, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,8284, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCOMD = new[] {
    new itemplate(OCE.VPCOMD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,8292, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPCOMD, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,8300, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCOMQ = new[] {
    new itemplate(OCE.VPCOMQ, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,8308, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPCOMQ, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,8316, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCOMUB = new[] {
    new itemplate(OCE.VPCOMUB, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,8324, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPCOMUB, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,8332, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCOMUD = new[] {
    new itemplate(OCE.VPCOMUD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,8340, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPCOMUD, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,8348, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCOMUQ = new[] {
    new itemplate(OCE.VPCOMUQ, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,8356, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPCOMUQ, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,8364, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCOMUW = new[] {
    new itemplate(OCE.VPCOMUW, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,8372, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPCOMUW, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,8380, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPCOMW = new[] {
    new itemplate(OCE.VPCOMW, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,8388, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPCOMW, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,8396, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPERM2F128 = new[] {
    new itemplate(OCE.VPERM2F128, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,IMMEDIATE,0} ,7764, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VPERMIL2PD = new[] {
    new itemplate(OCE.VPERMIL2PD, 5, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,IMMEDIATE} ,4824, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPERMIL2PD, 5, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,IMMEDIATE} ,4833, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPERMIL2PD, 5, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,IMMEDIATE} ,4842, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VPERMIL2PD, 5, new ulong[] {YMMREG,YMMREG,YMMREG,RM_YMM,IMMEDIATE} ,4851, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VPERMIL2PS = new[] {
    new itemplate(OCE.VPERMIL2PS, 5, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,IMMEDIATE} ,4968, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPERMIL2PS, 5, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,IMMEDIATE} ,4977, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPERMIL2PS, 5, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,IMMEDIATE} ,4986, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VPERMIL2PS, 5, new ulong[] {YMMREG,YMMREG,YMMREG,RM_YMM,IMMEDIATE} ,4995, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VPERMILMO2PD = new[] {
    new itemplate(OCE.VPERMILMO2PD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,4752, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPERMILMO2PD, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,4761, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPERMILMO2PD, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,0} ,4770, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VPERMILMO2PD, 4, new ulong[] {YMMREG,YMMREG,YMMREG,RM_YMM,0} ,4779, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VPERMILMO2PS = new[] {
    new itemplate(OCE.VPERMILMO2PS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,4896, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPERMILMO2PS, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,4905, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPERMILMO2PS, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,0} ,4914, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VPERMILMO2PS, 4, new ulong[] {YMMREG,YMMREG,YMMREG,RM_YMM,0} ,4923, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VPERMILMZ2PD = new[] {
    new itemplate(OCE.VPERMILMZ2PD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,4788, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPERMILMZ2PD, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,4797, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPERMILMZ2PD, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,0} ,4806, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VPERMILMZ2PD, 4, new ulong[] {YMMREG,YMMREG,YMMREG,RM_YMM,0} ,4815, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VPERMILMZ2PS = new[] {
    new itemplate(OCE.VPERMILMZ2PS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,4932, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPERMILMZ2PS, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,4941, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPERMILMZ2PS, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,0} ,4950, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VPERMILMZ2PS, 4, new ulong[] {YMMREG,YMMREG,YMMREG,RM_YMM,0} ,4959, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VPERMILPD = new[] {
    new itemplate(OCE.VPERMILPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13004, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPERMILPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,13011, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VPERMILPD, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7732, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPERMILPD, 3, new ulong[] {YMMREG,RM_YMM,IMMEDIATE,0,0} ,7740, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VPERMILPS = new[] {
    new itemplate(OCE.VPERMILPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13018, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPERMILPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,13025, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VPERMILPS, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7748, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPERMILPS, 3, new ulong[] {YMMREG,RM_YMM,IMMEDIATE,0,0} ,7756, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VPERMILTD2PD = new[] {
    new itemplate(OCE.VPERMILTD2PD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,4716, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPERMILTD2PD, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,4725, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPERMILTD2PD, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,0} ,4734, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VPERMILTD2PD, 4, new ulong[] {YMMREG,YMMREG,YMMREG,RM_YMM,0} ,4743, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VPERMILTD2PS = new[] {
    new itemplate(OCE.VPERMILTD2PS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,4860, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPERMILTD2PS, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,4869, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPERMILTD2PS, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,YMMREG,0} ,4878, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VPERMILTD2PS, 4, new ulong[] {YMMREG,YMMREG,YMMREG,RM_YMM,0} ,4887, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VPEXTRB = new[] {
    new itemplate(OCE.VPEXTRB, 3, new ulong[] {REG64,XMMREG,IMMEDIATE,0,0} ,7772, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_LONG),
    new itemplate(OCE.VPEXTRB, 3, new ulong[] {REG32,XMMREG,IMMEDIATE,0,0} ,7772, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VPEXTRB, 3, new ulong[] {MEMORY,XMMREG,IMMEDIATE,0,0} ,7772, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SB),
    
};

        static itemplate[] instrux_VPEXTRD = new[] {
    new itemplate(OCE.VPEXTRD, 3, new ulong[] {REG64,XMMREG,IMMEDIATE,0,0} ,7796, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_LONG),
    new itemplate(OCE.VPEXTRD, 3, new ulong[] {RM_GPR, BITS32,XMMREG,IMMEDIATE,0,0} ,7796, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VPEXTRQ = new[] {
    new itemplate(OCE.VPEXTRQ, 3, new ulong[] {RM_GPR, BITS64,XMMREG,IMMEDIATE,0,0} ,7804, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ, OF.IF_LONG),
    
};

        static itemplate[] instrux_VPEXTRW = new[] {
    new itemplate(OCE.VPEXTRW, 3, new ulong[] {REG64,XMMREG,IMMEDIATE,0,0} ,7780, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_LONG),
    new itemplate(OCE.VPEXTRW, 3, new ulong[] {REG32,XMMREG,IMMEDIATE,0,0} ,7780, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VPEXTRW, 3, new ulong[] {MEMORY,XMMREG,IMMEDIATE,0,0} ,7780, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SW),
    new itemplate(OCE.VPEXTRW, 3, new ulong[] {REG64,XMMREG,IMMEDIATE,0,0} ,7788, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_LONG),
    new itemplate(OCE.VPEXTRW, 3, new ulong[] {REG32,XMMREG,IMMEDIATE,0,0} ,7788, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VPEXTRW, 3, new ulong[] {MEMORY,XMMREG,IMMEDIATE,0,0} ,7788, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SW),
    
};

        static itemplate[] instrux_VPHADDBD = new[] {
    new itemplate(OCE.VPHADDBD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15125, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPHADDBD, 1, new ulong[] {XMMREG,0,0,0,0} ,15132, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPHADDBQ = new[] {
    new itemplate(OCE.VPHADDBQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15139, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPHADDBQ, 1, new ulong[] {XMMREG,0,0,0,0} ,15146, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPHADDBW = new[] {
    new itemplate(OCE.VPHADDBW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15153, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPHADDBW, 1, new ulong[] {XMMREG,0,0,0,0} ,15160, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPHADDD = new[] {
    new itemplate(OCE.VPHADDD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13046, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPHADDD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13053, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPHADDDQ = new[] {
    new itemplate(OCE.VPHADDDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15167, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPHADDDQ, 1, new ulong[] {XMMREG,0,0,0,0} ,15174, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPHADDSW = new[] {
    new itemplate(OCE.VPHADDSW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13060, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPHADDSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13067, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPHADDUBD = new[] {
    new itemplate(OCE.VPHADDUBD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15181, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPHADDUBD, 1, new ulong[] {XMMREG,0,0,0,0} ,15188, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPHADDUBQ = new[] {
    new itemplate(OCE.VPHADDUBQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15195, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPHADDUBQ, 1, new ulong[] {XMMREG,0,0,0,0} ,15202, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPHADDUBWD = new[] {
    new itemplate(OCE.VPHADDUBWD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15209, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPHADDUBWD, 1, new ulong[] {XMMREG,0,0,0,0} ,15216, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPHADDUDQ = new[] {
    new itemplate(OCE.VPHADDUDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15223, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPHADDUDQ, 1, new ulong[] {XMMREG,0,0,0,0} ,15230, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPHADDUWD = new[] {
    new itemplate(OCE.VPHADDUWD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15237, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPHADDUWD, 1, new ulong[] {XMMREG,0,0,0,0} ,15244, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPHADDUWQ = new[] {
    new itemplate(OCE.VPHADDUWQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15251, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPHADDUWQ, 1, new ulong[] {XMMREG,0,0,0,0} ,15258, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPHADDW = new[] {
    new itemplate(OCE.VPHADDW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13032, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPHADDW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13039, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPHADDWD = new[] {
    new itemplate(OCE.VPHADDWD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15265, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPHADDWD, 1, new ulong[] {XMMREG,0,0,0,0} ,15272, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPHADDWQ = new[] {
    new itemplate(OCE.VPHADDWQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15251, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPHADDWQ, 1, new ulong[] {XMMREG,0,0,0,0} ,15258, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPHMINPOSUW = new[] {
    new itemplate(OCE.VPHMINPOSUW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13074, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPHSUBBW = new[] {
    new itemplate(OCE.VPHSUBBW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15279, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPHSUBBW, 1, new ulong[] {XMMREG,0,0,0,0} ,15286, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPHSUBD = new[] {
    new itemplate(OCE.VPHSUBD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13095, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPHSUBD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13102, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPHSUBDQ = new[] {
    new itemplate(OCE.VPHSUBDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15293, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPHSUBDQ, 1, new ulong[] {XMMREG,0,0,0,0} ,15300, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPHSUBSW = new[] {
    new itemplate(OCE.VPHSUBSW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13109, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPHSUBSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13116, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPHSUBW = new[] {
    new itemplate(OCE.VPHSUBW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13081, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPHSUBW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13088, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPHSUBWD = new[] {
    new itemplate(OCE.VPHSUBWD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15307, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPHSUBWD, 1, new ulong[] {XMMREG,0,0,0,0} ,15314, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPINSRB = new[] {
    new itemplate(OCE.VPINSRB, 4, new ulong[] {XMMREG,XMMREG,MEMORY,IMMEDIATE,0} ,7812, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SB, OF.IF_AR3),
    new itemplate(OCE.VPINSRB, 3, new ulong[] {XMMREG,MEMORY,IMMEDIATE,0,0} ,7820, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SB, OF.IF_AR3),
    new itemplate(OCE.VPINSRB, 4, new ulong[] {XMMREG,XMMREG,RM_GPR, BITS8,IMMEDIATE,0} ,7812, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SB, OF.IF_AR3),
    new itemplate(OCE.VPINSRB, 3, new ulong[] {XMMREG,RM_GPR, BITS8,IMMEDIATE,0,0} ,7820, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SB, OF.IF_AR3),
    new itemplate(OCE.VPINSRB, 4, new ulong[] {XMMREG,XMMREG,REG32,IMMEDIATE,0} ,7812, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SB, OF.IF_AR3),
    new itemplate(OCE.VPINSRB, 3, new ulong[] {XMMREG,REG32,IMMEDIATE,0,0} ,7820, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SB, OF.IF_AR3),
    
};

        static itemplate[] instrux_VPINSRD = new[] {
    new itemplate(OCE.VPINSRD, 4, new ulong[] {XMMREG,XMMREG,MEMORY,IMMEDIATE,0} ,7844, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SB, OF.IF_AR3),
    new itemplate(OCE.VPINSRD, 3, new ulong[] {XMMREG,MEMORY,IMMEDIATE,0,0} ,7852, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SB, OF.IF_AR3),
    new itemplate(OCE.VPINSRD, 4, new ulong[] {XMMREG,XMMREG,RM_GPR, BITS32,IMMEDIATE,0} ,7844, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SB, OF.IF_AR3),
    new itemplate(OCE.VPINSRD, 3, new ulong[] {XMMREG,RM_GPR, BITS32,IMMEDIATE,0,0} ,7852, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SB, OF.IF_AR3),
    
};

        static itemplate[] instrux_VPINSRQ = new[] {
    new itemplate(OCE.VPINSRQ, 4, new ulong[] {XMMREG,XMMREG,MEMORY,IMMEDIATE,0} ,7860, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SB, OF.IF_AR3, OF.IF_LONG),
    new itemplate(OCE.VPINSRQ, 3, new ulong[] {XMMREG,MEMORY,IMMEDIATE,0,0} ,7868, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SB, OF.IF_AR3, OF.IF_LONG),
    new itemplate(OCE.VPINSRQ, 4, new ulong[] {XMMREG,XMMREG,RM_GPR, BITS64,IMMEDIATE,0} ,7860, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SB, OF.IF_AR3, OF.IF_LONG),
    new itemplate(OCE.VPINSRQ, 3, new ulong[] {XMMREG,RM_GPR, BITS64,IMMEDIATE,0,0} ,7868, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SB, OF.IF_AR3, OF.IF_LONG),
    
};

        static itemplate[] instrux_VPINSRW = new[] {
    new itemplate(OCE.VPINSRW, 4, new ulong[] {XMMREG,XMMREG,MEMORY,IMMEDIATE,0} ,7828, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SB, OF.IF_AR3),
    new itemplate(OCE.VPINSRW, 3, new ulong[] {XMMREG,MEMORY,IMMEDIATE,0,0} ,7836, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SB, OF.IF_AR3),
    new itemplate(OCE.VPINSRW, 4, new ulong[] {XMMREG,XMMREG,RM_GPR, BITS16,IMMEDIATE,0} ,7828, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SB, OF.IF_AR3),
    new itemplate(OCE.VPINSRW, 3, new ulong[] {XMMREG,RM_GPR, BITS16,IMMEDIATE,0,0} ,7836, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SB, OF.IF_AR3),
    new itemplate(OCE.VPINSRW, 4, new ulong[] {XMMREG,XMMREG,REG32,IMMEDIATE,0} ,7828, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SB, OF.IF_AR3),
    new itemplate(OCE.VPINSRW, 3, new ulong[] {XMMREG,REG32,IMMEDIATE,0,0} ,7836, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SB, OF.IF_AR3),
    
};

        static itemplate[] instrux_VPMACSDD = new[] {
    new itemplate(OCE.VPMACSDD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,6336, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPMACSDD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,6345, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMACSDQH = new[] {
    new itemplate(OCE.VPMACSDQH, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,6354, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPMACSDQH, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,6363, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMACSDQL = new[] {
    new itemplate(OCE.VPMACSDQL, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,6372, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPMACSDQL, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,6381, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMACSSDD = new[] {
    new itemplate(OCE.VPMACSSDD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,6390, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPMACSSDD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,6399, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMACSSDQH = new[] {
    new itemplate(OCE.VPMACSSDQH, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,6408, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPMACSSDQH, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,6417, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMACSSDQL = new[] {
    new itemplate(OCE.VPMACSSDQL, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,6426, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPMACSSDQL, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,6435, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMACSSWD = new[] {
    new itemplate(OCE.VPMACSSWD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,6444, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPMACSSWD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,6453, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMACSSWW = new[] {
    new itemplate(OCE.VPMACSSWW, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,6462, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPMACSSWW, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,6471, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMACSWD = new[] {
    new itemplate(OCE.VPMACSWD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,6480, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPMACSWD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,6489, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMACSWW = new[] {
    new itemplate(OCE.VPMACSWW, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,6498, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPMACSWW, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,6507, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMADCSSWD = new[] {
    new itemplate(OCE.VPMADCSSWD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,6516, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPMADCSSWD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,6525, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMADCSWD = new[] {
    new itemplate(OCE.VPMADCSWD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,6534, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPMADCSWD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,6543, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMADDUBSW = new[] {
    new itemplate(OCE.VPMADDUBSW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13137, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPMADDUBSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13144, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMADDWD = new[] {
    new itemplate(OCE.VPMADDWD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13123, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPMADDWD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13130, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMAXSB = new[] {
    new itemplate(OCE.VPMAXSB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13151, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPMAXSB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13158, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMAXSD = new[] {
    new itemplate(OCE.VPMAXSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13179, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPMAXSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13186, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMAXSW = new[] {
    new itemplate(OCE.VPMAXSW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13165, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPMAXSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13172, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMAXUB = new[] {
    new itemplate(OCE.VPMAXUB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13193, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPMAXUB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13200, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMAXUD = new[] {
    new itemplate(OCE.VPMAXUD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13221, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPMAXUD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13228, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMAXUW = new[] {
    new itemplate(OCE.VPMAXUW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13207, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPMAXUW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13214, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMINSB = new[] {
    new itemplate(OCE.VPMINSB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13235, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPMINSB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13242, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMINSD = new[] {
    new itemplate(OCE.VPMINSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13263, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPMINSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13270, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMINSW = new[] {
    new itemplate(OCE.VPMINSW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13249, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPMINSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13256, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMINUB = new[] {
    new itemplate(OCE.VPMINUB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13277, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPMINUB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13284, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMINUD = new[] {
    new itemplate(OCE.VPMINUD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13305, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPMINUD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13312, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMINUW = new[] {
    new itemplate(OCE.VPMINUW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13291, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPMINUW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13298, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMOVMSKB = new[] {
    new itemplate(OCE.VPMOVMSKB, 2, new ulong[] {REG64,XMMREG,0,0,0} ,13319, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_LONG),
    new itemplate(OCE.VPMOVMSKB, 2, new ulong[] {REG32,XMMREG,0,0,0} ,13319, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    
};

        static itemplate[] instrux_VPMOVSXBD = new[] {
    new itemplate(OCE.VPMOVSXBD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13333, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VPMOVSXBQ = new[] {
    new itemplate(OCE.VPMOVSXBQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13340, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SW),
    
};

        static itemplate[] instrux_VPMOVSXBW = new[] {
    new itemplate(OCE.VPMOVSXBW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13326, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VPMOVSXDQ = new[] {
    new itemplate(OCE.VPMOVSXDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13361, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VPMOVSXWD = new[] {
    new itemplate(OCE.VPMOVSXWD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13347, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VPMOVSXWQ = new[] {
    new itemplate(OCE.VPMOVSXWQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13354, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VPMOVZXBD = new[] {
    new itemplate(OCE.VPMOVZXBD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13375, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VPMOVZXBQ = new[] {
    new itemplate(OCE.VPMOVZXBQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13382, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SW),
    
};

        static itemplate[] instrux_VPMOVZXBW = new[] {
    new itemplate(OCE.VPMOVZXBW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13368, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VPMOVZXDQ = new[] {
    new itemplate(OCE.VPMOVZXDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13403, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VPMOVZXWD = new[] {
    new itemplate(OCE.VPMOVZXWD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13389, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VPMOVZXWQ = new[] {
    new itemplate(OCE.VPMOVZXWQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13396, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VPMULDQ = new[] {
    new itemplate(OCE.VPMULDQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13494, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPMULDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13501, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMULHRSW = new[] {
    new itemplate(OCE.VPMULHRSW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13424, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPMULHRSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13431, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMULHUW = new[] {
    new itemplate(OCE.VPMULHUW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13410, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPMULHUW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13417, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMULHW = new[] {
    new itemplate(OCE.VPMULHW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13438, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPMULHW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13445, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMULLD = new[] {
    new itemplate(OCE.VPMULLD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13466, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPMULLD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13473, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMULLW = new[] {
    new itemplate(OCE.VPMULLW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13452, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPMULLW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13459, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPMULUDQ = new[] {
    new itemplate(OCE.VPMULUDQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13480, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPMULUDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13487, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPOR = new[] {
    new itemplate(OCE.VPOR, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13508, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPOR, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13515, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPPERM = new[] {
    new itemplate(OCE.VPPERM, 4, new ulong[] {XMMREG,XMMREG,XMMREG,RM_XMM,0} ,6552, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPPERM, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,6561, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPPERM, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,XMMREG,0} ,6570, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPPERM, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,6579, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPROTB = new[] {
    new itemplate(OCE.VPROTB, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,15321, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPROTB, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,15328, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPROTB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15335, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPROTB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15342, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPROTB, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,8404, OF.IF_AMD, OF.IF_SSE5),
    new itemplate(OCE.VPROTB, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,8412, OF.IF_AMD, OF.IF_SSE5),
    
};

        static itemplate[] instrux_VPROTD = new[] {
    new itemplate(OCE.VPROTD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,15349, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPROTD, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,15356, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPROTD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15363, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPROTD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15370, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPROTD, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,8420, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPROTD, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,8428, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPROTQ = new[] {
    new itemplate(OCE.VPROTQ, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,15377, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPROTQ, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,15384, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPROTQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15391, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPROTQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15398, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPROTQ, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,8436, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPROTQ, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,8444, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPROTW = new[] {
    new itemplate(OCE.VPROTW, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,15405, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPROTW, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,15412, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPROTW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15419, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPROTW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15426, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPROTW, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,8452, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPROTW, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,8460, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSADBW = new[] {
    new itemplate(OCE.VPSADBW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13522, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSADBW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13529, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSHAB = new[] {
    new itemplate(OCE.VPSHAB, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,15433, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHAB, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,15440, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHAB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15447, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHAB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15454, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSHAD = new[] {
    new itemplate(OCE.VPSHAD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,15461, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHAD, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,15468, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHAD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15475, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHAD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15482, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSHAQ = new[] {
    new itemplate(OCE.VPSHAQ, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,15489, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHAQ, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,15496, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHAQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15503, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHAQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15510, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSHAW = new[] {
    new itemplate(OCE.VPSHAW, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,15517, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHAW, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,15524, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHAW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15531, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHAW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15538, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSHLB = new[] {
    new itemplate(OCE.VPSHLB, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,15545, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHLB, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,15552, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHLB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15559, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHLB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15566, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSHLD = new[] {
    new itemplate(OCE.VPSHLD, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,15573, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHLD, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,15580, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHLD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15587, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHLD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15594, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSHLQ = new[] {
    new itemplate(OCE.VPSHLQ, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,15601, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHLQ, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,15608, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHLQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15615, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHLQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15622, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSHLW = new[] {
    new itemplate(OCE.VPSHLW, 3, new ulong[] {XMMREG,RM_XMM,XMMREG,0,0} ,15629, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHLW, 2, new ulong[] {XMMREG,XMMREG,0,0,0} ,15636, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHLW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,15643, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    new itemplate(OCE.VPSHLW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,15650, OF.IF_AMD, OF.IF_SSE5, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSHUFB = new[] {
    new itemplate(OCE.VPSHUFB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13536, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSHUFB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13543, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSHUFD = new[] {
    new itemplate(OCE.VPSHUFD, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7876, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSHUFHW = new[] {
    new itemplate(OCE.VPSHUFHW, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7884, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSHUFLW = new[] {
    new itemplate(OCE.VPSHUFLW, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,7892, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSIGNB = new[] {
    new itemplate(OCE.VPSIGNB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13550, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSIGNB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13557, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSIGND = new[] {
    new itemplate(OCE.VPSIGND, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13578, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSIGND, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13585, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSIGNW = new[] {
    new itemplate(OCE.VPSIGNW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13564, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSIGNW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13571, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSLLD = new[] {
    new itemplate(OCE.VPSLLD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13606, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSLLD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13613, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSLLD, 3, new ulong[] {XMMREG,XMMREG,IMMEDIATE,0,0} ,7948, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VPSLLD, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,7956, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    
};

        static itemplate[] instrux_VPSLLDQ = new[] {
    new itemplate(OCE.VPSLLDQ, 3, new ulong[] {XMMREG,XMMREG,IMMEDIATE,0,0} ,7900, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VPSLLDQ, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,7908, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    
};

        static itemplate[] instrux_VPSLLQ = new[] {
    new itemplate(OCE.VPSLLQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13620, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSLLQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13627, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSLLQ, 3, new ulong[] {XMMREG,XMMREG,IMMEDIATE,0,0} ,7964, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VPSLLQ, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,7972, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    
};

        static itemplate[] instrux_VPSLLW = new[] {
    new itemplate(OCE.VPSLLW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13592, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSLLW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13599, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSLLW, 3, new ulong[] {XMMREG,XMMREG,IMMEDIATE,0,0} ,7932, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VPSLLW, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,7940, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    
};

        static itemplate[] instrux_VPSRAD = new[] {
    new itemplate(OCE.VPSRAD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13648, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSRAD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13655, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSRAD, 3, new ulong[] {XMMREG,XMMREG,IMMEDIATE,0,0} ,7996, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VPSRAD, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,8004, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    
};

        static itemplate[] instrux_VPSRAW = new[] {
    new itemplate(OCE.VPSRAW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13634, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSRAW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13641, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSRAW, 3, new ulong[] {XMMREG,XMMREG,IMMEDIATE,0,0} ,7980, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VPSRAW, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,7988, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    
};

        static itemplate[] instrux_VPSRLD = new[] {
    new itemplate(OCE.VPSRLD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13676, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSRLD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13683, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSRLD, 3, new ulong[] {XMMREG,XMMREG,IMMEDIATE,0,0} ,8028, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VPSRLD, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,8036, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    
};

        static itemplate[] instrux_VPSRLDQ = new[] {
    new itemplate(OCE.VPSRLDQ, 3, new ulong[] {XMMREG,XMMREG,IMMEDIATE,0,0} ,7916, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VPSRLDQ, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,7924, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    
};

        static itemplate[] instrux_VPSRLQ = new[] {
    new itemplate(OCE.VPSRLQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13690, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSRLQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13697, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSRLQ, 3, new ulong[] {XMMREG,XMMREG,IMMEDIATE,0,0} ,8044, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VPSRLQ, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,8052, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    
};

        static itemplate[] instrux_VPSRLW = new[] {
    new itemplate(OCE.VPSRLW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13662, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSRLW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13669, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSRLW, 3, new ulong[] {XMMREG,XMMREG,IMMEDIATE,0,0} ,8012, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    new itemplate(OCE.VPSRLW, 2, new ulong[] {XMMREG,IMMEDIATE,0,0,0} ,8020, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    
};

        static itemplate[] instrux_VPSUBB = new[] {
    new itemplate(OCE.VPSUBB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13718, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSUBB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13725, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSUBD = new[] {
    new itemplate(OCE.VPSUBD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13746, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSUBD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13753, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSUBQ = new[] {
    new itemplate(OCE.VPSUBQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13760, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSUBQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13767, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSUBSB = new[] {
    new itemplate(OCE.VPSUBSB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13774, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSUBSB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13781, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSUBSW = new[] {
    new itemplate(OCE.VPSUBSW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13788, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSUBSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13795, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSUBUSB = new[] {
    new itemplate(OCE.VPSUBUSB, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13802, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSUBUSB, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13809, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSUBUSW = new[] {
    new itemplate(OCE.VPSUBUSW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13816, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSUBUSW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13823, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPSUBW = new[] {
    new itemplate(OCE.VPSUBW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13732, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPSUBW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13739, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPTEST = new[] {
    new itemplate(OCE.VPTEST, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13704, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPTEST, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,13711, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VPUNPCKHBW = new[] {
    new itemplate(OCE.VPUNPCKHBW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13830, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPUNPCKHBW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13837, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPUNPCKHDQ = new[] {
    new itemplate(OCE.VPUNPCKHDQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13858, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPUNPCKHDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13865, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPUNPCKHQDQ = new[] {
    new itemplate(OCE.VPUNPCKHQDQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13872, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPUNPCKHQDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13879, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPUNPCKHWD = new[] {
    new itemplate(OCE.VPUNPCKHWD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13844, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPUNPCKHWD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13851, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPUNPCKLBW = new[] {
    new itemplate(OCE.VPUNPCKLBW, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13886, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPUNPCKLBW, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13893, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPUNPCKLDQ = new[] {
    new itemplate(OCE.VPUNPCKLDQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13914, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPUNPCKLDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13921, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPUNPCKLQDQ = new[] {
    new itemplate(OCE.VPUNPCKLQDQ, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13928, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPUNPCKLQDQ, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13935, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPUNPCKLWD = new[] {
    new itemplate(OCE.VPUNPCKLWD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13900, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPUNPCKLWD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13907, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VPXOR = new[] {
    new itemplate(OCE.VPXOR, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13942, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VPXOR, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13949, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    
};

        static itemplate[] instrux_VRCPPS = new[] {
    new itemplate(OCE.VRCPPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13956, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VRCPPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,13963, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VRCPSS = new[] {
    new itemplate(OCE.VRCPSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13970, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VRCPSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13977, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VROUNDPD = new[] {
    new itemplate(OCE.VROUNDPD, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,8060, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VROUNDPD, 3, new ulong[] {YMMREG,RM_YMM,IMMEDIATE,0,0} ,8068, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VROUNDPS = new[] {
    new itemplate(OCE.VROUNDPS, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,8076, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VROUNDPS, 3, new ulong[] {YMMREG,RM_YMM,IMMEDIATE,0,0} ,8084, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VROUNDSD = new[] {
    new itemplate(OCE.VROUNDSD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,8092, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VROUNDSD, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,8100, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VROUNDSS = new[] {
    new itemplate(OCE.VROUNDSS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,8108, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VROUNDSS, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,8116, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VRSQRTPS = new[] {
    new itemplate(OCE.VRSQRTPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,13984, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VRSQRTPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,13991, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VRSQRTSS = new[] {
    new itemplate(OCE.VRSQRTSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,13998, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VRSQRTSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,14005, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VSHUFPD = new[] {
    new itemplate(OCE.VSHUFPD, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,8124, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VSHUFPD, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,8132, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VSHUFPD, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,IMMEDIATE,0} ,8140, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VSHUFPD, 3, new ulong[] {YMMREG,RM_YMM,IMMEDIATE,0,0} ,8148, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VSHUFPS = new[] {
    new itemplate(OCE.VSHUFPS, 4, new ulong[] {XMMREG,XMMREG,RM_XMM,IMMEDIATE,0} ,8156, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VSHUFPS, 3, new ulong[] {XMMREG,RM_XMM,IMMEDIATE,0,0} ,8164, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VSHUFPS, 4, new ulong[] {YMMREG,YMMREG,RM_YMM,IMMEDIATE,0} ,8172, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VSHUFPS, 3, new ulong[] {YMMREG,RM_YMM,IMMEDIATE,0,0} ,8180, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VSQRTPD = new[] {
    new itemplate(OCE.VSQRTPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,14012, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VSQRTPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,14019, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VSQRTPS = new[] {
    new itemplate(OCE.VSQRTPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,14026, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VSQRTPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,14033, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VSQRTSD = new[] {
    new itemplate(OCE.VSQRTSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14040, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VSQRTSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,14047, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VSQRTSS = new[] {
    new itemplate(OCE.VSQRTSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14054, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VSQRTSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,14061, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VSTMXCSR = new[] {
    new itemplate(OCE.VSTMXCSR, 1, new ulong[] {MEMORY,0,0,0,0} ,14068, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VSUBPD = new[] {
    new itemplate(OCE.VSUBPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14075, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VSUBPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,14082, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VSUBPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14089, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VSUBPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,14096, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VSUBPS = new[] {
    new itemplate(OCE.VSUBPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14103, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VSUBPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,14110, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VSUBPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14117, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VSUBPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,14124, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VSUBSD = new[] {
    new itemplate(OCE.VSUBSD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14131, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    new itemplate(OCE.VSUBSD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,14138, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VSUBSS = new[] {
    new itemplate(OCE.VSUBSS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14145, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    new itemplate(OCE.VSUBSS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,14152, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VTESTPD = new[] {
    new itemplate(OCE.VTESTPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,14173, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VTESTPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,14180, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VTESTPS = new[] {
    new itemplate(OCE.VTESTPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,14159, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VTESTPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,14166, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VUCOMISD = new[] {
    new itemplate(OCE.VUCOMISD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,14187, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SQ),
    
};

        static itemplate[] instrux_VUCOMISS = new[] {
    new itemplate(OCE.VUCOMISS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,14194, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SD),
    
};

        static itemplate[] instrux_VUNPCKHPD = new[] {
    new itemplate(OCE.VUNPCKHPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14201, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VUNPCKHPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,14208, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VUNPCKHPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14215, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VUNPCKHPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,14222, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VUNPCKHPS = new[] {
    new itemplate(OCE.VUNPCKHPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14229, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VUNPCKHPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,14236, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VUNPCKHPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14243, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VUNPCKHPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,14250, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VUNPCKLPD = new[] {
    new itemplate(OCE.VUNPCKLPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14257, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VUNPCKLPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,14264, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VUNPCKLPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14271, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VUNPCKLPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,14278, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VUNPCKLPS = new[] {
    new itemplate(OCE.VUNPCKLPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14285, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VUNPCKLPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,14292, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VUNPCKLPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14299, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VUNPCKLPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,14306, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VXORPD = new[] {
    new itemplate(OCE.VXORPD, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14313, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VXORPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,14320, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VXORPD, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14327, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VXORPD, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,14334, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VXORPS = new[] {
    new itemplate(OCE.VXORPS, 3, new ulong[] {XMMREG,XMMREG,RM_XMM,0,0} ,14341, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VXORPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,14348, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SO),
    new itemplate(OCE.VXORPS, 3, new ulong[] {YMMREG,YMMREG,RM_YMM,0,0} ,14355, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    new itemplate(OCE.VXORPS, 2, new ulong[] {YMMREG,RM_YMM,0,0,0} ,14362, OF.IF_AVX, OF.IF_SANDYBRIDGE, OF.IF_SY),
    
};

        static itemplate[] instrux_VZEROALL = new[] {
    new itemplate(OCE.VZEROALL, 0, new ulong[] {0,0,0,0,0} ,17973, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    
};

        static itemplate[] instrux_VZEROUPPER = new[] {
    new itemplate(OCE.VZEROUPPER, 0, new ulong[] {0,0,0,0,0} ,17979, OF.IF_AVX, OF.IF_SANDYBRIDGE),
    
};

        static itemplate[] instrux_WBINVD = new[] {
    new itemplate(OCE.WBINVD, 0, new ulong[] {0,0,0,0,0} ,22039, OF.IF_486, OF.IF_PRIV),
    
};

        static itemplate[] instrux_WRMSR = new[] {
    new itemplate(OCE.WRMSR, 0, new ulong[] {0,0,0,0,0} ,22043, OF.IF_PENT, OF.IF_PRIV),
    
};

        static itemplate[] instrux_WRSHR = new[] {
    new itemplate(OCE.WRSHR, 1, new ulong[] {RM_GPR, BITS32,0,0,0,0} ,16809, OF.IF_P6, OF.IF_CYRIX, OF.IF_SMM),
    
};

        static itemplate[] instrux_XADD = new[] {
    new itemplate(OCE.XADD, 2, new ulong[] {MEMORY,REG8,0,0,0} ,20882, OF.IF_486, OF.IF_SM),
    new itemplate(OCE.XADD, 2, new ulong[] {REG8,REG8,0,0,0} ,20882, OF.IF_486),
    new itemplate(OCE.XADD, 2, new ulong[] {MEMORY,REG16,0,0,0} ,16815, OF.IF_486, OF.IF_SM),
    new itemplate(OCE.XADD, 2, new ulong[] {REG16,REG16,0,0,0} ,16815, OF.IF_486),
    new itemplate(OCE.XADD, 2, new ulong[] {MEMORY,REG32,0,0,0} ,16821, OF.IF_486, OF.IF_SM),
    new itemplate(OCE.XADD, 2, new ulong[] {REG32,REG32,0,0,0} ,16821, OF.IF_486),
    new itemplate(OCE.XADD, 2, new ulong[] {MEMORY,REG64,0,0,0} ,16827, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.XADD, 2, new ulong[] {REG64,REG64,0,0,0} ,16827, OF.IF_X64),
    
};

        static itemplate[] instrux_XBTS = new[] {
    new itemplate(OCE.XBTS, 2, new ulong[] {REG16,MEMORY,0,0,0} ,16833, OF.IF_386, OF.IF_SW, OF.IF_UNDOC),
    new itemplate(OCE.XBTS, 2, new ulong[] {REG16,REG16,0,0,0} ,16833, OF.IF_386, OF.IF_UNDOC),
    new itemplate(OCE.XBTS, 2, new ulong[] {REG32,MEMORY,0,0,0} ,16839, OF.IF_386, OF.IF_SD, OF.IF_UNDOC),
    new itemplate(OCE.XBTS, 2, new ulong[] {REG32,REG32,0,0,0} ,16839, OF.IF_386, OF.IF_UNDOC),
    
};

        static itemplate[] instrux_XCHG = new[] {
    new itemplate(OCE.XCHG, 2, new ulong[] {REG_AX,REG16,0,0,0} ,22047, OF.IF_8086),
    new itemplate(OCE.XCHG, 2, new ulong[] {REG_EAX,REG32NA,0,0,0} ,22051, OF.IF_386),
    new itemplate(OCE.XCHG, 2, new ulong[] {REG_RAX,REG64,0,0,0} ,22055, OF.IF_X64),
    new itemplate(OCE.XCHG, 2, new ulong[] {REG16,REG_AX,0,0,0} ,22059, OF.IF_8086),
    new itemplate(OCE.XCHG, 2, new ulong[] {REG32NA,REG_EAX,0,0,0} ,22063, OF.IF_386),
    new itemplate(OCE.XCHG, 2, new ulong[] {REG64,REG_RAX,0,0,0} ,22067, OF.IF_X64),
    new itemplate(OCE.XCHG, 2, new ulong[] {REG_EAX,REG_EAX,0,0,0} ,22071, OF.IF_386, OF.IF_NOLONG),
    new itemplate(OCE.XCHG, 2, new ulong[] {REG8,MEMORY,0,0,0} ,22075, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.XCHG, 2, new ulong[] {REG8,REG8,0,0,0} ,22075, OF.IF_8086),
    new itemplate(OCE.XCHG, 2, new ulong[] {REG16,MEMORY,0,0,0} ,20887, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.XCHG, 2, new ulong[] {REG16,REG16,0,0,0} ,20887, OF.IF_8086),
    new itemplate(OCE.XCHG, 2, new ulong[] {REG32,MEMORY,0,0,0} ,20892, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.XCHG, 2, new ulong[] {REG32,REG32,0,0,0} ,20892, OF.IF_386),
    new itemplate(OCE.XCHG, 2, new ulong[] {REG64,MEMORY,0,0,0} ,20897, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.XCHG, 2, new ulong[] {REG64,REG64,0,0,0} ,20897, OF.IF_X64),
    new itemplate(OCE.XCHG, 2, new ulong[] {MEMORY,REG8,0,0,0} ,22079, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.XCHG, 2, new ulong[] {REG8,REG8,0,0,0} ,22079, OF.IF_8086),
    new itemplate(OCE.XCHG, 2, new ulong[] {MEMORY,REG16,0,0,0} ,20902, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.XCHG, 2, new ulong[] {REG16,REG16,0,0,0} ,20902, OF.IF_8086),
    new itemplate(OCE.XCHG, 2, new ulong[] {MEMORY,REG32,0,0,0} ,20907, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.XCHG, 2, new ulong[] {REG32,REG32,0,0,0} ,20907, OF.IF_386),
    new itemplate(OCE.XCHG, 2, new ulong[] {MEMORY,REG64,0,0,0} ,20912, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.XCHG, 2, new ulong[] {REG64,REG64,0,0,0} ,20912, OF.IF_X64),
    
};

        static itemplate[] instrux_XCRYPTCBC = new[] {
    new itemplate(OCE.XCRYPTCBC, 0, new ulong[] {0,0,0,0,0} ,17991, OF.IF_PENT, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_XCRYPTCFB = new[] {
    new itemplate(OCE.XCRYPTCFB, 0, new ulong[] {0,0,0,0,0} ,18003, OF.IF_PENT, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_XCRYPTCTR = new[] {
    new itemplate(OCE.XCRYPTCTR, 0, new ulong[] {0,0,0,0,0} ,17997, OF.IF_PENT, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_XCRYPTECB = new[] {
    new itemplate(OCE.XCRYPTECB, 0, new ulong[] {0,0,0,0,0} ,17985, OF.IF_PENT, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_XCRYPTOFB = new[] {
    new itemplate(OCE.XCRYPTOFB, 0, new ulong[] {0,0,0,0,0} ,18009, OF.IF_PENT, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_XGETBV = new[] {
    new itemplate(OCE.XGETBV, 0, new ulong[] {0,0,0,0,0} ,17127, OF.IF_NEHALEM),
    
};

        static itemplate[] instrux_XLAT = new[] {
    new itemplate(OCE.XLAT, 0, new ulong[] {0,0,0,0,0} ,22158, OF.IF_8086),
    
};

        static itemplate[] instrux_XLATB = new[] {
    new itemplate(OCE.XLATB, 0, new ulong[] {0,0,0,0,0} ,22158, OF.IF_8086),
    
};

        static itemplate[] instrux_XOR = new[] {
    new itemplate(OCE.XOR, 2, new ulong[] {MEMORY,REG8,0,0,0} ,22083, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.XOR, 2, new ulong[] {REG8,REG8,0,0,0} ,22083, OF.IF_8086),
    new itemplate(OCE.XOR, 2, new ulong[] {MEMORY,REG16,0,0,0} ,20917, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.XOR, 2, new ulong[] {REG16,REG16,0,0,0} ,20917, OF.IF_8086),
    new itemplate(OCE.XOR, 2, new ulong[] {MEMORY,REG32,0,0,0} ,20922, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.XOR, 2, new ulong[] {REG32,REG32,0,0,0} ,20922, OF.IF_386),
    new itemplate(OCE.XOR, 2, new ulong[] {MEMORY,REG64,0,0,0} ,20927, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.XOR, 2, new ulong[] {REG64,REG64,0,0,0} ,20927, OF.IF_X64),
    new itemplate(OCE.XOR, 2, new ulong[] {REG8,MEMORY,0,0,0} ,13385, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.XOR, 2, new ulong[] {REG8,REG8,0,0,0} ,13385, OF.IF_8086),
    new itemplate(OCE.XOR, 2, new ulong[] {REG16,MEMORY,0,0,0} ,20932, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.XOR, 2, new ulong[] {REG16,REG16,0,0,0} ,20932, OF.IF_8086),
    new itemplate(OCE.XOR, 2, new ulong[] {REG32,MEMORY,0,0,0} ,20937, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.XOR, 2, new ulong[] {REG32,REG32,0,0,0} ,20937, OF.IF_386),
    new itemplate(OCE.XOR, 2, new ulong[] {REG64,MEMORY,0,0,0} ,20942, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.XOR, 2, new ulong[] {REG64,REG64,0,0,0} ,20942, OF.IF_X64),
    new itemplate(OCE.XOR, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE, BITS8,0,0,0} ,16845, OF.IF_8086),
    new itemplate(OCE.XOR, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE, BITS8,0,0,0} ,16851, OF.IF_386),
    new itemplate(OCE.XOR, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE, BITS8,0,0,0} ,16857, OF.IF_X64),
    new itemplate(OCE.XOR, 2, new ulong[] {REG_AL,IMMEDIATE,0,0,0} ,22087, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.XOR, 2, new ulong[] {REG_AX,SBYTE16,0,0,0} ,16845, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.XOR, 2, new ulong[] {REG_AX,IMMEDIATE,0,0,0} ,20947, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.XOR, 2, new ulong[] {REG_EAX,SBYTE32,0,0,0} ,16851, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.XOR, 2, new ulong[] {REG_EAX,IMMEDIATE,0,0,0} ,20952, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.XOR, 2, new ulong[] {REG_RAX,SBYTE64,0,0,0} ,16857, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.XOR, 2, new ulong[] {REG_RAX,IMMEDIATE,0,0,0} ,20957, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.XOR, 2, new ulong[] {RM_GPR, BITS8,IMMEDIATE,0,0,0} ,20962, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.XOR, 2, new ulong[] {RM_GPR, BITS16,IMMEDIATE,0,0,0} ,16863, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.XOR, 2, new ulong[] {RM_GPR, BITS32,IMMEDIATE,0,0,0} ,16869, OF.IF_386, OF.IF_SM),
    new itemplate(OCE.XOR, 2, new ulong[] {RM_GPR, BITS64,IMMEDIATE,0,0,0} ,16875, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.XOR, 2, new ulong[] {MEMORY,IMMEDIATE, BITS8,0,0,0} ,20962, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.XOR, 2, new ulong[] {MEMORY,IMMEDIATE, BITS16,0,0,0} ,16863, OF.IF_8086, OF.IF_SM),
    new itemplate(OCE.XOR, 2, new ulong[] {MEMORY,IMMEDIATE, BITS32,0,0,0} ,16869, OF.IF_386, OF.IF_SM),
    
};

        static itemplate[] instrux_XORPD = new[] {
    new itemplate(OCE.XORPD, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17871, OF.IF_WILLAMETTE, OF.IF_SSE2, OF.IF_SO),
    
};

        static itemplate[] instrux_XORPS = new[] {
    new itemplate(OCE.XORPS, 2, new ulong[] {XMMREG,RM_XMM,0,0,0} ,17121, OF.IF_KATMAI, OF.IF_SSE),
    
};

        static itemplate[] instrux_XRSTOR = new[] {
    new itemplate(OCE.XRSTOR, 1, new ulong[] {MEMORY,0,0,0,0} ,17145, OF.IF_NEHALEM),
    
};

        static itemplate[] instrux_XSAVE = new[] {
    new itemplate(OCE.XSAVE, 1, new ulong[] {MEMORY,0,0,0,0} ,17139, OF.IF_NEHALEM),
    
};

        static itemplate[] instrux_XSETBV = new[] {
    new itemplate(OCE.XSETBV, 0, new ulong[] {0,0,0,0,0} ,17133, OF.IF_NEHALEM, OF.IF_PRIV),
    
};

        static itemplate[] instrux_XSHA1 = new[] {
    new itemplate(OCE.XSHA1, 0, new ulong[] {0,0,0,0,0} ,18021, OF.IF_PENT, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_XSHA256 = new[] {
    new itemplate(OCE.XSHA256, 0, new ulong[] {0,0,0,0,0} ,18027, OF.IF_PENT, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_XSTORE = new[] {
    new itemplate(OCE.XSTORE, 0, new ulong[] {0,0,0,0,0} ,21042, OF.IF_PENT, OF.IF_CYRIX),
    
};

        static itemplate[] instrux_CMOVcc = new[] {
    new itemplate(OCE.CMOVcc, 2, new ulong[] {REG16,MEMORY,0,0,0} ,9049, OF.IF_P6, OF.IF_SM),
    new itemplate(OCE.CMOVcc, 2, new ulong[] {REG16,REG16,0,0,0} ,9049, OF.IF_P6),
    new itemplate(OCE.CMOVcc, 2, new ulong[] {REG32,MEMORY,0,0,0} ,9056, OF.IF_P6, OF.IF_SM),
    new itemplate(OCE.CMOVcc, 2, new ulong[] {REG32,REG32,0,0,0} ,9056, OF.IF_P6),
    new itemplate(OCE.CMOVcc, 2, new ulong[] {REG64,MEMORY,0,0,0} ,9063, OF.IF_X64, OF.IF_SM),
    new itemplate(OCE.CMOVcc, 2, new ulong[] {REG64,REG64,0,0,0} ,9063, OF.IF_X64),
    
};

        static itemplate[] instrux_Jcc = new[] {
    new itemplate(OCE.Jcc, 1, new ulong[] {IMMEDIATE, NEAR,0,0,0,0} ,9070, OF.IF_386),
    new itemplate(OCE.Jcc, 1, new ulong[] {IMMEDIATE, BITS16, NEAR,0,0,0,0} ,9077, OF.IF_386),
    new itemplate(OCE.Jcc, 1, new ulong[] {IMMEDIATE, BITS32, NEAR,0,0,0,0} ,9084, OF.IF_386),
    new itemplate(OCE.Jcc, 1, new ulong[] {IMMEDIATE, SHORT,0,0,0,0} ,20968, OF.IF_8086),
    new itemplate(OCE.Jcc, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,20967, OF.IF_8086),
    new itemplate(OCE.Jcc, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,9085, OF.IF_386),
    new itemplate(OCE.Jcc, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,9091, OF.IF_8086),
    new itemplate(OCE.Jcc, 1, new ulong[] {IMMEDIATE,0,0,0,0} ,20968, OF.IF_8086),
    
};

        static itemplate[] instrux_SETcc = new[] {
    new itemplate(OCE.SETcc, 1, new ulong[] {MEMORY,0,0,0,0} ,16881, OF.IF_386, OF.IF_SB),
    new itemplate(OCE.SETcc, 1, new ulong[] {REG8,0,0,0,0} ,16881, OF.IF_386),
    
};

        static List<itemplate[]> inions = new List<itemplate[]>() {
    instrux_AAA,
    instrux_AAD,
    instrux_AAM,
    instrux_AAS,
    instrux_ADC,
    instrux_ADD,
    instrux_ADDPD,
    instrux_ADDPS,
    instrux_ADDSD,
    instrux_ADDSS,
    instrux_ADDSUBPD,
    instrux_ADDSUBPS,
    instrux_AESDEC,
    instrux_AESDECLAST,
    instrux_AESENC,
    instrux_AESENCLAST,
    instrux_AESIMC,
    instrux_AESKEYGENASSIST,
    instrux_AND,
    instrux_ANDNPD,
    instrux_ANDNPS,
    instrux_ANDPD,
    instrux_ANDPS,
    instrux_ARPL,
    instrux_BB0_RESET,
    instrux_BB1_RESET,
    instrux_BLENDPD,
    instrux_BLENDPS,
    instrux_BLENDVPD,
    instrux_BLENDVPS,
    instrux_BOUND,
    instrux_BSF,
    instrux_BSR,
    instrux_BSWAP,
    instrux_BT,
    instrux_BTC,
    instrux_BTR,
    instrux_BTS,
    instrux_CALL,
    instrux_CBW,
    instrux_CDQ,
    instrux_CDQE,
    instrux_CLC,
    instrux_CLD,
    instrux_CLFLUSH,
    instrux_CLGI,
    instrux_CLI,
    instrux_CLTS,
    instrux_CMC,
    instrux_CMP,
    instrux_CMPEQPD,
    instrux_CMPEQPS,
    instrux_CMPEQSD,
    instrux_CMPEQSS,
    instrux_CMPLEPD,
    instrux_CMPLEPS,
    instrux_CMPLESD,
    instrux_CMPLESS,
    instrux_CMPLTPD,
    instrux_CMPLTPS,
    instrux_CMPLTSD,
    instrux_CMPLTSS,
    instrux_CMPNEQPD,
    instrux_CMPNEQPS,
    instrux_CMPNEQSD,
    instrux_CMPNEQSS,
    instrux_CMPNLEPD,
    instrux_CMPNLEPS,
    instrux_CMPNLESD,
    instrux_CMPNLESS,
    instrux_CMPNLTPD,
    instrux_CMPNLTPS,
    instrux_CMPNLTSD,
    instrux_CMPNLTSS,
    instrux_CMPORDPD,
    instrux_CMPORDPS,
    instrux_CMPORDSD,
    instrux_CMPORDSS,
    instrux_CMPPD,
    instrux_CMPPS,
    instrux_CMPSB,
    instrux_CMPSD,
    instrux_CMPSQ,
    instrux_CMPSS,
    instrux_CMPSW,
    instrux_CMPUNORDPD,
    instrux_CMPUNORDPS,
    instrux_CMPUNORDSD,
    instrux_CMPUNORDSS,
    instrux_CMPXCHG,
    instrux_CMPXCHG16B,
    instrux_CMPXCHG486,
    instrux_CMPXCHG8B,
    instrux_COMEQPD,
    instrux_COMEQPS,
    instrux_COMEQSD,
    instrux_COMEQSS,
    instrux_COMFALSEPD,
    instrux_COMFALSEPS,
    instrux_COMFALSESD,
    instrux_COMFALSESS,
    instrux_COMISD,
    instrux_COMISS,
    instrux_COMLEPD,
    instrux_COMLEPS,
    instrux_COMLESD,
    instrux_COMLESS,
    instrux_COMLTPD,
    instrux_COMLTPS,
    instrux_COMLTSD,
    instrux_COMLTSS,
    instrux_COMNEQPD,
    instrux_COMNEQPS,
    instrux_COMNEQSD,
    instrux_COMNEQSS,
    instrux_COMNLEPD,
    instrux_COMNLEPS,
    instrux_COMNLESD,
    instrux_COMNLESS,
    instrux_COMNLTPD,
    instrux_COMNLTPS,
    instrux_COMNLTSD,
    instrux_COMNLTSS,
    instrux_COMORDPD,
    instrux_COMORDPS,
    instrux_COMORDSD,
    instrux_COMORDSS,
    instrux_COMPD,
    instrux_COMPS,
    instrux_COMSD,
    instrux_COMSS,
    instrux_COMTRUEPD,
    instrux_COMTRUEPS,
    instrux_COMTRUESD,
    instrux_COMTRUESS,
    instrux_COMUEQPD,
    instrux_COMUEQPS,
    instrux_COMUEQSD,
    instrux_COMUEQSS,
    instrux_COMULEPD,
    instrux_COMULEPS,
    instrux_COMULESD,
    instrux_COMULESS,
    instrux_COMULTPD,
    instrux_COMULTPS,
    instrux_COMULTSD,
    instrux_COMULTSS,
    instrux_COMUNEQPD,
    instrux_COMUNEQPS,
    instrux_COMUNEQSD,
    instrux_COMUNEQSS,
    instrux_COMUNLEPD,
    instrux_COMUNLEPS,
    instrux_COMUNLESD,
    instrux_COMUNLESS,
    instrux_COMUNLTPD,
    instrux_COMUNLTPS,
    instrux_COMUNLTSD,
    instrux_COMUNLTSS,
    instrux_COMUNORDPD,
    instrux_COMUNORDPS,
    instrux_COMUNORDSD,
    instrux_COMUNORDSS,
    instrux_CPUID,
    instrux_CPU_READ,
    instrux_CPU_WRITE,
    instrux_CQO,
    instrux_CRC32,
    instrux_CVTDQ2PD,
    instrux_CVTDQ2PS,
    instrux_CVTPD2DQ,
    instrux_CVTPD2PI,
    instrux_CVTPD2PS,
    instrux_CVTPH2PS,
    instrux_CVTPI2PD,
    instrux_CVTPI2PS,
    instrux_CVTPS2DQ,
    instrux_CVTPS2PD,
    instrux_CVTPS2PH,
    instrux_CVTPS2PI,
    instrux_CVTSD2SI,
    instrux_CVTSD2SS,
    instrux_CVTSI2SD,
    instrux_CVTSI2SS,
    instrux_CVTSS2SD,
    instrux_CVTSS2SI,
    instrux_CVTTPD2DQ,
    instrux_CVTTPD2PI,
    instrux_CVTTPS2DQ,
    instrux_CVTTPS2PI,
    instrux_CVTTSD2SI,
    instrux_CVTTSS2SI,
    instrux_CWD,
    instrux_CWDE,
    instrux_DAA,
    instrux_DAS,
    instrux_DB,
    instrux_DD,
    instrux_DEC,
    instrux_DIV,
    instrux_DIVPD,
    instrux_DIVPS,
    instrux_DIVSD,
    instrux_DIVSS,
    instrux_DMINT,
    instrux_DO,
    instrux_DPPD,
    instrux_DPPS,
    instrux_DQ,
    instrux_DT,
    instrux_DW,
    instrux_DY,
    instrux_EMMS,
    instrux_ENTER,
    instrux_EQU,
    instrux_EXTRACTPS,
    instrux_EXTRQ,
    instrux_F2XM1,
    instrux_FABS,
    instrux_FADD,
    instrux_FADDP,
    instrux_FBLD,
    instrux_FBSTP,
    instrux_FCHS,
    instrux_FCLEX,
    instrux_FCMOVB,
    instrux_FCMOVBE,
    instrux_FCMOVE,
    instrux_FCMOVNB,
    instrux_FCMOVNBE,
    instrux_FCMOVNE,
    instrux_FCMOVNU,
    instrux_FCMOVU,
    instrux_FCOM,
    instrux_FCOMI,
    instrux_FCOMIP,
    instrux_FCOMP,
    instrux_FCOMPP,
    instrux_FCOS,
    instrux_FDECSTP,
    instrux_FDISI,
    instrux_FDIV,
    instrux_FDIVP,
    instrux_FDIVR,
    instrux_FDIVRP,
    instrux_FEMMS,
    instrux_FENI,
    instrux_FFREE,
    instrux_FFREEP,
    instrux_FIADD,
    instrux_FICOM,
    instrux_FICOMP,
    instrux_FIDIV,
    instrux_FIDIVR,
    instrux_FILD,
    instrux_FIMUL,
    instrux_FINCSTP,
    instrux_FINIT,
    instrux_FIST,
    instrux_FISTP,
    instrux_FISTTP,
    instrux_FISUB,
    instrux_FISUBR,
    instrux_FLD,
    instrux_FLD1,
    instrux_FLDCW,
    instrux_FLDENV,
    instrux_FLDL2E,
    instrux_FLDL2T,
    instrux_FLDLG2,
    instrux_FLDLN2,
    instrux_FLDPI,
    instrux_FLDZ,
    instrux_FMADDPD,
    instrux_FMADDPS,
    instrux_FMADDSD,
    instrux_FMADDSS,
    instrux_FMSUBPD,
    instrux_FMSUBPS,
    instrux_FMSUBSD,
    instrux_FMSUBSS,
    instrux_FMUL,
    instrux_FMULP,
    instrux_FNCLEX,
    instrux_FNDISI,
    instrux_FNENI,
    instrux_FNINIT,
    instrux_FNMADDPD,
    instrux_FNMADDPS,
    instrux_FNMADDSD,
    instrux_FNMADDSS,
    instrux_FNMSUBPD,
    instrux_FNMSUBPS,
    instrux_FNMSUBSD,
    instrux_FNMSUBSS,
    instrux_FNOP,
    instrux_FNSAVE,
    instrux_FNSTCW,
    instrux_FNSTENV,
    instrux_FNSTSW,
    instrux_FPATAN,
    instrux_FPREM,
    instrux_FPREM1,
    instrux_FPTAN,
    instrux_FRCZPD,
    instrux_FRCZPS,
    instrux_FRCZSD,
    instrux_FRCZSS,
    instrux_FRNDINT,
    instrux_FRSTOR,
    instrux_FSAVE,
    instrux_FSCALE,
    instrux_FSETPM,
    instrux_FSIN,
    instrux_FSINCOS,
    instrux_FSQRT,
    instrux_FST,
    instrux_FSTCW,
    instrux_FSTENV,
    instrux_FSTP,
    instrux_FSTSW,
    instrux_FSUB,
    instrux_FSUBP,
    instrux_FSUBR,
    instrux_FSUBRP,
    instrux_FTST,
    instrux_FUCOM,
    instrux_FUCOMI,
    instrux_FUCOMIP,
    instrux_FUCOMP,
    instrux_FUCOMPP,
    instrux_FWAIT,
    instrux_FXAM,
    instrux_FXCH,
    instrux_FXRSTOR,
    instrux_FXSAVE,
    instrux_FXTRACT,
    instrux_FYL2X,
    instrux_FYL2XP1,
    instrux_GETSEC,
    instrux_HADDPD,
    instrux_HADDPS,
    instrux_HINT_NOP0,
    instrux_HINT_NOP1,
    instrux_HINT_NOP10,
    instrux_HINT_NOP11,
    instrux_HINT_NOP12,
    instrux_HINT_NOP13,
    instrux_HINT_NOP14,
    instrux_HINT_NOP15,
    instrux_HINT_NOP16,
    instrux_HINT_NOP17,
    instrux_HINT_NOP18,
    instrux_HINT_NOP19,
    instrux_HINT_NOP2,
    instrux_HINT_NOP20,
    instrux_HINT_NOP21,
    instrux_HINT_NOP22,
    instrux_HINT_NOP23,
    instrux_HINT_NOP24,
    instrux_HINT_NOP25,
    instrux_HINT_NOP26,
    instrux_HINT_NOP27,
    instrux_HINT_NOP28,
    instrux_HINT_NOP29,
    instrux_HINT_NOP3,
    instrux_HINT_NOP30,
    instrux_HINT_NOP31,
    instrux_HINT_NOP32,
    instrux_HINT_NOP33,
    instrux_HINT_NOP34,
    instrux_HINT_NOP35,
    instrux_HINT_NOP36,
    instrux_HINT_NOP37,
    instrux_HINT_NOP38,
    instrux_HINT_NOP39,
    instrux_HINT_NOP4,
    instrux_HINT_NOP40,
    instrux_HINT_NOP41,
    instrux_HINT_NOP42,
    instrux_HINT_NOP43,
    instrux_HINT_NOP44,
    instrux_HINT_NOP45,
    instrux_HINT_NOP46,
    instrux_HINT_NOP47,
    instrux_HINT_NOP48,
    instrux_HINT_NOP49,
    instrux_HINT_NOP5,
    instrux_HINT_NOP50,
    instrux_HINT_NOP51,
    instrux_HINT_NOP52,
    instrux_HINT_NOP53,
    instrux_HINT_NOP54,
    instrux_HINT_NOP55,
    instrux_HINT_NOP56,
    instrux_HINT_NOP57,
    instrux_HINT_NOP58,
    instrux_HINT_NOP59,
    instrux_HINT_NOP6,
    instrux_HINT_NOP60,
    instrux_HINT_NOP61,
    instrux_HINT_NOP62,
    instrux_HINT_NOP63,
    instrux_HINT_NOP7,
    instrux_HINT_NOP8,
    instrux_HINT_NOP9,
    instrux_HLT,
    instrux_HSUBPD,
    instrux_HSUBPS,
    instrux_IBTS,
    instrux_ICEBP,
    instrux_IDIV,
    instrux_IMUL,
    instrux_IN,
    instrux_INC,
    instrux_INCBIN,
    instrux_INSB,
    instrux_INSD,
    instrux_INSERTPS,
    instrux_INSERTQ,
    instrux_INSW,
    instrux_INT,
    instrux_INT01,
    instrux_INT03,
    instrux_INT1,
    instrux_INT3,
    instrux_INTO,
    instrux_INVD,
    instrux_INVEPT,
    instrux_INVLPG,
    instrux_INVLPGA,
    instrux_INVVPID,
    instrux_IRET,
    instrux_IRETD,
    instrux_IRETQ,
    instrux_IRETW,
    instrux_JCXZ,
    instrux_JECXZ,
    instrux_JMP,
    instrux_JMPE,
    instrux_JRCXZ,
    instrux_LAHF,
    instrux_LAR,
    instrux_LDDQU,
    instrux_LDMXCSR,
    instrux_LDS,
    instrux_LEA,
    instrux_LEAVE,
    instrux_LES,
    instrux_LFENCE,
    instrux_LFS,
    instrux_LGDT,
    instrux_LGS,
    instrux_LIDT,
    instrux_LLDT,
    instrux_LMSW,
    instrux_LOADALL,
    instrux_LOADALL286,
    instrux_LODSB,
    instrux_LODSD,
    instrux_LODSQ,
    instrux_LODSW,
    instrux_LOOP,
    instrux_LOOPE,
    instrux_LOOPNE,
    instrux_LOOPNZ,
    instrux_LOOPZ,
    instrux_LSL,
    instrux_LSS,
    instrux_LTR,
    instrux_LZCNT,
    instrux_MASKMOVDQU,
    instrux_MASKMOVQ,
    instrux_MAXPD,
    instrux_MAXPS,
    instrux_MAXSD,
    instrux_MAXSS,
    instrux_MFENCE,
    instrux_MINPD,
    instrux_MINPS,
    instrux_MINSD,
    instrux_MINSS,
    instrux_MONITOR,
    instrux_MONTMUL,
    instrux_MOV,
    instrux_MOVAPD,
    instrux_MOVAPS,
    instrux_MOVBE,
    instrux_MOVD,
    instrux_MOVDDUP,
    instrux_MOVDQ2Q,
    instrux_MOVDQA,
    instrux_MOVDQU,
    instrux_MOVHLPS,
    instrux_MOVHPD,
    instrux_MOVHPS,
    instrux_MOVLHPS,
    instrux_MOVLPD,
    instrux_MOVLPS,
    instrux_MOVMSKPD,
    instrux_MOVMSKPS,
    instrux_MOVNTDQ,
    instrux_MOVNTDQA,
    instrux_MOVNTI,
    instrux_MOVNTPD,
    instrux_MOVNTPS,
    instrux_MOVNTQ,
    instrux_MOVNTSD,
    instrux_MOVNTSS,
    instrux_MOVQ,
    instrux_MOVQ2DQ,
    instrux_MOVSB,
    instrux_MOVSD,
    instrux_MOVSHDUP,
    instrux_MOVSLDUP,
    instrux_MOVSQ,
    instrux_MOVSS,
    instrux_MOVSW,
    instrux_MOVSX,
    instrux_MOVSXD,
    instrux_MOVUPD,
    instrux_MOVUPS,
    instrux_MOVZX,
    instrux_MPSADBW,
    instrux_MUL,
    instrux_MULPD,
    instrux_MULPS,
    instrux_MULSD,
    instrux_MULSS,
    instrux_MWAIT,
    instrux_NEG,
    instrux_NOP,
    instrux_NOT,
    instrux_OR,
    instrux_ORPD,
    instrux_ORPS,
    instrux_OUT,
    instrux_OUTSB,
    instrux_OUTSD,
    instrux_OUTSW,
    instrux_PABSB,
    instrux_PABSD,
    instrux_PABSW,
    instrux_PACKSSDW,
    instrux_PACKSSWB,
    instrux_PACKUSDW,
    instrux_PACKUSWB,
    instrux_PADDB,
    instrux_PADDD,
    instrux_PADDQ,
    instrux_PADDSB,
    instrux_PADDSIW,
    instrux_PADDSW,
    instrux_PADDUSB,
    instrux_PADDUSW,
    instrux_PADDW,
    instrux_PALIGNR,
    instrux_PAND,
    instrux_PANDN,
    instrux_PAUSE,
    instrux_PAVEB,
    instrux_PAVGB,
    instrux_PAVGUSB,
    instrux_PAVGW,
    instrux_PBLENDVB,
    instrux_PBLENDW,
    instrux_PCLMULHQHQDQ,
    instrux_PCLMULHQLQDQ,
    instrux_PCLMULLQHQDQ,
    instrux_PCLMULLQLQDQ,
    instrux_PCLMULQDQ,
    instrux_PCMOV,
    instrux_PCMPEQB,
    instrux_PCMPEQD,
    instrux_PCMPEQQ,
    instrux_PCMPEQW,
    instrux_PCMPESTRI,
    instrux_PCMPESTRM,
    instrux_PCMPGTB,
    instrux_PCMPGTD,
    instrux_PCMPGTQ,
    instrux_PCMPGTW,
    instrux_PCMPISTRI,
    instrux_PCMPISTRM,
    instrux_PCOMB,
    instrux_PCOMD,
    instrux_PCOMEQB,
    instrux_PCOMEQD,
    instrux_PCOMEQQ,
    instrux_PCOMEQUB,
    instrux_PCOMEQUD,
    instrux_PCOMEQUQ,
    instrux_PCOMEQUW,
    instrux_PCOMEQW,
    instrux_PCOMFALSEB,
    instrux_PCOMFALSED,
    instrux_PCOMFALSEQ,
    instrux_PCOMFALSEUB,
    instrux_PCOMFALSEUD,
    instrux_PCOMFALSEUQ,
    instrux_PCOMFALSEUW,
    instrux_PCOMFALSEW,
    instrux_PCOMGEB,
    instrux_PCOMGED,
    instrux_PCOMGEQ,
    instrux_PCOMGEUB,
    instrux_PCOMGEUD,
    instrux_PCOMGEUQ,
    instrux_PCOMGEUW,
    instrux_PCOMGEW,
    instrux_PCOMGTB,
    instrux_PCOMGTD,
    instrux_PCOMGTQ,
    instrux_PCOMGTUB,
    instrux_PCOMGTUD,
    instrux_PCOMGTUQ,
    instrux_PCOMGTUW,
    instrux_PCOMGTW,
    instrux_PCOMLEB,
    instrux_PCOMLED,
    instrux_PCOMLEQ,
    instrux_PCOMLEUB,
    instrux_PCOMLEUD,
    instrux_PCOMLEUQ,
    instrux_PCOMLEUW,
    instrux_PCOMLEW,
    instrux_PCOMLTB,
    instrux_PCOMLTD,
    instrux_PCOMLTQ,
    instrux_PCOMLTUB,
    instrux_PCOMLTUD,
    instrux_PCOMLTUQ,
    instrux_PCOMLTUW,
    instrux_PCOMLTW,
    instrux_PCOMNEQB,
    instrux_PCOMNEQD,
    instrux_PCOMNEQQ,
    instrux_PCOMNEQUB,
    instrux_PCOMNEQUD,
    instrux_PCOMNEQUQ,
    instrux_PCOMNEQUW,
    instrux_PCOMNEQW,
    instrux_PCOMQ,
    instrux_PCOMTRUEB,
    instrux_PCOMTRUED,
    instrux_PCOMTRUEQ,
    instrux_PCOMTRUEUB,
    instrux_PCOMTRUEUD,
    instrux_PCOMTRUEUQ,
    instrux_PCOMTRUEUW,
    instrux_PCOMTRUEW,
    instrux_PCOMUB,
    instrux_PCOMUD,
    instrux_PCOMUQ,
    instrux_PCOMUW,
    instrux_PCOMW,
    instrux_PDISTIB,
    instrux_PERMPD,
    instrux_PERMPS,
    instrux_PEXTRB,
    instrux_PEXTRD,
    instrux_PEXTRQ,
    instrux_PEXTRW,
    instrux_PF2ID,
    instrux_PF2IW,
    instrux_PFACC,
    instrux_PFADD,
    instrux_PFCMPEQ,
    instrux_PFCMPGE,
    instrux_PFCMPGT,
    instrux_PFMAX,
    instrux_PFMIN,
    instrux_PFMUL,
    instrux_PFNACC,
    instrux_PFPNACC,
    instrux_PFRCP,
    instrux_PFRCPIT1,
    instrux_PFRCPIT2,
    instrux_PFRCPV,
    instrux_PFRSQIT1,
    instrux_PFRSQRT,
    instrux_PFRSQRTV,
    instrux_PFSUB,
    instrux_PFSUBR,
    instrux_PHADDBD,
    instrux_PHADDBQ,
    instrux_PHADDBW,
    instrux_PHADDD,
    instrux_PHADDDQ,
    instrux_PHADDSW,
    instrux_PHADDUBD,
    instrux_PHADDUBQ,
    instrux_PHADDUBW,
    instrux_PHADDUDQ,
    instrux_PHADDUWD,
    instrux_PHADDUWQ,
    instrux_PHADDW,
    instrux_PHADDWD,
    instrux_PHADDWQ,
    instrux_PHMINPOSUW,
    instrux_PHSUBBW,
    instrux_PHSUBD,
    instrux_PHSUBDQ,
    instrux_PHSUBSW,
    instrux_PHSUBW,
    instrux_PHSUBWD,
    instrux_PI2FD,
    instrux_PI2FW,
    instrux_PINSRB,
    instrux_PINSRD,
    instrux_PINSRQ,
    instrux_PINSRW,
    instrux_PMACHRIW,
    instrux_PMACSDD,
    instrux_PMACSDQH,
    instrux_PMACSDQL,
    instrux_PMACSSDD,
    instrux_PMACSSDQH,
    instrux_PMACSSDQL,
    instrux_PMACSSWD,
    instrux_PMACSSWW,
    instrux_PMACSWD,
    instrux_PMACSWW,
    instrux_PMADCSSWD,
    instrux_PMADCSWD,
    instrux_PMADDUBSW,
    instrux_PMADDWD,
    instrux_PMAGW,
    instrux_PMAXSB,
    instrux_PMAXSD,
    instrux_PMAXSW,
    instrux_PMAXUB,
    instrux_PMAXUD,
    instrux_PMAXUW,
    instrux_PMINSB,
    instrux_PMINSD,
    instrux_PMINSW,
    instrux_PMINUB,
    instrux_PMINUD,
    instrux_PMINUW,
    instrux_PMOVMSKB,
    instrux_PMOVSXBD,
    instrux_PMOVSXBQ,
    instrux_PMOVSXBW,
    instrux_PMOVSXDQ,
    instrux_PMOVSXWD,
    instrux_PMOVSXWQ,
    instrux_PMOVZXBD,
    instrux_PMOVZXBQ,
    instrux_PMOVZXBW,
    instrux_PMOVZXDQ,
    instrux_PMOVZXWD,
    instrux_PMOVZXWQ,
    instrux_PMULDQ,
    instrux_PMULHRIW,
    instrux_PMULHRSW,
    instrux_PMULHRWA,
    instrux_PMULHRWC,
    instrux_PMULHUW,
    instrux_PMULHW,
    instrux_PMULLD,
    instrux_PMULLW,
    instrux_PMULUDQ,
    instrux_PMVGEZB,
    instrux_PMVLZB,
    instrux_PMVNZB,
    instrux_PMVZB,
    instrux_POP,
    instrux_POPA,
    instrux_POPAD,
    instrux_POPAW,
    instrux_POPCNT,
    instrux_POPF,
    instrux_POPFD,
    instrux_POPFQ,
    instrux_POPFW,
    instrux_POR,
    instrux_PPERM,
    instrux_PREFETCH,
    instrux_PREFETCHNTA,
    instrux_PREFETCHT0,
    instrux_PREFETCHT1,
    instrux_PREFETCHT2,
    instrux_PREFETCHW,
    instrux_PROTB,
    instrux_PROTD,
    instrux_PROTQ,
    instrux_PROTW,
    instrux_PSADBW,
    instrux_PSHAB,
    instrux_PSHAD,
    instrux_PSHAQ,
    instrux_PSHAW,
    instrux_PSHLB,
    instrux_PSHLD,
    instrux_PSHLQ,
    instrux_PSHLW,
    instrux_PSHUFB,
    instrux_PSHUFD,
    instrux_PSHUFHW,
    instrux_PSHUFLW,
    instrux_PSHUFW,
    instrux_PSIGNB,
    instrux_PSIGND,
    instrux_PSIGNW,
    instrux_PSLLD,
    instrux_PSLLDQ,
    instrux_PSLLQ,
    instrux_PSLLW,
    instrux_PSRAD,
    instrux_PSRAW,
    instrux_PSRLD,
    instrux_PSRLDQ,
    instrux_PSRLQ,
    instrux_PSRLW,
    instrux_PSUBB,
    instrux_PSUBD,
    instrux_PSUBQ,
    instrux_PSUBSB,
    instrux_PSUBSIW,
    instrux_PSUBSW,
    instrux_PSUBUSB,
    instrux_PSUBUSW,
    instrux_PSUBW,
    instrux_PSWAPD,
    instrux_PTEST,
    instrux_PUNPCKHBW,
    instrux_PUNPCKHDQ,
    instrux_PUNPCKHQDQ,
    instrux_PUNPCKHWD,
    instrux_PUNPCKLBW,
    instrux_PUNPCKLDQ,
    instrux_PUNPCKLQDQ,
    instrux_PUNPCKLWD,
    instrux_PUSH,
    instrux_PUSHA,
    instrux_PUSHAD,
    instrux_PUSHAW,
    instrux_PUSHF,
    instrux_PUSHFD,
    instrux_PUSHFQ,
    instrux_PUSHFW,
    instrux_PXOR,
    instrux_RCL,
    instrux_RCPPS,
    instrux_RCPSS,
    instrux_RCR,
    instrux_RDM,
    instrux_RDMSR,
    instrux_RDPMC,
    instrux_RDSHR,
    instrux_RDTSC,
    instrux_RDTSCP,
    instrux_RESB,
    instrux_RESD,
    instrux_RESO,
    instrux_RESQ,
    instrux_REST,
    instrux_RESW,
    instrux_RESY,
    instrux_RET,
    instrux_RETF,
    instrux_RETN,
    instrux_ROL,
    instrux_ROR,
    instrux_ROUNDPD,
    instrux_ROUNDPS,
    instrux_ROUNDSD,
    instrux_ROUNDSS,
    instrux_RSDC,
    instrux_RSLDT,
    instrux_RSM,
    instrux_RSQRTPS,
    instrux_RSQRTSS,
    instrux_RSTS,
    instrux_SAHF,
    instrux_SAL,
    instrux_SALC,
    instrux_SAR,
    instrux_SBB,
    instrux_SCASB,
    instrux_SCASD,
    instrux_SCASQ,
    instrux_SCASW,
    instrux_SFENCE,
    instrux_SGDT,
    instrux_SHL,
    instrux_SHLD,
    instrux_SHR,
    instrux_SHRD,
    instrux_SHUFPD,
    instrux_SHUFPS,
    instrux_SIDT,
    instrux_SKINIT,
    instrux_SLDT,
    instrux_SMI,
    instrux_SMINT,
    instrux_SMINTOLD,
    instrux_SMSW,
    instrux_SQRTPD,
    instrux_SQRTPS,
    instrux_SQRTSD,
    instrux_SQRTSS,
    instrux_STC,
    instrux_STD,
    instrux_STGI,
    instrux_STI,
    instrux_STMXCSR,
    instrux_STOSB,
    instrux_STOSD,
    instrux_STOSQ,
    instrux_STOSW,
    instrux_STR,
    instrux_SUB,
    instrux_SUBPD,
    instrux_SUBPS,
    instrux_SUBSD,
    instrux_SUBSS,
    instrux_SVDC,
    instrux_SVLDT,
    instrux_SVTS,
    instrux_SWAPGS,
    instrux_SYSCALL,
    instrux_SYSENTER,
    instrux_SYSEXIT,
    instrux_SYSRET,
    instrux_TEST,
    instrux_UCOMISD,
    instrux_UCOMISS,
    instrux_UD0,
    instrux_UD1,
    instrux_UD2,
    instrux_UD2A,
    instrux_UD2B,
    instrux_UMOV,
    instrux_UNPCKHPD,
    instrux_UNPCKHPS,
    instrux_UNPCKLPD,
    instrux_UNPCKLPS,
    instrux_VADDPD,
    instrux_VADDPS,
    instrux_VADDSD,
    instrux_VADDSS,
    instrux_VADDSUBPD,
    instrux_VADDSUBPS,
    instrux_VAESDEC,
    instrux_VAESDECLAST,
    instrux_VAESENC,
    instrux_VAESENCLAST,
    instrux_VAESIMC,
    instrux_VAESKEYGENASSIST,
    instrux_VANDNPD,
    instrux_VANDNPS,
    instrux_VANDPD,
    instrux_VANDPS,
    instrux_VBLENDPD,
    instrux_VBLENDPS,
    instrux_VBLENDVPD,
    instrux_VBLENDVPS,
    instrux_VBROADCASTF128,
    instrux_VBROADCASTSD,
    instrux_VBROADCASTSS,
    instrux_VCMPEQPD,
    instrux_VCMPEQPS,
    instrux_VCMPEQSD,
    instrux_VCMPEQSS,
    instrux_VCMPEQ_OSPD,
    instrux_VCMPEQ_OSPS,
    instrux_VCMPEQ_OSSD,
    instrux_VCMPEQ_OSSS,
    instrux_VCMPEQ_UQPD,
    instrux_VCMPEQ_UQPS,
    instrux_VCMPEQ_UQSD,
    instrux_VCMPEQ_UQSS,
    instrux_VCMPEQ_USPD,
    instrux_VCMPEQ_USPS,
    instrux_VCMPEQ_USSD,
    instrux_VCMPEQ_USSS,
    instrux_VCMPFALSEPD,
    instrux_VCMPFALSEPS,
    instrux_VCMPFALSESD,
    instrux_VCMPFALSESS,
    instrux_VCMPFALSE_OSPD,
    instrux_VCMPFALSE_OSPS,
    instrux_VCMPFALSE_OSSD,
    instrux_VCMPFALSE_OSSS,
    instrux_VCMPGEPD,
    instrux_VCMPGEPS,
    instrux_VCMPGESD,
    instrux_VCMPGESS,
    instrux_VCMPGE_OQPD,
    instrux_VCMPGE_OQPS,
    instrux_VCMPGE_OQSD,
    instrux_VCMPGE_OQSS,
    instrux_VCMPGTPD,
    instrux_VCMPGTPS,
    instrux_VCMPGTSD,
    instrux_VCMPGTSS,
    instrux_VCMPGT_OQPD,
    instrux_VCMPGT_OQPS,
    instrux_VCMPGT_OQSD,
    instrux_VCMPGT_OQSS,
    instrux_VCMPLEPD,
    instrux_VCMPLEPS,
    instrux_VCMPLESD,
    instrux_VCMPLESS,
    instrux_VCMPLE_OQPD,
    instrux_VCMPLE_OQPS,
    instrux_VCMPLE_OQSD,
    instrux_VCMPLE_OQSS,
    instrux_VCMPLTPD,
    instrux_VCMPLTPS,
    instrux_VCMPLTSD,
    instrux_VCMPLTSS,
    instrux_VCMPLT_OQPD,
    instrux_VCMPLT_OQPS,
    instrux_VCMPLT_OQSD,
    instrux_VCMPLT_OQSS,
    instrux_VCMPNEQPD,
    instrux_VCMPNEQPS,
    instrux_VCMPNEQSD,
    instrux_VCMPNEQSS,
    instrux_VCMPNEQ_OQPD,
    instrux_VCMPNEQ_OQPS,
    instrux_VCMPNEQ_OQSD,
    instrux_VCMPNEQ_OQSS,
    instrux_VCMPNEQ_OSPD,
    instrux_VCMPNEQ_OSPS,
    instrux_VCMPNEQ_OSSD,
    instrux_VCMPNEQ_OSSS,
    instrux_VCMPNEQ_USPD,
    instrux_VCMPNEQ_USPS,
    instrux_VCMPNEQ_USSD,
    instrux_VCMPNEQ_USSS,
    instrux_VCMPNGEPD,
    instrux_VCMPNGEPS,
    instrux_VCMPNGESD,
    instrux_VCMPNGESS,
    instrux_VCMPNGE_UQPD,
    instrux_VCMPNGE_UQPS,
    instrux_VCMPNGE_UQSD,
    instrux_VCMPNGE_UQSS,
    instrux_VCMPNGTPD,
    instrux_VCMPNGTPS,
    instrux_VCMPNGTSD,
    instrux_VCMPNGTSS,
    instrux_VCMPNGT_UQPD,
    instrux_VCMPNGT_UQPS,
    instrux_VCMPNGT_UQSD,
    instrux_VCMPNGT_UQSS,
    instrux_VCMPNLEPD,
    instrux_VCMPNLEPS,
    instrux_VCMPNLESD,
    instrux_VCMPNLESS,
    instrux_VCMPNLE_UQPD,
    instrux_VCMPNLE_UQPS,
    instrux_VCMPNLE_UQSD,
    instrux_VCMPNLE_UQSS,
    instrux_VCMPNLTPD,
    instrux_VCMPNLTPS,
    instrux_VCMPNLTSD,
    instrux_VCMPNLTSS,
    instrux_VCMPNLT_UQPD,
    instrux_VCMPNLT_UQPS,
    instrux_VCMPNLT_UQSD,
    instrux_VCMPNLT_UQSS,
    instrux_VCMPORDPD,
    instrux_VCMPORDPS,
    instrux_VCMPORDSD,
    instrux_VCMPORDSS,
    instrux_VCMPORD_SPD,
    instrux_VCMPORD_SPS,
    instrux_VCMPORD_SSD,
    instrux_VCMPORD_SSS,
    instrux_VCMPPD,
    instrux_VCMPPS,
    instrux_VCMPSD,
    instrux_VCMPSS,
    instrux_VCMPTRUEPD,
    instrux_VCMPTRUEPS,
    instrux_VCMPTRUESD,
    instrux_VCMPTRUESS,
    instrux_VCMPTRUE_USPD,
    instrux_VCMPTRUE_USPS,
    instrux_VCMPTRUE_USSD,
    instrux_VCMPTRUE_USSS,
    instrux_VCMPUNORDPD,
    instrux_VCMPUNORDPS,
    instrux_VCMPUNORDSD,
    instrux_VCMPUNORDSS,
    instrux_VCMPUNORD_SPD,
    instrux_VCMPUNORD_SPS,
    instrux_VCMPUNORD_SSD,
    instrux_VCMPUNORD_SSS,
    instrux_VCOMISD,
    instrux_VCOMISS,
    instrux_VCVTDQ2PD,
    instrux_VCVTDQ2PS,
    instrux_VCVTPD2DQ,
    instrux_VCVTPD2PS,
    instrux_VCVTPH2PS,
    instrux_VCVTPS2DQ,
    instrux_VCVTPS2PD,
    instrux_VCVTPS2PH,
    instrux_VCVTSD2SI,
    instrux_VCVTSD2SS,
    instrux_VCVTSI2SD,
    instrux_VCVTSI2SS,
    instrux_VCVTSS2SD,
    instrux_VCVTSS2SI,
    instrux_VCVTTPD2DQ,
    instrux_VCVTTPS2DQ,
    instrux_VCVTTSD2SI,
    instrux_VCVTTSS2SI,
    instrux_VDIVPD,
    instrux_VDIVPS,
    instrux_VDIVSD,
    instrux_VDIVSS,
    instrux_VDPPD,
    instrux_VDPPS,
    instrux_VERR,
    instrux_VERW,
    instrux_VEXTRACTF128,
    instrux_VEXTRACTPS,
    instrux_VFMADD123PD,
    instrux_VFMADD123PS,
    instrux_VFMADD123SD,
    instrux_VFMADD123SS,
    instrux_VFMADD132PD,
    instrux_VFMADD132PS,
    instrux_VFMADD132SD,
    instrux_VFMADD132SS,
    instrux_VFMADD213PD,
    instrux_VFMADD213PS,
    instrux_VFMADD213SD,
    instrux_VFMADD213SS,
    instrux_VFMADD231PD,
    instrux_VFMADD231PS,
    instrux_VFMADD231SD,
    instrux_VFMADD231SS,
    instrux_VFMADD312PD,
    instrux_VFMADD312PS,
    instrux_VFMADD312SD,
    instrux_VFMADD312SS,
    instrux_VFMADD321PD,
    instrux_VFMADD321PS,
    instrux_VFMADD321SD,
    instrux_VFMADD321SS,
    instrux_VFMADDPD,
    instrux_VFMADDPS,
    instrux_VFMADDSD,
    instrux_VFMADDSS,
    instrux_VFMADDSUB123PD,
    instrux_VFMADDSUB123PS,
    instrux_VFMADDSUB132PD,
    instrux_VFMADDSUB132PS,
    instrux_VFMADDSUB213PD,
    instrux_VFMADDSUB213PS,
    instrux_VFMADDSUB231PD,
    instrux_VFMADDSUB231PS,
    instrux_VFMADDSUB312PD,
    instrux_VFMADDSUB312PS,
    instrux_VFMADDSUB321PD,
    instrux_VFMADDSUB321PS,
    instrux_VFMADDSUBPD,
    instrux_VFMADDSUBPS,
    instrux_VFMSUB123PD,
    instrux_VFMSUB123PS,
    instrux_VFMSUB123SD,
    instrux_VFMSUB123SS,
    instrux_VFMSUB132PD,
    instrux_VFMSUB132PS,
    instrux_VFMSUB132SD,
    instrux_VFMSUB132SS,
    instrux_VFMSUB213PD,
    instrux_VFMSUB213PS,
    instrux_VFMSUB213SD,
    instrux_VFMSUB213SS,
    instrux_VFMSUB231PD,
    instrux_VFMSUB231PS,
    instrux_VFMSUB231SD,
    instrux_VFMSUB231SS,
    instrux_VFMSUB312PD,
    instrux_VFMSUB312PS,
    instrux_VFMSUB312SD,
    instrux_VFMSUB312SS,
    instrux_VFMSUB321PD,
    instrux_VFMSUB321PS,
    instrux_VFMSUB321SD,
    instrux_VFMSUB321SS,
    instrux_VFMSUBADD123PD,
    instrux_VFMSUBADD123PS,
    instrux_VFMSUBADD132PD,
    instrux_VFMSUBADD132PS,
    instrux_VFMSUBADD213PD,
    instrux_VFMSUBADD213PS,
    instrux_VFMSUBADD231PD,
    instrux_VFMSUBADD231PS,
    instrux_VFMSUBADD312PD,
    instrux_VFMSUBADD312PS,
    instrux_VFMSUBADD321PD,
    instrux_VFMSUBADD321PS,
    instrux_VFMSUBADDPD,
    instrux_VFMSUBADDPS,
    instrux_VFMSUBPD,
    instrux_VFMSUBPS,
    instrux_VFMSUBSD,
    instrux_VFMSUBSS,
    instrux_VFNMADD123PD,
    instrux_VFNMADD123PS,
    instrux_VFNMADD123SD,
    instrux_VFNMADD123SS,
    instrux_VFNMADD132PD,
    instrux_VFNMADD132PS,
    instrux_VFNMADD132SD,
    instrux_VFNMADD132SS,
    instrux_VFNMADD213PD,
    instrux_VFNMADD213PS,
    instrux_VFNMADD213SD,
    instrux_VFNMADD213SS,
    instrux_VFNMADD231PD,
    instrux_VFNMADD231PS,
    instrux_VFNMADD231SD,
    instrux_VFNMADD231SS,
    instrux_VFNMADD312PD,
    instrux_VFNMADD312PS,
    instrux_VFNMADD312SD,
    instrux_VFNMADD312SS,
    instrux_VFNMADD321PD,
    instrux_VFNMADD321PS,
    instrux_VFNMADD321SD,
    instrux_VFNMADD321SS,
    instrux_VFNMADDPD,
    instrux_VFNMADDPS,
    instrux_VFNMADDSD,
    instrux_VFNMADDSS,
    instrux_VFNMSUB123PD,
    instrux_VFNMSUB123PS,
    instrux_VFNMSUB123SD,
    instrux_VFNMSUB123SS,
    instrux_VFNMSUB132PD,
    instrux_VFNMSUB132PS,
    instrux_VFNMSUB132SD,
    instrux_VFNMSUB132SS,
    instrux_VFNMSUB213PD,
    instrux_VFNMSUB213PS,
    instrux_VFNMSUB213SD,
    instrux_VFNMSUB213SS,
    instrux_VFNMSUB231PD,
    instrux_VFNMSUB231PS,
    instrux_VFNMSUB231SD,
    instrux_VFNMSUB231SS,
    instrux_VFNMSUB312PD,
    instrux_VFNMSUB312PS,
    instrux_VFNMSUB312SD,
    instrux_VFNMSUB312SS,
    instrux_VFNMSUB321PD,
    instrux_VFNMSUB321PS,
    instrux_VFNMSUB321SD,
    instrux_VFNMSUB321SS,
    instrux_VFNMSUBPD,
    instrux_VFNMSUBPS,
    instrux_VFNMSUBSD,
    instrux_VFNMSUBSS,
    instrux_VFRCZPD,
    instrux_VFRCZPS,
    instrux_VFRCZSD,
    instrux_VFRCZSS,
    instrux_VHADDPD,
    instrux_VHADDPS,
    instrux_VHSUBPD,
    instrux_VHSUBPS,
    instrux_VINSERTF128,
    instrux_VINSERTPS,
    instrux_VLDDQU,
    instrux_VLDMXCSR,
    instrux_VLDQQU,
    instrux_VMASKMOVDQU,
    instrux_VMASKMOVPD,
    instrux_VMASKMOVPS,
    instrux_VMAXPD,
    instrux_VMAXPS,
    instrux_VMAXSD,
    instrux_VMAXSS,
    instrux_VMCALL,
    instrux_VMCLEAR,
    instrux_VMINPD,
    instrux_VMINPS,
    instrux_VMINSD,
    instrux_VMINSS,
    instrux_VMLAUNCH,
    instrux_VMLOAD,
    instrux_VMMCALL,
    instrux_VMOVAPD,
    instrux_VMOVAPS,
    instrux_VMOVD,
    instrux_VMOVDDUP,
    instrux_VMOVDQA,
    instrux_VMOVDQU,
    instrux_VMOVHLPS,
    instrux_VMOVHPD,
    instrux_VMOVHPS,
    instrux_VMOVLHPS,
    instrux_VMOVLPD,
    instrux_VMOVLPS,
    instrux_VMOVMSKPD,
    instrux_VMOVMSKPS,
    instrux_VMOVNTDQ,
    instrux_VMOVNTDQA,
    instrux_VMOVNTPD,
    instrux_VMOVNTPS,
    instrux_VMOVNTQQ,
    instrux_VMOVQ,
    instrux_VMOVQQA,
    instrux_VMOVQQU,
    instrux_VMOVSD,
    instrux_VMOVSHDUP,
    instrux_VMOVSLDUP,
    instrux_VMOVSS,
    instrux_VMOVUPD,
    instrux_VMOVUPS,
    instrux_VMPSADBW,
    instrux_VMPTRLD,
    instrux_VMPTRST,
    instrux_VMREAD,
    instrux_VMRESUME,
    instrux_VMRUN,
    instrux_VMSAVE,
    instrux_VMULPD,
    instrux_VMULPS,
    instrux_VMULSD,
    instrux_VMULSS,
    instrux_VMWRITE,
    instrux_VMXOFF,
    instrux_VMXON,
    instrux_VORPD,
    instrux_VORPS,
    instrux_VPABSB,
    instrux_VPABSD,
    instrux_VPABSW,
    instrux_VPACKSSDW,
    instrux_VPACKSSWB,
    instrux_VPACKUSDW,
    instrux_VPACKUSWB,
    instrux_VPADDB,
    instrux_VPADDD,
    instrux_VPADDQ,
    instrux_VPADDSB,
    instrux_VPADDSW,
    instrux_VPADDUSB,
    instrux_VPADDUSW,
    instrux_VPADDW,
    instrux_VPALIGNR,
    instrux_VPAND,
    instrux_VPANDN,
    instrux_VPAVGB,
    instrux_VPAVGW,
    instrux_VPBLENDVB,
    instrux_VPBLENDW,
    instrux_VPCLMULHQHQDQ,
    instrux_VPCLMULHQLQDQ,
    instrux_VPCLMULLQHQDQ,
    instrux_VPCLMULLQLQDQ,
    instrux_VPCLMULQDQ,
    instrux_VPCMOV,
    instrux_VPCMPEQB,
    instrux_VPCMPEQD,
    instrux_VPCMPEQQ,
    instrux_VPCMPEQW,
    instrux_VPCMPESTRI,
    instrux_VPCMPESTRM,
    instrux_VPCMPGTB,
    instrux_VPCMPGTD,
    instrux_VPCMPGTQ,
    instrux_VPCMPGTW,
    instrux_VPCMPISTRI,
    instrux_VPCMPISTRM,
    instrux_VPCOMB,
    instrux_VPCOMD,
    instrux_VPCOMQ,
    instrux_VPCOMUB,
    instrux_VPCOMUD,
    instrux_VPCOMUQ,
    instrux_VPCOMUW,
    instrux_VPCOMW,
    instrux_VPERM2F128,
    instrux_VPERMIL2PD,
    instrux_VPERMIL2PS,
    instrux_VPERMILMO2PD,
    instrux_VPERMILMO2PS,
    instrux_VPERMILMZ2PD,
    instrux_VPERMILMZ2PS,
    instrux_VPERMILPD,
    instrux_VPERMILPS,
    instrux_VPERMILTD2PD,
    instrux_VPERMILTD2PS,
    instrux_VPEXTRB,
    instrux_VPEXTRD,
    instrux_VPEXTRQ,
    instrux_VPEXTRW,
    instrux_VPHADDBD,
    instrux_VPHADDBQ,
    instrux_VPHADDBW,
    instrux_VPHADDD,
    instrux_VPHADDDQ,
    instrux_VPHADDSW,
    instrux_VPHADDUBD,
    instrux_VPHADDUBQ,
    instrux_VPHADDUBWD,
    instrux_VPHADDUDQ,
    instrux_VPHADDUWD,
    instrux_VPHADDUWQ,
    instrux_VPHADDW,
    instrux_VPHADDWD,
    instrux_VPHADDWQ,
    instrux_VPHMINPOSUW,
    instrux_VPHSUBBW,
    instrux_VPHSUBD,
    instrux_VPHSUBDQ,
    instrux_VPHSUBSW,
    instrux_VPHSUBW,
    instrux_VPHSUBWD,
    instrux_VPINSRB,
    instrux_VPINSRD,
    instrux_VPINSRQ,
    instrux_VPINSRW,
    instrux_VPMACSDD,
    instrux_VPMACSDQH,
    instrux_VPMACSDQL,
    instrux_VPMACSSDD,
    instrux_VPMACSSDQH,
    instrux_VPMACSSDQL,
    instrux_VPMACSSWD,
    instrux_VPMACSSWW,
    instrux_VPMACSWD,
    instrux_VPMACSWW,
    instrux_VPMADCSSWD,
    instrux_VPMADCSWD,
    instrux_VPMADDUBSW,
    instrux_VPMADDWD,
    instrux_VPMAXSB,
    instrux_VPMAXSD,
    instrux_VPMAXSW,
    instrux_VPMAXUB,
    instrux_VPMAXUD,
    instrux_VPMAXUW,
    instrux_VPMINSB,
    instrux_VPMINSD,
    instrux_VPMINSW,
    instrux_VPMINUB,
    instrux_VPMINUD,
    instrux_VPMINUW,
    instrux_VPMOVMSKB,
    instrux_VPMOVSXBD,
    instrux_VPMOVSXBQ,
    instrux_VPMOVSXBW,
    instrux_VPMOVSXDQ,
    instrux_VPMOVSXWD,
    instrux_VPMOVSXWQ,
    instrux_VPMOVZXBD,
    instrux_VPMOVZXBQ,
    instrux_VPMOVZXBW,
    instrux_VPMOVZXDQ,
    instrux_VPMOVZXWD,
    instrux_VPMOVZXWQ,
    instrux_VPMULDQ,
    instrux_VPMULHRSW,
    instrux_VPMULHUW,
    instrux_VPMULHW,
    instrux_VPMULLD,
    instrux_VPMULLW,
    instrux_VPMULUDQ,
    instrux_VPOR,
    instrux_VPPERM,
    instrux_VPROTB,
    instrux_VPROTD,
    instrux_VPROTQ,
    instrux_VPROTW,
    instrux_VPSADBW,
    instrux_VPSHAB,
    instrux_VPSHAD,
    instrux_VPSHAQ,
    instrux_VPSHAW,
    instrux_VPSHLB,
    instrux_VPSHLD,
    instrux_VPSHLQ,
    instrux_VPSHLW,
    instrux_VPSHUFB,
    instrux_VPSHUFD,
    instrux_VPSHUFHW,
    instrux_VPSHUFLW,
    instrux_VPSIGNB,
    instrux_VPSIGND,
    instrux_VPSIGNW,
    instrux_VPSLLD,
    instrux_VPSLLDQ,
    instrux_VPSLLQ,
    instrux_VPSLLW,
    instrux_VPSRAD,
    instrux_VPSRAW,
    instrux_VPSRLD,
    instrux_VPSRLDQ,
    instrux_VPSRLQ,
    instrux_VPSRLW,
    instrux_VPSUBB,
    instrux_VPSUBD,
    instrux_VPSUBQ,
    instrux_VPSUBSB,
    instrux_VPSUBSW,
    instrux_VPSUBUSB,
    instrux_VPSUBUSW,
    instrux_VPSUBW,
    instrux_VPTEST,
    instrux_VPUNPCKHBW,
    instrux_VPUNPCKHDQ,
    instrux_VPUNPCKHQDQ,
    instrux_VPUNPCKHWD,
    instrux_VPUNPCKLBW,
    instrux_VPUNPCKLDQ,
    instrux_VPUNPCKLQDQ,
    instrux_VPUNPCKLWD,
    instrux_VPXOR,
    instrux_VRCPPS,
    instrux_VRCPSS,
    instrux_VROUNDPD,
    instrux_VROUNDPS,
    instrux_VROUNDSD,
    instrux_VROUNDSS,
    instrux_VRSQRTPS,
    instrux_VRSQRTSS,
    instrux_VSHUFPD,
    instrux_VSHUFPS,
    instrux_VSQRTPD,
    instrux_VSQRTPS,
    instrux_VSQRTSD,
    instrux_VSQRTSS,
    instrux_VSTMXCSR,
    instrux_VSUBPD,
    instrux_VSUBPS,
    instrux_VSUBSD,
    instrux_VSUBSS,
    instrux_VTESTPD,
    instrux_VTESTPS,
    instrux_VUCOMISD,
    instrux_VUCOMISS,
    instrux_VUNPCKHPD,
    instrux_VUNPCKHPS,
    instrux_VUNPCKLPD,
    instrux_VUNPCKLPS,
    instrux_VXORPD,
    instrux_VXORPS,
    instrux_VZEROALL,
    instrux_VZEROUPPER,
    instrux_WBINVD,
    instrux_WRMSR,
    instrux_WRSHR,
    instrux_XADD,
    instrux_XBTS,
    instrux_XCHG,
    instrux_XCRYPTCBC,
    instrux_XCRYPTCFB,
    instrux_XCRYPTCTR,
    instrux_XCRYPTECB,
    instrux_XCRYPTOFB,
    instrux_XGETBV,
    instrux_XLAT,
    instrux_XLATB,
    instrux_XOR,
    instrux_XORPD,
    instrux_XORPS,
    instrux_XRSTOR,
    instrux_XSAVE,
    instrux_XSETBV,
    instrux_XSHA1,
    instrux_XSHA256,
    instrux_XSTORE,
    instrux_CMOVcc,
    instrux_Jcc,
    instrux_SETcc,
};
    }
}
