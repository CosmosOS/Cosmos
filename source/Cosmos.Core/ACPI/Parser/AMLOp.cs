using ACPILib.AML;
using System.Collections.Generic;

namespace ACPILib.Parser
{
	public class AMLOp
	{
		public AMLOp Parent;

		public OpCode OpCode;

		public string Name;

		public int Start;
		public int Length;
		public int End
		{
			get { return (Start + Length); }
		}

		public object Value;

		public List<AMLOp> Nodes = new List<AMLOp>();

		public AMLOp(AMLOp parent)
		{
			Parent = parent;
		}

		public AMLOp(OpCode op, AMLOp parent)
		{
			OpCode = op;
			Parent = parent;
		}

		public override string ToString()
		{
			return OpCode.ToString();
		}

		public AMLOp FindScope()
		{
			AMLOp check = Parent;
			while (check != null)
			{
				if (check.OpCode != null && check.OpCode.Code == OpCodeEnum.Scope)
					return check;

				check = check.Parent;
			}

			return null;
		}

		public AMLOp FindMethod(string name)
		{
			if (OpCode != null && OpCode.Code == OpCodeEnum.Method && Name == name)
				return this;

			foreach(AMLOp node in Nodes)
			{
				AMLOp method = node.FindMethod(name);

				if (method != null)
					return method;
			}

			return null;
		}
	}
}
