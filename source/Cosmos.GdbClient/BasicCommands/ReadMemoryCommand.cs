using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.GdbClient.BasicCommands
{
    public class ReadMemoryCommand : CommandBase<byte[]>
    {
        public uint Address { get; set; }
        public uint Length { get; set; }

        public ReadMemoryCommand(uint address, uint length)
            : base(GdbController.Instance)
        {
            Address = address;
            Length = length;
        }

        protected override void Execute()
        {
            Controller.PacketReceived += new EventHandler<GdbPacketEventArgs>(Controller_PacketReceived);

            string cmd = "m" + Address.ToString("x") + "," + Length.ToString("x");
            Controller.Enqueue(new GdbPacket(cmd));
        }

        void Controller_PacketReceived(object sender, GdbPacketEventArgs e)
        {
            Controller.PacketReceived += new EventHandler<GdbPacketEventArgs>(Controller_PacketReceived);

            string data = e.Packet.PacketData;
            List<byte> result = new List<byte>();

            for(int i = 0; i < data.Length; i += 2)
                result.Add(byte.Parse(data.Substring(i, 2), System.Globalization.NumberStyles.HexNumber));

            Done(result.ToArray());
        }
    }
}
