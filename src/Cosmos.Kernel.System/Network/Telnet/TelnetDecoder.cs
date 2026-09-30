// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.System.Keyboard;

namespace Cosmos.Kernel.System.Network.Telnet;

/// <summary>
/// Turns what a Telnet client sends into keys. It takes the Telnet commands
/// out of the stream (RFC 854), answers the option negotiation, reads the
/// window size (RFC 1073), and decodes the rest as a terminal's keys: UTF-8
/// characters, control characters, and the escape sequences of the arrows,
/// the editing keys and the function keys.
/// </summary>
/// <remarks>
/// State carries over from one call to the next, so a command or a sequence
/// split between two segments decodes whole. The one exception is an escape
/// that ends a segment: it is the Escape key, since a terminal sends a
/// sequence in one write.
/// </remarks>
internal sealed class TelnetDecoder
{
    /// <summary>Interpret As Command: starts every Telnet command.</summary>
    internal const byte Iac = 255;

    /// <summary>The sender wants to enable an option on the receiver's side.</summary>
    internal const byte Do = 253;

    /// <summary>The sender wants the option disabled on the receiver's side.</summary>
    internal const byte Dont = 254;

    /// <summary>The sender enables, or offers to enable, an option on its own side.</summary>
    internal const byte Will = 251;

    /// <summary>The sender refuses, or disables, an option on its own side.</summary>
    internal const byte Wont = 252;

    /// <summary>Starts an option's subnegotiation.</summary>
    internal const byte Sb = 250;

    /// <summary>Ends a subnegotiation.</summary>
    internal const byte Se = 240;

    /// <summary>Interrupt Process: what a client sends for its interrupt key.</summary>
    internal const byte InterruptProcess = 244;

    /// <summary>Erase Character: what a client sends for its erase key in line mode.</summary>
    internal const byte EraseCharacter = 247;

    /// <summary>The server echoes what is typed (RFC 857).</summary>
    internal const byte OptionEcho = 1;

    /// <summary>No go-ahead signals: characters flow both ways at any time (RFC 858).</summary>
    internal const byte OptionSuppressGoAhead = 3;

    /// <summary>The client reports its window size (RFC 1073).</summary>
    internal const byte OptionWindowSize = 31;

    private const byte Escape = 0x1B;
    private const byte Delete = 0x7F;

    /// <summary>Largest window a client may report; a larger one is taken at this size, which bounds the screen it is given.</summary>
    private const int MaxWindowColumns = 512;

    /// <summary>Largest window a client may report, in rows.</summary>
    private const int MaxWindowRows = 256;

    /// <summary>Longest subnegotiation kept; the rest of a longer one is dropped.</summary>
    private const int MaxSubnegotiationLength = 64;

    /// <summary>Largest numeric parameter of an escape sequence; digits past it are ignored.</summary>
    private const int MaxSequenceParameter = 9999;

    private enum CommandState
    {
        Data,
        Command,
        Option,
        Subnegotiation,
        SubnegotiationCommand,
    }

    private enum KeyState
    {
        Text,
        Escape,
        ControlSequence,
        SingleShift,
    }

    /// <summary>Options enabled on the server's side.</summary>
    private readonly bool[] _localOptions = new bool[256];

    /// <summary>Options enabled, or asked for, on the client's side.</summary>
    private readonly bool[] _remoteOptions = new bool[256];

    private readonly List<byte> _subnegotiation = [];

    private CommandState _commandState;
    private byte _command;

    private KeyState _keyState;
    private bool _afterCarriageReturn;
    private int _utf8Remaining;
    private int _utf8Value;

    // The first two parameters of a control sequence: the key, and the xterm modifiers.
    private int _parameterIndex;
    private int _firstParameter;
    private int _secondParameter;

    /// <summary>Columns of the client's window, or zero until it reports them.</summary>
    public int WindowColumns { get; private set; }

    /// <summary>Rows of the client's window, or zero until it reports them.</summary>
    public int WindowRows { get; private set; }

