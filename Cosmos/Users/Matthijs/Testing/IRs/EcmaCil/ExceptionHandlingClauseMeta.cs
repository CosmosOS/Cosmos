using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil
{
    public class ExceptionHandlingClauseMeta
    {

        public TypeMeta CatchType
        {
            get;
            set;
        }

        public int FilterStart
        {
            get;
            set;
        }

        public ExceptionHandlingClauseFlagsEnum Flags
        {
            get;
            set;
        }

        public int HandlerEnd
        {
            get;
            set;
        }

        public int HandlerStart
        {
            get;
            set;
        }

        public int TryEnd
        {
            get;
            set;
        }

        public int TryStart
        {
            get;
            set;
        }
    }
}