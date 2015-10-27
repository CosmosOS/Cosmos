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

namespace DuNodes.Kernel.Base.Core
{
    public static unsafe class Memory
    {
        public static void MemAlloc(uint length)
        {
            Cosmos.Core.Heap.MemAlloc(length);
        }
        public static unsafe void MemRemove(byte start, uint offset, uint length)
        {
            if (offset >= length) return;
            byte* ptr = (byte*)start;
            for (uint i = offset; i < offset + length; i++ )
            {
                ptr[i] = (byte)0;
            }
        }
        public static unsafe void MemCopy(byte source, byte destination, uint offset, uint length)
        {
            if (offset >= length) return;
            byte* src = (byte*)source;
            byte* dst = (byte*)destination;
            for (uint i = offset; i < offset + length; i++)
            {
                dst[i] = src[i];
            }
        }
        public static unsafe void MemMove(byte source, byte destination, uint offset, uint length)
        {
            if (offset >= length) return;
            byte* src = (byte*)source;
            byte* dst = (byte*)destination;
            for (uint i = offset; i < offset + length; i++)
            {
                dst[i] = src[i];
                src[i] = 0;
            }
        }
        public static unsafe bool MemCompare(byte source1, byte source2, uint offset, uint length)
        {
            if (offset >= length) return false;
            byte* ptr1 = (byte*)source1;
            byte* ptr2 = (byte*)source2;
            for (uint i = offset; i < offset + length; i++)
            {
                if (ptr1[i] != ptr2[i]) return false;
            }
            return true;
        }
    }
}
