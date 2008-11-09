using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;

namespace Indy.IL2CPU.Tests.Assembler.X86 {
    public abstract class BaseTest {
        public Indy.IL2CPU.Assembler.X86.Assembler Assembler {
            get;
            private set;
        }

        [SetUp]
        protected void BeforeMethod() {
            Assembler = new Indy.IL2CPU.Assembler.X86.Assembler();
            Assembler.Initialize();
        }

        protected void Verify() {
            using (var xMS = new MemoryStream()) {
                Assembler.FlushBinary(xMS, 0x200000);
                xMS.Position = 0;
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
        }
    }
}