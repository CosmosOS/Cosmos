// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Text;
using Cosmos.Kernel.System.Sessions;

namespace Cosmos.Kernel.Plugs.System.IO;

internal sealed class ConsoleTextWriter : TextWriter
{
    public override Encoding Encoding => Encoding.Default;

    public override void Write(char value)
    {
        ConsoleSession session = SessionManager.RequireCurrent();
        session.Write(value);
        session.Flush();
    }

    public override void Write(string? value)
    {
        if (value is null)
        {
            return;
        }

        ConsoleSession session = SessionManager.RequireCurrent();
        session.Write(value);
        session.Flush();
    }

    public override void Write(ReadOnlySpan<char> buffer)
    {
        ConsoleSession session = SessionManager.RequireCurrent();
        session.Write(buffer);
        session.Flush();
    }
}
