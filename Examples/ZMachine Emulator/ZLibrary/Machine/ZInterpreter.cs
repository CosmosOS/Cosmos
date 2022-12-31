using Cosmos.Common.Extensions;
using ZLibrary.Machine.Opcodes;

namespace ZLibrary.Machine
{
    public class ZInterpreter
    {
        public int Finished;

        private Opcode[] OP0Opcodes;

        private Opcode[] OP1Opcodes;

        private Opcode[] OP2Opcodes;

        private Opcode[] VAROpcodes;

        private Opcode[] EXTOpcodes;

        private int _invokeCount;
        private ushort _argCount;

        private readonly ZMachine _machine;

        public ZInterpreter(ZMachine aMachine)
        {
            _machine = aMachine;
            Finished = 0;
            InitializeOpcodes();
        }

        private void InitializeOpcodes()
        {
            InitializeOP0Opcodes();
            InitializeOP1Opcodes();
            InitializeOP2Opcodes();
            InitializeVAROpcodes();
            InitializeEXTOpcodes();
        }

        private void InitializeOP0Opcodes()
        {
           OP0Opcodes = new Opcode[0x10]
            {
                new Opcodes._0OP.RTRUE(_machine), // 0x00
                new Opcodes._0OP.RFALSE(_machine), // 0x01
                new Opcodes._0OP.PRINT(_machine), // 0x02
                new Opcodes._0OP.PRINT_RET(_machine), // 0x03
                new Opcodes._0OP.NOP(_machine), // 0x04
                new Opcodes._0OP.SAVE(_machine), // 0x05
                new Opcodes._0OP.RESTORE(_machine), // 0x06
                new Opcodes._0OP.RESTART(_machine), // 0x07
                new Opcodes._0OP.RET_POPPED(_machine), // 0x08
                new Opcodes._0OP.POP(_machine), // 0x09
                new Opcodes._0OP.QUIT(_machine), // 0x0A
                new Opcodes._0OP.NEW_LINE(_machine), // 0x0B
                new Opcodes._0OP.SHOW_STATUS(_machine), // 0x0C
                new Opcodes._0OP.VERIFY(_machine), // 0x0D
                new Opcodes._0OP.EXTENDED(_machine), // 0x0E
                new Opcodes._0OP.PIRACY(_machine) // 0x0F
            };
        }

        private void InitializeOP1Opcodes()
        {
            OP1Opcodes = new Opcode[0x10]
            {
                new Opcodes._1OP.JZ(_machine), // 0x00
                new Opcodes._1OP.GET_SIBLING(_machine), // 0x01
                new Opcodes._1OP.GET_CHILD(_machine), // 0x02
                new Opcodes._1OP.GET_PARENT(_machine), //0x03
                new Opcodes._1OP.GET_PROP_LEN(_machine), // 0x04
                new Opcodes._1OP.INC(_machine), // 0x05
                new Opcodes._1OP.DEC(_machine), // 0x06
                new Opcodes._1OP.PRINT_ADDR(_machine), // 0x07
                new Opcodes._1OP.CALL_1S(_machine), // 0x08
                new Opcodes._1OP.REMOVE_OBJ(_machine), // 0x09
                new Opcodes._1OP.PRINT_OBJ(_machine), // 0x0A
                new Opcodes._1OP.RET(_machine), // 0x0B
                new Opcodes._1OP.JUMP(_machine), // 0x0C
                new Opcodes._1OP.PRINT_PADDR(_machine), // 0x0D 
                new Opcodes._1OP.LOAD(_machine), // 0x0E
                new Opcodes._1OP.NOT(_machine) // 0x0F
            };
        }

