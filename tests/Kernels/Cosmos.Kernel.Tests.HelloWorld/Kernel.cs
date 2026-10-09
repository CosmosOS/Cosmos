using System;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.HelloWorld;

public class Kernel : Sys.Kernel
{
    protected override void BeforeRun()
    {
        Log.WriteString("[HelloWorld] BeforeRun() reached!\n");
        Log.WriteString("[HelloWorld] Starting tests...\n");

        // Initialize test suite
        TR.Start("HelloWorld Basic Tests", expectedTests: 3);

        // Test 1: Basic arithmetic
        TR.Run("Test_BasicArithmetic", () =>
        {
            int result = 2 + 2;
            Assert.Equal(4, result);
        });

        // Test 2: Boolean logic
        TR.Run("Test_BooleanLogic", () =>
        {
            bool isTrue = true;
            Assert.True(isTrue);
            Assert.False(!isTrue);
        });

        // Test 3: Integer comparison
        TR.Run("Test_IntegerComparison", () =>
        {
            int a = 10;
            int b = 10;
            int c = 20;

            Assert.Equal(a, b);
            Assert.True(a < c);
            Assert.False(a > c);
        });

        // Finish test suite
        TR.Finish();

        // Output completion message
        Log.WriteString("\n[Tests Complete - System Halting]\n");
    }

    protected override void Run()
    {
        // All tests ran in BeforeRun; stop the main loop after one iteration
        Stop();
    }

    protected override void AfterRun()
    {
        // Flush coverage data and signal QEMU to terminate
        TR.Complete();
        Cosmos.Kernel.System.Power.Halt();
    }
}
