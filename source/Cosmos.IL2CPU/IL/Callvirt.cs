using System;
using System.Linq;
// using System.Collections.Generic;
// using System.Linq;
using CPU = Cosmos.Assembler.x86;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.Assembler;
using System.Reflection;

using Cosmos.IL2CPU.Plugs.System;
using XSharp.Compiler;
using SysReflection = System.Reflection;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Callvirt)]
    public class Callvirt : ILOp
    {
        public Callvirt(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xOpMethod = aOpCode as OpMethod;
            DoExecute(Assembler, aMethod, xOpMethod.Value, xOpMethod.ValueUID, aOpCode, DebugEnabled);
        }

        public static void DoExecute(Cosmos.Assembler.Assembler Assembler, MethodInfo aMethod, MethodBase aTargetMethod, uint aTargetMethodUID, ILOpCode aOp, bool debugEnabled)
        {
            string xCurrentMethodLabel = GetLabel(aMethod, aOp.Position);

            // mTargetMethodInfo = GetService<IMetaDataInfoService>().GetMethodInfo(mMethod
            //   , mMethod, mMethodDescription, null, mCurrentMethodInfo.DebugMode);
            string xNormalAddress = "";
            if (aTargetMethod.IsStatic
                || !aTargetMethod.IsVirtual
                || aTargetMethod.IsFinal)
            {
                xNormalAddress = LabelName.Get(aTargetMethod);
            }

            // mMethodIdentifier = GetService<IMetaDataInfoService>().GetMethodIdLabel(mMethod);

            int xArgCount = aTargetMethod.GetParameters().Length;
            uint xReturnSize = 0;
            var xMethodInfo = aTargetMethod as SysReflection.MethodInfo;
            if (xMethodInfo != null)
            {
                xReturnSize = Align(SizeOfType(xMethodInfo.ReturnType), 4);
            }

            // Extracted from MethodInformation: Calculated offset
            //             var xRoundedSize = ReturnSize;
            //if (xRoundedSize % 4 > 0) {
            //    xRoundedSize += (4 - (ReturnSize % 4));
            //}



            //ExtraStackSize = (int)xRoundedSize;
            uint xExtraStackSize = Call.GetStackSizeToReservate(aTargetMethod);
            uint xThisOffset = 0;
            var xParameters = aTargetMethod.GetParameters();
            foreach (var xItem in xParameters)
            {
                xThisOffset += Align(SizeOfType(xItem.ParameterType), 4);
            }

            // This is finding offset to self? It looks like we dont need offsets of other
            // arguments, but only self. If so can calculate without calculating all fields
            // Might have to go to old data structure for the offset...
            // Can we add this method info somehow to the data passed in?
            // mThisOffset = mTargetMethodInfo.Arguments[0].Offset;


            new Comment(Assembler, "ThisOffset = " + xThisOffset);
            Call.DoNullReferenceCheck(Assembler, debugEnabled, xThisOffset);

            if (!String.IsNullOrEmpty(xNormalAddress))
            {
                if (xExtraStackSize > 0)
                {
                    XS.Sub(XSRegisters.OldToNewRegister(CPU.RegistersEnum.ESP), (uint)xExtraStackSize);
                }
                new CPU.Call { DestinationLabel = xNormalAddress };
            }
            else
            {
                /*
             * On the stack now:
             * $esp                 Params
             * $esp + mThisOffset   This
             */
                Type xPopType = aOp.StackPopTypes.Last();
                if ((xPopType.IsPointer) || (xPopType.IsByRef))
                {
                    xPopType = xPopType.GetElementType();
                    string xTypeId = GetTypeIDLabel(xPopType);
                    new CPU.Push { DestinationRef = ElementReference.New(xTypeId), DestinationIsIndirect = true };
                }
                else
                {
                    XS.Set(XSRegisters.OldToNewRegister(CPU.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPU.RegistersEnum.ESP), sourceDisplacement: (int)xThisOffset);
                    new CPU.Mov { DestinationReg = CPU.RegistersEnum.EAX, SourceReg = CPU.RegistersEnum.EAX, SourceIsIndirect = true };
                    new CPU.Push { DestinationReg = CPU.RegistersEnum.EAX, DestinationIsIndirect = true };
                }
                new CPU.Push { DestinationValue = aTargetMethodUID };
                new CPU.Call { DestinationLabel = LabelName.Get(VTablesImplRefs.GetMethodAddressForTypeRef) };
                if (xExtraStackSize > 0)
                {
                    xThisOffset -= xExtraStackSize;
                }

                /*
             * On the stack now:
             * $esp                 Params
             * $esp + mThisOffset   This
             */

                //Call.EmitExceptionLogic( Assembler,
                //                        mCurrentILOffset,
                //                        mCurrentMethodInfo,
                //                        mLabelName + "_AfterAddressCheck",
                //                        true,
                //                        xEmitCleanup );
                XS.Pop(XSRegisters.OldToNewRegister(CPU.RegistersEnum.ECX));

                XS.Label(xCurrentMethodLabel + ".AfterAddressCheck");
                if (xMethodInfo.DeclaringType == typeof(object))
                {

                    /*
               * On the stack now:
               * $esp + 0                 Params
               * $esp + mThisOffset    This
               */
                    // we need to see if $this is a boxed object, and if so, we need to box it
                    XS.Set(XSRegisters.OldToNewRegister(CPU.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPU.RegistersEnum.ESP), sourceDisplacement: (int)xThisOffset);

                    //new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceValue = ( ( uint )InstanceTypeEnum.BoxedValueType ), Size = 32 };

                    // EAX contains the handle now, lets dereference it
                    new CPU.Mov { DestinationReg = CPU.RegistersEnum.EAX, SourceReg = CPU.RegistersEnum.EAX, SourceIsIndirect = true };

                    new CPU.Compare { DestinationReg = CPU.RegistersEnum.EAX, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceValue = (int)InstanceTypeEnum.BoxedValueType, Size = 32 };

                    /*
               * On the stack now:
               * $esp                 Params
               * $esp + mThisOffset   This
               *
               * ECX contains the method to call
               * EAX contains the type pointer (not the handle!!)
               */
                    new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.NotEqual, DestinationLabel = xCurrentMethodLabel + ".NotBoxedThis" };

                    /*
               * On the stack now:
               * $esp                 Params
               * $esp + mThisOffset   This
               *
               * ECX contains the method to call
               * EAX contains the type pointer (not the handle!!)
               */

                    XS.Add(XSRegisters.OldToNewRegister(CPU.RegistersEnum.EAX), (uint)ObjectImpl.FieldDataOffset);
                    new CPU.Mov { DestinationReg = CPU.RegistersEnum.ESP, DestinationIsIndirect = true, DestinationDisplacement = (int)xThisOffset, SourceReg = CPU.RegistersEnum.EAX };
                    /*
               * On the stack now:
               * $esp                 Params
               * $esp + mThisOffset   Pointer to address inside box
               *
               * ECX contains the method to call
               */
                }
                XS.Label(xCurrentMethodLabel + ".NotBoxedThis");
                if (xExtraStackSize > 0)
                {
                    XS.Sub(XSRegisters.OldToNewRegister(CPU.RegistersEnum.ESP), xExtraStackSize);
                }
                XS.Call(XSRegisters.ECX);
                XS.Label(xCurrentMethodLabel + ".AfterNotBoxedThis");
            }
            ILOp.EmitExceptionLogic(Assembler, aMethod, aOp, true,
                                    delegate ()
                                    {
                                        var xStackOffsetBefore = aOp.StackOffsetBeforeExecution.Value;

                                        uint xPopSize = 0;
                                        foreach (var type in aOp.StackPopTypes)
                                        {
                                            xPopSize += Align(SizeOfType(type), 4);
                                        }

                                        var xResultSize = xReturnSize;
                                        if (xResultSize % 4 != 0)
                                        {
                                            xResultSize += 4 - (xResultSize % 4);
                                        }

                                        ILOp.EmitExceptionCleanupAfterCall(Assembler, xResultSize, xStackOffsetBefore, xPopSize);
                                    });
            XS.Label(xCurrentMethodLabel + ".NoExceptionAfterCall");
            new Comment(Assembler, "Argument Count = " + xParameters.Length.ToString());
        }
    }
}
