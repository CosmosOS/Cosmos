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
    public partial class TestLogHandler : LogHandler
    {
        private ListViewGroup _currentGroup;

        public TestLogHandler()
        {
            InitializeComponent();
        }

        public override string Title
        {
            get
            {
                return "Test Cases";
            }
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

            if (message.Name == "TestCase_Started")
            {
                _currentGroup = new ListViewGroup(message["Name"]);
                listView.Groups.Add(_currentGroup);
            }
            else if (message.Name == "TestCase")
            {
                ListViewItem item = new ListViewItem(message["Message"]);
                item.ImageKey = message["Success"];
                item.Group = _currentGroup;
                listView.Items.Add(item);
            }
        }
    }
}
