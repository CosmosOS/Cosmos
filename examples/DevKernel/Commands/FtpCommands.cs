// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.IO;
using System.Net;
using Cosmos.Kernel.System.Diagnostics;
using CosmosFtpServer;
using DevKernel.Shell;
using SysThread = System.Threading.Thread;

namespace DevKernel.Commands;

/// <summary>
/// The FTP server of the CosmosFtpServer package, serving a directory of the
/// VFS on a thread of its own so the shell stays usable.
/// </summary>
internal static class FtpCommands
{
    /// <summary>Help section these commands are listed under.</summary>
    private const string Category = "Network";

    /// <summary>The FTP server <c>ftpd</c> started, if any.</summary>
    private static FtpServer? s_ftp;

    public static void Register(CommandShell shell)
    {
        shell.Register(
            Category,
            new ShellCommand
            {
                Name = "ftpd",
                Usage = "ftpd [dir] [port] [pasv-ip]|stop",
                Description = "Serve a directory over FTP (default: this one, port 21)",
                MaxArgs = 3,
                Execute = static (context, args) => RunFtpd(context, args),
            });
    }

    private static void RunFtpd(ShellContext context, CommandArgs args)
    {
        if (args.Count == 1 && args[0] == "stop")
        {
            if (s_ftp is not { IsListening: true })
            {
                Terminal.Error("The FTP server is not running.");
                return;
            }

            s_ftp.Close();
            Terminal.Success("FTP server stopped.");
            return;
        }

        if (s_ftp is { IsListening: true })
        {
            Terminal.Error($"The FTP server already runs on port {s_ftp.Port}.");
            return;
        }

        string root = context.ResolveNormalized(args.GetOrDefault(0, context.Cwd));
        int port = FtpServer.DefaultPort;
        if (args.Count > 1 && (!int.TryParse(args[1], out port) || port is < 1 or > ushort.MaxValue))
        {
            Terminal.Error($"Invalid port: {args[1]}");
            return;
        }

        // The address PASV replies name, for a guest behind QEMU's NAT:
        // 127.0.0.1 lets a client on the host, such as FileZilla, that
        // connects where the reply says reach the forwarded passive ports.
        IPAddress? passiveAddress = null;
        if (args.Count > 2 && !IPAddress.TryParse(args[2], out passiveAddress))
        {
            Terminal.Error($"Invalid IPv4 address: {args[2]}");
            return;
        }

        FtpServer server;
        try
        {
            server = new FtpServer(root, port)
            {
                Log = static message => Log.WriteString($"{message}\n"),
                PassiveAddress = passiveAddress,
            };
        }
        catch (DirectoryNotFoundException ex)
        {
            Terminal.Error(ex.Message);
            return;
        }

        new SysThread(() => Serve(server)).Start();
        s_ftp = server;
        Terminal.Success($"FTP server serving {root} on port {port}, passive ports {server.PassivePortMin}-{server.PassivePortMax}.");

        if (passiveAddress is null)
        {
            Terminal.Hint("Under QEMU, add 127.0.0.1 as pasv-ip for clients that connect where PASV says, such as FileZilla.");
        }

        if (!context.Network.IsConfigured)
        {
            Terminal.Hint("The network is not configured yet: run 'dhcp' or 'netconfig'.");
        }
    }

    private static void Serve(FtpServer server)
    {
        // The server's thread has no shell to report to: whatever stops it
        // goes to the log, and 'ftpd' can start a new one.
        try
        {
            server.Listen();
        }
        catch (Exception ex)
        {
            Log.WriteString($"[FTP] Server stopped by {ex.GetType().Name}: {ex.Message}\n");
        }
    }
}
