// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.IO;

namespace DevKernel.Shell;

/// <summary>
/// The shell's read-and-run loop, on the calling thread's console session:
/// the kernel's main loop runs it on the primary console, and every other
/// session runs it on a thread of its own.
/// </summary>
internal static class ShellLoop
{
    /// <summary>Runs the shell until its session has no more input or <c>exit</c> is typed.</summary>
    public static void Run(ShellContext context)
    {
        while (RunOnce(context))
        {
        }
    }

    /// <summary>Prompts for one command line and runs it.</summary>
    /// <returns>False once the shell should stop: the session closed, <c>exit</c> was typed, or a command failed with an error other than I/O.</returns>
    public static bool RunOnce(ShellContext context)
    {
        Terminal.WritePrompt(context.Prompt, context.Cwd);

        try
        {
            string? input = Console.ReadLine();
            if (input is null)
            {
                // No console left to read from; end the loop rather than
                // spin on it forever.
                return false;
            }

            if (input.Trim().Length != 0)
            {
                context.Shell.Execute(context, input);
            }

            return !context.ExitRequested;
        }
        catch (IOException ex)
        {
            // A disk that fails, or is pulled out mid-command, fails that
            // command only: USB disks come and go while the shell runs.
            Terminal.Error($"I/O error: {ex.Message}");
            return true;
        }
        catch (Exception ex)
        {
            Terminal.Error($"Exception: {ex.Message}");
            return false;
        }
    }
}
