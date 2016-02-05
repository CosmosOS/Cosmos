﻿using System.Text;

using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.System.Text
{
    [Plug(Target = typeof(EncoderFallback))]
    public static class EncoderFallbackImpl
    {
        // Encoders use this, but we plug their methods anwyays so we just fill empty for now.
        public static object get_InternalSyncObject()
        {
            return new object();
        }
    }
}
