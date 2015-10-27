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

namespace DuNodes.Kernel.Base.Crypto
{
    /// <summary>
    /// A hash developed by Splitty
    /// </summary>
    public static class RockPotato
    {
        public static string Hash(string input)
        {
            if (input == "") input = "RockPotato";
            byte[] chars = new byte[input.Length + 1];

            // Fill byte array
            for (int i = 0; i < input.Length; i++)
            {
                chars[i] = (byte)(input[i]);
            }

            uint seed = 0;

            // Calculate the seed
            for (int i = 0; i < input.Length; i++)
            {
                seed += (uint)((chars[i] * chars[i]) >> (i + 1));
            }

            // Allocate memory
            Core.Memory.MemAlloc(sizeof(UInt64));

            UInt64 final = 0;

            // Calculate the value
            for (int i = 0; i < input.Length; i++)
            {
                final += seed * chars[i];
            }

            // Expand the value to a length of 20
            int attempts = 0;
            do
            {
                final *= 2;
                final <<= ++attempts + ++attempts;
            } while (final.ToString().Length < 16);

            return final.ToString();
        }
    }
}
