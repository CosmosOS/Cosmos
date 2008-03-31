using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lost.JIT.AMD64OLD
{
	[Obsolete("")]
	class ImmediateOperand: InstructionOperand
	{
		internal ImmediateOperand(int size, ulong value):base(value.ToString(string.Format("0X{0}", size*2)))
		{
			switch (size)
			{
			case 1:
				byte b = checked((byte)value);
				break;
			case 2:
				ushort s = checked((ushort)value);
				break;
			case 4:
				uint i = checked((uint)value);
				break;
			case 8:
				break;
			default:
				throw new NotSupportedException("incorrect operand size");
			}
			_size = size;
			_value = (long)value;
		}

		int _size;
		long _value;

		/// <summary>
		/// Size of immediate operand, in bytes
		/// </summary>
		public int Size
		{
			get { return _size; }
		}
		/// <summary>
		/// Value of immediate operand
		/// </summary>
		public long Value
		{
			get { return _value; }
		}

		public override string ToString()
		{
			var val = _value.ToString(string.Format("X{0}", Size*2));
			return string.Format("0{0}h", val);
		}
	}
}