    /// <summary>
    /// Opens the negotiation: the server echoes and suppresses go-ahead,
    /// which puts a client in character-at-a-time mode, and asks for the
    /// window size.
    /// </summary>
    /// <param name="replies">Where the commands to send are appended.</param>
    public void Negotiate(List<byte> replies)
    {
        _localOptions[OptionEcho] = true;
        _localOptions[OptionSuppressGoAhead] = true;
        _remoteOptions[OptionSuppressGoAhead] = true;
        _remoteOptions[OptionWindowSize] = true;

        AppendCommand(replies, Will, OptionEcho);
        AppendCommand(replies, Will, OptionSuppressGoAhead);
        AppendCommand(replies, Do, OptionSuppressGoAhead);
        AppendCommand(replies, Do, OptionWindowSize);
    }

    /// <summary>Decodes received bytes.</summary>
    /// <param name="data">The bytes, as received.</param>
    /// <param name="keys">Where the decoded keys are appended.</param>
    /// <param name="replies">Where the answers to the client's negotiation are appended.</param>
    public void Decode(ReadOnlySpan<byte> data, List<KeyEvent> keys, List<byte> replies)
    {
        foreach (byte value in data)
        {
            DecodeCommand(value, keys, replies);
        }

        if (_keyState == KeyState.Escape)
        {
            keys.Add(Key('\x1b', ConsoleKeyEx.Escape));
            _keyState = KeyState.Text;
        }
    }

