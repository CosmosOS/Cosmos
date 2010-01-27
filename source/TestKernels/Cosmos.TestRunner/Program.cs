using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;
using Cosmos.Compiler.Builder;
using CommandEnum = Cosmos.TestKernelHelpers.TestReporter.CommandEnum;
using Cosmos.IL2CPU;
using Cosmos.Build.Common;

namespace Cosmos.TestRunner
{
    public static class Program
    {
        private static List<KeyValuePair<Type, int>> TestKernels;

        private static void Initialize()
        {
            TestKernels = new List<KeyValuePair<Type, int>>();
            TestKernels.Add(new KeyValuePair<Type, int>(typeof(Cosmos.SimpleTest.Program), 2));
        }

        private class TestResults
        {
            public string Name;
            public string Message;
            public bool Succeeded;

        }

        /// <summary>
        /// Determines if the test runner should run the kernel using qemu, or only compile (Basically useful for profiling purposes)
        /// </summary>
        private const bool NeedsToRunKernel = true;
        public static void Main()
        {
            try
            {
                Initialize();
                var xBuildOptions = BuildOptions.Load();
                List<TestResults> xResults = new List<TestResults>();
                foreach (var xItem in TestKernels)
                {
                    //xItem.Assembly.Location
                    string xMessage;
                    bool xReturn;
                    try
                    {
                        var xBuilder = new Builder();
                        Console.WriteLine("BuildPath = '{0}'", xBuilder.BuildPath);
                        xBuilder.TargetAssembly = xItem.Key.Assembly;
                        var xEvent = new AutoResetEvent(false);
                        xBuilder.BuildCompleted += delegate { xEvent.Set(); };
                        xBuilder.LogMessage += delegate(LogSeverityEnum aSeverity, string aMessage)
                        {
                            Console.WriteLine("Log: {0} - {1}", aSeverity, aMessage);
                        };

                        var options = Cosmos.Compiler.Builder.BuildOptions.Load();

                        options.DebugMode = DebugMode.None;
                        options.DebugPortId = 0;
                        options.UseGDB = false;

                        options.CompileIL = true; 
                        options.UseInternalAssembler = false; // force externel assemble and link
                        
                        options.Target = "ISO"; 

                        xBuilder.BeginCompile(options);

                        //  xBuilder.BeginCompile(options);
                        xEvent.WaitOne();
                        if (NeedsToRunKernel)  //HACK! always false hardcode
                        {

                            //new AssembleStep(options).Execute();
                            //new LinkStep(options).Execute();


                            new MakeISOStep(options).Execute();

                            //var xISOFile = Path.Combine(xBuilder.BuildPath, "Cosmos.iso");
                            // run qemu
                            xReturn = RunKernel(xItem.Key, xBuilder, xItem.Value, out xMessage);
                        }
                        else
                        {
                            xReturn = true;
                            xMessage = "";
                        }
                    }
                    catch (Exception E)
                    {
                        xMessage = E.ToString();
                        xReturn = false;
                    }
                    xResults.Add(new TestResults
                    {
                        Name = xItem.Key.Assembly.GetName().Name,
                        Message = xMessage,
                        Succeeded = xReturn
                    });
                }
                WriteResults(xResults);
                Console.WriteLine("Finished Test:" + DateTime.Now.ToLongTimeString()); 
            }
            catch (Exception E)
            {
                Console.WriteLine(E.ToString());
            }
        }

