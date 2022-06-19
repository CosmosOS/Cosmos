using ACPIAML.Interupter;
using ACPILib.AML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ACPILib.Parser2
{
    public class Parser
    {
        private Stream _source;

        public Parser(Stream s)
        {
            _source = s;
        }

        public ParseNode Parse()
        {
            return PreParse();
        }

        private ParseNode PreParse()
        {
            ParseNode root = new ParseNode()
            {
                Name = "\\"
            };

            while (_source.Position < _source.Length)
            {
                ParseNode op = ParseFullOpCodeNode();
                root.Nodes.Add(op);
            }

            return root;
        }

        private ParseNode ParseFullOpCodeNode()
        {
            //Read the opcode
            ParseNode op = ReadOpCode();
            OpCode info = op.Op;

            _source.Seek(op.DataStart, SeekOrigin.Begin);
            long methodBodyAddr = 0;

            //Parse opcode arguments
            if (info.ParseArgs.Length > 0)
            {
                bool parseArguments = false;

                switch (info.Code)
                {
                    case OpCodeEnum.Byte:
                    case OpCodeEnum.Word:
                    case OpCodeEnum.DWord:
                    case OpCodeEnum.QWord:
                    case OpCodeEnum.String:
                        op.ConstantValue = ParseSimpleArgument(info.ParseArgs[0]);
                        break;

                    case OpCodeEnum.NamePath:
                        op.Arguments.Add(StackObject.Create(ReadNameString()));
                        break;

                    default:
                        parseArguments = true;
                        break;
                }

                if (parseArguments) //If the opcode is not a constant
                {
                    for (int x = 0; x < info.ParseArgs.Length; x++)
                    {
                        switch (info.ParseArgs[x])
                        {
                            case ParseArgFlags.None:
                                break;

                            case ParseArgFlags.ByteData:
                            case ParseArgFlags.WordData:
                            case ParseArgFlags.DWordData:
                            case ParseArgFlags.CharList:
                            case ParseArgFlags.Name:
                            case ParseArgFlags.NameString:
                                {
                                    var arg = ParseSimpleArgument(info.ParseArgs[x]);
                                    if (arg != null)
                                    {
                                        op.Arguments.Add(arg);
                                    }
                                }
                                break;
                            case ParseArgFlags.DataObject:
                            case ParseArgFlags.TermArg:
                                {
                                    //HACK: todo make this properly
                                    methodBodyAddr = _source.Position;
                                    var arg = ParseFullOpCodeNode(); //parsenode

                                    op.Arguments.Add(StackObject.Create(arg));
                                }
                                break;

                            case ParseArgFlags.PackageLength:
                                var xx = op.Length = ReadPackageLength();
                                op.Arguments.Add(StackObject.Create(xx));
                                break;

                            case ParseArgFlags.FieldList:
                                while (_source.Position < op.End)
                                {
                                    op.Arguments.Add(StackObject.Create(ReadField()));
                                }
                                break;

                            case ParseArgFlags.ByteList:
                                if (_source.Position < op.End)
                                {
                                    op.ConstantValue = StackObject.Create(ReadBytes((int)(op.End - _source.Position)));
                                }
                                break;

                            case ParseArgFlags.DataObjectList:
                            case ParseArgFlags.TermList:
                            case ParseArgFlags.ObjectList:
                                var startPosition = _source.Position;
                                if (op.Arguments[1].Type == StackObjectType.String)
                                {
                                    if ((string)op.Arguments[1].Value == "CPUS")
                                    {
                                        ;
                                    }
                                }
                                while (_source.Position < op.End)
                                {
                                    ParseNode child = ParseFullOpCodeNode();

                                    op.Nodes.Add(child);
                                }

                                break;

                            case ParseArgFlags.Target:
                            case ParseArgFlags.SuperName:
                                ushort subOp2 = PeekUShort();
                                if (subOp2 == 0 || Definitions.IsNameRootPrefixOrParentPrefix((byte)subOp2) || Definitions.IsLeadingChar((byte)subOp2))
                                {
                                    //AMLOp namePath = new AMLOp(OpCodeTable.GetOpcode((ushort)OpCodeEnum.NamePath), op);
                                    // var xxx = ParseFullOpCodeNode();
                                    //xxx.Name =;
                                    var str = ReadNameString();
                                    op.Arguments.Add(StackObject.Create(str));


                                }
                                else
                                {
                                    _source.Seek(op.DataStart, SeekOrigin.Begin);
                                    var xxx = ParseFullOpCodeNode();
                                    op.Nodes.Add(xxx);
                                    ;
                                }
                                break;

                            default:
                                Console.WriteLine("psargs.c / line 913 - Unknown arg: " + info.ParseArgs[x]);
                                break;
                        }
                    }
                }
            }

            //Parse the opcode
            if ((info.Flags & OpCodeFlags.Named) == OpCodeFlags.Named)
            {
                for (int x = 0; x < info.ParseArgs.Length; x++)
                {
                    if (info.ParseArgs[x] == ParseArgFlags.Name)
                    {
                        op.Name = (string)op.Arguments[x].Value;
                        break;
                    }
                }
            }

            if (op.Op.Name == "Scope")
            {
                var orgPosition = op.DataStart;
                while (_source.Position < orgPosition + op.Length)
                {
                    ParseNode op2 = ParseFullOpCodeNode();
                    op.Nodes.Add(op2);
                }
            }
            //else if (op.Op.Name == "Method")
            //{
            //    //We add one because we expect a DualNamePrefix (0x2E)
            //    _source.Seek(methodBodyAddr, SeekOrigin.Begin);

            //    ////Read until function name string ends
            //    //while(_source.ReadByte() != 0)
            //    //{

            //    //}

            //    var codeEnd = op.DataStart + op.Length;

            //    while (_source.Position < codeEnd)
            //    {
            //        ParseNode op2 = ParseFullOpCodeNode();
            //        op.Nodes.Add(op2);
            //    }
            //}

            return op;
        }

        private ParseNode ReadField()
        {
            OpCodeEnum opCode;
            switch ((OpCodeEnum)PeekByte())
            {
                case OpCodeEnum.FieldOffset:

                    opCode = OpCodeEnum.ReservedField; ;
                    _source.Seek(1, SeekOrigin.Current);
                    break;

                case OpCodeEnum.FieldAccess:

                    opCode = OpCodeEnum.AccessField;
                    _source.Seek(1, SeekOrigin.Current);
                    break;

                case OpCodeEnum.FieldConnection:

                    opCode = OpCodeEnum.Connection;
                    _source.Seek(1, SeekOrigin.Current);
                    break;

                case OpCodeEnum.FieldExternalAccess:

                    opCode = OpCodeEnum.ExternalAccessField;
                    _source.Seek(1, SeekOrigin.Current);
                    break;

                default:
                    opCode = OpCodeEnum.NamedField;
                    break;
            }

            ParseNode node = new ParseNode()
            {
                Op = OpCodeTable.GetOpcode((ushort)opCode)
            };

            switch (opCode)
            {
                case OpCodeEnum.NamedField:
                    node.Name = Read4ByteName();
                    node.ConstantValue = StackObject.Create(ReadPackageLength());
                    break;
                case OpCodeEnum.ReservedField:
                    node.ConstantValue = StackObject.Create(ReadPackageLength());
                    break;
                case OpCodeEnum.AccessField:
                    node.ConstantValue = StackObject.Create((ReadByte() | ((uint)ReadByte() << 8)));
                    break;
                case OpCodeEnum.ExternalAccessField:
                    node.ConstantValue = StackObject.Create((ReadByte() | ((uint)ReadByte() << 8) | ((uint)ReadByte() << 16)));
                    break;

                default:
                    throw new Exception("psargs.c / line 703");
            }

            return node;
        }

        private int ReadPackageLength()
        {
            int length;

            byte b0 = (byte)ReadByte();

            byte sz = (byte)((b0 >> 6) & 3);

            if (sz == 0)
            {
                //out = (size_t)(code[*pc] & 0x3F);
                //(*pc)++;
                //return 0;

                length = b0 & 0x3F;
            }
            else if (sz == 1)
            {
                // *out = (size_t)(code[*pc] & 0x0F) | (size_t)(code[*pc + 1] << 4);

                length = ((b0 & 0x0F) | ReadByte() << 4);
            }
            else if (sz == 2)
            {
                length = ((b0 & 0x0F) | ReadByte() << 4) | (ReadByte() << 12);
            }
            else if (sz == 3)
            {
                length = ((b0 & 0x0F) | ReadByte() << 4) | (ReadByte() << 12) | (ReadByte() << 20);
            }
            else
            {
                throw new NotImplementedException();
            }

            return length;
        }

        private StackObject ParseSimpleArgument(ParseArgFlags arg)
        {
            switch (arg)
            {
                case ParseArgFlags.ByteData:
                    return StackObject.Create((byte)ReadByte());
                case ParseArgFlags.WordData:
                    return StackObject.Create(BitConverter.ToInt16(ReadBytes(2), 0));
                case ParseArgFlags.DWordData:
                    return StackObject.Create(BitConverter.ToInt32(ReadBytes(4), 0));
                case ParseArgFlags.QWordData:
                    return StackObject.Create(BitConverter.ToInt64(ReadBytes(8), 0));
                case ParseArgFlags.CharList: //Nullterminated string
                    string str = string.Empty;

                    byte read;
                    while ((read = (byte)ReadByte()) != 0)
                        str += (char)read;

                    return StackObject.Create(str);
                case ParseArgFlags.Name:
                case ParseArgFlags.NameString:
                    return StackObject.Create(ReadNameString());
            }

            return null;
        }
        private string ReadNameString()
        {
            var x = _source.Position;
            var b = (char)ReadByte();
            bool is_absolute = false;
            if (b == '\\')
            {
                is_absolute = true;
            }
            else
            {
                bool xx = false;
                while ((char)PeekByte() == '^')
                {
                    xx = true;
                    _source.Position++;
                }
                if (xx)
                {
                    b = (char)PeekByte();
                }
            }

            int segmentNumber = 0;
            bool UseBChar = false;
            //var b2 = (char)ReadByte();
            string o = "";
            if (b == '\0')
            {
                segmentNumber = 0;
            }
            else if (b == 0x2E)
            {
                //dual prefix
                segmentNumber = 2;
            }
            else if (b == 0x2F)
            {
                //dual prefix
                segmentNumber = ReadByte();
            }
            else
            {
                segmentNumber = 1; //default?
                o += b.ToString();
                UseBChar = true;
            }
            var len = segmentNumber * 4;
            if (UseBChar)
                len -= 1;
            for (int i = 0; i < len; i++)
            {
                o += ((char)ReadByte()).ToString();
            }

            foreach (var item in o)
            {
                if (!char.IsAscii(item))
                    throw new Exception("Check failed: Char is not ASCII");
            }
            return o;
            ////Read past prefix chars
            //while (Definitions.IsNameRootPrefixOrParentPrefix(PeekByte()))
            //{
            //    _source.Seek(1, SeekOrigin.Current);
            //}

            //int segments = 0;
            //var b = ReadByte();
            //switch (b)
            //{
            //    case 0: //Null string
            //        return string.Empty;

            //    case Definitions.DualNamePrefix:
            //        segments = 2;
            //        break;
            //    case Definitions.MultiNamePrefix:
            //        segments = ReadByte();
            //        break;

            //    default:
            //        segments = 1;

            //        _source.Seek(-1, SeekOrigin.Current);
            //        break;
            //}

            //string name = string.Empty;

            //for (int seg = 0; seg < segments; seg++)
            //{
            //    string nameSeg = Read4ByteName();

            //    name += nameSeg;

            //    if (seg < segments - 1)
            //        name += ".";
            //}

            //return name;
        }

        private ParseNode ReadOpCode()
        {
            long pos = _source.Position;

            ushort op = ReadUShort();
            OpCode? info = OpCodeTable.GetOpcode(op);
            switch (info.Class)
            {
                case OpCodeClass.ASCII:
                case OpCodeClass.Prefix:
                    info = OpCodeTable.GetOpcode((ushort)OpCodeEnum.NamePath);
                    pos -= 1; //The op code byte is the data itself
                    break;
                case OpCodeClass.ClassUnknown:
                    Console.WriteLine("Unknown AML opcode: 0x" + op.ToString("X") + " at " + _source.Position);
                    break;
                default:
                    _source.Seek(info.CodeByteSize, SeekOrigin.Current);
                    break;
            }

            return new ParseNode()
            {
                Op = info,
                Start = pos,
                DataStart = pos + info.CodeByteSize
            };
        }

        private string Read4ByteName()
        {
            byte[] dt = new byte[4];
            _source.Read(dt, 0, 4);

            return Encoding.ASCII.GetString(dt);
        }

        private byte[] ReadBytes(int num)
        {
            byte[] temp = new byte[num];
            _source.Read(temp, 0, num);

            return temp;
        }

        private byte PeekByte()
        {
            byte read = (byte)_source.ReadByte();

            _source.Seek(-1, SeekOrigin.Current);

            return read;
        }

        private ushort PeekUShort()
        {
            ushort code = (ushort)_source.ReadByte();
            if (code == Definitions.ExtendedOpCodePrefix)
            {
                code = (ushort)((code << 8) | (ushort)_source.ReadByte());

                _source.Seek(-2, SeekOrigin.Current);
            }
            else
            {
                _source.Seek(-1, SeekOrigin.Current);
            }

            return code;
        }

        private byte ReadByte()
        {
            return (byte)_source.ReadByte();
        }

        private ushort ReadUShort()
        {
            ushort code = (ushort)_source.ReadByte();
            if (code == Definitions.ExtendedOpCodePrefix)
            {
                code = (ushort)((code << 8) | (ushort)_source.ReadByte());
            }

            return code;
        }
    }
}