    private void DecodeCommand(byte value, List<KeyEvent> keys, List<byte> replies)
    {
        switch (_commandState)
        {
            case CommandState.Data:
                if (value == Iac)
                {
                    _commandState = CommandState.Command;
                }
                else
                {
                    DecodeKey(value, keys);
                }
                break;

            case CommandState.Command:
                _commandState = CommandState.Data;
                switch (value)
                {
                    case Iac:
                        DecodeKey(value, keys);
                        break;
                    case Do:
                    case Dont:
                    case Will:
                    case Wont:
                        _command = value;
                        _commandState = CommandState.Option;
                        break;
                    case Sb:
                        _subnegotiation.Clear();
                        _commandState = CommandState.Subnegotiation;
                        break;
                    case InterruptProcess:
                        keys.Add(Key('\x03', ConsoleKeyEx.C, control: true));
                        break;
                    case EraseCharacter:
                        keys.Add(Key('\b', ConsoleKeyEx.Backspace));
                        break;
                }
                break;

            case CommandState.Option:
                _commandState = CommandState.Data;
                Answer(_command, value, replies);
                break;

            case CommandState.Subnegotiation:
                if (value == Iac)
                {
                    _commandState = CommandState.SubnegotiationCommand;
                }
                else if (_subnegotiation.Count < MaxSubnegotiationLength)
                {
                    _subnegotiation.Add(value);
                }
                break;

            case CommandState.SubnegotiationCommand:
                if (value == Se)
                {
                    _commandState = CommandState.Data;
                    EndSubnegotiation();
                }
                else
                {
                    // IAC IAC is a data byte of 255 inside the subnegotiation.
                    _commandState = CommandState.Subnegotiation;
                    if (value == Iac && _subnegotiation.Count < MaxSubnegotiationLength)
                    {
                        _subnegotiation.Add(value);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Answers one negotiation command. A request that changes nothing is an
    /// acknowledgement and gets no answer, which is what keeps two sides from
    /// answering each other forever (RFC 854).
    /// </summary>
    private void Answer(byte command, byte option, List<byte> replies)
    {
        switch (command)
        {
            case Do:
                if (option is OptionEcho or OptionSuppressGoAhead)
                {
                    if (!_localOptions[option])
                    {
                        _localOptions[option] = true;
                        AppendCommand(replies, Will, option);
                    }
                }
                else
                {
                    AppendCommand(replies, Wont, option);
                }
                break;

            case Dont:
                if (_localOptions[option])
                {
                    _localOptions[option] = false;
                    AppendCommand(replies, Wont, option);
                }
                break;

            case Will:
                if (option is OptionSuppressGoAhead or OptionWindowSize)
                {
                    if (!_remoteOptions[option])
                    {
                        _remoteOptions[option] = true;
                        AppendCommand(replies, Do, option);
                    }
                }
                else
                {
                    AppendCommand(replies, Dont, option);
                }
                break;

            case Wont:
                if (_remoteOptions[option])
                {
                    _remoteOptions[option] = false;
                    AppendCommand(replies, Dont, option);
                }
                break;
        }
    }

    /// <summary>Reads a completed subnegotiation; only the window size is understood.</summary>
    private void EndSubnegotiation()
    {
        if (_subnegotiation.Count < 5 || _subnegotiation[0] != OptionWindowSize)
        {
            return;
        }

        int columns = (_subnegotiation[1] << 8) | _subnegotiation[2];
        int rows = (_subnegotiation[3] << 8) | _subnegotiation[4];

        // Zero means the client does not know that dimension.
        if (columns > 0 && rows > 0)
        {
            WindowColumns = Math.Min(columns, MaxWindowColumns);
            WindowRows = Math.Min(rows, MaxWindowRows);
        }
    }

    private void DecodeKey(byte value, List<KeyEvent> keys)
    {
        switch (_keyState)
        {
            case KeyState.Text:
                DecodeText(value, keys);
                break;

            case KeyState.Escape:
                DecodeAfterEscape(value, keys);
                break;

            case KeyState.ControlSequence:
                DecodeControlSequence(value, keys);
                break;

            case KeyState.SingleShift:
                _keyState = KeyState.Text;
                DecodeSingleShift(value, keys);
                break;
        }
    }

    private void DecodeText(byte value, List<KeyEvent> keys)
    {
        if (_utf8Remaining > 0)
        {
            if ((value & 0xC0) == 0x80)
            {
                _utf8Value = (_utf8Value << 6) | (value & 0x3F);
                if (--_utf8Remaining == 0)
                {
                    // A character past the Basic Multilingual Plane has no
                    // single char, and a surrogate is not a character.
                    char decoded = _utf8Value is > 0xFFFF or (>= 0xD800 and <= 0xDFFF) ? '?' : (char)_utf8Value;
                    keys.Add(CharacterKey(decoded));
                }

                return;
            }

            // A truncated sequence: report it, then read this byte afresh.
            _utf8Remaining = 0;
            keys.Add(CharacterKey('?'));
        }

        // CR LF and CR NUL are both the end of a line (RFC 854): Enter was
        // reported for the CR, so the byte after it is dropped.
        bool afterCarriageReturn = _afterCarriageReturn;
        _afterCarriageReturn = false;

        switch (value)
        {
            case (byte)'\r':
                _afterCarriageReturn = true;
                keys.Add(Key('\r', ConsoleKeyEx.Enter));
                return;
            case (byte)'\n':
                if (!afterCarriageReturn)
                {
                    keys.Add(Key('\r', ConsoleKeyEx.Enter));
                }
                return;
            case 0:
                return;
            case Escape:
                _keyState = KeyState.Escape;
                return;
            case (byte)'\b':
            case Delete:
                keys.Add(Key('\b', ConsoleKeyEx.Backspace));
                return;
            case (byte)'\t':
                keys.Add(Key('\t', ConsoleKeyEx.Tab));
                return;
        }

        if (value < 0x20)
        {
            // Control and a letter.
            if (value <= 26)
            {
                keys.Add(Key((char)value, LetterKey((char)('A' + value - 1)), control: true));
            }

            return;
        }

        if (value < 0x80)
        {
            keys.Add(CharacterKey((char)value));
        }
        else if ((value & 0xE0) == 0xC0)
        {
            _utf8Remaining = 1;
            _utf8Value = value & 0x1F;
        }
        else if ((value & 0xF0) == 0xE0)
        {
            _utf8Remaining = 2;
            _utf8Value = value & 0x0F;
        }
        else if ((value & 0xF8) == 0xF0)
        {
            _utf8Remaining = 3;
            _utf8Value = value & 0x07;
        }
    }

    private void DecodeAfterEscape(byte value, List<KeyEvent> keys)
    {
        switch (value)
        {
            case (byte)'[':
                _keyState = KeyState.ControlSequence;
                _parameterIndex = 0;
                _firstParameter = 0;
                _secondParameter = 0;
                return;

            case (byte)'O':
                _keyState = KeyState.SingleShift;
                return;

            case Escape:
                // Escape pressed twice: the first is a key of its own.
                keys.Add(Key('\x1b', ConsoleKeyEx.Escape));
                return;
        }

        _keyState = KeyState.Text;
        if (value is >= 0x20 and < 0x7F)
        {
            // A terminal sends Alt and a character as escape, then the character.
            KeyEvent key = CharacterKey((char)value);
            key.Modifiers |= ConsoleModifiers.Alt;
            keys.Add(key);
            return;
        }

        keys.Add(Key('\x1b', ConsoleKeyEx.Escape));
        DecodeText(value, keys);
    }

    private void DecodeControlSequence(byte value, List<KeyEvent> keys)
    {
        if (value is >= (byte)'0' and <= (byte)'9')
        {
            if (_parameterIndex == 0)
            {
                _firstParameter = Math.Min(_firstParameter * 10 + value - '0', MaxSequenceParameter);
            }
            else if (_parameterIndex == 1)
            {
                _secondParameter = Math.Min(_secondParameter * 10 + value - '0', MaxSequenceParameter);
            }

            return;
        }

        if (value == ';')
        {
            _parameterIndex++;
            return;
        }

        if (value is >= 0x20 and <= 0x3F)
        {
            // Private markers and intermediates: nothing a key needs.
            return;
        }

        _keyState = KeyState.Text;
        if (value is < 0x40 or > 0x7E)
        {
            // Not a sequence after all.
            return;
        }

        ConsoleKeyEx key = value switch
        {
            (byte)'A' => ConsoleKeyEx.UpArrow,
            (byte)'B' => ConsoleKeyEx.DownArrow,
            (byte)'C' => ConsoleKeyEx.RightArrow,
            (byte)'D' => ConsoleKeyEx.LeftArrow,
            (byte)'H' => ConsoleKeyEx.Home,
            (byte)'F' => ConsoleKeyEx.End,
            (byte)'P' => ConsoleKeyEx.F1,
            (byte)'Q' => ConsoleKeyEx.F2,
            (byte)'R' => ConsoleKeyEx.F3,
            (byte)'S' => ConsoleKeyEx.F4,
            (byte)'Z' => ConsoleKeyEx.Tab,
            (byte)'~' => TildeKey(_firstParameter),
            _ => ConsoleKeyEx.NoName,
        };

        if (key == ConsoleKeyEx.NoName)
        {
            return;
        }

        // xterm sends the modifiers as a second parameter: one more than a
        // bit set of Shift (1), Alt (2) and Control (4).
        int modifiers = Math.Max(_secondParameter - 1, 0);
        bool shift = (modifiers & 1) != 0 || value == 'Z';
        char keyChar = key == ConsoleKeyEx.Tab ? '\t' : '\0';
        keys.Add(new KeyEvent(keyChar, key, shift, (modifiers & 2) != 0, (modifiers & 4) != 0, KeyEvent.KeyEventType.Make));
    }

    private static void DecodeSingleShift(byte value, List<KeyEvent> keys)
    {
        ConsoleKeyEx key = value switch
        {
            (byte)'A' => ConsoleKeyEx.UpArrow,
            (byte)'B' => ConsoleKeyEx.DownArrow,
            (byte)'C' => ConsoleKeyEx.RightArrow,
            (byte)'D' => ConsoleKeyEx.LeftArrow,
            (byte)'H' => ConsoleKeyEx.Home,
            (byte)'F' => ConsoleKeyEx.End,
            (byte)'P' => ConsoleKeyEx.F1,
            (byte)'Q' => ConsoleKeyEx.F2,
            (byte)'R' => ConsoleKeyEx.F3,
            (byte)'S' => ConsoleKeyEx.F4,
            (byte)'M' => ConsoleKeyEx.Enter,
            _ => ConsoleKeyEx.NoName,
        };

        if (key != ConsoleKeyEx.NoName)
        {
            keys.Add(Key(key == ConsoleKeyEx.Enter ? '\r' : '\0', key));
        }
    }

    /// <summary>The key of a <c>CSI n ~</c> sequence, by its number (the VT220 and xterm set).</summary>
    private static ConsoleKeyEx TildeKey(int number) => number switch
    {
        1 or 7 => ConsoleKeyEx.Home,
        2 => ConsoleKeyEx.Insert,
        3 => ConsoleKeyEx.Delete,
        4 or 8 => ConsoleKeyEx.End,
        5 => ConsoleKeyEx.PageUp,
        6 => ConsoleKeyEx.PageDown,
        11 => ConsoleKeyEx.F1,
        12 => ConsoleKeyEx.F2,
        13 => ConsoleKeyEx.F3,
        14 => ConsoleKeyEx.F4,
        15 => ConsoleKeyEx.F5,
        17 => ConsoleKeyEx.F6,
        18 => ConsoleKeyEx.F7,
        19 => ConsoleKeyEx.F8,
        20 => ConsoleKeyEx.F9,
        21 => ConsoleKeyEx.F10,
        23 => ConsoleKeyEx.F11,
        24 => ConsoleKeyEx.F12,
        _ => ConsoleKeyEx.NoName,
    };

    /// <summary>The key a character is typed with: its letter or digit key, the space bar, or no particular key.</summary>
    private static KeyEvent CharacterKey(char value)
    {
        ConsoleKeyEx key = value switch
        {
            >= 'a' and <= 'z' => LetterKey((char)(value - 'a' + 'A')),
            >= 'A' and <= 'Z' => LetterKey(value),
            >= '0' and <= '9' => DigitKey(value),
            ' ' => ConsoleKeyEx.Spacebar,
            _ => ConsoleKeyEx.NoName,
        };

        return new KeyEvent(value, key, value is >= 'A' and <= 'Z', false, false, KeyEvent.KeyEventType.Make);
    }

    private static KeyEvent Key(char keyChar, ConsoleKeyEx key, bool control = false) =>
        new KeyEvent(keyChar, key, false, false, control, KeyEvent.KeyEventType.Make);

    /// <summary>The key of an upper-case letter; <see cref="ConsoleKeyEx"/> lists them in keyboard order, not alphabetical.</summary>
    private static ConsoleKeyEx LetterKey(char letter) => letter switch
    {
        'A' => ConsoleKeyEx.A,
        'B' => ConsoleKeyEx.B,
        'C' => ConsoleKeyEx.C,
        'D' => ConsoleKeyEx.D,
        'E' => ConsoleKeyEx.E,
        'F' => ConsoleKeyEx.F,
        'G' => ConsoleKeyEx.G,
        'H' => ConsoleKeyEx.H,
        'I' => ConsoleKeyEx.I,
        'J' => ConsoleKeyEx.J,
        'K' => ConsoleKeyEx.K,
        'L' => ConsoleKeyEx.L,
        'M' => ConsoleKeyEx.M,
        'N' => ConsoleKeyEx.N,
        'O' => ConsoleKeyEx.O,
        'P' => ConsoleKeyEx.P,
        'Q' => ConsoleKeyEx.Q,
        'R' => ConsoleKeyEx.R,
        'S' => ConsoleKeyEx.S,
        'T' => ConsoleKeyEx.T,
        'U' => ConsoleKeyEx.U,
        'V' => ConsoleKeyEx.V,
        'W' => ConsoleKeyEx.W,
        'X' => ConsoleKeyEx.X,
        'Y' => ConsoleKeyEx.Y,
        'Z' => ConsoleKeyEx.Z,
        _ => ConsoleKeyEx.NoName,
    };

    private static ConsoleKeyEx DigitKey(char digit) => digit switch
    {
        '0' => ConsoleKeyEx.D0,
        '1' => ConsoleKeyEx.D1,
        '2' => ConsoleKeyEx.D2,
        '3' => ConsoleKeyEx.D3,
        '4' => ConsoleKeyEx.D4,
        '5' => ConsoleKeyEx.D5,
        '6' => ConsoleKeyEx.D6,
        '7' => ConsoleKeyEx.D7,
        '8' => ConsoleKeyEx.D8,
        '9' => ConsoleKeyEx.D9,
        _ => ConsoleKeyEx.NoName,
    };

    private static void AppendCommand(List<byte> replies, byte command, byte option)
    {
        replies.Add(Iac);
        replies.Add(command);
        replies.Add(option);
    }
}
