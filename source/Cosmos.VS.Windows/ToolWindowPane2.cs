using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cosmos.VS.Windows
{
    public class ToolWindowPane2 : ToolWindowPane
    {
        protected DebuggerUC mUserControl;
        public DebuggerUC UserControl
        {
            get { return mUserControl; }
        }
        
        public ToolWindowPane2()
            : base(null)
        {
        }
    }
}