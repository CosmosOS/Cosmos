using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cosmos.Kernel.LogTail
{
    public partial class MainForm : Form
    {
        private Handlers.LogHandler[] _handlers;

        public MainForm()
        {
            InitializeComponent();
            BuildTabs();
        }

        private void BuildTabs()
        {
            _handlers = Handlers.LogHandler.GetHandlers();
            foreach (Handlers.LogHandler handler in _handlers)
            {
                TabPage page = new TabPage();
                page.Text = handler.Title;
                page.Controls.Add(handler);
                handler.Dock = DockStyle.Fill;
                tabControl.TabPages.Add(page);
            }
        }
    }
}