        private static void WriteResults(List<TestResults> results)
        {
            using (var xOut = XmlWriter.Create(Path.Combine(Environment.CurrentDirectory, "TestKernel-results.xml")))
            {
                xOut.WriteStartDocument(false);
                xOut.WriteStartElement("test-results");
                {
                    xOut.WriteAttributeString("name", "CosmosTestKernels");
                    xOut.WriteAttributeString("total", results.Count.ToString());
                    xOut.WriteAttributeString("failures", (from item in results
                                                           where !item.Succeeded
                                                           select item).Count().ToString());
                    xOut.WriteAttributeString("not-run", "0");
                    xOut.WriteAttributeString("date", DateTime.Now.ToString("yyyy-MM-dd"));
                    xOut.WriteAttributeString("time", DateTime.Now.ToString("hh:mm:ss"));
                    xOut.WriteStartElement("environment");
                    {
                        xOut.WriteAttributeString("nunit-version", "10.0.0.0");
                        xOut.WriteAttributeString("clr-version", "2.0.50727.3053");
                        xOut.WriteAttributeString("os-version", "Cosmos");
                        xOut.WriteAttributeString("platform", "Win32NT");
                        xOut.WriteAttributeString("cwd", Environment.CurrentDirectory);
                        xOut.WriteAttributeString("machine-name", "cosmos");
                        xOut.WriteAttributeString("user", "cosmos");
                        xOut.WriteAttributeString("user-domain", "cosmos");
                    }
                    xOut.WriteEndElement(); // environment
                    xOut.WriteStartElement("culture-info");
                    {
                        xOut.WriteAttributeString("current-culture", "en-US");
                        xOut.WriteAttributeString("current-uiculture", "en-US");
                    }
                    xOut.WriteEndElement(); // culture-info
                    xOut.WriteStartElement("test-suite");
                    {
                        xOut.WriteAttributeString("name", "CosmosTestKernels");
                        xOut.WriteAttributeString("success", (!(from item in results
                                                                where !item.Succeeded
                                                                select item).Any()).ToString());
                        xOut.WriteAttributeString("time", "0.000");
                        xOut.WriteAttributeString("asserts", results.Count.ToString());
                        xOut.WriteStartElement("results");
                        {
                            foreach (var xItem in results)
                            {
                                xOut.WriteStartElement("test-case");
                                {
                                    xOut.WriteAttributeString("name", xItem.Name);
                                    xOut.WriteAttributeString("executed", "True");
                                    xOut.WriteAttributeString("success", xItem.Succeeded.ToString());
                                    xOut.WriteAttributeString("time", "0.000");
                                    xOut.WriteAttributeString("asserts", "1");
                                    if (!xItem.Succeeded)
                                    {
                                        xOut.WriteStartElement("failure");
                                        {
                                            xOut.WriteStartElement("message");
                                            {
                                                xOut.WriteCData(xItem.Message);
                                            }
                                            xOut.WriteEndElement(); // message
                                            xOut.WriteStartElement("stack-trace");
                                            {
                                                xOut.WriteCData("(No stack information available)");
                                            }
                                            xOut.WriteEndElement(); // stack-trace
                                        }
                                        xOut.WriteEndElement(); // failure
                                    }
                                } xOut.WriteEndElement();// test-case
                            }
                        }
                        xOut.WriteEndElement();// results
                    }
                    xOut.WriteEndElement(); // test-suite
                } xOut.WriteEndElement(); // test results
            }
        }

        private static bool RunKernel(Type aType, Builder aBuilder, int aExpectedTests, out string aInfo)
        {
            //From v0.9.1 Qemu requires forward slashes in path
            String xBuildPath = aBuilder.BuildPath.Replace('\\', '/');
            AutoResetEvent xTestEvent = new AutoResetEvent(false);
            aInfo = "";
            int xTestsSucceeded = 0;
            int xTestsFailed = 0;
            Socket xClientSocket = null;

            #region TestThread
            var xListenThread = new Thread(delegate()
            {
                using (var xInputStream = new MemoryStream())
                {
                    var xCurPosition = xInputStream.Position;
                    var xReceiveInput = new Action(delegate
                    {
                        if (xClientSocket.Poll(5000, SelectMode.SelectRead) && xClientSocket.Available > 0)
                        {
                            byte[] xRecvBuff = new byte[1024];
                            var xTempInt = xClientSocket.Receive(xRecvBuff);
                            xInputStream.Seek(0, SeekOrigin.End);
                            xInputStream.Write(xRecvBuff, 0, xTempInt);
                            xInputStream.Position = xCurPosition;
                        }
                    });
                    var xReceiveString = new Func<string>(delegate
                    {
                        xReceiveInput();
                        var xLengthBytes = new byte[4];
                        if (xInputStream.Read(xLengthBytes, 0, 4) != 4)
                        {
                            throw new Exception("Not enough bytes read!");
                        }
                        int xLength = (int) BitConverter.ToUInt32(xLengthBytes, 0);
                        xReceiveInput();
                        var xStringBytes = new byte[xLength];
                        if (xInputStream.Read(xStringBytes, 0, xLength) != xLength)
                        {
                            throw new Exception("WRong number of bytes read!");
                        }
                        return Encoding.ASCII.GetString(xStringBytes);
                    });
                    do
                    {
                        if (xInputStream.Length > (xCurPosition + 4))
                        {
                            var xCommandBytes = new byte[4];
                            if (xInputStream.Read(xCommandBytes, 0, 4) != 4)
                            {
                                throw new Exception("Incorrect bytecount received!");
                            }
                            var xCommand = (CommandEnum)BitConverter.ToUInt32(xCommandBytes, 0);
                            switch (xCommand)
                            {
                                case CommandEnum.Initialized:
                                    Console.WriteLine("\tKernel Booted!");
                                    break;
                                case CommandEnum.TestRunCompleted:
                                    Console.WriteLine("\tKernel Tests Completed!");

                                    xTestEvent.Set();
                                    return;
                                case CommandEnum.TestCompleted:
                                    String xTest = xReceiveString();
                                    String xDescr = xReceiveString();

                                    var xResult = xInputStream.ReadByte() == 1;
                                    Console.WriteLine("\t" + xTest + " : " + xDescr + Environment.NewLine + "\tExecution Result : " + Convert.ToBoolean(xResult));
                                    if (xResult)
                                        xTestsSucceeded++;
                                    else
                                        xTestsFailed++;
                                    break;
                                case CommandEnum.String:
                                    Console.WriteLine("\t\tMessage = '{0}'", xReceiveString());
                                    break;
                                default:
                                    throw new Exception("Command '" + xCommand + "' not handled!");
                            }
                        }
                        else
                        {
                            xReceiveInput();
                        }
                        Thread.Sleep(25);
                    } while (true);
                }
            });
            #endregion
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
                + " -serial tcp:127.0.0.1:8544,server "
                , aBuilder.ToolsPath + @"qemu", false, true);

