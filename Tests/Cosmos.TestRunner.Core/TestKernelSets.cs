using System;
using System.Collections.Generic;

namespace Cosmos.TestRunner.Core
{
    public static class TestKernelSets
    {
        public static IEnumerable<Type> GetStableKernelTypes()
        {
            yield return typeof(VGACompilerCrash.Kernel);

            //yield return typeof(Cosmos.Compiler.Tests.Encryption.Kernel);
            yield return typeof(Cosmos.Compiler.Tests.Bcl.Kernel);
            yield return typeof(Cosmos.Compiler.Tests.SingleEchoTest.Kernel);
            yield return typeof(Cosmos.Compiler.Tests.SimpleWriteLine.Kernel.Kernel);
            yield return typeof(SimpleStructsAndArraysTest.Kernel);
            yield return typeof(Cosmos.Compiler.Tests.Exceptions.Kernel);
            yield return typeof(Cosmos.Compiler.Tests.LinqTests.Kernel);
            yield return typeof(Cosmos.Compiler.Tests.MethodTests.Kernel);

            yield return typeof(Cosmos.Kernel.Tests.Fat.Kernel);
            yield return typeof(Cosmos.Kernel.Tests.IO.Kernel);
            //yield return typeof(FrotzKernel.Kernel);
        }
    }
}
