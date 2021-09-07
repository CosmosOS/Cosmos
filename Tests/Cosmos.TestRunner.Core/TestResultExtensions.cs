using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Cosmos.TestRunner.Core
{
    public static class TestResultExtensions
    {
        private static Dictionary<ITestResult, XDocument> xmlCache = new Dictionary<ITestResult, XDocument>();

        public static XDocument ToXml(this ITestResult aTestResult)
        {
            if (xmlCache.TryGetValue(aTestResult, out var xDocument))
            {
                return xDocument;
            }

            var xRoot = new XElement("TestResult");
            xDocument = new XDocument(xRoot);
            
            foreach (var xConfig in aTestResult.KernelTestResults.GroupBy(r => r.RunConfiguration))
            {
                var xConfigElement = new XElement(
                    "Configuration",
                    new XAttribute("IsELF", xConfig.Key.IsELF.ToString()),
                    new XAttribute("RunTarget", xConfig.Key.RunTarget.ToString()));
                
                foreach (var xKernelTestResult in xConfig)
                {
                    var xKernelElement = new XElement(
                        "Kernel",
                        new XAttribute("Name", xKernelTestResult.KernelName),
                        new XAttribute("Result", xKernelTestResult.Result ? "Passed" : "Failed"));
                    
                    var xLogMessageElement = new XElement("FullLog", xKernelTestResult.TestLog);
                    xKernelElement.Add(xLogMessageElement);

                    xConfigElement.Add(xKernelElement);
                }

                xRoot.Add(xConfigElement);
            }

            xmlCache.Add(aTestResult, xDocument);

            return xDocument;
        }

        public static void SaveXmlToFile(this ITestResult aTestResult, string aFilePath)
        {
            var xDocument = aTestResult.ToXml();
            var xWriterSettings = new XmlWriterSettings()
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                OmitXmlDeclaration = true
            };

            using (var xWriter = XmlWriter.Create(aFilePath, xWriterSettings))
            {
                xDocument.Save(xWriter);
            }
        }
    }
}
