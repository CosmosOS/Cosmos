using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Cosmos.Compiler.Assembler.X86 {
    // todo: cache the EncodingOption and InstructionData instances..
    public abstract class Instruction : Cosmos.Compiler.Assembler.Instruction
    {
        [Flags]
        public enum InstructionSizes {
            None,
            Byte = 8,
            Word = 16,
            DWord = 32,
            QWord = 64,
            All = Byte | Word| DWord
        }

        public enum InstructionSize {
            None = 0,
            Byte = 8,
            Word = 16,
            DWord = 32,
            QWord = 64
        }
        [Flags]
        public enum OperandMemoryKinds { 
            Default = Address | IndirectReg | IndirectRegOffset,
            Address = 1,
            IndirectReg = 2,
            IndirectRegOffset = 4
        }

        static Instruction() {
                //mInstructionDatas = new SortedList<Type, InstructionData>(new TypeComparer());
                //foreach (Type xType in typeof(Instruction).Assembly.GetTypes()) {
                //    if (!xType.IsSubclassOf(typeof(Instruction))) {
                //        continue;
                //    }
                //    if (!xType.Namespace.StartsWith(typeof(Instruction).Namespace)) {
                //        continue;
                //    }
                //    if (xType.IsAbstract) {
                //        continue;
                //    }
                //    var xAttrib = xType.GetCustomAttributes(typeof(OpCodeAttribute), true).FirstOrDefault() as OpCodeAttribute;
                //    if (xAttrib == null) {
                //        continue;
                //    }
                //    var xNewInstructionData = new InstructionData();
                //    mInstructionDatas.Add(xType, xNewInstructionData);
                //    if (xType.IsSubclassOf(typeof(InstructionWithDestination))) {
                //        xNewInstructionData.HasDestinationOperand = true;
                //    }
                //    if (xType.IsSubclassOf(typeof(InstructionWithDestinationAndSource))) {
                //        xNewInstructionData.HasDestinationOperand = true;
                //        xNewInstructionData.HasSourceOperand = true;
                //    }
                //    var xMethod = xType.GetMethod("InitializeEncodingData", new Type[] { typeof(InstructionData) });
                //    if (xMethod != null) {
                //        xMethod.Invoke(null, new object[] { xNewInstructionData });
                //    }
                //}
        }

        protected Instruction() {
        }

        protected Instruction(bool aAddToAssembler)
        {
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
				case 80:
					return string.Empty;
                default:
                    return "dword";
                 //   throw new Exception("Invalid size: " + aSize);
            }
        }
    }
}
