using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.GdbClient.BasicCommands
{
    public class StepCommand : CommandBase<object>
    {
        public uint? Address { get; set; }

        public StepCommand() : base(GdbController.Instance) { Address = null; }

        public StepCommand(uint? address) : this() { Address = address; }

        protected override void Execute()
        {
            Controller.AcknowledgementReceived += new EventHandler(Controller_AcknowledgementReceived);
            string cmd = "s";
            if (Address.HasValue)
                cmd += Address.Value.ToString("x");
            Controller.Enqueue(new GdbPacket(cmd));
        }

        void Controller_AcknowledgementReceived(object sender, EventArgs e)
        {
            Controller.AcknowledgementReceived -= new EventHandler(Controller_AcknowledgementReceived);
            Done(null);
        }
    }
}
