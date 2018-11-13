using System;

using InlineIL;
using static InlineIL.IL.Emit;

using IL2CPU.API.Attribs;

namespace Cosmos.Core_Asm
{
    [Plug(Target = typeof(Array))]
    public static class ArrayImpl
    {
        public static int get_Length(Array aThis)
        {
            IL.DeclareLocals(
                false,
                ((LocalVar)typeof(Array)).Pinned());

            // push 'this'
            Ldarg_0();

            // pin the reference
            Stloc_0();
            Ldloc_0();

            // get pointer to object
            Conv_I();

            // length offset: 8
            Ldc_I4(8);
            Add();

            // load length from header
            Ldind_I4();

            // unpin the reference
            Ldnull();
            Stloc_0();

            return IL.Return<int>();
        }

        [PlugMethod(Assembler = typeof(ArrayInternalCopyAsm))]
        public static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length, bool reliable)
        {
            throw new NotImplementedException();
        }
    }
}
