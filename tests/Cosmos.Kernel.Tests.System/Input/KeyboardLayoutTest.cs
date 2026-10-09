// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.System.Input;
using Cosmos.Kernel.System.Input.Layouts;
using NUnit.Framework;

namespace Cosmos.Kernel.Tests.System.Input;

public class KeyboardLayoutTest
{
    // Every shipped layout, so a new one cannot ship without saying what its right Alt is.
    private static readonly KeyboardLayout[] s_shippedLayouts =
    [
        new USStandardLayout(),
        new FRStandardLayout(),
        new DEStandardLayout(),
        new ESStandardLayout(),
        new GBStandardLayout(),
        new TRStandardLayout(),
        new USDvorakLayout(),
    ];

    [TestFixture]
    public class ScanCodeMatchesKey : KeyboardLayoutTest
    {
        [TestCaseSource(nameof(s_shippedLayouts))]
        public void WhenRightAlt_EveryShippedLayoutMapsItToAltGrOrRAlt(KeyboardLayout layout)
        {
            bool altGr = layout.ScanCodeMatchesKey(KeyboardLayout.RightAltScanCode, Key.AltGr);
            bool rAlt = layout.ScanCodeMatchesKey(KeyboardLayout.RightAltScanCode, Key.RAlt);

            Assert.That(altGr ^ rAlt, Is.True, $"{layout.GetType().Name} maps the right Alt to {(altGr ? "AltGr" : "RAlt")} {(altGr == rAlt ? "and the other" : "")}");
        }

        [TestCase(typeof(DEStandardLayout), ExpectedResult = true)]
        [TestCase(typeof(ESStandardLayout), ExpectedResult = true)]
        [TestCase(typeof(TRStandardLayout), ExpectedResult = true)]
        [TestCase(typeof(USStandardLayout), ExpectedResult = false)]
        [TestCase(typeof(USDvorakLayout), ExpectedResult = false)]
        public bool WhenRightAlt_LayoutsWithAThirdLevelMapItToAltGr(Type layoutType)
        {
            KeyboardLayout layout = (KeyboardLayout)Activator.CreateInstance(layoutType)!;

            return layout.ScanCodeMatchesKey(KeyboardLayout.RightAltScanCode, Key.AltGr);
        }
    }
}
