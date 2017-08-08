using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RE = Cosmos.Assembler.X86.RegistersEnum;
namespace Cosmos.Assembler.X86
{
    public static class Registers
    {

        static RE[] Reg_CR      = { RE.CR0,  RE.CR1,  RE.CR2,  RE.CR3,  RE.CR4,  RE.CR5,  RE.CR6,   RE.CR7,  RE.CR8,  RE.CR9,  RE.CR10,  RE.CR11,  RE.CR12,  RE.CR13,  RE.CR14,  RE.CR15 };
        static RE[] Reg_DR      = { RE.DR0,  RE.DR1,  RE.DR2,  RE.DR3,  RE.DR4,  RE.DR5,  RE.DR6,   RE.DR7,  RE.DR8,  RE.DR9,  RE.DR10,  RE.DR11,  RE.DR12,  RE.DR13,  RE.DR14,  RE.DR15 };
        static RE[] Reg_FPU     = { RE.ST0,  RE.ST1,  RE.ST2,  RE.ST3,  RE.ST4,  RE.ST5,  RE.ST6,   RE.ST7 };
        static RE[] Reg_MMX     = { RE.MM0,  RE.MM1,  RE.MM2,  RE.MM3,  RE.MM4,  RE.MM5,  RE.MM6,   RE.MM7 };
        static RE[] Reg8        = { RE.AL,   RE.CL,   RE.DL,   RE.BL,   RE.AH,   RE.CH,   RE.DH,    RE.BH };
        static RE[] Reg8_rex    = { RE.AL,   RE.CL,   RE.DL,   RE.BL,   RE.SPL,  RE.BPL,  RE.SIL,   RE.DIL,  RE.R8B,  RE.R9B,  RE.R10B,  RE.R11B,  RE.R12B,  RE.R13B,  RE.R14B,  RE.R15B };
        static RE[] Reg16       = { RE.AX,   RE.CX,   RE.DX,   RE.BX,   RE.SP,   RE.BP,   RE.SI,    RE.DI,   RE.R8W,  RE.R9W,  RE.R10W,  RE.R11W,  RE.R12W,  RE.R13W,  RE.R14W,  RE.R15W };
        static RE[] Reg32       = { RE.EAX,  RE.ECX,  RE.EDX,  RE.EBX,  RE.ESP,  RE.EBP,  RE.ESI,   RE.EDI,  RE.R8D,  RE.R9D,  RE.R10D,  RE.R11D,  RE.R12D,  RE.R13D,  RE.R14D,  RE.R15D };
        static RE[] Reg64       = { RE.RAX,  RE.RCX,  RE.RDX,  RE.RBX,  RE.RSP,  RE.RBP,  RE.RSI,   RE.RDI,  RE.R8,   RE.R9,   RE.R10,   RE.R11,   RE.R12,   RE.R13,   RE.R14,   RE.R15 };
        static RE[] Reg_Seg     = { RE.ES,   RE.CS,   RE.SS,   RE.DS,   RE.FS,   RE.GS,   RE.SEGR6, RE.SEGR7 };
        static RE[] Reg_TR      = { RE.TR0,  RE.TR1,  RE.TR2,  RE.TR3,  RE.TR4,  RE.TR5,  RE.TR6,   RE.TR7 };
        static RE[] Reg_XMM     = { RE.XMM0, RE.XMM1, RE.XMM2, RE.XMM3, RE.XMM4, RE.XMM5, RE.XMM6,  RE.XMM7, RE.XMM8, RE.XMM9, RE.XMM10, RE.XMM11, RE.XMM12, RE.XMM13, RE.XMM14, RE.XMM15 };
        static RE[] Reg_YMM     = { RE.YMM0, RE.YMM1, RE.YMM2, RE.YMM3, RE.YMM4, RE.YMM5, RE.YMM6,  RE.YMM7, RE.YMM8, RE.YMM9, RE.YMM10, RE.YMM11, RE.YMM12, RE.YMM13, RE.YMM14, RE.YMM15 };

        /// <summary>
        /// Key = 32bit (eg EAX), value = 16 bit (eg AX)
        /// </summary>
        //private static Dictionary<RegistersEnum, RegistersEnum> m32BitTo16BitMapping = new Dictionary<RegistersEnum, RegistersEnum>();
        /// <summary>
        /// Key = 32bit (eg EAX), value = 16 bit (eg AL). 
        /// 
        /// </summary>
        //private static Dictionary<RegistersEnum, RegistersEnum> m32BitTo8BitMapping = new Dictionary<RegistersEnum, RegistersEnum>();
        //private static Dictionary<RegistersEnum, RegistersEnum> m16BitTo8BitMapping = new Dictionary<RegistersEnum, RegistersEnum>();

        private static Dictionary<RegistersEnum, string> mRegToName = new Dictionary<RegistersEnum, string>();
        private static Dictionary<string, RegistersEnum> mNameToReg = new Dictionary<string, RegistersEnum>();

