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

namespace DuNodes.System.Console
{
    public static partial class Console
    {
        public class ProgressBar
        {
            private bool flicker = true;
            private int value = 0;
            public int Value
            {
                get { return value; }
                set
                {
                    if (value >= 0 && value <= 100)
                    {
                        this.value = value;
                    }
                }
            }
            /// <summary>
            /// Initialize a new ProgressBar
            /// </summary>
            /// <param name="startValue">Value</param>
            /// <param name="Flicker">true = Very cool effect =)</param>
            public ProgressBar(int startValue, bool Flicker = false)
            {
                this.Value = startValue;
                this.flicker = Flicker;
                this.Refresh();
            }
            public void Increment()
            {
                this.Value++;
                this.Refresh();
            }
            public void Decrement()
            {
                this.Value--;
                this.Refresh();
            }
            /// <summary>
            /// INFO: MaxValue is 100 and MinValue is 0.
            /// </summary>
            /// <param name="value"></param>
            public void Draw()
            {
                int ct = CursorTop;
                int cl = CursorLeft;
                WriteLine();
                string buffer = "[                                                  ] ";
                Write(buffer);
                CursorLeft = cl + 1;
                if (flicker)
                {
                    for (int i = 0; i < this.value / 2; i++)
                    {
                        if (this.value / 2 <= 50) Write("=");
                    }
                }
                else
                {
                    string __buffer = "";
                    for (int i = 0; i < this.value / 2; i++)
                    {
                        if (this.value / 2 <= 50) __buffer += "=";
                    }
                    Write(__buffer);
                }
                CursorLeft = cl + 54;
                Write(this.value.ToString() + "%");
                CursorTop = ct;
                CursorLeft = cl;
            }
            private void Refresh()
            {
                this.Draw();
            }
        }
    }
}
