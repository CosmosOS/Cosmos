using Cosmos.TestRunner;

namespace SimpleStructsAndArraysTest
{
    public static class LoadLongFromStruct
    {
        public struct OurStruct
        {
            public OurStruct(long value)
            {
                Value = value;
            }

            public long Value;
        }

        public static void Execte()
        {
            var xStruct = new OurStruct(0x0102030405060708);
            var xLong = xStruct.Value;
            Assert.IsTrue(xLong == 0x0102030405060708, "After loading long from struct, value is wrong!");
        }
    }
}
