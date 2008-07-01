using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cosmos.Hardware.Network.TCPIPModel.NetworkLayer.IPv4;
using Cosmos.Hardware.Network;
using Cosmos.Kernel;

namespace FrodeIPv4Test
{
    public partial class IPTestForm : Form
    {
        public IPTestForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bytesTextBox.Text = "";

            IPv4Packet packet = new IPv4Packet();

            //packet.DestinationAddress = new IPv4Address(172, 28, 6, 1); //TODO: add parse
            packet.DestinationAddress = IPv4Address.Parse(destinationTextBox.Text);
            packet.SourceAddress = IPv4Address.Parse(sourceTextBox.Text);
            packet.Identification = ushort.Parse(identificationTextBox.Text);
            packet.FragmentFlags = IPv4Packet.Fragmentation.DoNotFragment;
            packet.FragmentOffset = ushort.Parse(fragmentOffsetTextBox.Text);
            packet.Protocol = IPv4Packet.Protocols.TCP;

            List<byte> data = new List<byte>();
            //data.Add(0xFF);
            //data.Add(0xFE);
            //data.Add(0xFD);
            //data.Add(0xFC);
            //data.Add(0xFB);
            //data.Add(0xFA);
            //packet.Data = data;

            foreach (char c in dataTextBox.Text.ToCharArray())
                data.Add((byte)c);

            packet.Data = data;
            

            packet.HeaderLength = packet.CalculateHeaderLength();
            packet.TotalLength = packet.CalculateTotalLength();
            packet.HeaderChecksum = packet.CalculateHeaderChecksum();

            foreach (byte b in packet.RawBytes())
                bytesTextBox.Text += b.ToHex(2) + ":";

            bytesTextBox.Text += Environment.NewLine + Environment.NewLine;

            bytesTextBox.Text += packet.ToString();

        }
    }
}
