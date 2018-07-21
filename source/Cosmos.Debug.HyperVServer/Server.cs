using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;

namespace Cosmos.Debug.HyperVServer
{
    internal sealed class Server
    {
        private readonly object _syncObject = new object();

        private bool _isRunning;

        private int _connectedClients;
        private Timer _timer;

        public Server()
        {
            _timer = new Timer(20 * 60 * 1000); // 20 minutes
            _timer.Elapsed += Timer_Elapsed;
        }

        public async Task<int> RunAsync()
        {
            _isRunning = true;

            _timer.Start();

            var listener = new TcpListener(IPAddress.Loopback, 4534);
            listener.Start();

            while (_isRunning)
            {
                var tcpClient = await listener.AcceptTcpClientAsync().ConfigureAwait(false);

                _ = Task.Run(
                    async () =>
                    {
                        while (tcpClient.Connected)
                        {
                            if (tcpClient.Available >= 4)
                            {
                                var buffer = new byte[4];
                                tcpClient.GetStream().Read(buffer, 0, buffer.Length);

                                if (buffer[0] == 0xFF
                                 && buffer[1] == 0xEC
                                 && buffer[2] == 0x34
                                 && buffer[3] == 0xFC)
                                {
                                    break;
                                }
                                else
                                {
                                    return;
                                }
                            }

                            await Task.Delay(1000).ConfigureAwait(false);
                        }

                        OnClientConnected();

                        using (var client = new Client(tcpClient))
                        {
                            await client.RunAsync().ConfigureAwait(false);
                        }

                        OnClientDisconnected();
                    });
            }

            return 0;
        }

        private void OnClientConnected()
        {
            lock (_syncObject)
            {
                if (_connectedClients == 0)
                {
                    _timer.Stop();
                    _isRunning = true;
                }

                _connectedClients++;
            }
        }

        private void OnClientDisconnected()
        {
            lock (_syncObject)
            {
                _connectedClients--;

                if (_connectedClients == 0)
                {
                    _timer.Start();
                }
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_syncObject)
            {
                _isRunning = false;
            }
        }
    }
}
