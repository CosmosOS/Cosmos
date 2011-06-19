using System;
using System.Runtime.InteropServices;

namespace Cosmos.Cosmos_VS_Debug
{
    /// <summary>
    /// This class will contain all methods that we need to import.
    /// </summary>
    internal class NativeMethods 
    {
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONDBLCLK = 0x0203;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_MBUTTONDOWN = 0x0207;

        //Including a private constructor to prevent a compiler-generated default constructor
        private NativeMethods()
        {
        }

        // Import the SendMessage function from user32.dll
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hwnd,
                                                int Msg,
                                                IntPtr wParam,
                                                [MarshalAs(UnmanagedType.IUnknown)] out object lParam);
    }
}