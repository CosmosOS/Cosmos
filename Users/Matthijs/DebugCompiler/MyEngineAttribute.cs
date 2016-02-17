using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace DebugCompiler
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MyEngineAttribute: NUnitAttribute, IFixtureBuilder
    {
        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo)
        {
            throw new System.NotImplementedException();
        }
    }
}
