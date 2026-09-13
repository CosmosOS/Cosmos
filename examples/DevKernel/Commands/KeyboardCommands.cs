using Cosmos.Kernel.System.Keyboard;
using Cosmos.Kernel.System.Keyboard.ScanMaps;
using DevKernel.Shell;

namespace DevKernel.Commands;

/// <summary>
/// Keyboard commands: showing and switching the scan map the shell reads
/// keys through.
/// </summary>
internal static class KeyboardCommands
{
    /// <summary>Help section these commands are listed under.</summary>
    private const string Category = "Keyboard";

    /// <summary>Names accepted by <c>layout</c>, in the order <c>help</c> shows them.</summary>
    private const string LayoutNames = "us|fr|de|es|gb|tr|dvorak";

    /// <summary>Name of the layout the shell currently reads through; the manager boots with US.</summary>
    private static string s_current = "us";

    public static void Register(CommandShell shell)
    {
        shell.Register(
            Category,
            new ShellCommand
            {
                Name = "layout",
                Usage = $"layout [{LayoutNames}]",
                Description = "Show or switch the keyboard layout",
                MaxArgs = 1,
                Execute = static (context, args) =>
                {
                    if (args.Count == 0)
                    {
                        Terminal.InfoLine("Layout", s_current);
                        return;
                    }

                    SwitchLayout(args.GetLower(0));
                },
            });
    }

    private static void SwitchLayout(string name)
    {
        ScanMapBase? layout = name switch
        {
            "us" => new USStandardLayout(),
            "fr" => new FRStandardLayout(),
            "de" => new DEStandardLayout(),
            "es" => new ESStandardLayout(),
            "gb" => new GBStandardLayout(),
            "tr" => new TRStandardLayout(),
            "dvorak" => new USDvorakLayout(),
            _ => null,
        };

        if (layout is null)
        {
            Terminal.Error($"Unknown layout '{name}'. Expected one of: {LayoutNames}");
            return;
        }

        KeyboardManager.SetKeyLayout(layout);
        s_current = name;
        Terminal.Success($"Keyboard layout set to {name}");
    }
}
