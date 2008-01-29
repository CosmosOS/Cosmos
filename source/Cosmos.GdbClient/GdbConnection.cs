using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace Cosmos.GdbClient
{
    public class GdbConnection
    {
        private TcpClient _client;
        private string _host;
        private int _port;
        private NetworkStream _stream;

        private byte[] _buffer;

        private Decoder _decoder = Encoding.ASCII.GetDecoder();

        private Queue<char> _parsed = new Queue<char>();

        public GdbConnection(string host, int port)
        {
            _port = port;
            _host = host;
            _buffer = new byte[1024];
        }

        public void Open()
        {
            if (_client != null)
                Close();

            _client = new TcpClient(AddressFamily.InterNetwork);
            _client.Connect(_host, _port);

            _stream = _client.GetStream();
            BeginRead();
        }

        private void BeginRead()
        {
            _stream.BeginRead(_buffer, 0, _buffer.Length, new AsyncCallback(EndRead), null);
        }

        private void EndRead(IAsyncResult result)
        {
            int count = _stream.EndRead(result);
            Append(count);
            BeginRead();
        }

        private void Append(int count)
        {
            char[] chars = GetChars(count);
            foreach (char c in chars)
                _parsed.Enqueue(c);

            Parse();
        }

        private void Parse()
        {
            while (_parsed.Count > 0)
            {
                char c = _parsed.Dequeue();
                
            }
        }

        private char[] GetChars(int count)
        {
            int count = _decoder.GetCharCount(_buffer, 0, count, false);
            char[] result = new char[count];
            _decoder.GetChars(_buffer, 0, count, result, 0, false);
            return result;
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
        }
    }
}
