using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Asm = Indy.IL2CPU.Assembler;
using Assembler=Indy.IL2CPU.Assembler.Assembler;

namespace Indy.IL2CPU.IL.X86 {
    [OpCode(OpCodeEnum.Newobj, false)]
    public class Newobj : Op {
        public MethodBase CtorDef;
        public string CurrentLabel;
        public int ILOffset;
        public MethodInformation MethodInformation;

        public Newobj(ILReader aReader,
                      MethodInformation aMethodInfo)
            : base(aReader,
                   aMethodInfo) {
            CtorDef = aReader.OperandValueMethod;
            CurrentLabel = GetInstructionLabel(aReader);
            MethodInformation = aMethodInfo;
            ILOffset = aReader.Position;
        }

        public override void DoAssemble() {
            Assemble(Assembler,
                     CtorDef,
                     Engine.RegisterType(CtorDef.DeclaringType),
                     CurrentLabel,
                     MethodInformation,
                     ILOffset);
        }

        public static void Assemble(Assembler.Assembler aAssembler,
                                    MethodBase aCtorDef,
                                    int aTypeId,
                                    string aCurrentLabel,
                                    MethodInformation aCurrentMethodInformation,
                                    int aCurrentILOffset) {
            if (aCtorDef != null) {
                Engine.QueueMethod(aCtorDef);
            } else {
                throw new ArgumentNullException("aCtorDef");
            }
            var xTypeInfo = Engine.GetTypeInfo(aCtorDef.DeclaringType);
            MethodInformation xCtorInfo = Engine.GetMethodInfo(aCtorDef,
                                                                   aCtorDef,
                                                                   Label.GenerateLabelName(aCtorDef),
                                                                   Engine.GetTypeInfo(aCtorDef.DeclaringType),
                                                                   aCurrentMethodInformation.DebugMode);
            if (xTypeInfo.NeedsGC) {
                int xObjectSize = ObjectUtilities.GetObjectStorageSize(aCtorDef.DeclaringType);
                for (int i = 1; i < xCtorInfo.Arguments.Length; i++) {
                    aAssembler.StackContents.Pop();
                }
                Engine.QueueMethod(GCImplementationRefs.AllocNewObjectRef);
                Engine.QueueMethod(GCImplementationRefs.IncRefCountRef);
                int xExtraSize = 20;
                new Pushd("0" + (xObjectSize + xExtraSize).ToString("X").ToUpper() + "h");
                new Assembler.X86.Call(Label.GenerateLabelName(GCImplementationRefs.AllocNewObjectRef));
                Engine.QueueMethod(CPU.Assembler.CurrentExceptionOccurredRef);
                //new CPUx86.Pushd(CPUx86.Registers.EAX);
                new Test(Registers.ECX,
                         2);
                //new CPUx86.JumpIfEquals(aCurrentLabel + "_NO_ERROR_1");
                //for (int i = 1; i < xCtorInfo.Arguments.Length; i++) {
                //    new CPUx86.Add(CPUx86.Registers.ESP, (xCtorInfo.Arguments[i].Size % 4 == 0 ? xCtorInfo.Arguments[i].Size : ((xCtorInfo.Arguments[i].Size / 4) * 4) + 1).ToString());
                //}
                //new CPUx86.Add("esp", "4");
                //Call.EmitExceptionLogic(aAssembler, aCurrentMethodInformation, aCurrentLabel + "_NO_ERROR_1", false);
                //new CPU.Label(aCurrentLabel + "_NO_ERROR_1");
                new Pushd(Registers.AtESP);
                new Pushd(Registers.AtESP);
                new Pushd(Registers.AtESP);
                new Pushd(Registers.AtESP);
                new Assembler.X86.Call(Label.GenerateLabelName(GCImplementationRefs.IncRefCountRef));
                //new CPUx86.Test("ecx", "2");
                //new CPUx86.JumpIfEquals(aCurrentLabel + "_NO_ERROR_2");
                //for (int i = 1; i < xCtorInfo.Arguments.Length; i++) {
                //    new CPUx86.Add(CPUx86.Registers.ESP, (xCtorInfo.Arguments[i].Size % 4 == 0 ? xCtorInfo.Arguments[i].Size : ((xCtorInfo.Arguments[i].Size / 4) * 4) + 1).ToString());
                //}
                //new CPUx86.Add("esp", "16");
                //Call.EmitExceptionLogic(aAssembler, aCurrentMethodInformation, aCurrentLabel + "_NO_ERROR_2", false);
                //new CPU.Label(aCurrentLabel + "_NO_ERROR_2");
                new Assembler.X86.Call(Label.GenerateLabelName(GCImplementationRefs.IncRefCountRef));
                //new CPUx86.Test("ecx", "2");
                //new CPUx86.JumpIfEquals(aCurrentLabel + "_NO_ERROR_3");
                //for (int i = 1; i < xCtorInfo.Arguments.Length; i++) {
                //    new CPUx86.Add(CPUx86.Registers.ESP, (xCtorInfo.Arguments[i].Size % 4 == 0 ? xCtorInfo.Arguments[i].Size : ((xCtorInfo.Arguments[i].Size / 4) * 4) + 1).ToString());
                //}
                //new CPUx86.Add("esp", "12");
                //Call.EmitExceptionLogic(aAssembler, aCurrentMethodInformation, aCurrentLabel + "_NO_ERROR_3", false);
                //new CPU.Label(aCurrentLabel + "_NO_ERROR_3");
                int xObjSize = 0;
                int xGCFieldCount = (from item in Engine.GetTypeFieldInfo(aCtorDef,
                                                                          out xObjSize).Values
                                     where item.NeedsGC
                                     select item).Count();
                new Assembler.X86.Pop(Registers.EAX);
                new Move("dword",
                         Registers.AtEAX,
                         "0" + aTypeId.ToString("X") + "h");
                new Move("dword",
                         "[eax + 4]",
                         "0" + InstanceTypeEnum.NormalObject.ToString("X") + "h");
                new Move("dword",
                         "[eax + 8]",
                         "0x" + xGCFieldCount.ToString("X"));
                int xSize = (from item in xCtorInfo.Arguments
                             select item.Size + (item.Size % 4 == 0
                                                     ? 0
                                                     : (4 - (item.Size % 4)))).Take(xCtorInfo.Arguments.Length - 1).Sum();
                for (int i = 1; i < xCtorInfo.Arguments.Length; i++) {
                    new Pushd("[esp + 0x" + (xSize + 4).ToString("X") + "]");
                }
                new Assembler.X86.Call(Label.GenerateLabelName(aCtorDef));
                new Test(Registers.ECX,
                         2);
                new JumpIfEqual(aCurrentLabel + "_NO_ERROR_4");
                for (int i = 1; i < xCtorInfo.Arguments.Length; i++) {
                    new Assembler.X86.Add(Registers.ESP,
                                          (xCtorInfo.Arguments[i].Size % 4 == 0
                                               ? xCtorInfo.Arguments[i].Size
                                               : ((xCtorInfo.Arguments[i].Size / 4) * 4) + 1).ToString());
                }
                new Assembler.X86.Add(Registers.ESP,
                                      "4");
                foreach (StackContent xStackInt in aAssembler.StackContents) {
                    new Assembler.X86.Add(Registers.ESP,
                                          xStackInt.Size.ToString());
                }
                Call.EmitExceptionLogic(aAssembler,
                                        aCurrentILOffset,
                                        aCurrentMethodInformation,
                                        aCurrentLabel + "_NO_ERROR_4",
                                        false,
                                        null);
                new Label(aCurrentLabel + "_NO_ERROR_4");
                new Assembler.X86.Pop(Registers.EAX);
                //				aAssembler.StackSizes.Pop();
                //	new CPUx86.Add(CPUx86.Registers.ESP, "4");
                for (int i = 1; i < xCtorInfo.Arguments.Length; i++) {
                    new Assembler.X86.Add(Registers.ESP,
                                          (xCtorInfo.Arguments[i].Size % 4 == 0
                                               ? xCtorInfo.Arguments[i].Size
                                               : ((xCtorInfo.Arguments[i].Size / 4) * 4) + 1).ToString());
                }
                new Push(Registers.EAX);
                aAssembler.StackContents.Push(new StackContent(4,
                                                               aCtorDef.DeclaringType));
            }else {
                int xSize = xTypeInfo.StorageSize;
                if(xSize % 4 != 0) {
                    xSize += 4 - (xSize % 4);
                }
                new CPUx86.Move("eax",
                                "esp");
                for(int i = 0; i < xSize / 4;i++) {
                    new CPUx86.Push("0");
                }
                new CPUx86.Push("esp");
                int xArgSize = (from item in xCtorInfo.Arguments
                             select item.Size + (item.Size % 4 == 0
                                                     ? 0
                                                     : (4 - (item.Size % 4)))).Take(xCtorInfo.Arguments.Length - 1).Sum();
                if (xArgSize < xSize) {
                    throw new NotImplementedException("Support for creating new structs using a ctor which argument size is less than the struct size is not yet supported!"); }
                xArgSize += xSize + 4;
                for (int i = 1; i < xCtorInfo.Arguments.Length; i++)
                {
                    new Pushd("[esp + 0x" + (xArgSize + 4).ToString("X") + "]");
                }
                new Assembler.X86.Call(Label.GenerateLabelName(aCtorDef));
                new Test(Registers.ECX,
                         2);
                new JumpIfEqual(aCurrentLabel + "_NO_ERROR_4");
                for (int i = 1; i < xCtorInfo.Arguments.Length; i++)
                {
                    new Assembler.X86.Add(Registers.ESP,
                                          (xCtorInfo.Arguments[i].Size % 4 == 0
                                               ? xCtorInfo.Arguments[i].Size
                                               : ((xCtorInfo.Arguments[i].Size / 4) * 4) + 1).ToString());
                }
                new Assembler.X86.Add(Registers.ESP,
                                      "4");
                foreach (StackContent xStackInt in aAssembler.StackContents)
                {
                    new Assembler.X86.Add(Registers.ESP,
                                          xStackInt.Size.ToString());
                }
                Call.EmitExceptionLogic(aAssembler,
                                        aCurrentILOffset,
                                        aCurrentMethodInformation,
                                        aCurrentLabel + "_NO_ERROR_4",
                                        false,
                                        null);
                new Label(aCurrentLabel + "_NO_ERROR_4");
                // at this point, there's the normal arguments on the stack, and the newly created object value
                for(int i = 0; i < xSize /4;i++) {
                    new CPUx86.Pop("eax");
                    new CPUx86.Move("[esp + " + (xArgSize + i * 4) + "]",
                                    "eax");
                }
            }
        }
    }
}