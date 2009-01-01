using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;

namespace Indy.IL2CPU.Tests.AssemblerTests.X86 {
    public abstract class BaseTest {
        public Indy.IL2CPU.Assembler.X86.Assembler Assembler {
            get;
            private set;
        }

        [SetUp]
        protected void BeforeMethod() {
            Assembler = new Indy.IL2CPU.Assembler.X86.Assembler();
            Assembler.Initialize();
            Assembler.Instructions.Clear();
            Assembler.DataMembers.Clear();
        }
        private static string mNasmPath;
        static BaseTest() {
            mNasmPath = Directory.GetCurrentDirectory();
            int xPos = mNasmPath.LastIndexOf("source", StringComparison.InvariantCultureIgnoreCase);
            if (xPos != -1) {
                mNasmPath = mNasmPath.Substring(0, xPos) + @"Build\Tools\NAsm\nasm.exe";
            } else {
                mNasmPath = Directory.GetCurrentDirectory();
                xPos = mNasmPath.LastIndexOf("BuildOutput", StringComparison.InvariantCultureIgnoreCase);
                mNasmPath = mNasmPath.Substring(0, xPos) + @"Build\Tools\NAsm\nasm.exe";
            }
        }

        /*protected void Verify() {
            if (Assembler.Instructions.Count == 0) {
                return;
            }
            using (var xMS = new MemoryStream()) {
                string xManStreamName = typeof(BaseTest).Namespace;
                xManStreamName += ".VerificationData.";
                xManStreamName += this.GetType().FullName.Substring(typeof(BaseTest).Namespace.Length + 1);
                var xStack = new StackTrace(1,
                                            false);
                var xName = xStack.GetFrame(0).GetMethod().Name;
                if (xName.StartsWith("Test")) {
                    xName = xName.Substring(4);
                }
                xManStreamName += "." + xName;
                if(typeof(BaseTest).Assembly.GetManifestResourceNames().Contains(xManStreamName + ".asm")) {
                    using (var xOut = new StringWriter()) {
                        Assembler.FlushText(xOut);
                        using (var xReader = new StreamReader(typeof(BaseTest).Assembly.GetManifestResourceStream(xManStreamName + ".asm"))) {
                            Assert.AreEqual(xReader.ReadToEnd(), xOut.ToString());
                        }
                    }
                }
                using (var xVerificationData = typeof(BaseTest).Assembly.GetManifestResourceStream(xManStreamName + ".bin")) {
                    if (xVerificationData == null) {
                        Assert.Fail("VerificationData not found! Did you forget to mark the file as Embedded Resource?");
                    }
                    Assembler.FlushBinary(xMS, 0x200000);
                    xMS.Position = 0;
                    xVerificationData.Position = 0;
                    if (xVerificationData.Length != xMS.Length) { 
                        Assert.Fail("Wrong data emitted");
                    }
                    while (true) {
                        var xVerData = xVerificationData.ReadByte();
                        var xActualData = xMS.ReadByte();
                        if (xVerData != xActualData) {
                            Assert.Fail("Wrong data emitted");
                        }
                        if (xVerData == -1) {
                            break;
                        }
                    }
                }
            }
        }*/

        protected void Verify() {
            if (Assembler.Instructions.Count == 0) {
                return;
            }
            var xAsmPath = Path.GetTempFileName();
            var xCBinPath = Path.GetTempFileName();
            var xNBinPath = Path.GetTempFileName();
            var xErrorPath = Path.GetTempFileName();
            try {
                using (var xNasmWriter = new StreamWriter(xAsmPath, false)) {
                    FileStream xNasmReader = null;
                    bool xResult = false;
                    string xMessage = null;
                    using (var xIndy86MS = new FileStream(xCBinPath, FileMode.Create)) {
                        Assembler.FlushBinary(xIndy86MS, 0x200000);
                        Assembler.FlushText(xNasmWriter);
                        xNasmWriter.Flush();
                        if (xNasmWriter.BaseStream.Length == 0) {
                            throw new Exception("No content found");
                        }
                        xNasmWriter.Close();
                        ProcessStartInfo pinfo = new ProcessStartInfo();
                        pinfo.Arguments = "\"" + xAsmPath + "\"" + " -o " + "\"" + xNBinPath + "\"";
                        //                    var xErrorFile = Path.Combine(tempPath, "TheOutput.err");
                        pinfo.Arguments += " -E\"" + xErrorPath + "\"";
                        //if (File.Exists(xErrorFile)) {
                        //    File.Delete(xErrorFile);
                        //}
                        //pinfo.WorkingDirectory = tempPath;
                        pinfo.FileName = mNasmPath;
                        pinfo.UseShellExecute ^= true;
                        pinfo.CreateNoWindow = true;
                        pinfo.RedirectStandardOutput = true;
                        pinfo.RedirectStandardError = true;
                        Process xProc = Process.Start(pinfo);
                        xProc.WaitForExit();
                        if (xProc.ExitCode != 0) {
                            xResult = false;
                            Assert.Fail("Error while invoking nasm.exe");
                        } else {
                            xNasmReader = File.OpenRead(xNBinPath);
                            xIndy86MS.Position = 0;
                            if (xNasmReader.Length != xIndy86MS.Length) {
                                xNasmReader.Close();
                                Assert.Fail("Binary size mismatch");
                            }
                            while (true) {
                                var xVerData = xNasmReader.ReadByte();
                                var xActualData = xIndy86MS.ReadByte();
                                if (xVerData != xActualData) {
                                    xNasmReader.Close();
                                    Assert.Fail("Binary data mismatch");
                                }
                                if (xVerData == -1) {
                                    break;
                                }
                            }
                            xNasmReader.Close();
                            xResult = true;
                        }
                    }
                }
            } finally {
                File.Delete(xAsmPath);
                File.Delete(xNBinPath);
                File.Delete(xCBinPath);
                File.Delete(xErrorPath);
            }
        }
    }
}