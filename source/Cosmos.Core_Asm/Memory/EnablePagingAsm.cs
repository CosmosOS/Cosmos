using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;
using XSharp;
using XSharp.Assembler;
using XSharp.Assembler.x86;

namespace Cosmos.Core_Asm.Memory
{
    class EnablePagingAsm : AssemblerMethod
    {
        //static void DoEnable(uint addr)
        //addr: EBP+8
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            //EAX: adddr
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 8);
            XS.Set(XSRegisters.CR3, XSRegisters.EAX);
            //Set the paging bit
            new Mov
            {
                DestinationReg = RegistersEnum.EAX,
                SourceReg = RegistersEnum.CR0
            };

            new Or()
            {
                DestinationReg = RegistersEnum.EAX,
                SourceValue = 0x80000001
            };

            new Mov
            {
                DestinationReg = RegistersEnum.CR0,
                SourceReg = RegistersEnum.EAX
            };

  
        }
    }
    class RefreshPagesAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            //reload CR3
            new Mov
            {
                DestinationReg = RegistersEnum.EAX,
                SourceReg = RegistersEnum.CR3
            };
            new Mov
            {
                DestinationReg = RegistersEnum.CR3,
                SourceReg = RegistersEnum.EAX
            };
        }
    }
    [Plug(Target = typeof(Core.Paging))]
    public class PagingImpl
    {
        [PlugMethod(Assembler = typeof(EnablePagingAsm))]
        public static void DoEnable(uint addr)
        {
            throw new NotImplementedException();
        }
        [PlugMethod(Assembler = typeof(RefreshPagesAsm))]
        public static void RefreshPages()
        {
            throw new NotImplementedException();
        }
    }
}
