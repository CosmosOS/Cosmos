using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
    [OpCode("stosd")]
    public class StoreSD : Instruction
    {
#warning todo: merge with stosb and stosw
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode=new byte[]{0xAB}
            });
        }
    }
}
