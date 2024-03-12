using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Cosmos.TestRunner.Core
{
    partial class Engine
    {
        protected int AllowedSecondsInKernel => mConfiguration.AllowedSecondsInKernel;
        protected IEnumerable<RunTargetEnum> RunTargets => mConfiguration.RunTargets;

        private bool ExecuteKernel(
            string assemblyFileName, string workingDirectory, RunConfiguration configuration, KernelTestResult aKernelTestResult)
        {
            OutputHandler.ExecuteKernelStart(aKernelTestResult.KernelName);

            var xAssemblyFile = Path.Combine(workingDirectory, "Kernel.asm");
            var xObjectFile = Path.Combine(workingDirectory, "Kernel.obj");
            var xTempObjectFile = Path.Combine(workingDirectory, "Kernel.o");
            var xIsoFile = Path.Combine(workingDirectory, "Kernel.iso");

            if (KernelPkg == "X86")
            {
                RunTask("TheRingMaster", () => RunTheRingMaster(assemblyFileName));
            }
            RunTask("IL2CPU", () => RunIL2CPU(assemblyFileName, xAssemblyFile));
            RunTask("Nasm", () => RunNasm(xAssemblyFile, xObjectFile, configuration.IsELF));
            if (configuration.IsELF)
            {
                File.Move(xObjectFile, xTempObjectFile);

                RunTask("Ld", () => RunLd(xTempObjectFile, xObjectFile));
                RunTask("ExtractMapFromElfFile", () => RunExtractMapFromElfFile(workingDirectory, xObjectFile));
            }

            string xHarddiskPath;
            if (configuration.RunTarget == RunTargetEnum.HyperV)
            {
                xHarddiskPath = Path.Combine(workingDirectory, "Harddisk.vhdx");
                var xOriginalHarddiskPath = Path.Combine(GetCosmosUserkitFolder(), "Build", "HyperV", "Filesystem.vhdx");
                File.Copy(xOriginalHarddiskPath, xHarddiskPath);
            }
            else
            {
                xHarddiskPath = Path.Combine(workingDirectory, "Harddisk.vmdk");
                var xOriginalHarddiskPath = Path.Combine(GetCosmosUserkitFolder(), "Build", "VMware", "Workstation", "FilesystemTest.vmdk");
                File.Copy(xOriginalHarddiskPath, xHarddiskPath);
            }

            RunTask("MakeISO", () => MakeIso(xObjectFile, xIsoFile));

            Console.WriteLine("assemblyFileName=" + assemblyFileName);

            if (assemblyFileName.EndsWith("NetworkTest.dll"))
            {
                var serverThread = new Thread(StartTcpServer);
                serverThread.Start();

                RunTask("RunISO", () => RunIsoInVMware(xIsoFile, xHarddiskPath));
            }
            else
            {
                switch (configuration.RunTarget)
                {
                    case RunTargetEnum.Bochs:
                        RunTask("RunISO", () => RunIsoInBochs(xIsoFile, xHarddiskPath, workingDirectory));
                        break;
                    case RunTargetEnum.Qemu:
                        RunTask("RunISO", () => RunIsoInQemu(xIsoFile, xHarddiskPath, workingDirectory));
                        break;
                    case RunTargetEnum.VMware:
                        RunTask("RunISO", () => RunIsoInVMware(xIsoFile, xHarddiskPath));
                        break;
                    case RunTargetEnum.HyperV:
                        RunTask("RunISO", () => RunIsoInHyperV(xIsoFile, xHarddiskPath));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("RunTarget " + configuration.RunTarget + " not implemented!");
                }
            }

            OutputHandler.ExecuteKernelEnd(assemblyFileName);

            return mKernelResult;
        }

        private void RunTask(string aTaskName, Action aAction)
        {
            if (aAction == null)
            {
                throw new ArgumentNullException(nameof(aAction));
            }

            OutputHandler.TaskStart(aTaskName);

            try
            {
                aAction();
            }
            catch(Exception e)
            {
                OutputHandler.LogError(e.ToString());
            }
            finally
            {
                OutputHandler.TaskEnd(aTaskName);
            }
        }

        private void StartTcpServer()
        {
            var listener = new TcpListener(IPAddress.Loopback, 12345);
            listener.Start();

            Console.WriteLine("TCP server started in a new thread, waiting connection from test kernel...");

            IPEndPoint localEndPoint = listener.LocalEndpoint as IPEndPoint;
            Console.WriteLine($"IP: {localEndPoint.Address}, Port: {localEndPoint.Port}");

            var client = listener.AcceptTcpClient();
            var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            var clientIPAddress = remoteEndPoint.Address;
            Console.WriteLine("Test kernel connected! Beginning tests...");

            IPAddress remoteIPAddress = null;

            using (NetworkStream stream = client.GetStream())
            {
                // Test 1: Send simple message
                string testMessage = "Hello from the testrunner!";
                byte[] messageBytes = Encoding.ASCII.GetBytes(testMessage);
                stream.Write(messageBytes, 0, messageBytes.Length);
                Console.WriteLine($"Sent: {testMessage}");

                // Test 2: Receive a message from kernel
                byte[] bufferIp = new byte[1024];
                int bytesIpRead = stream.Read(bufferIp, 0, bufferIp.Length);
                string ip = Encoding.ASCII.GetString(bufferIp, 0, bytesIpRead);
                remoteIPAddress = IPAddress.Parse(ip);
                Console.WriteLine($"Received: {ip}");

                // Test 2.2: Receive a message from kernel
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received: {receivedMessage}");

                // Test 3: Send back the received message, capitalized
                string replyMessage = receivedMessage.ToUpper();
                byte[] replyBytes = Encoding.ASCII.GetBytes(replyMessage);
                stream.Write(replyBytes, 0, replyBytes.Length);
                Console.WriteLine($"Sent: {replyMessage}");

                // Test 4: Receive a big packet from kernel to test TCP sequencing
                byte[] buffer2 = new byte[6000];
                int totalBytesRead = 0;

                while (totalBytesRead < 6000)
                {
                    totalBytesRead += stream.Read(buffer2, totalBytesRead, 6000 - totalBytesRead);
                }

                // Test 5: Send back the received message
                stream.Write(buffer2, 0, buffer2.Length);
                Console.WriteLine($"Sent: {replyMessage}");
            }

            client.Close();
            listener.Stop();

            ConnectToTcpServer(remoteIPAddress);
        }

        private void ConnectToTcpServer(IPAddress ip)
        {
            var xClient = new TcpClient();

            Console.WriteLine("Attempting to connect to the kernel...");

            try
            {
                // Test 6: Test TCPListener implementation
                xClient.Connect(ip, 4343);
                Console.WriteLine("Connected to the kernel!");

                using (NetworkStream stream = xClient.GetStream())
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received: {receivedMessage}");

                    string testMessage = "Hello from the testrunner again!";
                    byte[] messageBytes = Encoding.ASCII.GetBytes(testMessage);
                    stream.Write(messageBytes, 0, messageBytes.Length);
                    Console.WriteLine($"Sent: {testMessage}");
                }

                xClient.Close();
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Failed to connect to the kernel. Error: {ex.Message}");
            }
        }
    }
}
