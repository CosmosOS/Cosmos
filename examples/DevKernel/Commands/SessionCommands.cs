// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using Cosmos.Kernel.System.Sessions;
using DevKernel.Shell;

namespace DevKernel.Commands;

/// <summary>
/// Console sessions: the virtual consoles on the display, and switching
/// between them.
/// </summary>
internal static class SessionCommands
{
    /// <summary>Help section these commands are listed under.</summary>
    private const string Category = "Sessions";

    /// <summary>Width of the session-number column of the listing.</summary>
    private const int IdColumnWidth = 4;

    /// <summary>Width of the session-name column of the listing.</summary>
    private const int NameColumnWidth = 24;

    public static void Register(CommandShell shell)
    {
        shell.Register(
            Category,
            new ShellCommand
            {
                Name = "sessions",
                Aliases = ["who"],
                Usage = "sessions",
                Description = "List the console sessions",
                Execute = static (context, args) => ListSessions(),
            },
            new ShellCommand
            {
                Name = "tty",
                Usage = "tty",
                Description = "Show which session this shell runs on",
                Execute = static (context, args) => PrintCurrentSession(),
            },
            new ShellCommand
            {
                Name = "chvt",
                Usage = "chvt <n>",
                Description = "Show session n on the display (also Alt+Fn)",
                MinArgs = 1,
                MaxArgs = 1,
                Execute = static (context, args) => SwitchTo(args[0]),
            },
            new ShellCommand
            {
                Name = "openvt",
                Usage = "openvt",
                Description = "Open a new virtual console with a shell",
                Execute = static (context, args) => OpenVirtualConsole(context),
            },
            new ShellCommand
            {
                Name = "exit",
                Aliases = ["logout"],
                Usage = "exit",
                Description = "Close this session",
                Execute = static (context, args) => Exit(context),
            });
    }

    private static void ListSessions()
    {
        ConsoleSession? current = SessionManager.Current;

        Terminal.Header("Console Sessions:");
        foreach (ConsoleSession session in SessionManager.GetSessions())
        {
            string activeMark = session.IsActive ? "*" : " ";
            string currentMark = ReferenceEquals(session, current) ? ">" : " ";
            Console.Write($"  {activeMark}{currentMark} ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(session.Id.ToString().PadRight(IdColumnWidth));
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(session.Name.PadRight(NameColumnWidth));
            Console.ResetColor();
            string kind = session.IsRemote ? "remote " : "local  ";
            Console.WriteLine($"{kind}{session.Cols}x{session.Rows}");
        }

        Terminal.Muted("  * on the display   > this shell");
    }

    private static void PrintCurrentSession()
    {
        if (SessionManager.Current is { } session)
        {
            Terminal.Info($"{session.Name} (session {session.Id})");
        }
    }

    private static void SwitchTo(string argument)
    {
        if (!int.TryParse(argument, out int id) || !SessionManager.Switch(id))
        {
            Terminal.Error($"No session {argument}; 'sessions' lists them.");
        }
    }

    private static void OpenVirtualConsole(ShellContext context)
    {
        // A full session table or a scheduler that does not run fails this
        // command only; escaping, it would end the shell, and on the primary
        // console the kernel's main loop.
        try
        {
            ConsoleSession session = SessionShells.OpenVirtualConsole(context);
            Terminal.Success($"Opened {session.Name}: Alt+F{session.Id} or 'chvt {session.Id}' shows it.");
        }
        catch (InvalidOperationException ex)
        {
            Terminal.Error(ex.Message);
        }
    }

    private static void Exit(ShellContext context)
    {
        if (SessionManager.Current is not { } session || ReferenceEquals(session, SessionManager.Primary))
        {
            Terminal.Error("The primary console cannot be closed.");
            return;
        }

        context.ExitRequested = true;
    }
}
