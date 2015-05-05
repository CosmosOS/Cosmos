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
            properties.SetProperty(BuildProperties.StackCorruptionDetectionEnabledString, "False");
            Assert.AreEqual(
                false, 
                properties.GetProperty<bool>(BuildProperties.StackCorruptionDetectionEnabledString, true));
            Assert.AreEqual(
                false,
                properties.StackCorruptionDetectionEnabled);
            Assert.AreEqual(
                "False",
                properties.GetProperty(BuildProperties.StackCorruptionDetectionEnabledString));
        }
    }
}
