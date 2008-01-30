using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.GdbClient
{
    /// <summary>
    /// Controls the Gdp Protocol.
    /// </summary>
    public class GdbController
    {
        private static GdbController _instance;
        /// <summary>
        /// Gets or sets the default GdbConnection instance.
        /// </summary>
        public static GdbController Instance
        {
            get { return _instance; }
            set { _instance = value; }
        }

        private GdbConnection _connection;

        /// <summary>
        /// The parser queue.
        /// </summary>
        private Queue<char> _parserQueue = new Queue<char>();

        /// <summary>
        /// The sender queue.
        /// </summary>
        private Queue<GdbPacket> _sendQueue = new Queue<GdbPacket>();

        /// <summary>
        /// The current packet.
        /// </summary>
        private StringBuilder _currentPacket = new StringBuilder();

        /// <summary>
        /// When a packet is received.
        /// </summary>
        public event EventHandler<GdbPacketEventArgs> PacketReceived;

        /// <summary>
        /// When an acknowledgement is received.
        /// </summary>
        public event EventHandler AcknowledgementReceived;

        /// <summary>
        /// Creates a new instance of the <see cref="GdbController"/> class.
        /// </summary>
        /// <param name="connection"></param>
        public GdbController(GdbConnection connection)
        {
            _connection = connection;
            connection.DataReceived += new EventHandler<DataReceivedEventArgs>(connection_DataReceived);
        }

        /// <summary>
        /// Called when data is received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void connection_DataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (_parserQueue)
            {
                for (int i = 0; i < e.Data.Length; i++)
                    _parserQueue.Enqueue(e.Data[i]);
                Parse();
            }
        }

        /// <summary>
        /// Parses the current data stream.
        /// </summary>
        private void Parse()
        {
            lock (_parserQueue)
            {
                while (_parserQueue.Count != 0)
                {
                    char c = _parserQueue.Dequeue();

                    if (c == '+' || c == '-' || c == '#')
                    {
                        if (c == '#')
                        {
                            _currentPacket.Append(c);
                            _currentPacket.Append(_parserQueue.Dequeue());
                            _currentPacket.Append(_parserQueue.Dequeue());
                        }

                        // I don't get this.
                        if (_currentPacket.ToString() == "#")
                            _currentPacket = new StringBuilder();

                        if (_currentPacket.Length != 0)
                        {
                            GdbPacket packet;
                            try
                            {
                                packet = GdbPacket.FromString(_currentPacket.ToString());
                            }
                            catch
                            {
                                _currentPacket = new StringBuilder();
                                SendFail();
                                continue;
                            }

                            OnPacketReceived(packet);
                            SendAcknowledge();
                            GotAcknowledge();
                            _currentPacket = new StringBuilder();
                        }

                        if (c == '+')
                        {
                            GotAcknowledge(); // Okay, next packet.
                            continue;
                        }
                        else if (c == '-')
                        {
                            SendPacket(); // Resend last please.
                            continue;
                        }
                    }
                    _currentPacket.Append(c);
                }
            }
        }

        private void OnPacketReceived(GdbPacket packet)
        {
            if (packet.PacketData.StartsWith("S"))
                return;

            if (this.PacketReceived != null)
                PacketReceived(this, new GdbPacketEventArgs(packet));
        }

        private void OnAcknowledgementReceived()
        {
            if (this.AcknowledgementReceived != null)
                AcknowledgementReceived(this, EventArgs.Empty);
        }

        private void SendFail()
        {
            _connection.Send("-");
        }

        private void SendAcknowledge()
        {
            _connection.Send("+");
        }

        /// <summary>
        /// Occurs when a acknowledge is recieved.
        /// </summary>
        private void GotAcknowledge()
        {
            lock (_sendQueue)
            {
                if (_sendQueue.Count != 0)
                    _sendQueue.Dequeue();
                SendPacket();
            }
            OnAcknowledgementReceived();
        }

        /// <summary>
        /// Acknowledges the packet.
        /// </summary>
        private void SendPacket()
        {
            lock (_sendQueue)
            {
                if(_sendQueue.Count != 0)
                    _connection.Send(_sendQueue.Peek().ToString());
            }
        }

        /// <summary>
        /// Sends data.
        /// </summary>
        public void Enqueue(GdbPacket packet)
        {
            lock (_sendQueue)
            {
                _sendQueue.Enqueue(packet);
                if (_sendQueue.Count == 1)
                    SendPacket();
            }
        }

        /// <summary>
        /// Opens up extended commands.
        /// </summary>
        public void Extended()
        {
            Normal();
            _connection.Send("!");
        }

        /// <summary>
        /// Opens up normal commands.
        /// </summary>
        public void Normal()
        {
            _connection.Open();
        }

        public void Close()
        {
            _connection.Close();
        }
    }
}
