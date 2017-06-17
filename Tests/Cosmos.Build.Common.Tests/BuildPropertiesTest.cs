using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmos.Build.Common.Tests
{
    [TestClass]
    public class BuildPropertiesTest
    {
        [TestMethod]
        public void TestParsing()
        {
            var properties = new BuildProperties();
            properties.SetProperty(BuildPropertyNames.StackCorruptionDetectionEnabledString, "False");
            Assert.AreEqual(
                false,
                properties.GetProperty<bool>(BuildPropertyNames.StackCorruptionDetectionEnabledString, true));
            Assert.AreEqual(
                false,
                properties.StackCorruptionDetectionEnabled);
            Assert.AreEqual(
                "False",
                properties.GetProperty(BuildPropertyNames.StackCorruptionDetectionEnabledString));
        }
    }
}
