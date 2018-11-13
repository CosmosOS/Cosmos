using System;
using System.Collections.Generic;

namespace Cosmos.TestRunner.Full
{
    public static class TestKernelSets
    {
        // Kernel types to run: the ones that will run in test runners that use the default engine configuration.
        public static IEnumerable<Type> GetKernelTypesToRun()
        {
            //yield return typeof(KernelGen3.Boot);
            return GetStableKernelTypes();
        }

        // Stable kernel types: the ones that are stable and will run in AppVeyor
        public static IEnumerable<Type> GetStableKernelTypes()
        {
            yield return typeof(BoxingTests.Kernel);
            yield return typeof(Cosmos.Compiler.Tests.TypeSystem.Kernel);
            yield return typeof(Cosmos.Compiler.Tests.Bcl.Kernel);
            //yield return typeof(Cosmos.Compiler.Tests.Encryption.Kernel);
            yield return typeof(Cosmos.Compiler.Tests.Exceptions.Kernel);
            yield return typeof(Cosmos.Compiler.Tests.MethodTests.Kernel);
            yield return typeof(Cosmos.Compiler.Tests.SingleEchoTest.Kernel);
            yield return typeof(Cosmos.Kernel.Tests.Fat.Kernel);
            yield return typeof(Cosmos.Kernel.Tests.IO.Kernel);
            yield return typeof(SimpleStructsAndArraysTest.Kernel);
            yield return typeof(Cosmos.Kernel.Tests.DiskManager.Kernel);

            //yield return typeof(KernelGen3.Boot);

            /* Please see the notes on the kernel itself before enabling it */
            //yield return typeof(GraphicTest.Kernel);
            /* Please see the notes on the kernel itself before enabling it */
            //yield return typeof(ConsoleTest.Kernel);
            /* This is a bit slow and works only because ring check is disabled to decide if leave it enabled */
            yield return typeof(MemoryOperationsTest.Kernel);
        }
    }
}
