using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKYPE4COMLib;

namespace Cosmos.Tools.SkypeBot
{
    /// <summary>
    /// Represents the skype bot.
    /// </summary>
    public class SkypeBot
    {
        public const string Cosmos_Dev = "VXO9PL3t2HgKSEPgytWb7EM0OaRRmfhWLKd8v-2SV_6m43_Bli5m6mFNNQxjv8TyTb1GpaXZfz_5VFYS17NvKSLLICj4MWZslQ";

        /// <summary>
        /// Creates a new skype bot using the specified blob.
        /// </summary>
        /// <param name="blob"></param>
        public SkypeBot(string blob)
        {
            Skype s = new SKYPE4COMLib.SkypeClass();
            s.Attach(5, true);
            Chat c = s.CreateChatUsingBlob(blob);
        }
    }
}
