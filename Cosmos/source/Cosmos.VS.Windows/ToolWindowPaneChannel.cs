using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace Cosmos.VS.Windows
{
    public class ToolWindowPaneChannel : ToolWindowPane
    {
        public event EventHandler UpdateWindow;

        protected DebuggerChannelUC mUserControl;

        public DebuggerChannelUC UserControl => mUserControl;

        public ToolWindowPaneChannel()
            : base(null)
        {
        }

        protected virtual void OnUpdateWindow(EventArgs e)
        {
            UpdateWindow?.Invoke(this, e);
        }
    }
}