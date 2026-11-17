// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.System.Sessions;

namespace Cosmos.Kernel.Plugs.System.IO;

/// <summary>
/// Standard input: reads the calling thread's console session, which is the
/// keyboard for a local console and the network for a remote one. The
/// session reads the way .NET's console reader does, a line at a time
/// through its line editor, handed out character by character by
/// <see cref="Read"/> and <see cref="Peek"/>, the new line last; with no
/// session to read, every member reports the end of the stream.
/// </summary>
internal sealed class KeyboardTextReader : TextReader
{
    /// <summary>Reads the next character, waiting for a line to be entered when none is pending.</summary>
    /// <returns>The character, or -1 once the session is closed.</returns>
    public override int Read()
    {
        return SessionManager.CurrentInput is { } session ? session.ReadChar() : -1;
    }

    /// <summary>Looks at the next character without taking it, waiting for a line to be entered when none is pending.</summary>
    /// <returns>The character, or -1 once the session is closed.</returns>
    public override int Peek()
    {
        return SessionManager.CurrentInput is { } session ? session.PeekChar() : -1;
    }

    /// <summary>Reads a line: the rest of one <see cref="Read"/> started on, or the next one entered.</summary>
    /// <returns>The line without its terminator, or null once the session is closed.</returns>
    public override string? ReadLine()
    {
        return SessionManager.CurrentInput is { } session ? session.ReadLine() : null;
    }
}
