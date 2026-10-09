// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using Cosmos.Executable.Lua;
using DevKernel.Shell;

namespace DevKernel.Commands;

/// <summary>
/// The Lua interpreter of the Cosmos.Executable.Lua package, on the
/// shell's console session: a script, a chunk, or the interactive prompt.
/// </summary>
internal static class LuaCommands
{
    /// <summary>Help section these commands are listed under.</summary>
    private const string Category = "Scripting";

    /// <summary>The option that runs the rest of the line as a chunk, as in <c>lua -e</c>.</summary>
    private const string ChunkOption = "-e";

    public static void Register(CommandShell shell)
    {
        shell.Register(
            Category,
            new ShellCommand
            {
                Name = "lua",
                Usage = "lua [script [args]]|-e <code>",
                Description = "Run a Lua script or chunk, or the Lua prompt",
                MaxArgs = ShellCommand.UnlimitedArgs,
                Execute = static (context, args) => RunLua(context, args),
            });
    }

    private static void RunLua(ShellContext context, CommandArgs args)
    {
        LuaInterpreter lua = new() { WorkingDirectory = context.Cwd };

        // os.execute runs a line of this shell, which may change its directory
        lua.ExecuteCommand = command =>
        {
            int status = RunShellCommand(context, command);
            lua.WorkingDirectory = context.Cwd;
            return status;
        };

        try
        {
            if (args.Count == 0)
            {
                Terminal.Hint($"{LuaDef.LUA_VERSION}. os.exit() leaves the prompt.");
                lua.RunPrompt();
            }
            else if (args[0] == ChunkOption)
            {
                if (args.Count == 1)
                {
                    CommandShell.PrintUsage(args.Command);
                    return;
                }

                lua.DoString(args.RawTail.Substring(args.RawTail.IndexOf(ChunkOption, StringComparison.Ordinal) + ChunkOption.Length).Trim(), "=(command line)");
            }
            else
            {
                string[] scriptArgs = new string[args.Count - 1];
                for (int i = 0; i < scriptArgs.Length; i++)
                {
                    scriptArgs[i] = args[i + 1];
                }

                lua.DoFile(args[0], scriptArgs);
            }
        }
        catch (Exception ex)
        {
            // One clause that tells the exceptions apart: the kernel enters
            // the first typed catch clause whatever the exception's type
            switch (ex)
            {
                case LuaException error:
                    Terminal.Error(error.Message);
                    if (error.LuaStackTrace is not null)
                    {
                        Terminal.Muted(error.LuaStackTrace);
                    }

                    break;
                case LuaExitException { ExitCode: not 0 } exit:
                    Terminal.Muted($"Exited with code {exit.ExitCode}.");
                    break;
                case LuaExitException:
                    break; // os.exit(): the script is done
                default:
                    throw;
            }
        }

        // The files the script left open, which no collector closes
        lua.Dispose();
    }

    /// <summary>Runs <paramref name="line"/> as if it were typed at the prompt; 0 unless it threw.</summary>
    private static int RunShellCommand(ShellContext context, string line)
    {
        try
        {
            context.Shell.Execute(context, line);
            return 0;
        }
        catch (Exception ex)
        {
            Terminal.Error($"Exception: {ex.Message}");
            return 1;
        }
    }
}
