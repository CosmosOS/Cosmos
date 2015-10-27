/*
Copyright (c) 2012-2013, dewitcher Team
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice
   this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System.Collections.Generic;

namespace DuNodes.Kernel.Base.Console
{
    public unsafe static partial class Console
    {
        public unsafe static class VideoRAM
        {
            internal class VideoBuffer
            {
                public string id;
                public byte[] data;
                public int X, Y;
            }
            private static Stack<VideoBuffer> vbufferStack = new Stack<VideoBuffer>();
            private static List<VideoBuffer> vbufferList = new List<VideoBuffer>();
            /// <summary>
            /// Pushes what is in video RAM onto a stack
            /// </summary>
            public static void PushContents()
            {
                VideoBuffer vb = new VideoBuffer();
                byte* VideoRam = (byte*)0xB8000;
                vb.data = new byte[4250];
                for (int i = 0; i < 4250; i++)
                {
                    byte b = VideoRam[i];
                    vb.data[i] = b;
                }
                vb.X = CursorLeft;
                vb.Y = CursorTop;
                vbufferStack.Push(vb);
            }
            /// <summary>
            /// Pops the content of the stack into Video RAM
            /// </summary>
            public static void PopContents()
            {
                VideoBuffer vb = vbufferStack.Pop();
                byte* VideoRam = (byte*)0xB8000;

                for (int i = 0; i < 4250; i++)
                {
                    VideoRam[i] = vb.data[i];

                }
                CursorLeft = vb.X;
                CursorTop = vb.Y;
            }
            /// <summary>
            /// Saves the Console content
            /// </summary>
            /// <param name="num"></param>
            /// <returns></returns>
            public static bool SetContent(string name)
            {
                bool found = false;
                for (int i = 0; i < vbufferList.Count; i++)
                {
                    if (vbufferList[i].id == name)
                    {
                        found = true;
                        // Set new content
                        byte* vram = (byte*)0xB8000;
                        vbufferList[i].data = new byte[4250];
                        vbufferList[i].id = name;
                        for (int j = 0; j < 4250; j++)
                        {
                            byte b = vram[j];
                            vbufferList[i].data[j] = b;
                        }
                        vbufferList[i].X = CursorLeft;
                        vbufferList[i].Y = CursorTop;
                        return true;
                    }
                }
                if (!found)
                {
                    VideoBuffer vb = new VideoBuffer();
                    byte* vram = (byte*)0xB8000;
                    vb.data = new byte[4250];
                    vb.id = name;
                    for (int i = 0; i < 4250; i++)
                    {
                        byte b = vram[i];
                        vb.data[i] = b;
                    }
                    vb.X = CursorLeft;
                    vb.Y = CursorTop;
                    vbufferList.Add(vb);
                    return true;
                }
                return false;
            }
            /// <summary>
            /// Restores the Console content
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public static bool RetContent(string name)
            {
                for (int i = 0; i < vbufferList.Count; i++)
                {
                    if (vbufferList[i].id == name)
                    {
                        // restore content
                        byte* vram = (byte*)0xB8000;
                        for (int j = 0; j < 4250; j++)
                        {
                            vram[j] = vbufferList[i].data[j];
                        }
                        CursorLeft = vbufferList[i].X;
                        CursorTop = vbufferList[i].Y;
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
