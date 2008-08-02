using System;
using System.Collections.Generic;
using System.IO;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using System.Reflection;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
    [OpCode(OpCodeEnum.Isinst)]
    public class Isinst : Op {
        private int mTypeId;
        private string mThisLabel;
        private string mNextOpLabel;
        private int mCurrentILOffset;
        private bool mDebugMode;

        public static void ScanOp(ILReader aReader,
                                  MethodInformation aMethodInfo,
                                  SortedList<string, object> aMethodData) {
            Type xType = aReader.OperandValueType;
            if (xType == null) {
                throw new Exception("Unable to determine Type!");
            }
            Engine.RegisterType(xType);
            Call.ScanOp(Engine.GetMethodBase(typeof(VTablesImpl),
                                             "IsInstance",
                                             "System.Int32",
                                             "System.Int32"));
            Newobj.ScanOp(typeof(InvalidCastException).GetConstructor(new Type[0]));
        }

        public Isinst(ILReader aReader,
                      MethodInformation aMethodInfo)
            : base(aReader,
                   aMethodInfo) {
            Type xType = aReader.OperandValueType;
            if (xType == null) {
                throw new Exception("Unable to determine Type!");
            }
            Type xTypeDef = xType;
            mTypeId = Engine.RegisterType(xTypeDef);
            mThisLabel = GetInstructionLabel(aReader.Position);
            mNextOpLabel = GetInstructionLabel(aReader.NextPosition);
            mCurrentILOffset = (int)aReader.Position;
            mDebugMode = aMethodInfo.DebugMode;
        }

        public override void DoAssemble() {
            string mReturnNullLabel = mThisLabel + "_ReturnNull";
            new CPUx86.Move(CPUx86.Registers.EAX,
                            "[esp]");
            new CPUx86.Compare(CPUx86.Registers.EAX,
                               "0");
            new CPUx86.JumpIfZero(mReturnNullLabel);
            new CPUx86.Pushd(CPUx86.Registers.AtEAX);
            new CPUx86.Pushd("0x" + mTypeId.ToString("X"));
            Assembler.StackContents.Push(new StackContent(4,
                                                          typeof(object)));
            Assembler.StackContents.Push(new StackContent(4,
                                                          typeof(object)));
            MethodBase xMethodIsInstance = Engine.GetMethodBase(typeof(VTablesImpl),
                                                                "IsInstance",
                                                                "System.Int32",
                                                                "System.Int32");
            Engine.QueueMethod(xMethodIsInstance);
            Op xOp = new Call(xMethodIsInstance,
                              mCurrentILOffset,
                              mDebugMode,
                              mThisLabel+ "_After_IsInstance_Call");
            xOp.Assembler = Assembler;
            xOp.Assemble();
            new Label(mThisLabel + "_After_IsInstance_Call");
            Assembler.StackContents.Pop();
            new CPUx86.Pop(CPUx86.Registers.EAX);
            new CPUx86.Compare(CPUx86.Registers.EAX,
                               "0");
            new CPUx86.JumpIfNotEqual(mNextOpLabel);
            new CPU.Label(mReturnNullLabel);
            new CPUx86.Add("esp",
                           "4");
            new CPUx86.Pushd("0");
        }
    }
}