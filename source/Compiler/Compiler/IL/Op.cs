using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Cosmos.Compiler.IL
{
    public abstract class Op {
        public virtual void Scan(ILReader aReader, Scanner aScanner)
        {
            // dont do anything here
        }
    }
}