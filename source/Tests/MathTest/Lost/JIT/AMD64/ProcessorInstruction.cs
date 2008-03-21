using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;

namespace Lost.JIT.AMD64
{
	class ProcessorInstruction
	{
		const byte Lock = 0xF0;
		const byte OperandSizeOverride = 0x66;
		const byte AddressSizeOverride = 0x67;
		const byte Rep = 0xF3;
		const byte RepE = 0xF3;
		const byte RepNE = 0xF2;

		internal ProcessorInstruction()
		{
			_labels = new HashSet<InstructionLabel>();
			_prefixes = new HashSet<InstructionPrefix>();
		}

		internal ProcessorInstruction(string opCode, InstructionOperand[] operands, InstructionLabel[] labels, InstructionPrefix[] prefixes, string comments)
		{
			_opCode = opCode;
			_operands = new List<InstructionOperand>(operands);
			_labels = new HashSet<InstructionLabel>(labels);
			_prefixes = new HashSet<InstructionPrefix>(prefixes);
			_comments = comments;
		}

		ICollection<InstructionLabel> Labels
		{
			get { return _labels; }
		} HashSet<InstructionLabel> _labels;
		ICollection<InstructionPrefix> Prefixes
		{
			get { return _prefixes; }
		} HashSet<InstructionPrefix> _prefixes;
		IList<InstructionOperand> Operands
		{
			get { return _operands; }
		} List<InstructionOperand> _operands;
		string OpCode
		{
			get { return _opCode; }
		} string _opCode;
		string Comments
		{
			get { return _comments; }
		} string _comments;

		#region Encoding
		InstructionOperand Dest
		{
			get { return _operands[0]; }
		}
		InstructionOperand Source
		{
			get { return _operands[1]; }
		}

		#region Instructions
		void AddWithCarry(Stream dest)
		{
			if (Dest.IsGeneralPurposeRegister)
				if (Dest.Register == Register.AX && Source.IsImmediate)
				{
					switch (Dest.Size)
					{
					case 1:
						if (Source.Size != null)
							Debug.Assert(Source.Size == 1);

						dest.WriteByte(0x14);
						break;
					case 2:
						if (Source.Size != null)
							Debug.Assert(Source.Size == 2);

						dest.WriteByte(OperandSizeOverride);
						dest.WriteByte(0x15);
						break;
					case 4:
						if (Source.Size != null)
							Debug.Assert(Source.Size == 4);

						dest.WriteByte(0x15);
						break;
					case 8:
						if (Source.Size != null)
							Debug.Assert(Source.Size == 4);

						dest.WriteByte((byte)Rex.Wide);
						dest.WriteByte(0x15);
						break;
					default:
						throw new NotSupportedException();
					}
					Source.WriteTo(dest);
					return;
				} //adc al/ax/etc, imm
			if (Source.IsImmediate)
			{
				switch (Dest.Size)
				{
				case 1:
					if (Source.Size != null)
						Debug.Assert(Source.Size == 4);

					dest.WriteByte(AddressSizeOverride);
					dest.WriteByte(0x80);
					dest.EncodeIndirectMemory(2, Dest.Register);
					break;
				}
				Source.WriteTo(dest);
				return;
			}
			Debug.Assert(false);
		}
		#endregion

		public void Encode(Stream dest)
		{
			switch (_opCode)
			{
			case "adc":
				AddWithCarry(dest);
				break;
			default:
				throw new NotSupportedException(_opCode);
			}
		}

		static void EncodeRegisterRegister(Rex rex, Register dest, Register source, byte opcode, Stream stream)
		{
			if (rex != Rex.None) stream.WriteByte((byte)rex);
			stream.WriteByte(opcode);

			stream.EncodeRegisters(dest, source);
		}
		static void EncodeRegister(Rex rex, Register dest, byte opcode, Stream stream)
		{
			if (rex != Rex.None) stream.WriteByte((byte)rex);

			dest &= Register.Legacy;
			Debug.Assert((opcode | (byte)Register.Legacy) == 0);
			opcode |= (byte)dest;

			stream.WriteByte(opcode);
		}
		#endregion

		#region Parsing
		public static ProcessorInstruction Parse(string code)
		{
			var labels = InstructionLabel.ExtractLabels(ref code);
			var prefixes = InstructionPrefix.ExtractPrefixes(ref code);
			var instr = ExtractInstruction(ref code);
			var comments = ExtractComments(ref code);
			var operands = InstructionOperand.GetOperands(code);

			var result = new ProcessorInstruction(instr.ToUpper(), operands, labels, prefixes, comments);

			return result;
		}

		static readonly Regex _regexComments = new Regex(
			@"^(?<rest>.*)\/\/\s*(?<comments>.*)\s*$",
			RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
		private static string ExtractComments(ref string code)
		{
			var match = _regexComments.Match(code);
			if (match == null) return null;
			if (!match.Success) return null;

			var value = match.Groups["comments"].Value;
			code = match.Groups["rest"].Value;

			return value;
		}

		static readonly Regex _regexInstruction = new Regex(
			@"^\s*(?<instr>\w+)(?<rest>.*$)",
			RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

		private static string ExtractInstruction(ref string code)
		{
			var match = _regexInstruction.Match(code);

			var value = match.Groups["instr"].Value;
			code = match.Groups["rest"].Value;

			return value;
		}
		#endregion
	}
}
