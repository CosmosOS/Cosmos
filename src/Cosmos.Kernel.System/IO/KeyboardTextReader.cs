using Cosmos.Kernel.System.Keyboard;
using Cosmos.Kernel.System.Sessions;

namespace Cosmos.Kernel.System.IO;

/// <summary>
/// Standard input: reads the calling thread's console session, which is the
/// keyboard for a local console and the network for a remote one.
/// </summary>
internal sealed class KeyboardTextReader : TextReader
{
    public override int Read()
    {
        if (SessionManager.CurrentInput is { } session && session.TryReadKey(out KeyEvent? result))
        {
            return result.KeyChar;
        }
        else
        {
            return -1;
        }
    }

    public override int Peek()
    {
        return SessionManager.CurrentInput is { } session && session.TryPeekKey(out KeyEvent? result) ? result.KeyChar : -1;
    }

    public override string? ReadLine()
    {
        return SessionManager.CurrentInput is { } session ? LineEditor.ReadLine(session) : null;
    }
}
