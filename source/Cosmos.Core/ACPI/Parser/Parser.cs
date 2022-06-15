using ACPILib.AML;
using System;
using System.IO;
using System.Text;

namespace ACPILib.Parser
{
	public class Parser
	{
		private Stream _source;

		public Parser(Stream source)
		{
			_source = source;
		}

		#region Stream reading tools
		private byte PeekByte()
		{
			byte val = (byte)_source.ReadByte();
			_source.Seek(-1, SeekOrigin.Current);

			return val;
		}

		private byte[] PeekBytes(int number)
		{
			byte[] buffer = new byte[number];
			int moved = _source.Read(buffer, 0, number);

			_source.Seek(-moved, SeekOrigin.Current);

			return buffer;
		}

		private byte[] ReadBytes(int number)
		{
			byte[] buffer = new byte[number];
			_source.Read(buffer, 0, number);

			return buffer;
		}
		#endregion

		#region AML reading tools
		private OpCode ReadOpcode()
		{
			ushort opCodeValue = (ushort)_source.ReadByte();
			if (opCodeValue == 0x5B) //Extended op prefix
			{
				opCodeValue = (ushort)((opCodeValue << 8) | (byte)_source.ReadByte());
			}

			return OpCodeTable.GetOpcode(opCodeValue);
		}

		private ushort PeekOpcode()
		{
			ushort opCodeValue = (ushort)_source.ReadByte();
			if (opCodeValue == 0x5B) //Extended op prefix
			{
				opCodeValue = (ushort)((opCodeValue << 8) | (byte)_source.ReadByte());

				_source.Seek(-2, SeekOrigin.Current);
			}
			else
			{
				_source.Seek(-1, SeekOrigin.Current);
			}

			return opCodeValue;
		}

