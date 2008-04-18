using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
	[Serializable]
	public enum Condition
	{
		/// <summary>
		/// DEST > SOURCE (signed)
		/// </summary>
		Greater,
		/// <summary>
		/// DEST >= SOURCE (singed)
		/// </summary>
		GreaterOrEqual,
		/// <summary>
		/// DEST &lt; SOURCE (signed)
		/// </summary>
		Less,
		/// <summary>
		/// DEST &lt;= SOURCE (signed)
		/// </summary>
		LessOrEqual,

		/// <summary>
		/// DEST > SOURCE (unsigned)
		/// </summary>
		Above,
		/// <summary>
		/// DEST >= SOURCE (unsigned)
		/// </summary>
		AboveOrEqual,
		/// <summary>
		/// DEST &lt; SOURCE (unsigned)
		/// </summary>
		Below,
		/// <summary>
		/// DEST &lt;= SOURCE (unsigned)
		/// </summary>
		BelowOrEqual,
	}

	public static class ConditionHelperExt
	{
		public static string GetMnemonics(this Condition condition)
		{
			switch (condition)
			{
			case Condition.Above:
				return "a";
			case Condition.AboveOrEqual:
				return "ae";
			case Condition.Below:
				return "b";
			case Condition.BelowOrEqual:
				return "be";

			case Condition.Greater:
				return "g";
			case Condition.GreaterOrEqual:
				return "ge";
			case Condition.Less:
				return "l";
			case Condition.LessOrEqual:
				return "le";

			default:
				throw new NotSupportedException();
			}
		}
	}
}
