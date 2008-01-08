using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace Cosmos.Tools.SkypeBot
{
    [XmlRoot("meeting")]
    public class ScribeSkypeBot : SkypeBot
    {
        private bool _logging;
        
        private List<SkypeMessage> _messages = new List<SkypeMessage>();
        [XmlElement("message")]
        public List<SkypeMessage> Message
        {
            get
            {
                return _messages;
            }
        }

        private DateTime _started;
        [XmlAttribute("date")]
        public DateTime Started
        {
            get
            {
                return _started;
            }
            set
            {
                _started = value;
            }
        }

        public ScribeSkypeBot()
            : base("")
        {
        }

        public ScribeSkypeBot(string blob)
            : base(blob)
        {
            XmlSerializer ser = new XmlSerializer(GetType());
        }

        protected override void OnMessageReceived(SkypeMessage msg)
        {
            if (msg.Body.ToLower() == "/ss")
            {
                _logging = false;
                string filename = "C:\\logs\\" + _started.ToString().Replace('\\', '-').Replace('/', '-').Replace(':', '.') + ".xml";
                using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
                {
                    XmlSerializer ser = new XmlSerializer(GetType());
                    ser.Serialize(writer, this);
                }
            }
            else if (msg.Body.ToLower() == "/sb")
            {
                _messages.Clear();
                _started = DateTime.Now.ToUniversalTime();
                _logging = true;
                SendMessage(string.Format("Scribe: Meeting Started {0} UTC.", _started.ToString()));
            }
            else if (msg.Body.ToLower() == "/se")
            {
                SendMessage(string.Format("Scribe: Meeting Ended {0} UTC.", DateTime.Now.ToUniversalTime().ToString()));
                SendMessage(string.Format("/ss"));
            }
            else if (_logging)
            {
                LogMessage(msg);
            }

            base.OnMessageReceived(msg);
        }

        private void LogMessage(SkypeMessage message)
        {
            _messages.Add(message);
        }
    }
}
