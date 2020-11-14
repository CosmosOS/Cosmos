using System.Diagnostics;

using Avalonia;
using Avalonia.Logging.Serilog;
using Avalonia.Markup.Xaml;

using Serilog;

using Cosmos.TestRunner.UI.Views;

namespace Cosmos.TestRunner.UI
{
    internal class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private static void Main(string[] args)
        {
            InitializeLogging();
            BuildAvaloniaApp().Start<MainWindow>();
        }

        private static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI();

        [Conditional("DEBUG")]
        private static void InitializeLogging()
        {
            SerilogLogger.Initialize(new LoggerConfiguration()
                .MinimumLevel.Warning()
                .WriteTo.Trace(outputTemplate: "{Area}: {Message}")
                .CreateLogger());
        }
    }
}
