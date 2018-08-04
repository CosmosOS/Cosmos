using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Exceptions
{
    public static class TestTryFinally
    {
        public static void Execute()
        {
            ClearToggles();

            // test the normal flow, no explicit return, no return value
            TestNormalFlowNoReturnValue();

            Assert.IsTrue(mWasBeforeTry, "NormalFlowNoReturnValue.WasBeforeTry");
            Assert.IsTrue(mWasInTry, "NormalFlowNoReturnValue.WasInTry");
            Assert.IsTrue(mWasInFinally, "NormalFlowNoReturnValue.WasInFinally");
            Assert.IsTrue(mWasAfterFinally, "NormalFlowNoReturnValue.WasAfterFinally");

            ClearToggles();
            TestExplicitReturnNoReturnValue();

            Assert.IsTrue(mWasBeforeTry, "ExplicitReturnNoReturnValue.WasBeforeTry");
            Assert.IsTrue(mWasInTry, "ExplicitReturnNoReturnValue.WasInTry");
            Assert.IsTrue(mWasInFinally, "ExplicitReturnNoReturnValue.WasInFinally");
            Assert.IsFalse(mWasAfterFinally, "ExplicitReturnNoReturnValue.WasAfterFinally");

            ClearToggles();
            TestNestedFinally();

            Assert.IsTrue(mWasBeforeTry, "ExplicitReturnNoReturnValue.WasBeforeTry");
            Assert.IsTrue(mWasInTry, "ExplicitReturnNoReturnValue.WasInTry");
            Assert.IsTrue(mWasInFinally, "ExplicitReturnNoReturnValue.WasInFinally");
            Assert.IsTrue(mWasInTry2, "ExplicitReturnNoReturnValue.WasInTry2");
            Assert.IsTrue(mWasInFinally2, "ExplicitReturnNoReturnValue.WasInFinally2");
            Assert.IsTrue(mWasAfterFinally, "ExplicitReturnNoReturnValue.WasAfterFinally");
        }

        private static bool mWasBeforeTry;
        private static bool mWasInTry;
        private static bool mWasInFinally;
        private static bool mWasAfterFinally;

        private static bool mWasInTry2;
        private static bool mWasInFinally2;

        private static void ClearToggles()
        {
            mWasBeforeTry = false;
            mWasInTry = false;
            mWasInFinally = false;
            mWasAfterFinally = false;

            mWasInTry2 = false;
            mWasInFinally2 = false;
        }

        private static void TestNormalFlowNoReturnValue()
        {
            mWasBeforeTry = true;

            try
            {
                mWasInTry = true;
            }
            finally
            {
                mWasInFinally = true;
            }

            mWasAfterFinally = true;
        }

        private static void TestExplicitReturnNoReturnValue()
        {
            mWasBeforeTry = true;

            try
            {
                mWasInTry = true;
                return;
            }
            finally
            {
                mWasInFinally = true;
            }

            mWasAfterFinally = true;
        }

        private static void TestNestedFinally()
        {
            mWasBeforeTry = true;

            try
            {
                mWasInTry = true;
            }
            finally
            {
                try
                {
                    mWasInTry2 = true;
                }
                finally
                {
                    mWasInFinally2 = true;
                }

                mWasInFinally = true;
            }

            mWasAfterFinally = true;
        }
    }
}
