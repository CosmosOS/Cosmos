using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Windows.Config.Tasks
{
    [global::System.Serializable]
    public class TaskException : Exception
    {
        public TaskException() { }
        public TaskException(string message) : base(message) { }
        public TaskException(string message, Exception inner) : base(message, inner) { }
        protected TaskException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
