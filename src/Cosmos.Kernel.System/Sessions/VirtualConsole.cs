// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Keyboard;

namespace Cosmos.Kernel.System.Sessions;

/// <summary>
/// A local console, typed at from the keyboard: the primary console on
/// <see cref="KernelConsole.Default"/>, and every console a kernel adds with
/// <see cref="SessionManager.CreateVirtualConsole"/>.
/// </summary>
internal sealed class VirtualConsole : ConsoleSession
{
    internal VirtualConsole(KernelConsole screen)
        : base(screen)
    {
    }

    /// <inheritdoc/>
    public override string Name => $"tty{Id}";

    /// <inheritdoc/>
    public override bool IsRemote => false;

    /// <summary>
    /// Until a second session exists, nothing routes the keyboard, and the
    /// console takes its keys from the keyboard's queue, as
    /// <see cref="Console"/> did before sessions: a kernel that never adds a
    /// session reads the keyboard the way it always has. Once keys are
    /// routed, they are queued straight onto the session on the display.
    /// </summary>
    private protected override void PollInput()
    {
        if (SessionManager.IsRoutingKeyboard || !KernelFeatures.Keyboard)
        {
            return;
        }

        while (KeyboardManager.TryReadKey(out KeyEvent? key))
        {
            EnqueueInput(key);
        }
    }
}
