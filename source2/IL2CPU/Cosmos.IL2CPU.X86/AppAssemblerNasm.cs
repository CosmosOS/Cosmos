using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;
using System.Reflection;
using System.Diagnostics.SymbolStore;
using Microsoft.Samples.Debugging.CorSymbolStore;
using Cosmos.Debug.Common;
using Cosmos.Build.Common;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.IL2CPU.X86
{
    public class AppAssemblerNasm : AppAssembler
    {
        public AppAssemblerNasm(byte comNumber)
            : base(comNumber)
        {
        }
        protected override void InitILOps()
        {
            InitILOps(typeof(ILOp));
        }

        private IList<DebugInfo.Local_Argument_Info> mLocals_Arguments_Infos = new List<DebugInfo.Local_Argument_Info>();

        protected override void MethodBegin(MethodInfo aMethod)
        {
            base.MethodBegin(aMethod);
            if (aMethod.PluggedMethod != null)
            {
                new Label("PLUG_FOR___" + MethodInfoLabelGenerator.GenerateLabelName(aMethod.PluggedMethod.MethodBase));
            }
            else
            {
                new Label(aMethod.MethodBase);
            }
            var xMethodLabel = Label.LastFullLabel;
            if (aMethod.MethodBase.IsStatic && aMethod.MethodBase is ConstructorInfo)
            {
                new Comment("This is a static constructor. see if it has been called already, and if so, return.");
                var xName = DataMember.FilterStringForIncorrectChars("CCTOR_CALLED__" + MethodInfoLabelGenerator.GetFullName(aMethod.MethodBase.DeclaringType));
                var xAsmMember = new DataMember(xName, (byte)0);
                Assembler.DataMembers.Add(xAsmMember);
                new Compare { DestinationRef = ElementReference.New(xName), DestinationIsIndirect = true, Size = 8, SourceValue = 1 };
                new ConditionalJump { Condition = ConditionalTestEnum.Equal, DestinationLabel = ".BeforeQuickReturn" };
                new Move { DestinationRef = ElementReference.New(xName), DestinationIsIndirect = true, Size = 8, SourceValue = 1 };
                new Jump { DestinationLabel = ".AfterCCTorAlreadyCalledCheck" };
                new Label(".BeforeQuickReturn");
                new Move { DestinationReg = RegistersEnum.ECX, SourceValue = 0 };
                new Return { };
                new Label(".AfterCCTorAlreadyCalledCheck");
            }

            new Push { DestinationReg = Registers.EBP };
            new Move { DestinationReg = Registers.EBP, SourceReg = Registers.ESP };
            //new CPUx86.Push("0");
            //if (!(aLabelName.Contains("Cosmos.Kernel.Serial") || aLabelName.Contains("Cosmos.Kernel.Heap"))) {
            //    new CPUx86.Push(LdStr.GetContentsArrayName(aAssembler, aLabelName));
            //    MethodBase xTempMethod = Engine.GetMethodBase(Engine.GetType("Cosmos.Kernel", "Cosmos.Kernel.Serial"), "Write", "System.Byte", "System.String");
            //    new CPUx86.Call(MethodInfoLabelGenerator.GenerateLabelName(xTempMethod));
            //    Engine.QueueMethod(xTempMethod);
            //}
            if (aMethod.MethodAssembler == null && aMethod.PlugMethod == null)
            {
                // the body of aMethod is getting emitted
                var xBody = aMethod.MethodBase.GetMethodBody();
                if (xBody != null)
                {
                    foreach (var xLocal in xBody.LocalVariables)
                    {
                        mLocals_Arguments_Infos.Add(new DebugInfo.Local_Argument_Info
                        {
                            MethodLabelName = xMethodLabel,
                            IsArgument = false,
                            Index = xLocal.LocalIndex,
                            Offset = (int)ILOp.GetEBPOffsetForLocal(aMethod, xLocal.LocalIndex)
                        });
                        new Comment("Local " + xLocal.LocalIndex);
                        new Sub { DestinationReg = Registers.ESP, SourceValue = ILOp.Align(ILOp.SizeOfType(xLocal.LocalType), 4) };
                    }
                }

                // debug info:
                var xIdxOffset = 0u;
                if(!aMethod.MethodBase.IsStatic){
                    mLocals_Arguments_Infos.Add(new DebugInfo.Local_Argument_Info{
                        MethodLabelName=xMethodLabel,
                        IsArgument=true,
                        Index=0,
                        Offset=IL.Ldarg.GetArgumentDisplacement(aMethod, 0)
                    });
                    xIdxOffset++;
                }

                var xParamCount = (ushort)aMethod.MethodBase.GetParameters().Length;
                for (ushort i = 0; i < xParamCount; i++)
                {
                    mLocals_Arguments_Infos.Add(new DebugInfo.Local_Argument_Info
                    {
                        MethodLabelName = xMethodLabel,
                        IsArgument = true,
                        Index = (int)(i + xIdxOffset),
                        Offset = (int)IL.Ldarg.GetArgumentDisplacement(aMethod, (ushort)(i + xIdxOffset))
                    });
                }
            }

            //foreach (var xLocal in aLocals) {
            //  aAssembler.StackContents.Push(new StackContent(xLocal.Size, xLocal.VariableType));
            //  for (int i = 0; i < (xLocal.Size / 4); i++) {
            //    new CPUx86.Push { DestinationValue = 0 };
            //  }
            //}
            //if (aDebugMode && aIsNonDebuggable) {
            //  new CPUx86.Call { DestinationLabel = "DebugPoint_DebugSuspend" };
            //}
            #region Load CodeOffset
            if (DebugMode == DebugMode.Source)
            {
                var xSymbolReader = GetSymbolReaderForAssembly(aMethod.MethodBase.DeclaringType.Assembly);
                if (xSymbolReader != null)
                {
                    var xSmbMethod = xSymbolReader.GetMethod(new SymbolToken(aMethod.MethodBase.MetadataToken));
                    // This gets the Sequence Points.
                    // Sequence Points are spots that identify what the compiler/debugger says is a spot
                    // that a breakpoint can occur one. Essentially, an atomic source line in C#
                    if (xSmbMethod != null)
                    {
                        xCodeOffsets = new int[xSmbMethod.SequencePointCount];
                        var xCodeDocuments = new ISymbolDocument[xSmbMethod.SequencePointCount];
                        xCodeLineNumbers = new int[xSmbMethod.SequencePointCount];
                        var xCodeColumns = new int[xSmbMethod.SequencePointCount];
                        var xCodeEndLines = new int[xSmbMethod.SequencePointCount];
                        var xCodeEndColumns = new int[xSmbMethod.SequencePointCount];
                        xSmbMethod.GetSequencePoints(xCodeOffsets, xCodeDocuments
                         , xCodeLineNumbers, xCodeColumns, xCodeEndLines, xCodeEndColumns);
                    }
                }
            }
            #endregion
        }

        protected override void MethodEnd(MethodInfo aMethod)
        {
            base.MethodEnd(aMethod);
            uint xReturnSize = 0;
            var xMethInfo = aMethod.MethodBase as System.Reflection.MethodInfo;
            if (xMethInfo != null)
            {
                xReturnSize = ILOp.Align(ILOp.SizeOfType(xMethInfo.ReturnType), 4);
            }
            new Label(ILOp.GetMethodLabel(aMethod) + EndOfMethodLabelNameNormal);
            new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceValue = 0 };
            var xTotalArgsSize = (from item in aMethod.MethodBase.GetParameters()
                                  select (int)ILOp.Align(ILOp.SizeOfType(item.ParameterType), 4)).Sum();
            if (!aMethod.MethodBase.IsStatic)
            {
                if (aMethod.MethodBase.DeclaringType.IsValueType)
                {
                    xTotalArgsSize += 4; // only a reference is passed
                }
                else
                {
                    xTotalArgsSize += (int)ILOp.Align(ILOp.SizeOfType(aMethod.MethodBase.DeclaringType), 4);
                }
            }

            if (aMethod.PluggedMethod != null)
            {
                xReturnSize = 0;
                xMethInfo = aMethod.PluggedMethod.MethodBase as System.Reflection.MethodInfo;
                if (xMethInfo != null)
                {
                    xReturnSize = ILOp.Align(ILOp.SizeOfType(xMethInfo.ReturnType), 4);
                }
                xTotalArgsSize = (from item in aMethod.PluggedMethod.MethodBase.GetParameters()
                                  select (int)ILOp.Align(ILOp.SizeOfType(item.ParameterType), 4)).Sum();
                if (!aMethod.PluggedMethod.MethodBase.IsStatic)
                {
                    if (aMethod.PluggedMethod.MethodBase.DeclaringType.IsValueType)
                    {
                        xTotalArgsSize += 4; // only a reference is passed
                    }
                    else
                    {
                        xTotalArgsSize += (int)ILOp.Align(ILOp.SizeOfType(aMethod.PluggedMethod.MethodBase.DeclaringType), 4);
                    }
                }
            }

            if (xReturnSize > 0)
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
                var xOffset = GetResultCodeOffset(xReturnSize, (uint)xTotalArgsSize);
                for (int i = 0; i < xReturnSize / 4; i++)
                {
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Move
                    {
                        DestinationReg = CPUx86.Registers.EBP,
                        DestinationIsIndirect = true,
                        DestinationDisplacement = (int)(xOffset + ((i + 0) * 4)),
                        SourceReg = Registers.EAX
                    };
                    //        new CPUx86.Move {
                    //            DestinationReg = CPUx86.Registers.EBP,
                    //            DestinationIsIndirect = true,
                    //            DestinationDisplacement = (int)(xOffset + ((i + 1) * 4) + 4 - xReturnSize),
                    //            SourceReg = Registers.EAX
                    //        };
                }
                // extra stack space is the space reserved for example when a "public static int TestMethod();" method is called, 4 bytes is pushed, to make room for result;
            }
            new Label(ILOp.GetMethodLabel(aMethod) + EndOfMethodLabelNameException);
            //for (int i = 0; i < aLocAllocItemCount; i++) {
            //  new CPUx86.Call { DestinationLabel = aHeapFreeLabel };
            //}
            //if (aDebugMode && aIsNonDebuggable) {
            //  new CPUx86.Call { DestinationLabel = "DebugPoint_DebugResume" };
            //}

            //if ((from xLocal in aLocals
            //     where xLocal.IsReferenceType
            //     select 1).Count() > 0 || (from xArg in aArgs
            //                               where xArg.IsReferenceType
            //                               select 1).Count() > 0) {
            //  new CPUx86.Push { DestinationReg = Registers.ECX };
            //  //foreach (MethodInformation.Variable xLocal in aLocals) {
            //  //  if (xLocal.IsReferenceType) {
            //  //    Op.Ldloc(aAssembler,
            //  //             xLocal,
            //  //             false,
            //  //             aGetStorageSizeDelegate(xLocal.VariableType));
            //  //    new CPUx86.Call { DestinationLabel = aDecRefLabel };
            //  //  }
            //  //}
            //  //foreach (MethodInformation.Argument xArg in aArgs) {
            //  //  if (xArg.IsReferenceType) {
            //  //    Op.Ldarg(aAssembler,
            //  //             xArg,
            //  //             false);
            //  //    //,                                 aGetStorageSizeDelegate(xArg.ArgumentType)
            //  //    new CPUx86.Call { DestinationLabel = aDecRefLabel };
            //  //  }
            //  //}
            //  // todo: add GC code
            //  new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
            //}
            if (aMethod.MethodAssembler == null && aMethod.PlugMethod == null)
            {
                var xBody = aMethod.MethodBase.GetMethodBody();
                if (xBody != null)
                {
                    for (int j = xBody.LocalVariables.Count - 1; j >= 0; j--)
                    {
                        int xLocalSize = (int)ILOp.Align(ILOp.SizeOfType(xBody.LocalVariables[j].LocalType), 4);
                        new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = (uint)xLocalSize };
                    }
                }
            }
            //new CPUx86.Add(CPUx86.Registers_Old.ESP, "0x4");
            //new CPUx86.Compare { DestinationReg = Registers.EBP, SourceReg = Registers.ESP };
            //new CPUx86.ConditionalJump { Condition = ConditionalTestEnum.Equal, DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(aMethod.MethodBase) + EndOfMethodLabelNameException + "__2" };
            //new CPUx86.Xchg { DestinationReg = Registers.BX, SourceReg = Registers.BX };
            //new CPUx86.Halt();
            // TODO: not nice coding, still a test
            //new CPUx86.Move { DestinationReg = Registers.ESP, SourceReg = Registers.EBP };
            new Label(ILOp.GetMethodLabel(aMethod) + EndOfMethodLabelNameException + "__2");
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBP };
            var xRetSize = ((int)xTotalArgsSize) - ((int)xReturnSize);
            if (xRetSize < 0)
            {
                xRetSize = 0;
            }
            WriteDebug(aMethod.MethodBase, (uint)xRetSize, IL.Call.GetStackSizeToReservate(aMethod.MethodBase));
            new CPUx86.Return { DestinationValue = (uint)xRetSize };
        }

        public static uint GetResultCodeOffset(uint aResultSize, uint aTotalArgumentSize)
        {
            uint xOffset = 8;
            if ((aTotalArgumentSize > 0) && (aTotalArgumentSize >= aResultSize))
            {
                xOffset += aTotalArgumentSize;
                xOffset -= aResultSize;
            }
            return xOffset;
        }

        private static ISymbolReader GetSymbolReaderForAssembly(Assembly aAssembly)
        {
            try
            {
                return SymbolAccess.GetReaderForFile(aAssembly.Location);
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }

        protected override void MethodBegin(string aMethodName)
        {
            base.MethodBegin(aMethodName);
            new Label(aMethodName);
            new Push { DestinationReg = Registers.EBP };
            new Move { DestinationReg = Registers.EBP, SourceReg = Registers.ESP };
            xCodeOffsets = new int[0];
        }

        protected override void MethodEnd(string aMethodName)
        {
            base.MethodEnd(aMethodName);
            new Label("_END_OF_" + aMethodName);
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBP };
            new CPUx86.Return();
        }

        private static HashSet<string> mDebugLines = new HashSet<string>();
        private static void WriteDebug(MethodBase aMethod, uint aSize, uint aSize2)
        {
            var xLine = String.Format("{0}\t{1}\t{2}", MethodInfoLabelGenerator.GenerateFullName(aMethod), aSize, aSize2);

        }
        private List<DebugInfo.MLDebugSymbol> mSymbols = new List<DebugInfo.MLDebugSymbol>();

        // These are all temp functions until we move to the new assembler.
        // They are used to clean up the old assembler slightly while retaining compatibiltiy for now
        public static string TmpPosLabel(MethodInfo aMethod, int aOffset)
        {
            // todo: fix to be small again. 
            return ILOp.GetLabel(aMethod, aOffset);
            //TODO: Change to Hex output, will be smaller and slightly faster for NASM      
            //return "POS_" + aMethod.UID + "_" + aOffset;
        }

        public static string TmpPosLabel(MethodInfo aMethod, ILOpCode aOpCode)
        {
            return TmpPosLabel(aMethod, aOpCode.Position);
        }

        public static string TmpBranchLabel(MethodInfo aMethod, ILOpCode aOpCode)
        {
            return TmpPosLabel(aMethod, ((ILOpCodes.OpBranch)aOpCode).Value);
        }

        protected override void BeforeOp(MethodInfo aMethod, ILOpCode aOpCode)
        {
            base.BeforeOp(aMethod, aOpCode);
            new Label(TmpPosLabel(aMethod, aOpCode));
            #region Collection debug information
            if (mSymbols != null)
            {
                var xMLSymbol = new DebugInfo.MLDebugSymbol();
                xMLSymbol.LabelName = TmpPosLabel(aMethod, aOpCode);
                xMLSymbol.MethodName = aMethod.MethodBase.GetFullName();
                int xStackSize = (from item in mAssembler.Stack
                                  let xSize = (item.Size % 4 == 0)
                                                  ? item.Size
                                                  : (item.Size + (4 - (item.Size % 4)))
                                  select xSize).Sum();
                xMLSymbol.StackDifference = -1;
                if (aMethod.MethodBase != null)
                {
                    var xBody = aMethod.MethodBase.GetMethodBody();
                    if (xBody != null)
                    {
                        var xLocalsSize = (from item in xBody.LocalVariables
                                           select (int)ILOp.Align(ILOp.SizeOfType(item.LocalType), 4)).Sum();
                        xMLSymbol.StackDifference = xLocalsSize + xStackSize;
                    }
                }
                try
                {
                    xMLSymbol.AssemblyFile = aMethod.MethodBase.DeclaringType.Assembly.Location;
                }
                catch (NotSupportedException)
                {
                    xMLSymbol.AssemblyFile = "DYNAMIC: " + aMethod.MethodBase.DeclaringType.Assembly.FullName;
                }
                xMLSymbol.MethodToken = aMethod.MethodBase.MetadataToken;
                xMLSymbol.TypeToken = aMethod.MethodBase.DeclaringType.MetadataToken;
                xMLSymbol.ILOffset = aOpCode.Position;
                mSymbols.Add(xMLSymbol);
            }
            #endregion
            EmitTracer(aMethod, aOpCode, aMethod.MethodBase.DeclaringType.Namespace, xCodeOffsets);
        }

        public TraceAssemblies TraceAssemblies;
        public DebugMode DebugMode;

        protected void EmitTracer(MethodInfo aMethod, ILOpCode aOp, string aNamespace, int[] aCodeOffsets)
        {
            // NOTE - These if statements can be optimized down - but clarity is
            // more importnat the optimizations would not offer much benefit

            // Determine if a new DebugStub should be emitted
            //bool xEmit = false;
            // Skip NOOP's so we dont have breakpoints on them
            //TODO: Each IL op should exist in IL, and descendants in IL.X86.
            // Because of this we have this hack
            if (aOp.OpCode == ILOpCode.Code.Nop)
            {
                return;
            }
            else if (DebugMode == DebugMode.None)
            {
                return;
            }
            else if (DebugMode == DebugMode.Source)
            {
                // If the current position equals one of the offsets, then we have
                // reached a new atomic C# statement
                if (aCodeOffsets != null)
                {
                    var xIndex = Array.IndexOf(aCodeOffsets, aOp.Position);
                    if (xIndex == -1)
                    {
                        return;
                    }
                    // 0xFEEFEE means hiddenline -> we dont want to stop there
                    if (xCodeLineNumbers[xIndex] == 0xFEEFEE)
                    {
                        return;
                    }
                }
            }

            // Check options for Debug Level
            // Set based on TracedAssemblies
            if (TraceAssemblies == TraceAssemblies.Cosmos || TraceAssemblies == TraceAssemblies.User)
            {
                if (aNamespace.StartsWith("System.", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
                else if (aNamespace.ToLower() == "system")
                {
                    return;
                }
                else if (aNamespace.StartsWith("Microsoft.", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
            }
            if (TraceAssemblies == TraceAssemblies.User)
            {
                //TODO: Maybe an attribute that could be used to turn tracing on and off
                //TODO: This doesnt match Cosmos.Kernel exact vs Cosmos.Kernel., so a user 
                // could do Cosmos.KernelMine and it will fail. Need to fix this
                if (aNamespace.StartsWith("Cosmos.Kernel", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
                else if (aNamespace.StartsWith("Cosmos.Sys", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
                else if (aNamespace.StartsWith("Cosmos.Hardware", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
                else if (aNamespace.StartsWith("Cosmos.IL2CPU", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
            }
            // If we made it this far, emit the Tracer
            new CPUx86.Call { DestinationLabel = "DebugStub_TracerEntry" };
        }

        private int[] xCodeOffsets;
        private int[] xCodeLineNumbers;
        protected override void AfterOp(MethodInfo aMethod, ILOpCode aOpCode)
        {
            base.AfterOp(aMethod, aOpCode);
            var xContents = "";
            foreach (var xStackItem in mAssembler.Stack)
            {
                xContents += ILOp.Align((uint)xStackItem.Size, 4);
                xContents += ", ";
            }
            if (xContents.EndsWith(", "))
            {
                xContents = xContents.Substring(0, xContents.Length - 2);
            }
            new Comment("Stack contains " + mAssembler.Stack.Count + " items: (" + xContents + ")");
        }

        public DebugInfo DebugInfo
        {
            get;
            set;
        }

        public void FinalizeDebugInfo()
        {
            this.DebugInfo.WriteSymbolsListToFile(mSymbols);
            this.DebugInfo.WriteAllLocalsArgumentsInfos(mLocals_Arguments_Infos);
        }
    }
}