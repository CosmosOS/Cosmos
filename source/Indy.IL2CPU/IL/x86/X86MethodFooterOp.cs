using System;
using System.Linq;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86
{
    public class X86MethodFooterOp : MethodFooterOp
    {
        public readonly uint TotalArgsSize = 0;
        public readonly uint ReturnSize = 0;
        public readonly MethodInformation.Variable[] Locals;
        public readonly MethodInformation.Argument[] Args;
        public readonly bool DebugMode;
        public readonly bool MethodIsNonDebuggable;
        public readonly uint LocAllocItemCount;

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
                LocAllocItemCount = 0;
                if (aMethodInfo.LabelName.Contains("TestMethodThreeParams") || aMethodInfo.LabelName.Contains("TestMethodComplicated"))
                {
                    System.Diagnostics.Debugger.Break();
                }
                if (aMethodInfo.MethodData.ContainsKey(Localloc.LocAllocCountMethodDataEntry)) {
                    LocAllocItemCount = (uint)aMethodInfo.MethodData[Localloc.LocAllocCountMethodDataEntry];
                }
            }
        }

        public override void DoAssemble()
        {
            uint xArgSize = 0;
            foreach (var xItem in Args) {
                xArgSize += xItem.Size;
            }
            AssembleFooter(ReturnSize, Assembler, Locals, Args, xArgSize, DebugMode, MethodIsNonDebuggable, LocAllocItemCount);
        }

        public static void AssembleFooter(uint aReturnSize, Assembler.Assembler aAssembler, MethodInformation.Variable[] aLocals, MethodInformation.Argument[] aArgs, uint aTotalArgsSize, bool aDebugMode, bool aIsNonDebuggable, uint aLocAllocItemCount)
        {
            uint xReturnSize = aReturnSize;
            if (xReturnSize % 4 > 0)
            {
                xReturnSize += 4 - xReturnSize % 4;
            }
            new Label(EndOfMethodLabelNameNormal);
            new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceValue = 0 };
            if (aReturnSize > 0)
            {
                //var xArgSize = (from item in aArgs
                //                let xSize = item.Size + item.Offset
                //                select xSize).FirstOrDefault();
                //new Comment(String.Format("ReturnSize = {0}, ArgumentSize = {1}",
                //                          aReturnSize,
                //                          xArgSize));
                //int xOffset = 4;
                //if(xArgSize>0) {
                //    xArgSize -= xReturnSize;
                //    xOffset = xArgSize;
                //}
                int xOffset = 4;
                if (aArgs.Length > 0)
                {
                    // old code:
                    //xOffset = aArgs.First().Offset + 4;
                    //if (xOffset < 0)
                    //{
                    //    xOffset = 0;
                    //}

                    // new code:
                    xOffset = (int)((from item in aArgs
                               select item.Offset + item.Size).First());
                }
                for (int i = 0; i < xReturnSize / 4; i++)
                {
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; 
                    new CPUx86.Move {
                        DestinationReg = CPUx86.Registers.EBP,
                        DestinationIsIndirect = true,
                        DestinationDisplacement = (int)(xOffset + ((i + 1) * 4) + 4 - xReturnSize),
                        SourceReg = Registers.EAX
                    };
                }
            }
            new CPUx86.Jump { DestinationLabel = EndOfMethodLabelNameException };
            new Label(EndOfMethodLabelNameException);
            for (int i = 0; i < aLocAllocItemCount;i++ )
            {
                new CPUx86.Call { DestinationLabel = Label.GenerateLabelName(typeof(RuntimeEngine).GetMethod("Heap_Free")) };
            }
            if (aDebugMode && aIsNonDebuggable)
            {
                new CPUx86.Call { DestinationLabel = "DebugPoint_DebugResume" };
            }
            if ((from xLocal in aLocals
                 where xLocal.IsReferenceType
                 select 1).Count() > 0 || (from xArg in aArgs
                                           where xArg.IsReferenceType
                                           select 1).Count() > 0) {
                new CPUx86.Push { DestinationReg = Registers.ECX };
                Engine.QueueMethod(GCImplementationRefs.DecRefCountRef);
                foreach (MethodInformation.Variable xLocal in aLocals) {
                    if (xLocal.IsReferenceType) {
                        Op.Ldloc(aAssembler,
                                 xLocal,
                                 false);
                        new CPUx86.Call { DestinationLabel = Label.GenerateLabelName(GCImplementationRefs.DecRefCountRef) };
                    }
                }
                foreach (MethodInformation.Argument xArg in aArgs) {
                    if (xArg.IsReferenceType) {
                        Op.Ldarg(aAssembler,
                                 xArg,
                                 false);
                        new CPUx86.Call { DestinationLabel = Label.GenerateLabelName(GCImplementationRefs.DecRefCountRef) };
                    }
                }
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
            }
            for (int j = aLocals.Length - 1; j >= 0; j--)
            {
                int xLocalSize = aLocals[j].Size;
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = (uint)xLocalSize };
            }
            //new CPUx86.Add(CPUx86.Registers_Old.ESP, "0x4");
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBP };
            var xRetSize = ((int)aTotalArgsSize) - ((int)xReturnSize);
            if(xRetSize<0) {
                xRetSize = 0;
            }
            new CPUx86.Return { DestinationValue = (uint)xRetSize };
        }
    }
}