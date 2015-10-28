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

using DuNodes.System.Core;
//TODO Reactivate bsod
namespace DuNodes.System.Extensions
{
    /// <summary>
    /// Useful string extensions
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Checks if the string starts with [string]
        /// </summary>
        /// <param name="__str"></param>
        /// <param name="__expression"></param>
        /// <returns></returns>
        public static bool _StartsWith(this string __str, string __expression)
        {
            string str = "";
            if (__expression.Length <= __str.Length)
            {
                for (int i = 0; i < (__expression.Length); i++)
                {
                    str += __str[i];
                    if (str == __expression) return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Checks if the string ends with [string]
        /// </summary>
        /// <param name="__str"></param>
        /// <param name="__expression"></param>
        /// <returns></returns>
        public static bool _EndsWith(this string __str, string __expression)
        {
            string str = "";
            if (__expression.Length <= __str.Length)
            {
                for (int i = ((__str.Length - 1) - (__expression.Length - 1)); i == (__str.Length - 1); i++)
                {
                    str += __str[i];
                    if (str == __expression) return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Returns the char at position [string[index]]
        /// </summary>
        /// <param name="str"></param>
        /// <param name="null_based_index"></param>
        /// <returns></returns>
        public static char _GetCharAt(this string str, int null_based_index)
        {
            if (null_based_index >= 0 && null_based_index < str.Length)
                return str[null_based_index];
            else
            {
             //  Bluescreen.Init("string._GetCharAt", "null_based_index must be >= 0 and <= the length of the string");
                return char.MinValue;
            }
        }
        /// <summary>
        /// Removes the char at position [string[index]]
        /// </summary>
        /// <param name="str"></param>
        /// <param name="null_based_index"></param>
        /// <returns></returns>
        public static string _RemoveCharAt(this string str, int null_based_index)
        {
            if (null_based_index < str.Length)
            {
                string _str = "";
                for (int i = 0; i < null_based_index; i++) _str += str[i];
                for (int i = null_based_index + 1; i < str.Length; i++) _str += str[i];
                return _str;
            }
            else
            {
               // Bluescreen.Init("string._GetCharAt", "null_based_index must be >= 0 and <= the length of the string");
                return str;
            }
        }
    }
}
