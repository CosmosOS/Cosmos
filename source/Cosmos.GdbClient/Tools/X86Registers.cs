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
            string[] registers = hex.SplitCount(16, 16, 16, 16, 16, 16, 16, 16, 16, 16);

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
