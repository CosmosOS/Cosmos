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

// Splitty was here
namespace DuNodes.Kernel.Base.IO
{
    public unsafe class MemoryStream : Stream
    {
        private bool eof;
        public bool EOF { get { return eof; } }
        private byte* data;
        private uint length;
        public MemoryStream(byte* dat)
        {
            eof = false;
            length = sizeof(uint);
            data = dat;
        }
        public MemoryStream(byte[] dat)
        {
            eof = false;
            length = (uint)dat.Length;
            fixed (byte* ptr = dat)
            {
                data = ptr;
            }
        }
        internal override byte ReadByte(uint p)
        {
            if (p > length) eof = true; else eof = false;
            if (!eof) return data[p];
            else return byte.MinValue;
        }
        internal override void WriteByte(uint p, byte b)
        {
            if (p > length) eof = true; else eof = false;
            if (!eof) data[p] = b;
        }
    }
}