        static Registers()
        {
            foreach( RegistersEnum element in Enum.GetValues( typeof( RegistersEnum ) ) )
            {
                mRegToName.Add( element, element.ToString().ToLower() );
                mNameToReg.Add( element.ToString().ToLower(), element );
            }
        }

        // Just look at the register array to find a proper mapping
        public static RegistersEnum? Get8BitRegistersForRegister( RegistersEnum aReg )
        {
            if( Is32Bit( aReg ) )
            {
                //if( m32BitTo8BitMapping.ContainsKey( aReg ) )
                //{
                //    return m32BitTo8BitMapping[ aReg ];
                //}
                return null;
            }
            if( Is16Bit( aReg ) )
            {
                //if( m16BitTo8BitMapping.ContainsKey( aReg ) )
                //{
                //    return m16BitTo8BitMapping[ aReg ];
                //}
                return null;
            }
            if( Is128Bit( aReg ) )
            {
                throw new Exception( "128bit registers don't have 8bit variants!" );
            }
            return aReg;
        }

        public static bool IsCR( RegistersEnum aReg )
        {
            return Reg_CR.Contains( aReg );
        }

        public static RegistersEnum? Get16BitRegisterForRegister( RegistersEnum aReg )
        {
            if( Is32Bit( aReg ) )
            {
                //if( m32BitTo16BitMapping.ContainsKey( aReg ) )
                //{
                //    return m32BitTo16BitMapping[ aReg ];
                //}
                return null;
            }
            if( Is128Bit( aReg ) )
            {
                throw new Exception( "128bit registers don't have 8bit variants!" );
            }
            if( Is16Bit( aReg ) )
            {
                return aReg;
            }
            //if( m16BitTo8BitMapping.ContainsKey( aReg ) )
            //{
            //    return m16BitTo8BitMapping[ aReg ];
            //}
            return aReg;
        }

        public static RegistersEnum? Get32BitRegisterForRegister( RegistersEnum aReg )
        {
            if( Is32Bit( aReg ) )
            {
                return aReg;
            }
            if( Is128Bit( aReg ) )
            {
                throw new Exception( "128bit registers don't have 32bit variants!" );
            }
            if( Is16Bit( aReg ) )
            {
                //if( m32BitTo16BitMapping.ContainsValue( aReg ) )
                //{
                //    return ( from item in m32BitTo16BitMapping
                //             where item.Value == aReg
                //             select item.Key ).Single();
                //}
                return null;
            }
            //if( m32BitTo8BitMapping.ContainsValue( aReg ) )
            //{
            //    return ( from item in m32BitTo8BitMapping
            //             where item.Value == aReg
            //             select item.Key ).Single();
            //}
            return null;
        }

        public static string GetRegisterName( RegistersEnum aRegister )
        {
            return mRegToName[ aRegister ];
        }

        public static RegistersEnum? GetRegister( string aName )
        {
            if( mNameToReg.ContainsKey( aName ) )
            {
                return mNameToReg[ aName ];
            }
            else
            {
                return null;
            }
        }

        public static byte GetSize( RegistersEnum aRegister )
        {
            if( Is128Bit( aRegister ) ) { return 128; }
            if( Is32Bit( aRegister ) ) { return 32; }
            if( Is16Bit( aRegister ) ) { return 16; }
            if( Is8Bit( aRegister ) ) { return 8; }
            throw new NotImplementedException();
        }

        public static bool Is8Bit( RegistersEnum aRegister )
        {
            return Reg8.Contains( aRegister );   
        }

        //public static Guid Get 

        public static bool Is128Bit( RegistersEnum aRegister )
        {
            return Reg_XMM.Contains( aRegister ) || Reg_YMM.Contains( aRegister );   
        }

        public static bool IsSegment( RegistersEnum aRegister )
        {
            return Reg_Seg.Contains( aRegister );  
        }

        public static bool Is32Bit( RegistersEnum aRegister )
        {
            return Reg32.Contains( aRegister );   
        }

        public static bool Is64Bit( RegistersEnum aRegister )
        {
            return Reg64.Contains( aRegister );
        }

        public static bool Is16Bit( RegistersEnum aRegister )
        {
            return Reg16.Contains( aRegister );
        }

        public static List<RegistersEnum> GetRegisters()
        {
            List<RegistersEnum> registers = new List<RegistersEnum>();
            foreach( RegistersEnum xField in Enum.GetValues( typeof( RegistersEnum ) ) )
            {
                registers.Add( xField );
            }

            return registers;
        }

        public static List<RegistersEnum> GetReg8()
        {
            return Reg8.ToList();
        }

        public static List<RegistersEnum> GetReg16()
        {
            return Reg16.ToList();
        }

        public static List<RegistersEnum> GetReg32()
        {
            return Reg32.ToList();
        }

        public static List<RegistersEnum> GetReg64()
        {
            return Reg64.ToList();
        }

        public static List<RegistersEnum> GetRegCR()
        {
            return Reg_CR.ToList();
        }

        public static List<RegistersEnum> GetRegXMM()
        {
            return Reg_XMM.ToList();
        }

        public static List<RegistersEnum> GetRegYMM()
        {
            return Reg_YMM.ToList();
        }

    }
}