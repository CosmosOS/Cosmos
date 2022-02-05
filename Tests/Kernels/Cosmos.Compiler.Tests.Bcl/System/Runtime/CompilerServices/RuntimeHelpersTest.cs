using Cosmos.TestRunner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Compiler.Tests.Bcl.System.Runtime.CompilerServices
{
    struct NoReference
    {
        int x;
        int y;
    }

    struct Reference
    {
        int a;
        object b;
    }

    static class RuntimeHelpersTest
    {
        public static void Execute()
        {
            Assert.IsFalse(RuntimeHelpers.IsReferenceOrContainsReferences<int>(), "int is not and does not contain references");
            Assert.IsFalse(RuntimeHelpers.IsReferenceOrContainsReferences<NoReference>(), "NoReference struct is not and does not contain references");
            Assert.IsTrue(RuntimeHelpers.IsReferenceOrContainsReferences<object>(), "object is reference type");
            Assert.IsTrue(RuntimeHelpers.IsReferenceOrContainsReferences<Reference>(), "Reference contains a reference type");
        }
    }
}
