using System.Drawing;

using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Drawing
{
    [Plug("System.Drawing.KnownColorTable, System.Drawing.Primitives")]
    public static class KnownColorTableImpl
    {
        // https://github.com/dotnet/corefx/blob/release/2.1/src/Common/src/System/Drawing/KnownColorTable.cs#L488
        public static void UpdateSystemColors(int[] colorTable)
        {
            // Hard-coded constants, based on default Windows settings.
            colorTable[(int)KnownColor.ActiveBorder] = unchecked((int)0xFFD4D0C8);
            colorTable[(int)KnownColor.ActiveCaption] = unchecked((int)0xFF0054E3);
            colorTable[(int)KnownColor.ActiveCaptionText] = unchecked((int)0xFFFFFFFF);
            colorTable[(int)KnownColor.AppWorkspace] = unchecked((int)0xFF808080);
            colorTable[(int)KnownColor.ButtonFace] = unchecked((int)0xFFF0F0F0);
            colorTable[(int)KnownColor.ButtonHighlight] = unchecked((int)0xFFFFFFFF);
            colorTable[(int)KnownColor.ButtonShadow] = unchecked((int)0xFFA0A0A0);
            colorTable[(int)KnownColor.Control] = unchecked((int)0xFFECE9D8);
            colorTable[(int)KnownColor.ControlDark] = unchecked((int)0xFFACA899);
            colorTable[(int)KnownColor.ControlDarkDark] = unchecked((int)0xFF716F64);
            colorTable[(int)KnownColor.ControlLight] = unchecked((int)0xFFF1EFE2);
            colorTable[(int)KnownColor.ControlLightLight] = unchecked((int)0xFFFFFFFF);
            colorTable[(int)KnownColor.ControlText] = unchecked((int)0xFF000000);
            colorTable[(int)KnownColor.Desktop] = unchecked((int)0xFF004E98);
            colorTable[(int)KnownColor.GradientActiveCaption] = unchecked((int)0xFFB9D1EA);
            colorTable[(int)KnownColor.GradientInactiveCaption] = unchecked((int)0xFFD7E4F2);
            colorTable[(int)KnownColor.GrayText] = unchecked((int)0xFFACA899);
            colorTable[(int)KnownColor.Highlight] = unchecked((int)0xFF316AC5);
            colorTable[(int)KnownColor.HighlightText] = unchecked((int)0xFFFFFFFF);
            colorTable[(int)KnownColor.HotTrack] = unchecked((int)0xFF000080);
            colorTable[(int)KnownColor.InactiveBorder] = unchecked((int)0xFFD4D0C8);
            colorTable[(int)KnownColor.InactiveCaption] = unchecked((int)0xFF7A96DF);
            colorTable[(int)KnownColor.InactiveCaptionText] = unchecked((int)0xFFD8E4F8);
            colorTable[(int)KnownColor.Info] = unchecked((int)0xFFFFFFE1);
            colorTable[(int)KnownColor.InfoText] = unchecked((int)0xFF000000);
            colorTable[(int)KnownColor.Menu] = unchecked((int)0xFFFFFFFF);
            colorTable[(int)KnownColor.MenuBar] = unchecked((int)0xFFF0F0F0);
            colorTable[(int)KnownColor.MenuHighlight] = unchecked((int)0xFF3399FF);
            colorTable[(int)KnownColor.MenuText] = unchecked((int)0xFF000000);
            colorTable[(int)KnownColor.ScrollBar] = unchecked((int)0xFFD4D0C8);
            colorTable[(int)KnownColor.Window] = unchecked((int)0xFFFFFFFF);
            colorTable[(int)KnownColor.WindowFrame] = unchecked((int)0xFF000000);
            colorTable[(int)KnownColor.WindowText] = unchecked((int)0xFF000000);
        }
    }
}
