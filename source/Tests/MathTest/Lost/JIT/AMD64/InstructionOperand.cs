using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;

namespace Lost.JIT.AMD64
{
	class InstructionOperand
	{
		public InstructionOperand(string value)
		{
			_stringValue = value;
		}
			  
		static readonly Regex _regex = new Regex(
			@"^(\s*(?<operand>(\w|\[|\]|\+|\-)+)\s*\,)*\s*$",
			RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

		internal static InstructionOperand[] GetOperands(string code)
		{
			if (code.Trim() != "") code += ",";
			var match = _regex.Match(code);

			var result = new InstructionOperand[match.Groups["operand"].Captures.Count];
			for (int i = 0; i < result.Length; i++)
				result[i] = InstructionOperand.Parse(match.Groups["operand"].Captures[i].Value);

			return result;
		}

		private static InstructionOperand Parse(string operand)
		{
			return new InstructionOperand(operand);
		}

		string _stringValue;

		public override string ToString()
		{
			return _stringValue;
		}

		#region Parsing stuff
		public bool IsGeneralPurposeRegister
		{
			get
			{
				if (_isGeneralPurposeRegister == null)
					throw new NotImplementedException();
				return _isGeneralPurposeRegister.Value;
			}
		} bool? _isGeneralPurposeRegister;

		public int Size
		{
			get
			{
				if (_size == null)
					throw new NotImplementedException();

				return _size.Value;
			}
		} int? _size;
		public ulong Value
		{
			get
			{
				if (_value == null) throw new NotImplementedException();

				return _value.Value;
			}
		} ulong? _value;

		public Register Register
		{
			get
			{
				if (_register == null)
					throw new NotImplementedException();

				return _register.Value;
			}
		} Register? _register;

		public bool IsImmediate
		{
			get
			{
				if (_immediate == null)
					throw new NotImplementedException();

				return _immediate.Value;
			}
		} bool? _immediate;

		public void WriteTo(Stream dest)
		{
			Debug.Assert(IsImmediate);
			
			ulong value = Value;
			for (int i = 0; i < Size; i++, value >>= 8)
				dest.WriteByte((byte)(value & 0xFF));
		}
		#endregion
	}
}
