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

            /// <summary>
            /// True if by default large (32bit), false if small (16bit)
            /// </summary>
            public bool? DefaultSize;
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
                    return "non-existing size!!";
                    throw new Exception("Invalid size: " + aSize);
            }
        }

        public override bool DetermineSize(Indy.IL2CPU.Assembler.Assembler aAssembler, out ulong aSize) {
            InstructionData xInstructionData = null;
            using (mInstructionDatasLocker.AcquireReaderLock()) {
                mInstructionDatas.TryGetValue(this.GetType(), out xInstructionData);
            }
            if (xInstructionData == null) {
                return base.DetermineSize(aAssembler, out aSize);
            }
            var xWithDestAndSourceAndSize = this as InstructionWithDestinationAndSourceAndSize;
            if (xWithDestAndSourceAndSize != null) {
                return DetermineSize(xWithDestAndSourceAndSize, xInstructionData, out aSize);
            }
            var xWithDestAndSource = this as InstructionWithDestinationAndSource;
            if (xWithDestAndSource != null) {
                return base.DetermineSize(aAssembler, out aSize);
            }
            var xWithDestAndSize = this as InstructionWithDestinationAndSize;
            if (xWithDestAndSize != null) {
                return base.DetermineSize(aAssembler, out aSize);
            } 
            var xWithDest = this as InstructionWithDestination;
            if (xWithDest != null) {
                return base.DetermineSize(aAssembler, out aSize);
            }
            if (xInstructionData.EncodingOptions.Count > 0) {
                // todo: improve
                aSize = (ulong)xInstructionData.EncodingOptions[0].OpCode.Length;
                return true;
            }
            aSize = 0;
            return false;
        }

        private static bool DetermineSize(InstructionWithDestinationAndSourceAndSize aInstruction, InstructionData aInstructionData, out ulong aSize) {
            var xTheEncodingOption = GetInstructionEncodingOption(aInstruction, aInstructionData);
            aSize = (ulong)xTheEncodingOption.OpCode.Length;
            if (xTheEncodingOption.NeedsModRMByte) {
                aSize += 1;
            }
            if (aInstruction.DestinationValue.HasValue || aInstruction.DestinationRef != null) {
                aSize += 4;
            }
            if (aInstruction.SourceValue.HasValue || aInstruction.SourceRef != null) {
                aSize += 4;
            }
            if (aInstructionData.DefaultSize.HasValue && aInstructionData.DefaultSize.Value) {
                if (aInstruction.Size < 16) {
                    aSize += 1;
                }
            }

            return true;
        }

        private static InstructionData.InstructionEncodingOption GetInstructionEncodingOption(InstructionWithDestinationAndSourceAndSize aInstruction, InstructionData aInstructionData) {
            InstructionData.InstructionEncodingOption xTheEncodingOption = null;
            for (int i = 0; i < aInstructionData.EncodingOptions.Count; i++) {
                var xEncodingOption = aInstructionData.EncodingOptions[i];
                if (!((xEncodingOption.DestinationReg.HasValue && aInstruction.DestinationReg != Guid.Empty) ||
                     (!xEncodingOption.DestinationReg.HasValue && aInstruction.DestinationReg == Guid.Empty))) {
                    // mismatch
                    continue;
                }
                if (!((xEncodingOption.DestinationMemory && (aInstruction.DestinationValue != null && aInstruction.DestinationIsIndirect)) ||
                      (!xEncodingOption.DestinationMemory && (aInstruction.DestinationValue == null && !aInstruction.DestinationIsIndirect))) && aInstruction.DestinationIsIndirect) {
                    continue;
                }
                if (!((xEncodingOption.DestinationImmediate && aInstruction.DestinationValue != null) ||
                    (!xEncodingOption.DestinationImmediate && aInstruction.DestinationValue == null))) {
                    continue;
                }
                if (!((xEncodingOption.SourceReg.HasValue && aInstruction.SourceReg != Guid.Empty) ||
                     (!xEncodingOption.SourceReg.HasValue && aInstruction.SourceReg == Guid.Empty))) {
                    // mismatch
                    continue;
                }
                if (!((xEncodingOption.SourceMemory && (aInstruction.SourceValue != null && aInstruction.SourceIsIndirect)) ||
                      (!xEncodingOption.SourceMemory && (aInstruction.SourceValue == null && !aInstruction.SourceIsIndirect))) && aInstruction.SourceIsIndirect) {
                    continue;
                }
                if (!((xEncodingOption.SourceImmediate && aInstruction.SourceValue != null) ||
                    (!xEncodingOption.SourceImmediate && aInstruction.SourceValue == null))) {
                    continue;
                }
                xTheEncodingOption = xEncodingOption;
                break;
            }
            if (xTheEncodingOption == null) {
                throw new Exception("No valid EncodingOption found!");
            }
            return xTheEncodingOption;
        }

        public override byte[] GetData(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            InstructionData xInstructionData = null;
            using (mInstructionDatasLocker.AcquireReaderLock()) {
                mInstructionDatas.TryGetValue(this.GetType(), out xInstructionData);
            }
            if (xInstructionData == null) {
                return base.GetData(aAssembler);
            }
            var xWithDestAndSourceAndSize = this as InstructionWithDestinationAndSourceAndSize;
            if (xWithDestAndSourceAndSize != null) {
                return GetData(aAssembler, xWithDestAndSourceAndSize, xInstructionData);
            }
            var xWithDestAndSource = this as InstructionWithDestinationAndSource;
            if (xWithDestAndSource != null) {
                return base.GetData(aAssembler);
            }
            var xWithDestAndSize = this as InstructionWithDestinationAndSize;
            if (xWithDestAndSize != null) {
                return base.GetData(aAssembler);
            }
            var xWithDest = this as InstructionWithDestination;
            if (xWithDest != null) {
                return base.GetData(aAssembler);
            }
            if (xInstructionData.EncodingOptions.Count > 0) {
                // todo: improve
                return xInstructionData.EncodingOptions[0].OpCode;
            }
            return null;
        }

        private static byte[] GetData(Indy.IL2CPU.Assembler.Assembler aAssembler, InstructionWithDestinationAndSourceAndSize aInstruction, InstructionData aInstructionData) {
            var xEncodingOption = GetInstructionEncodingOption(aInstruction, aInstructionData);
            var xSize = xEncodingOption.OpCode.Length;
            if (xEncodingOption.NeedsModRMByte) {
                xSize += 1;
            }
            if (aInstruction.DestinationValue.HasValue || aInstruction.DestinationRef != null) {
                xSize += 4;
            }
            if (aInstruction.SourceValue.HasValue || aInstruction.SourceRef != null) {
                xSize += 4;
            }
            int xExtraOffset = 0;
            if (aInstructionData.DefaultSize.HasValue && aInstructionData.DefaultSize.Value) {
                if (aInstruction.Size < 16) {
                    xSize += 1;
                    xExtraOffset = 1;
                }
            }
            var xBuffer = new byte[xSize];
            Array.Copy(xEncodingOption.OpCode, 0, xBuffer, xExtraOffset, xEncodingOption.OpCode.Length);
            if (xExtraOffset == 1) {
                throw new Exception("OperandSize prefix needed!");
            }
            if (xEncodingOption.NeedsModRMByte) {
                if(aInstruction.DestinationReg != Guid.Empty && !aInstruction.DestinationIsIndirect) {
                    xBuffer[xEncodingOption.OpCode.Length] |= 0xC0;    
                }
                if(aInstruction.DestinationReg != Guid.Empty && aInstruction.DestinationIsIndirect) {
                    //
                }
                // todo: add more ModRM stuff
            }
            if (aInstruction.DestinationReg != Guid.Empty) {
                xBuffer[xEncodingOption.DestinationRegByte] |= (byte)(EncodeRegister(aInstruction.DestinationReg) << xEncodingOption.DestinationRegBitShiftLeft);
            }
            // todo: add more options
            if (aInstruction.SourceValue.HasValue) {
                int xOffset = xEncodingOption.OpCode.Length + xExtraOffset;
                if(xEncodingOption.NeedsModRMByte) {
                    xOffset++;
                }
                Array.Copy(BitConverter.GetBytes(aInstruction.SourceValue.Value), 0, xBuffer, xOffset, 4);
            }
            if(aInstructionData.DefaultSize.HasValue) {
                if(!aInstructionData.DefaultSize.Value && aInstruction.Size > 16) {
                    xBuffer[xEncodingOption.OperandSizeByte] |= (byte)(1 << xEncodingOption.OperandSizeBitShiftLeft);
                }
            }


            //
            return xBuffer;
        }

        private static byte EncodeRegister(Guid aRegister) {
            // todo: implement support for other registers
            if (!Registers.Is32Bit(aRegister)) {
                throw new Exception("Register not supported!");
            }
            if (aRegister == Registers.EAX) return 0x0;
            if (aRegister == Registers.ECX) return 0x1;
            if (aRegister == Registers.EDX) return 0x2;
            if (aRegister == Registers.EBX) return 0x3;
            if (aRegister == Registers.ESP) return 0x4;
            if (aRegister == Registers.EBP) return 0x5;
            if (aRegister == Registers.ESI) return 0x6;
            if (aRegister == Registers.EDI) return 0x7;
            throw new Exception("Register not supported!");
        }

        private ulong? mDataSize;

        public override bool IsComplete(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            var xWithDestAndSourceAndSize = this as InstructionWithDestinationAndSourceAndSize;
            if (xWithDestAndSourceAndSize != null) {
                ulong xAddress;
                if (xWithDestAndSourceAndSize.DestinationRef != null && !xWithDestAndSourceAndSize.DestinationRef.Resolve(aAssembler, out xAddress)) {
                    return false;
                }
                if (xWithDestAndSourceAndSize.DestinationRef != null && !xWithDestAndSourceAndSize.SourceRef.Resolve(aAssembler, out xAddress)) {
                    return false;
                }
            }
            var xWithDestAndSource = this as InstructionWithDestinationAndSource;
            if (xWithDestAndSource != null) {
                ulong xAddress;
                if (xWithDestAndSource.DestinationRef != null && !xWithDestAndSource.DestinationRef.Resolve(aAssembler, out xAddress)) {
                    return false;
                }
                if (xWithDestAndSource.DestinationRef != null && !xWithDestAndSource.SourceRef.Resolve(aAssembler, out xAddress)) {
                    return false;
                }
            }
            var xWithDestAndSize = this as InstructionWithDestinationAndSize;
            if (xWithDestAndSize != null) {
                ulong xAddress;
                if (xWithDestAndSize.DestinationRef != null && !xWithDestAndSize.DestinationRef.Resolve(aAssembler, out xAddress)) {
                    return false;
                }
            }
            var xWithDest = this as InstructionWithDestination;
            if (xWithDest != null) {
                ulong xAddress;
                if (xWithDest.DestinationRef!=null && !xWithDest.DestinationRef.Resolve(aAssembler, out xAddress)) {
                    return false;
                }
            }
            
            return true;
        }

        public override ulong? ActualAddress {
            get {
                if (!StartAddress.HasValue) {
                    return null;
                }
                if(!mDataSize.HasValue){
                    return null;
                }
                return StartAddress.Value + mDataSize.Value;
            }
        }

        private static bool IsLargeRegister(Guid aRegister) {
            return Registers.Is32Bit(aRegister);
        }
    }
}
