// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Text;
using Cosmos.Kernel.System.Sessions;

namespace Cosmos.Kernel.Plugs.System.IO;

internal sealed class ConsoleStream : Stream
{
    private readonly StringBuilder _readLineSB;
    private bool _canRead, _canWrite;

    public sealed override bool CanRead => _canRead;

    public sealed override bool CanWrite => _canWrite;

    public sealed override bool CanSeek => false;

    public sealed override long Length => throw new NotSupportedException();

    public sealed override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public ConsoleStream(FileAccess access)
    {
        _readLineSB = new();
        _canRead = (access & FileAccess.Read) == FileAccess.Read;
        _canWrite = (access & FileAccess.Write) == FileAccess.Write;
    }

    public override void Flush()
    {
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        ConsoleSession session = SessionManager.RequireCurrent();

        string value = Console.OutputEncoding.GetString(buffer);
        session.Write(value);
        session.Flush();
    }

    public override int Read(Span<byte> buffer)
    {
        if (buffer.IsEmpty)
        {
            return 0;
        }

        if (_readLineSB.Length == 0)
        {
            ConsoleSession? session = SessionManager.CurrentInput;
            string? line = session is null ? null : LineEditor.ReadLine(session);
            if (line is null)
            {
                return 0;
            }

            _readLineSB.Append(line);
            _readLineSB.Append(Environment.NewLine);
        }

        // Encode line into buffer.
        Encoder encoder = Console.InputEncoding.GetEncoder();
        int bytesUsedTotal = 0;
        int charsUsedTotal = 0;
        foreach (ReadOnlyMemory<char> chunk in _readLineSB.GetChunks())
        {
            encoder.Convert(chunk.Span, buffer, flush: false, out int charsUsed, out int bytesUsed, out bool completed);
            buffer = buffer.Slice(bytesUsed);
            bytesUsedTotal += bytesUsed;
            charsUsedTotal += charsUsed;
            if (!completed || buffer.IsEmpty)
            {
                break;
            }
        }
        _readLineSB.Remove(0, charsUsedTotal);
        return bytesUsedTotal;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        ValidateWrite(buffer, offset, count);
        Write(new ReadOnlySpan<byte>(buffer, offset, count));
    }

    public override void WriteByte(byte value)
    {
        SessionManager.RequireCurrent().Write((char)value);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ValidateRead(buffer, offset, count);
        return Read(new Span<byte>(buffer, offset, count));
    }

    public override int ReadByte()
    {
        byte b = 0;
        int result = Read(new Span<byte>(ref b));
        return result != 0 ? b : -1;
    }

    protected override void Dispose(bool disposing)
    {
        _readLineSB.Clear();
        _canRead = false;
        _canWrite = false;
        base.Dispose(disposing);
    }

    public sealed override void SetLength(long value) => throw new NotSupportedException();

    public sealed override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    private void ValidateRead(byte[] buffer, int offset, int count)
    {
        ValidateBufferArguments(buffer, offset, count);

        if (!_canRead)
        {
            throw new NotSupportedException();
        }
    }

    private void ValidateWrite(byte[] buffer, int offset, int count)
    {
        ValidateBufferArguments(buffer, offset, count);

        if (!_canWrite)
        {
            throw new NotSupportedException();
        }
    }
}
