using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Management.Automation.Runspaces;
using System.Net.Sockets;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
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

        private Process _vmClientProcess;

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
            _namedPipeClient = new NamedPipeClientStream(
                ".",
                "CosmosSerialHyperV",
                PipeDirection.InOut,
                PipeOptions.Asynchronous);

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
                PipeOptions.Asynchronous,
                256,
                256,
                pipeSecurity,
                HandleInheritability.None);

            var namedPipeConnectionTask = Task.Run(WaitForNamedPipeConnections);

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

            await namedPipeConnectionTask.ConfigureAwait(false);
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
            _vmClientProcess = Process.Start("vmconnect", $"\"localhost\" \"{vmName}\"");

            RunScript(
                StartVm,
                ("vmName", vmName));
        }

        private void StopVirtualMachine(string vmName)
        {
            if (!_vmClientProcess.HasExited)
            {
                _vmClientProcess.Kill();
            }

            RunScript(
                StopVm,
                ("vmName", vmName));
        }

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
            await Task.WhenAll(
                _namedPipeServer.WaitForConnectionAsync(),
                _namedPipeClient.ConnectAsync()
                ).ConfigureAwait(false);

            await Task.WhenAll(
                Task.Run(ReadPipeAsync),
                Task.Run(WritePipeAsync)
                ).ConfigureAwait(false);
        }

        private async Task ReadPipeAsync()
        {
            var buffer = new byte[256];

            while (_namedPipeClient.IsConnected
                && _namedPipeServer.IsConnected)
            {
                var byteCount = await _namedPipeClient.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

                if (byteCount > 0)
                {
                    await _namedPipeServer.WriteAsync(buffer, 0, byteCount).ConfigureAwait(false);
                }
                else
                {
                    await Task.Delay(500).ConfigureAwait(false);
                }
            }
        }

        private async Task WritePipeAsync()
        {
            var buffer = new byte[256];

            while (_namedPipeClient.IsConnected
                && _namedPipeServer.IsConnected)
            {
                var byteCount = await _namedPipeServer.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

                if (byteCount > 0)
                {
                    await _namedPipeClient.WriteAsync(buffer, 0, byteCount).ConfigureAwait(false);
                }
                else
                {
                    await Task.Delay(500).ConfigureAwait(false);
                }
            }
        }
    }
}
