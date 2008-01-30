using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.GdbClient.BasicCommands
{
    public class ReadMemoryCommand : CommandBase<byte[]>
    {
        public ReadMemoryCommand() : base(GdbController.Instance) { }

        protected override void Execute()
        {
        }
    }
}
