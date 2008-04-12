using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RsenkTest.Commands
{
    abstract class ParameterBase : CommandBase
    {
        public abstract override string Name { get; }
        public abstract override string Summary { get; }
        public abstract override void Help();
        public abstract void Execute();
    }
}
