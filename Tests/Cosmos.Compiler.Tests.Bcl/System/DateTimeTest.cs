using System;

using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    public class DateTimeTest
    {
        public static void Execute()
        {
            DateTime xDateTime = new DateTime(2017, 4, 7, 16, 47, 32, 462);

            Assert.IsTrue(xDateTime.Year == 2017, "DateTime.Year is not working");
            Assert.IsTrue(xDateTime.Month == 4, "DateTime.Month is not working");
            Assert.IsTrue(xDateTime.Day == 7, "DateTime.Day is not working");
            Assert.IsTrue(xDateTime.Hour == 16, "DateTime.Hour is not working");
            Assert.IsTrue(xDateTime.Minute == 47, "DateTime.Minute is not working");
            Assert.IsTrue(xDateTime.Second == 32, "DateTime.Second is not working");
            Assert.IsTrue(xDateTime.Millisecond == 462, "DateTime.Millisecond is not working");

            Assert.IsTrue(DateTime.Now.Year >= 2016, "DateTime.Now is returning an year lower than 2016");

            Assert.IsTrue(xDateTime.ToString() == "2017-04-07 16:47:32", "DateTime.ToString() is not working");

            TimeSpan x2Days = TimeSpan.FromDays(2);

            xDateTime = xDateTime.Add(x2Days);
            Assert.IsTrue(xDateTime.Day == 9, "DateTime.Add() is not working");

            xDateTime = xDateTime.Subtract(x2Days);
            Assert.IsTrue(xDateTime.Day == 7, "DateTime.Subtract() is not working");
        }
    }
}
