using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Cosmos.Tools.SkypeBot
{
    /// <summary>
    /// Represents a single message.
    /// </summary>
    public struct SkypeMessage
    {
        [XmlElement("body")]
        public string Body { get; set; }

        [XmlAttribute("from")]
        public string From { get; set;  }

        [XmlAttribute("sent")]
        public DateTime Sent { get; set; }
    }

    /// <summary>
    /// Represents an event to do with a <see cref="SkypeMessage"/>.
    /// </summary>
    public class SkypeMessageEventArgs : EventArgs
    {
        public SkypeMessage Message { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="SkypeMessageEventArgs"/> class.
        /// </summary>
        /// <param name="message"></param>
        public SkypeMessageEventArgs(SkypeMessage message)
        {
            Message = message;
        }
    }
}
