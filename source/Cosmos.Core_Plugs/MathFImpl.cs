using IL2CPU.API.Attribs;
using System;

namespace Cosmos.Core_Plugs
{
    // Temporary implementations, inneficent. Should use proper methods eventually.
    // See: https://linasm.sourceforge.net/docs/instructions/fpu.php
    [Plug(Target = typeof(MathF))]
    public static class MathFImpl
    {
    	public static float Sqrt(float d) => (float)Math.Sqrt(d);
        
        public static float Tan(float x) => (float)Math.Tan(x);
        
        public static float Sin(float a) => (float)Math.Sin(a);
        
		public static float Cos(float d) => (float)Math.Cos(d);
    }
}
