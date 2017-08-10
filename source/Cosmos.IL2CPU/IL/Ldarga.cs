using System;
using System.Reflection;

using Cosmos.IL2CPU.ILOpCodes;
using XSharp.Common;
using static XSharp.Common.XSRegisters;


namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldarga)]
    public class Ldarga : ILOp
    {
        public Ldarga(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xOpVar = (OpVar)aOpCode;
            DoExecute(Assembler, aMethod, xOpVar.Value);
        }

        public static void DoExecute(Cosmos.Assembler.Assembler Assembler, _MethodInfo aMethod, ushort aParam)
        {
            var xDisplacement = Ldarg.GetArgumentDisplacement(aMethod, aParam);

            /*
             * The function GetArgumentDisplacement() does not give the correct displacement for the Ldarga opcode
             * we need to "fix" it subtracting the argSize and 4
             */
            Type xArgType;
            if (aMethod.MethodBase.IsStatic)
            {
                xArgType = aMethod.MethodBase.GetParameters()[aParam].ParameterType;
            }
            else
            {
                if (aParam == 0u)
                {
                    xArgType = aMethod.MethodBase.DeclaringType;
                    if (xArgType.GetTypeInfo().IsValueType)
                    {
                        xArgType = xArgType.MakeByRefType();
                    }
                }
                else
                {
                    xArgType = aMethod.MethodBase.GetParameters()[aParam - 1].ParameterType;
                }
            }

            uint xArgRealSize = SizeOfType(xArgType);
            uint xArgSize = Align(xArgRealSize, 4);
            XS.Comment("Arg idx = " + aParam);
            XS.Comment("Arg type = " + xArgType);
            XS.Comment("Arg real size = " + xArgRealSize + " aligned size = " + xArgSize);

            xDisplacement -= (int)(xArgSize - 4);
            XS.Comment("Real displacement " + xDisplacement);

            XS.Set(EAX, EBP);
            XS.Set(EBX, (uint)(xDisplacement));
            XS.Add(EAX, EBX);
            XS.Push(EAX);
        }
    }
}
