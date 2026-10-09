// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Network.Protocols.Tcp;

/// <summary>
/// Represents a TCP connection status.
/// </summary>
internal enum Status
{
    /// <summary>
    /// Wait for a connection request from any remote TCP and port.
    /// </summary>
    LISTEN,

    /// <summary>
    /// Wait for a matching connection request after having sent a connection request.
    /// </summary>
    SYN_SENT,

    /// <summary>
    /// Wait for a confirming connection request acknowledgment after having both received and sent a connection request.
    /// </summary>
    SYN_RECEIVED,

    /// <summary>
    /// Represents an open connection, data received can be delivered to the user. The normal state for the data transfer phase of the connection.
    /// </summary>
    ESTABLISHED,

    /// <summary>
    /// Wait for a connection termination request from the remote TCP, or an acknowledgment of the connection termination request previously sent.
    /// </summary>
    FIN_WAIT1,

    /// <summary>
    /// Wait for a connection termination request from the remote TCP.
    /// </summary>
    FIN_WAIT2,

    /// <summary>
    /// Wait for a connection termination request from the local user.
    /// </summary>
    CLOSE_WAIT,

    /// <summary>
    /// Wait for a connection termination request acknowledgment from the remote TCP.
    /// </summary>
    CLOSING,

    /// <summary>
    /// Wait for an acknowledgment of the connection termination request previously sent to the remote TCP (which includes an acknowledgment of its connection termination request).
    /// </summary>
    LAST_ACK,

    /// <summary>
    /// Wait for enough time to pass to be sure the remote TCP received the acknowledgment of its connection termination request.
    /// </summary>
    TIME_WAIT,

    /// <summary>
    /// Represents no connection state.
    /// </summary>
    CLOSED
}
