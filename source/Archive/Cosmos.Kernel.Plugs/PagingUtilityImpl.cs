using System;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs
{
    [Plug(Target=typeof(Kernel.PagingUtility))]
    public class PagingUtility
    {
        [PlugMethod(Assembler=typeof(Assemblers.ASMEnablePaging))]
        public static void EnablePaging()
        {
            //Assembler
        }
        [PlugMethod(Assembler=typeof(Assemblers.ASMDisablePaging))]
        public static void DisablePaging()
        {
            //Assembler
        }

        [PlugMethod(Assembler=typeof(Assemblers.ASMEnablePSE))]
        public static void EnablePSE()
        {
            //Assembler
        }
        [PlugMethod(Assembler=typeof(Assemblers.ASMDisablePSE))]
        public static void DisablePSE()
        {
            //Assembler
        }

        [PlugMethod(Assembler=typeof(Assemblers.ASMSetPageDirectory))]
        public static void SetPageDirectory(uint Address)
        {
            //Assembler
        }
    }
}