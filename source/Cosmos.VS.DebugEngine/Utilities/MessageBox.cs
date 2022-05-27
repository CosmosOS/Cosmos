using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using static Microsoft.VisualStudio.VSConstants;

namespace Cosmos.VS.DebugEngine.Utilities
{
    /// <summary>
    /// Shows message boxes.
    /// </summary>
    public class MessageBox
    {
        /// <summary>
        /// Shows a message box that is parented to the main Visual Studio window.
        /// </summary>
        /// <returns>
        /// The result of which button on the message box was clicked.
        /// </returns>
        /// <example>
        /// <code>
        /// VS.MessageBox.Show("Title", "The message");
        /// </code>
        /// </example>
        public MessageBoxResult Show(string line1,
            string line2 = "",
            OLEMSGICON icon = OLEMSGICON.OLEMSGICON_INFO,
            OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_OKCANCEL,
            OLEMSGDEFBUTTON defaultButton = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST)
        {
            int result = 0;
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                result = VsShellUtilities.ShowMessageBox(ServiceProvider.GlobalProvider, line2, line1, icon, buttons, defaultButton);
            });


            return (MessageBoxResult)result;
        }

        /// <summary>
        /// Shows a message box that is parented to the main Visual Studio window.
        /// </summary>
        /// <returns>
        /// The result of which button on the message box was clicked.
        /// </returns>
        /// <example>
        /// <code>
        /// await VS.MessageBox.ShowAsync("Title", "The message");
        /// </code>
        /// </example>
        public async Task<MessageBoxResult> ShowAsync(string line1,
            string line2 = "",
            OLEMSGICON icon = OLEMSGICON.OLEMSGICON_INFO,
            OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_OKCANCEL,
            OLEMSGDEFBUTTON defaultButton = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return Show(line1, line2, icon, buttons, defaultButton);
        }

        /// <summary>
        /// Shows an error message box.
        /// </summary>
        /// <returns>The result of which button on the message box was clicked.</returns>
        public MessageBoxResult ShowError(string line1, string line2 = "")
        {
            MessageBoxResult returnval = 0;
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
               returnval = Show(line1, line2, OLEMSGICON.OLEMSGICON_CRITICAL);
            });
            return returnval;
        }

        /// <summary>
        /// Shows an error message box.
        /// </summary>
        /// <returns>The result of which button on the message box was clicked.</returns>
        public async Task<MessageBoxResult> ShowErrorAsync(string line1, string line2 = "")
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return ShowError(line1, line2);
        }

        /// <summary>
        /// Shows a warning message box.
        /// </summary>
        /// <returns>The result of which button on the message box was clicked.</returns>
        public MessageBoxResult ShowWarning(string line1, string line2 = "")
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return Show(line1, line2, OLEMSGICON.OLEMSGICON_WARNING);
        }

        /// <summary>
        /// Shows a warning message box.
        /// </summary>
        /// <returns>The result of which button on the message box was clicked.</returns>
        public async Task<MessageBoxResult> ShowWarningAsync(string line1, string line2 = "")
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return ShowWarning(line1, line2);
        }

        /// <summary>
        /// Shows a yes/no/cancel message box.
        /// </summary>
        /// <returns>true if the user clicks the 'Yes' button.</returns>
        public bool ShowConfirm(string line1, string line2 = "")
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return Show(line1, line2, OLEMSGICON.OLEMSGICON_QUERY, OLEMSGBUTTON.OLEMSGBUTTON_YESNO) == MessageBoxResult.IDYES;
        }

        /// <summary>
        /// Shows a yes/no/cancel message box.
        /// </summary>
        /// <returns>true if the user clicks the 'Yes' button.</returns>
        public async Task<bool> ShowConfirmAsync(string line1, string line2 = "")
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return ShowConfirm(line1, line2);
        }
    }
}
