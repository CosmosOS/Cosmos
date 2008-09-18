
namespace Cosmos.Debug {
	public static class Debugger {
        public static void Break() { } // Plugged
        public static unsafe void Send(int aLength, char* aText) { } // Plugged
        public static void TraceOff() { } // Plugged
        public static void TraceOn() { } // Plugged
        
        public static unsafe void Send(string aText) {
            var xChars = aText.ToCharArray();
            fixed(char* xPtr = &xChars[0]) {
                Send(aText.Length, xPtr);
            }
        }
        
	}
}