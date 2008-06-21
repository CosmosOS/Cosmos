using System;
using System.Linq;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86
{
    public class X86MethodFooterOp : MethodFooterOp
    {
        public readonly int TotalArgsSize = 0;
        public readonly int ReturnSize = 0;
        public readonly MethodInformation.Variable[] Locals;
        public readonly MethodInformation.Argument[] Args;
        public readonly bool DebugMode;
        public readonly bool MethodIsNonDebuggable;

        public X86MethodFooterOp(ILReader aReader, MethodInformation aMethodInfo)
            : base(aReader, aMethodInfo)
        {
            if (aMethodInfo != null)
            {
                //			if (aMethodInfo.Locals.Length > 0) {
                //				TotalLocalsSize += aMethodInfo.Locals[aMethodInfo.Locals.Length - 1].Offset + aMethodInfo.Locals[aMethodInfo.Locals.Length - 1].Size;
                //			}
                Locals = aMethodInfo.Locals.ToArray();
                Args = aMethodInfo.Arguments.ToArray();
                ReturnSize = aMethodInfo.ReturnSize;
                DebugMode = aMethodInfo.DebugMode;
                MethodIsNonDebuggable = aMethodInfo.IsNonDebuggable;
            }
        }

        public override void DoAssemble()
        {
            AssembleFooter(ReturnSize, Assembler, Locals, Args, (from item in Args
                                                                 select item.Size).Sum(), DebugMode, MethodIsNonDebuggable);
        }

        public static void AssembleFooter(int aReturnSize, Assembler.Assembler aAssembler, MethodInformation.Variable[] aLocals, MethodInformation.Argument[] aArgs, int aTotalArgsSize, bool aDebugMode, bool aIsNonDebuggable)
        {
            int xReturnSize = aReturnSize;
            if (xReturnSize % 4 > 0)
            {
                xReturnSize += 4 - xReturnSize % 4;
            }
            new Label(EndOfMethodLabelNameNormal);
            new CPUx86.Move("ecx", "0");
            if (aReturnSize > 0)
            {
                int xOffset = 4;
                if (aArgs.Length > 0)
                {
                    xOffset = aArgs.First().Offset+4;
                    if (xOffset < 0) {
                        xOffset = 0; }
                }
                for (int i = 0; i < xReturnSize / 4; i++)
                {
                    new Assembler.X86.Pop(Registers.EAX);
                    new CPUx86.Move("[ebp + " + (xOffset + ((i + 1) * 4) + 4 - xReturnSize) + "]",
                                    Registers.EAX);
                }
            }
            new CPUx86.Jump(EndOfMethodLabelNameException);
            new Label(EndOfMethodLabelNameException);
            if (aDebugMode && aIsNonDebuggable)
            {
                new CPUx86.Call("DebugPoint_DebugResume");
            }
            if ((from xLocal in aLocals
                 where xLocal.IsReferenceType
                 select 1).Count() > 0 || (from xArg in aArgs
                                           where xArg.IsReferenceType
                                           select 1).Count() > 0) {
                new CPUx86.Push("ecx");
                Engine.QueueMethod(GCImplementationRefs.DecRefCountRef);
                foreach (MethodInformation.Variable xLocal in aLocals) {
                    if (xLocal.IsReferenceType) {
                        Op.Ldloc(aAssembler,
                                 xLocal,
                                 false);
                        new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.DecRefCountRef));
                    }
                }
                foreach (MethodInformation.Argument xArg in aArgs) {
                    if (xArg.IsReferenceType) {
                        Op.Ldarg(aAssembler,
                                 xArg,
                                 false);
                        new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.DecRefCountRef));
                    }
                }
                new CPUx86.Pop("ecx");
            }
            for (int j = aLocals.Length - 1; j >= 0; j--)
            {
                int xLocalSize = aLocals[j].Size;
                new CPUx86.Add(CPUx86.Registers.ESP, "0x" + xLocalSize.ToString("X"));
            }
            //new CPUx86.Add(CPUx86.Registers.ESP, "0x4");
            new CPUx86.Popd(CPUx86.Registers.EBP);
            new CPUx86.Ret(aTotalArgsSize - xReturnSize);
        }
    }
}