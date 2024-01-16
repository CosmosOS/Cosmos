using System;
using System.IO;

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


//Modified to be used on cosmos
namespace Cosmos.System.IO
{
    // Provides the platform-agnostic functionality for streams used as console input and output.
    // Platform-specific implementations derive from ConsoleStream to implement Read and Write
    // (and optionally Flush), as well as any additional ctor/Dispose logic necessary.
    public abstract class ConsoleStream : Stream
    {
        private bool _canRead, _canWrite;

        internal ConsoleStream(FileAccess access)
        {
            _canRead = (access & FileAccess.Read) == FileAccess.Read;
            _canWrite = (access & FileAccess.Write) == FileAccess.Write;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ValidateWrite(buffer, offset, count);
            Write(new ReadOnlySpan<byte>(buffer, offset, count));
        }

        public override void WriteByte(byte value) => Write(new byte[] { value });

        public override int Read(byte[] buffer, int offset, int count)
        {
            ValidateRead(buffer, offset, count);
            return Read(new Span<byte>(buffer, offset, count));
        }

        public override int ReadByte()
        {
            byte b = 0;
            int result = Read(new Span<byte>(new byte[1] { b }));
            return result != 0 ? b : -1;
        }

        protected override void Dispose(bool disposing)
        {
            _canRead = false;
            _canWrite = false;
            base.Dispose(disposing);
        }

        public sealed override bool CanRead => _canRead;

        public sealed override bool CanWrite => _canWrite;

        public sealed override bool CanSeek => false;

        public sealed override long Length => throw new NotSupportedException();

        public sealed override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() { }

        public sealed override void SetLength(long value) => throw new NotSupportedException();

        public sealed override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        protected void ValidateRead(byte[] buffer, int offset, int count)
        {
            ValidateBufferArguments(buffer, offset, count);

            if (!_canRead)
            {
                throw new NotSupportedException();
            }
        }

        protected void ValidateWrite(byte[] buffer, int offset, int count)
        {
            ValidateBufferArguments(buffer, offset, count);

            if (!_canWrite)
            {
                throw new NotSupportedException();
            }
        }
    }
}

