using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DevKernel.Shell;

/// <summary>
/// The command registry and dispatcher. Commands are matched by a linear scan
/// rather than a hash map: with a few dozen entries the scan is free, and it
/// keeps the shell off <see cref="Dictionary{TKey,TValue}"/>'s string hashing
/// in an environment where the runtime is deliberately minimal.
/// </summary>
internal sealed class CommandShell
{
    /// <summary>Narrowest the help listing's usage column may be, matching the plain label column.</summary>
    private const int MinUsageColumnWidth = Terminal.LabelColumnWidth;

    /// <summary>Widest the usage column may grow before long syntaxes stop dictating the layout.</summary>
    private const int MaxUsageColumnWidth = 38;

    /// <summary>Spaces between the usage column and the description column.</summary>
    private const int HelpColumnGap = 2;

    /// <summary>Indent placed before every help entry.</summary>
    private const string HelpIndent = "  ";

    private readonly List<ShellCommand> _commands = new();
    private int _usageColumnWidth = MinUsageColumnWidth;

    /// <summary>Every registered command, in registration order.</summary>
    public IReadOnlyList<ShellCommand> Commands => _commands;

    /// <summary>
    /// Adds <paramref name="commands"/> to the registry under <paramref name="category"/>.
    /// Registration order is the order <c>help</c> lists them in, so commands of
    /// one category must be registered together.
    /// </summary>
    public void Register(string category, params ShellCommand[] commands)
    {
        for (int i = 0; i < commands.Length; i++)
        {
            ShellCommand command = commands[i];
            command.Category = category;
            _commands.Add(command);

            int width = command.Usage.Length + HelpColumnGap;
            if (width > _usageColumnWidth && width <= MaxUsageColumnWidth)
            {
                _usageColumnWidth = width;
            }
        }
    }

    /// <summary>Finds the command invoked by <paramref name="token"/> (its name or one of its aliases).</summary>
    public bool TryResolve(string token, [NotNullWhen(true)] out ShellCommand? command)
    {
        for (int i = 0; i < _commands.Count; i++)
        {
            if (_commands[i].Matches(token))
            {
                command = _commands[i];
                return true;
            }
        }

        command = null;
        return false;
    }

    /// <summary>
    /// Parses one input line, validates its argument count, and runs the
    /// matching handler. Unknown commands and malformed invocations report
    /// themselves; exceptions escape to the caller.
    /// </summary>
    public void Execute(ShellContext context, string line)
    {
        string trimmed = line.Trim();
        string[] tokens = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            return;
        }

        string token = tokens[0].ToLower();
        if (!TryResolve(token, out ShellCommand? command))
        {
            Terminal.Error($"\"{token}\" is not a command");
            Terminal.Info("Type 'help' for available commands.");
            return;
        }

        CommandArgs args = new(command, trimmed, tokens);
        if (args.Count < command.MinArgs || args.Count > command.MaxArgs)
        {
            PrintUsage(command);
            return;
        }

        command.Execute(context, args);
    }

    /// <summary>Reports how <paramref name="command"/> should have been invoked.</summary>
    public static void PrintUsage(ShellCommand command)
    {
        Terminal.Error("Usage: " + command.Usage);
    }

    /// <summary>Lists every command, grouped under its category heading.</summary>
    public void PrintHelp()
    {
        string category = string.Empty;
        for (int i = 0; i < _commands.Count; i++)
        {
            ShellCommand command = _commands[i];
            if (command.Category != category)
            {
                category = command.Category;
                if (i > 0)
                {
                    Console.WriteLine();
                }

                Terminal.Header(category + ":");
            }

            Console.Write(HelpIndent);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(command.Usage.PadRight(_usageColumnWidth));
            Console.ResetColor();
            Console.WriteLine(command.Description);
        }
    }
}
