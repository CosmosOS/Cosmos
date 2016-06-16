using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using XSharp.Compiler;
using Assembler = Cosmos.Assembler.Assembler;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.Core.Plugs
{
    [Plug(Target = typeof(Cosmos.Core.IOPortBase))]
    public class IOPortImpl
    {

        #region Write8
        private class Write8Assembler : AssemblerMethod
        {
            public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
            {
                //TODO: This is a lot of work to write to a single port.
                // We need to have some kind of inline ASM option that can
                // emit a single out instruction
                XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: 0x0C);
                XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 0x08);
                XS.WriteToPortDX(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.AL));
            }
        }
        [PlugMethod(Assembler=typeof(Write8Assembler))]
        public static void Write8(UInt16 aPort, byte aData) { }
        #endregion

        #region Write16
        private class Write16Assembler : AssemblerMethod
        {
            public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
            {
                XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBP), sourceDisplacement: 0x0C);
                XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBP), sourceDisplacement: 0x08);
                XS.WriteToPortDX(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.AX));
            }
        }
        [PlugMethod(Assembler = typeof(Write16Assembler))]
        public static void Write16(UInt16 aPort, UInt16 aData) { }
        #endregion

        #region Write32
        private class Write32Assembler : AssemblerMethod
        {
            public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
            {
                XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBP), sourceDisplacement: 0x0C);
                XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBP), sourceDisplacement: 0x08);
                XS.WriteToPortDX(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            }
        }
        [PlugMethod(Assembler = typeof(Write32Assembler))]
        public static void Write32(UInt16 aPort, UInt32 aData) { }
        #endregion

        #region Read8
        private class Read8Assembler : AssemblerMethod
        {
            public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
            {
                XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBP), sourceDisplacement: 0x08);
                //TODO: Do we need to clear rest of EAX first?
                //    MTW: technically not, as in other places, it _should_ be working with AL too..
                XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 0);
                XS.ReadFromPortDX(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.AL));
                XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            }
        }
        [PlugMethod(Assembler = typeof(Read8Assembler))]
        public static byte Read8(UInt16 aPort) { return 0; }
        #endregion

        #region Read16
        private class Read16Assembler : AssemblerMethod
        {
            public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
            {
                XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBP), sourceDisplacement: 0x08);
                XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 0);
                XS.ReadFromPortDX(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.AX));
                XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            }
        }
        [PlugMethod(Assembler = typeof(Read16Assembler))]
        public static UInt16 Read16(UInt16 aPort) { return 0; }
        #endregion

        #region Read32
        private class Read32Assembler : AssemblerMethod
        {
            public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
            {
                XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBP), sourceDisplacement: 0x08);
                XS.ReadFromPortDX(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            }
        }
        [PlugMethod(Assembler = typeof(Read32Assembler))]
        public static UInt32 Read32(UInt16 aPort) { return 0; }
        #endregion

    }
}