            System.Threading.Thread.Sleep(500);  //give it time to launch

            // Variable to tell how many times we tried to connect to the serial port of QEMU
            Int32 Tries = 0;
            // Create the socket.
            xClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            do
            {
                try
                {
                    // Try to connect to the endpoint.
                    xClientSocket.Connect(new IPEndPoint(IPAddress.Loopback, 8544));
                }
                catch (SocketException e)
                {
                    // If the connection goes wrong, show the error message, increment 
                    // Tries and sleep for 10 seconds.
                    Console.WriteLine("\tConnection to the Serial port failed. We have tried " + Tries.ToString() + " times." + Environment.NewLine + e.Message);
                    Tries++;
                    Thread.Sleep(5000);
                }
                // It tries to connect until it is connected or 50 seconds have passed from the first try.
            } while ((!xClientSocket.Connected) && (Tries < 5));

            // If it is connected...
            if (xClientSocket.Connected)
            {
                // ...show the message and start the test thread.
                Console.WriteLine("\tConnected to the serial port after " + Tries.ToString() + " tries");
                xListenThread.Start();
            }
            //...otherwise...
            else
            {
                // ...kill QEMU and close the app.
                xProcess.Kill();
                return false;
            }

            // Wait for 120 seconds until all the tests are completed.
            // The completion is checked through an AutoResetEvent.
            Boolean ExitedNormally = false;
            int xTimeout = 120000;
            while ((ExitedNormally == false) && (xTimeout > 0))
            {
                if (xTestEvent.WaitOne(1000))
                    ExitedNormally = true;
                xTimeout -= 1000;
            }

            // If some error caused the QEmu Process to be closed...
            if (xProcess.HasExited)
            {
                // ...write everything that happened on the window and close the app.
                Console.WriteLine(xProcess.StandardError.ReadToEnd());
                Console.WriteLine(xProcess.StandardOutput.ReadToEnd());
                return false;
            }
            else
            {
                // If the QEmu process after 120 seconds still haven't completed the tests...
                if (!ExitedNormally)
                {
                    // ...something has gone wrong, so kill the process and close the app showing a message of error.
                    xProcess.Kill();
                    Console.WriteLine("Error : The QEmu process is not closed after 120 seconds of wait.");
                    return false;
                }
                // ...otherwise...
                else
                {
                    // Kill the process (QEmu does not close by itself even if the virtual machine is halted)
                    xProcess.Kill();

                    String OperationResult = String.Format("{0} tests expected, {1} tests succeeded, {2} tests failed", aExpectedTests, xTestsSucceeded, xTestsFailed);
                    Console.WriteLine(OperationResult);
                    if (xTestsFailed > 0)
                    {
                        aInfo = String.Format("{0} tests succeeded, {1} tests failed", xTestsSucceeded, xTestsFailed);
                        return false;
                    }
                    else
                    {
                        if (xTestsSucceeded != aExpectedTests)
                        {
                            aInfo = String.Format("{0} tests expected, {1} tests ran", aExpectedTests, xTestsSucceeded);
                            return false;
                        }
                        return true;
                    }
                }
            }
        }
    }
}