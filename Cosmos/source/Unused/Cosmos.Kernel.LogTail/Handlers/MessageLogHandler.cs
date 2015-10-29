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
    public partial class MessageLogHandler : LogHandler
    {
        public override string Title
        {
            get
            {
                return "Messages";
            }
        }

        public MessageLogHandler()
        {
            InitializeComponent();
        }

        public override void Clear()
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new Action(Clear));
                return;
            }

            listView.Items.Clear();
        }

        private delegate void Handler(LogMessage message);

        public override void HandleMessage(LogMessage message)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new Handler(HandleMessage), message);
                return;
            }

            // We only handle these.
            if (message.Name != "Warning" && message.Name != "Error" && message.Name != "Message")
                return;

            // Create the item.
            ListViewItem item = new ListViewItem(new string[] { message["Module"], message["String"] });
            item.ImageKey = message.Name.ToLowerInvariant();

            // Add it.
            listView.Items.Add(item);
        }
    }
}
