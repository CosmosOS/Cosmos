using System.Collections.Generic;
using System.Reflection;

using Cosmos.Assembler.ARMv7;

namespace ASharp.Compiler
{
    public static class ASRegisters
    {
        public class Register
        {
            public readonly string Name;
            public readonly RegistersEnum RegEnum;

            public Register(RegistersEnum regEnum)
            {
                Name = Registers.GetRegisterName(regEnum);
                RegEnum = regEnum;
            }

            public static implicit operator RegistersEnum(Register register)
            {
                return register.RegEnum;
            }
        }

        private static readonly Dictionary<string, Register> mRegisters;

        static ASRegisters()
        {
            mRegisters = new Dictionary<string, Register>();

            foreach (var xField in typeof(ASRegisters).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                mRegisters.Add(xField.Name, (Register)xField.GetValue(null));
            }
        }

        public static Dictionary<string, Register> GetRegisters()
        {
            return mRegisters;
        }

        public static readonly Register r0 = new Register(RegistersEnum.r0);
        public static readonly Register r1 = new Register(RegistersEnum.r1);
        public static readonly Register r2 = new Register(RegistersEnum.r2);
        public static readonly Register r3 = new Register(RegistersEnum.r3);
        public static readonly Register r4 = new Register(RegistersEnum.r4);
        public static readonly Register r5 = new Register(RegistersEnum.r5);
        public static readonly Register r6 = new Register(RegistersEnum.r6);
        public static readonly Register r7 = new Register(RegistersEnum.r7);
        public static readonly Register r8 = new Register(RegistersEnum.r8);
        public static readonly Register r9 = new Register(RegistersEnum.r9);
        public static readonly Register r10 = new Register(RegistersEnum.r10);
        public static readonly Register r11 = new Register(RegistersEnum.r11);
        public static readonly Register r12 = new Register(RegistersEnum.r12);
        public static readonly Register sp = new Register(RegistersEnum.sp);
        public static readonly Register lr = new Register(RegistersEnum.lr);
        public static readonly Register pc = new Register(RegistersEnum.pc);
    }
}
