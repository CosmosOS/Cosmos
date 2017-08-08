using System;
using System.Collections;
using System.Collections.Generic;
using Cosmos.TestRunner.Core;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Cosmos.TestRunner.UnitTest
{
    public static class MySource
    {
        public static IEnumerable<Type> ProvideData()
        {
            return TestKernelSets.GetStableKernelTypes();
        }
    }
}
