using System;
using System.Linq;
using Indy.IL2CPU;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Cosmos.IL2CPU.ILOpCodes;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Newobj )]
    public class Newobj : ILOp
    {
        public Newobj( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            OpMethod xMethod = ( OpMethod )aOpCode;
            // Is this checking for plugs?
            // if (DynamicMethodEmit.GetHasDynamicMethod(CtorDef)) {
            //   CtorDef = DynamicMethodEmit.GetDynamicMethod(CtorDef);
            // }

            //var xAllocInfo = GetService<IMetaDataInfoService>().GetMethodInfo(GCImplementationRefs.AllocNewObjectRef, false);

            //             Assemble(
            //                 Assembler,
            //                 CtorDef,
            //                 GetService<IMetaDataInfoService>().GetTypeIdLabel(CtorDef.DeclaringType),
            //                 CurrentLabel,
            //                 MethodInformation,
            //                 (int)ILOffset,
            //                 mNextLabel,
            //                 GetService<IMetaDataInfoService>().GetTypeInfo(CtorDef.DeclaringType),
            //                 GetService<IMetaDataInfoService>().GetMethodInfo(CtorDef, false),
            //                 GetServiceProvider(),
            //                 xAllocInfo.LabelName
            //             );
            // }
            // 
            //         public static void Assemble(
            //             Assembler.Assembler aAssembler,
            //             MethodBase aCtorDef,
            //             string aTypeId,
            //             string aCurrentLabel,
            //             MethodInformation aCurrentMethodInformation,
            //             int aCurrentILOffset,
            //             string aNextLabel,
            //             TypeInformation aCtorDeclTypeInfo,
            //             MethodInformation aCtorMethodInfo,
            //             IServiceProvider aServiceProvider,
            //             string aAllocMemLabel
            //         )

            //TODO: What is this for?
            //             if (aCtorDef != null) {
            //                 //if (!aCtorDef.DeclaringType.FullName.StartsWith("Indy.IL2CPU.MultiArrayEmit.ContType"))
            //                 //    Engine.QueueMethod(aCtorDef);
            //             } else {
            //                 throw new ArgumentNullException("aCtorDef");
            //             }

            var xType = xMethod.Value.DeclaringType;

            // If not ValueType, then we need gc
            if( !xType.IsValueType )
            {
                var xParams = xMethod.Value.GetParameters();
                for( int i = 1; i < xParams.Length; i++ )
                {
                    Assembler.Stack.Pop();
                }

                uint xMemSize = GetFieldStorageSize( xType );
                int xExtraSize = 20;
                new CPUx86.Push { DestinationValue = ( uint )( xMemSize + xExtraSize ) };

                new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName( GCImplementationRefs.AllocNewObjectRef ) };
                new CPUx86.Test { DestinationReg = CPUx86.Registers.ECX, SourceValue = 2 };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
                //var xIncRefInfo = aServiceProvider.GetService<IMetaDataInfoService>().GetMethodInfo( GCImplementationRefs.IncRefCountRef, false );
                // TODO: Why call incref twice?
                //                 new Assembler.X86.Call { DestinationLabel = xIncRefInfo.LabelName };
                //                 new CPUx86.Call { DestinationLabel = xIncRefInfo.LabelName };

                new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName( GCImplementationRefs.IncRefCountRef ) };

                uint xObjSize = 0;
                //int xGCFieldCount = ( from item in aCtorDeclTypeInfo.Fields.Values
                //where item.NeedsGC
                //select item ).Count();

                //int xGCFieldCount = ( from item in aCtorDeclTypeInfo.Fields.Values
                //where item.NeedsGC
                //select item ).Count();
                int xGCFieldCount = xType.GetFields().Count( x => x.FieldType.IsValueType );

                string strTypeId = xMethod.Value.DeclaringType.FullName;

                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EBX, SourceRef = Indy.IL2CPU.Assembler.ElementReference.New( strTypeId ), SourceIsIndirect = true };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EBX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceValue = ( uint )InstanceTypeEnum.NormalObject, Size = 32 };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 8, SourceValue = ( uint )xGCFieldCount, Size = 32 };
                uint xSize = ( uint )( ( ( from item in xParams
                                           let xQSize = Align(GetFieldStorageSize( item.GetType() ), 4 )
                                           select ( int )xQSize ).Take( xParams.Length - 1 ).Sum() ) );

                for( int i = 1; i < xParams.Length; i++ )
                {
                    new Comment( String.Format( "Arg {0}: {1}", i, GetFieldStorageSize( xParams[i].GetType() ) ) );
                    for( int j = 0; j < GetFieldStorageSize( xParams[ i ].GetType() ); j += 4 )
                    {
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = ( int )( xSize + 4 ) };
                    }
                }

                new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName( xMethod.Value ) };
                new CPUx86.Test { DestinationReg = CPUx86.Registers.ECX, SourceValue = 2 };
                //new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = aCurrentLabel + "_NO_ERROR_4" };
								throw new Exception("Notimpl");

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
                PushAlignedParameterSize( xMethod.Value );  

                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
								//foreach( StackContent xStackInt in Assembler.Stack )
								//{
								//    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = ( uint )xStackInt.Size };
								//}
                //Call.EmitExceptionLogic( aAssembler,
                //                        ( uint )aCurrentILOffset,
                //                        aCurrentMethodInformation,
                //                        aCurrentLabel + "_NO_ERROR_4",
                //                        false,
                //                        null );
                //new Label( aCurrentLabel + "_NO_ERROR_4" );
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
                PushAlignedParameterSize( xMethod.Value ); 

                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                Assembler.Stack.Push( 4, xMethod.Value.DeclaringType );
                throw new NotImplementedException();
            }
            else
            {
                //                 /*
                //                  * Current sitation on stack:
                //                  *   $ESP       Arg
                //                  *   $ESP+..    other items
                //                  *   
                //                  * What should happen:
                //                  *  + The stack should be increased to allow space to contain:
                //                  *         + .ctor arguments
                //                  *         + struct _pointer_ (ref to start of emptied space)
                //                  *         + empty space for struct
                //                  *  + arguments should be copied to the new place
                //                  *  + old place where arguments were should be cleared
                //                  *  + pointer should be set
                //                  *  + call .ctor
                //                  */                
                //                 var xStorageSize = aCtorDeclTypeInfo.StorageSize;
                //                 if (xStorageSize % 4 != 0)
                //                 {
                //                     xStorageSize += 4 - (xStorageSize % 4);
                //                 }
                //                 uint xArgSize = 0;
                //                 foreach (var xArg in aCtorMethodInfo.Arguments.Skip(1))
                //                 {
                //                     xArgSize += xArg.Size + (xArg.Size % 4 == 0
                //                                                             ? 0
                //                                                             : (4 - (xArg.Size % 4)));
                //                 }
                //                 int xExtraArgSize = (int)(xStorageSize - xArgSize);
                //                 if (xExtraArgSize < 0)
                //                 {
                //                     xExtraArgSize = 0;
                //                 }
                //                 if (xExtraArgSize>0)
                //                 {
                //                     new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = (uint)xExtraArgSize };
                //                 }
                //                 new CPUx86.Push { DestinationReg = Registers.ESP };
                //                 aAssembler.Stack.Push(new StackContent(4));
                //                 //at this point, we need to move copy all arguments over. 
                //                 for (int i = 0;i<(xArgSize/4);i++)
                //                 {
                //                     new CPUx86.Push { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = (int)(xStorageSize + 4) }; // + 4 because the ptr is pushed too
                //                     new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = (int)(xStorageSize + 4 + 4), SourceValue = 0, Size = 32 };
                //                 }
                //                 var xCall = new Call(aCtorDef,
                //                                      (uint)aCurrentILOffset,
                //                                      true,
                //                                      aNextLabel);
                //                 xCall.SetServiceProvider(aServiceProvider);
                //                 xCall.Assembler = aAssembler;
                //                 xCall.Assemble();
                //                 aAssembler.Stack.Push(new StackContent((int)xStorageSize,
                //                                                                aCtorDef.DeclaringType));
                throw new NotImplementedException();
            }
        }

        private uint Align( uint aSize, uint aAlign )
        {
            return aSize % 4 == 0 ? aSize : ( ( aSize / aAlign ) * aAlign ) + 1;
        }

        private void PushAlignedParameterSize( System.Reflection.MethodBase aMethod )
        {
            System.Reflection.ParameterInfo[] xParams = aMethod.GetParameters();

            uint xSize;

            for( int i = 1; i < xParams.Length; i++ )
            {
                xSize = GetFieldStorageSize( xParams[ i ].GetType() );
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = Align( xSize, 4 ) };
            }
        }
        // using System.Collections.Generic;
        // using System.Diagnostics;
        // using System.Linq;
        // using System.Reflection;
        // using Indy.IL2CPU.Assembler;
        // using Indy.IL2CPU.Assembler.X86;
        // using CPU=Indy.IL2CPU.Assembler;
        // using CPUx86=Indy.IL2CPU.Assembler.X86;
        // using Asm=Indy.IL2CPU.Assembler;
        // using Assembler=Indy.IL2CPU.Assembler.Assembler;
        // using Indy.IL2CPU.Compiler;
        // 
        // namespace Indy.IL2CPU.IL.X86
        // {
        //     [OpCode(OpCodeEnum.Newobj)]
        //     public class Newobj : Op
        //     {
        //         public MethodBase CtorDef;
        //         public string CurrentLabel;
        //         public uint ILOffset;
        //         public MethodInformation MethodInformation;
        // 
        //         public static void ScanOp(MethodBase aCtor, IServiceProvider aProvider)
        //         {
        //             Call.ScanOp(aCtor, aProvider);
        //             Call.ScanOp(GCImplementationRefs.AllocNewObjectRef, aProvider);
        //             Call.ScanOp(CPU.Assembler.CurrentExceptionOccurredRef, aProvider);
        //             Call.ScanOp(GCImplementationRefs.IncRefCountRef, aProvider);
        //         }
        // 
        //         public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData, IServiceProvider aProvider)
        //         {
        //             var xCtorDef = aReader.OperandValueMethod;
        //             ScanOp(xCtorDef, aProvider);
        //         }
        // 
        //         public Newobj(ILReader aReader,
        //                       MethodInformation aMethodInfo)
        //             : base(aReader,
        //                    aMethodInfo)
        //         {
        //             CtorDef = aReader.OperandValueMethod;
        //             CurrentLabel = GetInstructionLabel(aReader);
        //             MethodInformation = aMethodInfo;
        //             ILOffset = aReader.Position;
        //             mNextLabel = GetInstructionLabel(aReader.NextPosition);
        //         }
        // 
        //         private string mNextLabel;
        // }

        //TODO: Likely this is used by other things besides newobj
        // and thus should be moved to a different location
        private uint GetFieldStorageSize( Type aType )
        {
            if( aType.FullName == "System.Void" )
            {
                return 0;
            }
            else if( ( !aType.IsValueType && aType.IsClass ) || aType.IsInterface )
            {
                return 4;
            }
            switch( aType.FullName )
            {
                case "System.Char":
                    return 2;
                case "System.Byte":
                case "System.SByte":
                    return 1;
                case "System.UInt16":
                case "System.Int16":
                    return 2;
                case "System.UInt32":
                case "System.Int32":
                    return 4;
                case "System.UInt64":
                case "System.Int64":
                    return 8;
                //TODO: for now hardcode IntPtr and UIntPtr to be 32-bit
                case "System.UIntPtr":
                case "System.IntPtr":
                    return 4;
                case "System.Boolean":
                    return 1;
                case "System.Single":
                    return 4;
                case "System.Double":
                    return 8;
                case "System.Decimal":
                    return 16;
                case "System.Guid":
                    return 16;
                case "System.Enum":
                    return 4;
                case "System.DateTime":
                    return 8;
            }
            if( aType.FullName != null && aType.FullName.EndsWith( "*" ) )
            {
                // pointer
                return 4;
            }
            // array
            //TypeSpecification xTypeSpec = aType as TypeSpecification;
            //if (xTypeSpec != null) {
            //    return 4;
            //}
            if( aType.IsEnum )
            {
                return GetFieldStorageSize( aType.GetField( "value__" ).FieldType );
            }
            if( aType.IsValueType )
            {
                var xSla = aType.StructLayoutAttribute;
                if( xSla != null )
                {
                    if( xSla.Size > 0 )
                    {
                        return ( uint )xSla.Size;
                    }
                }
            }
            uint xResult;
            //GetTypeFieldInfo(aType,  out xResult);
            throw new Exception( "TODO" );
            return xResult;
        }

    }
}
