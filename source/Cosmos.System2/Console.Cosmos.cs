using System;
using System.IO;
using Cosmos.Core;
using Cosmos.System.IO;

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Cosmos.System
{
    // Inspired on the UnixConsoleStream, but removed some functionalities that are not needed for this use case.
    //Modified to be used on cosmos
    public partial class Console
    {
        internal sealed class CosmosConsoleStream : ConsoleStream
        {
            internal CosmosConsoleStream(FileAccess access) : base(access)
            {
            }

            public override void Flush() => Global.Console.UpdateCursorFromCache();
            public override void Write(ReadOnlySpan<byte> buffer) => Global.Console.Write(buffer);
            public override int Read(Span<byte> buffer) => Global.Console.StdInReader.ReadLine(buffer);
        }
    }
}

