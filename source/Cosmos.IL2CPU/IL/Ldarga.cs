using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.Assembler.x86;
using XSharp.Compiler;
using SysReflection = System.Reflection;


namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldarga)]
    public class Ldarga : ILOp
    {
        public Ldarga(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
#if false
            var xOpVar = (OpVar)aOpCode;

            var xDisplacement = Ldarg.GetArgumentDisplacement(aMethod, xOpVar.Value);

            /*
             * The function GetArgumentDisplacement() does not give the correct displacement for the Ldarga opcode
             * we need to "fix" it subtracting the argSize and 4
             */
            Type xArgType;
            if (aMethod.MethodBase.IsStatic)
            {
                xArgType = aMethod.MethodBase.GetParameters()[xOpVar.Value].ParameterType;
            }
            else
            {
                if (xOpVar.Value == 0u)
                {
                    xArgType = aMethod.MethodBase.DeclaringType;
                    if (xArgType.IsValueType)
                    {
                        xArgType = xArgType.MakeByRefType();
                    }
                }
                else
                {
                    xArgType = aMethod.MethodBase.GetParameters()[xOpVar.Value - 1].ParameterType;
                }
            }

            uint xArgRealSize = SizeOfType(xArgType);
            uint xArgSize = Align(xArgRealSize, 4);
            XS.Comment("Arg type = " + xArgType.ToString());
            XS.Comment("Arg real size = " + xArgRealSize + " aligned size = " + xArgSize);

            xDisplacement -= (int)(xArgSize - 4);
            XS.Comment("Real displacement " + xDisplacement);

            XS.Set(XSRegisters.EBX, (uint)(xDisplacement));
            XS.Set(XSRegisters.EAX, XSRegisters.EBP);
            XS.Add(XSRegisters.EAX, XSRegisters.EBX);
            XS.Push(XSRegisters.EAX);
#endif

            var xOpVar = (OpVar)aOpCode;
            DoExecute(Assembler, aMethod, xOpVar.Value);
        }

        public static void DoExecute(Cosmos.Assembler.Assembler Assembler, MethodInfo aMethod, ushort aParam)
        {
            var xDisplacement = Ldarg.GetArgumentDisplacement(aMethod, aParam);
            //var xType = Ldarg.GetArgumentType(aMethod, aParam);

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
                    if (xArgType.IsValueType)
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

            XS.Set(XSRegisters.EAX, XSRegisters.EBP);
            XS.Set(XSRegisters.EBX, (uint)(xDisplacement));
            XS.Add(XSRegisters.EAX, XSRegisters.EBX);
            XS.Push(XSRegisters.EAX);
        }
    }
}
