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

namespace DuNodes.Kernel.Base.Crypto
{
    /// <summary>
    /// Rot13
    /// </summary>
	public static class ROT13
	{
        private static char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();
        private static char[] rot13 = "NOPQRSTUVWXYZABCDEFGHIJKLMnopqrstuvwxyzabcdefghijklm".ToCharArray();
        public static string encrypt(string str)
        {
            string retstr = string.Empty;
            for (int x = 0; x < str.Length; x++)
            {
                for (int y = 0; y < 53; y++)
                {
                    if (y > 51) { retstr += str[x]; }
                    else if (str[x] == alphabet[y]) { retstr += rot13[y]; break; }
                }
            }
            return retstr;
        }
        public static string decrypt(string str)
        {
            return encrypt(str);
        }
	}
    /// <summary>
    /// Holy cow...
    /// </summary>
    public static class ROT26
    {
        /// <summary>
        /// That's a joke, isn't it!?
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string encrypt(string str)
        {
            return "DUMBNESS DETECTED!\nrot26(str) == rot13(rot13(str)) == str";
        }
        /// <summary>
        /// That's a joke, isn't it!?
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string decrypt(string str)
        {
            return "DUMBNESS DETECTED!\nrot26(str) == rot13(rot13(str)) == str";
        }
    }
    public static class ROT47
    {
        private static char[] alphabet = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~".ToCharArray();
        private static char[] rot47 = "PQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNO".ToCharArray();
        public static string encrypt(string str)
        {
            string retstr = string.Empty;
            for (int x = 0; x < str.Length; x++)
            {
                for (int y = 0; y < 95; y++)
                {
                    if (y > 93) { retstr += str[x]; }
                    else if (str[x] == alphabet[y]) { retstr += rot47[y]; break; }
                }
            }
            return retstr;
        }
        public static string decrypt(string str)
        {
            return encrypt(str);
        }
    }
}
