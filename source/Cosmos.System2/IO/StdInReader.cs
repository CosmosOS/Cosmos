using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cosmos.System.Keyboard;

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
namespace Cosmos.System.IO
{

    // Heavily modified to be used on cosmos
    internal sealed class StdInReader : TextReader
    {
        private readonly StringBuilder readLineSB; // SB that holds readLine output.  This is a field simply to enable reuse; it's only used in ReadLine.
        private readonly Stack<KeyEvent> tmpKeys = new(); // temporary working stack; should be empty outside of ReadLine
        private readonly Stack<KeyEvent> availableKeys = new(); // a queue of already processed key infos available for reading
        private readonly Encoding encoding;

        internal StdInReader(Encoding encoding)
        {
            readLineSB = new StringBuilder();
            this.encoding = encoding;
        }

        /// <summary>
        /// Try to intercept the key pressed.
        /// </summary>
        public ConsoleKeyInfo ReadKey(out bool previouslyProcessed)
        {
            if (availableKeys.Count > 0)
            {
                previouslyProcessed = true;
                var key = availableKeys.Pop();

                return key.ToConsoleKeyInfo();
            }

            previouslyProcessed = false;
            return KeyboardManager.ReadKey().ToConsoleKeyInfo();
        }
        public override string? ReadLine()
        {
            bool isEnter = ReadLineCore(consumeKeys: true);
            string? line = null;
            if (isEnter || readLineSB.Length > 0)
            {
                line = readLineSB.ToString();
                readLineSB.Clear();
            }
            return line;
        }
        public bool ReadLineCore(bool consumeKeys)
        {

            // _availableKeys either contains a line that was already read,
            // or we need to read a new line from stdin.
            bool freshKeys = availableKeys.Count == 0;
            // Don't carry over chars from previous ReadLine call.
            readLineSB.Clear();
            KeyEvent current;
            int currentCount = 0;

            try
            {
                while (true)
                {
                    current = freshKeys ? KeyboardManager.ReadKey() : availableKeys.Pop();

                    if (!consumeKeys && current.Key is not ConsoleKeyEx.Backspace or ConsoleKeyEx.Delete) // backspace and delete are the only characters not written out below.
                    {
                        tmpKeys.Push(current);
                    }

                    if (current.Key is ConsoleKeyEx.Enter or ConsoleKeyEx.NumEnter)
                    {
                        if (freshKeys)
                        {
                            /* Do a 'WriteLine' */
                            Global.Console.CachedX = Global.Console.Cols;
                            Global.Console.UpdateCursorFromCache();
                        }
                        return true;
                    }

                    //Check for "special" keys
                    switch (current.Key)
                    {
                        case ConsoleKeyEx.Backspace:
                            {
                                bool removed = consumeKeys ? readLineSB.Length > 0 : tmpKeys.TryPop(out _);

                                if (removed && freshKeys && currentCount > 0)
                                {
                                    currentCount--;
                                    Global.Console.X--;
                                    readLineSB.Remove(currentCount, 1);

                                    //Move characters to the left
                                    /* We write directly to the TextScreen to only update console cursor once */
                                    Global.Console.Write(encoding.GetBytes(readLineSB.ToString(currentCount, readLineSB.Length - currentCount)));
                                    Global.Console.Write(encoding.GetBytes("\0")[0]);
                                    Global.Console.CachedX = Global.Console.X;
                                    Global.Console.CachedY = Global.Console.Y;
                                }
                                continue;
                            }
                        case ConsoleKeyEx.Delete:
                            {
                                bool removed = consumeKeys ? readLineSB.Length > currentCount : availableKeys.TryPop(out _);

                                if (removed && freshKeys)
                                {
                                    /* No Cursor Updates are needed */
                                    readLineSB.Remove(currentCount, 1);

                                    //Move characters to the left
                                    Global.Console.Write(encoding.GetBytes(readLineSB.ToString(currentCount, readLineSB.Length - currentCount)));
                                    Global.Console.Write(encoding.GetBytes("\0")[0]);
                                    Global.Console.CachedX = Global.Console.X;
                                    Global.Console.CachedY = Global.Console.Y;
                                }
                                continue;
                            }
                        case ConsoleKeyEx.LeftArrow:
                            if (currentCount > 0)
                            {
                                Global.Console.X--;
                                currentCount--;
                            }
                            continue;
                        case ConsoleKeyEx.RightArrow:
                            if (currentCount < readLineSB.Length)
                            {
                                Global.Console.X++;
                                currentCount++;
                            }
                            continue;
                    }

                    if (current.KeyChar == '\0')
                    {
                        continue;
                    }

                    //Write the character to the screen
                    if (currentCount == readLineSB.Length)
                    {
                        readLineSB.Append(current.KeyChar);

                        Global.Console.Write(encoding.GetBytes(new char[] { current.KeyChar }));
                        Global.Console.UpdateCursorFromCache();
                        currentCount++;
                    }
                    else
                    {
                        Global.Console.X++;
                        currentCount++;
                        readLineSB.Insert(currentCount, current.KeyChar);
 
                        //Shift the characters to the right
                        /* We write directly to the TextScreen to only update console cursor once */
                        Global.Console.Write(encoding.GetBytes(readLineSB.ToString(currentCount, readLineSB.Length - currentCount)));
                        Global.Console.CachedY = Global.Console.Y;
                        Global.Console.CachedX = Global.Console.X;
                    }
                }
            }
            finally
            {
                // If we're not consuming the read input, make the keys available for a future read
                while (tmpKeys.Count > 0)
                {
                    availableKeys.Push(tmpKeys.Pop());
                }
            }
        }
        public override int Read()
        {
            if (KeyboardManager.TryReadKey(out KeyEvent result))
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
            // If there aren't any keys in our processed keys stack, read a line to populate it.
            if (availableKeys.Count == 0)
            {
                ReadLineCore(consumeKeys: false);
            }

            // Now if there are keys, use the first.
            if (availableKeys.Count > 0)
            {
                KeyEvent keyInfo = availableKeys.Peek();
                if (keyInfo.KeyChar != '\0')
                {
                    return keyInfo.KeyChar;
                }
            }

            // EOL
            return -1;
        }

        internal int ReadLine(Span<byte> buffer)
        {

            if (buffer.IsEmpty)
            {
                return 0;
            }

            // Don't read a new line if there are remaining characters in the StringBuilder.
            if (readLineSB.Length == 0)
            {
                bool isEnter = ReadLineCore(consumeKeys: true);
                if (isEnter)
                {
                    readLineSB.Append(Environment.NewLine);
                }
            }

            // Encode line into buffer.
            Encoder encoder = encoding.GetEncoder();
            int bytesUsedTotal = 0;
            int charsUsedTotal = 0;
            foreach (ReadOnlyMemory<char> chunk in readLineSB.GetChunks())
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
            readLineSB.Remove(0, charsUsedTotal);
            return bytesUsedTotal;
        }
    }
}