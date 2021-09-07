using System;

using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class RandomTests
    {
        public static void Execute()
        {
            var xRandom = new Random();
            var xRandomNumber = xRandom.Next();

            Assert.IsTrue(xRandomNumber >= 0, "Random.Next is returning a negative integer!");
            Assert.IsTrue(xRandomNumber != xRandom.Next(), "Two random numbers generated after each other are not the same");

            xRandomNumber = xRandom.Next(10);

            Assert.IsTrue(xRandomNumber >= 0 && xRandomNumber < 10, "Random.Next(int) is returning an integer outside of the specified range!");

            xRandomNumber = xRandom.Next(40, 45);

            Assert.IsTrue(xRandomNumber >= 40 && xRandomNumber < 45, "Random.Next(int, int) is returning an integer outside of the specified range!");

            double randomDouble = xRandom.NextDouble();
            Assert.IsTrue(randomDouble >= 0 && randomDouble <= 1 && !double.IsNaN(randomDouble), "Random.NextDouble works correctly");
        }
    }
}
