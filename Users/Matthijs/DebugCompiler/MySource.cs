using System;
using System.Collections;
using System.Collections.Generic;
using Cosmos.TestRunner.Core;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DebugCompiler
{
    public static class MySource
    {
        public static IEnumerable<string> ProvideData()
        {
            return TestKernelSets.GetStableKernelTypes();
        }
    }
}
