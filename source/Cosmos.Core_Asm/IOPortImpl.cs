using System.Security.Cryptography;
using Cosmos.Core;
using IL2CPU.API;
using IL2CPU.API.Attribs;

using XSharp;
using XSharp.Assembler;

namespace Cosmos.Core_Asm
{
    [Plug(Target = typeof(IOPort))]
    public class IOPortImpl
    {

        #region Write8

        private class Write8Assembler : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                //TODO: This is a lot of work to write to a single port.
                // We need to have some kind of inline ASM option that can
                // emit a single out instruction
                XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: 0x0C);
                XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 0x08);
                XS.WriteToPortDX(XSRegisters.AL);
            }
        }

        [PlugMethod(Assembler = typeof(Write8Assembler))]
        public static void Write8(ushort aPort, byte aData) => throw null;

        #endregion

        #region WriteMany8WithWait (many)

        private class WriteMany8WithWaitAssembler : AssemblerMethod {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo) {
                // the port index is in EBP+8
                // the reference to the byte array is in EBP+12
                XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: 16); // EDX = Port (ebp+16)
                XS.Set(XSRegisters.ECX, XSRegisters.EBP, sourceDisplacement: 12); // ECX = Pointer to array (ebp+12)

                XS.Lea(XSRegisters.ESI, XSRegisters.ECX, sourceDisplacement: 16); // ESI = Data* (ecx+16)
                XS.Set(XSRegisters.EBX, XSRegisters.ECX, sourceDisplacement: 8); // EBX = Length (ecx+8)

                XS.Label(".loop");
                XS.Set(XSRegisters.AX, XSRegisters.ESI, sourceIsIndirect: true); // ax = *esi
                XS.WriteToPortDX(XSRegisters.AX);
                XS.LiteralCode("out 0x80, al");
                XS.LiteralCode("out 0x80, al");
                XS.LiteralCode("out 0x80, al");
                XS.LiteralCode("out 0x80, al"); // Wait 400 ns between each word
                XS.Add(XSRegisters.ESI, 2); // esi++

                XS.Sub(XSRegisters.EBX, 2); // ebx--
                XS.Jump(XSharp.Assembler.x86.ConditionalTestEnum.NotZero, ".loop"); // if (ebx != 0) goto .loop
            }
        }

        [PlugMethod(Assembler = typeof(WriteMany8WithWaitAssembler))]
        public static void WriteMany8WithWait(ushort aPort, byte[] aData) => throw null;

        #endregion
        
        #region Write16

        private class Write16Assembler : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: 0x0C);
                XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 0x08);
                XS.WriteToPortDX(XSRegisters.AX);
            }
        }

        [PlugMethod(Assembler = typeof(Write16Assembler))]
        public static void Write16(ushort aPort, ushort aData) => throw null;

        #endregion

        #region Write32

        private class Write32Assembler : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: 0x0C);
                XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 0x08);
                XS.WriteToPortDX(XSRegisters.EAX);
            }
        }

        [PlugMethod(Assembler = typeof(Write32Assembler))]
        public static void Write32(ushort aPort, uint aData) => throw null;

        #endregion

        #region Read8

        private class Read8Assembler : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: 0x08);
                //TODO: Do we need to clear rest of EAX first?
                //    MTW: technically not, as in other places, it _should_ be working with AL too..
                XS.Set(XSRegisters.EAX, 0);
                XS.ReadFromPortDX(XSRegisters.AL);
                XS.Push(XSRegisters.EAX);
            }
        }

        [PlugMethod(Assembler = typeof(Read8Assembler))]
        public static byte Read8(ushort aPort) => throw null;

        #endregion

        #region Read8 (many)

        private class Read8AssemblerMany : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                // the port index is in EBP+16
                // the reference to the byte array is in EBP+12
                XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: 16); // EDX = Port (ebp+16)
                XS.Set(XSRegisters.ECX, XSRegisters.EBP, sourceDisplacement: 12); // ECX = Pointer to array (ebp+12)

                XS.Lea(XSRegisters.ESI, XSRegisters.ECX, sourceDisplacement: 16); // ESI = Data* (ecx+16)
                XS.Set(XSRegisters.EBX, XSRegisters.ECX, sourceDisplacement: 8); // EBX = Length (ecx+8)
                
                XS.Label(".loop");
                XS.ReadFromPortDX(XSRegisters.AX);
                XS.Set(XSRegisters.ESI, XSRegisters.AX, destinationIsIndirect: true); // *esi = ax
                XS.Add(XSRegisters.ESI, 2); // esi++

                XS.Sub(XSRegisters.EBX, 2); // ebx--
                XS.Jump(XSharp.Assembler.x86.ConditionalTestEnum.NotZero, ".loop"); // if (ebx != 0) goto .loop
            }
        }

        [PlugMethod(Assembler = typeof(Read8AssemblerMany))]
        public static void Read8(ushort aPort, byte[] aData) => throw null;

        #endregion

        #region Read16

        private class Read16Assembler : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: 0x08);
                XS.Set(XSRegisters.EAX, 0);
                XS.ReadFromPortDX(XSRegisters.AX);
                XS.Push(XSRegisters.EAX);
            }
        }

        [PlugMethod(Assembler = typeof(Read16Assembler))]
        public static ushort Read16(ushort aPort) => throw null;

        #endregion

        #region Read16 (many)

        private class Read16ManyAssembler : AssemblerMethod {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo) {
                // the port index is in EBP+16
                // the reference to the byte array is in EBP+12
                XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: 16); // EDX = Port (ebp+16)
                XS.Set(XSRegisters.ECX, XSRegisters.EBP, sourceDisplacement: 12); // ECX = Pointer to array (ebp+12)

                XS.Lea(XSRegisters.ESI, XSRegisters.ECX, sourceDisplacement: 16); // ESI = Data* (ecx+16)
                XS.Set(XSRegisters.EBX, XSRegisters.ECX, sourceDisplacement: 8); // EBX = Length (ecx+8)

                XS.Label(".loop");
                XS.ReadFromPortDX(XSRegisters.AX);
                XS.Set(XSRegisters.ESI, XSRegisters.AX, destinationIsIndirect: true); // *esi = ax
                XS.Add(XSRegisters.ESI, 2); // esi++

                XS.Sub(XSRegisters.EBX, 1); // ebx--
                XS.Jump(XSharp.Assembler.x86.ConditionalTestEnum.NotZero, ".loop"); // if (ebx != 0) goto .loop
            }
        }

        [PlugMethod(Assembler = typeof(Read16ManyAssembler))]
        public static void Read16(ushort aPort, ushort[] aData) => throw null;

        #endregion

        #region Read32

        private class Read32Assembler : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                XS.Set(XSRegisters.EDX, XSRegisters.EBP, sourceDisplacement: 0x08);
                XS.ReadFromPortDX(XSRegisters.EAX);
                XS.Push(XSRegisters.EAX);
            }
        }

        [PlugMethod(Assembler = typeof(Read32Assembler))]
        public static uint Read32(ushort aPort) => throw null;

        #endregion

    }
}
