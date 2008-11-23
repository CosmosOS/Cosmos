using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.Tests.AssemblerTests.X86 {
    partial class InvalidOpcodeTester {
        private static void AddExceptions() {
            opcodesException.Add(typeof(Not), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false }
            });
        }
    }
}
