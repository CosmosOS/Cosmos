using System;
using System.Collections;
using System.Collections.Generic;
using Cosmos.TestRunner.Core;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DebugCompiler
{
    public class MySource: IEnumerable<ITestFixtureData>
    {
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<ITestFixtureData> GetEnumerator()
        {
            yield return new TestFixtureData("A");
            yield return new TestFixtureData("B");
            yield return new TestFixtureData("C");
        }

        public static IEnumerable<Type> ProvideData()
        {
            return TestKernelSets.GetStableKernelTypes();
        }
    }
}
