// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Text;
using Cosmos.Kernel.System.Keyboard;
using Cosmos.Kernel.System.Network.Telnet;
using NUnit.Framework;

namespace Cosmos.Kernel.Tests.System.Network.Telnet;

public class TelnetDecoderTest
{
    private const byte Iac = 255;
    private const byte Sb = 250;
    private const byte Se = 240;
    private const byte Will = 251;
    private const byte Wont = 252;
    private const byte Do = 253;
    private const byte Dont = 254;
    private const byte InterruptProcess = 244;
    private const byte Echo = 1;
    private const byte SuppressGoAhead = 3;
    private const byte TerminalType = 24;
    private const byte WindowSize = 31;

    private TelnetDecoder _target = null!;
    private List<KeyEvent> _keys = null!;
    private List<byte> _replies = null!;

    [SetUp]
    public void Setup()
    {
        _target = new TelnetDecoder();
        _keys = [];
        _replies = [];
    }

    private void Feed(params byte[] data) => _target.Decode(data, _keys, _replies);

    private void Feed(string text) => Feed(Encoding.UTF8.GetBytes(text));

    [TestFixture]
    public class Negotiate : TelnetDecoderTest
    {
        [Test]
        public void WhenOpened_EchoAndSuppressGoAheadAreOffered_AndTheWindowSizeIsAskedFor()
        {
            _target.Negotiate(_replies);

            Assert.That(_replies, Is.EqualTo(new byte[]
            {
                Iac, Will, Echo,
                Iac, Will, SuppressGoAhead,
                Iac, Do, SuppressGoAhead,
                Iac, Do, WindowSize,
            }));
        }

        [Test]
        public void WhenClientAcceptsEveryOffer_NothingIsAnswered()
        {
            _target.Negotiate(_replies);
            _replies.Clear();

            Feed(Iac, Do, Echo, Iac, Do, SuppressGoAhead, Iac, Will, SuppressGoAhead, Iac, Will, WindowSize);

            Assert.That(_replies, Is.Empty);
        }

        [Test]
        public void WhenClientAsksForAnOptionTheServerLacks_ItIsRefused()
        {
            Feed(Iac, Do, TerminalType);

            Assert.That(_replies, Is.EqualTo(new byte[] { Iac, Wont, TerminalType }));
        }

        [Test]
        public void WhenClientOffersAnOptionTheServerDoesNotWant_ItIsDeclined()
        {
            Feed(Iac, Will, TerminalType);

            Assert.That(_replies, Is.EqualTo(new byte[] { Iac, Dont, TerminalType }));
        }

        [Test]
        public void WhenClientTurnsOffEcho_TheServerConfirmsOnce()
        {
            _target.Negotiate(_replies);
            _replies.Clear();

            Feed(Iac, Dont, Echo, Iac, Dont, Echo);

            Assert.That(_replies, Is.EqualTo(new byte[] { Iac, Wont, Echo }));
        }
    }

    [TestFixture]
    public class WindowColumns : TelnetDecoderTest
    {
        [Test]
        public void WhenNoWindowSizeWasReported_ItIsZero()
        {
            Feed("abc");

            Assert.That(_target.WindowColumns, Is.Zero);
        }

        [Test]
        public void WhenWindowSizeIsReported_ItIsRead()
        {
            Feed(Iac, Sb, WindowSize, 0, 100, 0, 30, Iac, Se);

            Assert.That((_target.WindowColumns, _target.WindowRows), Is.EqualTo((100, 30)));
        }

        [Test]
        public void WhenWindowSizeHoldsAnEscapedIac_ItIsUnescaped()
        {
            Feed(Iac, Sb, WindowSize, 0, Iac, Iac, 0, 40, Iac, Se);

            Assert.That((_target.WindowColumns, _target.WindowRows), Is.EqualTo((255, 40)));
        }

        [Test]
        public void WhenWindowSizeIsSplitBetweenSegments_ItIsReadWhole()
        {
            Feed(Iac);
            Feed(Sb, WindowSize, 0);
            Feed(80, 0, 24, Iac);
            Feed(Se);

            Assert.That((_target.WindowColumns, _target.WindowRows), Is.EqualTo((80, 24)));
        }

        [Test]
        public void WhenAWindowDimensionIsZero_TheReportIsIgnored()
        {
            Feed(Iac, Sb, WindowSize, 0, 0, 0, 24, Iac, Se);

            Assert.That(_target.WindowColumns, Is.Zero);
        }

        [Test]
        public void WhenWindowIsHuge_ItIsCapped()
        {
            Feed(Iac, Sb, WindowSize, 0x27, 0x10, 0x27, 0x10, Iac, Se);

            Assert.That((_target.WindowColumns, _target.WindowRows), Is.EqualTo((512, 256)));
        }
    }

    [TestFixture]
    public class Decode : TelnetDecoderTest
    {
        [Test]
        public void WhenTextArrives_EachCharacterIsAKey()
        {
            Feed("aZ 7");

            Assert.That(_keys.Select(k => k.KeyChar), Is.EqualTo("aZ 7"));
            Assert.That(_keys.Select(k => k.Key), Is.EqualTo(new[] { ConsoleKeyEx.A, ConsoleKeyEx.Z, ConsoleKeyEx.Spacebar, ConsoleKeyEx.D7 }));
        }

