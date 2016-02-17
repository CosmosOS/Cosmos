using System;
using System.Linq;

using Cosmos.Debug.Common;
using Cosmos.IL2CPU;
using NUnit.Framework;

namespace Cosmos.Core.Tests
{
    [TestFixture]
    public class FieldOrderTests
    {
        // the memory stuff requires DataLookupEntry.DataBlock to be the first field
        //[TestMethod]
        //public unsafe void TestFieldOrderingOfDataLookupEntry()
        //{
        //    ILOp.mPlugManager = new PlugManager(delegate(Exception exception)
        //                                        {
        //                                            throw new Exception("Error: " + exception.Message, exception);
        //                                        });
        //    using (var xDbg = new DebugInfo(":memory:", true))
        //    {
        //        try
        //        {
        //            var xInfo = ILOp.GetFieldsInfo(typeof(DataLookupEntry), false).OrderBy(i => i.Offset).ToArray();
        //            Assert.IsNotNull(xInfo);
        //            Assert.AreEqual(3, xInfo.Length);
        //            Assert.AreEqual(nameof(DataLookupEntry.DataBlock), xInfo[0].Field.Name);
        //        }
        //        finally
        //        {
        //            ILOp.mPlugManager = null;
        //        }
        //    }
        //}
    }
}
