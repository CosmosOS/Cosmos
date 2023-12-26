using System;
using Sys = Cosmos.System;
using Cosmos.TestRunner;
using Cosmos.System.Graphics;
using System.Text;
using Cosmos.System.ExtendedASCII;
using Cosmos.System.ScanMaps;

/*
 * Please note this is an atypical TestRunner:
 * - no Assertion can be done
 * - it cannot be executed automatically
 *
 * it exists to make easier tests while changing low level stuff (it would be better and faster to use the Demo kernel but
 * sometimes it is a problem to make it see modifications done at low level)
 *
 * Remember to comment this test again on TestKernelSets.cs when you are ready to merge your modifications!
 */
namespace ConsoleTest
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Let's Test Console!");

            /* This is needed to enable the Codepage based Encodings */
            Encoding.RegisterProvider(CosmosEncodingProvider.Instance);
        }

        protected override void Run()
        {
            try
            {
                TestConsoleEncoding();

                TestVGAResolutions();

                TestController.Completed();
            }
            catch (Exception e)
            {
                mDebugger.Send("Exception occurred: " + e.Message);
                mDebugger.Send(e.Message);
                TestController.Failed();
            }            
        }

        public void TestConsoleEncoding()
        {
            Console.Clear();

            Console.WriteLine("Testing Console Enconding");

            Console.WriteLine($"ConsoleInputEncoding {Console.InputEncoding.BodyName}");

            Console.WriteLine($"ConsoleOutputEncoding {Console.OutputEncoding.BodyName}");

            /* Let's change it in the legacy IBM437 encoding */
            Console.InputEncoding = Cosmos.System.ExtendedASCII.CosmosEncodingProvider.Instance.GetEncoding(437);
            Console.OutputEncoding = Cosmos.System.ExtendedASCII.CosmosEncodingProvider.Instance.GetEncoding(437);

            Console.WriteLine($"ConsoleInputEncoding in now {Console.InputEncoding.BodyName}");
            Console.WriteLine($"ConsoleOutputEncoding in now {Console.OutputEncoding.BodyName}");

            Console.WriteLine("Let's write some accented characters: èòàùì");
            Console.WriteLine("Let's print all the CP437 codepage");

            Sys.Global.Debugger.SendInternal("");

            Console.Write("Ç ü é â ä à å ç ê ë è ï î ì Ä Å\n" +
                          "É æ Æ ô ö ò û ù ÿ Ö Ü ¢ £ ¥ ₧ ƒ\n" +
                          "á í ó ú ñ Ñ ª º ¿ ⌐ ¬ ½ ¼ ¡ « »\n" +
                          "░ ▒ ▓ │ ┤ ╡ ╢ ╖ ╕ ╣ ║ ╗ ╝ ╜ ╛ ┐\n" +
                          "└ ┴ ┬ ├ ─ ┼ ╞ ╟ ╚ ╔ ╩ ╦ ╠ ═ ╬ ╧\n" +
                          "╨ ╤ ╥ ╙ ╘ ╒ ╓ ╫ ╪ ┘ ┌ █ ▄ ▌ ▐ ▀\n" +
                          "α ß Γ π Σ σ µ τ Φ Θ Ω δ ∞ φ ε ∩\n" +
                          "≡ ± ≥ ≤ ⌠ ⌡ ÷ ≈ ° ∙ · √ ⁿ ² ■ \u00A0\n");
            //Console.WriteLine();

            Console.WriteLine("The following line should appear as a continuos line of '─'");
            Console.WriteLine("──────────────────────────────────────────────────────────");

            Console.ReadKey();
            Console.WriteLine("The next line should be empty");
            Console.WriteLine();
            Console.WriteLine("True follows...");
            Console.WriteLine(true);
            Console.WriteLine("The letter 'A'");
            Console.WriteLine('A');
            char[] charBuffer = new char[] { 'A', 'B', 'C' };
            Console.WriteLine("Then ABC");
            Console.WriteLine(charBuffer);
            Console.WriteLine("...42.42");
            Console.WriteLine(42.42);
            Console.WriteLine("...42.42 (float)");
            Console.WriteLine(42.42f);
            Console.WriteLine("...42");
            Console.WriteLine(42);
            Console.WriteLine("...42 (long)");
            Console.WriteLine(42L);
            Console.ReadKey();
            object test = "Test";
            Console.WriteLine("...Test (as object)");
            Console.WriteLine(test);
            Console.WriteLine("The next line should be empty (null object)");
            object s = null;
            Console.WriteLine(s);
            Console.WriteLine("...42 (uint)");
            Console.WriteLine(42U);
            Console.WriteLine("...42 (ulong)");
            Console.WriteLine(42UL);
            Console.WriteLine("...BC");
            Console.WriteLine(charBuffer, 1, 2);

            Console.WriteLine("Test Format arg0 {0}", "test");
            Console.WriteLine("Test Format arg0 {0} arg1 {1}", "test", 42);
            Console.WriteLine("Test Format arg0 {0} arg1 {1} arg2 {2}", "test", 42, 69.69);
            Console.WriteLine("Test Format arg0 {0} arg1 {1} arg2 {2} arg3 {3}", "test", 42, 69.69, 25000L);
            /* String.Format does not support x or X and probably neither the rest of "special" formatting */
            //Console.WriteLine("Test Format (hex) {0:x}", 42);

            Console.WriteLine("Layout switched to DE...");
            Sys.KeyboardManager.SetKeyLayout(new DEStandardLayout());
            Console.WriteLine("Write in german now I'll read it with Console.ReadLine()...");

            var str = Console.ReadLine();
            Console.WriteLine($"You have written: {str}");

            Console.WriteLine("Write in german now I'll read it with Console.ReadKey()...");
            var character = Console.ReadKey();
            Console.WriteLine($"You have written: {character.KeyChar}");

            Console.WriteLine("Press any key to terminate this test...");

            Console.ReadKey();
        }

        public void TestVGAResolutions()
        {
            Console.SetWindowSize(40, 25);
            TestVariousConsoleFunctions();
            Console.SetWindowSize(40, 50);
            TestVariousConsoleFunctions();
            Console.SetWindowSize(80, 25);
            TestVariousConsoleFunctions();
            Console.SetWindowSize(80, 50);
            TestVariousConsoleFunctions();
            Console.SetWindowSize(90, 30);
            TestVariousConsoleFunctions();
            Console.SetWindowSize(90, 60);
            TestVariousConsoleFunctions();
        }

        public void TestVariousConsoleFunctions()
        {
            Console.Clear();
            Console.WriteLine("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");
            Console.Write("Reading input: ");
            Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Green; Console.BackgroundColor = ConsoleColor.Blue;
            Console.WriteLine("Colored text :)");
            Console.ResetColor();
            Console.Write("Press enter to continue to the next step: "); Console.ReadLine();
            Console.WriteLine("Press a key to move the cursor up");
            Console.ReadKey();
            var cursor = Console.GetCursorPosition();
            Console.SetCursorPosition(cursor.Left, cursor.Top - 1);
        }
    }
}