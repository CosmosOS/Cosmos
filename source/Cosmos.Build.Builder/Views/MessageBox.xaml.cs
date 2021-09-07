using System.Windows;

namespace Cosmos.Build.Builder.Views
{
    public partial class MessageBox : Window
    {
        public MessageBox(string Content)
        {
            InitializeComponent();
            lblMain.Text = Content;
        }

        public static void Show(string Content)
        {
            var window = new MessageBox(Content);
            //this workarounds a bug that when the main window is minimized then brought to front, the message box is no longer visible
            window.Topmost = true;
            window.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Topmost = false;
            Close();
        }
    }
}
