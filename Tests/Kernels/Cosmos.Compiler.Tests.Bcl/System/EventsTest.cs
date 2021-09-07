using System;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    internal class TestClass
    {
        private static bool handlerInvoked;

        public event EventHandler EventFired = (sender, args) =>
        {
            handlerInvoked = true;
        };

        private void OnEventFired(EventArgs e)
        {
            EventHandler handler = EventFired;
            handler?.Invoke(this, e);
        }

        public bool ExecuteTest()
        {
            OnEventFired(null);
            return handlerInvoked;
        }
    }

    public class EventsTest
    {
        public static void Execute()
        {
            var test = new TestClass();
            Assert.IsTrue(test.ExecuteTest(), "Event handler was not invoked.");
        }
    }
}
