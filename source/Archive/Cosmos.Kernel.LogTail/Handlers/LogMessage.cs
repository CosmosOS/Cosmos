using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Kernel.LogTail.Handlers
{
    /// <summary>
    /// Represents a single log message.
    /// </summary>
    public class LogMessage
    {
        private string _name;
        /// <summary>
        /// Gets the name of the message.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// The values.
        /// </summary>
        private Dictionary<string, string> _values = new Dictionary<string, string>();

        /// <summary>
        /// Gets a specific attribute.
        /// </summary>
        /// <param name="name">The name of the atttribute.</param>
        /// <returns></returns>
        public string this[string name]
        {
            get
            {
                string result;
                _values.TryGetValue(name, out result);
                return result;
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LogMessage"/> class.
        /// </summary>
        /// <param name="name"></param>
        public LogMessage(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Adds an attribute.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddAttribute(string name, string value)
        {
            _values.Add(name, value);
        }
    }
}
