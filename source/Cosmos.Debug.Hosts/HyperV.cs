using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

using Cosmos.Build.Common;

namespace Cosmos.Debug.Hosts
{
    public class HyperV : Host
    {
        private readonly string _vmName;
        private readonly string _hardDiskPath;

        private readonly TcpClient _client;
        private readonly StreamWriter _streamWriter;

        private bool _isVmCreated;

        public HyperV(Dictionary<string, string> aParams, bool aUseGDB, string harddisk = "Filesystem.vhdx")
            : base(aParams, aUseGDB)
        {
            _vmName = "Cosmos-" + Guid.NewGuid().ToString();

            if (Process.GetProcessesByName("Cosmos.Debug.HyperVServer").Length == 0)
            {
                var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var serverPath = Path.Combine(assemblyDirectory, @"HyperVServer\Cosmos.Debug.HyperVServer.exe");

                Process.Start(new ProcessStartInfo(serverPath) { UseShellExecute = true });
            }

            _client = new TcpClient();
            _client.Connect(IPAddress.Loopback, 4534);

            _client.GetStream().Write(new byte[] { 0xFF, 0xEC, 0x34, 0xFC }, 0, 4);

            _streamWriter = new StreamWriter(_client.GetStream());

            _hardDiskPath = Path.Combine(CosmosPaths.Build, "HyperV", harddisk);
        }

        public override void Start() =>
            Task.Run(
                () =>
                {
                    if (!_isVmCreated)
                    {
                        _isVmCreated = true;
                        SendCommand("CreateVirtualMachine", _vmName, _hardDiskPath, mParams["ISOFile"]);
                    }

                    SendCommand("StartVirtualMachine", _vmName);
                });

        public override void Stop() =>
            Task.Run(
                () =>
                {
                    SendCommand("StopVirtualMachine", _vmName);
                    SendCommand("RemoveVirtualMachine", _vmName);

                    _client.Close();

                    _streamWriter.Dispose();
                    _client.Dispose();
                });

        private void SendCommand(string commandName, params string[] args)
        {
            _streamWriter.WriteLine(String.Join(";", Args()));
            _streamWriter.Flush();

            if (_streamWriter.BaseStream.ReadByte() != 0x20)
            {
                throw new Exception("Server error!");
            }

            IEnumerable<string> Args()
            {
                yield return commandName;

                foreach (var arg in args)
                {
                    yield return arg;
                }
            }
        }
    }
}
