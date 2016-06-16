using System;
using System.Linq;
using Cosmos.Assembler;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.ILOpCodes;
using System.Reflection;

using Cosmos.IL2CPU.Plugs.System;
using XSharp.Compiler;
using SysReflection = System.Reflection;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Newobj)]
    public class Newobj : ILOp
    {
        public Newobj(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            OpMethod xMethod = (OpMethod)aOpCode;
            string xCurrentLabel = GetLabel(aMethod, aOpCode);
            var xType = xMethod.Value.DeclaringType;

            Assemble(Assembler, aMethod, xMethod, xCurrentLabel, xType, xMethod.Value);
        }

        public static void Assemble(Cosmos.Assembler.Assembler aAssembler, MethodInfo aMethod, OpMethod xMethod, string currentLabel, Type objectType, MethodBase constructor)
        {
            // call cctor:
            if (aMethod != null)
            {
                var xCctor = (objectType.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic) ?? new ConstructorInfo[0]).SingleOrDefault();
                if (xCctor != null)
                {
                    XS.Call(LabelName.Get(xCctor));
                    ILOp.EmitExceptionLogic(aAssembler, aMethod, xMethod, true, null, ".AfterCCTorExceptionCheck");
                    XS.Label(".AfterCCTorExceptionCheck");
                }
            }

            if (objectType.IsValueType)
            {
                #region Valuetypes

                XS.Comment("ValueType");
                /*
                 * Current sitation on stack:
                 *   $ESP       Arg
                 *   $ESP+..    other items
                 *
                 * What should happen:
                 *  + The stack should be increased to allow space to contain:
                 *         + .ctor arguments
                 *         + struct _pointer_ (ref to start of emptied space)
                 *         + empty space for struct
                 *  + arguments should be copied to the new place
                 *  + old place where arguments were should be cleared
                 *  + pointer should be set
                 *  + call .ctor
                 */

                // Size of return value - we need to make room for this on the stack.
                uint xStorageSize = Align(SizeOfType(objectType), 4);
                XS.Comment("StorageSize: " + xStorageSize);
                if (xStorageSize == 0)
                {
                    throw new Exception("ValueType storage size cannot be 0.");
                }

                //var xStorageSize = aCtorDeclTypeInfo.StorageSize;

                uint xArgSize = 0;
                var xParameterList = constructor.GetParameters();
                foreach (var xParam in xParameterList)
                {
                    xArgSize = xArgSize + Align(SizeOfType(xParam.ParameterType), 4);
                }
                XS.Comment("ArgSize: " + xArgSize);

                // Set ESP so we can push the struct ptr
                int xShift = (int)(xArgSize - xStorageSize);
                XS.Comment("Shift: " + xShift);
                if (xShift < 0)
                {
                    XS.Sub(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), (uint)Math.Abs(xShift));
                }
                else if (xShift > 0)
                {
                    XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), (uint)xShift);
                }

                // push struct ptr
                XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP));

                // Shift args
                foreach (var xParam in xParameterList)
                {
                    uint xArgSizeForThis = Align(SizeOfType(xParam.ParameterType), 4);
                    for (int i = 1; i <= xArgSizeForThis / 4; i++)
                    {
                        new CPUx86.Push { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, DestinationDisplacement = (int)xStorageSize };
                    }
                }

                new Call(aAssembler).Execute(aMethod, xMethod);

                // Need to put these *after* the call because the Call pops the args from the stack
                // and we have mucked about on the stack, so this makes it right before the next
                // op.

                #endregion Valuetypes
            }
            else
            {
                // If not ValueType, then we need gc

                var xParams = constructor.GetParameters();

                // array length + 8
                bool xHasCalcSize = false;

                #region Special string handling
                // try calculating size:
                if (constructor.DeclaringType == typeof(string))
                {
                    if (xParams.Length == 1
                        && xParams[0].ParameterType == typeof(char[]))
                    {
                        xHasCalcSize = true;
                        XS.Set(XSRegisters.EAX, XSRegisters.ESP, sourceIsIndirect: true);

                        // EAX contains a memory handle now, lets dereference it to a pointer
                        XS.Set(XSRegisters.EAX, XSRegisters.EAX, sourceIsIndirect: true);
                        XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), sourceDisplacement: 8);
                        XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX), 2);
                        XS.Multiply(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));
                        XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                    }
                    else if (xParams.Length == 3
                             && (xParams[0].ParameterType == typeof(char[]) || xParams[0].ParameterType == typeof(char*))
                             && xParams[1].ParameterType == typeof(int)
                             && xParams[2].ParameterType == typeof(int))
                    {
                        xHasCalcSize = true;
                        XS.Set(XSRegisters.EAX, XSRegisters.ESP, sourceIsIndirect: true);
                        XS.ShiftLeft(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 1);
                        XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                    }
                    else if (xParams.Length == 2
                             && xParams[0].ParameterType == typeof(char)
                             && xParams[1].ParameterType == typeof(int))
                    {
                        xHasCalcSize = true;
                        XS.Set(XSRegisters.EAX, XSRegisters.ESP, sourceIsIndirect: true);
                        XS.ShiftLeft(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 1);
                        XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                    }
                    else
                    {
                        throw new NotImplementedException("In NewObj, a string ctor implementation is missing!");
                    }
                }
                #endregion Special string handling

                uint xMemSize = GetStorageSize(objectType);
                int xExtraSize = 12; // additional size for set values after alloc
                new CPUx86.Push { DestinationValue = (uint)(xMemSize + xExtraSize) };
                if (xHasCalcSize)
                {
                    XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                    XS.Add(XSRegisters.ESP, XSRegisters.EAX, destinationIsIndirect: true);
                }

                // todo: probably we want to check for exceptions after calling Alloc
                XS.Call(LabelName.Get(GCImplementationRefs.AllocNewObjectRef));
                XS.Label(".AfterAlloc");
                new CPUx86.Push { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true };
                new CPUx86.Push { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true };

                // it's on the stack now 3 times. Once from the Alloc return value, twice from the pushes

                //? ?? uint xObjSize;// = 0;
                //int xGCFieldCount = ( from item in aCtorDeclTypeInfo.Fields.Values
                //where item.NeedsGC
                //select item ).Count();

                //int xGCFieldCount = ( from item in aCtorDeclTypeInfo.Fields.Values
                //where item.NeedsGC
                //select item ).Count();
                int xGCFieldCount = objectType.GetFields().Count(x => x.FieldType.IsValueType);

                // todo: use a cleaner approach here. this class shouldnt assemble the string
                string strTypeId = GetTypeIDLabel(constructor.DeclaringType);

                XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                XS.Set(XSRegisters.EAX, XSRegisters.EAX, sourceIsIndirect: true);
                XS.Set(XSRegisters.EBX, strTypeId, sourceIsIndirect: true);
                XS.Set(XSRegisters.EAX, XSRegisters.EBX, destinationIsIndirect: true);
                new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EAX, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceValue = (uint)InstanceTypeEnum.NormalObject, Size = 32 };
                new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EAX, DestinationIsIndirect = true, DestinationDisplacement = 8, SourceValue = (uint)xGCFieldCount, Size = 32 };
                uint xSize = (uint)(from item in xParams
                                    let xQSize = Align(SizeOfType(item.ParameterType), 4)
                                    select (int)xQSize).Take(xParams.Length).Sum();

                foreach (var xParam in xParams)
                {
                    uint xParamSize = Align(SizeOfType(xParam.ParameterType), 4);
                    new Comment(aAssembler, String.Format("Arg {0}: {1}", xParam.Name, xParamSize));
                    for (int i = 0; i < xParamSize; i += 4)
                    {
                        new CPUx86.Push { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, DestinationDisplacement = (int)(xSize + 4) };
                    }
                }

                XS.Call(LabelName.Get(constructor));
                // should the complete error handling happen by ILOp.EmitExceptionLogic?
                if (aMethod != null)
                {
                    // todo: only happening for real methods now, not for ctor's ?
                    XS.Test(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX), 2);
                    string xNoErrorLabel = currentLabel + ".NoError" + LabelName.LabelCount.ToString();
                    XS.Jump(CPUx86.ConditionalTestEnum.Equal, xNoErrorLabel);

                    //for( int i = 1; i < aCtorMethodInfo.Arguments.Length; i++ )
                    //{
                    //    new CPUx86.Add
                    //    {
                    //        DestinationReg = CPUx86.Registers.ESP,
                    //        SourceValue = ( aCtorMethodInfo.Arguments[ i ].Size % 4 == 0
                    //             ? aCtorMethodInfo.Arguments[ i ].Size
                    //             : ( ( aCtorMethodInfo.Arguments[ i ].Size / 4 ) * 4 ) + 1 )
                    //    };
                    //}
                    PushAlignedParameterSize(constructor);

                    // an exception occurred, we need to cleanup the stack, and jump to the exit
                    XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 4);

                    //new Comment(aAssembler, "[ Newobj.Execute cleanup start count = " + aAssembler.Stack.Count.ToString() + " ]");
                    //foreach( var xStackInt in Assembler.Stack )
                    //{
                    //    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = ( uint )xStackInt.Size };
                    //}

                    new Comment(aAssembler, "[ Newobj.Execute cleanup end ]");
                    Jump_Exception(aMethod);
                    XS.Label(xNoErrorLabel);
                }
                XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));

                //for( int i = 1; i < aCtorMethodInfo.Arguments.Length; i++ )
                //{
                //    new CPUx86.Add
                //    {
                //        DestinationReg = CPUx86.Registers.ESP,
                //        SourceValue = ( aCtorMethodInfo.Arguments[ i ].Size % 4 == 0
                //             ? aCtorMethodInfo.Arguments[ i ].Size
                //             : ( ( aCtorMethodInfo.Arguments[ i ].Size / 4 ) * 4 ) + 1 )
                //    };
                //}
                PushAlignedParameterSize(constructor);

                XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            }
        }

        private static void PushAlignedParameterSize(MethodBase aMethod)
        {
            ParameterInfo[] xParams = aMethod.GetParameters();

            uint xSize;
            XS.Comment("[ Newobj.PushAlignedParameterSize start count = " + xParams.Length.ToString() + " ]");
            for (int i = 0; i < xParams.Length; i++)
            {
                xSize = SizeOfType(xParams[i].ParameterType);
                XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), Align(xSize, 4));
            }
            XS.Comment("[ Newobj.PushAlignedParameterSize end ]");
        }
    }
}
