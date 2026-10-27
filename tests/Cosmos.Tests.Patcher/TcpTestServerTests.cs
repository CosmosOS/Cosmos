// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.TestRunner.Engine.Hosts;

namespace Cosmos.Tests.Patcher;

/// <summary>
/// The engine's TCP test server as the kernel's <c>TCP_ServerAccept</c> test
/// drives it: the connect-and-echo exchange starts on the <c>tcp-connect</c>
/// host request, not on a clock, so it reaches a listener that is up, and
/// the request is the server's alone, the QMP monitor never seeing it.
/// </summary>
[Collection("PatcherTests")]
public class TcpTestServerTests
{
    /// <summary>Bound by the OS on a free port, so the tests share no port with a run on the same machine.</summary>
    private static int FreePort()
    {
        using TcpListener probe = new(IPAddress.Loopback, 0);
        probe.Start();
        return ((IPEndPoint)probe.LocalEndpoint).Port;
    }

    // Before the request the server connects to nothing: a listener on its
    // connect port sees no connection for as long as it cares to wait.
    // After the request, the exchange runs: the server connects, sends
    // TEST_FROM_HOST and takes the echo back.
    [Fact]
    public async Task TryDispatch_ConnectRequest_RunsTheEchoExchangeOnRequestOnly()
    {
        int connectPort = FreePort();
        using TcpListener kernel = new(IPAddress.Loopback, connectPort);
        kernel.Start();

        using TcpTestServer server = new(listenPort: FreePort(), connectPort: connectPort, lingerPort: FreePort());
        server.Start();
        try
        {
            await Task.Delay(300);
            Assert.False(kernel.Pending(), "the server must not connect before the kernel asks");
            Assert.Null(server.LastEchoFromKernel);

            Assert.True(server.TryDispatch(TcpTestServer.ConnectRequest));

            using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(5));
            using TcpClient accepted = await kernel.AcceptTcpClientAsync(timeout.Token);
            NetworkStream stream = accepted.GetStream();
            byte[] buffer = new byte[64];
            int read = await stream.ReadAsync(buffer, timeout.Token);
            Assert.Equal("TEST_FROM_HOST", Encoding.ASCII.GetString(buffer, 0, read));

            await stream.WriteAsync(buffer.AsMemory(0, read), timeout.Token);

            while (server.LastEchoFromKernel is null && !timeout.IsCancellationRequested)
            {
                await Task.Delay(20, CancellationToken.None);
            }

            Assert.Equal("TEST_FROM_HOST", server.LastEchoFromKernel);
        }
        finally
        {
            await server.StopAsync();
        }
    }

    // Every other request is the QMP monitor's: the server declines it so
    // the host hands it on.
    [Theory]
    [InlineData("key-press a")]
    [InlineData("pci-unplug 0")]
    [InlineData("tcp-connect now")]
    public void TryDispatch_DeclinesTheMonitorsRequests(string request)
    {
        using TcpTestServer server = new(listenPort: FreePort(), connectPort: FreePort(), lingerPort: FreePort());

        Assert.False(server.TryDispatch(request));
    }

    // A request before Start or after StopAsync is the server's to answer,
    // reported and dropped rather than passed to the monitor as unknown.
    [Fact]
    public void TryDispatch_ConnectRequest_WhileStoppedIsTakenAndDropped()
    {
        using TcpTestServer server = new(listenPort: FreePort(), connectPort: FreePort(), lingerPort: FreePort());

        Assert.True(server.TryDispatch(TcpTestServer.ConnectRequest));
        Assert.Null(server.LastEchoFromKernel);
    }
}
