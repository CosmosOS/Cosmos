using System;
using System.Collections.Generic;
using System.Linq;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using System.Reflection;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
    [OpCode(OpCodeEnum.Callvirt)]
    public class Callvirt : Op {
        private readonly int mMethodIdentifier;
        private readonly string mNormalAddress;
        private readonly string mMethodDescription;
        private readonly int mThisOffset;
        private readonly uint mArgumentCount;
        private readonly uint mReturnSize;
        private readonly string mLabelName;
        private readonly MethodInformation mCurrentMethodInfo;
        private readonly MethodInformation mTargetMethodInfo;
        private readonly uint mCurrentILOffset;
        private readonly int mExtraStackSpace;

        public static void ScanOp(ILReader aReader,
                                  MethodInformation aMethodInfo,
                                  SortedList<string, object> aMethodData) {
            MethodBase xMethod = aReader.OperandValueMethod;
            if (xMethod == null) {
                throw new Exception("Unable to determine Method!");
            }
            MethodBase xMethodDef = xMethod;
            var xMethodDescription = CPU.Label.GenerateLabelName(xMethodDef);
            var xTargetMethodInfo = Engine.GetMethodInfo(xMethodDef,
                                                         xMethodDef,
                                                         xMethodDescription,
                                                         null,
                                                         aMethodInfo.DebugMode);
            foreach (var xParam in xMethodDef.GetParameters()) {
                Engine.RegisterType(xParam.ParameterType);
            }
            Engine.RegisterType(xTargetMethodInfo.ReturnType);
            Engine.QueueMethod(xMethodDef);
            Engine.QueueMethod(VTablesImplRefs.GetMethodAddressForTypeRef);
            Engine.QueueMethod(CPU.Assembler.CurrentExceptionOccurredRef);
            Engine.RegisterType(typeof(NullReferenceException));
            Engine.QueueMethod(typeof(NullReferenceException).GetConstructor(new Type[0]));
        }

        public Callvirt(ILReader aReader,
                        MethodInformation aMethodInfo)
            : base(aReader,
                   aMethodInfo) {
            mLabelName = GetInstructionLabel(aReader);
            mCurrentMethodInfo = aMethodInfo;
            MethodBase xMethod = aReader.OperandValueMethod;
            if (xMethod == null) {
                throw new Exception("Unable to determine Method!");
            }
            MethodBase xMethodDef = xMethod;
            mMethodDescription = CPU.Label.GenerateLabelName(xMethodDef);
            mTargetMethodInfo = Engine.GetMethodInfo(xMethodDef,
                                                     xMethodDef,
                                                     mMethodDescription,
                                                     null,
                                                     aMethodInfo.DebugMode);
            if (xMethodDef.IsStatic || !xMethodDef.IsVirtual || xMethod.IsFinal) {
                Engine.QueueMethod(xMethodDef);
                mNormalAddress = CPU.Label.GenerateLabelName(xMethodDef);
            }
            mMethodIdentifier = Engine.GetMethodIdentifier(xMethodDef);
            Engine.QueueMethod(VTablesImplRefs.GetMethodAddressForTypeRef);
            mArgumentCount = (uint)mTargetMethodInfo.Arguments.Length;
            mReturnSize = mTargetMethodInfo.ReturnSize;
            mThisOffset = mTargetMethodInfo.Arguments[0].Offset;
            if (mTargetMethodInfo.ExtraStackSize > 0) {
                mThisOffset -= mTargetMethodInfo.ExtraStackSize;
            }
            mCurrentILOffset = aReader.Position;
            mExtraStackSpace = mTargetMethodInfo.ExtraStackSize;
        }

        public override void DoAssemble() {
            new Comment("ThisOffset = " + mThisOffset);
            Action xEmitCleanup = delegate() {
                                      foreach (MethodInformation.Argument xArg in mTargetMethodInfo.Arguments) {
                                          new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = xArg.Size };
                                      }
                                  };
            if (!String.IsNullOrEmpty(mNormalAddress)) {
                EmitCompareWithNull(Assembler,
                                    mCurrentMethodInfo,
                                    delegate(CPUx86.Compare c){
                                        c.DestinationReg = CPUx86.Registers.ESP;
                                        c.DestinationIsIndirect = true;
                                        c.DestinationDisplacement = mThisOffset;
                                    },
                                    mLabelName,
                                    mLabelName + "_AfterNullRefCheck",
                                    xEmitCleanup,
                                    (int)mCurrentILOffset);
                new CPU.Label(mLabelName + "_AfterNullRefCheck");
                if (mExtraStackSpace > 0) {
                    new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = (uint)mExtraStackSpace };
                }
                new CPUx86.Call { DestinationLabel = mNormalAddress };
            } else {
                /*
                 * On the stack now:
                 * $esp                 Params
                 * $esp + mThisOffset   This
                 */
                //Assembler.Add(new CPUx86.Pop("eax"));
                //Assembler.Add(new CPUx86.Pushd("eax"));
                EmitCompareWithNull(Assembler,
                                    mCurrentMethodInfo,
                                    delegate(CPUx86.Compare c) {
                                        c.DestinationReg = CPUx86.Registers.ESP;
                                        c.DestinationIsIndirect = true;
                                        c.DestinationDisplacement = mThisOffset;
                                    }, 
                                    mLabelName,
                                    mLabelName + "_AfterNullRefCheck",
                                    xEmitCleanup,
                                    (int)mCurrentILOffset);
                new CPU.Label(mLabelName + "_AfterNullRefCheck");
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = mThisOffset };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
                new CPUx86.Push { DestinationValue = (uint)mMethodIdentifier };
                new CPUx86.Call { DestinationLabel = CPU.Label.GenerateLabelName(VTablesImplRefs.GetMethodAddressForTypeRef) };

                /*
                 * On the stack now:
                 * $esp                 Params
                 * $esp + mThisOffset   This
                 * 
                 * EAX contains the method to call
                 */
                Call.EmitExceptionLogic(Assembler,
                                        mCurrentILOffset,
                                        mCurrentMethodInfo,
                                        mLabelName + "_AfterAddressCheck",
                                        true,
                                        xEmitCleanup);
                new CPU.Label(mLabelName + "_AfterAddressCheck");
                if (mTargetMethodInfo.Arguments[0].ArgumentType == typeof(object)) {
                    /*
                     * On the stack now:
                     * $esp                     method to call
                     * $esp + 4                 Params
                     * $esp + mThisOffset + 4   This
                     */
                    new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = mThisOffset + 4 };
                    new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceValue = ((uint)InstanceTypeEnum.BoxedValueType), Size = 32 };
                    /*
                     * On the stack now:
                     * $esp                 Params
                     * $esp + mThisOffset   This
                     * 
                     * EAX contains the method to call
                     */
                    new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotEqual, DestinationLabel = mLabelName + "_NOT_BOXED_THIS" };
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                    /*
                     * On the stack now:
                     * $esp                 Params
                     * $esp + mThisOffset   This
                     * 
                     * ECX contains the method to call
                     */
                    new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = mThisOffset };
                    /*
                     * On the stack now:
                     * $esp                 Params
                     * $esp + mThisOffset   This
                     * 
                     * ECX contains the method to call
                     * EAX contains $This, but boxed
                     */
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = (uint)ObjectImpl.FieldDataOffset };
                    new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = mThisOffset, SourceReg = CPUx86.Registers.EAX };
                    /*
                     * On the stack now:
                     * $esp                 Params
                     * $esp + mThisOffset   Pointer to address inside box
                     * 
                     * ECX contains the method to call
                     */
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.ECX };
                    /*
                     * On the stack now:
                     * $esp                    Method to call
                     * $esp + 4                Params
                     * $esp + mThisOffset + 4  This
                     */
                }
                new CPU.Label(mLabelName + "_NOT_BOXED_THIS");
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                if (mExtraStackSpace > 0) {
                    new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = (uint)mExtraStackSpace };
                }
                new CPUx86.Call { DestinationReg = CPUx86.Registers.EAX };
                new CPU.Label(mLabelName + "__AFTER_NOT_BOXED_THIS");
            }
            Call.EmitExceptionLogic(Assembler,
                               mCurrentILOffset,
                               mCurrentMethodInfo,
                               mLabelName + "__NO_EXCEPTION_AFTER_CALL",
                               true,
                               delegate()
                               {
                                   var xResultSize = mTargetMethodInfo.ReturnSize;
                                   if (xResultSize % 4 != 0)
                                   {
                                       xResultSize += 4 - (xResultSize % 4);
                                   }
                                   for (int i = 0; i < xResultSize / 4; i++)
                                   {
                                       new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                                   }
                               });

            new CPU.Label(mLabelName + "__NO_EXCEPTION_AFTER_CALL");
            new CPU.Comment("Argument Count = " + mArgumentCount.ToString());
            for (int i = 0; i < mArgumentCount; i++) {
                Assembler.StackContents.Pop();
            }
            if (mReturnSize > 0) {
                Assembler.StackContents.Push(new StackContent((int)mReturnSize));
            }
        }
    }
}