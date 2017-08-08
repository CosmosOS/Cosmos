using System.Xml;

namespace Cosmos.TestRunner.Core
{
    public partial class OutputHandlerXml
    {
        private XmlAttribute NewXmlAttribute(string name, string value)
        {
            var xResult = mDocument.CreateAttribute(name);
            xResult.Value = value;
            return xResult;
        }
    }
}
