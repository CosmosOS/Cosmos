using ACPIAML.Interupter;
using ACPILib.AML;
using System.Collections.Generic;

namespace ACPILib.Parser2
{
	public class ParseNode
	{
		public OpCode Op;
		public string Name;

		public long Start;
		public long DataStart;
		public long Length;

		public long End
		{
			get { return Start + Length + Op.CodeByteSize; }
		}

		public StackObject? ConstantValue;
		public List<StackObject> Arguments = new List<StackObject>();
		public List<ParseNode> Nodes = new List<ParseNode>();

		public override string ToString()
		{
			return Op.ToString();
		}
	}
}
