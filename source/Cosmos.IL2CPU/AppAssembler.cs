//#define VMT_DEBUG
//#define COSMOSDEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;

using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using Cosmos.Build.Common;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.IL2CPU.API;
using Cosmos.IL2CPU.X86.IL;
using Cosmos.Debug.Symbols;
using Cosmos.IL2CPU.Extensions;
using XSharp.Common;
using static XSharp.Common.XSRegisters;
using Call = Cosmos.Assembler.x86.Call;
using Label = Cosmos.Assembler.Label;

namespace Cosmos.IL2CPU
{
    public class AppAssembler : IDisposable
    {
        public const string EndOfMethodLabelNameNormal = ".END__OF__METHOD_NORMAL";
        public const string EndOfMethodLabelNameException = ".END__OF__METHOD_EXCEPTION";
        protected const string InitStringIDsLabel = "___INIT__STRINGS_TYPE_ID_S___";
        protected List<LOCAL_ARGUMENT_INFO> mLocals_Arguments_Infos = new List<LOCAL_ARGUMENT_INFO>();
        protected ILOp[] mILOpsLo = new ILOp[256];
        protected ILOp[] mILOpsHi = new ILOp[256];
        public bool ShouldOptimize = false;
        public DebugInfo DebugInfo { get; set; }
        protected TextWriter mLog;
        protected Dictionary<string, ModuleDefinition> mLoadedModules = new Dictionary<string, ModuleDefinition>();
        protected DebugInfo.SequencePoint[] mSequences = new DebugInfo.SequencePoint[0];
        public TraceAssemblies TraceAssemblies;
        public bool DebugEnabled = false;
        public bool StackCorruptionDetection = false;
        public StackCorruptionDetectionLevel StackCorruptionDetectionLevel;
        public DebugMode DebugMode;
        public bool IgnoreDebugStubAttribute;
        protected static HashSet<string> mDebugLines = new HashSet<string>();
        protected List<MethodIlOp> mSymbols = new List<MethodIlOp>();
        protected List<INT3Label> mINT3Labels = new List<INT3Label>();
        public readonly CosmosAssembler Assembler;
        //
        protected string mCurrentMethodLabel;
        protected long mCurrentMethodLabelEndGuid;
        protected long mCurrentMethodGuid;

        public AppAssembler(int aComPort, string assemblerLogFile)
        {
            Assembler = CreateAssembler(aComPort);
            mLog = new StreamWriter(File.OpenWrite(assemblerLogFile));
            InitILOps();
        }

        protected virtual CosmosAssembler CreateAssembler(int aComPort)
        {
            return new CosmosAssembler(aComPort);
        }

        public void Dispose()
        {
            if (mLog != null)
            {
                mLog.Dispose();
                mLog = null;
            }
            GC.SuppressFinalize(this);
        }

