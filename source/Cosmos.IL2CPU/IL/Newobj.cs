using System;
using System.Linq;
using Cosmos.Assembler;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.IL2CPU.IL.CustomImplementations.System;
using System.Reflection;
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

        public static void Assemble(Cosmos.Assembler.Assembler aAssembler,  MethodInfo aMethod, OpMethod xMethod, string currentLabel, Type objectType, MethodBase constructor)
        {
            // call cctor:
            if (aMethod != null)
            {
                var xCctor = (objectType.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic) ?? new ConstructorInfo[0]).SingleOrDefault();
                if (xCctor != null)
                {
                    new CPUx86.Call { DestinationLabel = LabelName.Get(xCctor) };
                    ILOp.EmitExceptionLogic(aAssembler, aMethod, xMethod, true, null, ".AfterCCTorExceptionCheck");
                    new Label(".AfterCCTorExceptionCheck");
                }
            }

            if (objectType.IsValueType)
            {
                new Comment("ValueType");
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
                new Comment("StorageSize: " + xStorageSize);
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
                new Comment("ArgSize: " + xArgSize);

                // Set ESP so we can push the struct ptr
                int xShift = (int)(xArgSize - xStorageSize);
                new Comment("Shift: " + xShift);
                if (xShift < 0)
                {
                    new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = (uint)Math.Abs(xShift) };
                }
                else if (xShift > 0)
                {
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = (uint)xShift };
                }
                // push struct ptr
                new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP };
                // Shift args
                foreach (var xParam in xParameterList)
                {
                    uint xArgSizeForThis = Align(SizeOfType(xParam.ParameterType), 4);
                    for (int i = 1; i <= xArgSizeForThis / 4; i++)
                    {
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = (int)xStorageSize };
                    }
                }

                new Call(aAssembler).Execute(aMethod, xMethod);
                // Need to put these *after* the call because the Call pops the args from the stack
                // and we have mucked about on the stack, so this makes it right before the next
                // op.
            }
            else
            {
                // If not ValueType, then we need gc

                var xParams = constructor.GetParameters();

                // array length + 8
                bool xHasCalcSize = false;
                // try calculating size:
                if (constructor.DeclaringType == typeof(string))
                {
                    if (xParams.Length == 1 && xParams[0].ParameterType == typeof(char[]))
                    {
                        xHasCalcSize = true;
                        new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                        new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true, SourceDisplacement = 8 };
                        new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDX, SourceValue = 2 };
                        new CPUx86.Multiply { DestinationReg = CPUx86.Registers.EDX };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                    }
                    else if (xParams.Length == 3 && xParams[0].ParameterType == typeof(char[]) && xParams[1].ParameterType == typeof(int) && xParams[2].ParameterType == typeof(int))
                    {
                        xHasCalcSize = true;
                        new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                        new CPUx86.ShiftLeft { DestinationReg = CPUx86.Registers.EAX, SourceValue = 1 };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                    }
                    else if (xParams.Length == 2 && xParams[0].ParameterType == typeof(char) && xParams[1].ParameterType == typeof(int))
                    {
                        xHasCalcSize = true;
                        new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                        new CPUx86.ShiftLeft { DestinationReg = CPUx86.Registers.EAX, SourceValue = 1 };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                    }
                    else
                        throw new NotImplementedException("In NewObj is a string ctor implementation missing!");
                }
                uint xMemSize = GetStorageSize(objectType);
                int xExtraSize = 12; // additional size for set values after alloc
                new CPUx86.Push { DestinationValue = (uint)(xMemSize + xExtraSize) };
                if (xHasCalcSize)
                {
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
                }

                // todo: probably we want to check for exceptions after calling Alloc
                new CPUx86.Call { DestinationLabel = LabelName.Get(GCImplementationRefs.AllocNewObjectRef) };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };

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

                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EBX, SourceRef = Cosmos.Assembler.ElementReference.New(strTypeId), SourceIsIndirect = true };
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EBX };
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceValue = (uint)InstanceTypeEnum.NormalObject, Size = 32 };
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 8, SourceValue = (uint)xGCFieldCount, Size = 32 };
                uint xSize = (uint)(((from item in xParams
                                      let xQSize = Align(SizeOfType(item.ParameterType), 4)
                                      select (int)xQSize).Take(xParams.Length).Sum()));

                foreach (var xParam in xParams)
                {
                    uint xParamSize = Align(SizeOfType(xParam.ParameterType), 4);
                    new Comment(aAssembler, String.Format("Arg {0}: {1}", xParam.Name, xParamSize));
                    for (int i = 0; i < xParamSize; i += 4)
                    {
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = (int)(xSize + 4) };
                    }
                }

                new CPUx86.Call { DestinationLabel = LabelName.Get(constructor) };
                if (aMethod != null)
                {
                    new CPUx86.Test { DestinationReg = CPUx86.Registers.ECX, SourceValue = 2 };
                    string xNoErrorLabel = currentLabel + ".NoError" + LabelName.LabelCount.ToString();
                    new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = xNoErrorLabel };

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
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                    //new Comment(aAssembler, "[ Newobj.Execute cleanup start count = " + aAssembler.Stack.Count.ToString() + " ]");
                    //foreach( var xStackInt in Assembler.Stack )
                    //{
                    //    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = ( uint )xStackInt.Size };
                    //}

                    uint xESPOffset = 0;
                    foreach (var xParam in xParams)
                    {
                        xESPOffset += Align(SizeOfType(xParam.ParameterType), 4);
                    }
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = xESPOffset };

                    new Comment(aAssembler, "[ Newobj.Execute cleanup end ]");
                    Jump_Exception(aMethod);
                    new Label(xNoErrorLabel);
                }
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };

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

                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            }
        }

        private static void PushAlignedParameterSize(SysReflection.MethodBase aMethod)
        {
            SysReflection.ParameterInfo[] xParams = aMethod.GetParameters();

            uint xSize;
            new Comment("[ Newobj.PushAlignedParameterSize start count = " + xParams.Length.ToString() + " ]");
            for (int i = 0; i < xParams.Length; i++)
            {
                xSize = SizeOfType(xParams[i].ParameterType);
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = Align(xSize, 4) };
            }
            new Comment("[ Newobj.PushAlignedParameterSize end ]");
        }
    }
}
