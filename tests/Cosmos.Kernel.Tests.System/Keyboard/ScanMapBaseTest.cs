// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.System.Keyboard;
using Cosmos.Kernel.System.Keyboard.ScanMaps;
using NUnit.Framework;

namespace Cosmos.Kernel.Tests.System.Keyboard;

public class ScanMapBaseTest
{
    // Every shipped layout, so a new one cannot ship without saying what its right Alt is.
    private static readonly ScanMapBase[] s_shippedLayouts =
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
    public class ScanCodeMatchesKey : ScanMapBaseTest
    {
        [TestCaseSource(nameof(s_shippedLayouts))]
        public void WhenRightAlt_EveryShippedLayoutMapsItToAltGrOrRAlt(ScanMapBase layout)
        {
            bool altGr = layout.ScanCodeMatchesKey(ScanMapBase.RightAltScanCode, ConsoleKeyEx.AltGr);
            bool rAlt = layout.ScanCodeMatchesKey(ScanMapBase.RightAltScanCode, ConsoleKeyEx.RAlt);

            Assert.That(altGr ^ rAlt, Is.True, $"{layout.GetType().Name} maps the right Alt to {(altGr ? "AltGr" : "RAlt")} {(altGr == rAlt ? "and the other" : "")}");
        }

        [TestCase(typeof(DEStandardLayout), ExpectedResult = true)]
        [TestCase(typeof(ESStandardLayout), ExpectedResult = true)]
        [TestCase(typeof(TRStandardLayout), ExpectedResult = true)]
        [TestCase(typeof(USStandardLayout), ExpectedResult = false)]
        [TestCase(typeof(USDvorakLayout), ExpectedResult = false)]
        public bool WhenRightAlt_LayoutsWithAThirdLevelMapItToAltGr(Type layoutType)
        {
            ScanMapBase layout = (ScanMapBase)Activator.CreateInstance(layoutType)!;

            return layout.ScanCodeMatchesKey(ScanMapBase.RightAltScanCode, ConsoleKeyEx.AltGr);
        }
    }
}
