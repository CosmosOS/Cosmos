using IL2CPU.API.Attribs;
using System;
using System.Drawing;

namespace Cosmos.System_Plugs.Interop
{
    [Plug("Interop+User32, System.Drawing.Primitives", IsOptional = true)]
    class User32Impl
    {
        static uint[] colorTable;
        // https://github.com/dotnet/corefx/blob/release/2.1/src/Common/src/System/Drawing/KnownColorTable.cs#L488
        static void InitColors()
        {
            colorTable = new uint[27];
            // Hard-coded constants, based on default Windows settings.
            colorTable[(uint)KnownColor.ActiveBorder] = unchecked(0xFFD4D0C8);
            colorTable[(uint)KnownColor.ActiveCaption] = unchecked(0xFF0054E3);
            colorTable[(uint)KnownColor.ActiveCaptionText] = unchecked(0xFFFFFFFF);
            colorTable[(uint)KnownColor.AppWorkspace] = unchecked(0xFF808080);
            colorTable[(uint)KnownColor.ButtonFace] = unchecked(0xFFF0F0F0);
            colorTable[(uint)KnownColor.ButtonHighlight] = unchecked(0xFFFFFFFF);
            colorTable[(uint)KnownColor.ButtonShadow] = unchecked(0xFFA0A0A0);
            colorTable[(uint)KnownColor.Control] = unchecked(0xFFECE9D8);
            colorTable[(uint)KnownColor.ControlDark] = unchecked(0xFFACA899);
            colorTable[(uint)KnownColor.ControlDarkDark] = unchecked(0xFF716F64);
            colorTable[(uint)KnownColor.ControlLight] = unchecked(0xFFF1EFE2);
            colorTable[(uint)KnownColor.ControlLightLight] = unchecked(0xFFFFFFFF);
            colorTable[(uint)KnownColor.ControlText] = unchecked(0xFF000000);
            colorTable[(uint)KnownColor.Desktop] = unchecked(0xFF004E98);
            colorTable[(uint)KnownColor.GradientActiveCaption] = unchecked(0xFFB9D1EA);
            colorTable[(uint)KnownColor.GradientInactiveCaption] = unchecked(0xFFD7E4F2);
            colorTable[(uint)KnownColor.GrayText] = unchecked(0xFFACA899);
            colorTable[(uint)KnownColor.Highlight] = unchecked(0xFF316AC5);
            colorTable[(uint)KnownColor.HighlightText] = unchecked(0xFFFFFFFF);
            colorTable[(uint)KnownColor.HotTrack] = unchecked(0xFF000080);
            colorTable[(uint)KnownColor.InactiveBorder] = unchecked(0xFFD4D0C8);
            colorTable[(uint)KnownColor.InactiveCaption] = unchecked(0xFF7A96DF);
            colorTable[(uint)KnownColor.InactiveCaptionText] = unchecked(0xFFD8E4F8);
            colorTable[(uint)KnownColor.Info] = unchecked(0xFFFFFFE1);
            colorTable[(uint)KnownColor.InfoText] = unchecked(0xFF000000);
            colorTable[(uint)KnownColor.Menu] = unchecked(0xFFFFFFFF);
            colorTable[(uint)KnownColor.MenuBar] = unchecked(0xFFF0F0F0);
            colorTable[(uint)KnownColor.MenuHighlight] = unchecked(0xFF3399FF);
            colorTable[(uint)KnownColor.MenuText] = unchecked(0xFF000000);
            colorTable[(uint)KnownColor.ScrollBar] = unchecked(0xFFD4D0C8);
            colorTable[(uint)KnownColor.Window] = unchecked(0xFFFFFFFF);
            colorTable[(uint)KnownColor.WindowFrame] = unchecked(0xFF000000);
            colorTable[(uint)KnownColor.WindowText] = unchecked(0xFF000000);
        }

        [PlugMethod(Signature = "System_UInt32__Interop_User32_GetSysColor_System_Int32_")]
        public static uint GetSysColor(int aIndex)
        {
            if(colorTable is null)
            {
                InitColors();
            }
            return colorTable[aIndex];
        }
    }
}
