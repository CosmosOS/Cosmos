using System;
using System.IO;

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
namespace Cosmos.System.IO
{
    //Modified to be used on cosmos
    public sealed partial class SyncTextReader : TextReader
    {
        public bool KeyAvailable => KeyboardManager.KeyAvailable && Global.Console is not null;
        public bool IsStdIn => Inner is not null;
        internal StdInReader? Inner => _in as StdInReader;
        public ConsoleKeyInfo ReadKey(out bool previouslyProcessed) => Inner!.ReadKey(out previouslyProcessed);
        public int ReadLine(Span<byte> buffer) => Inner!.ReadLine(buffer);
    }
}
