using System.Windows;

namespace Cosmos.Deploy.USB
{
    public partial class App : Application
    {
        public static string ObjFile;
        public static string Title = "Cosmos USB Deployment Tool";

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length < 1)
            {
                MessageBox.Show("Not enough parameters.", Title);
                Shutdown(-1);
            }

            ObjFile = e.Args[0];
        }
    }
}