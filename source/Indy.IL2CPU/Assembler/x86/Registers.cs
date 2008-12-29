using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public static class Registers {
        public static readonly Guid EAX = new Guid("{e1d2ed1d-d0e7-4020-91fa-fc3e9fabed49}");
        public static readonly Guid AX = new Guid("{b42db635-46ea-4ebd-8444-7a05bdc35e59}");
        public static readonly Guid AH = new Guid("{deedc4ae-260f-4b2b-9d11-c8cb8f03d481}");
        public static readonly Guid AL = new Guid("{aba6031f-2b28-448e-b447-0c82139a513d}");
        public static readonly Guid EBX = new Guid("{1cba44f4-8ed0-4f51-8cb2-e206fda0179c}");
        public static readonly Guid BX = new Guid("{a78c11c5-d96e-4192-b2f6-73c9c0e5e001}");
        public static readonly Guid BH = new Guid("{f9264fa9-4884-49d8-aa00-522f456b4955}");
        public static readonly Guid BL = new Guid("{74873218-cb95-4623-91f4-528d736c9202}");
        public static readonly Guid ECX = new Guid("{0301c5a0-51f9-4915-9151-6db72240333d}");
        public static readonly Guid CX = new Guid("{0b912ac1-7c9a-4099-bb1c-a65e4682d9ce}");
        public static readonly Guid CH = new Guid("{d24a53bc-0346-42d5-9206-f94c83e3e1a4}");
        public static readonly Guid CL = new Guid("{bf9623f1-f631-440d-bbb8-ec49c956ac6d}");
        public static readonly Guid EDX = new Guid("{a1307efb-ebbe-40b4-a081-2a12376c0ea5}");
        public static readonly Guid DX = new Guid("{1310d3c8-4c0c-4a24-b3cc-4ca8bd7b2fbe}");
        public static readonly Guid DH = new Guid("{2cbad390-bb54-4515-84ba-72c24b143034}");
        public static readonly Guid DL = new Guid("{59d64c55-a0e9-4d55-838d-61bb96caa1ab}");
        public static readonly Guid CS = new Guid("{ab247b99-a49d-4b01-931a-62a9f252fb82}");
        public static readonly Guid DS = new Guid("{6c10fb28-0ac1-44bf-99ff-2023a744f8ed}");
        public static readonly Guid ES = new Guid("{2482da91-e1e3-472f-880c-16232887d2ae}");
        public static readonly Guid FS = new Guid("{582c5557-5c46-4fa1-a8c6-082573d47de2}");
        public static readonly Guid GS = new Guid("{440e590c-2d2a-43f4-a8b0-d93f2a7a66d1}");
        public static readonly Guid SS = new Guid("{2e34c648-22f8-4002-a780-96cbd843e18b}");
        public static readonly Guid ESP = new Guid("{eecce0ba-9182-4319-a4cb-458a15171be5}");
        public static readonly Guid EBP = new Guid("{20867aa8-a7e5-4940-a5df-7a266eb57efa}");
        public static readonly Guid ESI = new Guid("{ab49b4e5-cc33-45bd-94f7-3188c916a153}");
        public static readonly Guid EDI = new Guid("{0ccba916-8a8c-46d7-aee9-211d95a140c9}");
        public static readonly Guid SI = new Guid("{c6278940-2584-474b-a197-7666c92f9b0b}");
        public static readonly Guid DI = new Guid("{59b80ecd-e523-4209-b9e7-dfba33c909dd}");
        public static readonly Guid SP = new Guid("{d45d1bba-48da-4766-9c32-c976e568de07}");
        public static readonly Guid BP = new Guid("{1b7705ba-6ae2-456d-8a01-a6520a678d36}");
        public static readonly Guid CR0 = new Guid("{eec47a09-9b12-45d1-afaa-a94e7d06a147}");
        public static readonly Guid CR1 = new Guid("{eec47a09-9b12-45d1-afaa-a94e7d06a148}");
        public static readonly Guid CR2 = new Guid("{eec47a09-9b12-45d1-afaa-a94e7d06a149}");
        public static readonly Guid CR3 = new Guid("{eec47a09-9b12-45d1-afaa-a94e7d06a14a}");
        public static readonly Guid CR4 = new Guid("{eec47a09-9b12-45d1-afaa-a94e7d06a14b}");
        public static readonly Guid XMM0 = new Guid("{D57DED71-D20B-4350-B56D-E2216187B135}");
        public static readonly Guid XMM1 = new Guid("{3DB5D30F-5C5E-4F80-9D65-C604D5BC175B}");
        public static readonly Guid XMM2 = new Guid("{BD0E1002-4646-4243-B6AA-C6773F347ED9}");
        public static readonly Guid XMM3 = new Guid("{7EFD3557-0C49-4E5C-A2EA-05D5AE9C75D5}");
        public static readonly Guid XMM4 = new Guid("{F8D846DE-6A93-4263-BDF2-FC17913921AB}");
        public static readonly Guid XMM5 = new Guid("{02829734-AC18-4E56-888D-A92243924292}");
        public static readonly Guid XMM6 = new Guid("{29356F03-4A37-4C7D-B452-02B0E78C0646}");
        public static readonly Guid XMM7 = new Guid("{D5334D4A-EF4B-45DF-8E3E-BDFC848D65B1}");
        /// <summary>
        /// Key = 32bit (eg EAX), value = 16 bit (eg AX)
        /// </summary>
        private static SortedList<Guid, Guid> m32BitTo16BitMapping = new SortedList<Guid, Guid>();
        /// <summary>
        /// Key = 32bit (eg EAX), value = 16 bit (eg AL). 
        /// 
        /// </summary>
        private static SortedList<Guid, Guid> m32BitTo8BitMapping = new SortedList<Guid, Guid>();
        private static SortedList<Guid, Guid> m16BitTo8BitMapping = new SortedList<Guid, Guid>();

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
        }

        public static Guid Get8BitRegistersForRegister(Guid aReg) {
            if(Is32Bit(aReg)) {
                if(m32BitTo8BitMapping.ContainsKey(aReg)) {
                    return m32BitTo8BitMapping[aReg];
                }
                return Guid.Empty;
            }
            if(Is16Bit(aReg)) {
                if (m16BitTo8BitMapping.ContainsKey(aReg)) {
                    return m16BitTo8BitMapping[aReg];
                }
                return Guid.Empty;
            }
            if(Is128Bit(aReg)) {
                throw new Exception("128bit registers don't have 8bit variants!");
            }
            return aReg;
        }

        public static bool IsCR(Guid aReg) {
            return aReg == CR0 ||aReg == CR1 ||aReg == CR2 ||aReg == CR3 ||aReg == CR4;
        }

        public static Guid Get16BitRegisterForRegister(Guid aReg) {
            if (Is32Bit(aReg)) {
                if (m32BitTo16BitMapping.ContainsKey(aReg)) {
                    return m32BitTo16BitMapping[aReg];
                }
                return Guid.Empty;
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

        public static Guid Get32BitRegisterForRegister(Guid aReg) {
            if(Is32Bit(aReg)) {
                return aReg;
            }
            if(Is128Bit(aReg)) {
                throw new Exception("128bit registers don't have 32bit variants!");
            }
            if(Is16Bit(aReg)) {
                if(m32BitTo16BitMapping.ContainsValue(aReg)) {
                    return m32BitTo16BitMapping.Keys[m32BitTo16BitMapping.IndexOfValue(aReg)];
                }
                return Guid.Empty;
            }
            if (m32BitTo8BitMapping.ContainsValue(aReg)) {
                return m32BitTo8BitMapping.Keys[m32BitTo8BitMapping.IndexOfValue(aReg)];
            }
            return Guid.Empty;
        }

        public static string GetRegisterName(Guid aRegister) {
            var xType = typeof(Registers);
            foreach (var xField in xType.GetFields()) {
                var xFieldId = (Guid)xField.GetValue(null);
                if (xFieldId == aRegister) { return xField.Name; }
            }
            throw new Exception("Register '" + aRegister + "' not valid!");
        }

        public static Guid GetRegister(string aName) {
            var xType = typeof(Registers);
            var xField = xType.GetField(aName);
            return (Guid)xField.GetValue(null);
        }

        public static byte GetSize(Guid aRegister) {
            if (Is128Bit(aRegister)) { return 128; }
            if (Is32Bit(aRegister)) { return 32; }
            if (Is16Bit(aRegister)) { return 16; }
            if (Is8Bit(aRegister)) { return 8; }
            throw new NotImplementedException();
        }

        public static bool Is8Bit(Guid aRegister) {
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

        public static bool Is128Bit(Guid aRegister) {
            return aRegister == XMM0 || aRegister == XMM1 || aRegister == XMM2 || aRegister == XMM3 || aRegister == XMM4 || aRegister == XMM5 || aRegister == XMM6 || aRegister == XMM7;
        }

        public static bool IsSegment(Guid aRegister) {
            return aRegister == CS || aRegister == DS || aRegister == ES || aRegister == FS || aRegister == GS || aRegister == SS;
        }

        public static bool Is32Bit(Guid aRegister) {
            return aRegister == EAX || aRegister == EBX || aRegister == ECX || aRegister == EDX || aRegister == ESP || aRegister == EBP || aRegister == ESI || aRegister == EDI || aRegister == CR0 || aRegister == CR1 || aRegister == CR2 || aRegister == CR3 || aRegister == CR4;
        }

        public static bool Is16Bit(Guid aRegister) {
            return aRegister == AX || aRegister == BX || aRegister == CX || aRegister == DX || aRegister == CS || aRegister == DS || aRegister == ES || aRegister == FS || aRegister == GS || aRegister == SS || aRegister == SI || aRegister == DI || aRegister == SP || aRegister == BP;
        }

        public static List<Guid> GetRegisters()
        {
            List<Guid> registers = new List<Guid>();
            var xType = typeof(Registers);
            foreach (var xField in xType.GetFields())
            {
                var xFieldId = (Guid)xField.GetValue(null);
                registers.Add(xFieldId);
            }

            return registers;
        }

        public static List<Guid> Get8BitRegisters() {
            var xResult = new List<Guid>();
            foreach(var xItem in GetRegisters()) {
                if(Is8Bit(xItem)) {
                    xResult.Add(xItem);
                }
            }
            return xResult;
        }

        public static List<Guid> Get16BitRegisters() {
            var xResult = new List<Guid>();
            foreach (var xItem in GetRegisters()) {
                if (Is16Bit(xItem)) {
                    xResult.Add(xItem);
                }
            }
            return xResult;
        }

        public static List<Guid> Get32BitRegisters() {
            var xResult = new List<Guid>();
            foreach (var xItem in GetRegisters()) {
                if (Is32Bit(xItem)) {
                    xResult.Add(xItem);
                }
            }
            return xResult;
        }

        public static List<Guid> GetCRs()
        {
            List<Guid> registers = new List<Guid>();
            registers.Add(CR0);
            registers.Add(CR1);
            registers.Add(CR2);
            registers.Add(CR3);
            registers.Add(CR4);
            return registers;
        }
        public static List<Guid> getXMMs()
        {
            List<Guid> registers = new List<Guid>();
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

    }
}