        protected void MethodBegin(_MethodInfo aMethod)
        {
            XS.Comment("---------------------------------------------------------");
            XS.Comment("Assembly: " + aMethod.MethodBase.DeclaringType.GetTypeInfo().Assembly.FullName);
            XS.Comment("Type: " + aMethod.MethodBase.DeclaringType);
            XS.Comment("Name: " + aMethod.MethodBase.Name);
            XS.Comment("Plugged: " + (aMethod.PlugMethod == null ? "No" : "Yes"));
            // for now:
            var shouldIncludeArgAndLocalsComment = true;
            if (shouldIncludeArgAndLocalsComment)
            {
                if (aMethod.MethodAssembler == null && !aMethod.IsInlineAssembler)
                {
                    // the body of aMethod is getting emitted
                    var xLocals = aMethod.MethodBase.GetLocalVariables();
                    for (int i = 0; i < xLocals.Count; i++)
                    {
                        XS.Comment(String.Format("Local {0} at EBP-{1}", i, ILOp.GetEBPOffsetForLocal(aMethod, i)));
                    }

                    var xIdxOffset = 0u;
                    if (!aMethod.MethodBase.IsStatic)
                    {
                        XS.Comment(String.Format("Argument[0] $this at EBP+{0}, size = {1}", X86.IL.Ldarg.GetArgumentDisplacement(aMethod, 0), ILOp.Align(ILOp.SizeOfType(aMethod.MethodBase.DeclaringType), 4)));
                        xIdxOffset++;
                    }

                    string x = aMethod.MethodBase.Name;
                    string y = aMethod.MethodBase.DeclaringType.Name;
                    var xParams = aMethod.MethodBase.GetParameters();
                    var xParamCount = (ushort)xParams.Length;

                    for (ushort i = 0; i < xParamCount; i++)
                    {
                        var xOffset = X86.IL.Ldarg.GetArgumentDisplacement(aMethod, (ushort)(i + xIdxOffset));
                        var xSize = ILOp.SizeOfType(xParams[i].ParameterType);
                        // if last argument is 8 byte long, we need to add 4, so that debugger could read all 8 bytes from this variable in positiv direction
                        XS.Comment(String.Format("Argument[{3}] {0} at EBP+{1}, size = {2}", xParams[i].Name, xOffset, xSize, (xIdxOffset + i)));
                    }

                    var xMethodInfo = aMethod.MethodBase as MethodInfo;
                    if (xMethodInfo != null)
                    {
                        var xSize = ILOp.Align(ILOp.SizeOfType(xMethodInfo.ReturnType), 4);
                        XS.Comment(String.Format("Return size: {0}", xSize));
                    }
                }
            }

            // Issue label that is used for calls etc.
            string xMethodLabel;
            if (aMethod.PluggedMethod != null)
            {
                xMethodLabel = "PLUG_FOR___" + LabelName.Get(aMethod.PluggedMethod.MethodBase);
            }
            else
            {
                xMethodLabel = LabelName.Get(aMethod.MethodBase);
            }
            XS.Label(xMethodLabel);

            // Alternative asm labels for the method
            var xAsmLabelAttributes = aMethod.MethodBase.GetCustomAttributes<AsmLabelAttribute>();
            foreach (var xAttribute in xAsmLabelAttributes)
            {
                XS.Label(xAttribute.Label);
            }

            //Assembler.WriteDebugVideo("Method " + aMethod.UID);

            // We could use same GUID as MethodLabelStart, but its better to keep GUIDs unique globaly for items
            // so during debugging they can never be confused as to what they point to.
            mCurrentMethodGuid = DebugInfo.CreateId();

            // We issue a second label for GUID. This is increases label count, but for now we need a master label first.
            // We issue a GUID label to reduce amount of work and time needed to construct debugging DB.
            var xLabelGuid = DebugInfo.CreateId();
            new Label("GUID_" + xLabelGuid.ToString());

            mCurrentMethodLabel = "METHOD_" + xLabelGuid.ToString();
            Label.LastFullLabel = mCurrentMethodLabel;

            if (DebugEnabled && StackCorruptionDetection)
            {
                // if StackCorruption detection is active, we're also going to emit a stack overflow detection
                XS.Set(EAX, "Before_Kernel_Stack");
                XS.Compare(EAX, ESP);
                XS.Jump(ConditionalTestEnum.LessThan, mCurrentMethodLabel + ".StackOverflowCheck_End");
                XS.ClearInterruptFlag();
                // don't remove the call. It seems pointless, but we need it to retrieve the EIP value
                new Call { DestinationLabel = mCurrentMethodLabel + ".StackOverflowCheck_GetAddress" };
                XS.Label(mCurrentMethodLabel + ".StackOverflowCheck_GetAddress");
                XS.Pop(EAX);
                new Mov { DestinationRef = ElementReference.New("DebugStub_CallerEIP"), DestinationIsIndirect = true, SourceReg = RegistersEnum.EAX };
                XS.Call("DebugStub_SendStackOverflowOccurred");
                XS.Halt();
                XS.Label(mCurrentMethodLabel + ".StackOverflowCheck_End");
            }

            mCurrentMethodLabelEndGuid = DebugInfo.CreateId();

            if (aMethod.MethodBase.IsStatic && aMethod.MethodBase is ConstructorInfo)
            {
                XS.Comment("Static constructor. See if it has been called already, return if so.");
                var xName = DataMember.FilterStringForIncorrectChars("CCTOR_CALLED__" + LabelName.GetFullName(aMethod.MethodBase.DeclaringType));
                XS.DataMember(xName, 1, "db", "0");
                XS.Compare(xName, 1, destinationIsIndirect: true, size: RegisterSize.Byte8);
                XS.Jump(ConditionalTestEnum.Equal, ".BeforeQuickReturn");
                XS.Set(xName, 1, destinationIsIndirect: true, size: RegisterSize.Byte8);
                XS.Jump(".AfterCCTorAlreadyCalledCheck");
                XS.Label(".BeforeQuickReturn");
                XS.Set(ECX, 0);
                XS.Return();
                XS.Label(".AfterCCTorAlreadyCalledCheck");
            }

            XS.Push(EBP);
            XS.Set(EBP, ESP);

            if (DebugMode == DebugMode.Source)
            {
                // Would be nice to use xMethodSymbols.GetSourceStartEnd but we cant
                // because its not implemented by the unmanaged code underneath.
                //
                // This doesnt seem right to store as a field, but old code had it that way so we
                // continue using a field for now.
                mSequences = DebugInfo.GetSequencePoints(aMethod.MethodBase, true);
                if (mSequences.Length > 0)
                {
                    DebugInfo.AddDocument(mSequences[0].Document);

                    var xMethod = new Method();
                    xMethod.ID = mCurrentMethodGuid;
                    xMethod.TypeToken = aMethod.MethodBase.DeclaringType.GetTypeInfo().GetMetadataToken();
                    xMethod.MethodToken = aMethod.MethodBase.MetadataToken;
                    xMethod.LabelStartID = xLabelGuid;
                    xMethod.LabelEndID = mCurrentMethodLabelEndGuid;
                    xMethod.LabelCall = xMethodLabel;
                    long xAssemblyFileID;
                    if (DebugInfo.AssemblyGUIDs.TryGetValue(aMethod.MethodBase.DeclaringType.GetTypeInfo().Assembly, out xAssemblyFileID))
                    {
                        xMethod.AssemblyFileID = xAssemblyFileID;
                    }
                    xMethod.DocumentID = DebugInfo.DocumentGUIDs[mSequences[0].Document.ToLower()];
                    xMethod.LineColStart = ((Int64)mSequences[0].LineStart << 32) + mSequences[0].ColStart;
                    xMethod.LineColEnd = ((Int64)(mSequences[mSequences.Length - 1].LineEnd) << 32) + mSequences[mSequences.Length - 1].ColEnd;
                    DebugInfo.AddMethod(xMethod);
                }
            }

            if (aMethod.MethodAssembler == null && aMethod.PlugMethod == null && !aMethod.IsInlineAssembler)
            {
                // the body of aMethod is getting emitted
                    var xLocalsOffset = mLocals_Arguments_Infos.Count;
                    aMethod.LocalVariablesSize = 0;
                var xLocals = DebugSymbolReader.GetLocalVariableInfos(aMethod.MethodBase);
                for (int i = 0; i < xLocals.Count; i++)
                {
                    {
                        var xInfo = new LOCAL_ARGUMENT_INFO
                                    {
                                        METHODLABELNAME = xMethodLabel,
                                        IsArgument = false,
                                        INDEXINMETHOD = xLocals[i].Slot,
                                        NAME = xLocals[i].Name,
                                        OFFSET = 0 - (int) ILOp.GetEBPOffsetForLocalForDebugger(aMethod, i),
                                        TYPENAME = xLocals[i].Type.AssemblyQualifiedName
                                    };
                        mLocals_Arguments_Infos.Add(xInfo);

                        var xSize = ILOp.Align(ILOp.SizeOfType(xLocals[i].Type), 4);
                        XS.Comment(String.Format("Local {0}, Size {1}", i, xSize));
                        for (int j = 0; j < xSize / 4; j++)
                        {
                            XS.Push(0);
                        }
                        aMethod.LocalVariablesSize += xSize;
                    }
                }

                // debug info:
                var xIdxOffset = 0u;
                if (!aMethod.MethodBase.IsStatic)
                {
                    mLocals_Arguments_Infos.Add(new LOCAL_ARGUMENT_INFO
                    {
                        METHODLABELNAME = xMethodLabel,
                        IsArgument = true,
                        NAME = "this:" + X86.IL.Ldarg.GetArgumentDisplacement(aMethod, 0),
                        INDEXINMETHOD = 0,
                        OFFSET = X86.IL.Ldarg.GetArgumentDisplacement(aMethod, 0),
                        TYPENAME = aMethod.MethodBase.DeclaringType.AssemblyQualifiedName
                    });

                    xIdxOffset++;
                }

                var xParams = aMethod.MethodBase.GetParameters();
                var xParamCount = (ushort)xParams.Length;

                for (ushort i = 0; i < xParamCount; i++)
                {
                    var xOffset = X86.IL.Ldarg.GetArgumentDisplacement(aMethod, (ushort)(i + xIdxOffset));
                    // if last argument is 8 byte long, we need to add 4, so that debugger could read all 8 bytes from this variable in positiv direction
                    xOffset -= (int)ILOp.Align(ILOp.SizeOfType(xParams[i].ParameterType), 4) - 4;
                    mLocals_Arguments_Infos.Add(new LOCAL_ARGUMENT_INFO
                    {
                        METHODLABELNAME = xMethodLabel,
                        IsArgument = true,
                        INDEXINMETHOD = (int)(i + xIdxOffset),
                        NAME = xParams[i].Name,
                        OFFSET = xOffset,
                        TYPENAME = xParams[i].ParameterType.AssemblyQualifiedName
                    });
                }
            }
        }

