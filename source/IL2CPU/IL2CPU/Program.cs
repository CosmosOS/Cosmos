using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Indy.IL2CPU;
using System.Threading;
using System.Xml;

namespace IL2CPU
{
    public enum LogChannelCommand : byte
    {
        /// <summary>
        /// Sends over a log message. data format &lt;command&gt;&lt;log severity&gt;&lt;message length (32 bit)&gt;&lt;message as utf16&gt;
        /// </summary>
        LogMessage = 0,
        /// <summary>
        /// Sends over status message for compiling methods. 
        /// data format &lt;command&gt;&lt;current (Int32)&gt;&lt;max (Int32)&gt;
        /// </summary>
        CompilingMethods = 1,
        /// <summary>
        /// Sends over status message for compiling static. 
        /// data format &lt;command&gt;&lt;current (Int32)&gt;&lt;max (Int32)&gt;
        /// </summary>
        CompilingFields = 2,
        /// <summary>
        /// Notifies the host that this is the last message before closing the communication channel
        /// </summary>
        EndOfProcessing = 3
    }
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {

                var xEngine = new Engine();
                string xAssembly = "";
                TargetPlatformEnum xTarget = TargetPlatformEnum.X86;
                List<string> xPlugs = new List<string>();
                string xOutputDir = "";
                DebugMode xDebugMode = DebugMode.None;
                string xLogPipeName = "CosmosCompileLog";
                byte xComport = 1;
                for (int i = 0; i < args.Length; i++)
                {
                    if (!args[i].StartsWith("/") && !args[i].StartsWith("-"))
                    {
                        throw new Exception("Wrong command-line argument: '" + args[i] + "'");
                    }
                    Console.WriteLine("Parsing command line option '{0}'", args[i].Substring(1));
                    switch (args[i].Substring(1).ToLowerInvariant())
                    {
                        case "trace":
                            {
                                xEngine.TraceAssemblies = (TraceAssemblies)Enum.Parse(typeof(TraceAssemblies),
                                                                                       args[i + 1],
                                                                                       true);
                                break;
                            }
                        case "assembly":
                            {
                                xAssembly = args[i + 1];
                                break;
                            }
                        case "target":
                            {
                                xTarget = (TargetPlatformEnum)Enum.Parse(typeof(TargetPlatformEnum),
                                                                          args[i + 1],
                                                                          true);
                                break;
                            }
                        case "plug":
                            {
                                xPlugs.Add(args[i + 1]);
                                break;
                            }
                        case "output":
                            {
                                xOutputDir = args[i + 1];
                                break;
                            }
                        case "comport":
                            {
                                xComport = Byte.Parse(args[i + 1]);
                                break;
                            }
                        case "logpipe":
                            {
                                xLogPipeName = args[i + 1];
                                break;
                            }
                        default:
                            throw new Exception("Argument not supported: '" + args[0] + "'");
                    }
                    i++;
                }
                if (File.Exists(@"d:\logdebug.xml"))
                {
                    File.Delete(@"d:\logdebug.xml");
                }
                //System.Diagnostics.Debugger.Launch();
                //using (var xLogStream = NamedPipeStream.Create(xLogPipeName, NamedPipeStream.ServerMode.Bidirectional)) {
                //using(var xLogStream = new System.IO.Pipes.NamedPipeClientStream(xLogPipeName)){
                //                xLogStream.Listen();
                using (var xLogStream = new NamedPipeStream(xLogPipeName, FileAccess.ReadWrite))
                {
                    //                xLogStream.Connect();
                    var xLogLock = new Object();
                    using (XmlWriter xWriter = XmlWriter.Create(@"d:\logdebug.xml"))
                    {
                        xWriter.WriteStartDocument();
                        xWriter.WriteStartElement("logdebug");
                        xEngine.CompilingMethods += delegate(int aCur,
                                                             int aMax)
                                                        {
                                                            lock (xLogLock)
                                                            {
                                                                var xData = new byte[9];
                                                                xData[0] = (byte)LogChannelCommand.CompilingMethods;
                                                                Array.Copy(BitConverter.GetBytes(aCur),
                                                                           0,
                                                                           xData,
                                                                           1,
                                                                           4);
                                                                Array.Copy(BitConverter.GetBytes(aMax),
                                                                           0,
                                                                           xData,
                                                                           5,
                                                                           4);
                                                                xWriter.WriteStartElement("Entry");
                                                                xWriter.WriteAttributeString("command",
                                                                                             xData[0].ToString());
                                                                xWriter.WriteAttributeString("current", aCur.ToString());
                                                                xWriter.WriteAttributeString("max", aMax.ToString());
                                                                xWriter.WriteEndElement();
                                                                xWriter.Flush();
                                                                xLogStream.Write(xData,
                                                                                 0,
                                                                                 9);
                                                                xLogStream.Flush();
                                                            }
                                                        };
                        xEngine.CompilingStaticFields += delegate(int aCur,
                                                                  int aMax)
                                                             {
                                                                 lock (xLogLock)
                                                                 {
                                                                     var xData = new byte[9];
                                                                     xData[0] = (byte)LogChannelCommand.CompilingFields;
                                                                     Array.Copy(BitConverter.GetBytes(aCur),
                                                                                0,
                                                                                xData,
                                                                                1,
                                                                                4);
                                                                     Array.Copy(BitConverter.GetBytes(aMax),
                                                                                0,
                                                                                xData,
                                                                                5,
                                                                                4);
                                                                     xWriter.WriteStartElement("Entry");
                                                                     xWriter.WriteAttributeString("command",
                                                                                                  xData[0].ToString());
                                                                     xWriter.WriteAttributeString("current",
                                                                                                  aCur.ToString());
                                                                     xWriter.WriteAttributeString("max", aMax.ToString());
                                                                     xWriter.WriteEndElement();
                                                                     xWriter.Flush();
                                                                     xLogStream.Write(xData,
                                                                                      0,
                                                                                      9);
                                                                     xLogStream.Flush();
                                                                 }
                                                             };
                        xEngine.DebugLog += delegate(LogSeverityEnum aSeverity,
                                                     string aMessage)
                                                {
                                                    lock (xLogLock)
                                                    {
                                                        var xData =
                                                            new byte[Encoding.Unicode.GetByteCount(aMessage) + 2 + 4];
                                                        xData[0] = (byte)LogChannelCommand.LogMessage;
                                                        xData[1] = (byte)aSeverity;
                                                        Array.Copy(
                                                            BitConverter.GetBytes(Encoding.Unicode.GetByteCount(aMessage)),
                                                            0,
                                                            xData,
                                                            2,
                                                            4);
                                                        Array.Copy(Encoding.Unicode.GetBytes(aMessage),
                                                                   0,
                                                                   xData,
                                                                   6,
                                                                   Encoding.Unicode.GetByteCount(aMessage));
                                                        xWriter.WriteStartElement("Entry");
                                                        xWriter.WriteAttributeString("command", xData[0].ToString());
                                                        xWriter.WriteAttributeString("length",
                                                                                     Encoding.Unicode.GetByteCount(
                                                                                         aMessage).ToString());
                                                        xWriter.WriteCData(aMessage);
                                                        xWriter.WriteEndElement();
                                                        xWriter.Flush();
                                                        xLogStream.Write(xData,
                                                                         0,
                                                                         xData.Length);
                                                        xLogStream.Flush();
                                                    }
                                                };
                        xEngine.Execute(xAssembly,
                                        xTarget,
                                        g => Path.Combine(xOutputDir,
                                                          g + ".asm"),
                                        xPlugs,
                                        xDebugMode,
                                        false,
                                        xComport,
                                        xOutputDir,
                                        false);
                        xLogStream.WriteByte((byte)LogChannelCommand.EndOfProcessing);
                        xLogStream.Flush();

                        xLogStream.Flush();
                        xWriter.WriteEndDocument();
                    }
                }


            }
            catch (Exception e)
            {
                Console.WriteLine("General error: " + e.ToString());
            }
        }
    }
}