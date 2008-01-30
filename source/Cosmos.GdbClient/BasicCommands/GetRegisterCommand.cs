using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.GdbClient.BasicCommands
{
    public class GetRegisterCommand : CommandBase<string>
    {
        public int Index { get; set; }

        public GetRegisterCommand(int index)
            : base(GdbController.Instance)
        {
            Index = index;
        }

        protected override void Execute()
        {
            Controller.PacketReceived += new EventHandler<GdbPacketEventArgs>(Controller_PacketReceived);
            Controller.Enqueue(new GdbPacket("p" + Index.ToString("x")));
        }

        void Controller_PacketReceived(object sender, GdbPacketEventArgs e)
        {
            Controller.PacketReceived -= new EventHandler<GdbPacketEventArgs>(Controller_PacketReceived);
            Done(e.Packet.PacketData);
        }
    }
}
