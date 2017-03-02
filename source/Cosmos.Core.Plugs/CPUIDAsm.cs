using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.IL2CPU.Plugs;
using XSharp.Compiler;

namespace Cosmos.Core.Plugs
{
        public class CPUIDAsm : AssemblerMethod
        {
            public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
            {
                XS.Pushfd();
                XS.Pushfd();
                XS.Xor(XSRegisters.ESP, 0x00200000, destinationIsIndirect: true);
                XS.Popfd();
                XS.Pushfd();
                XS.Pop(XSRegisters.EAX);
                XS.Xor(XSRegisters.EAX, XSRegisters.ESP, destinationIsIndirect: true);
                XS.Popfd();
                XS.And(XSRegisters.EAX, 0x00200000);
                XS.Set(XSRegisters.EAX, 1);
                XS.Push(XSRegisters.EAX);
            }
        }
}
