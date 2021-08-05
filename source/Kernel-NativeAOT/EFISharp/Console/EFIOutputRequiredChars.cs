namespace EfiSharp
{
    /// <summary>
    /// All output devices must support the Unicode drawing character codes defined here.
    /// From Related Definitions section from https://uefi.org/sites/default/files/resources/UEFI%20Spec%202.8B%20May%202020.pdf#G16.1016966
    /// </summary>
    //Not exactly required for this to be included here but it would be a pain to figure
    //out which box drawing chars are supported otherwise.
    //TODO Decide if these should be here, in EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL or removed.
    public static class EFIOutputRequiredChars
    {
        //*******************************************************
        // UNICODE DRAWING CHARACTERS
        //*******************************************************

        //Light and heavy solid lines
        public const char BOXDRAW_HORIZONTAL = (char)0x2500;
        public const char BOXDRAW_VERTICAL = (char)0x2502;
        //Light and heavy line box components
        public const char BOXDRAW_DOWN_RIGHT = (char)0x250c;
        public const char BOXDRAW_DOWN_LEFT = (char)0x2510;
        public const char BOXDRAW_UP_RIGHT = (char)0x2514;
        public const char BOXDRAW_UP_LEFT = (char)0x2518;
        public const char BOXDRAW_VERTICAL_RIGHT = (char)0x251c;
        public const char BOXDRAW_VERTICAL_LEFT = (char)0x2524;
        public const char BOXDRAW_DOWN_HORIZONTAL = (char)0x252c;
        public const char BOXDRAW_UP_HORIZONTAL = (char)0x2534;
        public const char BOXDRAW_VERTICAL_HORIZONTAL = (char)0x253c;

        //Double lines
        public const char BOXDRAW_DOUBLE_HORIZONTAL = (char)0x2550;
        public const char BOXDRAW_DOUBLE_VERTICAL = (char)0x2551;
        //Light and double line box components
        public const char BOXDRAW_DOWN_RIGHT_DOUBLE = (char)0x2552;
        public const char BOXDRAW_DOWN_DOUBLE_RIGHT = (char)0x2553;
        public const char BOXDRAW_DOUBLE_DOWN_RIGHT = (char)0x2554;
        public const char BOXDRAW_DOWN_LEFT_DOUBLE = (char)0x2555;
        public const char BOXDRAW_DOWN_DOUBLE_LEFT = (char)0x2556;
        public const char BOXDRAW_DOUBLE_DOWN_LEFT = (char)0x2557;

        public const char BOXDRAW_UP_RIGHT_DOUBLE = (char)0x2558;
        public const char BOXDRAW_UP_DOUBLE_RIGHT = (char)0x2559;
        public const char BOXDRAW_DOUBLE_UP_RIGHT = (char)0x255a;
        public const char BOXDRAW_UP_LEFT_DOUBLE = (char)0x255b;
        public const char BOXDRAW_UP_DOUBLE_LEFT = (char)0x255c;
        public const char BOXDRAW_DOUBLE_UP_LEFT = (char)0x255d;

        public const char BOXDRAW_VERTICAL_RIGHT_DOUBLE = (char)0x255e;
        public const char BOXDRAW_VERTICAL_DOUBLE_RIGHT = (char)0x255f;
        public const char BOXDRAW_DOUBLE_VERTICAL_RIGHT = (char)0x2560;

        public const char BOXDRAW_VERTICAL_LEFT_DOUBLE = (char)0x2561;
        public const char BOXDRAW_VERTICAL_DOUBLE_LEFT = (char)0x2562;
        public const char BOXDRAW_DOUBLE_VERTICAL_LEFT = (char)0x2563;

        public const char BOXDRAW_DOWN_HORIZONTAL_DOUBLE = (char)0x2564;
        public const char BOXDRAW_DOWN_DOUBLE_HORIZONTAL = (char)0x2565;
        public const char BOXDRAW_DOUBLE_DOWN_HORIZONTAL = (char)0x2566;

        public const char BOXDRAW_UP_HORIZONTAL_DOUBLE = (char)0x2567;
        public const char BOXDRAW_UP_DOUBLE_HORIZONTAL = (char)0x2568;
        public const char BOXDRAW_DOUBLE_UP_HORIZONTAL = (char)0x2569;

        public const char BOXDRAW_VERTICAL_HORIZONTAL_DOUBLE = (char)0x256a;
        public const char BOXDRAW_VERTICAL_DOUBLE_HORIZONTAL = (char)0x256b;
        public const char BOXDRAW_DOUBLE_VERTICAL_HORIZONTAL = (char)0x256c;

        //*******************************************************
        // EFI Required Block Elements Code Chart
        //*******************************************************

        //Block elements
        public const char BLOCKELEMENT_FULL_BLOCK = (char)0x2588;
        //Shade characters
        public const char BLOCKELEMENT_LIGHT_SHADE = (char)0x2591;

        //*******************************************************
        // EFI Required Geometric Shapes Code Chart
        //*******************************************************

        //Geometric shapes
        public const char GEOMETRICSHAPE_UP_TRIANGLE = (char)0x25b2;
        public const char GEOMETRICSHAPE_RIGHT_TRIANGLE = (char)0x25ba;
        public const char GEOMETRICSHAPE_DOWN_TRIANGLE = (char)0x25bc;
        public const char GEOMETRICSHAPE_LEFT_TRIANGLE = (char)0x25c4;

        //*******************************************************
        // EFI Required Arrow shapes
        //*******************************************************

        //Simple arrows
        public const char ARROW_UP = (char)0x2191;
        public const char ARROW_DOWN = (char)0x2193;
    }
}
