using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.x86 {
    public enum RegistersEnum: byte
    {
        EAX,
        AX,
        AH,
        AL,
        EBX,
        BX,
        BH,
        BL,
        ECX,
        CX,
        CH,
        CL,
        EDX,
        DX,
        DH,
        DL,
        CS,
        DS,
        ES,
        FS,
        GS,
        SS,
        ESP,
        SP,
        EBP,
        BP,
        ESI,
        SI,
        EDI,
        DI,
        CR0,
        CR1,
        CR2,
        CR3,
        CR4,
        XMM0,
        XMM1,
        XMM2,
        XMM3,
        XMM4,
        XMM5,
        XMM6,
        XMM7,
        ST0,
        ST1,
        ST2,
        ST3,
        ST4,
        ST5,
        ST6,
        ST7,
        EIP,
    }
    public static class Registers {
        /// <summary>
        /// Key = 32bit (eg EAX), value = 16 bit (eg AX)
        /// </summary>
        private static Dictionary<RegistersEnum, RegistersEnum> m32BitTo16BitMapping = new Dictionary<RegistersEnum, RegistersEnum>();
        /// <summary>
        /// Key = 32bit (eg EAX), value = 16 bit (eg AL).
        ///
        /// </summary>
        private static Dictionary<RegistersEnum, RegistersEnum> m32BitTo8BitMapping = new Dictionary<RegistersEnum, RegistersEnum>();
        private static Dictionary<RegistersEnum, RegistersEnum> m16BitTo8BitMapping = new Dictionary<RegistersEnum, RegistersEnum>();

        private static Dictionary<RegistersEnum, string> mRegToName=new Dictionary<RegistersEnum, string>();
        private static Dictionary<string, RegistersEnum> mNameToReg=new Dictionary<string, RegistersEnum>();

        static Registers() {
            m32BitTo16BitMapping.Add(RegistersEnum.EAX, RegistersEnum.AX);
            m32BitTo16BitMapping.Add(RegistersEnum.EBX, RegistersEnum.BX);
            m32BitTo16BitMapping.Add(RegistersEnum.ECX, RegistersEnum.CX);
            m32BitTo16BitMapping.Add(RegistersEnum.EDX, RegistersEnum.DX);
            m32BitTo16BitMapping.Add(RegistersEnum.ESI, RegistersEnum.SI);
            m32BitTo16BitMapping.Add(RegistersEnum.EDI, RegistersEnum.DI);
            m32BitTo16BitMapping.Add(RegistersEnum.EBP, RegistersEnum.BP);
            m32BitTo16BitMapping.Add(RegistersEnum.ESP, RegistersEnum.SP);

            m32BitTo8BitMapping.Add(RegistersEnum.EAX, RegistersEnum.AL);
            m32BitTo8BitMapping.Add(RegistersEnum.EBX, RegistersEnum.BL);
            m32BitTo8BitMapping.Add(RegistersEnum.ECX, RegistersEnum.CL);
            m32BitTo8BitMapping.Add(RegistersEnum.EDX, RegistersEnum.DL);

            m16BitTo8BitMapping.Add(RegistersEnum.AX, RegistersEnum.AL);
            m16BitTo8BitMapping.Add(RegistersEnum.BX, RegistersEnum.BL);
            m16BitTo8BitMapping.Add(RegistersEnum.CX, RegistersEnum.CL);
            m16BitTo8BitMapping.Add(RegistersEnum.DX, RegistersEnum.DL);

            mRegToName.Add(RegistersEnum.EAX, "EAX");
            mRegToName.Add(RegistersEnum.AX, "AX");
            mRegToName.Add(RegistersEnum.AH, "AH");
            mRegToName.Add(RegistersEnum.AL, "AL");
            mRegToName.Add(RegistersEnum.EBX, "EBX");
            mRegToName.Add(RegistersEnum.BX, "BX");
            mRegToName.Add(RegistersEnum.BH, "BH");
            mRegToName.Add(RegistersEnum.BL, "BL");
            mRegToName.Add(RegistersEnum.ECX, "ECX");
            mRegToName.Add(RegistersEnum.CX, "CX");
            mRegToName.Add(RegistersEnum.CH, "CH");
            mRegToName.Add(RegistersEnum.CL, "CL");
            mRegToName.Add(RegistersEnum.EDX, "EDX");
            mRegToName.Add(RegistersEnum.DX, "DX");
            mRegToName.Add(RegistersEnum.DH, "DH");
            mRegToName.Add(RegistersEnum.DL, "DL");
            mRegToName.Add(RegistersEnum.CS, "CS");
            mRegToName.Add(RegistersEnum.DS, "DS");
            mRegToName.Add(RegistersEnum.ES, "ES");
            mRegToName.Add(RegistersEnum.FS, "FS");
            mRegToName.Add(RegistersEnum.GS, "GS");
            mRegToName.Add(RegistersEnum.SS, "SS");
            mRegToName.Add(RegistersEnum.ESP, "ESP");
            mRegToName.Add(RegistersEnum.SP, "SP");
            mRegToName.Add(RegistersEnum.EBP, "EBP");
            mRegToName.Add(RegistersEnum.BP, "BP");
            mRegToName.Add(RegistersEnum.ESI, "ESI");
            mRegToName.Add(RegistersEnum.SI, "SI");
            mRegToName.Add(RegistersEnum.EDI, "EDI");
            mRegToName.Add(RegistersEnum.DI, "DI");
            mRegToName.Add(RegistersEnum.CR0, "CR0");
            mRegToName.Add(RegistersEnum.CR1, "CR1");
            mRegToName.Add(RegistersEnum.CR2, "CR2");
            mRegToName.Add(RegistersEnum.CR3, "CR3");
            mRegToName.Add(RegistersEnum.CR4, "CR4");
            mRegToName.Add(RegistersEnum.XMM0, "XMM0");
            mRegToName.Add(RegistersEnum.XMM1, "XMM1");
            mRegToName.Add(RegistersEnum.XMM2, "XMM2");
            mRegToName.Add(RegistersEnum.XMM3, "XMM3");
            mRegToName.Add(RegistersEnum.XMM4, "XMM4");
            mRegToName.Add(RegistersEnum.XMM5, "XMM5");
            mRegToName.Add(RegistersEnum.XMM6, "XMM6");
            mRegToName.Add(RegistersEnum.XMM7, "XMM7");
            mRegToName.Add(RegistersEnum.ST0, "ST0");
            mRegToName.Add(RegistersEnum.ST1, "ST1");
            mRegToName.Add(RegistersEnum.ST2, "ST");
            mRegToName.Add(RegistersEnum.ST3, "ST3");
            mRegToName.Add(RegistersEnum.ST4, "ST4");
            mRegToName.Add(RegistersEnum.ST5, "ST5");
            mRegToName.Add(RegistersEnum.ST6, "ST6");
            mRegToName.Add(RegistersEnum.ST7, "ST7");
            mRegToName.Add(RegistersEnum.EIP, "EIP");
            mNameToReg.Add("EIP", RegistersEnum.EIP);
            mNameToReg.Add("EAX", RegistersEnum.EAX);
            mNameToReg.Add("AX",  RegistersEnum.AX);
            mNameToReg.Add("AH",  RegistersEnum.AH);
            mNameToReg.Add("AL",  RegistersEnum.AL);
            mNameToReg.Add("EBX", RegistersEnum.EBX);
            mNameToReg.Add("BX",  RegistersEnum.BX);
            mNameToReg.Add("BH",  RegistersEnum.BH);
            mNameToReg.Add("BL",  RegistersEnum.BL);
            mNameToReg.Add("ECX", RegistersEnum.ECX);
            mNameToReg.Add("CX",  RegistersEnum.CX);
            mNameToReg.Add("CH",  RegistersEnum.CH);
            mNameToReg.Add("CL",  RegistersEnum.CL);
            mNameToReg.Add("EDX", RegistersEnum.EDX);
            mNameToReg.Add("DX",  RegistersEnum.DX);
            mNameToReg.Add("DH", RegistersEnum.DH);
            mNameToReg.Add("DL", RegistersEnum.DL);
            mNameToReg.Add("CS", RegistersEnum.CS);
            mNameToReg.Add("DS", RegistersEnum.DS);
            mNameToReg.Add("ES", RegistersEnum.ES);
            mNameToReg.Add("FS", RegistersEnum.FS);
            mNameToReg.Add("GS", RegistersEnum.GS);
            mNameToReg.Add("SS", RegistersEnum.SS);
            mNameToReg.Add("ESP", RegistersEnum.ESP);
            mNameToReg.Add("SP",  RegistersEnum.SP);
            mNameToReg.Add("EBP", RegistersEnum.EBP);
            mNameToReg.Add("BP",  RegistersEnum.BP);
            mNameToReg.Add("ESI", RegistersEnum.ESI);
            mNameToReg.Add("SI",  RegistersEnum.SI);
            mNameToReg.Add("EDI", RegistersEnum.EDI);
            mNameToReg.Add("DI",  RegistersEnum.DI);
            mNameToReg.Add("CR0", RegistersEnum.CR0);
            mNameToReg.Add("CR1", RegistersEnum.CR1);
            mNameToReg.Add("CR2", RegistersEnum.CR2);
            mNameToReg.Add("CR3", RegistersEnum.CR3);
            mNameToReg.Add("CR4", RegistersEnum.CR4);
            mNameToReg.Add("XMM0", RegistersEnum.XMM0);
            mNameToReg.Add("XMM1", RegistersEnum.XMM1);
            mNameToReg.Add("XMM2", RegistersEnum.XMM2);
            mNameToReg.Add("XMM3", RegistersEnum.XMM3);
            mNameToReg.Add("XMM4", RegistersEnum.XMM4);
            mNameToReg.Add("XMM5", RegistersEnum.XMM5);
            mNameToReg.Add("XMM6", RegistersEnum.XMM6);
            mNameToReg.Add("XMM7", RegistersEnum.XMM7);
            mNameToReg.Add("ST0", RegistersEnum.ST0);
            mNameToReg.Add("ST1", RegistersEnum.ST1);
            mNameToReg.Add("ST2", RegistersEnum.ST2);
            mNameToReg.Add("ST3", RegistersEnum.ST3);
            mNameToReg.Add("ST4", RegistersEnum.ST4);
            mNameToReg.Add("ST5", RegistersEnum.ST5);
            mNameToReg.Add("ST6", RegistersEnum.ST6);
            mNameToReg.Add("ST7", RegistersEnum.ST7);
        }

        public static RegistersEnum? Get8BitRegistersForRegister(RegistersEnum aReg)
        {
            if(Is32Bit(aReg)) {
                if(m32BitTo8BitMapping.ContainsKey(aReg)) {
                    return m32BitTo8BitMapping[aReg];
                }
                return null;
            }
            if(Is16Bit(aReg)) {
                if (m16BitTo8BitMapping.ContainsKey(aReg)) {
                    return m16BitTo8BitMapping[aReg];
                }
                return null;
            }
            if(Is128Bit(aReg)) {
                throw new Exception("128bit registers don't have 8bit variants!");
            }
            return aReg;
        }

        public static bool IsCR(RegistersEnum aReg)
        {
            return aReg == RegistersEnum.CR0 ||aReg == RegistersEnum.CR1 ||aReg == RegistersEnum.CR2 ||aReg == RegistersEnum.CR3 ||aReg == RegistersEnum.CR4;
        }

        public static RegistersEnum? Get16BitRegisterForRegister(RegistersEnum aReg)
        {
            if (Is32Bit(aReg)) {
                if (m32BitTo16BitMapping.ContainsKey(aReg)) {
                    return m32BitTo16BitMapping[aReg];
                }
                return null;
            }
            if (Is128Bit(aReg)) {
                throw new Exception("128bit registers don't have 8bit variants!");
            }
            if (Is16Bit(aReg)) {
                return aReg;
            }
            if (m16BitTo8BitMapping.ContainsKey(aReg)) {
                return m16BitTo8BitMapping[aReg];
            }
            return aReg;
        }

        public static RegistersEnum? Get32BitRegisterForRegister(RegistersEnum aReg)
        {
            if(Is32Bit(aReg)) {
                return aReg;
            }
            if(Is128Bit(aReg)) {
                throw new Exception("128bit registers don't have 32bit variants!");
            }
            if(Is16Bit(aReg)) {
                if(m32BitTo16BitMapping.ContainsValue(aReg)) {
                    return (from item in m32BitTo16BitMapping
                            where item.Value == aReg
                            select item.Key).Single();
                }
                return null;
            }
            if (m32BitTo8BitMapping.ContainsValue(aReg)) {
                return (from item in m32BitTo8BitMapping
                        where item.Value == aReg
                        select item.Key).Single();
            }
            return null;
        }

        public static string GetRegisterName(RegistersEnum aRegister)
        {
            return mRegToName[aRegister];
        }

        public static RegistersEnum? GetRegister(string aName)
        {
            if (mNameToReg.ContainsKey(aName))
            {
                return mNameToReg[aName];
            }else
            {
                return null;
            }
        }

        public static byte GetSize(RegistersEnum aRegister) {
            if (Is128Bit(aRegister)) { return 128; }
			if (Is80Bit(aRegister)) { return 80; }
            if (Is32Bit(aRegister)) { return 32; }
            if (Is16Bit(aRegister)) { return 16; }
            if (Is8Bit(aRegister)) { return 8; }
            throw new NotImplementedException();
        }

        public static bool Is8Bit(RegistersEnum aRegister)
        {
            return
                aRegister == RegistersEnum.AL ||
                aRegister == RegistersEnum.AH ||
                aRegister == RegistersEnum.BL ||
                aRegister == RegistersEnum.BH ||
                aRegister == RegistersEnum.CL ||
                aRegister == RegistersEnum.CH ||
                aRegister == RegistersEnum.DL ||
                aRegister == RegistersEnum.DH;
        }

        //public static Guid Get

		public static bool Is80Bit(RegistersEnum aRegister)
		{
			return
				aRegister == RegistersEnum.ST0 ||
				aRegister == RegistersEnum.ST1 ||
				aRegister == RegistersEnum.ST2 ||
				aRegister == RegistersEnum.ST3 ||
				aRegister == RegistersEnum.ST4 ||
				aRegister == RegistersEnum.ST5 ||
				aRegister == RegistersEnum.ST6 ||
				aRegister == RegistersEnum.ST7;
		}

        public static bool Is128Bit(RegistersEnum aRegister)
        {
            return aRegister == RegistersEnum.XMM0 || aRegister == RegistersEnum.XMM1 || aRegister == RegistersEnum.XMM2 || aRegister == RegistersEnum.XMM3 || aRegister == RegistersEnum.XMM4 || aRegister == RegistersEnum.XMM5 || aRegister == RegistersEnum.XMM6 || aRegister == RegistersEnum.XMM7;
        }

        public static bool IsSegment(RegistersEnum aRegister)
        {
            return aRegister == RegistersEnum.CS || aRegister == RegistersEnum.DS || aRegister == RegistersEnum.ES || aRegister == RegistersEnum.FS || aRegister == RegistersEnum.GS || aRegister == RegistersEnum.SS;
        }

        public static bool Is32Bit(RegistersEnum aRegister)
        {
            return aRegister == RegistersEnum.EAX || aRegister == RegistersEnum.EBX || aRegister == RegistersEnum.ECX || aRegister == RegistersEnum.EDX || aRegister == RegistersEnum.ESP || aRegister == RegistersEnum.EBP || aRegister == RegistersEnum.ESI || aRegister == RegistersEnum.EDI || aRegister == RegistersEnum.CR0 || aRegister == RegistersEnum.CR1 || aRegister == RegistersEnum.CR2 || aRegister == RegistersEnum.CR3 || aRegister == RegistersEnum.CR4
                || aRegister == RegistersEnum.EIP;
        }

        public static bool Is16Bit(RegistersEnum aRegister)
        {
            return aRegister == RegistersEnum.AX || aRegister == RegistersEnum.BX || aRegister == RegistersEnum.CX || aRegister == RegistersEnum.DX || aRegister == RegistersEnum.CS || aRegister == RegistersEnum.DS || aRegister == RegistersEnum.ES || aRegister == RegistersEnum.FS || aRegister == RegistersEnum.GS || aRegister == RegistersEnum.SS || aRegister == RegistersEnum.SI || aRegister == RegistersEnum.DI || aRegister == RegistersEnum.SP || aRegister == RegistersEnum.BP;
        }

        public static List<RegistersEnum> GetRegisters()
        {
            List<RegistersEnum> registers = new List<RegistersEnum>();
            foreach (RegistersEnum xField in Enum.GetValues(typeof(RegistersEnum)))
            {
                registers.Add(xField);
            }

            return registers;
        }

        public static List<RegistersEnum> Get8BitRegisters()
        {
            var xResult = new List<RegistersEnum>();
            foreach(var xItem in GetRegisters()) {
                if(Is8Bit(xItem)) {
                    xResult.Add(xItem);
                }
            }
            return xResult;
        }

        public static List<RegistersEnum> Get16BitRegisters()
        {
            var xResult = new List<RegistersEnum>();
            foreach (var xItem in GetRegisters()) {
                if (Is16Bit(xItem)) {
                    xResult.Add(xItem);
                }
            }
            return xResult;
        }

        public static List<RegistersEnum> Get32BitRegisters()
        {
            var xResult = new List<RegistersEnum>();
            foreach (var xItem in GetRegisters()) {
                if (Is32Bit(xItem)) {
                    xResult.Add(xItem);
                }
            }
            return xResult;
        }

        public static List<RegistersEnum> GetCRs()
        {
            var registers = new List<RegistersEnum>();
            registers.Add(RegistersEnum.CR0);
            registers.Add(RegistersEnum.CR1);
            registers.Add(RegistersEnum.CR2);
            registers.Add(RegistersEnum.CR3);
            registers.Add(RegistersEnum.CR4);
            return registers;
        }
        public static List<RegistersEnum> getXMMs()
        {
            var registers = new List<RegistersEnum>();
            registers.Add(RegistersEnum.XMM0);
            registers.Add(RegistersEnum.XMM1);
            registers.Add(RegistersEnum.XMM2);
            registers.Add(RegistersEnum.XMM3);
            registers.Add(RegistersEnum.XMM4);
            registers.Add(RegistersEnum.XMM5);
            registers.Add(RegistersEnum.XMM6);
            registers.Add(RegistersEnum.XMM7);
            return registers;
        }

        public static List<RegistersEnum> getSTs()
        {
            var registers = new List<RegistersEnum>();
            registers.Add(RegistersEnum.ST0);
            registers.Add(RegistersEnum.ST1);
            registers.Add(RegistersEnum.ST2);
            registers.Add(RegistersEnum.ST3);
            registers.Add(RegistersEnum.ST4);
            registers.Add(RegistersEnum.ST5);
            registers.Add(RegistersEnum.ST6);
            registers.Add(RegistersEnum.ST7);
            return registers;
        }
    }
}
