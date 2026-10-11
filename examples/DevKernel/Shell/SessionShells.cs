// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using Cosmos.Kernel.System.Sessions;
using Sys = Cosmos.Kernel.System;

namespace DevKernel.Shell;

/// <summary>
/// Starts shells on console sessions other than the primary console: the
/// virtual consoles the kernel opens at boot, and the Telnet connections.
/// </summary>
internal static class SessionShells
{
    /// <summary>Opens a virtual console and starts a shell on it.</summary>
    /// <param name="template">The shell whose commands and network state the new one shares.</param>
    /// <returns>The new console.</returns>
    public static ConsoleSession OpenVirtualConsole(ShellContext template)
    {
        ConsoleSession session = SessionManager.CreateVirtualConsole();
        ShellContext context = template.CreateSibling();
        try
        {
            SessionManager.Start(session, () => Run(session, context));
        }
        catch (InvalidOperationException)
        {
            session.Close();
            throw;
        }

        return session;
    }

    /// <summary>Greets the calling thread's session, then runs the shell on it until it exits.</summary>
    public static void Run(ConsoleSession session, ShellContext context)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"CosmosOS {Sys.Kernel.VersionString} - {session.Name} (session {session.Id})");
        Console.ResetColor();
        Terminal.Hint(session.IsRemote
            ? "Type 'help' for available commands, 'exit' to disconnect."
            : "Alt+F1..F12 switches sessions. Type 'help' for available commands.");
        Console.WriteLine();

        ShellLoop.Run(context);
    }
}
