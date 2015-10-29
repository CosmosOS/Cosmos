using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.X86
{
    /// <summary>
    /// 
    /// </summary>
    public enum InstructionSet
    {
        /// <summary>
        /// 8086 to 486 instructions. No special instruction set
        /// </summary>
        LEGACY,
        /// <summary>
        /// FPU instruction set
        /// </summary>
        FPU,
        /// <summary>
        /// MMX Technology
        /// </summary>
        MMX,
        /// <summary>
        /// AMD 3DNow! instruction set
        /// </summary>
        AMD_3DNOW,
        /// <summary>
        /// Streaming SIMD Extensions 1
        /// </summary>
        SSE1,
        /// <summary>
        /// Streaming SIMD Extensions 2
        /// </summary>
        SSE2,
        /// <summary>
        /// Streaming SIMD Extensions 3
        /// </summary>
        SSE3,
        /// <summary>
        /// Supplemental Streaming SIMD Extensions 3
        /// </summary>
        SSSE3,
        /// <summary>
        /// Streaming SIMD Extensions 4A (AMD)
        /// </summary>
        SSE4A,
        /// <summary>
        /// Streaming SIMD Extensions 4.1
        /// </summary>
        SSE41,
        /// <summary>
        /// Streaming SIMD Extensions 4.2
        /// </summary>
        SSE42,
        /// <summary>
        /// Virtualization Technology Extensions
        /// </summary>
        VMX,
        /// <summary>
        /// Safer Mode Extensions
        /// </summary>
        SMX,
        /// <summary>
        /// Fused Multiply/Add instructions( Intel 3 operands, AMD 4 operands )
        /// </summary>
        FMA,
        /// <summary>
        /// AMD64, EMT64, INTEL64 instruction set
        /// </summary>
        LONG,
        /// <summary>
        /// Mask used for InstructionSet flags
        /// </summary>

        //CPU_8086            = 0x00010000,
        //CPU_80186           = 0x00020000,
        //CPU_80286           = 0x00040000,
        //CPU_80386           = 0x00080000,
        //CPU_80486           = 0x00100000,
        //PentiumI            = 0x00200000,
        //PentiumMMX          = 0x00400000,
        //PentiumPro          = 0x00800000,
        //PentiumII           = 0x01000000,
        //PentiumIII          = 0x02000000,
        //Pentium4            = 0x04000000,
        //Core1               = 0x08000000,
        //Core2               = 0x10000000,
        //Corei7              = 0x20000000,
        //Itanium             = 0x40000000,
        //Other               = 0x80000000,
        //CPUMask             = 0xFFFF0000,

    }
}
