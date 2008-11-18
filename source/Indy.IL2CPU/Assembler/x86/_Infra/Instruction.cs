using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Indy.IL2CPU.Assembler.X86 {
    // todo: cache the EncodingOption and InstructionData instances..
    public abstract class Instruction : Indy.IL2CPU.Assembler.Instruction {
        [Flags]
        public enum InstructionSizes {
            None,
            Byte=8,
            Word=16,
            DWord=32,
            QWord = 64,
        }

        public enum InstructionSize {
            None = 0,
            Byte = 8,
            Word = 16,
            DWord = 32,
            QWord = 64,
        }
        public class InstructionData {
            public class InstructionEncodingOption {
                public Action<byte[], Instruction> ModifyBytes;

                public byte[] OpCode;

                /// <summary>
                /// If ModR/M byte needed, set to true. If true, all other fields on <see cref="InstructionEncodingOption"/>
                /// which refer to <see cref="OpCode"/> bytes, can assume an extra ModRM byte.
                /// </summary>
                public bool NeedsModRMByte;
                public byte InitialModRMByteValue;

                public InstructionSizes AllowedSizes;
                public InstructionSize DefaultSize = InstructionSize.DWord;
                /// <summary>
                /// the indx in OpCode where the OperandSize bit is encoded
                /// </summary>
                public byte? OperandSizeByte;
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
                public byte? DestinationRegByte;
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
                /// the index in OpCode where the SourceReg bit is encoded
                /// </summary>
                public byte? SourceRegByte;
                /// <summary>
                /// the amount of bits the SourceReg bits gets shifted to left, if necessary
                /// </summary>
                public byte SourceRegBitShiftLeft;
                /// <summary>
                /// is this EncodingOption valid for situations where the Source operand is memory?
                /// </summary>
                public bool SourceMemory;
                /// <summary>
                /// is this EncodingOption valid for situations where the Source is an immediate value
                /// </summary>
                public bool SourceImmediate;

            }

            public bool HasDestinationOperand;
            public bool HasSourceOperand;

            /// <summary>
            /// True if by default large (32bit), false if small (16bit)
            /// </summary>
            //public InstructionSize DefaultSize = InstructionSize.DWord;
            public List<InstructionEncodingOption> EncodingOptions = new List<InstructionEncodingOption>();
        }

        private static SortedList<Type, InstructionData> mInstructionDatas;
        private static ReaderWriterLocker mInstructionDatasLocker;
        static Instruction() {
            mInstructionDatasLocker = new ReaderWriterLocker();
            using (mInstructionDatasLocker.AcquireWriterLock()) {
                mInstructionDatas = new SortedList<Type, InstructionData>(new TypeComparer());
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
                        xMethod.Invoke(null, new object[] { xNewInstructionData });
                    }
                }
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

        private static bool GetEffectiveInstructionInfo(Instruction aInstruction, IInstructionWithDestination aInstructionWithDestination, IInstructionWithSize aInstructionWithSize, IInstructionWithSource aInstructionWithSource, out InstructionData aInstructionData, out InstructionData.InstructionEncodingOption aEncodingOption) {
            using (mInstructionDatasLocker.AcquireReaderLock()) {
                mInstructionDatas.TryGetValue(aInstruction.GetType(), out aInstructionData);
            }
            if (aInstructionData == null) {
                aEncodingOption = null;
                return false;
            }
            aEncodingOption = null;
            for (int i = 0; i < aInstructionData.EncodingOptions.Count; i++) {
                
                var xEncodingOption = aInstructionData.EncodingOptions[i];
                if (aInstructionWithDestination != null) {
                    if (!(((xEncodingOption.DestinationMemory || xEncodingOption.DestinationReg.HasValue) && (aInstructionWithDestination.DestinationReg != Guid.Empty || aInstructionWithDestination.DestinationValue.HasValue)) ||
                         (!(xEncodingOption.DestinationMemory || xEncodingOption.DestinationReg.HasValue) && aInstructionWithDestination.DestinationReg == Guid.Empty && aInstructionWithDestination.DestinationValue.HasValue))) {
                        // mismatch
                        continue;
                    }
                    if ((!((xEncodingOption.DestinationMemory && (aInstructionWithDestination.DestinationValue != null && aInstructionWithDestination.DestinationIsIndirect)) ||
                          (!xEncodingOption.DestinationMemory && (aInstructionWithDestination.DestinationValue == null && !aInstructionWithDestination.DestinationIsIndirect))) &&
                         !((xEncodingOption.DestinationMemory && (aInstructionWithDestination.DestinationReg != Guid.Empty && aInstructionWithDestination.DestinationIsIndirect)) ||
                          (!xEncodingOption.DestinationMemory && (aInstructionWithDestination.DestinationReg != Guid.Empty && !aInstructionWithDestination.DestinationIsIndirect)))) && aInstructionWithDestination.DestinationIsIndirect) {
                        continue;
                    }
                    if (!((xEncodingOption.DestinationImmediate && aInstructionWithDestination.DestinationValue != null && !aInstructionWithDestination.DestinationIsIndirect) ||
                        (!xEncodingOption.DestinationImmediate && (aInstructionWithDestination.DestinationValue == null || aInstructionWithDestination.DestinationIsIndirect)))) {
                        continue;
                    }
                    if (xEncodingOption.DestinationReg.HasValue && xEncodingOption.DestinationReg != Guid.Empty && aInstructionWithDestination.DestinationReg != Guid.Empty && aInstructionWithDestination.DestinationIsIndirect == xEncodingOption.DestinationMemory) {
                        if (xEncodingOption.DestinationReg != aInstructionWithDestination.DestinationReg) {
                                switch (xEncodingOption.DefaultSize) {
                                    case InstructionSize.Byte: {
                                            if ((xEncodingOption.AllowedSizes & InstructionSizes.Byte) != 0) {
                                                var xTheActualReg = Registers.Get8BitRegistersForRegister(aInstructionWithDestination.DestinationReg);
                                                if (xTheActualReg != xEncodingOption.DestinationReg.Value) {
                                                    continue;
                                                }
                                            }
                                            break;
                                        }
                                    case InstructionSize.Word: {
                                            if ((xEncodingOption.AllowedSizes & InstructionSizes.Word) != 0) {
                                                var xTheActualReg = Registers.Get16BitRegisterForRegister(aInstructionWithDestination.DestinationReg);
                                                if (xTheActualReg != xEncodingOption.DestinationReg.Value) {
                                                    continue;
                                                }
                                            }
                                            break;
                                        }
                                    case InstructionSize.DWord: {
                                            if ((xEncodingOption.AllowedSizes & InstructionSizes.DWord) != 0) {
                                                var xTheActualReg = Registers.Get32BitRegisterForRegister(aInstructionWithDestination.DestinationReg);
                                                if (xTheActualReg != xEncodingOption.DestinationReg.Value) {
                                                    continue;
                                                }
                                            }
                                            break;
                                        }
                                    default:
                                        throw new Exception("InstructionSize not implemented yet!");
                                }
                        }
                    }
                }
                if (aInstructionWithSource != null) {
                    if (!(((xEncodingOption.SourceMemory || xEncodingOption.SourceReg.HasValue) && (aInstructionWithSource.SourceReg != Guid.Empty || aInstructionWithSource.SourceValue.HasValue)) ||
                         (!(xEncodingOption.SourceMemory || xEncodingOption.SourceReg.HasValue) && aInstructionWithSource.SourceReg == Guid.Empty && aInstructionWithSource.SourceValue.HasValue))) {
                        // mismatch
                        continue;
                    }

                    if ((!((xEncodingOption.SourceMemory && (aInstructionWithSource.SourceValue != null && aInstructionWithSource.SourceIsIndirect)) ||
                          (!xEncodingOption.SourceMemory && (aInstructionWithSource.SourceValue == null && !aInstructionWithSource.SourceIsIndirect))) &&
                         !((xEncodingOption.SourceMemory && (aInstructionWithSource.SourceReg != Guid.Empty && aInstructionWithSource.SourceIsIndirect)) ||
                          (!xEncodingOption.SourceMemory && (aInstructionWithSource.SourceReg != Guid.Empty && !aInstructionWithSource.SourceIsIndirect)))) && aInstructionWithSource.SourceIsIndirect) {
                        continue;
                    }
                    if (!((xEncodingOption.SourceImmediate && aInstructionWithSource.SourceValue != null && !aInstructionWithSource.SourceIsIndirect) ||
                        (!xEncodingOption.SourceImmediate && (aInstructionWithSource.SourceValue == null || aInstructionWithSource.SourceIsIndirect)))) {
                        continue;
                    }
                }
                aEncodingOption = xEncodingOption;
                break;
            }
            if (aEncodingOption == null) {
                throw new Exception("No valid EncodingOption found!");
            }
            return true;
        }

        private static bool DetermineSize(Indy.IL2CPU.Assembler.Assembler aAssembler, out ulong aSize, Instruction aInstruction, IInstructionWithDestination aInstructionWithDestination, IInstructionWithSize aInstructionWithSize, IInstructionWithSource aInstructionWithSource, InstructionData aInstructionData, InstructionData.InstructionEncodingOption aEncodingOption) {
            aSize = (ulong)aEncodingOption.OpCode.Length;
            if (aEncodingOption.NeedsModRMByte) {
                aSize += 1;
                bool xSIB = false;
                if (aInstructionWithDestination != null &&
                    (((/*aInstructionWithDestination.DestinationReg == Registers.EBP ||*/ aInstructionWithDestination.DestinationReg == Registers.ESP) && aInstructionWithDestination.DestinationIsIndirect) /*||
                     aInstructionWithDestination.DestinationReg == Registers.ESP*/)) {
                    aSize++;
                    xSIB = true;
                } else {
                    if (aInstructionWithDestination != null && aInstructionWithDestination.DestinationReg == Registers.EBP && aInstructionWithDestination.DestinationIsIndirect && aInstructionWithDestination.DestinationDisplacement == 0) {
                        aSize++;
                        xSIB = true;
                    } else {
                        if (aInstructionWithSource != null &&
                            ((aInstructionWithSource.SourceReg == Registers.EBP && (aInstructionWithSource.SourceReg != Guid.Empty && aInstructionWithSource.SourceIsIndirect)) /*||
                         aInstructionWithSource.SourceReg == Registers.ESP*/
                                                                                )) {
                            aSize++;
                            xSIB = true;
                        }
                    }
                }
                if (aInstructionWithDestination != null && aInstructionWithDestination.DestinationIsIndirect && aInstructionWithDestination.DestinationDisplacement > 0) {
                    if (aInstructionWithDestination.DestinationDisplacement < 128) {
                        aSize += 1; // for now use 16bit displacement
                    } else {
                        if (aInstructionWithDestination.DestinationDisplacement <= Int16.MaxValue) {
                            aSize += 4;
                        } else {
                            aSize += 4;
                        }
                    }
                    if (xSIB) {
                        // aSize -= 1;
                    }
                }
            }
            if (aInstructionWithDestination != null && aInstructionWithSize != null) {
                if (aInstructionWithDestination != null && aInstructionWithDestination.DestinationValue.HasValue) {
                    aSize += (ulong)aInstructionWithSize.Size / 8;
                }
            }
            if (aInstructionWithDestination != null && aInstructionWithDestination.DestinationRef != null) {
                aSize += 4;
            }
            if (aInstructionWithSource != null && aInstructionWithSource.SourceValue.HasValue) {
                aSize += (ulong)aInstructionWithSize.Size / 8;
            }
            if (aInstructionWithSource != null && aInstructionWithSource.SourceRef != null) {
                aSize += 4;
            }
            if (aInstructionWithDestination != null && (aInstructionWithDestination.DestinationValue.HasValue && !(aInstructionWithDestination.DestinationIsIndirect && aInstructionWithDestination.DestinationDisplacement > 0))) {
                aSize += (ulong)aEncodingOption.DefaultSize / 8;
            }
            if (aEncodingOption.DefaultSize == InstructionSize.DWord && aInstructionWithSize != null && aInstructionWithSize.Size == 16) {
                aSize += 1;
            }
            if (aEncodingOption.DefaultSize == InstructionSize.Word && aInstructionWithSize != null && aInstructionWithSize.Size == 32) {
                aSize += 1;
            }
            aInstruction.mDataSize = aSize;
            return true;
        }
        public override bool DetermineSize(Indy.IL2CPU.Assembler.Assembler aAssembler, out ulong aSize) {
            var xInstructionWithDestination = this as IInstructionWithDestination;
            var xInstructionWithSource = this as IInstructionWithSource;
            var xInstructionWithSize = this as IInstructionWithSize;
            InstructionData xInstructionData = null;
            InstructionData.InstructionEncodingOption xEncodingOption = null;
            if (!GetEffectiveInstructionInfo(this, xInstructionWithDestination, xInstructionWithSize, xInstructionWithSource, out xInstructionData, out xEncodingOption)) {
                return base.DetermineSize(aAssembler, out aSize);
            }
            return DetermineSize(aAssembler, out aSize, this, xInstructionWithDestination, xInstructionWithSize, xInstructionWithSource, xInstructionData, xEncodingOption);
        }

        public override bool IsComplete(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            var xWithDest = this as IInstructionWithDestination;
            var xWithSource = this as IInstructionWithSource;
            var xWithSize = this as IInstructionWithSize;
            ulong xAddress;
            if (xWithDest != null) {
                if (xWithDest.DestinationRef != null && !xWithDest.DestinationRef.Resolve(aAssembler, out xAddress)) {
                    return false;
                }
            }
            if (xWithSource != null) {
                if (xWithSource.SourceRef != null && !xWithSource.SourceRef.Resolve(aAssembler, out xAddress)) {
                    return false;
                }
            }
            return true;
        }

        private ulong? mDataSize;

        public override ulong? ActualAddress {
            get {
                if (!StartAddress.HasValue) {
                    return null;
                }
                if (!mDataSize.HasValue) {
                    return null;
                }
                return StartAddress.Value + mDataSize.Value;
            }
        }

        public override byte[] GetData(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            var xInstructionWithDestination = this as IInstructionWithDestination;
            var xInstructionWithSource = this as IInstructionWithSource;
            var xInstructionWithSize = this as IInstructionWithSize;
            InstructionData xInstructionData = null;
            InstructionData.InstructionEncodingOption xEncodingOption = null;
            if (!GetEffectiveInstructionInfo(this, xInstructionWithDestination, xInstructionWithSize, xInstructionWithSource, out xInstructionData, out xEncodingOption)) {
                return base.GetData(aAssembler);
            }
            return GetData(aAssembler, this, xInstructionWithDestination, xInstructionWithSize, xInstructionWithSource, xInstructionData, xEncodingOption);
        }

        private static byte[] GetData(Indy.IL2CPU.Assembler.Assembler aAssembler, Instruction aInstruction, IInstructionWithDestination aInstructionWithDestination, IInstructionWithSize aInstructionWithSize, IInstructionWithSource aInstructionWithSource, InstructionData aInstructionData, InstructionData.InstructionEncodingOption aEncodingOption) {
            ulong xSize = 0;
            Instruction.DetermineSize(aAssembler, out xSize, aInstruction, aInstructionWithDestination, aInstructionWithSize, aInstructionWithSource, aInstructionData, aEncodingOption);
            if (xSize == 0) {
                return new byte[0];
            }
            var xBuffer = new byte[xSize];
            int xExtraOffset = 0;
            int xOpCodeOffset = 0;
            if (aInstructionWithSize != null) {
                if (aEncodingOption.DefaultSize == InstructionSize.DWord && aInstructionWithSize.Size == 16) {
                    xOpCodeOffset += 1;
                    xExtraOffset++;
                    xBuffer[0] = 0x66;
                }
                if (aEncodingOption.DefaultSize == InstructionSize.Word && aInstructionWithSize.Size == 32) {
                    xOpCodeOffset += 1;
                    xExtraOffset++;
                    xBuffer[0] = 0x66;
                }
            }
            Array.Copy(aEncodingOption.OpCode, 0, xBuffer, xExtraOffset, aEncodingOption.OpCode.Length);
            if (aInstructionWithDestination != null) {
                if (aInstructionWithDestination.DestinationReg != Guid.Empty && aEncodingOption.DestinationRegByte.HasValue && !aEncodingOption.NeedsModRMByte) {
                    xBuffer[aEncodingOption.DestinationRegByte.Value + xExtraOffset] |= (byte)(EncodeRegister(aInstructionWithDestination.DestinationReg) << aEncodingOption.DestinationRegBitShiftLeft);
                }
            }
            //if (aInstructionWithSource != null && aInstructionWithSource.SourceReg != Guid.Empty) {
            //    var xIdx = aEncodingOption.OpCode.Length + xExtraOffset;
            //    if(xIdx >= xBuffer.Length) {
            //        xIdx = xBuffer.Length - 1;
            //    }
            //    xBuffer[xIdx] |= (byte)(EncodeRegister(aInstructionWithSource.SourceReg) << 3);
            //}

            if (aEncodingOption.NeedsModRMByte) {
                xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] = aEncodingOption.InitialModRMByteValue;
                if (aInstructionWithDestination != null) {
                    if (aInstructionWithDestination.DestinationReg != Guid.Empty) {
                        xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= EncodeRegister(aInstructionWithDestination.DestinationReg);
                    }
                    if (aInstructionWithSource != null && aInstructionWithSource.SourceReg != Guid.Empty) {
                        xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= (byte)(EncodeRegister(aInstructionWithSource.SourceReg) << 3);
                    }
                    byte? xSIB = null;
                    if (aInstructionWithDestination.DestinationIsIndirect) {
                        if (((/*aInstructionWithDestination.DestinationReg == Registers.EBP ||*/ aInstructionWithDestination.DestinationReg == Registers.ESP) && (aInstructionWithDestination.DestinationIsIndirect))) {
                            if (aInstructionWithDestination.DestinationReg == Registers.EBP) {
                                xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 1 << 6;
                                xSIB = 0;
                            }
                            if (aInstructionWithDestination.DestinationReg == Registers.ESP) {
                                xSIB = 0x24;
                            }
                        } else {
                            if (!(aInstructionWithSource != null &&
                                ((aInstructionWithSource.SourceReg == Registers.EBP && !(aInstructionWithSource.SourceReg != Guid.Empty && aInstructionWithSource.SourceIsIndirect)) ||
                                 aInstructionWithSource.SourceReg == Registers.ESP)) && (aInstructionWithDestination.DestinationReg == Guid.Empty && aInstructionWithDestination.DestinationIsIndirect)) {
                                // todo: fix for 16bit mode, it should then be 0x36
                                //xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 0x5;
                                //ulong xAddress = 0;
                                //if (!(aInstructionWithDestination.DestinationRef != null && aInstructionWithDestination.DestinationRef.Resolve(aAssembler, out xAddress))) {
                                //    if (aInstructionWithDestination.DestinationValue.HasValue) {
                                //        xAddress = aInstructionWithDestination.DestinationValue.Value;
                                //    }
                                //}
                                //xAddress += (ulong)aInstructionWithDestination.DestinationDisplacement;
                                //Array.Copy(BitConverter.GetBytes((uint)xAddress), 0, xBuffer, aEncodingOption.OpCode.Length + xExtraOffset + 1, 4);
                                //xExtraOffset += 4;
                                // todo: fix for 16bit mode, it should then be 0x36
                                xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 0x5;

                                ulong xAddress = 0;
                                if (!(aInstructionWithDestination.DestinationRef != null && aInstructionWithDestination.DestinationRef.Resolve(aAssembler, out xAddress))) {
                                    if (aInstructionWithDestination.DestinationValue.HasValue) {
                                        xAddress = aInstructionWithDestination.DestinationValue.Value;
                                    }
                                }
                                xAddress += (ulong)aInstructionWithDestination.DestinationDisplacement;
                                Array.Copy(BitConverter.GetBytes((uint)xAddress), 0, xBuffer, aEncodingOption.OpCode.Length + xExtraOffset + 1, 4);
                                xExtraOffset += 4;
                            }
                        }
                    }

                    if (aInstructionWithDestination != null && aInstructionWithDestination.DestinationReg == Registers.EBP && aInstructionWithDestination.DestinationIsIndirect && aInstructionWithDestination.DestinationDisplacement == 0) {
                        xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 0x40;
                        xExtraOffset++;
                    }
                        //}
                        if (aInstructionWithSource != null && aInstructionWithSource.SourceReg != Guid.Empty && aEncodingOption.SourceRegByte != null) {
                            xBuffer[aEncodingOption.OpCode.Length + xExtraOffset - 1 + aEncodingOption.SourceRegByte.Value] |= (byte)(EncodeRegister(aInstructionWithSource.SourceReg) << aEncodingOption.SourceRegBitShiftLeft);
                        }
                        //SBArray.Resize(ref xBuffer, xBuffer.Length + 1);
                        if (aInstructionWithDestination.DestinationReg != Guid.Empty && aInstructionWithDestination.DestinationIsIndirect && aInstructionWithDestination.DestinationDisplacement > 0) {
                            var xSIBOffset = 0;
                            if (xSIB != null) {
                                //xExtraOffset++;
                                xSIBOffset = 1;
                                xBuffer[aEncodingOption.OpCode.Length + xExtraOffset + xSIBOffset] = xSIB.Value;
                            }
                            //xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 3 << 6;
                            // todo: optimize for different displacement sizes
                            if (aInstructionWithDestination.DestinationDisplacement < 128) {
                                xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 2 << 5; // for now use 8bit value
                                Array.Copy(BitConverter.GetBytes((byte)aInstructionWithDestination.DestinationDisplacement), 0, xBuffer, aEncodingOption.OpCode.Length + xExtraOffset + xSIBOffset + 1, 1);
                                xExtraOffset += 1;
                            } else {
                                xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] |= 2 << 6; // for now use 8bit value
                                xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] &= 0xBF; // clear the 1 << 6
                                Array.Copy(BitConverter.GetBytes(aInstructionWithDestination.DestinationDisplacement), 0, xBuffer, aEncodingOption.OpCode.Length + xExtraOffset + xSIBOffset + 1, 4);
                                xExtraOffset += 4;
                            }
                            //}
                            if (xSIB != null) {
                                xExtraOffset++;
                            }
                        } else {
                            if (xSIB != null) {
                                xExtraOffset++;
                                xBuffer[aEncodingOption.OpCode.Length + xExtraOffset] = xSIB.Value;
                            }

                        }
                }
                //EncodeModRMByte(aInstruction.DestinationReg, aInstruction.DestinationIsIndirect, aInstruction.DestinationDisplacement > 0, aInstruction.DestinationDisplacement > 255, out xSIB);

            }
            if (aInstructionWithDestination != null) {
                if (aInstructionWithDestination.DestinationValue.HasValue && !aInstructionWithDestination.DestinationIsIndirect) {
                    int xOffset = aEncodingOption.OpCode.Length + xExtraOffset;
                    if (aEncodingOption.NeedsModRMByte) {
                        xOffset++;
                    }
                    var xInstrSize = 0;
                    var xValue = aInstructionWithDestination.DestinationValue.Value;
                    if (aInstructionWithSize != null) {
                        xInstrSize = aInstructionWithSize.Size / 8;
                    } else {
                        //                        throw new NotImplementedException("size not known");
                        xInstrSize = (int)aEncodingOption.DefaultSize / 8;
                    }
                    Array.Copy(BitConverter.GetBytes(xValue), 0, xBuffer, xOffset, xInstrSize);
                }
            }
            // todo: add more options
            if (aInstructionWithSource != null) {
                if (aInstructionWithSource.SourceValue.HasValue) {
                    int xOffset = aEncodingOption.OpCode.Length + xExtraOffset;
                    if (aEncodingOption.NeedsModRMByte) {
                        xOffset++;
                    }
                    var xInstrSize = 0;
                    if (aInstructionWithSize != null) {
                        xInstrSize = aInstructionWithSize.Size / 8;
                    } else {
                        throw new NotImplementedException("size not known");
                    }
                    Array.Copy(BitConverter.GetBytes(aInstructionWithSource.SourceValue.Value), 0, xBuffer, xOffset, xInstrSize);
                }
            }
            if (aInstructionWithSize != null) {
                if (aEncodingOption.OperandSizeByte.HasValue) 
                {
                    if(aInstructionWithSize.Size != 8) {
                        xBuffer[aEncodingOption.OperandSizeByte.Value + xOpCodeOffset] |= (byte)(1 << aEncodingOption.OperandSizeBitShiftLeft);
                    }
                }
            }

            if(aEncodingOption.ModifyBytes != null) {
                aEncodingOption.ModifyBytes(xBuffer, aInstruction);
            }
            //
            return xBuffer;
        }

        private static byte EncodeRegister(Guid aRegister) {
            // todo: implement support for other registers
            if (aRegister == Registers.EAX) return 0x0;
            if (aRegister == Registers.ECX) return 0x1;
            if (aRegister == Registers.EDX) return 0x2;
            if (aRegister == Registers.EBX) return 0x3;
            if (aRegister == Registers.ESP) return 0x4;
            if (aRegister == Registers.EBP) return 0x5;
            if (aRegister == Registers.ESI) return 0x6;
            if (aRegister == Registers.EDI) return 0x7;

            if (aRegister == Registers.AX) return 0x0;
            if (aRegister == Registers.CX) return 0x1;
            if (aRegister == Registers.DX) return 0x2;
            if (aRegister == Registers.BX) return 0x3;
            if (aRegister == Registers.SP) return 0x4;
            if (aRegister == Registers.BP) return 0x5;
            if (aRegister == Registers.SI) return 0x6;
            if (aRegister == Registers.DI) return 0x7;

            if (aRegister == Registers.AL) return 0x0;
            if (aRegister == Registers.CL) return 0x1;
            if (aRegister == Registers.DL) return 0x2;
            if (aRegister == Registers.BL) return 0x3;
            if (aRegister == Registers.AH) return 0x4;
            if (aRegister == Registers.CH) return 0x5;
            if (aRegister == Registers.DH) return 0x6;
            if (aRegister == Registers.BH) return 0x7;
            throw new Exception("Register not supported!");
        }


        /*
        private static bool SizeIsSelected(InstructionSizes aSizes, byte aSize) {
            switch(aSize) {
                case 8: return (aSizes & InstructionSizes.Byte) != 0;
                case 16: return (aSizes & InstructionSizes.Byte) != 0;
                case 32: return (aSizes & InstructionSizes.Byte) != 0;
                default: throw new NotImplementedException();
            }
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
            if(aInstruction.ToString() == "mov word [ESP], 0x47") {
             Console.Write("");   
            }
            ulong xSize = 0;
            Instruction.DetermineSize(aInstruction, aInstructionData, out xSize);
            if(xSize==0) {
                return new byte[0];
            }
            var xBuffer = new byte[xSize];
            int xExtraOffset = 0;
            int xOpCodeOffset = 0;
            if (xEncodingOption.DefaultSize == InstructionSize.DWord && aInstruction.Size == 16) {
                xOpCodeOffset += 1;
                xExtraOffset++;
                xBuffer[0] = 0x66;
            }
            if (xEncodingOption.DefaultSize == InstructionSize.Word && aInstruction.Size == 32) {
                xOpCodeOffset += 1;
                xExtraOffset++;
                xBuffer[0] = 0x66;
            }
            Array.Copy(xEncodingOption.OpCode, 0, xBuffer, xExtraOffset, xEncodingOption.OpCode.Length);
            byte? xSIB = null;
            if (xEncodingOption.NeedsModRMByte) {
                xBuffer[xEncodingOption.OpCode.Length + xExtraOffset] = EncodeModRMByte(aInstruction.DestinationReg, aInstruction.DestinationIsIndirect, aInstruction.DestinationDisplacement > 0, aInstruction.DestinationDisplacement > 255, out xSIB);
                //byte 
                // = EncodeModRMByte()
                //if(aInstruction.DestinationReg != Guid.Empty && !aInstruction.DestinationIsIndirect) {
                //    xBuffer[xEncodingOption.OpCode.Length] |= 0xC0;    
                //}
                //if(aInstruction.DestinationReg != Guid.Empty && aInstruction.DestinationIsIndirect) {
                //    //
                //}
                // todo: add more ModRM stuff
            }
            if (aInstruction.DestinationReg != Guid.Empty && xEncodingOption.DestinationRegByte.HasValue && !xEncodingOption.NeedsModRMByte) {
                xBuffer[xEncodingOption.DestinationRegByte.Value + xExtraOffset] |= (byte)(EncodeRegister(aInstruction.DestinationReg) << xEncodingOption.DestinationRegBitShiftLeft);
            }
            if (aInstruction.SourceReg != Guid.Empty && xEncodingOption.SourceRegByte.HasValue) {
                xBuffer[xEncodingOption.SourceRegByte.Value + xExtraOffset] |= (byte)(EncodeRegister(aInstruction.SourceReg) << xEncodingOption.SourceRegBitShiftLeft);
            }
            if(xSIB!=null) {
                xBuffer[xEncodingOption.OpCode.Length + xExtraOffset + 1] = xSIB.Value;
                xExtraOffset++;
            }
            // todo: add more options
            if (aInstruction.SourceValue.HasValue) {
                int xOffset = xEncodingOption.OpCode.Length + xExtraOffset;
                if(xEncodingOption.NeedsModRMByte) {
                    xOffset++;
                }
                Array.Copy(BitConverter.GetBytes(aInstruction.SourceValue.Value), 0, xBuffer, xOffset, aInstruction.Size / 8);
            }
            if (xEncodingOption.OperandSizeByte.HasValue) {
                if (aInstruction.Size > 8) {
                    xBuffer[xEncodingOption.OperandSizeByte.Value + xOpCodeOffset] |= (byte)(1 << xEncodingOption.OperandSizeBitShiftLeft);
                }
            }


            //
            return xBuffer;
        }

        private static byte EncodeModRMByte(Guid aRegister, bool aIndirect, bool aOffset, bool aOffsetIs32Bit, out byte? aSIB) {
            byte xModRM = 0;
            xModRM |= EncodeRegister(aRegister);
            aSIB = null;
            if(!aOffset) {
                if (aRegister == Registers.EBP) {
                    xModRM |= 1 << 6;
                    aSIB = 0;
                }
                if(aRegister == Registers.ESP) {
                    aSIB = 0x24;
                }
            }
            if(aOffset) {
                throw new NotImplementedException("Add support for offsets");
            }
            return xModRM;
        }

        

        private static bool IsLargeRegister(Guid aRegister) {
            return Registers.Is32Bit(aRegister);
        }
         */
    }
}
