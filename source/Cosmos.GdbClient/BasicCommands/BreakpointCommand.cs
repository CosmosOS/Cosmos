using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.GdbClient.BasicCommands
{
    public class BreakpointCommand : CommandBase<object>
    {
        public uint Address { get; set; }
        public bool Set { get; set; }

        public BreakpointCommand(uint address, bool set)
            : base(GdbController.Instance)
        {
            Address = address;
            Set = set;
        }

        protected override void Execute()
        {
            Controller.AcknowledgementReceived += new EventHandler(Controller_AcknowledgementReceived);

            string cmd = "B";
            cmd += Address.ToString("x");
            cmd += "," + (Set ? "S" : "C");
        }

        void Controller_AcknowledgementReceived(object sender, EventArgs e)
        {
            Controller.AcknowledgementReceived -= new EventHandler(Controller_AcknowledgementReceived);
            Done(null);
        }
    }
}