        protected void MethodEnd(_MethodInfo aMethod)
        {
            XS.Comment("End Method: " + aMethod.MethodBase.Name);

            uint xReturnSize = 0;
            var xMethInfo = aMethod.MethodBase as MethodInfo;
            if (xMethInfo != null)
            {
                xReturnSize = ILOp.Align(ILOp.SizeOfType(xMethInfo.ReturnType), 4);
            }
            var xMethodLabel = ILOp.GetLabel(aMethod);
            //if (aMethod.PlugMethod == null
            //    && !aMethod.IsInlineAssembler)
            {
                XS.Label(xMethodLabel + EndOfMethodLabelNameNormal);

                XS.Comment("Following code is for debugging. Adjust accordingly!");
                XS.Set("static_field__Cosmos_Core_INTs_mLastKnownAddress", xMethodLabel + EndOfMethodLabelNameNormal, destinationIsIndirect: true);
            }

            XS.Set(ECX, 0);
            var xTotalArgsSize = (from item in aMethod.MethodBase.GetParameters()
                                  select (int)ILOp.Align(ILOp.SizeOfType(item.ParameterType), 4)).Sum();
            if (!aMethod.MethodBase.IsStatic)
            {
                if (aMethod.MethodBase.DeclaringType.GetTypeInfo().IsValueType)
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
                xMethInfo = aMethod.PluggedMethod.MethodBase as MethodInfo;
                if (xMethInfo != null)
                {
                    xReturnSize = ILOp.Align(ILOp.SizeOfType(xMethInfo.ReturnType), 4);
                }
                xTotalArgsSize = (from item in aMethod.PluggedMethod.MethodBase.GetParameters()
                                  select (int)ILOp.Align(ILOp.SizeOfType(item.ParameterType), 4)).Sum();
                if (!aMethod.PluggedMethod.MethodBase.IsStatic)
                {
                    if (aMethod.PluggedMethod.MethodBase.DeclaringType.GetTypeInfo().IsValueType)
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
                var xOffset = GetResultCodeOffset(xReturnSize, (uint)xTotalArgsSize);
                for (int i = 0; i < ((int)(xReturnSize / 4)); i++)
                {
                    XS.Pop(EAX);
                    XS.Set(EBP, EAX, destinationDisplacement: (int)(xOffset + ((i + 0) * 4)));
                }
                // extra stack space is the space reserved for example when a "public static int TestMethod();" method is called, 4 bytes is pushed, to make room for result;
            }
            var xLabelExc = xMethodLabel + EndOfMethodLabelNameException;
            XS.Label(xLabelExc);
            if (aMethod.MethodAssembler == null && aMethod.PlugMethod == null && !aMethod.IsInlineAssembler)
            {
                    uint xLocalsSize = 0;
                    var xLocalInfos = DebugSymbolReader.GetLocalVariableInfos(aMethod.MethodBase);
                    for (int j = xLocalInfos.Count - 1; j >= 0; j--)
                    {
                        xLocalsSize += ILOp.Align(ILOp.SizeOfType(xLocalInfos[j].Type), 4);

                        if (xLocalsSize >= 256)
                        {
                            XS.Add(ESP, 255);
                            xLocalsSize -= 255;
                        }
                    }
                    if (xLocalsSize > 0)
                    {
                        XS.Add(ESP, xLocalsSize);
                    }
                }

            if (DebugEnabled && StackCorruptionDetection)
            {
                // if debugstub is active, emit a stack corruption detection. at this point EBP and ESP should have the same value.
                // if not, we should somehow break here.
                XS.Set(EAX, ESP);
                XS.Set(EBX, EBP);
                XS.Compare(EAX, EBX);
                XS.Jump(ConditionalTestEnum.Equal, xLabelExc + "__2");
                XS.ClearInterruptFlag();
                // don't remove the call. It seems pointless, but we need it to retrieve the EIP value
                new Call { DestinationLabel = xLabelExc + ".MethodFooterStackCorruptionCheck_Break_on_location" };
                XS.Label(xLabelExc + ".MethodFooterStackCorruptionCheck_Break_on_location");
                XS.Pop(ECX);
                XS.Push(EAX);
                XS.Push(EBX);
                new Mov { DestinationRef = ElementReference.New("DebugStub_CallerEIP"), DestinationIsIndirect = true, SourceReg = RegistersEnum.ECX };
                XS.Call("DebugStub_SendSimpleNumber");
                XS.Add(ESP, 4);
                XS.Call("DebugStub_SendSimpleNumber");
                XS.Add(ESP, 4);
                XS.Call("DebugStub_SendStackCorruptionOccurred");
                XS.Halt();
            }
            XS.Label(xLabelExc + "__2");
            XS.Pop(EBP);
            var xRetSize = ((int)xTotalArgsSize) - ((int)xReturnSize);
            if (xRetSize < 0)
            {
                xRetSize = 0;
            }
            WriteDebug(aMethod.MethodBase, (uint)xRetSize, X86.IL.Call.GetStackSizeToReservate(aMethod.MethodBase));
            new Return { DestinationValue = (uint)xRetSize };

            // Final, after all code. Points to op AFTER method.
            new Label("GUID_" + mCurrentMethodLabelEndGuid.ToString());
        }

        public void FinalizeDebugInfo()
        {
            DebugInfo.AddDocument(null, true);
            DebugInfo.AddAssemblies(null, true);
            DebugInfo.AddMethod(null, true);
            DebugInfo.WriteAllLocalsArgumentsInfos(mLocals_Arguments_Infos);
            DebugInfo.AddSymbols(mSymbols, true);
            DebugInfo.AddINT3Labels(mINT3Labels, true);
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

        public void ProcessMethod(_MethodInfo aMethod, List<ILOpCode> aOpCodes)
        {
            try
            {
                // We check this here and not scanner as when scanner makes these
                // plugs may still have not yet been scanned that it will depend on.
                // But by the time we make it here, they have to be resolved.
                if (aMethod.Type == _MethodInfo.TypeEnum.NeedsPlug && aMethod.PlugMethod == null)
                {
                    throw new Exception("Method needs plug, but no plug was assigned.");
                }

                if (aMethod.MethodBase.Name == "InitializeArray")
                {
                    ;
                }

                // todo: MtW: how to do this? we need some extra space.
                //		see ConstructLabel for extra info
                if (aMethod.UID > 0x00FFFFFF)
                {
                    throw new Exception("Too many methods.");
                }

                MethodBegin(aMethod);
                mLog.WriteLine("Method '{0}', ID = '{1}'", aMethod.MethodBase.GetFullName(), aMethod.UID);
                mLog.Flush();
                if (aMethod.MethodAssembler != null)
                {
                    var xAssembler = (AssemblerMethod)Activator.CreateInstance(aMethod.MethodAssembler);
                    xAssembler.AssembleNew(Assembler, aMethod.PluggedMethod);
                }
                else if (aMethod.IsInlineAssembler)
                {
                    aMethod.MethodBase.Invoke(null, new object[aMethod.MethodBase.GetParameters().Length]);
                }
                else
                {
                    // now emit the actual assembler code for this method.

                    //Conditions under which we should emit an INT3 instead of a plceholder NOP:
                    /* - First instruction in a Method / Loop / If / Else etc.
                 *   -- In essence, whenever there is a opening {
                 *   -- C# Debug builds automatically insert NOPs at these locations (otherwise NOP is not used)
                 *   -- So only insert an INT3 when we are about to insert a NOP that came from IL code
                 */

                    /* We group opcodes together by logical statement. Each statement will have its logical stack cleared.
                 * Also, this lets us do optimizations later on.
                 */
                    bool emitINT3 = true;
                    DebugInfo.SequencePoint xPreviousSequencePoint = null;
                    var xCurrentGroup = new List<ILOpCode>();
                    CompilerHelpers.Debug($"AppAssembler: Method: {aMethod.MethodBase.GetFullName()}");
                    foreach (var xRawOpcode in aOpCodes)
                    {
                        var xSP = mSequences.FirstOrDefault(q => q.Offset == xRawOpcode.Position && q.LineStart != 0xFEEFEE);
                        // detect if we're at a new statement.
                        if (xPreviousSequencePoint == null && xSP != null)
                        {

                        }
                        if (xSP != null && xCurrentGroup.Count > 0)
                        {
                            EmitInstructions(aMethod, xCurrentGroup, ref emitINT3);
                            xCurrentGroup.Clear();
                            xPreviousSequencePoint = xSP;
                        }
                        xCurrentGroup.Add(xRawOpcode);
                    }
                    if (xCurrentGroup.Count > 0)
                    {
                        EmitInstructions(aMethod, xCurrentGroup, ref emitINT3);
                    }
                }
                MethodEnd(aMethod);
            }
            catch (Exception E)
            {
                throw new Exception("Error compiling method '" + aMethod.MethodBase.GetFullName() + "': " + E.ToString(), E);
            }
        }

        private void BeforeEmitInstructions(_MethodInfo aMethod, List<ILOpCode> aCurrentGroup)
        {
            // do optimizations
        }

        private void AfterEmitInstructions(_MethodInfo aMethod, List<ILOpCode> aCurrentGroup)
        {
            // do optimizations

            //if (Assembler.Stack.Count > 0)
            //{
            //    if (mDebugStackErrors)
            //    {
            //        Console.WriteLine("StackCorruption in Analytical stack:");
            //        Console.WriteLine("- Method: {0}", aMethod.MethodBase.GetFullName());
            //        Console.WriteLine("- Last ILOpCode offset: {0}", aCurrentGroup.Last().Position.ToString("X"));
            //    }
            //}
        }

        //private static bool mDebugStackErrors = true;
        private void EmitInstructions(_MethodInfo aMethod, List<ILOpCode> aCurrentGroup, ref bool emitINT3)
        {
            ILOpCode.ILInterpretationDebugLine(() => "---- Group");
            InterpretInstructionsToDetermineStackTypes(aCurrentGroup);
            BeforeEmitInstructions(aMethod, aCurrentGroup);
            var xFirstInstruction = true;
            foreach (var xOpCode in aCurrentGroup)
            {
                ushort xOpCodeVal = (ushort)xOpCode.OpCode;
                ILOp xILOp;
                if (xOpCodeVal <= 0xFF)
                {
                    xILOp = mILOpsLo[xOpCodeVal];
                }
                else
                {
                    xILOp = mILOpsHi[xOpCodeVal & 0xFF];
                }
                mLog.Flush();

                //Only emit INT3 as per conditions above...
                bool INT3Emitted = false;
                BeforeOp(aMethod, xOpCode, emitINT3, out INT3Emitted, xFirstInstruction);
                xFirstInstruction = false;
                //Emit INT3 on the first non-NOP instruction immediately after a NOP
                // - This is because TracePoints for NOP are automatically ignored in code called below this
                emitINT3 = (emitINT3 && !INT3Emitted) || xILOp is Nop;

                XS.Comment(xILOp.ToString());
                var xNextPosition = xOpCode.Position + 1;

                #region Exception handling support code

                _ExceptionRegionInfo xCurrentRegion = null;
                var xBody = aMethod.MethodBase.GetMethodBody();
                // todo: add support for nested handlers using a stack or so..
                foreach (_ExceptionRegionInfo xHandler in xBody.GetExceptionRegionInfos(aMethod.MethodBase.DeclaringType.GetTypeInfo().Module))
                {
                    if (xHandler.TryOffset > 0)
                    {
                        if (xHandler.TryOffset <= xNextPosition && (xHandler.TryLength + xHandler.TryOffset) > xNextPosition)
                        {
                            if (xCurrentRegion == null)
                            {
                                xCurrentRegion = xHandler;
                                continue;
                            }
                            else if (xHandler.TryOffset > xCurrentRegion.TryOffset && (xHandler.TryLength + xHandler.TryOffset) < (xCurrentRegion.TryLength + xCurrentRegion.TryOffset))
                            {
                                // only replace if the current found handler is narrower
                                xCurrentRegion = xHandler;
                                continue;
                            }
                        }
                    }
                    if (xHandler.HandlerOffset > 0)
                    {
                        if (xHandler.HandlerOffset <= xNextPosition && (xHandler.HandlerOffset + xHandler.HandlerLength) > xNextPosition)
                        {
                            if (xCurrentRegion == null)
                            {
                                xCurrentRegion = xHandler;
                                continue;
                            }
                            else if (xHandler.HandlerOffset > xCurrentRegion.HandlerOffset && (xHandler.HandlerOffset + xHandler.HandlerLength) < (xCurrentRegion.HandlerOffset + xCurrentRegion.HandlerLength))
                            {
                                // only replace if the current found handler is narrower
                                xCurrentRegion = xHandler;
                                continue;
                            }
                        }
                    }
                    if (xHandler.Kind.HasFlag(ExceptionRegionKind.Filter))
                    {
                        if (xHandler.FilterOffset > 0)
                        {
                            if (xHandler.FilterOffset <= xNextPosition)
                            {
                                if (xCurrentRegion == null)
                                {
                                    xCurrentRegion = xHandler;
                                    continue;
                                }
                                else if (xHandler.FilterOffset > xCurrentRegion.FilterOffset)
                                {
                                    // only replace if the current found handler is narrower
                                    xCurrentRegion = xHandler;
                                    continue;
                                }
                            }
                        }
                    }
                }

                #endregion

                var xNeedsExceptionPush = (xCurrentRegion != null) && (((xCurrentRegion.HandlerOffset > 0 && xCurrentRegion.HandlerOffset == xOpCode.Position) || (xCurrentRegion.Kind.HasFlag(ExceptionRegionKind.Filter) && xCurrentRegion.FilterOffset > 0 && xCurrentRegion.FilterOffset == xOpCode.Position)) && (xCurrentRegion.Kind == ExceptionRegionKind.Catch));
                if (xNeedsExceptionPush)
                {
                    Push(DataMember.GetStaticFieldName(ExceptionHelperRefs.CurrentExceptionRef), true);
                    XS.Push(0);
                }
                xILOp.DebugEnabled = DebugEnabled;
                xILOp.Execute(aMethod, xOpCode);

                AfterOp(aMethod, xOpCode);
                //mLog.WriteLine( " end: " + Stack.Count.ToString() );
            }
            AfterEmitInstructions(aMethod, aCurrentGroup);
        }

        /// <summary>
        /// This method takes care of "interpreting" the instructions per group (statement). This is necessary to
        /// reliably able to tell what sizes are involved in certain actions.
        /// </summary>
        /// <param name="aCurrentGroup"></param>
        private static void InterpretInstructionsToDetermineStackTypes(List<ILOpCode> aCurrentGroup)
        {
            var xNeedsInterpreting = true;
            // see if we need to interpret the instructions at all.
            foreach (var xOp in aCurrentGroup)
            {
                foreach (var xStackEntry in xOp.StackPopTypes.Concat(xOp.StackPushTypes))
                {
                    if (xStackEntry == null)
                    {
                        xNeedsInterpreting = true;
                        break;
                    }
                }
                if (xNeedsInterpreting)
                {
                    break;
                }
            }
            var xIteration = 0;
            var xGroupILByILOffset = aCurrentGroup.ToDictionary(i => i.Position);
            while (xNeedsInterpreting)
            {
                ILOpCode.ILInterpretationDebugLine(() => String.Format("--------- New Interpretation iteration (xIteration = {0})", xIteration));
                xIteration++;
                if (xIteration > 20)
                {
                    // Situation not resolved. Now give error with first offset needing types:
                    foreach (var xOp in aCurrentGroup)
                    {
                        foreach (var xStackEntry in xOp.StackPopTypes.Concat(xOp.StackPushTypes))
                        {
                            if (xStackEntry == null)
                            {
                                throw new Exception(string.Format("Safety exception. Handled {0} iterations. Instruction needing info: {1}", xIteration, xOp));
                            }
                        }
                    }
                }

                aCurrentGroup.ForEach(i => i.Processed = false);

                var xMaxInterpreterRecursionDepth = 25000;
                var xCurStack = new Stack<Type>();
                var xSituationChanged = false;
                aCurrentGroup.First().InterpretStackTypes(xGroupILByILOffset, xCurStack, ref xSituationChanged, xMaxInterpreterRecursionDepth);
                if (!xSituationChanged)
                {
                    // nothing changed, now give error with first offset needing types:
                    foreach (var xOp in aCurrentGroup)
                    {
                        foreach (var xStackEntry in xOp.StackPopTypes.Concat(xOp.StackPushTypes))
                        {
                            if (xStackEntry == null)
                            {
                                throw new Exception("After interpreting stack types, nothing changed! (First instruction needing types = " + xOp + ")");
                            }
                        }
                    }
                }
                xNeedsInterpreting = false;
                foreach (var xOp in aCurrentGroup)
                {
                    foreach (var xStackEntry in xOp.StackPopTypes.Concat(xOp.StackPushTypes))
                    {
                        if (xStackEntry == null)
                        {
                            xNeedsInterpreting = true;
                            break;
                        }
                    }
                    if (xNeedsInterpreting)
                    {
                        break;
                    }
                }
            }
            foreach (var xOp in aCurrentGroup)
            {
                foreach (var xStackEntry in xOp.StackPopTypes.Concat(xOp.StackPushTypes))
                {
                    if (xStackEntry == null)
                    {
                        throw new Exception(String.Format("Instruction '{0}' has not been fully analysed yet!", xOp));
                    }
                }
            }
        }

        protected void InitILOps()
        {
            InitILOps(typeof(ILOp));
        }

        protected virtual void InitILOps(Type aAssemblerBaseOp)
        {
            foreach (var xType in aAssemblerBaseOp.GetTypeInfo().Assembly.GetExportedTypes())
            {
                var xTypeInfo = xType.GetTypeInfo();
                if (xTypeInfo.IsSubclassOf(aAssemblerBaseOp))
                {
                    var xAttribs = xTypeInfo.GetCustomAttributes<OpCodeAttribute>(false);
                    foreach (var xAttrib in xAttribs)
                    {
                        var xOpCode = (ushort)xAttrib.OpCode;
                        var xCtor = xTypeInfo.GetConstructor(new Type[] { typeof(Assembler.Assembler) });
                        var xILOp = (ILOp)xCtor.Invoke(new object[] { Assembler });
                        if (xOpCode <= 0xFF)
                        {
                            mILOpsLo[xOpCode] = xILOp;
                        }
                        else
                        {
                            mILOpsHi[xOpCode & 0xFF] = xILOp;
                        }
                    }
                }
            }
        }

        protected void Move(string aDestLabelName, int aValue)
        {
            XS.Set(aDestLabelName, (uint)aValue, destinationIsIndirect: true, size: RegisterSize.Int32);
        }

        protected void Push(uint aValue)
        {
            XS.Push(aValue);
        }

        protected void Push(string aLabelName, bool isIndirect = false)
        {
            XS.Push(aLabelName, isIndirect: isIndirect);
        }

        protected void Call(MethodBase aMethod)
        {
            XS.Call(LabelName.Get(aMethod));
        }

        protected void Jump(string aLabelName)
        {
            XS.Jump(aLabelName);
        }

        protected _FieldInfo ResolveField(_MethodInfo method, string fieldId, bool aOnlyInstance)
        {
            return ILOp.ResolveField(method.MethodBase.DeclaringType, fieldId, aOnlyInstance);
        }

        protected void Ldarg(_MethodInfo aMethod, int aIndex)
        {
            X86.IL.Ldarg.DoExecute(Assembler, aMethod, (ushort)aIndex);
        }

        protected void Call(_MethodInfo aMethod, _MethodInfo aTargetMethod, string aNextLabel)
        {
            var xSize = X86.IL.Call.GetStackSizeToReservate(aTargetMethod.MethodBase);
            if (xSize > 0)
            {
                XS.Sub(ESP, xSize);
            }
            XS.Call(ILOp.GetLabel(aTargetMethod));
            var xMethodInfo = aMethod.MethodBase as MethodInfo;

            uint xReturnsize = 0;
            if (xMethodInfo != null)
            {
                xReturnsize = ILOp.SizeOfType(((MethodInfo)aMethod.MethodBase).ReturnType);
            }

            ILOp.EmitExceptionLogic(Assembler, aMethod, null, true,
                     delegate ()
                     {
                         var xResultSize = xReturnsize;
                         if (xResultSize % 4 != 0)
                         {
                             xResultSize += 4 - (xResultSize % 4);
                         }
                         for (int i = 0; i < xResultSize / 4; i++)
                         {
                             XS.Add(ESP, 4);
                         }
                     }, aNextLabel);
        }

        protected void Ldflda(_MethodInfo aMethod, _FieldInfo aFieldInfo)
        {
            X86.IL.Ldflda.DoExecute(Assembler, aMethod, aMethod.MethodBase.DeclaringType, aFieldInfo, false, false, aFieldInfo.DeclaringType);
        }

        protected void Ldsflda(_MethodInfo aMethod, _FieldInfo aFieldInfo)
        {
            X86.IL.Ldsflda.DoExecute(Assembler, aMethod, DataMember.GetStaticFieldName(aFieldInfo.Field), aMethod.MethodBase.DeclaringType, null);
        }

        protected int GetVTableEntrySize()
        {
            return 24; // todo: retrieve from actual type info
        }

        public const string InitVMTCodeLabel = "___INIT__VMT__CODE____";
        public virtual void GenerateVMTCode(HashSet<TypeInfo> aTypesSet, HashSet<MethodBase> aMethodsSet, Func<TypeInfo, uint> aGetTypeID, Func<MethodBase, uint> aGetMethodUID)
        {
            XS.Comment("---------------------------------------------------------");
            XS.Label(InitVMTCodeLabel);
            XS.Push(EBP);
            XS.Set(EBP, ESP);
            mSequences = new DebugInfo.SequencePoint[0];

            var xTypesFieldRef = VTablesImplRefs.VTablesImplDef.GetField("mTypes",
                                                                         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            string xTheName = DataMember.GetStaticFieldName(xTypesFieldRef);
            DataMember xDataMember = (from item in Cosmos.Assembler.Assembler.CurrentInstance.DataMembers
                                      where item.Name == xTheName
                                      select item).FirstOrDefault();
            if (xDataMember != null)
            {
                Cosmos.Assembler.Assembler.CurrentInstance.DataMembers.Remove((from item in Cosmos.Assembler.Assembler.CurrentInstance.DataMembers
                                                                               where item == xDataMember
                                                                               select item).First());
            }
            var xData = new byte[16 + (aTypesSet.Count * GetVTableEntrySize())];
            var xTemp = BitConverter.GetBytes(aGetTypeID(typeof(Array).GetTypeInfo()));
            Array.Copy(xTemp, 0, xData, 0, 4);
            xTemp = BitConverter.GetBytes(0x80000002);
            Array.Copy(xTemp, 0, xData, 4, 4);
            xTemp = BitConverter.GetBytes(aTypesSet.Count);
            Array.Copy(xTemp, 0, xData, 8, 4);
            xTemp = BitConverter.GetBytes(GetVTableEntrySize());
            Array.Copy(xTemp, 0, xData, 12, 4);
            XS.DataMemberBytes(xTheName + "_Contents", xData);
            XS.DataMember(xTheName, 1, "db", "0, 0, 0, 0, 0, 0, 0, 0");
            XS.Set(xTheName, xTheName + "_Contents", destinationIsIndirect: true, destinationDisplacement: 4);
#if VMT_DEBUG
            using (var xVmtDebugOutput = XmlWriter.Create(File.OpenWrite(@"vmt_debug.xml")))
            {
                xVmtDebugOutput.WriteStartDocument();
                xVmtDebugOutput.WriteStartElement("VMT");
#endif
            //Push((uint)aTypesSet.Count);
            foreach (var xType in aTypesSet)
            {
#if VMT_DEBUG
                    xVmtDebugOutput.WriteStartElement("Type");
                    xVmtDebugOutput.WriteAttributeString("TypeId", aGetTypeID(xType).ToString());
                    if (xType.BaseType != null)
                    {
                        xVmtDebugOutput.WriteAttributeString("BaseTypeId", aGetTypeID(xType.BaseType.GetTypeInfo()).ToString());
                    }
                    xVmtDebugOutput.WriteAttributeString("Name", xType.FullName);
#endif
                // value contains true if the method is an interface method definition
                SortedList<MethodBase, bool> xEmittedMethods = new SortedList<MethodBase, bool>(new MethodBaseComparer());
                foreach (MethodBase xMethod in xType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (aMethodsSet.Contains(xMethod))
                    { //) && !xMethod.IsAbstract) {
                        if (!xEmittedMethods.ContainsKey(xMethod))
                        {
                            xEmittedMethods.Add(xMethod, false);
                        }
                    }
                }
                foreach (MethodBase xCtor in xType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (aMethodsSet.Contains(xCtor))
                    { // && !xCtor.IsAbstract) {
                        if (!xEmittedMethods.ContainsKey(xCtor))
                        {
                            xEmittedMethods.Add(xCtor, false);
                        }
                    }
                }
                foreach (var xIntf in xType.GetInterfaces())
                {
                    foreach (var xMethodIntf in xIntf.GetTypeInfo().GetMethods())
                    {
                        var xActualMethod = xType.GetMethod(xIntf.FullName + "." + xMethodIntf.Name,
                                                            (from xParam in xMethodIntf.GetParameters()
                                                             select xParam.ParameterType).ToArray());

                        if (xActualMethod == null)
                        {
                            // get private implemenation
                            xActualMethod = xType.GetMethod(xMethodIntf.Name,
                                                            (from xParam in xMethodIntf.GetParameters()
                                                             select xParam.ParameterType).ToArray());
                        }
                        if (xActualMethod == null)
                        {
                            try
                            {
                                if (!xIntf.GetTypeInfo().IsGenericType)
                                {
                                    var xMap = xType.GetRuntimeInterfaceMap(xIntf);
                                    for (int k = 0; k < xMap.InterfaceMethods.Length; k++)
                                    {
                                        if (xMap.InterfaceMethods[k] == xMethodIntf)
                                        {
                                            xActualMethod = xMap.TargetMethods[k];
                                            break;
                                        }
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }
                        if (aMethodsSet.Contains(xMethodIntf))
                        {
                            if (!xEmittedMethods.ContainsKey(xMethodIntf))
                            {
                                xEmittedMethods.Add(xMethodIntf, true);
                            }
                        }

                    }
                }
                int? xBaseIndex = null;
                if (xType.BaseType == null)
                {
                    xBaseIndex = (int)aGetTypeID(xType);
                }
                else
                {
                    for (int t = 0; t < aTypesSet.Count; t++)
                    {
                        // todo: optimize check
                        var xItem = aTypesSet.Skip(t).First();
                        if (xItem.ToString() == xType.BaseType.ToString())
                        {
                            xBaseIndex = (int)aGetTypeID(xItem);
                            break;
                        }
                    }
                }
                if (xBaseIndex == null)
                {
                    throw new Exception("Base type not found!");
                }
                for (int x = xEmittedMethods.Count - 1; x >= 0; x--)
                {
                    if (!aMethodsSet.Contains(xEmittedMethods.Keys[x]))
                    {
                        xEmittedMethods.RemoveAt(x);
                    }
                }
                if (!xType.IsInterface)
                {
                    Move("VMT__TYPE_ID_HOLDER__" + DataMember.FilterStringForIncorrectChars(LabelName.GetFullName(xType) + " ASM_IS__" + xType.Assembly.GetName().Name), (int)aGetTypeID(xType));
                    XS.DataMember("VMT__TYPE_ID_HOLDER__" + DataMember.FilterStringForIncorrectChars(LabelName.GetFullName(xType) + " ASM_IS__" + xType.Assembly.GetName().Name), aGetTypeID(xType));
                    Push(aGetTypeID(xType));
                    Push((uint)xBaseIndex.Value);
                    xData = new byte[16 + (xEmittedMethods.Count * 4)];
                    xTemp = BitConverter.GetBytes(aGetTypeID(typeof(Array).GetTypeInfo()));
                    Array.Copy(xTemp, 0, xData, 0, 4);
                    xTemp = BitConverter.GetBytes(0x80000002); // embedded array
                    Array.Copy(xTemp, 0, xData, 4, 4);
                    xTemp = BitConverter.GetBytes(xEmittedMethods.Count); // embedded array
                    Array.Copy(xTemp, 0, xData, 8, 4);
                    xTemp = BitConverter.GetBytes(4); // embedded array
                    Array.Copy(xTemp, 0, xData, 12, 4);
                    string xDataName = "____SYSTEM____TYPE___" + DataMember.FilterStringForIncorrectChars(LabelName.GetFullName(xType) + " ASM_IS__" + xType.Assembly.GetName().Name) + "__MethodIndexesArray";
                    Cosmos.Assembler.Assembler.CurrentInstance.DataMembers.Add(new DataMember(xDataName, xData));
                    Push(xDataName);
                    Push(0);
                    xDataName = "____SYSTEM____TYPE___" + DataMember.FilterStringForIncorrectChars(LabelName.GetFullName(xType) + " ASM_IS__" + xType.Assembly.GetName().Name) + "__MethodAddressesArray";
                    Cosmos.Assembler.Assembler.CurrentInstance.DataMembers.Add(new DataMember(xDataName, xData));
                    Push(xDataName);
                    Push(0);
                    xData = new byte[16 + Encoding.Unicode.GetByteCount(xType.FullName + ", " + xType.Module.Assembly.GetName().FullName)];
                    xTemp = BitConverter.GetBytes(aGetTypeID(typeof(Array).GetTypeInfo()));
                    Array.Copy(xTemp, 0, xData, 0, 4);
                    xTemp = BitConverter.GetBytes(0x80000002); // embedded array
                    Array.Copy(xTemp, 0, xData, 4, 4);
                    xTemp = BitConverter.GetBytes((xType.FullName + ", " + xType.Module.Assembly.GetName().FullName).Length);
                    Array.Copy(xTemp, 0, xData, 8, 4);
                    xTemp = BitConverter.GetBytes(2); // embedded array
                    Array.Copy(xTemp, 0, xData, 12, 4);
                    xDataName = "____SYSTEM____TYPE___" + DataMember.FilterStringForIncorrectChars(LabelName.GetFullName(xType) + " ASM_IS__" + xType.Assembly.GetName().Name);
                    Cosmos.Assembler.Assembler.CurrentInstance.DataMembers.Add(new DataMember(xDataName, xData));
                    Push("0" + xEmittedMethods.Count.ToString("X") + "h");
                    Call(VTablesImplRefs.SetTypeInfoRef);
                }
                for (int j = 0; j < xEmittedMethods.Count; j++)
                {
                    MethodBase xMethod = xEmittedMethods.Keys[j];
#if VMT_DEBUG
                        xVmtDebugOutput.WriteStartElement("Method");
                        xVmtDebugOutput.WriteAttributeString("Id", aGetMethodUID(xMethod).ToString());
                        xVmtDebugOutput.WriteAttributeString("Name", xMethod.GetFullName());
                        xVmtDebugOutput.WriteEndElement();
#endif
                    var xMethodId = aGetMethodUID(xMethod);
                    if (!xType.IsInterface)
                    {
                        if (xEmittedMethods.Values[j])
                        {
                            var xNewMethod = xType.GetMethod(xMethod.DeclaringType.FullName + "." + xMethod.Name,
                                                                (from xParam in xMethod.GetParameters()
                                                                 select xParam.ParameterType).ToArray());

                            if (xNewMethod == null)
                            {
                                // get private implementation
                                xNewMethod = xType.GetMethod(xMethod.Name,
                                                                (from xParam in xMethod.GetParameters()
                                                                 select xParam.ParameterType).ToArray());
                            }
                            if (xNewMethod == null)
                            {
                                try
                                {
                                    var xMap = xType.GetRuntimeInterfaceMap(xMethod.DeclaringType);
                                    for (int k = 0; k < xMap.InterfaceMethods.Length; k++)
                                    {
                                        if (xMap.InterfaceMethods[k] == xMethod)
                                        {
                                            xNewMethod = xMap.TargetMethods[k];
                                            break;
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            }
                            xMethod = xNewMethod;
                        }

                        Push(aGetTypeID(xType));
                        Push((uint)j);

                        Push(xMethodId);
                        if (xMethod.IsAbstract)
                        {
                            // abstract methods dont have bodies, oiw, are not emitted
                            Push(0);
                        }
                        else
                        {
                            Push(ILOp.GetLabel(xMethod));
                        }
                        Call(VTablesImplRefs.SetMethodInfoRef);
                    }
                }
#if VMT_DEBUG
                    xVmtDebugOutput.WriteEndElement(); // type
#endif
            }
#if VMT_DEBUG
                xVmtDebugOutput.WriteEndElement(); // types
                xVmtDebugOutput.WriteEndDocument();
            }
#endif

            XS.Label("_END_OF_" + InitVMTCodeLabel);
            XS.Pop(EBP);
            XS.Return();
        }

        public void ProcessField(FieldInfo aField)
        {
            string xFieldName = DataMember.GetStaticFieldName(aField);
            if (Cosmos.Assembler.Assembler.CurrentInstance.DataMembers.Count(x => x.Name == xFieldName) == 0)
            {
                var xItemList = (from item in aField.GetCustomAttributes(false)
                                 where item.GetType().FullName == "ManifestResourceStreamAttribute"
                                 select item).ToList();

                object xItem = null;
                if (xItemList.Count > 0)
                    xItem = xItemList[0];
                string xManifestResourceName = null;
                if (xItem != null)
                {
                    var xItemType = xItem.GetType();
                    xManifestResourceName = (string)xItemType.GetTypeInfo().GetField("ResourceName").GetValue(xItem);
                }
                if (xManifestResourceName != null)
                {
                    // todo: add support for manifest streams again
                    //string xFileName = Path.Combine(mOutputDir,
                    //                                (xCurrentField.DeclaringType.Assembly.FullName + "__" + xManifestResourceName).Replace(",",
                    //                                                                                                                       "_") + ".res");
                    var xTarget = new StringBuilder();
                    using (var xStream = aField.DeclaringType.GetTypeInfo().Assembly.GetManifestResourceStream(xManifestResourceName))
                    {
                        if (xStream == null)
                        {
                            throw new Exception("Resource '" + xManifestResourceName + "' not found!");
                        }
                        xTarget.Append("0,");
                        // todo: abstract this array code out.
                        xTarget.Append((uint)ObjectUtils.InstanceTypeEnum.StaticEmbeddedArray);
                        xTarget.Append(",");
                        xTarget.Append((int)xStream.Length);
                        xTarget.Append(",");
                        xTarget.Append("1,");
                        while (xStream.Position < xStream.Length)
                        {
                            xTarget.Append(xStream.ReadByte());
                            xTarget.Append(",");
                        }
                        xTarget.Append(",");
                    }

                    Assembler.DataMembers.Add(new DataMember("___" + xFieldName + "___Contents",
                                                              "db",
                                                              xTarget));
                    Assembler.DataMembers.Add(new DataMember(xFieldName,
                                                              "dd",
                                                              "___" + xFieldName + "___Contents"));
                }
                else
                {
                    var xFieldType = aField.FieldType.GetTypeInfo();
                    uint xFieldSize = ILOp.SizeOfType(aField.FieldType);
                    byte[] xData = new byte[xFieldSize];

                    if (xFieldType.IsValueType)
                    {
                        DebugSymbolReader.TryGetStaticFieldValue(aField.Module, aField.MetadataToken, ref xData);
                    }

                    var xAsmLabelAttributes = aField.GetCustomAttributes<AsmLabelAttribute>();
                    if (xAsmLabelAttributes != null && xAsmLabelAttributes.Count() > 0)
                    {
                        Assembler.DataMembers.Add(new DataMember(
                            xFieldName, xAsmLabelAttributes.Select(a => a.Label), xData));
                    }
                    else
                    {
                        Assembler.DataMembers.Add(new DataMember(xFieldName, xData));
                    }
                }
            }
        }

        /// <summary>
        /// Generates a forwarding stub, which transforms from the actual method to the plug.
        /// </summary>
        /// <param name="aFrom">The method to forward to the plug</param>
        /// <param name="aTo">The plug</param>
        internal void GenerateMethodForward(_MethodInfo aFrom, _MethodInfo aTo)
        {
            var xMethodLabel = ILOp.GetLabel(aFrom);
            var xEndOfMethodLabel = xMethodLabel + EndOfMethodLabelNameNormal;

            // todo: completely get rid of this kind of trampoline code
            MethodBegin(aFrom);
            {
                var xExtraSpaceToSkipDueToObjectPointerAccess = 0u;

                var xFromParameters = aFrom.MethodBase.GetParameters();
                var xParams = aTo.MethodBase.GetParameters().ToArray();
                if (aTo.IsWildcard)
                {
                    xParams = aFrom.MethodBase.GetParameters();
                }

                int xCurParamIdx = 0;
                var xCurParamOffset = 0;
                if (!aFrom.MethodBase.IsStatic)
                {
                    Ldarg(aFrom, 0);

                    if (!aTo.IsWildcard)
                    {
                        var xObjectPointerAccessAttrib = xParams[0].GetCustomAttribute<ObjectPointerAccessAttribute>(true);
                        if (xObjectPointerAccessAttrib != null)
                        {
                            XS.Comment("Skipping the reference to the next object reference.");
                            XS.Add(ESP, 4);
                            xExtraSpaceToSkipDueToObjectPointerAccess += 4;
                        }
                        else
                        {
                            if (ILOp.TypeIsReferenceType(aFrom.MethodBase.DeclaringType) && !ILOp.TypeIsReferenceType(xParams[0].ParameterType))
                            {
                                throw new Exception("Original method argument $this is a reference type. Plug attribute first argument is not an argument type, nor was it marked with ObjectPointerAccessAttribute! Method: " + aFrom.MethodBase.GetFullName() + " Parameter: " + xParams[0].Name);
                            }
                        }

                        xParams = xParams.Skip(1).ToArray();
                    }
                    xCurParamOffset = 1;
                }

                var xOriginalParamsIdx = 0;
                foreach (var xParam in xParams)
                {
                    var xFieldAccessAttrib = xParam.GetCustomAttribute<FieldAccessAttribute>(true);
                    var xObjectPointerAccessAttrib = xParam.GetCustomAttribute<ObjectPointerAccessAttribute>(true);
                    if (xFieldAccessAttrib != null)
                    {
                        // field access
                        XS.Comment("Loading address of field '" + xFieldAccessAttrib.Name + "'");
                        var xFieldInfo = ResolveField(aFrom, xFieldAccessAttrib.Name, false);
                        if (xFieldInfo.IsStatic)
                        {
                            Ldsflda(aFrom, xFieldInfo);
                        }
                        else
                        {
                            Ldarg(aFrom, 0);
                            Ldflda(aFrom, xFieldInfo);
                        }
                    }
                    else if (xObjectPointerAccessAttrib != null)
                    {
                        xOriginalParamsIdx++;
                        Ldarg(aFrom, xCurParamIdx + xCurParamOffset);
                        XS.Add(ESP, 4);
                        xExtraSpaceToSkipDueToObjectPointerAccess += 4;
                    }
                    else
                    {
                        if (ILOp.TypeIsReferenceType(xFromParameters[xOriginalParamsIdx].ParameterType) && !ILOp.TypeIsReferenceType(xParams[xCurParamIdx].ParameterType))
                        {
                            throw new Exception("Original method argument $this is a reference type. Plug attribute first argument is not an argument type, nor was it marked with ObjectPointerAccessAttribute! Method: " + aFrom.MethodBase.GetFullName() + " Parameter: " + xParam.Name);
                        }
                        // normal field access
                        XS.Comment("Loading parameter " + (xCurParamIdx + xCurParamOffset));
                        Ldarg(aFrom, xCurParamIdx + xCurParamOffset);
                        xCurParamIdx++;
                        xOriginalParamsIdx++;
                    }
                }
                Call(aFrom, aTo, xEndOfMethodLabel);
            }
            MethodEnd(aFrom);
        }

        protected static void WriteDebug(MethodBase aMethod, uint aSize, uint aSize2)
        {
            var xLine = String.Format("{0}\t{1}\t{2}", LabelName.GetFullName(aMethod), aSize, aSize2);
        }

        // These are all temp functions until we move to the new assembler.
        // They are used to clean up the old assembler slightly while retaining compatibiltiy for now
        public static string TmpPosLabel(_MethodInfo aMethod, int aOffset)
        {
            return ILOp.GetLabel(aMethod, aOffset);
        }

        public static string TmpPosLabel(_MethodInfo aMethod, ILOpCode aOpCode)
        {
            return TmpPosLabel(aMethod, aOpCode.Position);
        }

        public static string TmpBranchLabel(_MethodInfo aMethod, ILOpCode aOpCode)
        {
            return TmpPosLabel(aMethod, ((OpBranch)aOpCode).Value);
        }

        public void EmitEntrypoint(MethodBase aEntrypoint)
        {
            // at the time the datamembers for literal strings are created, the type id for string is not yet determined.
            // for now, we fix this at runtime.
            XS.Label(InitStringIDsLabel);
            XS.Push(EBP);
            XS.Set(EBP, ESP);
            XS.Set(EAX, ILOp.GetTypeIDLabel(typeof(String)), sourceIsIndirect: true);
            new Mov { DestinationRef = ElementReference.New("static_field__System_String_Empty"), DestinationIsIndirect = true, SourceRef = ElementReference.New(LdStr.GetContentsArrayName("")), DestinationDisplacement = 4 };

            var xMemberId = 0;

            foreach (var xDataMember in Assembler.DataMembers)
            {
                if (!xDataMember.Name.StartsWith("StringLiteral"))
                {
                    continue;
                }
                if (xDataMember.Name.EndsWith("__Handle"))
                {
                    continue;
                }
                if (xMemberId % 100 == 0)
                {
                    Assembler.WriteDebugVideo(".");
                }
                xMemberId++;
                new Mov { DestinationRef = ElementReference.New(xDataMember.Name), DestinationIsIndirect = true, SourceReg = RegistersEnum.EAX };
            }
            Assembler.WriteDebugVideo("Done");
            XS.Pop(EBP);
            XS.Return();

            XS.Label(CosmosAssembler.EntryPointName);
            XS.Push(EBP);
            XS.Set(EBP, ESP);
            Assembler.WriteDebugVideo("Initializing VMT.");
            XS.Call(InitVMTCodeLabel);
            Assembler.WriteDebugVideo("Initializing string IDs.");
            XS.Call(InitStringIDsLabel);
            Assembler.WriteDebugVideo("Done initializing string IDs");
            // we now need to do "newobj" on the entry point, and after that, call .Start on it
            var xCurLabel = CosmosAssembler.EntryPointName + ".CreateEntrypoint";
            XS.Label(xCurLabel);
            Assembler.WriteDebugVideo("Now create the kernel class");
            Newobj.Assemble(Cosmos.Assembler.Assembler.CurrentInstance, null, null, xCurLabel, aEntrypoint.DeclaringType, aEntrypoint);
            Assembler.WriteDebugVideo("Kernel class created");
            xCurLabel = CosmosAssembler.EntryPointName + ".CallStart";
            XS.Label(xCurLabel);
            X86.IL.Call.DoExecute(Assembler, null, aEntrypoint.DeclaringType.GetTypeInfo().BaseType.GetMethod(CompilerEngine.UseGen3Kernel ? "EntryPoint" : "Start"), null, xCurLabel, CosmosAssembler.EntryPointName + ".AfterStart", DebugEnabled);
            XS.Label(CosmosAssembler.EntryPointName + ".AfterStart");
            XS.Pop(EBP);
            XS.Return();

            if (ShouldOptimize)
            {
                Optimizer.Optimize(Assembler);
            }
        }

        protected void AfterOp(_MethodInfo aMethod, ILOpCode aOpCode)
        {
        }

        protected void BeforeOp(_MethodInfo aMethod, ILOpCode aOpCode, bool emitInt3NotNop, out bool INT3Emitted, bool hasSourcePoint)
        {
            string xLabel = TmpPosLabel(aMethod, aOpCode);
            Assembler.CurrentIlLabel = xLabel;
            XS.Label(xLabel);

            if (aMethod.MethodBase.DeclaringType != typeof(VTablesImpl))
            {
                Assembler.EmitAsmLabels = false;
                try
                {
                    //Assembler.WriteDebugVideo(String.Format("Method {0}:{1}.", aMethod.UID, aOpCode.Position.ToString("X")));
                    //Assembler.WriteDebugVideo(xLabel);
                }
                finally
                {
                    Assembler.EmitAsmLabels = true;
                }
            }

            uint? xStackDifference = null;

            if (mSymbols != null)
            {
                var xMLSymbol = new MethodIlOp();
                xMLSymbol.LabelName = xLabel;

                var xStackSize = aOpCode.StackOffsetBeforeExecution.Value;

                xMLSymbol.StackDiff = -1;
                if (aMethod.MethodBase != null)
                {
                    var xLocals = DebugSymbolReader.GetLocalVariableInfos(aMethod.MethodBase);
                        var xLocalsSize = (from item in xLocals
                                           select ILOp.Align(ILOp.SizeOfType(item.Type), 4)).Sum();
                        xMLSymbol.StackDiff = checked((int)(xLocalsSize + xStackSize));
                        xStackDifference = (uint?)xMLSymbol.StackDiff;
                }
                xMLSymbol.IlOffset = aOpCode.Position;
                xMLSymbol.MethodID = mCurrentMethodGuid;

                mSymbols.Add(xMLSymbol);
                DebugInfo.AddSymbols(mSymbols);
            }
            DebugInfo.AddSymbols(mSymbols, false);

            bool INT3PlaceholderEmitted = false;
            EmitTracer(aMethod, aOpCode, aMethod.MethodBase.DeclaringType.Namespace, emitInt3NotNop, out INT3Emitted, out INT3PlaceholderEmitted, hasSourcePoint);

            if (INT3Emitted || INT3PlaceholderEmitted)
            {
                var xINT3Label = new INT3Label();
                xINT3Label.LabelName = xLabel;
                xINT3Label.MethodID = mCurrentMethodGuid;
                xINT3Label.LeaveAsINT3 = INT3Emitted;
                mINT3Labels.Add(xINT3Label);
                DebugInfo.AddINT3Labels(mINT3Labels);
            }

            if (DebugEnabled && StackCorruptionDetection && StackCorruptionDetectionLevel == StackCorruptionDetectionLevel.AllInstructions)
            {
                // if debugstub is active, emit a stack corruption detection. at this point, the difference between EBP and ESP
                // should be equal to the local variables sizes and the IL stack.
                // if not, we should break here.

                // first, calculate the expected difference
                if (xStackDifference == null)
                {
                    xStackDifference = aMethod.LocalVariablesSize;
                    xStackDifference += aOpCode.StackOffsetBeforeExecution;
                }

                XS.Comment("Stack difference = " + xStackDifference);

                // if debugstub is active, emit a stack corruption detection. at this point EBP and ESP should have the same value.
                // if not, we should somehow break here.
                XS.Set(EAX, ESP);
                XS.Set(EBX, EBP);
                if (xStackDifference != 0)
                {
                    XS.Add(EAX, xStackDifference.Value);
                }
                XS.Compare(EAX, EBX);
                XS.Jump(ConditionalTestEnum.Equal, xLabel + ".StackCorruptionCheck_End");
                XS.Push(EAX);
                XS.Push(EBX);
                XS.Call("DebugStub_SendSimpleNumber");
                XS.Add(ESP, 4);
                XS.Call("DebugStub_SendSimpleNumber");

                XS.ClearInterruptFlag();
                // don't remove the call. It seems pointless, but we need it to retrieve the EIP value
                XS.Call(xLabel + ".StackCorruptionCheck_GetAddress");
                XS.Label(xLabel + ".StackCorruptionCheck_GetAddress");
                XS.Pop(EAX);
                new Mov { DestinationRef = ElementReference.New("DebugStub_CallerEIP"), DestinationIsIndirect = true, SourceReg = RegistersEnum.EAX };
                XS.Call("DebugStub_SendStackCorruptionOccurred");
                XS.Halt();
                XS.Label(xLabel + ".StackCorruptionCheck_End");
            }
        }

        protected void EmitTracer(_MethodInfo aMethod, ILOpCode aOp, string aNamespace, bool emitInt3NotNop, out bool INT3Emitted, out bool INT3PlaceholderEmitted, bool isNewSourcePoint)
        {
            // NOTE - These if statements can be optimized down - but clarity is
            // more important than the optimizations. Furthermore the optimizations available
            // would not offer much benefit

            // Determine if a new DebugStub should be emitted

            INT3Emitted = false;
            INT3PlaceholderEmitted = false;

            if (aOp.OpCode == ILOpCode.Code.Nop)
            {
                // Skip NOOP's so we dont have breakpoints on them
                //TODO: Each IL op should exist in IL, and descendants in IL.X86.
                // Because of this we have this hack
                return;
            }
            else if (DebugEnabled == false)
            {
                return;
            }
            else if (DebugMode == DebugMode.Source)
            {
                // If the current position equals one of the offsets, then we have
                // reached a new atomic C# statement
                if (!isNewSourcePoint)
                {
                    return;
                }
            }

            // Check if the DebugStub has been disabled for this method
            if ((!IgnoreDebugStubAttribute) && (aMethod.DebugStubOff))
            {
                return;
            }

            // This test fixes issue #15638
            if (null != aNamespace)
            {
                // Check options for Debug RingEnum
                // Set based on TracedAssemblies
                if (TraceAssemblies > TraceAssemblies.None)
                {
                    if (TraceAssemblies < TraceAssemblies.All)
                    {
                        if (aNamespace.StartsWith("System.", StringComparison.OrdinalIgnoreCase))
                        {
                            return;
                        }
                        if (aNamespace.ToLower() == "system")
                        {
                            return;
                        }
                        if (aNamespace.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase))
                        {
                            return;
                        }
                    }

                    if (TraceAssemblies < TraceAssemblies.Cosmos)
                    {
                        //TODO: Maybe an attribute that could be used to turn tracing on and off
                        if (aNamespace.StartsWith("Cosmos.", StringComparison.OrdinalIgnoreCase))
                        {
                            return;
                        }
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
            // If we made it this far without a return, emit the Tracer
            // We used to emit an INT3, but this meant the DS would brwak after every C# line
            // Breaking that frequently is of course, pointless and slow.
            // So now we emit mostly NOPs and only put an INT3 when told to.
            // We should only be told to put an INT3 at the start of method but this may change so search for more comments on this.
            if (emitInt3NotNop)
            {
                INT3Emitted = true;
                XS.Int3();
            }
            else
            {
                INT3PlaceholderEmitted = true;
                XS.DebugNoop();
            }
        }
    }
}
