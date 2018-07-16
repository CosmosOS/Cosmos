using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Management.Automation.Runspaces;
using System.Net.Sockets;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Cosmos.Debug.HyperVServer
{
    internal sealed class Client : IDisposable
    {
        private const string CreateVm = "CreateVm.ps1";
        private const string RemoveVm = "RemoveVm.ps1";
        private const string StartVm = "StartVm.ps1";
        private const string StopVm = "StopVm.ps1";

        private readonly Runspace _runspace;
        private readonly TcpClient _tcpClient;

        private NamedPipeClientStream _namedPipeClient;
        private NamedPipeServerStream _namedPipeServer;

        public Client(TcpClient tcpClient)
        {
            _runspace = RunspaceFactory.CreateRunspace();
            _runspace.Open();

            _tcpClient = tcpClient;
        }

        public async Task RunAsync()
        {
            _namedPipeClient = new NamedPipeClientStream(".", "CosmosSerial1", PipeDirection.InOut);

            var pipeSecurity = new PipeSecurity();

            var identity = WindowsIdentity.GetCurrent().User;

            var pipeAccessRule = new PipeAccessRule(
                identity,
                PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance,
                AccessControlType.Allow);

            pipeSecurity.AddAccessRule(pipeAccessRule);
            pipeSecurity.SetOwner(identity);

            _namedPipeServer = new NamedPipeServerStream(
                "CosmosSerial",
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.None,
                256,
                256,
                pipeSecurity,
                HandleInheritability.None);

            _ = Task.Run(WaitForNamedPipeConnections);

            using (var reader = new StreamReader(_tcpClient.GetStream()))
            {
                while (_tcpClient.Connected)
                {
                    if (_tcpClient.Available > 0)
                    {
                        var command = await reader.ReadLineAsync().ConfigureAwait(false);
                        var parts = command.Split(';');

                        switch (parts[0])
                        {
                            case "CreateVirtualMachine":
                                CreateVirtualMachine(parts[1], parts[2], parts[3]);
                                break;
                            case "RemoveVirtualMachine":
                                RemoveVirtualMachine(parts[1]);
                                break;
                            case "StartVirtualMachine":
                                StartVirtualMachine(parts[1]);
                                break;
                            case "StopVirtualMachine":
                                StopVirtualMachine(parts[1]);
                                break;
                        }

                        reader.BaseStream.WriteByte(0x20);
                    }
                    else
                    {
                        await Task.Delay(1000).ConfigureAwait(false);
                    }
                }
            }
        }

        public void Dispose()
        {
            _runspace.Dispose();
            _tcpClient.Dispose();

            _namedPipeClient.Dispose();
            _namedPipeServer.Dispose();
        }

        private void CreateVirtualMachine(string vmName, string hardDiskPath, string isoPath) =>
           RunScript(
               CreateVm,
               ("vmName", vmName),
               ("hardDiskPath", hardDiskPath),
               ("isoPath", isoPath));

        private void RemoveVirtualMachine(string vmName) =>
            RunScript(
                RemoveVm,
                ("vmName", vmName));

        private void StartVirtualMachine(string vmName)
        {
            Process.Start("vmconnect", $"\"localhost\" \"{vmName}\"");

            RunScript(
                StartVm,
                ("vmName", vmName));
        }

        private void StopVirtualMachine(string vmName) =>
            RunScript(
                StopVm,
                ("vmName", vmName));

        private void RunScript(string scriptName, params (string Name, object Value)[] parameters)
        {
            using (var pipeline = _runspace.CreatePipeline())
            {
                var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var scriptPath = Path.Combine(assemblyDirectory, "scripts", scriptName);
                var command = new Command(File.ReadAllText(scriptPath), true);

                foreach (var (name, value) in parameters)
                {
                    command.Parameters.Add(name, value);
                }

                pipeline.Commands.Add(command);

                _ = pipeline.Invoke();
            }
        }

        private async Task WaitForNamedPipeConnections()
        {
            await _namedPipeServer.WaitForConnectionAsync().ConfigureAwait(false);
            await _namedPipeClient.ConnectAsync().ConfigureAwait(false);

            await ConnectPipesAsync().ConfigureAwait(false);
        }

        private async Task ConnectPipesAsync()
        {
            var buffer = new byte[256];

            while (true)
            {
                var readByteCount = await _namedPipeClient.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

                if (readByteCount > 0)
                {
                    _namedPipeServer.Write(buffer, 0, readByteCount);
                }

                var writtenByteCount = await _namedPipeServer.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

                if (writtenByteCount > 0)
                {
                    _namedPipeClient.Write(buffer, 0, writtenByteCount);
                }

                await Task.Delay(100).ConfigureAwait(false);
            }
        }
    }
}