		private AMLOp ReadAmlOp(AMLOp parent)
		{
			AMLOp op = new AMLOp(parent);

			op.Start = (int)_source.Position;
			op.OpCode = ReadOpcode();

			return op;
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

		private string ReadNameString()
		{
			string name = string.Empty;

			int start = (int)_source.Position;
			int end = (int)_source.Position;

			while (Definitions.IsNameRootPrefixOrParentPrefix(PeekByte()))
			{
				start++;
				_source.Seek(start, SeekOrigin.Begin);
			}

			end = start;

			//Find the size of the segment
			_source.Seek(start, SeekOrigin.Begin);
			int length = 0;
			switch (PeekByte())
			{
				case 0x0: //Null name
					end++;
					start = end;
					break;
				case Definitions.DualNamePrefix:
					//Two name segments
					start = end = (int)_source.Position + 1;
					end += (2 * Definitions.NameSize);

					length = end - start;
					break;
				case Definitions.MultiNamePrefix:
					//Multiple name segments, 4 chars each
					_source.Seek(start + 1, SeekOrigin.Begin);
					byte count = PeekByte();

					start = end = (int)_source.Position + 1;
					end += (count * Definitions.NameSize);

					length = end - start;
					break;
				default:
					//Single segment
					length = Definitions.NameSize;
					end += length;
					break;
			}

			_source.Seek(start, SeekOrigin.Begin);
			for (int x = 0; x < length; x++)
			{
				byte readByte = (byte)_source.ReadByte();
				if (readByte == 0x0)
					continue;

				name += (char)readByte;

				if ((x + 1) % 4 == 0 && x + 1 < length)
					name += ".";
			}

			_source.Seek(end, SeekOrigin.Begin);

			return name;
		}

		private string ReadNamePath()
		{
			return ReadNameString(); //Is this the same as the path?
		}

		private AMLOp ReadField(AMLOp parent)
		{
			OpCodeEnum type = (OpCodeEnum)PeekByte();

			AMLOp amlOp = null;
			switch (type)
			{
				case OpCodeEnum.FieldOffset:
					amlOp = new AMLOp(OpCodeTable.GetOpcode((ushort)OpCodeEnum.ReservedField), parent);
					_source.Seek(1, SeekOrigin.Current);

					amlOp.Value = ReadPackageLength(); //Read the value as a PkgLength
					break;
				case OpCodeEnum.FieldAccess:
				case OpCodeEnum.FieldExternalAccess:
					amlOp = new AMLOp(OpCodeTable.GetOpcode((ushort)((type == OpCodeEnum.FieldAccess) ? OpCodeEnum.AccessField : OpCodeEnum.ByteList)), parent);
					_source.Seek(1, SeekOrigin.Current);

					int accessType = (byte)_source.ReadByte();
					int accessAttribute = (byte)_source.ReadByte();

					amlOp.Value = (int)(accessType | (accessAttribute << 8));

					if (type == OpCodeEnum.FieldExternalAccess) //This one has a 3rd byte
					{
						amlOp.Value = (int)amlOp.Value | (_source.ReadByte() << 16);
					}
					break;
				case OpCodeEnum.FieldConnection:
					amlOp = new AMLOp(OpCodeTable.GetOpcode((ushort)OpCodeEnum.Connection), parent);
					_source.Seek(1, SeekOrigin.Current);

					if (PeekByte() == (byte)OpCodeEnum.Buffer)
					{
						_source.Seek(1, SeekOrigin.Current);

						int end = (int)_source.Position + ReadPackageLength();

						while (_source.Position < end)
						{
							AMLOp arg = new AMLOp(OpCodeTable.GetOpcode((ushort)OpCodeEnum.ByteList), parent);

							OpCodeEnum actualOpCode = (OpCodeEnum)(byte)_source.ReadByte();
							int bufferLength = 0;
							switch (actualOpCode)
							{
								case OpCodeEnum.Byte:
									bufferLength = (byte)_source.ReadByte();
									break;
								case OpCodeEnum.Word:
									bufferLength = BitConverter.ToInt16(ReadBytes(2), 0);
									break;
								case OpCodeEnum.DWord:
									bufferLength = BitConverter.ToInt32(ReadBytes(4), 0);
									break;
							}

							arg.Value = ReadBytes(bufferLength);

							amlOp.Nodes.Add(arg);
						}
					}
					else
					{
						AMLOp arg = new AMLOp(OpCodeTable.GetOpcode((ushort)OpCodeEnum.NamePath), parent);
						arg.Value = ReadNameString();

						amlOp.Nodes.Add(arg);
					}
					break;
				default:
					amlOp = new AMLOp(OpCodeTable.GetOpcode((ushort)OpCodeEnum.NamedField), parent);

					amlOp.Name = Encoding.ASCII.GetString(ReadBytes(4));

					amlOp.Value = ReadPackageLength(); //Read the value as a PkgLength
					break;
			}

			return amlOp;
		}

		private AMLOp ReadTermArg(AMLOp parent)
		{
			byte[] peek4 = PeekBytes(4);

			//Check if all of the chars are letters
			bool probalbyString = true;
			for (int x = 0; x < peek4.Length; x++)
			{
				if (!char.IsDigit((char)peek4[x]) && !char.IsUpper((char)peek4[x]))
				{
					probalbyString = false;
					break;
				}
			}

			if (probalbyString) //No idea what I'm doing here
			{
				AMLOp str = new AMLOp(OpCodeTable.GetOpcode(0x41), parent);
				str.Value = ReadNameString();

				return str;
			}

			return ReadAmlOp(parent);
		}
		#endregion

		#region AML parsing
		public AMLOp Parse()
		{
			AMLOp root = new AMLOp(null);
			root.Name = "Root";

			while (_source.Position < _source.Length)
			{
				AMLOp op = ReadAmlOp(null);

				root.Nodes.Add(op);

				ParseOp(op, false);
			}

			return root;
		}

		private void ParseOp(AMLOp op, bool isMethodBody)
		{
			if (op.OpCode == null)
				return;

			for (int x = 0; x < op.OpCode.ParseArgs.Length; x++)
			{
				switch (op.OpCode.ParseArgs[x])
				{
					//None
					case ParseArgFlags.None:
						break;

					//Simple values
					case ParseArgFlags.Name:
					case ParseArgFlags.NameString:
						op.Name = ReadNameString();
						break;
					case ParseArgFlags.ByteData:
						op.Nodes.Add(new AMLOp(OpCodeTable.GetOpcode((ushort)OpCodeEnum.Byte), op)
						{
							Value = (byte)_source.ReadByte()
						});
						break;
					case ParseArgFlags.WordData:
						op.Nodes.Add(new AMLOp(OpCodeTable.GetOpcode((ushort)OpCodeEnum.Word), op)
						{
							Value = BitConverter.ToInt16(ReadBytes(2), 0)
						});
						break;
					case ParseArgFlags.DWordData:
						op.Nodes.Add(new AMLOp(OpCodeTable.GetOpcode((ushort)OpCodeEnum.DWord), op)
						{
							Value = BitConverter.ToInt32(ReadBytes(4), 0)
						});
						break;
					case ParseArgFlags.QWordData:
						op.Nodes.Add(new AMLOp(OpCodeTable.GetOpcode((ushort)OpCodeEnum.QWord), op)
						{
							Value = BitConverter.ToInt64(ReadBytes(8), 0)
						});
						break;
					case ParseArgFlags.CharList:
						//Null terminated string
						string value = "";

						byte read;
						while ((read = (byte)_source.ReadByte()) != 0x0)
						{
							value += (char)read;
						}

						op.Nodes.Add(new AMLOp(OpCodeTable.GetOpcode((ushort)OpCodeEnum.String), op)
						{
							Value = value
						});
						break;
					case ParseArgFlags.ByteList:
						int length = op.End - (int)_source.Position + 1;
						if (length < 0)
						{
							throw new ArgumentOutOfRangeException();
						}

						op.Nodes.Add(new AMLOp(OpCodeTable.GetOpcode((ushort)OpCodeEnum.ByteList), op)
						{
							Value = ReadBytes(length)
						});
						break;

					//Complex values
					case ParseArgFlags.PackageLength:
						op.Length = ReadPackageLength();
						break;
					case ParseArgFlags.ObjectList:
					case ParseArgFlags.DataObjectList:
					case ParseArgFlags.TermList:
						if (op.OpCode.Code == OpCodeEnum.Method)
						{
							//Methods are too buggy, skip them

							_source.Seek(op.End + 1, SeekOrigin.Begin);
							break;
						}

						while (_source.Position < op.End)
						{
							AMLOp term = ReadTermArg(op);

							op.Nodes.Add(term);

							ParseOp(term, (op.OpCode.Code == OpCodeEnum.Method));
						}
						break;
					case ParseArgFlags.DataObject:
					case ParseArgFlags.TermArg:

						AMLOp arg = ReadTermArg(op);

						if (isMethodBody || op.OpCode.Code == OpCodeEnum.Method)
						{
							if (arg.Value is string)
							{
								AMLOp scope = arg.FindScope();
								if (scope != null)
								{
									AMLOp method = scope.FindMethod((string)arg.Value);

									if (method != null)
									{
										AMLOp call = new ACPILib.Parser.AMLOp(OpCodeTable.GetOpcode((ushort)OpCodeEnum.MethodCall), op);
										call.Value = arg.Value;
										arg = call;
									}
								}
							}
						}

						op.Nodes.Add(arg);
						ParseOp(arg, isMethodBody);

						break;
					case ParseArgFlags.FieldList:
						while (_source.Position < op.End)
						{
							op.Nodes.Add(ReadField(op));
						}
						break;
					case ParseArgFlags.SimpleName:
					case ParseArgFlags.NameOrReference:
						ushort subOp = PeekOpcode();
						if (subOp == 0 || Definitions.IsNameRootPrefixOrParentPrefix((byte)subOp) || Definitions.IsLeadingChar((byte)subOp))
						{
							AMLOp namePath = new AMLOp(OpCodeTable.GetOpcode((ushort)OpCodeEnum.NamePath), op);
							op.Nodes.Add(namePath);

							namePath.Value = ReadNamePath();
						}
						else
						{
							AMLOp term = ReadAmlOp(op);
							op.Nodes.Add(term);
							ParseOp(term, isMethodBody);
						}
						break;
					case ParseArgFlags.Target:
					case ParseArgFlags.SuperName:
						ushort subOp2 = PeekOpcode();
						if (subOp2 == 0 || Definitions.IsNameRootPrefixOrParentPrefix((byte)subOp2) || Definitions.IsLeadingChar((byte)subOp2))
						{
							AMLOp namePath = new AMLOp(OpCodeTable.GetOpcode((ushort)OpCodeEnum.NamePath), op);
							op.Nodes.Add(namePath);

							namePath.Value = ReadNamePath();
						}
						else
						{
							AMLOp term = ReadAmlOp(op);
							op.Nodes.Add(term);
							ParseOp(term, isMethodBody);
						}
						break;

					default:
						throw new NotImplementedException();
				}
			}
		}
		#endregion
	}
}
