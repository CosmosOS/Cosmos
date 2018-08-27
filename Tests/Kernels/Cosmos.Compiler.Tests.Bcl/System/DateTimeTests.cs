using System;

using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    internal static class DateTimeTests
    {
        public static void Execute()
        {
            var someDateTime = new DateTime(2017, 4, 7, 16, 47, 32, 462);

            Assert.IsTrue(someDateTime.Year == 2017, "DateTime.Year is not working");
            Assert.IsTrue(someDateTime.Month == 4, "DateTime.Month is not working");
            Assert.IsTrue(someDateTime.Day == 7, "DateTime.Day is not working");
            Assert.IsTrue(someDateTime.Hour == 16, "DateTime.Hour is not working");
            Assert.IsTrue(someDateTime.Minute == 47, "DateTime.Minute is not working");
            Assert.IsTrue(someDateTime.Second == 32, "DateTime.Second is not working");
            Assert.IsTrue(someDateTime.Millisecond == 462, "DateTime.Millisecond is not working");

            Assert.IsTrue(DateTime.Now.Year >= 2018, "DateTime.Now is returning an year lower than 2018");

            Assert.IsTrue(someDateTime.ToString() == "2017-04-07 16:47:32", "DateTime.ToString() is not working");

            TimeSpan twoDaysTimeSpan = TimeSpan.FromDays(2);

            someDateTime = someDateTime.Add(twoDaysTimeSpan);
            Assert.IsTrue(someDateTime.Day == 9, "DateTime.Add() is not working");

            someDateTime = someDateTime.Subtract(twoDaysTimeSpan);
            Assert.IsTrue(someDateTime.Day == 7, "DateTime.Subtract() is not working");
        }
    }
}
