using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Kernel;

namespace Cosmos.Shell.Console.Tests
{
    /// <summary>
    /// Represents a simple test case for the kernel/il2cpu.
    /// </summary>
    public abstract class TestBase
    {
        /// <summary>
        /// Gets the name of the test caes.
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Prepares the test case for use.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Stops the test case.
        /// </summary>
        public abstract void Teardown();

        /// <summary>
        /// Runs the test case.
        /// </summary>
        public abstract void Test();

        /// <summary>
        /// Makes sure a certain condition is true.
        /// </summary>
        protected bool Assert(bool condition, string message)
        {
            if (!condition)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.Write("TEST FAILED: ");
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.WriteLine(message);
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.Write("TEST SUCCEEDED: ");
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.WriteLine(message);
            }
            DebugUtil.SendTestAssert(condition, message);
            return condition;
        }

        /// <summary>
        /// Makes sure a certain condition is false.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected bool AssertFalse(bool condition, string message)
        {
            return !Assert(!condition, message);
        }
    }
}
