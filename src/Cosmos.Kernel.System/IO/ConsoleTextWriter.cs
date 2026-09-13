using System.Globalization;
using System.Text;
using Cosmos.Kernel.System.Graphics;

namespace Cosmos.Kernel.System.IO;

internal sealed class ConsoleTextWriter : TextWriter
{
    public override Encoding Encoding => Encoding.Default;
    public override void Write(char value)
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        KernelConsole.Default.Write(value);
        KernelConsole.Default.Canvas.Display();
    }
    public override void Write(string? value)
    {
        if (value is null)
        {
            return;
        }

        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        KernelConsole.Default.Write(value);
        KernelConsole.Default.Canvas.Display();
    }

    public override void Write(ReadOnlySpan<char> buffer)
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        KernelConsole.Default.Write(buffer);
        KernelConsole.Default.Canvas.Display();
    }
}
