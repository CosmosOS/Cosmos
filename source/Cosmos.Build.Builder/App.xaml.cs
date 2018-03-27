using System.IO;
using System.Windows;

using Cosmos.Build.Installer;

namespace Cosmos.Build.Builder
{
    public partial class App : Application
    {
        internal static IBuilderConfiguration BuilderConfiguration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length == 0)
            {
                MessageBox.Show("Builder not meant to be called directly. Use install-VS2017.bat instead.");
                Shutdown();
            }

            var configuration = new CommandLineBuilderConfiguration(e.Args);

            var vsPath = configuration.VsPath;

            if (Directory.Exists(vsPath))
            {
                Paths.VSPath = vsPath;
                Paths.UpdateVSPath();
            }
            else
            {
                // For debugging, set params to something like this:
                // -VSPath=C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise

                MessageBox.Show("Visual Studio path must be provided. (-VSPATH or /VSPATH)");
                Shutdown();
            }

            BuilderConfiguration = configuration;

            base.OnStartup(e);
        }
    }
}
