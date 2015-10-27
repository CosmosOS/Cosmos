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

using System;

namespace DuNodes.Kernel.Base.IO
{
    public abstract class Stream
    {
        public bool canRead;
        public bool canWrite;
        public uint Position;
        public byte Read()
        {
            if (canRead)
            {
                uint tmp = Position;
                Position++;
                return ReadByte(tmp);
            }
            else
                throw new Exception("Can not read!");
        }
        public void Write(byte b)
        {
            if (canWrite)
            {
                uint tmp = Position;
                Position++;
                WriteByte(tmp, b);
            }
            else
                throw new Exception("Can not Write!");
        }
        public virtual void Flush()
        {
        }
        public void Close()
        {
            Flush();
        }
        internal virtual byte ReadByte(uint p)
        {
            throw new Exception("Read not implemented!");
        }
        internal virtual void WriteByte(uint p, byte b)
        {
            throw new Exception("Write not implemented!");
        }
    }
}
