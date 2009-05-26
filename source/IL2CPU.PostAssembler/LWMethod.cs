using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2CPU.PostAssembler
{
    public class LWMethod
    {
        string mName;
        List<LWInstruction> mInstructions = new List<LWInstruction>();

        public LWMethod(string name, IEnumerable<LWInstruction> instructions)
            : this(name)
        {
            mInstructions.AddRange(instructions);
        }


        public LWMethod(string name)
        {
            mName = name;
        }

        public string Name { get { return mName; } }

        public List<LWInstruction> Instructions
        {
            get
            {
                return this.mInstructions;
            }
        }

    }
}
