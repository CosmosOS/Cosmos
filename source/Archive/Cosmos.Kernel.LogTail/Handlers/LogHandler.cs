using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cosmos.Kernel.LogTail.Handlers
{
    public partial class LogHandler : UserControl
    {
        public virtual String Title
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public LogHandler()
        {
            InitializeComponent();
        }

        // For designer support.
        public virtual void HandleMessage(LogMessage message)
        {
            throw new NotImplementedException();
        }

        public virtual void Clear()
        {
            throw new NotImplementedException();
        }

        public static LogHandler[] GetHandlers()
        {
            List<LogHandler> handlers = new List<LogHandler>();

            foreach (Type t in typeof(LogHandler).Assembly.GetTypes())
            {
                if (t != typeof(LogHandler) && typeof(LogHandler).IsAssignableFrom(t))
                    handlers.Add((LogHandler)Activator.CreateInstance(t));
            }

            return handlers.ToArray();
        }
    }
}
