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

        public override void HandleMessage(LogMessage message)
        {
            // We only handle these.
            if (message.Name != "Warning" && message.Name != "Error")
                return;

            // Create the item.
            ListViewItem item = new ListViewItem(new string[] { message["Module"], message["String"] });
            item.ImageKey = message.Name.ToLowerInvariant();

            // Add it.
            listView.Items.Add(item);
        }
    }
}
