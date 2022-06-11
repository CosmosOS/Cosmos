using System.Drawing;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.Interop;

[Plug("Interop+User32, System.Drawing.Primitives")]
internal class User32Impl
{
    private static uint[] colorTable;

    // https://github.com/dotnet/corefx/blob/release/2.1/src/Common/src/System/Drawing/KnownColorTable.cs#L488
    private static void InitColors()
    {
        colorTable = new uint[27];
        // Hard-coded constants, based on default Windows settings.
        colorTable[(uint)KnownColor.ActiveBorder] = 0xFFD4D0C8;
        colorTable[(uint)KnownColor.ActiveCaption] = 0xFF0054E3;
        colorTable[(uint)KnownColor.ActiveCaptionText] = 0xFFFFFFFF;
        colorTable[(uint)KnownColor.AppWorkspace] = 0xFF808080;
        colorTable[(uint)KnownColor.ButtonFace] = 0xFFF0F0F0;
        colorTable[(uint)KnownColor.ButtonHighlight] = 0xFFFFFFFF;
        colorTable[(uint)KnownColor.ButtonShadow] = 0xFFA0A0A0;
        colorTable[(uint)KnownColor.Control] = 0xFFECE9D8;
        colorTable[(uint)KnownColor.ControlDark] = 0xFFACA899;
        colorTable[(uint)KnownColor.ControlDarkDark] = 0xFF716F64;
        colorTable[(uint)KnownColor.ControlLight] = 0xFFF1EFE2;
        colorTable[(uint)KnownColor.ControlLightLight] = 0xFFFFFFFF;
        colorTable[(uint)KnownColor.ControlText] = 0xFF000000;
        colorTable[(uint)KnownColor.Desktop] = 0xFF004E98;
        colorTable[(uint)KnownColor.GradientActiveCaption] = 0xFFB9D1EA;
        colorTable[(uint)KnownColor.GradientInactiveCaption] = 0xFFD7E4F2;
        colorTable[(uint)KnownColor.GrayText] = 0xFFACA899;
        colorTable[(uint)KnownColor.Highlight] = 0xFF316AC5;
        colorTable[(uint)KnownColor.HighlightText] = 0xFFFFFFFF;
        colorTable[(uint)KnownColor.HotTrack] = 0xFF000080;
        colorTable[(uint)KnownColor.InactiveBorder] = 0xFFD4D0C8;
        colorTable[(uint)KnownColor.InactiveCaption] = 0xFF7A96DF;
        colorTable[(uint)KnownColor.InactiveCaptionText] = 0xFFD8E4F8;
        colorTable[(uint)KnownColor.Info] = 0xFFFFFFE1;
        colorTable[(uint)KnownColor.InfoText] = 0xFF000000;
        colorTable[(uint)KnownColor.Menu] = 0xFFFFFFFF;
        colorTable[(uint)KnownColor.MenuBar] = 0xFFF0F0F0;
        colorTable[(uint)KnownColor.MenuHighlight] = 0xFF3399FF;
        colorTable[(uint)KnownColor.MenuText] = 0xFF000000;
        colorTable[(uint)KnownColor.ScrollBar] = 0xFFD4D0C8;
        colorTable[(uint)KnownColor.Window] = 0xFFFFFFFF;
        colorTable[(uint)KnownColor.WindowFrame] = 0xFF000000;
        colorTable[(uint)KnownColor.WindowText] = 0xFF000000;
    }

    [PlugMethod(Signature = "System_UInt32__Interop_User32_GetSysColor_System_Int32_")]
    public static uint GetSysColor(int aIndex)
    {
        if (colorTable is null)
        {
            InitColors();
        }

        return colorTable[aIndex];
    }
}
