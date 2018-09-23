using System;

using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    internal static class TimeSpanTests
    {
        public static void Execute()
        {
            TimeSpan twoDaysTimeSpan = TimeSpan.FromDays(2);

            Assert.IsTrue(twoDaysTimeSpan.Ticks == 2.0 * 24.0 * 60.0 * 60.0 * 1000.0 * 10000.0, "TimeSpan.FromDays() is not working");

            Assert.IsTrue(twoDaysTimeSpan.TotalDays == 2.0, "TimeSpan.TotalDays is not working");
            Assert.IsTrue(twoDaysTimeSpan.TotalHours == 2.0 * 24.0, "TimeSpan.TotalHours is not working");
            Assert.IsTrue(twoDaysTimeSpan.TotalMinutes == 2.0 * 24.0 * 60.0, "TimeSpan.TotalMinutes is not working");
            Assert.IsTrue(twoDaysTimeSpan.TotalSeconds == 2.0 * 24.0 * 60.0 * 60.0, "TimeSpan.TotalSeconds is not working");
            Assert.IsTrue(twoDaysTimeSpan.TotalMilliseconds == 2.0 * 24.0 * 60.0 * 60.0 * 1000.0, "TimeSpan.TotalMilliseconds is not working");

            Assert.IsTrue(twoDaysTimeSpan.ToString() == "2.00:00:00", "TimeSpan.ToString() is not working");

            Assert.IsTrue(TimeSpan.FromHours(523).Ticks == 523.0 * 60.0 * 60.0 * 1000.0 * 10000.0, "TimeSpan.FromHours() is not working");
            Assert.IsTrue(TimeSpan.FromMinutes(5638).Ticks == 5638.0 * 60.0 * 1000.0 * 10000.0, "TimeSpan.FromMinutes() is not working");
            Assert.IsTrue(TimeSpan.FromSeconds(36452).Ticks == 36452 * 1000.0 * 10000.0, "TimeSpan.FromSeconds() is not working");
            Assert.IsTrue(TimeSpan.FromMilliseconds(50394039).Ticks == 50394039.0 * 10000.0, "TimeSpan.FromMilliseconds() is not working");

            TimeSpan someTimeSpan = new TimeSpan(5, 3, 47, 32, 543);

            Assert.IsTrue(someTimeSpan.Days == 5, "TimeSpan.Days is not working");
            Assert.IsTrue(someTimeSpan.Hours == 3, "TimeSpan.Hours is not working");
            Assert.IsTrue(someTimeSpan.Minutes == 47, "TimeSpan.Minutes is not working");
            Assert.IsTrue(someTimeSpan.Seconds == 32, "TimeSpan.Seconds is not working");
            Assert.IsTrue(someTimeSpan.Milliseconds == 543, "TimeSpan.Milliseconds is not working");

            Assert.IsTrue(someTimeSpan.ToString() == "5.03:47:32.5430000", "TimeSpan.ToString() is not working");

            someTimeSpan = someTimeSpan.Add(twoDaysTimeSpan);
            Assert.IsTrue(someTimeSpan.Days == 7, "TimeSpan.Add() is not working");

            someTimeSpan = someTimeSpan.Subtract(twoDaysTimeSpan);
            Assert.IsTrue(someTimeSpan.Days == 5, "TimeSpan.Subtract() is not working");

            someTimeSpan = someTimeSpan.Negate();
            Assert.IsTrue(someTimeSpan.Days == -5, "TimeSpan.Negate() is not working");

            someTimeSpan = someTimeSpan.Duration();
            Assert.IsTrue(someTimeSpan.Days == 5, "TimeSpan.Duration() is not working");

            // there was a bug in Newobj, ctor args size needed to be lower or equal to the struct fields size
            // the test seems to be equal to the test above, but the test above uses Call and this uses Newobj
            Assert.IsTrue(new TimeSpan(5, 3, 47, 32, 543).ToString() == "5.03:47:32.5430000", "TimeSpan.ToString() is not working");
        }
    }
}
