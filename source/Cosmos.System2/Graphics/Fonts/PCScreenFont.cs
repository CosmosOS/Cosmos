using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Graphics.Fonts
{
    /// <summary>
    /// Represents a font in the PC Screen Font (PCF) format.
    /// </summary>
    public class PCScreenFont : Font
    {
        readonly List<UnicodeMapping> unicodeMappings; // Maps the fonts to the corresponding unicode characters

        #region DefaultFontData
        // Credit to fcambus https://github.com/fcambus/spleen under BSD-2 License
        //
        // Copyright (c) 2018-2020, Frederic Cambus
        // All rights reserved.

        // Redistribution and use in source and binary forms, with or without
        // modification, are permitted provided that the following conditions are met:

        //  * Redistributions of source code must retain the above copyright
        //    notice, this list of conditions and the following disclaimer.

        //  * Redistributions in binary form must reproduce the above copyright
        //    notice, this list of conditions and the following disclaimer in the
        //    documentation and/or other materials provided with the distribution.

        // THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
        // AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
        // IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
        // ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS
        // BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
        // CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
        // SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
        // INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
        // CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
        // ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
        // POSSIBILITY OF SUCH DAMAGE.

        static PCScreenFont _Default = null;
        public static PCScreenFont Default
        {
            get
            {
                if(_Default == null)
                {
                    _Default = LoadFont(Convert.FromBase64String("NgQDEAAAAAAAZjxmZmY8ZgAAAAAAABgYGBgYAAAYGBgYGAAAAABsbAAAAAAAAAAAAAAAAAAAAHyCmqKiopqCfAAAAAAAAAB8grqqsqqqgnwAAAAAAHwAAAAAAAAAAAAAAAAAAAAcJgwGJhwAAAAAAAAAAAAAAAAAAAAYPDwYAAAAAAAAAAwYMAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABgYMAAAGDgYGBg8AAAAAAAAAAAAAGCQIBKWbBgwcNSUHgQEADAYAHzGxsb+xsbGxgAAAAAYMAB8xsbG/sbGxsYAAAAAOGwAfMbGxv7GxsbGAAAAADJMAHzGxsb+xsbGxgAAAAAwGAB+wMDA+MDAwH4AAAAAOGwAfsDAwPjAwMB+AAAAAGxsAH7AwMD4wMDAfgAAAAAwGAB+GBgYGBgYGH4AAAAAAAB+1tbWdhYWFhYWAAAAAAA8ZmAwPGZmZmY8DAZmPAAMGAB+GBgYGBgYGH4AAAAAOGwAfhgYGBgYGBh+AAAAAGZmAH4YGBgYGBgYfgAAAAAAAHxmZmb2ZmZmZnwAAAAAMBgAfMbGxsbGxsZ8AAAAABgwAHzGxsbGxsbGfAAAAAA4bAB8xsbGxsbGxnwAAAAAMkwAfMbGxsbGxsZ8AAAAAAAAAAAAxmw4OGzGAAAAAAAAAnzGzs7W1ubmxnyAAAAAAAAAAAAAAAAAAAAAAAAAAAAAGBgYGBgYGAAYGAAAAAAAZmZmZgAAAAAAAAAAAAAAAABsbP5sbGxs/mxsAAAAAAAQftDQ0HwWFhYW/BAAAAAAAAZmbAwYGDA2ZmAAAAAAAAA4bGxsOHDazMx6AAAAAAAYGBgYAAAAAAAAAAAAAAAADhgwMGBgYGBgYDAwGA4AAHAYDAwGBgYGBgYMDBhwAAAAAABmPBj/" +
                    "GDxmAAAAAAAAAAAAABgYfhgYAAAAAAAAAAAAAAAAAAAAABgYMAAAAAAAAAAAAAB+AAAAAAAAAAAAAAAAAAAAAAAAGBgAAAAAAAMDBgYMDBgYMDBgYMDAAAAAfMbGzt725sbGfAAAAAAAABg4eFgYGBgYGH4AAAAAAAB8xgYGDBgwYMb+AAAAAAAAfMYGBjwGBgbGfAAAAAAAAMDAzMzMzP4MDAwAAAAAAAD+xsDA/AYGBsZ8AAAAAAAAfMbAwPzGxsbGfAAAAAAAAP7GBgYMGDAwMDAAAAAAAAB8xsbGfMbGxsZ8AAAAAAAAfMbGxsZ+BgbGfAAAAAAAAAAAABgYAAAAGBgAAAAAAAAAAAAYGAAAABgYMAAAAAAABgwYMGBgMBgMBgAAAAAAAAAAAH4AAH4AAAAAAAAAAABgMBgMBgYMGDBgAAAAAAAAfMYGDBgwMAAwMAAAAAAAAAB8wtra2trewHwAAAAAAAB8xsbG/sbGxsbGAAAAAAAA/MbGxvzGxsbG/AAAAAAAAH7AwMDAwMDAwH4AAAAAAAD8xsbGxsbGxsb8AAAAAAAAfsDAwPjAwMDAfgAAAAAAAH7AwMD4wMDAwMAAAAAAAAB+wMDA3sbGxsZ+AAAAAAAAxsbGxv7GxsbGxgAAAAAAAH4YGBgYGBgYGH4AAAAAAAB+GBgYGBgYGBjwAAAAAAAAxsbGzPjMxsbGxgAAAAAAAMDAwMDAwMDAwH4AAAAAAADG7v7WxsbGxsbGAAAAAAAAxsbm5tbWzs7GxgAAAAAAAHzGxsbGxsbGxnwAAAAAAAD8xsbG/MDAwMDAAAAAAAAAfMbGxsbGxtbWfBgMAAAAAPzGxsb8xsbGxsYAAAAAAAB+wMDAfAYGBgb8AAAAAAAA/xgYGBgYGBgYGAAAAAAAAMbGxsbGxsbGxn4AAAAAAADGxsbGxsbGbDgQAAAAAAAAxsbGxsbG1v7u" +
                    "xgAAAAAAAMbGxmw4bMbGxsYAAAAAAADGxsbGfgYGBgb8AAAAAAAA/gYGDBgwYMDA/gAAAAAAPjAwMDAwMDAwMDAwMD4AAMDAYGAwMBgYDAwGBgMDAAB8DAwMDAwMDAwMDAwMfAAAEDhsxgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD+AAAwGAwAAAAAAAAAAAAAAAAAAAAAAHwGfsbGxn4AAAAAAADAwMD8xsbGxsb8AAAAAAAAAAAAfsDAwMDAfgAAAAAAAAYGBn7GxsbGxn4AAAAAAAAAAAB+xsb+wMB+AAAAAAAAHjAwMHwwMDAwMAAAAAAAAAAAAH7GxsbGxnwGBvwAAADAwMD8xsbGxsbGAAAAAAAAGBgAOBgYGBgYHAAAAAAAABgYABgYGBgYGBgYGHAAAADAwMDM2PDw2MzGAAAAAAAAMDAwMDAwMDAwHAAAAAAAAAAAAOzW1tbWxsYAAAAAAAAAAAD8xsbGxsbGAAAAAAAAAAAAfMbGxsbGfAAAAAAAAAAAAPzGxsbGxvzAwMAAAAAAAAB+xsbGxsZ+BgYGAAAAAAAAfsbAwMDAwAAAAAAAAAAAAH7AwHwGBvwAAAAAAAAwMDB8MDAwMDAeAAAAAAAAAAAAxsbGxsbGfgAAAAAAAAAAAMbGxsZsOBAAAAAAAAAAAADGxtbW1tZuAAAAAAAAAAAAxmw4OGzGxgAAAAAAAAAAAMbGxsbGxn4GBvwAAAAAAAD+BgwYMGD+AAAAAAAOGBgYGBhwcBgYGBgYDgAAABgYGBgYGBgYGBgYGAAAAHAYGBgYGA4OGBgYGBhwAAAyfkwAAAAAAAAAAAAAAAAwGADGxsbGxsbGxn4AAAAAAAB+wMDAwMDAwMB+GBgwAAAAbGwAxsbGxsbGfgAAAAAADBgwAH7Gxv7AwH4AAAAAABA4bAB8Bn7GxsZ+AAAAAAAAbGwAfAZ+xsbGfgAA" +
                    "AAAAYDAYAHwGfsbGxn4AAAAAADhsOAB8Bn7GxsZ+AAAAAAAAAAAAfsDAwMDAfhgYMAAAEDhsAH7Gxv7AwH4AAAAAAABsbAB+xsb+wMB+AAAAAABgMBgAfsbG/sDAfgAAAAAAAGZmADgYGBgYGBwAAAAAABg8ZgA4GBgYGBgcAAAAAAAwGAwAOBgYGBgYHAAAAABsbAB8xsbG/sbGxsYAAAAAOGw4fMbGxv7GxsbGAAAAABgwAH7AwMD4wMDAfgAAAAAAAAAAAG4WFn7Q0G4AAAAAAAB+2NjY/tjY2NjeAAAAAAAQOGwAfMbGxsbGfAAAAAAAAGxsAHzGxsbGxnwAAAAAAGAwGAB8xsbGxsZ8AAAAAAAQOGwAxsbGxsbGfgAAAAAAYDAYAMbGxsbGxn4AAAAAAABsbADGxsbGxsZ+Bgb8AGxsAHzGxsbGxsbGfAAAAABsbADGxsbGxsbGxn4AAAAAAAAAAAh+yMjIyMh+CAAAAAAAOGxgYGD4YGDA/gAAAAAAAMPDZjwYPBg8GBgAAAAAGDAAxsbGxsbGxsZ+AAAAADhsAMbGxsbGxsbGfgAAAAAADBgwAHwGfsbGxn4AAAAAAAwYMAA4GBgYGBgcAAAAAAAMGDAAfMbGxsbGfAAAAAAADBgwAMbGxsbGxn4AAAAAADJ+TAD8xsbGxsbGAAAAADJMAMbm5tbWzs7GxgAAAAAAOAw8TDwAfAAAAAAAAAAAADhsbDgAAHwAAAAAAAAAAAAAGBgAGBgwYMDGfAAAAAAYMADGxsbGfgYGBvwAAAAAAAAAAAAA/gYGBgAAAAAAAABAwEBCRuwYMGzSggwQHgAAQMBAQkbsGDBw1JQeBAQAAAAYGAAYGBgYGBgYAAAAAAAAAAAAADNmzGYzAAAAAAAAAAAAAADMZjNmzAAAAAAAEUQRRBFEEUQRRBFEEUQRRFWqVapVqlWqVapVqlWqVardd9" +
                    "133Xfdd9133Xfdd913GBgYGBgYGBgYGBgYGBgYGBgYGBgYGBj4GBgYGBgYGBgYGBgYGBj4GPgYGBgYGBgYNjY2NjY2NvY2NjY2NjY2NgAAAAAAAAD+NjY2NjY2NjYAAAAAAAD4GPgYGBgYGBgYNjY2NjY29gb2NjY2NjY2NjY2NjY2NjY2NjY2NjY2NjYAAAAAAAD+BvY2NjY2NjY2NjY2NjY29gb+AAAAAAAAADY2NjY2Njb+AAAAAAAAAAAYGBgYGBj4GPgAAAAAAAAAAAAAAAAAAPgYGBgYGBgYGBgYGBgYGBgfAAAAAAAAAAAYGBgYGBgY/wAAAAAAAAAAAAAAAAAAAP8YGBgYGBgYGBgYGBgYGBgfGBgYGBgYGBgAAAAAAAAA/wAAAAAAAAAAGBgYGBgYGP8YGBgYGBgYGBgYGBgYGB8YHxgYGBgYGBg2NjY2NjY2NzY2NjY2NjY2NjY2NjY2NzA/AAAAAAAAAAAAAAAAAD8wNzY2NjY2NjY2NjY2Njb3AP8AAAAAAAAAAAAAAAAA/wD3NjY2NjY2NjY2NjY2NjcwNzY2NjY2NjYAAAAAAAD/AP8AAAAAAAAANjY2NjY29wD3NjY2NjY2NhgYGBgYGP8A/wAAAAAAAAA2NjY2NjY2/wAAAAAAAAAAAAAAAAAA/wD/GBgYGBgYGAAAAAAAAAD/NjY2NjY2NjY2NjY2NjY2PwAAAAAAAAAAGBgYGBgYHxgfAAAAAAAAAAAAAAAAAB8YHxgYGBgYGBgAAAAAAAAAPzY2NjY2NjY2NjY2NjY2Nv82NjY2NjY2NhgYGBgYGP8Y/xgYGBgYGBgYGBgYGBgY+AAAAAAAAAAAAAAAAAAAAB8YGBgYGBgYGP////////////////////8AAAAAAAAAAP//////////8PDw8PDw8PDw8PDw8PDw8A8PDw8PDw8PDw8PDw8PDw///////////w" +
                    "AAAAAAAAAAAAAAwMD8xsbGxsb8wMAAAAAAeMzMzPjMxsbm3AAAAAAAMn5MAHwGfsbGxn4AAAAAANhw2Ax8xsbGxsZ8AAAAAAAyfkwAfMbGxsbGfAAAAAAAAAAAAnzGztbmxnyAAAAAAAAAAADMzMzMzMz2wMDAAAAMGDAAxsbGxsbGfgYG/AAAAMDAwPzGxsbGxvzAwMAAAHwAfMbGxv7GxsbGAAAAAAAAAHwAfAZ+xsbGfgAAAABsOAB8xsbG/sbGxsYAAAAAAGxsOAB8Bn7GxsZ+AAAAAAAAfMbGxv7GxsbGxgwIBgAAAAAAAHwGfsbGxn4MCAYAGDAAfsDAwMDAwMB+AAAAAAAMGDAAfsDAwMDAfgAAAAAAAAAAABgYfhgYAH4AAAAAOGwAfsDAwMDAwMB+AAAAAAAQOGwAfsDAwMDAfgAAAAAYGAB+wMDAwMDAwH4AAAAAAAAYGAB+wMDAwMB+AAAAAAAAAAAYGAB+ABgYAAAAAABsOBB+wMDAwMDAwH4AAAAAADhsbDgAAAAAAAAAAAAAAABsOBAAfsDAwMDAfgAAAAAAAAAAAAAAGBgAAAAAAAAAbDgQ/MbGxsbGxsb8AAAAAGw4FgYGfsbGxsbGfgAAAAAAOEwMOGB8AAAAAAAAAAAAAAAGHwZ+xsbGxsZ+AAAAAAB8AH7AwMD4wMDAfgAAAAAAAAB8AH7Gxv7AwH4AAAAAGBgAfsDAwPjAwMB+AAAAAAAAGBgAfsbG/sDAfgAAAAAAAH7AwMD4wMDAwH4MCAYAAAAAAAB+xsb+wMB+DAgGAGw4EH7AwMD4wMDAfgAAAAAAbDgQAH7Gxv7AwH4AAAAAOGwAfsDAwN7GxsZ+AAAAAAAQOGwAfsbGxsbGfAYG/ABsOAB+wMDA3sbGxn4AAAAAAGxsOAB+xsbGxsZ8Bgb8ABgYAH7AwMDexsbGfgAAAAAAABgYAH7GxsbGxnwGBvwA" +
                    "AAB+wMDA3sbGxsZ+GBgwAAAYGDAAfsbGxsbGfAYG/AA4bADGxsbG/sbGxsYAAAAAOGwAwMDA/MbGxsbGAAAAAAAAxv/Gxv7GxsbGxgAAAAAAAMDwwPzGxsbGxsYAAAAAMkwAfhgYGBgYGBh+AAAAAAAyfkwAOBgYGBgYHAAAAAAAfgB+GBgYGBgYGH4AAAAAAAAAfgA4GBgYGBgcAAAAAAAAfhgYGBgYGBgYfgwIBgAAABgYADgYGBgYGBwMCAYAGBgAfhgYGBgYGBh+AAAAAAAAAAAAOBgYGBgYHAAAAAA4bAB+GBgYGBgYGPAAAAAAABg8ZgAYGBgYGBgYGBhwAAAAxsbGzPjMxsbGxhgYMAAAAMDAwMzY8PDYzMYYGDAAAAAAAADGzNjw2MzGAAAAABgwAMDAwMDAwMDAfgAAAAAYMAAwMDAwMDAwMBwAAAAAAADAwMDAwMDAwMB+GBgwAAAAMDAwMDAwMDAwHBgYMABsOBDAwMDAwMDAwH4AAAAAbDgQMDAwMDAwMDAcAAAAAAAAYGBoeHDg4GBgPgAAAAAAADAwNDw4cHAwMBwAAAAAGDAAxubm1tbOzsbGAAAAAAAMGDAA/MbGxsbGxgAAAAAAAMbG5ubW1s7OxsYYGDAAAAAAAAD8xsbGxsbGGBgwAGw4EMbm5tbWzs7GxgAAAAAAbDgQAPzGxsbGxsYAAAAAAADGxubm1tbOzsbGBgYMAAAAAAAA/MbGxsbGxgYGDAAAfAB8xsbGxsbGxnwAAAAAAAAAfAB8xsbGxsZ8AAAAAGbMAHzGxsbGxsbGfAAAAAAANmzYAHzGxsbGxnwAAAAAAAB+2NjY3tjY2Nh+AAAAAAAAAAAAbtbW3tDQbgAAAAAYMAD8xsbG/MbGxsYAAAAAAAwYMAB+xsDAwMDAAAAAAAAA/MbGxvzGxsbGxhgYMAAAAAAAAH7GwMDAwMAYGDAAbDgQ/" +
                    "MbGxvzGxsbGAAAAAABsOBAAfsbAwMDAwAAAAAAYMAB+wMDAfAYGBvwAAAAAAAwYMAB+wMB8Bgb8AAAAADhsAH7AwMB8BgYG/AAAAAAAEDhsAH7AwHwGBvwAAAAAAAB+wMDAfAYGBgb8GBgwAAAAAAAAfsDAfAYG/BgYMABsOBB+wMDAfAYGBvwAAAAAAGw4EAB+wMB8Bgb8AAAAAAAA/xgYGBgYGBgYGAwMGAAAADAwMHwwMDAwMB4MDBgAbDgQ/xgYGBgYGBgYAAAAAGw4EDAwfDAwMDAwHgAAAAAAAP8YGBgYfhgYGBgAAAAAAAAwMDB8MHwwMDAeAAAAADJMAMbGxsbGxsbGfAAAAAAAMn5MAMbGxsbGxnwAAAAAAHwAxsbGxsbGxsZ+AAAAAAAAAHwAxsbGxsbGfgAAAABsOADGxsbGxsbGxn4AAAAAAGxsOADGxsbGxsZ+AAAAADhsOMbGxsbGxsbGfgAAAAAAOGw4AMbGxsbGxn4AAAAAZswAxsbGxsbGxsZ+AAAAAAA2bNgAxsbGxsbGfgAAAAAAAMbGxsbGxsbGxn4MCAYAAAAAAADGxsbGxsZ+DAgGAGxsAMbGxsZ+BgYG/AAAAAAYMAD+BgwYMGDAwP4AAAAAAAwYMAD+BgwYMGD+AAAAABgYAP4GDBgwYMDA/gAAAAAAABgYAP4GDBgwYP4AAAAAbDgQ/gYMGDBgwMD+AAAAAABsOBAA/gYMGDBg/gAAAAAAGBgYGAAAAAAAAAAAAAAAABgYGBgAAAAAAAAAAAAAAABmZmZmAAAAAAAAAAAAAAAAZmZmZgAAAAAAAAAAAAAAAAAAHDZg+GD4YDYcAAAAAAAAAAAAAAAAAADb2wAAAAArvxjek8mxXt++clq7QmTG2JO3FXQci2SR9d4pRkLsb8ogFfAGJ2Enh+BuQ1DFG8W0N8Nppu6Ar2+bk6F2oSP1JHJT81tlGf" +
                    "T8k90m6KYQ9PfJzpJI9pRvYOwHxLmXbaS/EQ3GtB9NE7BdujEnKdWNUYduNroAlnrwwyADf9jaF9vJlBnUv+iD4vaReWqm4ZU4/yiys/ymp9iu+FTMKNyaa/vedj/Y17whesh/kXEJVG2VFqyWPL713ROKYgC3DQXCoe6MaWQyTjWcXyl1zS63eiSiPh/fGsFhjhRgoMpr0426fUN/fX3Z61yLmpxwtDfETskW5u73nhEcoRKOO5zvkpN2Hjz/jt1JutJ9lxwospjibjUurzdDOX5EcjxLpr+xbMJlX6SI5qfuAiykxK3FweU3opifXRJAXHuASj2p5asL1pIvBnmQKpykTvTEohEhwXq2HaRltMi0RMciRhQ2Mp6bjJEgEuInkFFVNK0KCtAQtWjj8ABZ8P1b+gZX2HC0khouPa/8g+Z3eWjCHSWyQocQJmgVowQ9lClDEyUWJHe4eo5/FEtLVBUZMxss1dPsiXizgsT7dqvk2sC3FsxfdV3GmA4fj6cQhdcoAOY3L/fRtZauxjs3X9IpnB9vAIR2sAhngEf/XV7zFJ3LW/gubskeiD6dxi5971BcGV/j5fwbJu/I4EMgiVhiXnm67n559Exy0mG6Wancqs7tUnqRbPIWbE+zWogdvO0j8QZmMxoJaHGuQs8ZpYJ/J6IbNuL8BNweIpEh3YtxUW73zrjY7h13ZGFAIp+efYTIR2wbO+aU0Mi1V+80udVXU1aHjQR0f0CCPpxL6pFLfCSkIdODZyi3e7ja1nnpFSqtxOlt21mrN3wzaWLeY8SRYOfZSNz4wykuhE/7t5RrajT04QEUS975pGveUP4TRtQIw2I7DFT5v5ifnJJVnWW8BWOdmfDJ0tKdT9ELMZUtcICCd1AqwGyS2Mof7ZRxqTkKVBhprYgR0RtP5SUq1wX+j5ZOIfMZIIljN2Qy9Yq1VdOKxgnwx3TsIA/lQuVgBJS" +
                    "aEHXIbiU06rxASif22C6tUirLra4/Oq5PXdjdwR45+gqZhWcyrITId0Eeop2IHkAHYECbBQZF8b6DQvmq1QyJVFodLffUnKwD/yIKbr/FngkvEWOV//Un1mkPJueyckExV7xZ2UOeHbosCIQqqFp5h/jjStFSJq+6Er9k+oRngHrG3LrPhXdjw3XbMvB0XC0lPJF+w17PvLnpMGQaHirvMdPrK+Y1BSoyr+EVMrvWq8wJvHvTwME+R2KsqshTuRYvYI2NrHKTXfiW3gYJvzRbWqbfN/zMCWH/eJYE/SYMJf41tbpYEzziHJteU10bCWTE7YmHBh1uGTF2VuJtkpNfXCcG/ioeq5a6UVpzZk5g5RjbEj8vnpU2dZvuD2h/drvktBGDgw7AGkrp3V8GswyV8dpykc6eSIjHFMY/0VcULmOW3EA5UJPwt0IV+oRZdlrMOKApeGNVj7PDIAKww0MjVMU65aKSWF21E1UmEURvRMtdBYkTy5Y7/2jHjNsP0qAKdHY+11GnoeqlhO22jIN5YCt8GWdsmDNpkWrjOKvOlviA0pywKKL4WKaIRgzsToxgGmyAqrZgL+TpJ+AbXAHisNhPtP9veuWVjRX8XCtKxf2Iaa+Cotjb5FjJljiHO3kAUk9/eZTfUUoUdzCK8KrQMb/tM/xiUss3eDiQtirXP718hJPjvcLveU/I9WN7r2EIKZs5CZO5+NnhlnHO8v4MteGqfu4yB3URyBaPvsH+OllGn9nkeNlIX8aTapMBNGRPQZ+ZWQuJGUo//sv8bHgqZceOGf29Sa9obmsURsycUEg+Qd27mxHoQXg38VlEFXDkbWkv1jWAS434GiPJpofuLAGgMJctmlhtnYzcnno7DvUUIXtgJXwea/5DeqtwuS7hTga0Gn2Gu0+7xqWlWR+OeXQ4xkWVk8okhz0TSOr3MPOVSI0heC3r78oNdD3amadZG+05vS" +
                    "Do3fjQ4VKNLFbk83Lrq+jBTdiq2DPm4aIJYTIVfJBGyvctYWl4J8fExT6NI6AKAX/bTrq7dJbL37EU79ToEOpkyr0xJ2uUFvdersZ2h/X55riXF98pPt+NFeHDweGvcuZMr0ZtFxTxqfU02qjKxDoUxWcRrOH88aDbEhYIMwkQWj7KQklVVUxqaa1OvQWSq6LWjkT4fizglazm1BpKfw3ISwDC5DnUziQ0w6PGA3t9iAn/RRWvYg3hmFKqY0An6KjAj7LIetql9cISOKdKyb3A/aGl9WgHrdB3JaexCuZrVGIkFYARUHOq/vbNyfWsGSprbLHbxwdtO84bydclVkYG6ZLnA4Xz0VuuhzlCZeVgP51WiCvqJ/eytGb0/2HfktiWlXx9amHttAM/w/k88TgCSIbvxTZzzIA6Gc40N5XbSHAc7+iSi8Rh89aiNDIzkLVJYnJ/jkn0F84ECqAySxzCnqkZ/Wn8bRfQviwjMZpK2b/XUS39fw5lKX2WAednOLFlNIv4DIAGzZ/XLOJLQqHqCX+xvk8wNYgxmzPPAfhkj0IqGrGKrLSWs8vaY/iRwG4jC5r2IJc8amxU/ZvbfRWBr+Rwi9a4PZsJlgvgLlobueZ7Oi/RksnDYw5PIi313/X7PyO57Ua+weK++C/EcPv/qKX3vf1Awv2SH6CRR6XRxM+32VSpdfhxH7o+m6k/yC68icBi0TeXglW2uIRmHMhZzoz0yszfswXg/HulaOKEAVPuNE/e3ujnWLHcOYr2PGDYf0cgtevAWuU0HkODLQznQ9IJSddf+oUC4V5xbd0EeW6QUoxHAA4JKDrVExtBgSIlJDpYx7qR72dAEZIDQZZps1KN34cVXb5BIy/5aRJ5s2BJiYsqbJvUPIkvcQtRP16XRzFGYR0KQQoM4cWIcmOIi/DAWoxENsFExx9cMrfVqZjNGOQpzGuU/gMyNxQQoXC1tjwTAM" +
                    "KRSshOrdJLg1yKrhAauURGUsfdrBcOEwHiUQwTSOAiniMZiJsldA/eLHbQlXhnhhod3xINcXu2Ny0rahC55PLsvF2yrtim9vsNaqDI00XCJOl0WgT5lWaBLVM6WVftM1nyJlhDiG6hu8q+sxomTAnOUmIn7FM4pYIvgEMHjzKuB2Eh5oJG/U2UwAD+L0xSGSZaM0v1yOA9J18W2szttu2ESjcalC104DzkVeKUjiSCVtuYX5ws+UQXtzjn2d2u5mEGlQCxi14BgMHYXIe/6/X7IJxs4m0ebagyG1giOo8W6NeZagr5W8br84fNR1dvSdEJ7FmaStMsw6kocGEkHopjjSNqL9KGcJmapAD//6YA//+oAP//qQD//64A//+vAMkC//+zAP//IiDPJf//tAD//7gA//+5AP//vgD//8AA///BAP//wgD//8MA///IAP//ygD//8sAAQT//8wA//+2AP//pwD//80A///OAP//zwAHBP//0AAQAf//0gD//9MA///UAP//1QD//9cA///YAP//IACgAAAgASACIAMgBCAFIAYgByAIIAkgCiAvIP//IQD//yIA//8jAP//JAD//yUAlSL//yYA//8nAP//KAD//ykA//8qAJsi//8rAP//LAD//y0AEiATIBIirQAQIBEgliL//y4AmSL//y8AmCL//zAA6iT//zEAYCT//zIAYST//zMAYiT//zQAYyT//zUAZCT//zYAZST//zcAZiT//zgAZyT//zkAaCT//zoA//87AP//PAD//z0AnCL//z4A//8/AP//QAD//0EAEASRA7Yk//9CABIEkgO3JP//QwAhBLgk//9EALkk//9FABUElQO6JP//RgC7JP//RwC8JP//SAAdBJcDvST//0kABgSZA74k//9KAAgEvyT//0sAGgSaAyohwCT//0wAwST//00AHAScA8Ik//9OAJ0DwyT//0" +
                    "8AHgSfA8Qk//9QACAEoQPFJP//UQDGJP//UgDHJP//UwAFBMgk//9UACIEpAPJJP//VQDKJP//VgDLJP//VwDMJP//WAAlBKcDzST//1kArgTOJP//WgCWA88k//9bAP//XAD//10A//9eAP//XwD//2AA//9hADAE0CT//2IA0ST//2MAQQTSJP//ZADTJP//ZQA1BNQk//9mANUk//9nANYk//9oANck//9pAFYE2CT//2oAWATZJP//awDaJP//bADbJP//bQDcJP//bgDdJP//bwA+BN4k//9wAEAE3yT//3EA4CT//3IA4ST//3MAVQRzAOIk//90AOMk//91AOQk//92AOUk//93AOYk//94AEUE5yT//3kAQwToJP//egDpJP//ewD//3wA//99AP//fgD//9kA///HAP///AD//+kA///iAP//5AD//+AA///lAP//5wD//+oA///rAFEE///oAP//7wBXBP//7gD//+wA///EAP//xQArIf//yQD//+YA///GAP//9AD///YA///yAP//+wD///kA////AP//1gD//9wA//+iAP//owD//6UA///aAP//2wD//+EA///tAP//8wD///oA///xAP//0QD//6oA//+6AP//vwD//90A//+sAP//vQD//7wA//+hAP//qwBqIv//uwBrIv//kSX//5Il//+TJf//AiUDJX8lfSV7JXcleSV1Jf//JCUrJSolKSUoJSclJiUlJf//YSX//2Il//9WJf//VSX//2Ml//9RJf//VyX//10l//9cJf//WyX//xAlEyUSJREl//8UJRclFiUVJf//NCU7JTolOSU4JTclNiU1Jf//LCUzJTIlMSUwJS8lLiUtJf//HCUjJSIlISUgJR8lHiUdJf//ACUBJX4lfCV6JXYleCV0Jf//PCVLJUolSSVIJUclRiVFJUQlQyVCJUElQCU/JT4" +
                    "lPSX//14l//9fJf//WiX//1Ql//9pJf//ZiX//2Al//9QJf//bCX//2cl//9oJf//ZCX//2Ul//9ZJf//WCX//1Il//9TJf//ayX//2ol//8YJRslGiUZJf//DCUPJQ4lDSX//4gl//+EJf//jCX//5Al//+AJf//3gD//98A///jAP//8AD///UA///4AP//tQC8A////QD///4A//8AAf//AQH//wIB0AT//wMB0QT//wQB//8FAf//BgH//wcB//+xAP//CAH//wkB//8KAf//CwH///cA//8MAf//sAD//w0B//+3AP//DgH//w8B//+yAP//EQH//xIB//8TAf//FgH//xcB//8YAf//GQH//xoB//8bAf//HAH//x0B//8eAf//HwH//yAB//8hAf//IgH//yMB//8kAf//JQH//yYB//8nAf//KAH//ykB//8qAf//KwH//y4B//8vAf//MAH//zEB//80Af//NQH//zYB//83Af//OAH//zkB//86Af//OwH//zwB//89Af//PgH//0EB//9CAf//QwH//0QB//9FAf//RgH//0cB//9IAf//SgH//0sB//9MAf//TQH//1AB//9RAf//UgH//1MB//9UAf//VQH//1YB//9XAf//WAH//1kB//9aAf//WwH//1wB//9dAf//XgH//18B//9gAf//YQH//2IB//9jAf//ZAH//2UB//9mAf//ZwH//2gB//9pAf//agH//2sB//9sAf//bQH//24B//9vAf//cAH//3EB//9yAf//cwH//3gB//95Af//egH//3sB//98Af//fQH//34B//8YIP//GSD//xwg//8dIP//rCD//yYg/////////////////////////////////////////////////////////////////////////////////////////////////////////" +
                    "////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////w=="));
                }

                return _Default;
            }
        } 
        #endregion

        enum PSFVersion1Mode
        {
            MODE512 = 1,
            HASTAB = 2,
            HASSEQ = 4,
            MAXMODE = 5
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PCScreenFont"/> class.
        /// </summary>
        /// <param name="width">The width of a single character in pixels</param>
        /// <param name="height">The height of a single character in pixels</param>
        /// <param name="data">The PCF data.</param>
        /// <param name="unicodeMappings">The mappings of Unicode characters to font indexes.</param>
        public PCScreenFont(byte width, byte height, byte[] data, List<UnicodeMapping> unicodeMappings) : base(width, height, data)
        {
            this.unicodeMappings = unicodeMappings;
        }

        /// <summary>
        /// Loads the given PC Screen Font using the given raw data array.
        /// </summary>
        /// <param name="fontData">The raw PCF data.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Thrown when a PCF version 2 file is provided.</exception>
        /// <exception cref="ArgumentException">Thrown when the provided font data is incorrect.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fontData"/> is <see langword="null"/>.</exception>
        public static PCScreenFont LoadFont(byte[] fontData)
        {
            byte charHeight;
            byte charWidth;
            byte[] parsedFontData;
            var mappings = new List<UnicodeMapping>();

            bool version1 = fontData[0] == 0x36 && fontData[1] == 0x04;
            bool version2 = BitConverter.ToUInt32(fontData, 0) == 0x864ab572;

            // Check the header
            if (!version1 && !version2)
            {
                Global.Debugger.Send($"PCF load error: Invalid magic {fontData[0]} {fontData[1]} {fontData[2]} {fontData[3]}");
                throw new Exception($"Invalid magic value {fontData[0]} {fontData[1]} {fontData[2]} {fontData[3]}.");
            }

            if (version1)
            {
                byte mode = fontData[2];
                charHeight = fontData[3];
                charWidth = 8; //Always 8 in this case
                int length = (mode & (int)PSFVersion1Mode.MODE512) == 1 ? 512 : 256;
                bool hasUnicodeTable = (mode & (int)PSFVersion1Mode.HASTAB) > 0;
                ushort seperator = 0xFFFF;
                ushort sequenceStart = 0xFFFE;
                parsedFontData = new byte[length * charHeight]; //Every row is one byte

                for (int i = 0; i < length; i++)
                {
                    for (int k = 0; k < charHeight; k++)
                    {
                        parsedFontData[(i * charHeight) + k] = fontData[4 + (i * charHeight) + k];
                    }
                }

                int position = 4 + (length * charHeight);
                if (hasUnicodeTable)
                {
                    mappings = new List<UnicodeMapping>();
                    var currentEntry = new List<byte>();
                    while (position < fontData.Length)
                    {
                        if (BitConverter.ToUInt16(fontData, position) == seperator)
                        {
                            var mapping = new UnicodeMapping
                            {
                                FontPosition = mappings.Count,
                                UnicodeCharacters = new List<ushort>(),
                                UnicodeCharactersWithModifiers = new List<ushort[]>(),
                                ASCIICharacters = new List<byte>()
                            };
                            for (int i = 0; i < currentEntry.Count / 2; i++)
                            {
                                mapping.UnicodeCharacters.Add(BitConverter.ToUInt16(currentEntry.ToArray(), i * 2));
                            }

                            // At this point we filter combined unicode letters out of the unicode charactesr
                            bool reachedFirstSeperator = false;
                            var unicodeCombination = new List<ushort>();
                            int index = 0;
                            while (index < mapping.UnicodeCharacters.Count)
                            {
                                if (mapping.UnicodeCharacters[index] == sequenceStart)
                                {
                                    mapping.UnicodeCharacters.RemoveAt(index);
                                    if (!reachedFirstSeperator) {
                                        reachedFirstSeperator = true;
                                    }
                                    else {
                                        mapping.UnicodeCharactersWithModifiers.Add(unicodeCombination.ToArray());
                                    }
                                }
                                else
                                {
                                    if (reachedFirstSeperator) {
                                        unicodeCombination.Add(mapping.UnicodeCharacters[index]);
                                        mapping.UnicodeCharacters.RemoveAt(index);
                                    }
                                    else {
                                        index++;
                                    }
                                }
                            }

                            // Now convert all the unicode characters we can to ASCII
                            foreach (var uc in mapping.UnicodeCharacters)
                            {
                                byte ac = Encoding.ASCII.GetBytes(Encoding.Unicode.GetString(BitConverter.GetBytes(uc)))[0];
                                if (!(ac == 63 && uc != 0x003F))
                                {
                                    if (!mapping.ASCIICharacters.Contains(ac)) {
                                        mapping.ASCIICharacters.Add(ac);
                                    }
                                }
                            }

                            mappings.Add(mapping);
                            currentEntry.Clear();
                            position++; // Skip the second seperator character as well
                        }
                        else
                        {
                            currentEntry.Add(fontData[position]);
                        }

                        position++;
                    }
                }

                return new PCScreenFont(charWidth, charHeight, parsedFontData, mappings);
            }
            if (version2)
            {
                uint length = BitConverter.ToUInt32(fontData, 16);
                uint height = BitConverter.ToUInt32(fontData, 24);
                uint width = BitConverter.ToUInt32(fontData, 28);
                charHeight = (byte)height;
                charWidth = (byte)width;

                uint bytesPerRow = (width + 7) / 8;
                uint charDataSize = charHeight * bytesPerRow;
                parsedFontData = new byte[length * charDataSize];

                for (int i = 0; i < length; i++)
                {
                    for (int k = 0; k < charHeight; k++)
                    {
                        int dataIndex = 32 + (i * (byte)charDataSize) + (k * (byte)bytesPerRow);
                        for (int j = 0; j < bytesPerRow; j++)
                        {
                            parsedFontData[(i * charDataSize) + (k * bytesPerRow) + j] = fontData[dataIndex + j];
                        }
                    }
                }


                return new PCScreenFont(charWidth, charHeight, parsedFontData, mappings);
            }

            throw new ArgumentException("The font data is incorrect.", nameof(fontData));
        }

        /// <summary>
        /// Converts the PC screen font to a VGA font.
        /// </summary>
        public byte[] CreateVGAFont()
        {
            byte[] font = new byte[256 * Height * Width / 8];

            // Find ' ' and ? char to use index if nothing is found
            int emptyOffset = FindASCIIOffset((byte)' ');
            int questionMarkOffset = FindASCIIOffset((byte)'?');

            for (int i = 0; i < 256; i++)
            {
                // Find font offset
                int offset = FindASCIIOffset((byte)i);
                if (offset >= 256) // If nothing was found
                {
                    if (i < 32) {
                        offset = emptyOffset;
                    }
                    else {
                        offset = questionMarkOffset;
                    }
                }

                for (int k = 0; k < Height; k++)
                {
                    font[(i * Height) + k] = Data[(offset * Height) + k];
                }
            }

            return font;
        }

        private int FindASCIIOffset(byte i)
        {
            int offset;
            for (offset = 0; offset < unicodeMappings.Count; offset++)
            {
                UnicodeMapping mapping = unicodeMappings[offset];
                if (mapping.ASCIICharacters.Contains(i))
                {
                    break;
                }
            }

            return offset;
        }
    }

    /// <summary>
    /// Represents a Unicode to font position mapping.
    /// </summary>
    public struct UnicodeMapping
    {
        public int FontPosition;
        public List<ushort> UnicodeCharacters;
        public List<ushort[]> UnicodeCharactersWithModifiers;
        public List<byte> ASCIICharacters;
    }
}
