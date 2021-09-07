using System;
using Microsoft.VisualStudio.Shell;

namespace Cosmos.VS.Windows
{
    public class ToolWindowPane2 : ToolWindowPane
    {
        public event EventHandler UpdateWindow;

        public sealed override object Content
        {
            get => base.Content;
            set => base.Content = value;
        }

        public DebuggerUC UserControl => Content as DebuggerUC;

        public ToolWindowPane2()
            : base(null)
        {
        }

        protected virtual void OnUpdateWindow(EventArgs e)
        {
            UpdateWindow?.Invoke(this, e);
        }
    }
}
