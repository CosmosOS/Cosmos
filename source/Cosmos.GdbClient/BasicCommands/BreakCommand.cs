using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.GdbClient.BasicCommands
{
    public class BreakCommand : CommandBase<object>
    {
        public BreakCommand() : base(GdbController.Instance) { }

        protected override void Execute()
        {
            Controller.AcknowledgementReceived += new EventHandler(Controller_AcknowledgementReceived);
            Controller.Enqueue(new GdbPacket("b"));
        }

        void Controller_AcknowledgementReceived(object sender, EventArgs e)
        {
            Controller.AcknowledgementReceived -= new EventHandler(Controller_AcknowledgementReceived);
            Done(null);
        }
    }
}
