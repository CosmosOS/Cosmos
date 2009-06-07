using System;
using System.Reflection;
using System.Linq;
using CPUx86=Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Compiler;
using CPU=Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86
{
    public abstract class Op : IL.Op
    {
        private bool mNeedsExceptionPush;
        private Type mCatchType;
        private string mNextInstructionLabel;
        private bool mNeedsTypeCheck = false;

        /// <summary>
        /// Emits code for checking a given address for null, and emits a "throw new NullRefException();" if so.
        /// </summary>
        /// <param name="aAssembler"></param>
        /// <param name="aAddress"></param>
        public static void EmitCompareWithNull(Assembler.Assembler aAssembler,
                                               MethodInformation aMethodInfo,
                                               Action<CPUx86.Compare> aInitAddress,
                                               string aCurrentLabel,
                                               string aNextLabel,
                                               Action aEmitCleanupMethod,
                                               int aCurrentILOffset,
                                               string aNullRefExcTypeId,
                                                TypeInformation aNullRefExcTypeInfo,
                                                MethodInformation aNullRefExcCtorMethodInfo,
                                                IServiceProvider aServiceProvider)
        {
            //new CPUx86.Move("ecx",
            //"[esp]");
            aInitAddress(new CPUx86.Compare { SourceValue = 0, Size = 32 });
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Zero, DestinationLabel = aCurrentLabel + "_Step1" };
            new CPUx86.Jump { DestinationLabel = aNextLabel };
            //new CPUx86.JumpIfNotEqual(aNextLabel);
            new CPU.Label(aCurrentLabel + "_Step1");
            Type xNullRefExcType = typeof(NullReferenceException);
            var xAllocInfo = aServiceProvider.GetService<IMetaDataInfoService>().GetMethodInfo(GCImplementationRefs.AllocNewObjectRef,
                                                                              false);
            Newobj.Assemble(aAssembler,
                            xNullRefExcType.GetConstructor(new Type[0]),
                            aNullRefExcTypeId,
                            aCurrentLabel,
                            aMethodInfo,
                            aCurrentILOffset,
                            aCurrentLabel + "__After_NullRef_ctor",
                            aNullRefExcTypeInfo,
                            aNullRefExcCtorMethodInfo, 
                            aServiceProvider,
                            xAllocInfo.LabelName);
            new CPU.Label(aCurrentLabel + "__After_NullRef_ctor");
            aAssembler.StackContents.Pop();
            var xCurrExcLabel = aServiceProvider.GetService<IMetaDataInfoService>().GetStaticFieldLabel(CPU.Assembler.CurrentExceptionRef);
            new CPUx86.Move
            {
                DestinationRef = ElementReference.New(xCurrExcLabel),
                DestinationIsIndirect = true,
                SourceReg = CPUx86.Registers.EAX
            };
            var xExcOccurrentInfo =
                aServiceProvider.GetService<IMetaDataInfoService>().GetMethodInfo(
                    CPU.Assembler.CurrentExceptionOccurredRef, false);
            new CPUx86.Call { DestinationLabel = xExcOccurrentInfo.LabelName };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceValue = 3 };
            aEmitCleanupMethod();
            Call.EmitExceptionLogic(aAssembler,
                                    (uint)aCurrentILOffset,
                                    aMethodInfo,
                                    aNextLabel,
                                    false,
                                    null);
        }

        public Op(ILReader aReader,
                  MethodInformation aMethodInfo)
            : base(aReader,
                   aMethodInfo)
        {
            if (aReader != null && aReader.Position==0x4A)
            {
             Console.Write("");
            }
            if (aMethodInfo != null && aMethodInfo.CurrentHandler != null)
            {
                mNeedsExceptionPush = ((aMethodInfo.CurrentHandler.HandlerOffset > 0 && aMethodInfo.CurrentHandler.HandlerOffset == aReader.Position) || ((aMethodInfo.CurrentHandler.Flags & ExceptionHandlingClauseOptions.Filter) > 0 && aMethodInfo.CurrentHandler.FilterOffset > 0 && aMethodInfo.CurrentHandler.FilterOffset == aReader.Position)) && (aMethodInfo.CurrentHandler.Flags == ExceptionHandlingClauseOptions.Clause);
                // todo: add support for exception clear again
                //mNeedsExceptionClear = ((aMethodInfo.CurrentHandler.HandlerOffset + aMethodInfo.CurrentHandler.HandlerLength) == (aReader.Offset + 1)) || 
                //    ((aMethodInfo.CurrentHandler.FilterOffset+aMethodInfo.CurrentHandler.Filterle == (aReader.Offset + 1));
                if (mNeedsExceptionPush && aMethodInfo.CurrentHandler.CatchType != null)
                {
                    mCatchType = aMethodInfo.CurrentHandler.CatchType;
                }
            }
            if (mCatchType != null && mCatchType.FullName != "System.Exception")
            {
                var xHandler = (from item in aMethodInfo.Method.GetMethodBody().ExceptionHandlingClauses
                                where item.TryOffset == aMethodInfo.CurrentHandler.TryOffset && item.TryLength == aMethodInfo.CurrentHandler.TryLength && item.HandlerOffset == aMethodInfo.CurrentHandler.HandlerOffset && item.Flags == ExceptionHandlingClauseOptions.Clause
                                select item).FirstOrDefault();
                if (xHandler != null)
                {
                    mNextInstructionLabel = GetInstructionLabel(xHandler.HandlerOffset);
                }
                else
                {
                    // Here we need to detect where to leave to when this catch clause doesnt' handle a specific exception, and is the last one
                    throw new NotImplementedException("TODO: Implement exiting here!");
                }
            }
            else
            {
                mCatchType = null;
            }
            if (mCatchType != null && aMethodInfo != null && aMethodInfo.CurrentHandler != null && aMethodInfo.CurrentHandler.HandlerOffset > 0)
            {
                mNeedsTypeCheck = aMethodInfo.CurrentHandler.HandlerOffset == aReader.NextPosition;
            }
        }

        public static void Ldarg(Assembler.Assembler aAssembler,
                                 MethodInformation.Argument aArg)
        {
            Ldarg(aAssembler,
                  aArg,
                  true);
        }

        public static void Ldarg(Assembler.Assembler aAssembler,
                                 MethodInformation.Argument aArg,
                                 bool aAddGCCode)
        {
            foreach (int xAddress in aArg.VirtualAddresses.Reverse())
            {
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = xAddress };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            }
            aAssembler.StackContents.Push(new StackContent((int)aArg.Size,
                                                           aArg.ArgumentType));
            if (aAddGCCode && aArg.IsReferenceType)
            {
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.IncRefCountRef) };
            }
        }

        public static void Ldflda(Assembler.Assembler aAssembler,
                                  TypeInformation aType,
                                  TypeInformation.Field aField)
        {
            Ldflda(aAssembler,
                   aType,
                   aField,
                   true);
        }

        public static void Ldflda(Assembler.Assembler aAssembler,
                                  TypeInformation aType,
                                  TypeInformation.Field aField,
                                  bool aDerefExternalAddress)
        {
            int aExtraOffset = 0;
            if (aType.NeedsGC)
            {
                aExtraOffset = 12;
            }
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };

            new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = (uint)(aField.Offset + aExtraOffset) };
            aAssembler.StackContents.Pop();
            aAssembler.StackContents.Push(new StackContent(4,
                                                           aField.FieldType));
            if (aDerefExternalAddress && aField.IsExternalField)
            {
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
            }
            else
            {
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            }
        }

        public static void EmitNotImplementedException(Assembler.Assembler aAssembler, IServiceProvider aServiceProvider, string aMessage, string aCurrentLabel, MethodInformation aCurrentMethodInfo, uint aCurrentOffset, string aNextLabel)
        {
            EmitException(aAssembler, aServiceProvider, typeof (NotImplementedException), aMessage,
                          aCurrentLabel, aCurrentMethodInfo, aCurrentOffset, aNextLabel);
        }

        public static void EmitNotSupportedException(Assembler.Assembler aAssembler, IServiceProvider aServiceProvider, string aMessage, string aCurrentLabel, MethodInformation aCurrentMethodInfo, uint aCurrentOffset, string aNextLabel)
        {
            EmitException(aAssembler, aServiceProvider, typeof(NotSupportedException), aMessage,
                          aCurrentLabel, aCurrentMethodInfo, aCurrentOffset, aNextLabel);
        }

        private static void EmitException(Assembler.Assembler aAssembler, IServiceProvider aServiceProvider, Type aException, string aMessage, string aCurrentLabel, MethodInformation aCurrentMethodInfo, uint aCurrentOffset, string aNextLabel)
        {
            var xLdStr = new LdStr(aMessage);
            xLdStr.SetServiceProvider(aServiceProvider);
            xLdStr.Assembler = aAssembler;
            xLdStr.Assemble();
            var xAllocInfo = aServiceProvider.GetService<IMetaDataInfoService>().GetMethodInfo(GCImplementationRefs.AllocNewObjectRef,
                                                                              false);
            Newobj.Assemble(aAssembler,
                            aException.GetConstructor(new Type[] {typeof (string)}),
                            aServiceProvider.GetService<IMetaDataInfoService>().GetTypeIdLabel(aException),
                            aCurrentLabel,
                            aCurrentMethodInfo,
                            (int)aCurrentOffset,
                            aNextLabel,
                            aServiceProvider.GetService<IMetaDataInfoService>().GetTypeInfo(aException),
                            aServiceProvider.GetService<IMetaDataInfoService>().GetMethodInfo(
                                aException.GetConstructor(new Type[] {typeof (string)}), false),
                            aServiceProvider,
                            xAllocInfo.LabelName);
        }

        public static void Multiply(Assembler.Assembler aAssembler, IServiceProvider aServiceProvider, string aCurrentLabel, MethodInformation aCurrentMethodInfo, uint aCurrentOffset, string aNextLabel)
        {
            StackContent xStackContent = aAssembler.StackContents.Pop();
            new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
            if (xStackContent.IsFloat)
            {
                EmitNotSupportedException(aAssembler, aServiceProvider, "Floats are not yet supported!", aCurrentLabel,
                                          aCurrentMethodInfo, aCurrentOffset, aNextLabel);
                aServiceProvider.GetService<IMetaDataInfoService>().LogMessage(LogSeverityEnum.Error, "Floats are not yet supported!");
            }
            else
            {
                if (xStackContent.Size > 4)
                {
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                    new CPUx86.Multiply { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 32 };
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
                    new CPUx86.Push { DestinationValue = 0 };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                }
                else
                {
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Multiply { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 32 };
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                }
            }
        }

        public static void Ldfld(Assembler.Assembler aAssembler,
                                 TypeInformation aType,
                                 string aFieldName)
        {
            Ldfld(aAssembler,
                  aType,
                  aType.Fields[aFieldName]);
        }

        public static void Ldfld(Assembler.Assembler aAssembler,
                                 TypeInformation aType,
                                 TypeInformation.Field aField)
        {
            Ldfld(aAssembler,
                  aType,
                  aField,
                  true);
        }

        public static void Ldfld(Assembler.Assembler aAssembler,
                                 TypeInformation aType,
                                 TypeInformation.Field aField,
                                 bool aAddGCCode)
        {
            Ldfld(aAssembler,
                  aType,
                  aField,
                  aAddGCCode,
                  true);
        }

        public static void Ldfld(Assembler.Assembler aAssembler,
                                 TypeInformation aType,
                                 TypeInformation.Field aField,
                                 bool aAddGCCode,
                                 bool aDerefExternalField)
        {
            aAssembler.StackContents.Pop();
            int aExtraOffset = 0;
            if (aType.NeedsGC)
            {
                aExtraOffset = 12;
            }
            new Comment("Type = '" + aType.TypeDef.FullName + "', NeedsGC = " + aType.NeedsGC);
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ECX, SourceValue = (uint)(aField.Offset + aExtraOffset) };
            if (aField.IsExternalField && aDerefExternalField)
            {
                new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true };
            }
            for (int i = 1; i <= (aField.Size / 4); i++)
            {
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true, SourceDisplacement = (aField.Size - (i * 4)) };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            }
            switch (aField.Size % 4)
            {
                case 1:
                    {
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.AL, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                        break;
                    }
                case 2:
                    {
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.AX, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                        break;
                    }

                case 3: //For Release
                    {
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true };
                        new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.EAX, SourceValue = 8 };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                        break;
                    }
                case 0:
                    {
                        break;
                    }
                default:
                    throw new Exception("Remainder size "  +aField.FieldType.ToString() + (aField.Size) + " not supported!");
            }
            if (aAddGCCode && aField.NeedsGC)
            {
                new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
                new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.IncRefCountRef) };
            }
            aAssembler.StackContents.Push(new StackContent(aField.Size,
                                                           aField.FieldType));
        }

        public static void Stfld(Assembler.Assembler aAssembler,
                                 TypeInformation aType,
                                 TypeInformation.Field aField)
        {
            aAssembler.StackContents.Pop();
            int xRoundedSize = aField.Size;
            if (xRoundedSize % 4 != 0)
            {
                xRoundedSize += 4 - (xRoundedSize % 4);
            }
            int aExtraOffset = 0;
            if (aType.NeedsGC)
            {
                aExtraOffset = 12;
            }
            if (aField.NeedsGC)
            {
                new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4 };
                //Ldfld(aAssembler, aType, aField, false);
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = (aField.Offset + aExtraOffset) };
                new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.DecRefCountRef) };
            }
            new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = xRoundedSize };
            new CPUx86.Add
            {
                DestinationReg = CPUx86.Registers.ECX,
                SourceValue = (uint)(aField.Offset + aExtraOffset)
            };
            for (int i = 0; i < (aField.Size / 4); i++)
            {
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = i * 4, SourceReg = CPUx86.Registers.EAX };
            }
            switch (aField.Size % 4)
            {
                case 1:
                    {
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = ((aField.Size / 4) * 4), SourceReg = CPUx86.Registers.AL };
                        break;
                    }
                case 2:
                    {
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = ((aField.Size / 4) * 4), SourceReg = CPUx86.Registers.AX };
                        break;
                    }

                case 3: //TODO 
                    break;
                case 0:
                    {
                        break;
                    }
                default:
                    throw new Exception("Remainder size " + (aField.Size % 4) + " not supported!");
            }
            if (aField.NeedsGC)
            {
                new CPUx86.Push { DestinationReg = CPUx86.Registers.ECX };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.DecRefCountRef) };
                new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.DecRefCountRef) };
            }
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
            aAssembler.StackContents.Pop();
        }

        public static void AddWithOverflow(Assembler.Assembler aAssembler,
                                           bool signed)
        {
            //Emit
            throw new NotImplementedException();
            //Add(aAssembler);
            throw new NotImplementedException();
            //if (signed)
            //{
            //    new CPUx86.Interrupt(CPUx86.Interrupt.INTO);
            //} else
            //{

            //}
        }

        public static void Add(Assembler.Assembler aAssembler, IServiceProvider aServiceProvider, string aCurrentLabel, MethodInformation aCurrentMethodInfo, uint aCurrentOffset, string aNextLabel)
        {
            StackContent xSize = aAssembler.StackContents.Pop();
            if (xSize.IsFloat)
            {

                if (xSize.Size > 4)
                {
                    EmitNotImplementedException(aAssembler, aServiceProvider, "Doubles not yet supported (add)", aCurrentLabel, aCurrentMethodInfo, aCurrentOffset, aNextLabel);
                    return;
                }
                else
                {
                    new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                    new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                    new CPUx86.SSE.AddSS { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.XMM1 };
                    new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.XMM0 };
                }
            }
            if (xSize.Size > 8)
            {
                EmitNotImplementedException(aAssembler, aServiceProvider, "Size '" + xSize.Size + "' not supported (add)", aCurrentLabel, aCurrentMethodInfo, aCurrentOffset, aNextLabel);
                return;
            }
            if (xSize.Size > 4)
            {
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
                new CPUx86.AddWithCarry { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceReg = CPUx86.Registers.EDX };
            }
            else
            {
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
            }
        }

        public static void Ldloc(Assembler.Assembler aAssembler,
                                 MethodInformation.Variable aLocal, uint aStorageSize)
        {
            Ldloc(aAssembler,
                  aLocal,
                  true, aStorageSize);
        }

        public static void Ldloc(Assembler.Assembler aAssembler,
                                 MethodInformation.Variable aLocal,
                                 bool aAddGCCode,
                                 uint aStorageSize)
        {
            if (aLocal.VirtualAddresses.Length > 1)
            {
                foreach (int s in aLocal.VirtualAddresses)
                {
                    new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = s };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                }
            }
            else
            {
                new CPUx86.Xor { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EAX };

                switch (aStorageSize)
                {
                    case 1:
                        {
                            new CPUx86.Move { DestinationReg = CPUx86.Registers.AL, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = aLocal.VirtualAddresses.First() };
                            break;
                        }
                    case 2:
                        {
                            new CPUx86.Move { DestinationReg = CPUx86.Registers.AX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = aLocal.VirtualAddresses.First() };

                            break;
                        }
                    case 4:
                        {
                            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = aLocal.VirtualAddresses.First() };
                            break;
                        }
                }
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                if (aAddGCCode && aLocal.IsReferenceType)
                {
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.IncRefCountRef) };
                }
            }
            aAssembler.StackContents.Push(new StackContent(aLocal.Size,
                                                           aLocal.VariableType));
        }

        protected override void AssembleHeader()
        {
            base.AssembleHeader();
            new Comment("Next Instruction = " + mNextInstructionLabel);
            string xCurExceptionFieldName = DataMember.GetStaticFieldName(IL2CPU.Assembler.Assembler.CurrentExceptionRef);
            if (mNeedsTypeCheck)
            {
                // call VTablesImpl.IsInstance to see the actual instance name..
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceRef = ElementReference.New(xCurExceptionFieldName), SourceIsIndirect = true };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
                new CPUx86.Push { DestinationRef = ElementReference.New(GetService<IMetaDataInfoService>().GetTypeIdLabel(mCatchType)), DestinationIsIndirect = true };
                var xIsInstInfo = GetService<IMetaDataInfoService>().GetMethodInfo(VTablesImplRefs.IsInstanceRef, false);
                new CPUx86.Call { DestinationLabel = xIsInstInfo.LabelName };
                new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = mNextInstructionLabel };
            }
            if (mNeedsExceptionPush)
            {
                new CPUx86.Push { DestinationRef = ElementReference.New(xCurExceptionFieldName), DestinationIsIndirect = true };
                Assembler.StackContents.Push(new StackContent(4,
                                                              typeof(Exception)));
            }
        }
    }
}