        private void InitializeOP2Opcodes()
        {
            OP2Opcodes = new Opcode[0x20]
            {
                new Opcodes._2OP.ILLEGAL(_machine), // 0x00
                new Opcodes.VAR.JE(_machine), // 0x01
                new Opcodes.VAR.JL(_machine), // 0x02
                new Opcodes.VAR.JG(_machine), // 0x03
                new Opcodes._2OP.DEC_CHK(_machine), // 0x04
                new Opcodes._2OP.INC_CHK(_machine), // 0x05
                new Opcodes._2OP.JIN(_machine), // 0x06
                new Opcodes._2OP.TEST(_machine), // 0x07
                new Opcodes._2OP.OR(_machine), // 0x08
                new Opcodes._2OP.AND(_machine), // 0x09
                new Opcodes._2OP.TEST_ATTR(_machine), // 0x0A
                new Opcodes._2OP.SET_ATTR(_machine), // 0x0B
                new Opcodes._2OP.CLEAR_ATTR(_machine), // 0x0C
                new Opcodes._2OP.STORE(_machine), // 0x0D
                new Opcodes._2OP.INSERT_OBJ(_machine), // 0x0E
                new Opcodes._2OP.LOADW(_machine), // 0x0F
                new Opcodes._2OP.LOADB(_machine), // 0x10
                new Opcodes._2OP.GET_PROP(_machine), // 0x11
                new Opcodes._2OP.GET_PROP_ADDR(_machine), // 0x12
                new Opcodes._2OP.GET_NEXT_PROP(_machine), // 0x13
                new Opcodes._2OP.ADD(_machine), // 0x14
                new Opcodes._2OP.SUB(_machine), // 0x15
                new Opcodes._2OP.MUL(_machine), // 0x16
                new Opcodes._2OP.DIV(_machine), // 0x17
                new Opcodes._2OP.MOD(_machine), // 0x18
                new Opcodes._2OP.CALL_2S(_machine), // 0x19
                new Opcodes._2OP.CALL_2N(_machine), // 0x1A
                new Opcodes._2OP.SET_COLOR(_machine), // 0x1B
                new Opcodes._2OP.THROW(_machine), // 0x1C
                null, // 0x1F
                null, // 0x1E
                null // 0x1F
            };
        }

        private void InitializeVAROpcodes()
        {
            VAROpcodes = new Opcode[0x40]
        {
            new Opcodes.VAR.ILLEGAL(_machine),
            new Opcodes.VAR.JE(_machine),
            new Opcodes.VAR.JL(_machine),
            new Opcodes.VAR.JG(_machine),
            null,
            new Opcodes._2OP.INC_CHK(_machine),
            null,
            null,
            new Opcodes._2OP.OR(_machine),
            new Opcodes._2OP.AND(_machine),
            null,
            null,
            null,
            new Opcodes._2OP.STORE(_machine),
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            new Opcodes.VAR.CALL(_machine), // 0x20
            new Opcodes.VAR.STOREW(_machine), // 0x21
            new Opcodes.VAR.STOREB(_machine), // 0x22
            new Opcodes.VAR.PUT_PROP(_machine), // 0x23
            new Opcodes.VAR.SREAD(_machine), // 0x24
            new Opcodes.VAR.PRINT_CHAR(_machine), // 0x25
            new Opcodes.VAR.PRINT_NUM(_machine), // 0x26
            new Opcodes.VAR.RANDOM(_machine), // 0x27
            new Opcodes.VAR.PUSH(_machine), // 0x28
            new Opcodes.VAR.PULL(_machine), // 0x29
            new Opcodes.VAR.SPLIT_WINDOW(_machine), // 0x2A
            new Opcodes.VAR.SET_WINDOW(_machine), // 0x2B
            new Opcodes.VAR.CALL_VS2(_machine), // 0x2C
            new Opcodes.VAR.ERASE_WINDOW(_machine), // 0x2D
            new Opcodes.VAR.ERASE_LINE(_machine), // 0x2E
            new Opcodes.VAR.SET_CURSOR(_machine), // 0x2F
            new Opcodes.VAR.GET_CURSOR(_machine), // 0x30
            new Opcodes.VAR.SET_TEXT_STYLE(_machine), // 0x31
            new Opcodes.VAR.BUFFER_MODE(_machine), // 0x32
            new Opcodes.VAR.OUTPUT_STREAM(_machine), // 0x33
            new Opcodes.VAR.INPUT_STREAM(_machine), // 0x34
            null, // 0x35
            new Opcodes.VAR.READ_CHAR(_machine), // 0x36
            new Opcodes.VAR.SCAN_TABLE(_machine), // 0x37
            new Opcodes.VAR.NOT(_machine), // 0x38
            new Opcodes.VAR.CALL_VN(_machine), // 0x39
            new Opcodes.VAR.CALL_VN2(_machine), // 0x3A
            new Opcodes.VAR.TOKENISE(_machine), // 0x3B
            new Opcodes.VAR.ENCODE_TEXT(_machine), // 0x3C
            new Opcodes.VAR.COPY_TABLE(_machine), // 0x3D
            new Opcodes.VAR.PRINT_TABLE(_machine), // 0x3E
            new Opcodes.VAR.CHECK_ARG_COUNT(_machine), // 0x3F
        };
        }

