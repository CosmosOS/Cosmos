using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace Frotz.Other {
    public class ZMetaData {
        public String RawMetaData { get; private set; }
        public string RawBiblographic { get; private set; } // TODO Remove this

        public ZMetaData(String Metadata) {
            this.RawMetaData = Metadata;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Metadata);
            XmlNodeList elements = doc.GetElementsByTagName("bibliographic");
            Console.WriteLine("NODE:" + elements.Count);

            if (elements.Count > 0) {
                RawBiblographic = elements[0].InnerXml;
            }
        }
    }
}
