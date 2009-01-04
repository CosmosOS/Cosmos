using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Builder;
using Indy.IL2CPU;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using CommandEnum = Cosmos.TestKernelHelpers.TestReporter.CommandEnum;
using System.Net;

namespace Cosmos.TestRunner {
    public static class Program {
        private static List<Type> TestKernels;

        private static void Initialize() {
            TestKernels = new List<Type>();
            TestKernels.Add(typeof(Cosmos.SimpleTest.Program));
        }

        public static void Main() {
            try {
                Initialize();
                // for now, just build all kernels.
                foreach (var xItem in TestKernels) {
                    //xItem.Assembly.Location
                    var xBuilder = new Builder() {
                        BuildPath = Options.BuildPath,
                        UseInternalAssembler = false
                    };
                    xBuilder.TargetAssembly = xItem.Assembly;
                    var xEvent = new AutoResetEvent(false);
                    xBuilder.CompileCompleted += delegate { xEvent.Set(); };
                    xBuilder.BeginCompile(DebugMode.None, 99, false);
                    xEvent.WaitOne();
                    xBuilder.Assemble();
                    xBuilder.Link();
                    xBuilder.MakeISO();
                    var xISOFile = Path.Combine(xBuilder.BuildPath, "Cosmos.iso");
                    // run qemu
                    RunKernel(xItem, xBuilder);
                }
            } catch (Exception E) {
                Console.WriteLine(E.ToString());
            } finally {
                Console.WriteLine("Done.");
                Console.ReadLine();
            }
        }

        private static void RunKernel(Type aType, Builder aBuilder) {
            //From v0.9.1 Qemu requires forward slashes in path
            var xBuildPath = aBuilder.BuildPath.Replace('\\', '/');
            var xTestEvent = new AutoResetEvent(false);
            var xTcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            xTcpServer.Bind(new IPEndPoint(IPAddress.Loopback, 8544));
            xTcpServer.Listen(10);
            using (var xDebug = new FileStream(@"d:\debug", FileMode.Create)) {
                #region test
                var xListenThread = new Thread(delegate() {
                    var xClientSocket = xTcpServer.Accept();
                    using (var xInputStream = new MemoryStream()) {
                        var xCurPosition = xInputStream.Position;
                        var xReceiveInput = new Action(delegate {
                            if (xClientSocket.Poll(5000, SelectMode.SelectRead) && xClientSocket.Available > 0) {
                                byte[] xRecvBuff = new byte[1024];
                                var xTempInt = xClientSocket.Receive(xRecvBuff);
                                xInputStream.Seek(0, SeekOrigin.End);
                                xInputStream.Write(xRecvBuff, 0, xTempInt);
                                xInputStream.Position = xCurPosition;
                                if (xTempInt > 0) {
                                    xDebug.Write(xRecvBuff, 0, xTempInt);
                                    xDebug.Flush();
                                    Console.WriteLine("Debug: BufferSize = {0}", xInputStream.Length);
                                }
                            }
                        });
                        var xReceiveString = new Func<string>(delegate {
                            xReceiveInput();
                            var xLengthBytes = new byte[4];
                            if (xInputStream.Read(xLengthBytes, 0, 4) != 4) {
                                throw new Exception("Not enough bytes read!");
                            }
                            int xLength = BitConverter.ToInt32(xLengthBytes, 0);
                            xReceiveInput();
                            var xStringBytes = new byte[xLength];
                            if (xInputStream.Read(xStringBytes, 0, xLength) != xLength) {
                                throw new Exception("WRong number of bytes read!");
                            }
                            return Encoding.ASCII.GetString(xStringBytes);
                        });
                        do {
                            if (xInputStream.Length > (xCurPosition + 4)) {
                                var xCommandBytes = new byte[4];
                                if (xInputStream.Read(xCommandBytes, 0, 4) != 4) {
                                    throw new Exception("Incorrect bytecount received!");
                                }
                                var xCommand = (CommandEnum)BitConverter.ToUInt32(xCommandBytes, 0);
                                switch (xCommand) {
                                    case CommandEnum.Initialized:
                                        Console.WriteLine("\tKernel Booted!");
                                        break;
                                    case CommandEnum.TestRunCompleted:
                                        Console.WriteLine("\tKernel Tests Completed!");
                                        xTestEvent.Set();
                                        return;
                                    case CommandEnum.TestCompleted:
                                        var xTest = xReceiveString();
                                        xReceiveString(); // for now discard description
                                        var xResult = xInputStream.ReadByte() == 1;
                                        break;
                                    case CommandEnum.String:
                                        Console.WriteLine("\t\tMessage = '{0}'", xReceiveString());
                                        break;
                                    default:
                                        throw new Exception("Command '" + xCommand + "' not handled!");
                                }
                            } else {
                                xReceiveInput();
                            }
                            Thread.Sleep(25);
                        } while (true);
                    }
                });
                #endregion
                xListenThread.IsBackground = true;
                xListenThread.Start();
                Thread.Sleep(250);
                var xProcess = Global.Call(aBuilder.ToolsPath + @"qemu\qemu.exe",
                    " -L ."
                    // CD ROM image
                    + " -cdrom \"" + aBuilder.BuildPath.Replace('\\', '/') + "Cosmos.iso\""
                    // Boot CD ROM
                    + " -boot d"
                    // Setup serial port
                    // Might allow serial file later for post debugging of CPU
                    // etc since serial to TCP on a byte level is likely highly innefficient
                    // with the packet overhead
                    //
                    // COM0 - used for test result reporting
                    + " -serial tcp:127.0.0.1:8544,nodelay"
                    , aBuilder.ToolsPath + @"qemu", false, true);
                while (!xProcess.HasExited) {
                    if (xTestEvent.WaitOne(1000)) {
                        break;
                    }
                }
                if (!xProcess.HasExited) {
                    xProcess.Kill();
                } else {
                    if (xProcess.ExitCode != 0) {
                        Console.WriteLine(xProcess.StandardError.ReadToEnd());
                        Console.WriteLine(xProcess.StandardOutput.ReadToEnd());
                    }
                }
            }
        }
    }
}