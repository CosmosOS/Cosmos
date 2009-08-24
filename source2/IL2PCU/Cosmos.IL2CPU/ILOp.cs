using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU
{
    public abstract class ILOp
    {
        protected readonly Assembler Assembler;
        protected ILOp( Assembler aAsmblr )
        {
            Assembler = aAsmblr;
        }

        // This is called execute and not assemble, as the scanner
        // could be used for other things, profiling, analysis, reporting, etc
        public abstract void Execute( MethodInfo aMethod, ILOpCode aOpCode );


        public virtual void Execute( MethodInfo aMethod, ILOpCode aOpCode, ILOpCode aNextOpCode )
        {
            Execute( aMethod, aOpCode); 
        }
    }
}
