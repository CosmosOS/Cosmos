using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.GdbClient.BasicCommands
{
    /// <summary>
    /// Represents the continue command.
    /// </summary>
    public class ContinueCommand : CommandBase<object>
    {
        public ContinueCommand(GdbController controller) : base(controller) { }

        void Controller_AcknowledgementReceived(object sender, EventArgs e)
        {
            Controller.AcknowledgementReceived -= new EventHandler(Controller_AcknowledgementReceived);
            Done(null);
        }

        protected override void Execute()
        {
            Controller.AcknowledgementReceived += new EventHandler(Controller_AcknowledgementReceived);
            Controller.Enqueue(new GdbPacket("c"));
        }
    }
}