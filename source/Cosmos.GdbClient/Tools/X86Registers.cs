using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.GdbClient.Tools
{
    /// <summary>
    /// Represents the X86 registers.
    /// </summary>
    public struct X86Registers
    {
        private byte[][] _registers;

        public byte[][] Registers
        {
            get { return _registers; }
            set { _registers = value; }
        }

        public static X86Registers FromString(string hex)
        {
            // i'm expecting following order: eax, ebx, ecx, edx, esi, edi, ebp, 
            // esp, cs, ds, ss, es, fs, gs, eflags, eip

            // the first 8 are 4byte ones, the following 6 are 2byte, the following 2 4byte,

            byte[] lengths = new byte[] {
                
  2 * 4,                         /* %gs */
  2 * 4,                        /* %fs */
  2 * 4,                        /* %es */
  2 * 4,                        /* %ds */
  2 * 4,                        /* %edi */
  2 * 4,                        /* %esi */
  2 * 4,                        /* %ebp */
  2 * 4,                        /* %esp */
  2 * 4,                        /* %ebx */
  2 * 4,                        /* %edx */
  2 * 4,                       /* %ecx */
  2 * 4,                       /* %eax */
  2 * 4,                       /* UNKNOWN */
  2 * 4,                       /* UNKNOWN */
  2 * 4,                       /* %eip */
  2 * 4,                       /* %cs */
  2 * 4,                       /* %eflags */
  2 * 4,                       /* UNKNOWN */
  2 * 4                       /* %ss */
              };

            string[] registers = hex.SplitCount(lengths);

            X86Registers result = new X86Registers();
            result._registers = new byte[registers.Length][];

            for (int i = 0; i < registers.Length; i++)
            {
                result._registers[i] = registers[i].GetBytes();
            }

            return result;
        }
    }
}
