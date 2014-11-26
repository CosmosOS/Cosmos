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
        public const RegistersEnum EAX = RegistersEnum.EAX;
        public const RegistersEnum AX = RegistersEnum.AX;
        public const RegistersEnum AH = RegistersEnum.AH;
        public const RegistersEnum AL = RegistersEnum.AL;
        public const RegistersEnum EBX = RegistersEnum.EBX;
        public const RegistersEnum BX = RegistersEnum.BX;
        public const RegistersEnum BH = RegistersEnum.BH;
        public const RegistersEnum BL = RegistersEnum.BL;
        public const RegistersEnum ECX = RegistersEnum.ECX;
        public const RegistersEnum CX = RegistersEnum.CX;
        public const RegistersEnum CH = RegistersEnum.CH;
        public const RegistersEnum CL = RegistersEnum.CL;
        public const RegistersEnum EDX = RegistersEnum.EDX;
        public const RegistersEnum DX = RegistersEnum.DX;
        public const RegistersEnum DH = RegistersEnum.DH;
        public const RegistersEnum DL = RegistersEnum.DL;
        public const RegistersEnum CS = RegistersEnum.CS;
        public const RegistersEnum DS = RegistersEnum.DS;
        public const RegistersEnum ES = RegistersEnum.ES;
        public const RegistersEnum FS = RegistersEnum.FS;
        public const RegistersEnum GS = RegistersEnum.GS;
        public const RegistersEnum SS = RegistersEnum.SS;
        public const RegistersEnum ESP = RegistersEnum.ESP;
        public const RegistersEnum SP = RegistersEnum.SP;
        public const RegistersEnum EBP = RegistersEnum.EBP;
        public const RegistersEnum BP = RegistersEnum.BP;
        public const RegistersEnum ESI = RegistersEnum.ESI;
        public const RegistersEnum SI = RegistersEnum.SI;
        public const RegistersEnum EDI = RegistersEnum.EDI;
        public const RegistersEnum DI = RegistersEnum.DI;
        public const RegistersEnum CR0 = RegistersEnum.CR0;
        public const RegistersEnum CR1 = RegistersEnum.CR1;
        public const RegistersEnum CR2 = RegistersEnum.CR2;
        public const RegistersEnum CR3 = RegistersEnum.CR3;
        public const RegistersEnum CR4 = RegistersEnum.CR4;
        public const RegistersEnum XMM0 = RegistersEnum.XMM0;
        public const RegistersEnum XMM1 = RegistersEnum.XMM1;
        public const RegistersEnum XMM2 = RegistersEnum.XMM2;
        public const RegistersEnum XMM3 = RegistersEnum.XMM3;
        public const RegistersEnum XMM4 = RegistersEnum.XMM4;
        public const RegistersEnum XMM5 = RegistersEnum.XMM5;
        public const RegistersEnum XMM6 = RegistersEnum.XMM6;
        public const RegistersEnum XMM7 = RegistersEnum.XMM7;
        public const RegistersEnum ST0 = RegistersEnum.ST0;
        public const RegistersEnum ST1 = RegistersEnum.ST1;
        public const RegistersEnum ST2 = RegistersEnum.ST2;
        public const RegistersEnum ST3 = RegistersEnum.ST3;
        public const RegistersEnum ST4 = RegistersEnum.ST4;
        public const RegistersEnum ST5 = RegistersEnum.ST5;
        public const RegistersEnum ST6 = RegistersEnum.ST6;
        public const RegistersEnum ST7 = RegistersEnum.ST7;
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
            m32BitTo16BitMapping.Add(EAX, AX);
            m32BitTo16BitMapping.Add(EBX, BX);
            m32BitTo16BitMapping.Add(ECX, CX);
            m32BitTo16BitMapping.Add(EDX, DX);
            m32BitTo16BitMapping.Add(ESI, SI);
            m32BitTo16BitMapping.Add(EDI, DI);
            m32BitTo16BitMapping.Add(EBP, BP);
            m32BitTo16BitMapping.Add(ESP, SP);

            m32BitTo8BitMapping.Add(EAX, AL );
            m32BitTo8BitMapping.Add(EBX, BL );
            m32BitTo8BitMapping.Add(ECX, CL );
            m32BitTo8BitMapping.Add(EDX, DL );

            m16BitTo8BitMapping.Add(AX, AL);
            m16BitTo8BitMapping.Add(BX, BL);
            m16BitTo8BitMapping.Add(CX, CL);
            m16BitTo8BitMapping.Add(DX, DL);

            mRegToName.Add(EAX, "EAX");
            mRegToName.Add(AX, "AX");
            mRegToName.Add(AH, "AH");
            mRegToName.Add(AL, "AL");
            mRegToName.Add(EBX, "EBX");
            mRegToName.Add(BX, "BX");
            mRegToName.Add(BH, "BH");
            mRegToName.Add(BL, "BL");
            mRegToName.Add(ECX, "ECX");
            mRegToName.Add(CX, "CX");
            mRegToName.Add(CH, "CH");
            mRegToName.Add(CL, "CL");
            mRegToName.Add(EDX, "EDX");
            mRegToName.Add(DX, "DX");
            mRegToName.Add(DH, "DH");
            mRegToName.Add(DL, "DL");
            mRegToName.Add(CS, "CS");
            mRegToName.Add(DS, "DS");
            mRegToName.Add(ES, "ES");
            mRegToName.Add(FS, "FS");
            mRegToName.Add(GS, "GS");
            mRegToName.Add(SS, "SS");
            mRegToName.Add(ESP, "ESP");
            mRegToName.Add(SP, "SP");
            mRegToName.Add(EBP, "EBP");
            mRegToName.Add(BP, "BP");
            mRegToName.Add(ESI, "ESI");
            mRegToName.Add(SI, "SI");
            mRegToName.Add(EDI, "EDI");
            mRegToName.Add(DI, "DI");
            mRegToName.Add(CR0, "CR0");
            mRegToName.Add(CR1, "CR1");
            mRegToName.Add(CR2, "CR2");
            mRegToName.Add(CR3, "CR3");
            mRegToName.Add(CR4, "CR4");
            mRegToName.Add(XMM0, "XMM0");
            mRegToName.Add(XMM1, "XMM1");
            mRegToName.Add(XMM2, "XMM2");
            mRegToName.Add(XMM3, "XMM3");
            mRegToName.Add(XMM4, "XMM4");
            mRegToName.Add(XMM5, "XMM5");
            mRegToName.Add(XMM6, "XMM6");
            mRegToName.Add(XMM7, "XMM7");
            mRegToName.Add(ST0, "ST0");
            mRegToName.Add(ST1, "ST1");
            mRegToName.Add(ST2, "ST");
            mRegToName.Add(ST3, "ST3");
            mRegToName.Add(ST4, "ST4");
            mRegToName.Add(ST5, "ST5");
            mRegToName.Add(ST6, "ST6");
            mRegToName.Add(ST7, "ST7");
            mRegToName.Add(RegistersEnum.EIP, "EIP");
            mNameToReg.Add("EIP", RegistersEnum.EIP);
            mNameToReg.Add("EAX", EAX);
            mNameToReg.Add("AX", AX);
            mNameToReg.Add("AH", AH);
            mNameToReg.Add("AL", AL);
            mNameToReg.Add("EBX", EBX);
            mNameToReg.Add("BX", BX);
            mNameToReg.Add("BH", BH);
            mNameToReg.Add("BL", BL);
            mNameToReg.Add("ECX", ECX);
            mNameToReg.Add("CX", CX);
            mNameToReg.Add("CH", CH);
            mNameToReg.Add("CL", CL);
            mNameToReg.Add("EDX", EDX);
            mNameToReg.Add("DX", DX);
            mNameToReg.Add("DH", DH);
            mNameToReg.Add("DL", DL);
            mNameToReg.Add("CS", CS);
            mNameToReg.Add("DS", DS);
            mNameToReg.Add("ES", ES);
            mNameToReg.Add("FS", FS);
            mNameToReg.Add("GS", GS);
            mNameToReg.Add("SS", SS);
            mNameToReg.Add("ESP", ESP);
            mNameToReg.Add("SP", SP);
            mNameToReg.Add("EBP", EBP);
            mNameToReg.Add("BP", BP);
            mNameToReg.Add("ESI", ESI);
            mNameToReg.Add("SI", SI);
            mNameToReg.Add("EDI", EDI);
            mNameToReg.Add("DI", DI);
            mNameToReg.Add("CR0", CR0);
            mNameToReg.Add("CR1", CR1);
            mNameToReg.Add("CR2", CR2);
            mNameToReg.Add("CR3", CR3);
            mNameToReg.Add("CR4", CR4);
            mNameToReg.Add("XMM0", XMM0);
            mNameToReg.Add("XMM1", XMM1);
            mNameToReg.Add("XMM2", XMM2);
            mNameToReg.Add("XMM3", XMM3);
            mNameToReg.Add("XMM4", XMM4);
            mNameToReg.Add("XMM5", XMM5);
            mNameToReg.Add("XMM6", XMM6);
            mNameToReg.Add("XMM7", XMM7);
            mNameToReg.Add("ST0", ST0);
            mNameToReg.Add("ST1", ST1);
            mNameToReg.Add("ST2", ST2);
            mNameToReg.Add("ST3", ST3);
            mNameToReg.Add("ST4", ST4);
            mNameToReg.Add("ST5", ST5);
            mNameToReg.Add("ST6", ST6);
            mNameToReg.Add("ST7", ST7);
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
            return aReg == CR0 ||aReg == CR1 ||aReg == CR2 ||aReg == CR3 ||aReg == CR4;
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
                aRegister == AL ||
                aRegister == AH ||
                aRegister == BL ||
                aRegister == BH ||
                aRegister == CL ||
                aRegister == CH ||
                aRegister == DL ||
                aRegister == DH;
        }

        //public static Guid Get 

		public static bool Is80Bit(RegistersEnum aRegister)
		{
			return
				aRegister == ST0 ||
				aRegister == ST1 ||
				aRegister == ST2 ||
				aRegister == ST3 ||
				aRegister == ST4 ||
				aRegister == ST5 ||
				aRegister == ST6 ||
				aRegister == ST7;
		}

        public static bool Is128Bit(RegistersEnum aRegister)
        {
            return aRegister == XMM0 || aRegister == XMM1 || aRegister == XMM2 || aRegister == XMM3 || aRegister == XMM4 || aRegister == XMM5 || aRegister == XMM6 || aRegister == XMM7;
        }

        public static bool IsSegment(RegistersEnum aRegister)
        {
            return aRegister == CS || aRegister == DS || aRegister == ES || aRegister == FS || aRegister == GS || aRegister == SS;
        }

        public static bool Is32Bit(RegistersEnum aRegister)
        {
            return aRegister == EAX || aRegister == EBX || aRegister == ECX || aRegister == EDX || aRegister == ESP || aRegister == EBP || aRegister == ESI || aRegister == EDI || aRegister == CR0 || aRegister == CR1 || aRegister == CR2 || aRegister == CR3 || aRegister == CR4
                || aRegister == RegistersEnum.EIP;
        }

        public static bool Is16Bit(RegistersEnum aRegister)
        {
            return aRegister == AX || aRegister == BX || aRegister == CX || aRegister == DX || aRegister == CS || aRegister == DS || aRegister == ES || aRegister == FS || aRegister == GS || aRegister == SS || aRegister == SI || aRegister == DI || aRegister == SP || aRegister == BP;
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
            registers.Add(CR0);
            registers.Add(CR1);
            registers.Add(CR2);
            registers.Add(CR3);
            registers.Add(CR4);
            return registers;
        }
        public static List<RegistersEnum> getXMMs()
        {
            var registers = new List<RegistersEnum>();
            registers.Add(XMM0);
            registers.Add(XMM1);
            registers.Add(XMM2);
            registers.Add(XMM3);
            registers.Add(XMM4);
            registers.Add(XMM5);
            registers.Add(XMM6);
            registers.Add(XMM7);
            return registers;
        }

        public static List<RegistersEnum> getSTs()
        {
            var registers = new List<RegistersEnum>();
            registers.Add(ST0);
            registers.Add(ST1);
            registers.Add(ST2);
            registers.Add(ST3);
            registers.Add(ST4);
            registers.Add(ST5);
            registers.Add(ST6);
            registers.Add(ST7);
            return registers;
        }
    }
}