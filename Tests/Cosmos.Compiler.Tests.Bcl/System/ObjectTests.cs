using System;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    public class ObjectTests
    {
        public class MyType
        {
            public int IntField;
            public object ReferenceField;

            public MyType Clone()
            {
                return (MyType)MemberwiseClone();
            }
        }

        public static void Execute()
        {
            var xMyType = new MyType();

            xMyType.IntField = 42;
            xMyType.ReferenceField = new object();

            var xCloneType = xMyType.Clone();
            
            Assert.AreEqual(xMyType.IntField, xCloneType.IntField, "Cloned object has a different IntField value!");
            Assert.IsTrue(ReferenceEquals(xMyType.ReferenceField, xCloneType.ReferenceField), "References of field aren't the same!");

            xCloneType.IntField = 56;

            Assert.AreEqual(xMyType.IntField, 42, "Cloned object is linked to original object!");
        }
    }
}
