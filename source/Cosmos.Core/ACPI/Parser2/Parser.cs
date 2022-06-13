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

			return null;
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
						op.Arguments.Add(ReadNameString());
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
									object arg = ParseSimpleArgument(info.ParseArgs[x]);
									if (arg != null)
									{
										op.Arguments.Add(arg);
									}
								}
								break;

							case ParseArgFlags.DataObject:
							case ParseArgFlags.TermArg:
								{
									ParseNode arg = ParseFullOpCodeNode();

									op.Arguments.Add(arg);
								}
								break;

							case ParseArgFlags.PackageLength:
								op.Arguments.Add(op.Length = ReadPackageLength());
								break;

							case ParseArgFlags.FieldList:
								while(_source.Position < op.End)
								{
									op.Arguments.Add(ReadField());
								}
								break;

							case ParseArgFlags.ByteList:
								if (_source.Position < op.End)
								{
									op.ConstantValue = ReadBytes((int)(op.End - _source.Position));
								}
								break;

							case ParseArgFlags.DataObjectList:
							case ParseArgFlags.TermList:
							case ParseArgFlags.ObjectList:

								if (op.Op.Code == OpCodeEnum.Method)
									_source.Seek(op.End, SeekOrigin.Begin);
								else
								{
									while (_source.Position < op.End)
									{
										ParseNode child = ParseFullOpCodeNode();

										op.Nodes.Add(child);
									}
								}

								break;

							default:
								throw new Exception("psargs.c / line 913 - Unknown arg: " + op.Op.ParseArgs[x].ToString());
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
						op.Name = (string)op.Arguments[x];
						break;
					}
				}
			}

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
					node.ConstantValue = ReadPackageLength();
					break;
				case OpCodeEnum.ReservedField:
					node.ConstantValue = ReadPackageLength();
					break;
				case OpCodeEnum.AccessField:
					node.ConstantValue = (ReadByte() | ((uint)ReadByte() << 8));
					break;
				case OpCodeEnum.ExternalAccessField:
					node.ConstantValue = (ReadByte() | ((uint)ReadByte() << 8) | ((uint)ReadByte() << 16));
					break;

				default:
					throw new Exception("psargs.c / line 703");
			}

			return node;
		}

		private int ReadPackageLength()
		{
			int length = 0;

			byte b0 = (byte)_source.ReadByte();

			int byteCount = (b0 >> 6);

			byte firstMask = (byte)(byteCount > 0 ? 0x0F : 0x3F);

			for (int b = 0; b < byteCount; b++)
			{
				length |= ((byte)_source.ReadByte() << ((byteCount << 3) - 4));
			}

			length |= (b0 & firstMask);

			return length;
		}

		private object ParseSimpleArgument(ParseArgFlags arg)
		{
			switch (arg)
			{
				case ParseArgFlags.ByteData:
					return (byte)_source.ReadByte();
				case ParseArgFlags.WordData:
					return BitConverter.ToInt16(ReadBytes(2), 0);
				case ParseArgFlags.DWordData:
					return BitConverter.ToInt32(ReadBytes(4), 0);
				case ParseArgFlags.QWordData:
					return BitConverter.ToInt64(ReadBytes(8), 0);
				case ParseArgFlags.CharList: //Nullterminated string
					string str = string.Empty;

					byte read;
					while ((read = (byte)_source.ReadByte()) != 0)
						str += (char)read;

					return str;
				case ParseArgFlags.Name:
				case ParseArgFlags.NameString:
					return ReadNameString();
			}

			return null;
		}

		private string ReadNameString()
		{
			//Read past prefix chars
			while (Definitions.IsNameRootPrefixOrParentPrefix(PeekByte()))
			{
				_source.Seek(1, SeekOrigin.Current);
			}

			int segments = 0;
			switch(ReadByte())
			{
				case 0: //Null string
					return string.Empty;

				case Definitions.DualNamePrefix:
					segments = 2;
					break;
				case Definitions.MultiNamePrefix:
					segments = ReadByte();
					break;

				default:
					segments = 1;

					_source.Seek(-1, SeekOrigin.Current);
					break;
			}

			string name = string.Empty;

			for (int seg = 0; seg < segments; seg++)
			{
				string nameSeg = Read4ByteName();

				name += nameSeg;

				if (seg < segments - 1)
					name += ".";
			}

			return name;
		}

		private ParseNode ReadOpCode()
		{
			long pos = _source.Position;

			ushort op = PeekOpcode();
			OpCode info = OpCodeTable.GetOpcode(op);

			switch (info.Class)
			{
				case OpCodeClass.ASCII:
				case OpCodeClass.Prefix:
					info = OpCodeTable.GetOpcode((ushort)OpCodeEnum.NamePath);
					pos -= 1; //The op code byte is the data itself
					break;
				case OpCodeClass.ClassUnknown:
					throw new Exception("Unknown AML opcode: 0x" + op.ToString("X2"));
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

		private byte ReadByte()
		{
			return (byte)_source.ReadByte();
		}

		private ushort PeekOpcode()
		{
			ushort code = (ushort)_source.ReadByte();
			if(code == Definitions.ExtendedOpCodePrefix)
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
	}
}
