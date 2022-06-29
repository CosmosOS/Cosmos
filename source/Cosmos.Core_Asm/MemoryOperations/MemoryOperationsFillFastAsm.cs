using XSharp.Assembler;
using XSharp.Assembler.x86;
using XSharp;
using static XSharp.XSRegisters;

namespace Cosmos.Core_Asm.MemoryOperations
{
    public class MemoryOperationsFill16BlocksAsm : AssemblerMethod
    {
        private const int DestDisplacement = 16;
        private const int ValueDisplacement = 12;
        private const int BlocksNumDisplacement = 8;

        /*
         *
         * public static unsafe void Fill16Blocks(
         *                       byte *dest, [ebp + 16]
         *                       int value, [ebp + 12]
         *                       int BlocksNum) [ebp + 8]
         */
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            /* First we copy dest, value and DestSize from EBP (stack) to 3 different registers */
            XS.Comment("Destination (int pointer)");
            XS.Set(EAX, EBP, sourceDisplacement: DestDisplacement);

            XS.Comment("Value");
            XS.Set(EBX, EBP, sourceDisplacement: ValueDisplacement);

            XS.Comment("BlocksNum");
            XS.Set(ECX, EBP, sourceDisplacement: BlocksNumDisplacement);

            /*
             * Now we need to copy 'value' (EBX) to an SSE register but we should not simply do a copy (!)
             * but all the register with 'value' repeating!
             * That is in the 16 byte SSE register should go this repeating pattern:
             * |value|value|value|value
             * luckily we don't need to do a loop for this there is the SSE3 instruction for this shufps
             */
            XS.MoveD(XMM0, EBX);
            XS.SSE.Shufps(XMM0, XMM0, 0x0000); // This broadcast the first element of XMM0 on the other 3

            /* Do the 'loop' */
            XS.Xor(EDI, EDI); // EDI is 0
            XS.Label(".loop");
            //XS.SSE.MoveUPS(EAX, XMM0, destinationIsIndirect: true, destinationDisplacement: EDI);
            XS.LiteralCode("movups[EAX + EDI], XMM0");
            XS.Add(EDI, 16);
            XS.Sub(ECX, 1);
            //XS.LiteralCode("jnz .loop");
            XS.Jump(ConditionalTestEnum.NotZero, ".loop");

            //XS.Return();
        }
    }
}
