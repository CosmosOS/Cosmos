using System.Text;
using Cosmos.TestRunner.Protocol;

namespace Cosmos.TestRunner.Engine.Protocol;

/// <summary>
/// Picks the <see cref="Ds2Vs.HostRequest"/> frames out of the UART stream
/// while the guest runs, one byte at a time, so the host can act on them
/// before the guest's test gives up waiting. Every other frame is left to
/// <see cref="UartMessageParser"/>, which reads the whole log afterwards.
/// </summary>
internal sealed class HostRequestScanner
{
    /// <summary>Frame header: [MAGIC:4][Command:1][Length:2].</summary>
    private const int HeaderLengthBytes = 7;

    /// <summary>Offset of the Command byte within the header.</summary>
    private const int CommandOffset = 4;

    /// <summary>Bit shift of the high byte of the little-endian payload length.</summary>
    private const int LengthHighShift = 8;

    /// <summary>Longest request accepted; a longer length field means the header was noise.</summary>
    private const int MaxRequestBytes = 256;

    private static readonly byte[] Magic =
    {
        Consts.SerialSignatureByte0,
        Consts.SerialSignatureByte1,
        Consts.SerialSignatureByte2,
        Consts.SerialSignatureByte3
    };

    private readonly byte[] _header = new byte[HeaderLengthBytes];
    private int _headerLength;
    private byte[]? _payload;
    private int _payloadLength;

    /// <summary>
    /// Takes the next UART byte. Returns the request once its last byte
    /// arrived, and null otherwise.
    /// </summary>
    public string? Feed(byte value)
    {
        if (_payload is not null)
        {
            _payload[_payloadLength++] = value;
            if (_payloadLength < _payload.Length)
            {
                return null;
            }

            string request = Encoding.ASCII.GetString(_payload);
            _payload = null;
            return request;
        }

        if (_headerLength < Magic.Length)
        {
            // A byte that breaks the magic may still start the next one.
            if (value == Magic[_headerLength])
            {
                _header[_headerLength++] = value;
            }
            else
            {
                _headerLength = value == Magic[0] ? 1 : 0;
            }

            return null;
        }

        _header[_headerLength++] = value;
        if (_headerLength == CommandOffset + 1 && value != Ds2Vs.HostRequest)
        {
            _headerLength = 0;
            return null;
        }

        if (_headerLength < HeaderLengthBytes)
        {
            return null;
        }

        _headerLength = 0;
        int length = _header[HeaderLengthBytes - 2] | (_header[HeaderLengthBytes - 1] << LengthHighShift);
        if (length is > 0 and <= MaxRequestBytes)
        {
            _payload = new byte[length];
            _payloadLength = 0;
        }

        return null;
    }
}
