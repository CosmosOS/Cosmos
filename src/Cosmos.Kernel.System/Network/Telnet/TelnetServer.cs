// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Network.TCP;
using Cosmos.Kernel.System.Sessions;

namespace Cosmos.Kernel.System.Network.Telnet;

/// <summary>
/// A Telnet server: every client that connects gets a console session of
/// its own, and a thread bound to it that runs the shell the server was
/// given, so <see cref="Console"/> on that thread reads the client's keys
/// and writes to its terminal. The session is listed and numbered with the
/// others by <see cref="SessionManager"/>, and can be shown on the display.
/// </summary>
/// <remarks>
/// Telnet sends everything in the clear, password included: serve it on a
/// network you trust. A session's screen lives on the display's canvas, so
/// the server needs the kernel console.
/// </remarks>
public sealed class TelnetServer
{
    /// <summary>The port Telnet is served on by default.</summary>
    public const ushort DefaultPort = 23;

    /// <summary>How long a new connection may take to report its window size before its shell starts anyway, in milliseconds.</summary>
    private const uint NegotiationTimeoutMs = 500;

    /// <summary>How long a peer may leave the handshake half done before the server listens afresh, in milliseconds.</summary>
    private const uint HandshakeTimeoutMs = 5000;

    private readonly Action<ConsoleSession> _shell;

    /// <summary>Wakes the accepting thread: signaled as a peer's handshake moves, and by <see cref="Stop"/>.</summary>
    private readonly InterruptEvent _wake = new();

    private volatile bool _stopRequested;
    private volatile bool _running;

    /// <summary>The TCP port the server listens on.</summary>
    public ushort Port { get; }

    /// <summary>Whether the server accepts connections. It goes false shortly after <see cref="Stop"/>.</summary>
    public bool IsRunning => _running;

    /// <summary>Creates a server; <see cref="Start"/> starts it.</summary>
    /// <param name="shell">What each connection runs, on a thread bound to its session; the connection is closed when it returns.</param>
    /// <param name="port">The TCP port to listen on.</param>
    public TelnetServer(Action<ConsoleSession> shell, ushort port = DefaultPort)
    {
        ArgumentNullException.ThrowIfNull(shell);

        _shell = shell;
        Port = port;
    }

    /// <summary>Starts accepting connections, on a thread of the server's own.</summary>
    /// <exception cref="InvalidOperationException">The server already runs, networking or the scheduler is compiled out, there is no kernel console, or the thread did not start.</exception>
    public void Start()
    {
        if (!KernelFeatures.Network)
        {
            throw new InvalidOperationException("Networking is disabled. Set CosmosEnableNetwork=true in your csproj to enable it.");
        }

        if (!KernelFeatures.Scheduler)
        {
            throw new InvalidOperationException("The Telnet server runs its sessions on threads. Set CosmosEnableScheduler=true in your csproj to enable it.");
        }

        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        if (_running)
        {
            throw new InvalidOperationException($"The Telnet server already runs on port {Port}.");
        }

        _stopRequested = false;
        _running = true;
        if (!KernelThread.TryStart(AcceptConnections))
        {
            _running = false;
            throw new InvalidOperationException("The Telnet server's thread did not start: the scheduler is not running.");
        }
    }

    /// <summary>
    /// Stops accepting connections. The sessions already open stay open;
    /// <see cref="ConsoleSession.Close"/> ends one.
    /// </summary>
    public void Stop()
    {
        _stopRequested = true;
        _wake.Signal();
    }

    /// <summary>The server's thread: listens, and hands each connection to a session, until stopped.</summary>
    private void AcceptConnections()
    {
        Serial.WriteString($"[Telnet] Listening on port {Port}\n");
        try
        {
            while (!_stopRequested)
            {
                TcpStream stream = TcpStream.Listen(Port);
                if (WaitForPeer(stream))
                {
                    Accept(stream);
                }
                else
                {
                    stream.Close();
                }
            }
        }
        catch (Exception exception)
        {
            Serial.WriteString($"[Telnet] Server stopped by {exception.GetType().Name}: {exception.Message}\n");
        }
        finally
        {
            _running = false;
            Serial.WriteString($"[Telnet] Stopped listening on port {Port}\n");
        }
    }

    /// <summary>
    /// Waits until a peer completes the handshake, blocked until the
    /// listening connection's status moves.
    /// </summary>
    /// <returns>False when the server is stopping, or the handshake was reset or stalled, and the caller listens afresh.</returns>
    private bool WaitForPeer(TcpStream stream)
    {
        stream.ReceiveSignal = _wake;

        long ticksPerMillisecond = Stopwatch.Frequency / 1000;
        long handshakeDeadline = 0;
        while (!_stopRequested)
        {
            if (stream.IsOpen)
            {
                return true;
            }

            if (!stream.IsListening)
            {
                return false;
            }

            if (!stream.IsConnecting)
            {
                handshakeDeadline = 0;
                _wake.Wait();
                continue;
            }

            long now = Stopwatch.GetTimestamp();
            if (handshakeDeadline == 0)
            {
                handshakeDeadline = now + ticksPerMillisecond * HandshakeTimeoutMs;
            }
            else if (now >= handshakeDeadline)
            {
                return false;
            }

            _wake.Wait((uint)Math.Max((handshakeDeadline - now) / ticksPerMillisecond, 1));
        }

        return false;
    }

    /// <summary>Gives a connection its session, and starts the shell on it.</summary>
    private void Accept(TcpStream stream)
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        KernelConsole console = KernelConsole.Default;
        TelnetSession session = new(stream, new KernelConsole(console.Canvas, console.Font, TelnetSession.DefaultCols, TelnetSession.DefaultRows));
        session.Negotiate(NegotiationTimeoutMs);
        if (session.IsClosed)
        {
            return;
        }

        if (!SessionManager.TryRegister(session))
        {
            session.Write($"All {SessionManager.MaxSessions} console sessions are in use.\n");
            session.Close();
            return;
        }

        Serial.WriteString($"[Telnet] {session.Name} connected as session {session.Id}\n");
        try
        {
            SessionManager.Start(session, () => _shell(session));
        }
        catch (InvalidOperationException exception)
        {
            Serial.WriteString($"[Telnet] Session {session.Id} did not start: {exception.Message}\n");
            session.Close();
        }
    }
}