        private void InitializeEXTOpcodes()
        {
            EXTOpcodes = new Opcode[0x1e];
        }

        public void Interpret()
        {
            do
            {
                _machine.Memory.CodeByte(out byte opcode);
                long pc = _machine.Memory.PC;
                _argCount = 0;

                ZDebug.Output($"CODE: {pc - 1} -> {opcode.ToHex(2)}");

                ushort xArg0, xArg1, xArg2, xArg3, xArg4 = 0, xArg5 = 0, xArg6 = 0 , xArg7 = 0;
                if (opcode < 0x80) // 2OP Opcodes
                {
                    LoadOperand((byte)((opcode & 0x40) > 0 ? 2 : 1), out xArg0);
                    LoadOperand((byte)((opcode & 0x20) > 0 ? 2 : 1), out xArg1);

                    int op = opcode & 0x1f;
                    InvokeOpcode_2OP(OP2Opcodes[op], opcode, xArg0, xArg1);
                }
                else if (opcode < 0xb0) // 1OP opcodes
                {
                    LoadOperand((byte)(opcode >> 4), out xArg0);

                    int op = opcode & 0x0f;
                    InvokeOpcode_1OP(OP1Opcodes[op], opcode, xArg0);
                }
                else if (opcode < 0xc0) // 0OP opcodes
                {
                    int op = opcode - 0xb0;
                    InvokeOpcode_0OP(OP0Opcodes[op], opcode);
                }
                else // VAR opcodes
                {
                    _machine.Memory.CodeByte(out byte xSpecifier1);
                    int xArgsCount = LoadAllOperands(xSpecifier1, out xArg0, out xArg1, out xArg2, out xArg3);

                    // Call opcodes with up to 8 arguments
                    if (opcode == 0xec || opcode == 0xfa)
                    {
                        _machine.Memory.CodeByte(out byte xSpecifier2);
                        xArgsCount += LoadAllOperands(xSpecifier2, out xArg4, out xArg5, out xArg6, out xArg7);
                    }

                    int op = opcode - 0xc0;
                    InvokeOpcode_VAROP(VAROpcodes[op], opcode, xArg0, xArg1, xArg2, xArg3, xArg4, xArg5, xArg6, xArg7, (ushort)xArgsCount);
                }

                _machine.Tick();
            } while (Finished == 0);
        }

        private void InvokeOpcode_0OP(Opcode aInstruction, int aOpcode)
        {
            ZDebug.Output($"Invoking 0OP: {aOpcode.ToHex(2)} -> {aInstruction} -> {_invokeCount}");
            aInstruction.Execute();
            _invokeCount++;
        }

        private void InvokeOpcode_1OP(Opcode aInstruction, int aOpcode, ushort aArg)
        {
            ZDebug.Output($"Invoking 1OP: {aOpcode.ToHex(2)} -> {aInstruction} -> {_invokeCount}");
            aInstruction.Execute(aArg);
            _invokeCount++;
        }

