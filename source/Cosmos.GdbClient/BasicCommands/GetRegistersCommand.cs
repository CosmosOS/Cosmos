using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.GdbClient.BasicCommands
{
    public class GetRegistersCommand : CommandBase<string[]>
    {
        public GetRegistersCommand() : base(GdbController.Instance) { }

        protected override void Execute()
        {
            Controller.PacketReceived += new EventHandler<GdbPacketEventArgs>(Controller_PacketReceived);
            Controller.Enqueue(new GdbPacket("g"));
        }

        void Controller_PacketReceived(object sender, GdbPacketEventArgs e)
        {
            Controller.PacketReceived -= new EventHandler<GdbPacketEventArgs>(Controller_PacketReceived);
            Done(null);
        }
    }
}
