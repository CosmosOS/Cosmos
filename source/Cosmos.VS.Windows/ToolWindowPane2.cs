using Microsoft.VisualStudio.Shell;

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