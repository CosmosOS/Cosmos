using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil
{
    public class MethodBodyMeta
    {
        public LocalVariableMeta[] LocalVariables
        {
            get;
            set;
        }

        public ExceptionHandlingClauseMeta[] ExceptionHandlingClauses
        {
            get;
            set;
        }

        public bool InitLocals
        {
            get;
            set;
        }

        public IL.BaseInstruction[] Instructions
        {
            get;
            set;
        }
    }
}
