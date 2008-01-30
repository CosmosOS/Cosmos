using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace Cosmos.GdbClient
{
    public class GdbConnection
    {

        private TcpClient _client;
        private NetworkStream _stream;
        private string _host;
        private int _port;

        private byte[] _receiveBuffer;
        private Queue<byte[]> _sendBuffer = new Queue<byte[]>();

        private Decoder _decoder = Encoding.ASCII.GetDecoder();

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        #region Ctor

        public GdbConnection() : this(1234) { }

        public GdbConnection(string host) : this(host, 1234) { }

        public GdbConnection(int port) : this("localhost", port) { }

        public GdbConnection(string host, int port)
        {
            _port = port;
            _host = host;
            _receiveBuffer = new byte[1024];
        }

        #endregion

        #region Open/Close

        public void Open()
        {
            if (_client != null)
                Close();

            _client = new TcpClient(AddressFamily.InterNetwork);
            _client.Connect(_host, _port);

            _stream = _client.GetStream();
            BeginRead();
        }

        public void Close()
        {
            if (_stream != null)
            {
                _stream.Close();
                _stream.Dispose();
            }

            if (_client != null)
                _client.Close();

            _client = null;
            _stream = null;
            _sendBuffer.Clear();
        }
        #endregion

        #region Read
        private void BeginRead()
        {
            _stream.BeginRead(_receiveBuffer, 0, _receiveBuffer.Length, new AsyncCallback(EndRead), null);
        }

        private void EndRead(IAsyncResult result)
        {
            int count = _stream.EndRead(result);

            OnDataReceived(GetChars(count));

            BeginRead();
        }

        private char[] GetChars(int count)
        {
            lock (_decoder)
            {
                int charcount = _decoder.GetCharCount(_receiveBuffer, 0, count, false);
                char[] result = new char[charcount];
                _decoder.GetChars(_receiveBuffer, 0, count, result, 0, false);
                return result;
            }
        }

        protected void OnDataReceived(char[] data)
        {
            System.Diagnostics.Debug.WriteLine("<- " + new string(data));

            if (DataReceived != null)
                DataReceived(this, new DataReceivedEventArgs(data));
        }
        #endregion

        #region Send
        public void Send(string data)
        {

            byte[] bytes = Encoding.ASCII.GetBytes(data);

            lock (_sendBuffer)
                _sendBuffer.Enqueue(bytes);
            BeginSend();
        }

        private void BeginSend()
        {
            lock (_sendBuffer)
            {
                if (_sendBuffer.Count == 0)
                    return;
                byte[] buffer = _sendBuffer.Dequeue();

                System.Diagnostics.Debug.WriteLine("-> " + Encoding.ASCII.GetString(buffer));

                _stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(EndSend), null);
            }
        }

        private void EndSend(IAsyncResult result)
        {
            _stream.EndWrite(result);
            BeginSend();
        }
        #endregion
    }

    public class DataReceivedEventArgs : EventArgs
    {
        private char[] _data;

        public char[] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public DataReceivedEventArgs(char[] data)
        {
            _data = data;
        }
    }
}
