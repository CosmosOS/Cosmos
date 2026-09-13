using DevKernel.Shell;

namespace DevKernel.Commands;

/// <summary>
/// Assembles the shell's command set. Registration order is the order
/// <c>help</c> lists the sections in.
/// </summary>
internal static class CommandRegistry
{
    /// <summary>Builds a shell with every DevKernel command registered.</summary>
    public static CommandShell CreateDefault()
    {
        CommandShell shell = new();

        SystemCommands.Register(shell);
        KeyboardCommands.Register(shell);
        MemoryCommands.Register(shell);
        SchedulerCommands.Register(shell);
        GraphicsCommands.Register(shell);
        NetworkCommands.Register(shell);
        DiskCommands.Register(shell);
        PartitionCommands.Register(shell);
        MountCommands.Register(shell);
        FileCommands.Register(shell);

        return shell;
    }
}