        [TestCase("\r\n")]
        [TestCase("\r\0")]
        [TestCase("\r")]
        [TestCase("\n")]
        public void WhenALineEnds_OneEnterIsReported(string ending)
        {
            Feed(ending);

            Assert.That(_keys.Select(k => k.Key), Is.EqualTo(new[] { ConsoleKeyEx.Enter }));
        }

        [TestCase((byte)0x7F)]
        [TestCase((byte)0x08)]
        public void WhenTheEraseKeyIsSent_BackspaceIsReported(byte erase)
        {
            Feed(erase);

            Assert.That(_keys.Single().Key, Is.EqualTo(ConsoleKeyEx.Backspace));
        }

        [TestCase("\x1b[A", ConsoleKeyEx.UpArrow)]
        [TestCase("\x1b[B", ConsoleKeyEx.DownArrow)]
        [TestCase("\x1b[C", ConsoleKeyEx.RightArrow)]
        [TestCase("\x1b[D", ConsoleKeyEx.LeftArrow)]
        [TestCase("\x1bOA", ConsoleKeyEx.UpArrow)]
        [TestCase("\x1b[H", ConsoleKeyEx.Home)]
        [TestCase("\x1b[F", ConsoleKeyEx.End)]
        [TestCase("\x1b[1~", ConsoleKeyEx.Home)]
        [TestCase("\x1b[2~", ConsoleKeyEx.Insert)]
        [TestCase("\x1b[3~", ConsoleKeyEx.Delete)]
        [TestCase("\x1b[4~", ConsoleKeyEx.End)]
        [TestCase("\x1b[5~", ConsoleKeyEx.PageUp)]
        [TestCase("\x1b[6~", ConsoleKeyEx.PageDown)]
        [TestCase("\x1bOP", ConsoleKeyEx.F1)]
        [TestCase("\x1b[15~", ConsoleKeyEx.F5)]
        [TestCase("\x1b[24~", ConsoleKeyEx.F12)]
        public void WhenAKeySequenceArrives_ItsKeyIsReported(string sequence, ConsoleKeyEx expected)
        {
            Feed(sequence);

            Assert.That(_keys.Select(k => k.Key), Is.EqualTo(new[] { expected }));
        }

        [Test]
        public void WhenASequenceCarriesModifiers_TheyAreReported()
        {
            Feed("\x1b[1;5C");

            KeyEvent key = _keys.Single();
            Assert.That(key.Key, Is.EqualTo(ConsoleKeyEx.RightArrow));
            Assert.That(key.Modifiers, Is.EqualTo(ConsoleModifiers.Control));
        }

        [Test]
        public void WhenASequenceIsSplitBetweenSegments_ItDecodesWhole()
        {
            Feed("\x1b[");
            Feed("3~");

            Assert.That(_keys.Select(k => k.Key), Is.EqualTo(new[] { ConsoleKeyEx.Delete }));
        }

        [Test]
        public void WhenEscapeEndsASegment_EscapeIsReported()
        {
            Feed("\x1b");

            Assert.That(_keys.Single().Key, Is.EqualTo(ConsoleKeyEx.Escape));
        }

        [Test]
        public void WhenEscapePrecedesACharacter_AltAndTheCharacterAreReported()
        {
            Feed("\x1bx");

            KeyEvent key = _keys.Single();
            Assert.That(key.KeyChar, Is.EqualTo('x'));
            Assert.That(key.Modifiers, Is.EqualTo(ConsoleModifiers.Alt));
        }

        [Test]
        public void WhenAControlCharacterArrives_ControlAndItsLetterAreReported()
        {
            Feed(0x03);

            KeyEvent key = _keys.Single();
            Assert.That(key.Key, Is.EqualTo(ConsoleKeyEx.C));
            Assert.That(key.Modifiers, Is.EqualTo(ConsoleModifiers.Control));
        }

        [Test]
        public void WhenAMultiByteCharacterIsSplitBetweenSegments_ItDecodesWhole()
        {
            byte[] encoded = Encoding.UTF8.GetBytes("é");
            Feed(encoded[0]);
            Feed(encoded[1]);

            Assert.That(_keys.Single().KeyChar, Is.EqualTo('é'));
        }

        [Test]
        public void WhenCommandsAreInterleavedWithText_OnlyTheTextBecomesKeys()
        {
            Feed((byte)'a', Iac, Do, Echo, (byte)'b', Iac, Sb, WindowSize, 0, 80, 0, 24, Iac, Se, (byte)'c');

            Assert.That(_keys.Select(k => k.KeyChar), Is.EqualTo("abc"));
        }

        [Test]
        public void WhenTheClientInterrupts_ControlCIsReported()
        {
            Feed(Iac, InterruptProcess);

            KeyEvent key = _keys.Single();
            Assert.That(key.Key, Is.EqualTo(ConsoleKeyEx.C));
            Assert.That(key.Modifiers, Is.EqualTo(ConsoleModifiers.Control));
        }
    }
}
