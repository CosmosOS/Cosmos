// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.System.Keyboard;
using Cosmos.Kernel.System.Keyboard.ScanMaps;
using NUnit.Framework;

namespace Cosmos.Kernel.Tests.System.Keyboard.ScanMaps;

public class TRStandardLayoutTest
{
    // Set 1 make codes of the keys the cases below press.
    private const byte Digit2 = 0x03;
    private const byte Digit7 = 0x08;
    private const byte Q = 0x10;
    private const byte DotlessI = 0x17;
    private const byte A = 0x1E;
    private const byte W = 0x11;

    private TRStandardLayout _target = null!;

    [SetUp]
    public void Setup()
    {
        _target = new TRStandardLayout();
    }

    // AltGr reaches a layout as Control and Alt held together.
    private KeyEvent Convert(byte scanCode, bool ctrl = false, bool shift = false, bool alt = false)
    {
        KeyEvent? keyEvent = _target.ConvertScanCode(scanCode, ctrl, shift, alt, numLock: false, capsLock: false, scrollLock: false);
        Assert.That(keyEvent, Is.Not.Null);
        return keyEvent;
    }

    [TestFixture]
    public class ConvertScanCode : TRStandardLayoutTest
    {
        [TestCase(Q, ExpectedResult = '@')]
        [TestCase(Digit7, ExpectedResult = '{')]
        [TestCase(A, ExpectedResult = 'æ')]
        [TestCase(DotlessI, ExpectedResult = 'i')]
        public char WhenAltGr_AndKeyHasAThirdLevel_ResultIsTheThirdLevel(byte scanCode)
        {
            return Convert(scanCode, ctrl: true, alt: true).KeyChar;
        }

        [TestCase(A, ExpectedResult = 'Æ')]
        [TestCase(DotlessI, ExpectedResult = 'İ')]
        public char WhenAltGr_AndShift_ResultIsTheFourthLevel(byte scanCode)
        {
            return Convert(scanCode, ctrl: true, shift: true, alt: true).KeyChar;
        }

        [Test]
        public void WhenAltGr_AndKeyHasNoThirdLevel_ResultIsNoCharacter()
        {
            Assert.That(Convert(W, ctrl: true, alt: true).KeyChar, Is.EqualTo('\0'));
        }

        [Test]
        public void WhenAltGr_ModifiersReportControlAndAlt()
        {
            KeyEvent keyEvent = Convert(Q, ctrl: true, alt: true);

            Assert.That(keyEvent.Modifiers, Is.EqualTo(ConsoleModifiers.Control | ConsoleModifiers.Alt));
            Assert.That(keyEvent.Key, Is.EqualTo(ConsoleKeyEx.Q));
        }

        [TestCase(Q, false, ExpectedResult = 'q')]
        [TestCase(Q, true, ExpectedResult = 'Q')]
        [TestCase(Digit2, true, ExpectedResult = '\'')]
        [TestCase(DotlessI, false, ExpectedResult = 'ı')]
        public char WhenNoAltGr_ResultIsTheBaseOrShiftedLevel(byte scanCode, bool shift)
        {
            return Convert(scanCode, shift: shift).KeyChar;
        }
    }
}
