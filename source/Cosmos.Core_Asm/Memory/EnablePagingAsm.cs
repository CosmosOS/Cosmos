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
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            //mov eax, 0x123456A
            //mov cr3, eax

            //mov eax, cr0
            //or eax, 0x80000001
            //mov cr0, eax


            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 8); //EDX has page dir addr

            //Select our page directory
            new Mov
            {
                DestinationReg = RegistersEnum.CR3,
                SourceReg = RegistersEnum.EAX
            };

            //Set the paging bit
            new Mov
            {
                DestinationReg = RegistersEnum.EAX,
                SourceReg = RegistersEnum.CR0
            };

            new Or()
            {
                DestinationReg = RegistersEnum.EAX,
                SourceValue = 0x80000000
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
        public static void DoEnable(uint pagedir)
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
