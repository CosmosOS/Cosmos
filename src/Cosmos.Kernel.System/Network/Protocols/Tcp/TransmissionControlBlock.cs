// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Network.Protocols.Tcp;

/// <summary>
/// Represents a Transmission Control Block (TCB).
/// </summary>
internal class TransmissionControlBlock
{
    // Send sequence variables

    /// <summary>
    /// Send unacknowledged.
    /// </summary>
    public uint SndUna { get; set; }

    /// <summary>
    /// Send next.
    /// </summary>
    public uint SndNxt { get; set; }

    /// <summary>
    /// Send window.
    /// </summary>
    public ushort SndWnd { get; set; }

    /// <summary>
    /// Send urgent pointer.
    /// </summary>
    public uint SndUp { get; set; }

    /// <summary>
    /// Segment sequence number used for last window update.
    /// </summary>
    public uint SndWl1 { get; set; }

    /// <summary>
    /// Segment acknowledgment number used for last window update.
    /// </summary>
    public uint SndWl2 { get; set; }

    /// <summary>
    /// Initial send sequence number.
    /// </summary>
    public uint ISS { get; set; }

    // Receive sequence variables

    /// <summary>
    /// Receive next.
    /// </summary>
    public uint RcvNxt { get; set; }

    /// <summary>
    /// Receive window.
    /// </summary>
    public uint RcvWnd { get; set; }

    /// <summary>
    /// Receive urgent pointer.
    /// </summary>
    public uint RcvUp { get; set; }

    /// <summary>
    /// Initial receive sequence number.
    /// </summary>
    public uint IRS { get; set; }
}
