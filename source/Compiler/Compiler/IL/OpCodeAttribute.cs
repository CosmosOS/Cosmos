using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.IL
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class OpCodeAttribute : Attribute
    {
        public readonly OpCodeEnum OpCode;

        public OpCodeAttribute(OpCodeEnum aOpCode)
        {
            OpCode = aOpCode;
        }
    }
}