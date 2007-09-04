using System;

namespace HelloWorldMetal {
    // Restrictions for Metal compile is that static members only, and no heap usage
    // Basic string support is provided by using c strings in data sections
    class Program {
        // These would be output in the data section. Since no heap exists, 
        // all are treated as globals and space is fixed and preallocated
        static int Level = 0;
        static string StringValue = "Hello";

        static void Main() {
            // Local variables are ok too, since they are stack based
            int i = 0;
            // String literals translate to ldstr - these would automatically be pulled out
            // and put in the data section. String manipuation not permitted unless the actual
            // bytes are modified directly.
            //
            // Implement the P/Invoke interface. This will allow interfacing to Windows
            // and other systems.
            //
            // Certain statics can have points mapped and replaced with future metal code
            // or translations to P/Invokes.
            // Such as Console.Write. It can be determined by name and remapped to 
            // the Win32 calll.
            // Console.Write("Hello World");
            // For now - lets stick with P/Invoke only - I want to do some more investigation
            // around map replacement
            // So the current test would be - declare a P/Invoke for writing to console in Win32
            // then call it below with "HelloWorld"
        }
    }
}
