using System.Globalization;
using System.Text;
using Cosmos.Kernel.System.Sessions;

namespace Cosmos.Kernel.System.IO;

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
