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
        protected DebuggerChannelUC mUserControl;
        public DebuggerChannelUC UserControl
        {
            get { return mUserControl; }
        }

        public ToolWindowPaneChannel()
            : base(null)
        {
        }
    }
}
