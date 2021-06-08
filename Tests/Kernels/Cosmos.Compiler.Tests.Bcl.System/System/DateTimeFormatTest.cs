using Cosmos.TestRunner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class DateTimeFormatTest
    {
        public static void Execute()
        {
            DateTime date = new DateTime(2011, 6, 10);
            Assert.AreEqual("06/10/2011 00:00:00", date.ToString(), "Standard DateTime.ToString works");
            //Assert.AreEqual("June 10, 2011", date.ToString("MMMM dd, yyyy"), "Using DataTimeFormat for custom format works");
        }
    }
}
