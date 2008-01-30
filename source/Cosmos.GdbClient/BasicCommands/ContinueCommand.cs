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
        private uint? _address;

        public uint? Address
        {
            get { return _address; }
            set { _address = value; }
        }

        private byte? _signal;

        public byte? Signal
        {
            get { return _signal; }
            set { _signal = value; }
        }

        public ContinueCommand() : base(GdbController.Instance) { }

        public ContinueCommand(uint? address)
            : this()
        {
            _address = address;
        }

        void Controller_AcknowledgementReceived(object sender, EventArgs e)
        {
            Controller.AcknowledgementReceived -= new EventHandler(Controller_AcknowledgementReceived);
            Done(null);
        }

        protected override void Execute()
        {
            Controller.AcknowledgementReceived += new EventHandler(Controller_AcknowledgementReceived);
            string cmd = "c";

            if (_signal.HasValue)
            {
                cmd += _signal.Value.ToString("x");
                if (_address.HasValue)
                    cmd += ";";
            }

            if (_address.HasValue)
                cmd += _address.Value.ToString("x");

            Controller.Enqueue(new GdbPacket(cmd));
        }
    }
}