        private void InvokeOpcode_2OP(Opcode aInstruction, int aOpcode, ushort aArg0 ,ushort aArg1)
        {
            ZDebug.Output($"Invoking 2OP: {aOpcode.ToHex(2)} -> {aInstruction} -> {_invokeCount}");
            aInstruction.Execute(aArg0, aArg1);
            _invokeCount++;
        }

        private void InvokeOpcode_VAROP(Opcode aInstruction, int aOpcode, ushort aArg0, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            ZDebug.Output($"Invoking: {aOpcode.ToHex(2)} -> {aInstruction}:{aArgCount} -> {_invokeCount}");

            aInstruction.Execute(aArg0, aArg1, aArg2, aArg3, aArg4, aArg5, aArg6, aArg7, aArgCount);
            _invokeCount++;
        }

        /// <summary>
        /// Load an operand, either a variable or a constant.
        /// </summary>
        /// <param name="aType"></param>
        /// <param name="aArg"></param>
        private void LoadOperand(byte aType, out ushort aArg)
        {
            if ((aType & 0x02) > 0) // Variable
            {
                _machine.Memory.CodeByte(out byte xVariable);

                if (xVariable == 0)
                {
                    aArg = _machine.Memory.Stack.Pop();
                }
                else if (xVariable < 16)
                {
                    aArg = _machine.Memory.Stack[_machine.Memory.Stack.BP - xVariable];
                }
                else
                {
                    ushort xAddress = (ushort) (_machine.Story.Header.GlobalsOffset + 2 * (xVariable - 16));
                    _machine.Memory.GetWord(xAddress, out aArg);
                }
            }
            else if ((aType & 1) > 0) // Small Constant
            {
                _machine.Memory.CodeByte(out byte xValue);
                aArg = xValue;
            }
            else // Large Constant
            {
                _machine.Memory.CodeWord(out aArg);
            }

            _argCount++;
            ZDebug.Output($"  Storing operand: {_argCount - 1} -> {aArg}");
        }

        /// <summary>
        /// Given the operand specifier byte, load all (up to four) operands for a VAR or EXT opcode.
        /// </summary>
        /// <param name="aSpecifier"></param>
        /// <param name="aArg0"></param>
        /// <param name="aArg1"></param>
        /// <param name="aArg2"></param>
        /// <param name="aArg3"></param>
        private int LoadAllOperands(byte aSpecifier, out ushort aArg0, out ushort aArg1, out ushort aArg2, out ushort aArg3)
        {
            int xArgsCount = 0;
            aArg0 = 0;
            aArg1 = 0;
            aArg2 = 0;
            aArg3 = 0;

            byte xType = (byte)((aSpecifier >> 6) & 0x03);
            if (xType == 3)
            {
                return xArgsCount;
            }

            LoadOperand(xType, out aArg0);
            xArgsCount++;

            xType = (byte)((aSpecifier >> 4) & 0x03);
            if (xType == 3)
            {
                return xArgsCount;
            }

            LoadOperand(xType, out aArg1);
            xArgsCount++;

            xType = (byte)((aSpecifier >> 2) & 0x03);
            if (xType == 3)
            {
                return xArgsCount;
            }

            LoadOperand(xType, out aArg2);
            xArgsCount++;

            xType = (byte)((aSpecifier >> 0) & 0x03);
            if (xType == 3)
            {
                return xArgsCount;
            }

            LoadOperand(xType, out aArg3);
            xArgsCount++;

            return xArgsCount;
        }

        // TODO
        //private void Extended()
        //{
        //    _machine.Memory.CodeByte(out byte xOpcode);
        //    _machine.Memory.CodeByte(out byte xSpecifier);

        //    LoadAllOperands(xSpecifier);

        //    if (xOpcode < 0x1e)
        //    {
        //        Invoke(EXTOpcodes[xOpcode], xOpcode);
        //    }
        //}
    }
}
