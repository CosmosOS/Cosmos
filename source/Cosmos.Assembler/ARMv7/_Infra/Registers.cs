using System;
using System.Collections.Generic;

namespace Cosmos.Assembler.ARMv7
{
    /// <summary>
    /// ARMv7 Registers
    /// </summary>
    public enum RegistersEnum : byte
    {
        r0,
        r1,
        r2,
        r3,
        r4,
        r5,
        r6,
        r7,
        r8,
        r9,
        r10,
        r11,
        r12,
        sp,
        lr,
        pc
    }

    public static class Registers
    {
        private static Dictionary<RegistersEnum, string> mRegToName = new Dictionary<RegistersEnum, string>();
        private static Dictionary<string, RegistersEnum> mNameToReg = new Dictionary<string, RegistersEnum>();

        static Registers()
        {
            mRegToName.Add(RegistersEnum.r0, "r0");
            mRegToName.Add(RegistersEnum.r1, "r1");
            mRegToName.Add(RegistersEnum.r2, "r2");
            mRegToName.Add(RegistersEnum.r3, "r3");
            mRegToName.Add(RegistersEnum.r4, "r4");
            mRegToName.Add(RegistersEnum.r5, "r5");
            mRegToName.Add(RegistersEnum.r6, "r6");
            mRegToName.Add(RegistersEnum.r7, "r7");
            mRegToName.Add(RegistersEnum.r8, "r8");
            mRegToName.Add(RegistersEnum.r9, "r9");
            mRegToName.Add(RegistersEnum.r10, "r10");
            mRegToName.Add(RegistersEnum.r11, "r11");
            mRegToName.Add(RegistersEnum.r12, "r12");
            mRegToName.Add(RegistersEnum.sp, "sp");
            mRegToName.Add(RegistersEnum.lr, "lr");
            mRegToName.Add(RegistersEnum.pc, "pc");

            mNameToReg.Add("r0", RegistersEnum.r0);
            mNameToReg.Add("r1", RegistersEnum.r1);
            mNameToReg.Add("r2", RegistersEnum.r2);
            mNameToReg.Add("r3", RegistersEnum.r3);
            mNameToReg.Add("r4", RegistersEnum.r4);
            mNameToReg.Add("r5", RegistersEnum.r5);
            mNameToReg.Add("r6", RegistersEnum.r6);
            mNameToReg.Add("r7", RegistersEnum.r7);
            mNameToReg.Add("r8", RegistersEnum.r8);
            mNameToReg.Add("r9", RegistersEnum.r9);
            mNameToReg.Add("r10", RegistersEnum.r10);
            mNameToReg.Add("r11", RegistersEnum.r11);
            mNameToReg.Add("r12", RegistersEnum.r12);
            mNameToReg.Add("sp", RegistersEnum.sp);
            mNameToReg.Add("lr", RegistersEnum.lr);
            mNameToReg.Add("pc", RegistersEnum.pc);
        }

        /// <summary>
        /// Returns the name of the register for the given register.
        /// </summary>
        /// <param name="aRegister">The register.</param>
        /// <returns>The name of the given register.</returns>
        public static string GetRegisterName(RegistersEnum aRegister)
        {
            return mRegToName[aRegister];
        }

        /// <summary>
        /// Returns the register for the given register name.
        /// </summary>
        /// <param name="aName">The register name.</param>
        /// <returns>The register for the given register name.</returns>
        public static RegistersEnum? GetRegister(string aName)
        {
            if (mNameToReg.ContainsKey(aName))
            {
                return mNameToReg[aName];
            }
            else
            {
                return null;
            }
        }

        public static byte GetSize(RegistersEnum aRegister)
        {
            return 32;
        }

        public static bool Is32Bit(RegistersEnum aRegister)
        {
            return true;
        }

        /// <summary>
        /// Returns a List of registers
        /// </summary>
        /// <returns>List of registers.</returns>
        public static List<RegistersEnum> GetRegisters()
        {
            List<RegistersEnum> registers = new List<RegistersEnum>();
            foreach (RegistersEnum xField in Enum.GetValues(typeof(RegistersEnum)))
            {
                registers.Add(xField);
            }

            return registers;
        }

        //public static List<RegistersEnum> Get32BitRegisters()
        //{
        //    var xResult = new List<RegistersEnum>();
        //    foreach (var xItem in GetRegisters())
        //    {
        //        if (Is32Bit(xItem))
        //        {
        //            xResult.Add(xItem);
        //        }
        //    }
        //    return xResult;
        //}
    }
}
