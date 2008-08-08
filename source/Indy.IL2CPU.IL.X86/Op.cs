using System;
using System.Linq;
using System.Reflection;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
    public abstract class Op : IL.Op {
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
                                               string aAddress,
                                               string aCurrentLabel,
                                               string aNextLabel,
                                               Action aEmitCleanupMethod,
                                               int aCurrentILOffset) {
            new CPUx86.Compare("dword " + aAddress,
                               "0");
            new CPUx86.JumpIfNotEqual(aNextLabel);
            Type xNullRefExcType = typeof(NullReferenceException);
            Newobj.Assemble(aAssembler,
                            xNullRefExcType.GetConstructor(new Type[0]),
                            Engine.RegisterType(xNullRefExcType),
                            aCurrentLabel,
                            aMethodInfo,
                            aCurrentILOffset);
            aAssembler.StackContents.Pop();
            new CPUx86.Move("[" + DataMember.GetStaticFieldName(CPU.Assembler.CurrentExceptionRef) + "]",
                            "eax");
            Engine.QueueMethod(CPU.Assembler.CurrentExceptionOccurredRef);
            new CPUx86.Call(Label.GenerateLabelName(CPU.Assembler.CurrentExceptionOccurredRef));
            new CPUx86.Move("ecx",
                            "3");
            aEmitCleanupMethod();
            Call.EmitExceptionLogic(aAssembler,
                                    aCurrentILOffset,
                                    aMethodInfo,
                                    aNextLabel,
                                    false,
                                    null);
        }

        public Op(ILReader aReader,
                  MethodInformation aMethodInfo)
            : base(aReader,
                   aMethodInfo) {
            if (aMethodInfo != null && aMethodInfo.CurrentHandler != null) {
                mNeedsExceptionPush = ((aMethodInfo.CurrentHandler.HandlerOffset > 0 && aMethodInfo.CurrentHandler.HandlerOffset == aReader.Position) || ((aMethodInfo.CurrentHandler.Flags & ExceptionHandlingClauseOptions.Filter) > 0 && aMethodInfo.CurrentHandler.FilterOffset > 0 && aMethodInfo.CurrentHandler.FilterOffset == aReader.Position)) && (aMethodInfo.CurrentHandler.Flags == ExceptionHandlingClauseOptions.Clause);
                // todo: add support for exception clear again
                //mNeedsExceptionClear = ((aMethodInfo.CurrentHandler.HandlerOffset + aMethodInfo.CurrentHandler.HandlerLength) == (aReader.Offset + 1)) || 
                //    ((aMethodInfo.CurrentHandler.FilterOffset+aMethodInfo.CurrentHandler.Filterle == (aReader.Offset + 1));
                if (mNeedsExceptionPush && aMethodInfo.CurrentHandler.CatchType != null) {
                    mCatchType = aMethodInfo.CurrentHandler.CatchType;
                }
            }
            if (mCatchType != null && mCatchType.FullName != "System.Exception") {
                var xHandler = (from item in aMethodInfo.Method.GetMethodBody().ExceptionHandlingClauses
                                where item.TryOffset == aMethodInfo.CurrentHandler.TryOffset && item.TryLength == aMethodInfo.CurrentHandler.TryLength && item.HandlerOffset == aMethodInfo.CurrentHandler.HandlerOffset && item.Flags == ExceptionHandlingClauseOptions.Clause
                                select item).FirstOrDefault();
                if (xHandler != null) {
                    mNextInstructionLabel = GetInstructionLabel(xHandler.HandlerOffset);
                } else {
                    // Here we need to detect where to leave to when this catch clause doesnt' handle a specific exception, and is the last one
                    throw new NotImplementedException("TODO: Implement exiting here!");
                }
            } else {
                mCatchType = null;
            }
            if (mCatchType != null && aMethodInfo != null && aMethodInfo.CurrentHandler != null && aMethodInfo.CurrentHandler.HandlerOffset > 0) {
                mNeedsTypeCheck = aMethodInfo.CurrentHandler.HandlerOffset == aReader.NextPosition;
            }
        }

        public static void Ldarg(Assembler.Assembler aAssembler,
                                 MethodInformation.Argument aArg) {
            Ldarg(aAssembler,
                  aArg,
                  true);
        }

        public static void Ldarg(Assembler.Assembler aAssembler,
                                 MethodInformation.Argument aArg,
                                 bool aAddGCCode) {
            foreach (string xAddress in aArg.VirtualAddresses.Reverse()) {
                new Move(CPUx86.Registers.EAX,
                         "[" + xAddress + "]");
                new Push(CPUx86.Registers.EAX);
            }
            aAssembler.StackContents.Push(new StackContent(aArg.Size,
                                                           aArg.ArgumentType));
            if (aAddGCCode && aArg.IsReferenceType) {
                new CPUx86.Push(CPUx86.Registers.EAX);
                Engine.QueueMethod(GCImplementationRefs.IncRefCountRef);
                new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.IncRefCountRef));
            }
        }

        public static void Ldflda(Assembler.Assembler aAssembler,
                                  TypeInformation aType,
                                  TypeInformation.Field aField) {
            Ldflda(aAssembler,
                   aType,
                   aField,
                   true);
        }

        public static void Ldflda(Assembler.Assembler aAssembler,
                                  TypeInformation aType,
                                  TypeInformation.Field aField,
                                  bool aDerefExternalAddress) {
            int aExtraOffset = 0;
            if (aType.NeedsGC) {
                aExtraOffset = 12;
            }
            new Popd(CPUx86.Registers.EAX);

            new CPUx86.Add(CPUx86.Registers.EAX,
                           "0x" + (aField.Offset + aExtraOffset).ToString("X"));
            aAssembler.StackContents.Pop();
            aAssembler.StackContents.Push(new StackContent(4,
                                                           aField.FieldType));
            if (aDerefExternalAddress && aField.IsExternalField) {
                new Pushd(CPUx86.Registers.AtEAX);
            } else {
                new Pushd(CPUx86.Registers.EAX);
            }
        }

        public static void Multiply(Assembler.Assembler aAssembler) {
            StackContent xStackContent = aAssembler.StackContents.Pop();
            new CPUx86.Xor("edx",
                           "edx");
            if (xStackContent.IsFloat) {
                throw new Exception("Float support not yet supported!");
            } else {
                if (xStackContent.Size > 4) {
                    new CPUx86.Pop("eax");
                    new CPUx86.Add("esp",
                                   "4");
                    new CPUx86.Multiply("dword [esp]");
                    new CPUx86.Add("esp",
                                   "8");
                    new Pushd("0");
                    new Pushd("eax");
                } else {
                    new CPUx86.Pop("eax");
                    new CPUx86.Multiply("dword [esp]");
                    new CPUx86.Add("esp",
                                   "4");
                    new Pushd("eax");
                }
            }
        }

        public static void Ldfld(Assembler.Assembler aAssembler,
                                 TypeInformation aType,
                                 string aFieldName) {
            Ldfld(aAssembler,
                  aType,
                  aType.Fields[aFieldName]);
        }

        public static void Ldfld(Assembler.Assembler aAssembler,
                                 TypeInformation aType,
                                 TypeInformation.Field aField) {
            Ldfld(aAssembler,
                  aType,
                  aField,
                  true);
        }

        public static void Ldfld(Assembler.Assembler aAssembler,
                                 TypeInformation aType,
                                 TypeInformation.Field aField,
                                 bool aAddGCCode) {
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
                                 bool aDerefExternalField) {
            aAssembler.StackContents.Pop();
            int aExtraOffset = 0;
            if (aType.NeedsGC) {
                aExtraOffset = 12;
            }
            new Comment("Type = '" + aType.TypeDef.FullName + "', NeedsGC = " + aType.NeedsGC);
            new CPUx86.Pop("ecx");
            new CPUx86.Add("ecx",
                           "0x" + (aField.Offset + aExtraOffset).ToString("X"));
            if (aField.IsExternalField && aDerefExternalField) {
                new CPUx86.Move("ecx",
                                "[ecx]");
            }
                for (int i = 1; i <= (aField.Size / 4); i++) {
                    new CPUx86.Move("eax",
                                    "[ecx + 0x" + (aField.Size - (i * 4)).ToString("X") + "]");
                    new CPUx86.Pushd("eax");
                }
                switch (aField.Size%4) {
                    case 1: {
                        new CPUx86.Move("eax",
                                        "0");
                        new CPUx86.Move("al",
                                        "[ecx]");
                        new CPUx86.Push("eax");
                        break;
                    }
                    case 2: {
                        new CPUx86.Move("eax",
                                        "0");
                        new CPUx86.Move("ax",
                                        "[ecx]");
                        new CPUx86.Push("eax");
                        break;
                    }
                    case 0: {
                        break;
                    }
                    default:
                        throw new Exception("Remainder size " + (aField.Size) + " not supported!");
                }
            if (aAddGCCode && aField.NeedsGC) {
                new CPUx86.Pushd(Registers.AtESP);
                Engine.QueueMethod(GCImplementationRefs.IncRefCountRef);
                new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.IncRefCountRef));
            }
            aAssembler.StackContents.Push(new StackContent(aField.Size,
                                                           aField.FieldType));
        }

        public static void Stfld(Assembler.Assembler aAssembler,
                                 TypeInformation aType,
                                 TypeInformation.Field aField) {
            aAssembler.StackContents.Pop();
            int xRoundedSize = aField.Size;
            if (xRoundedSize % 4 != 0) {
                xRoundedSize += 4 - (xRoundedSize % 4);
            }
            int aExtraOffset = 0;
            if (aType.NeedsGC) {
                aExtraOffset = 12;
            }
            if (aField.NeedsGC) {
                new CPUx86.Pushd("[esp + 4]");
                //Ldfld(aAssembler, aType, aField, false);
                new CPUx86.Pop("eax");
                new CPUx86.Pushd("[eax + " + (aField.Offset + aExtraOffset) + "]");
                Engine.QueueMethod(GCImplementationRefs.DecRefCountRef);
                new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.DecRefCountRef));
            }
            new CPUx86.Move("ecx",
                            "[esp + 0x" + xRoundedSize.ToString("X") + "]");
            new CPUx86.Add("ecx",
                           "0x" + (aField.Offset + aExtraOffset).ToString("X"));
            for (int i = 0; i < (aField.Size / 4); i++) {
                new CPUx86.Pop("eax");
                new Move("dword [ecx + 0x" + (i * 4).ToString("X") + "]",
                         "eax");
            }
            switch (aField.Size % 4) {
                case 1: {
                    new CPUx86.Pop("eax");
                    new Move("byte [ecx + " + ((aField.Size / 4) * 4) + "]",
                             "al");
                    break;
                }
                case 2: {
                    new CPUx86.Pop("eax");
                    new Move("word [ecx + " + ((aField.Size / 4) * 4) + "]",
                             "ax");
                    break;
                }
                case 0: {
                    break;
                }
                default:
                    throw new Exception("Remainder size " + (aField.Size % 4) + " not supported!");
            }
            if (aField.NeedsGC) {
                new CPUx86.Pushd(Registers.ECX);
                new CPUx86.Pushd(Registers.EAX);
                new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.DecRefCountRef));
                new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.DecRefCountRef));
            }
            new CPUx86.Add("esp",
                           "4");
            aAssembler.StackContents.Pop();
        }

        public static void AddWithOverflow(Assembler.Assembler aAssembler,
                                           bool signed) {
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

        public static void Add(Assembler.Assembler aAssembler) {
            StackContent xSize = aAssembler.StackContents.Pop();
            if (xSize.IsFloat) {

                if (xSize.Size > 4)
                {
                    throw new Exception("doubles not supported yet!");
                }
                else
                {
                    new CPUx86.SSE.MovSS("xmm0", "[esp]");
                    new CPUx86.Add("esp", "4");
                    new CPUx86.SSE.MovSS("xmm1", "[esp]");
                    new CPUx86.SSE.AddSS("xmm0", "xmm1");
                    new CPUx86.SSE.MovSS("[esp]", "xmm0");
                }
            }
            if (xSize.Size > 8) {
                throw new Exception("Size '" + xSize.Size + "' not supported");
            }
            if (xSize.Size > 4) {
                new CPUx86.Pop("eax");
                new CPUx86.Pop("edx");
                new CPUx86.Add("[esp]",
                               "eax");
                new CPUx86.AddWithCarry("[esp + 4]",
                                        "edx");
            } else {
                new CPUx86.Pop("eax");
                new CPUx86.Add("[esp]",
                               "eax");
            }
        }

        public static void Ldloc(Assembler.Assembler aAssembler,
                                 MethodInformation.Variable aLocal) {
            Ldloc(aAssembler,
                  aLocal,
                  true);
        }

        public static void Ldloc(Assembler.Assembler aAssembler,
                                 MethodInformation.Variable aLocal,
                                 bool aAddGCCode) {
            if (aLocal.VirtualAddresses.Length > 1) {
                foreach (string s in aLocal.VirtualAddresses) {
                    new CPUx86.Move("eax",
                                    "[" + s + "]");
                    new CPUx86.Push("eax");
                }
            } else {
                new CPUx86.Xor("eax",
                               "eax");

                switch (Engine.GetFieldStorageSize(aLocal.VariableType)) {
                    case 1: {
                        new CPUx86.Move("al",
                                        "[" + aLocal.VirtualAddresses.First() + "]");
                        break;
                    }
                    case 2: {
                        new CPUx86.Move("ax",
                                        "[" + aLocal.VirtualAddresses.First() + "]");
                        break;
                    }
                    case 4: {
                        new CPUx86.Move("eax",
                                        "[" + aLocal.VirtualAddresses.First() + "]");
                        break;
                    }
                }
                new CPUx86.Push("eax");
                if (aAddGCCode && aLocal.IsReferenceType) {
                    new CPUx86.Push("eax");
                    Engine.QueueMethod(GCImplementationRefs.IncRefCountRef);
                    new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.IncRefCountRef));
                }
            }
            aAssembler.StackContents.Push(new StackContent(aLocal.Size,
                                                           aLocal.VariableType));
        }

        protected override void AssembleHeader() {
            base.AssembleHeader();
            new Comment("Next Instruction = " + mNextInstructionLabel);
            string xCurExceptionFieldName = DataMember.GetStaticFieldName(IL2CPU.Assembler.Assembler.CurrentExceptionRef);
            if (mNeedsTypeCheck) {
                // call VTablesImpl.IsInstance to see the actual instance name..
                new CPUx86.Move("eax",
                                "[" + xCurExceptionFieldName + "]");
                new CPUx86.Pushd(Registers.AtEAX);
                new CPUx86.Pushd(Engine.RegisterType(mCatchType).ToString());
                new CPUx86.Call(Label.GenerateLabelName(VTablesImplRefs.IsInstanceRef));
                new CPUx86.Compare(Registers.EAX,
                                   "0");
                new CPUx86.JumpIfEqual(mNextInstructionLabel);
            }
            if (mNeedsExceptionPush) {
                new CPUx86.Push("dword [" + xCurExceptionFieldName + "]");
                Assembler.StackContents.Push(new StackContent(4,
                                                              typeof(Exception)));
            }
        }
    }
}