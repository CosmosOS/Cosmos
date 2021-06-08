using Cosmos.TestRunner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class ObjectTest
    {
        public static void Execute()
        {
            object a = new object();
            Assert.IsFalse(a == null, "a is not null using equality");
            Assert.IsFalse(a.Equals(null), "a is not null using Equals");

            object b = new object();
            Assert.IsFalse(a == b, "a is not b using equality");
            Assert.IsFalse(a.Equals(b), "a is not b using equality");

            object c = a;
            Assert.IsTrue(a == a, "a == a");
            Assert.IsTrue(a == c, "a == c");
            Assert.IsTrue(a.Equals(a), "a.Equals(a)");
            Assert.IsTrue(a.Equals(c), "a.Equals(c)");
        }
    }
}
