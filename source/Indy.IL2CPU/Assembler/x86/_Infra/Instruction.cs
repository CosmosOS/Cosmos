using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Indy.IL2CPU.Assembler.X86 {
    public abstract class Instruction : Indy.IL2CPU.Assembler.Instruction {
        public class InstructionData {
            public class InstructionEncodingOption {
                public byte[] OpCode;

                /// <summary>
                /// If ModR/M byte needed, set to true. If true, all other fields on <see cref="InstructionEncodingOption"/>
                /// which refer to <see cref="OpCode"/> bytes, can assume an extra ModRM byte.
                /// </summary>
                public bool NeedsModRMByte;

                /// <summary>
                /// true is large, false small. if null, this option is not specific to a given size
                /// </summary>
                public bool? OperandSize;
                /// <summary>
                /// the indx in OpCode where the OperandSize bit is encoded
                /// </summary>
                public byte OperandSizeByte;
                /// <summary>
                /// the amount of bits the operandsize bit gets shifted to left, if neccessary
                /// </summary>
                public byte OperandSizeBitShiftLeft;

                /// <summary>
                /// is this EncodingOption valid for situations where the Destination is a register?
                /// if so, DestinationReg == Guid.Empty. if it is specific to a given register, the 32bit id is put in DestinationReg
                /// </summary>
                public Guid? DestinationReg;
                /// <summary>
                /// the index in OpCode where the DestinationReg bit is encoded
                /// </summary>
                public byte DestinationRegByte;
                /// <summary>
                /// the amount of bits the DestinationReg bits gets shifted to left, if neccessary
                /// </summary>
                public byte DestinationRegBitShiftLeft;


                /// <summary>
                /// is this EncodingOption valid for situations where the Destination operand is memory?
                /// </summary>
                public bool DestinationMemory;
                /// <summary>
                /// is this EncodingOption valid for situations where the Destination is an immediate value
                /// </summary>
                public bool DestinationImmediate;

                /// <summary>
                /// is this EncodingOption valid for situations where the Source is a register?
                /// if so, SourceReg == Guid.Empty. if it is specific to a given register, the 32bit id is put in SourceReg
                /// </summary>
                public Guid? SourceReg;
                /// <summary>
                /// is this EncodingOption valid for situations where the Destination operand is memory?
                /// </summary>
                public bool SourceMemory;
                /// <summary>
                /// is this EncodingOption valid for situations where the Destination is an immediate value
                /// </summary>
                public bool SourceImmediate;

            }

            public bool HasDestinationOperand;
            public bool HasSourceOperand;

            public List<InstructionEncodingOption> EncodingOptions = new List<InstructionEncodingOption>();
        }

        private static SortedList<Type, InstructionData> mInstructionDatas;
        private static ReaderWriterLocker mInstructionDatasLocker;
        static Instruction() {
            mInstructionDatasLocker = new ReaderWriterLocker();
            using (mInstructionDatasLocker.AcquireWriterLock()) {
                mInstructionDatas = new SortedList<Type, InstructionData>(new TypeComparer());
                int xInstructionsWithEncodingOptions = 0;
                foreach (Type xType in typeof(Instruction).Assembly.GetTypes()) {
                    if (!xType.IsSubclassOf(typeof(Instruction))) {
                        continue;
                    }
                    if (!xType.Namespace.StartsWith(typeof(Instruction).Namespace)) {
                        continue;
                    }
                    if (xType.IsAbstract) {
                        continue;
                    }
                    var xAttrib = xType.GetCustomAttributes(typeof(OpCodeAttribute), true).FirstOrDefault() as OpCodeAttribute;
                    if (xAttrib == null) {
                        continue;
                    }
                    var xNewInstructionData = new InstructionData();
                    mInstructionDatas.Add(xType, xNewInstructionData);
                    if (xType.IsSubclassOf(typeof(InstructionWithDestination))) {
                        xNewInstructionData.HasDestinationOperand = true;
                    }
                    if (xType.IsSubclassOf(typeof(InstructionWithDestinationAndSource))) {
                        xNewInstructionData.HasDestinationOperand = true;
                        xNewInstructionData.HasSourceOperand = true;
                    }
                    var xMethod = xType.GetMethod("InitializeEncodingData", new Type[] { typeof(InstructionData) });
                    if (xMethod != null) {
                        xMethod.Invoke(null, new object[]{xNewInstructionData});
                    }
                    if (xNewInstructionData.EncodingOptions.Count > 0) {
                        xInstructionsWithEncodingOptions++;
                    }
                }
                Console.WriteLine("Total Instructions = {0}, Instructions with encoding data = {1}", mInstructionDatas.Count, xInstructionsWithEncodingOptions);
            }
        }

        protected Instruction() { 
        }
        protected static string SizeToString(byte aSize) {
            switch (aSize) {
                case 8:
                    return "byte";
                case 16:
                    return "word";
                case 32:
                    return "dword";
                case 64:
                    return "qword";
                default:
                    throw new Exception("Invalid size: " + aSize);
            }
        } 
    }
}
