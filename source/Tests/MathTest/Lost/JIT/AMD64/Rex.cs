using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lost.JIT.AMD64
{
	[Flags]
	public enum Rex : byte
	{
		None = 0x40,
		/// <summary>
		/// Use 64-bit operand size
		/// </summary>
		/// <remarks>
		/// Setting the REX.W bit to 1 specifies a 64-bit operand size. Like the
		/// existing 66h operand-size prefix, the REX 64-bit operand-size override has no effect on byte
		/// operations. For non-byte operations, the REX operand-size override takes precedence over the 66h
		/// prefix. If a 66h prefix is used together with a REX prefix that has the REX.W bit set to 1, the 66h
		/// prefix is ignored. However, if a 66h prefix is used together with a REX prefix that has the REX.W bit
		/// cleared to 0, the 66h prefix is not ignored and the operand size becomes 16 bits.
		///</remarks>
		Wide = None | (1 << 3),
		/// <summary>
		/// Reg index from ModRM extension.
		/// </summary>
		/// <remarks>
		/// The REX.R bit adds a 1-bit (high) extension to the ModRM reg field (page 17)
		/// when that field encodes a GPR, XMM, control, or debug register. REX.R does not modify ModRM reg
		/// when that field specifies other registers or opcodes. REX.R is ignored in such cases.
		/// </remarks>
		Reg = None | (1 << 2),
		/// <summary>
		/// SIB index register extension. 
		/// </summary>
		NewRegIndex = None | (1 << 1),
		/// <summary>
		/// SIB index register extension. 
		/// Use Index mnenonic instead.
		/// </summary>
		X = None | (1 << 1),
		/// <summary>
		/// Extension of the ModRM r/m field1, SIB base field, or opcode reg field
		/// </summary>
		/// <remarks>
		/// The REX.B bit adds a 1-bit (high) extension to either the ModRM r/m field to specify
		/// a GPR or XMM register, or to the SIB base field to specify a GPR. (See Table 2-2 on page 40 for more
		/// about the REX.B bit.)
		/// </remarks>
		B = None | (1 << 0),
		/// <summary>
		/// Extension of opcode reg field
		/// </summary>
		NewRegOpcode = B,
		/// <summary>
		/// Extension of base field in SIB
		/// </summary>
		NewRegBase = B,
	}
}
