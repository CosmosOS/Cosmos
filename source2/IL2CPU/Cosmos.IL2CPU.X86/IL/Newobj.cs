using System;
using System.Linq;
using Cosmos.Compiler.Assembler;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.IL2CPU.IL.CustomImplementations.System;
using System.Reflection;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Newobj )]
    public class Newobj : ILOp
    {
        public Newobj( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            OpMethod xMethod = ( OpMethod )aOpCode;
            string xCurrentLabel = GetLabel( aMethod, aOpCode );
            var xType = xMethod.Value.DeclaringType;

            Assemble(Assembler, aMethod, xMethod, xCurrentLabel, xType, xMethod.Value);
        }

        public static void Assemble(Assembler aAssembler, MethodInfo aMethod, OpMethod xMethod, string currentLabel, Type objectType, MethodBase constructor)
        {
            // call cctor:
            if (aMethod != null)
            {
                var xCctor = (objectType.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic) ?? new ConstructorInfo[0]).SingleOrDefault();
                if (xCctor != null)
                {
                    new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(xCctor) };
                    ILOp.EmitExceptionLogic(aAssembler, aMethod, xMethod, true, null, ".AfterCCTorExceptionCheck");
                    new Label(".AfterCCTorExceptionCheck");
                }
            }

            // If not ValueType, then we need gc
            if (!objectType.IsValueType)
            {
                var xParams = constructor.GetParameters();
                for (int i = 0; i < xParams.Length; i++)
                {
                    aAssembler.Stack.Pop();
                }

                uint xMemSize = GetStorageSize(objectType);
                int xExtraSize = 20;
                new CPUx86.Push { DestinationValue = (uint)(xMemSize + xExtraSize) };

                // todo: probably we want to check for exceptions after calling Alloc
                new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.AllocNewObjectRef) };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };

                //??? uint xObjSize;// = 0;
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
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EBX, SourceRef = ElementReference.New(strTypeId), SourceIsIndirect = true };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EBX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceValue = (uint)InstanceTypeEnum.NormalObject, Size = 32 };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 8, SourceValue = (uint)xGCFieldCount, Size = 32 };
                uint xSize = (uint)(((from item in xParams
                                      let xQSize = Align(SizeOfType(item.GetType()), 4)
                                      select (int)xQSize).Take(xParams.Length).Sum()));

                foreach (var xParam in xParams)
                {
                    uint xParamSize = SizeOfType(xParams.GetType());
                    new Comment(aAssembler, String.Format("Arg {0}: {1}", xParam.Name, xParamSize));
                    for (int i = 0; i < xParamSize; i += 4)
                    {
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = (int)(xSize + 4) };
                    }
                }

                new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(constructor) };
                if (aMethod != null)
                {
                    new CPUx86.Test { DestinationReg = CPUx86.Registers.ECX, SourceValue = 2 };
                    string xNoErrorLabel = currentLabel + "_NO_ERROR_" + MethodInfoLabelGenerator.LabelCount.ToString();
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
                    new Comment(aAssembler, "[ Newobj.Execute cleanup start count = " + aAssembler.Stack.Count.ToString() + " ]");
                    //foreach( var xStackInt in Assembler.Stack )
                    //{
                    //    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = ( uint )xStackInt.Size };
                    //}

                    uint xESPOffset = 0;
                    foreach (var xParam in xParams)
                    {
                        xESPOffset += SizeOfType(xParams.GetType());
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
                aAssembler.Stack.Push(4, constructor.DeclaringType);
            }
            else
            {
                //throw new Exception("Not implemented yet: .ctor on value type");
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
                var xStorageSize = Align(SizeOfType(objectType), 4);
                //var xStorageSize = aCtorDeclTypeInfo.StorageSize;
                //uint xArgSize = 0;
                var xParams = constructor.GetParameters();
                uint xArgSize = (uint)(((from item in xParams.Skip(1)
                                         let xQSize = Align(SizeOfType(item.GetType()), 4)
                                         select (int)xQSize).Take(xParams.Length - 1).Sum()));

                //foreach( var xArg in aCtorMethodInfo.Arguments.Skip( 1 ) )
                //{
                //    xArgSize += xArg.Size + ( xArg.Size % 4 == 0
                //                                            ? 0
                //                                            : ( 4 - ( xArg.Size % 4 ) ) );
                //}
                int xExtraArgSize = (int)(xStorageSize - xArgSize);
                if (xExtraArgSize < 0)
                {
                    xExtraArgSize = 0;
                }
                if (xExtraArgSize > 0)
                {
                    new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = (uint)xExtraArgSize };
                }
                new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP };
                aAssembler.Stack.Push(new StackContents.Item(4, null));
                //at this point, we need to move copy all arguments over. 
                for (int i = 0; i < (xArgSize / 4); i++)
                {
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = (int)(xStorageSize + 4) }; // + 4 because the ptr is pushed too
                    new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = (int)(xStorageSize + 4 + 4), SourceValue = 0, Size = 32 };
                }
                new Call(aAssembler).Execute(aMethod, xMethod);
                //var xCall = new Call( aCtorDef,
                //                     ( uint )aCurrentILOffset,
                //                     true,
                //                     aNextLabel );
                //xCall.SetServiceProvider( aServiceProvider );
                //xCall.Assembler = aAssembler;
                //xCall.Assemble();
                aAssembler.Stack.Push(new StackContents.Item(xStorageSize, objectType));
            }
        }

        private static void PushAlignedParameterSize( System.Reflection.MethodBase aMethod )
        {
            System.Reflection.ParameterInfo[] xParams = aMethod.GetParameters();

            uint xSize;
            new Comment("[ Newobj.PushAlignedParameterSize start count = " + xParams.Length.ToString() + " ]" );
            for( int i = 0; i < xParams.Length; i++ )
            {
                xSize = SizeOfType( xParams[ i ].GetType() );
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = Align( xSize, 4 ) };
            }
            new Comment("[ Newobj.PushAlignedParameterSize end ]" );
        }
    }
}