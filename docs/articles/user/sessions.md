# Console Sessions and Telnet

In this article, we will discuss console sessions on Cosmos Gen3: running several shells side by side on the local display, switching between them the way Linux switches ttys, and serving a shell to remote terminals over Telnet.

A console session is one console that `System.Console` can be bound to: a screen, and the keys typed at it. `Console` follows the calling thread. A thread started on a session reads that session's keys and writes to its screen, so the same `Console.ReadLine()` and `Console.WriteLine()` code runs unchanged on every session, local or remote.

| Kind | Keys from | Screen shown on | Created by |
|---|---|---|---|
| Primary console, `tty1` | Local keyboard | The display | The kernel, on `KernelConsole.Default` |
| Virtual console, `tty2` onward | Local keyboard | The display, while it is the active session | `SessionManager.CreateVirtualConsole()` |
| Telnet session | The Telnet client | The client's terminal, and the display while it is the active session | `TelnetServer` of the [Cosmos.Network.Telnet](https://github.com/CosmosOS/Cosmos.Network.Telnet) package, one per connection |

The snippets below rely on `using Cosmos.Kernel.System.Sessions;`.

Sessions need the scheduler (`CosmosEnableScheduler`), since every session other than the primary console runs on a thread of its own. They also need the kernel console, since every screen is a grid on the display's canvas. The Telnet server also needs networking (`CosmosEnableNetwork`).

## The primary console

`tty1` is the console every kernel already has. The kernel's main loop (`Run()`) runs on it, so does every thread that was not started on another session, and it cannot be closed. A kernel that never opens a session behaves exactly as before: `Console` and `KeyboardManager` read the keyboard directly.

## Virtual consoles

`SessionManager.CreateVirtualConsole()` opens a local console the size of the primary one. It starts hidden, and `SessionManager.Start` starts a thread on it:

```csharp
ConsoleSession session = SessionManager.CreateVirtualConsole();

SessionManager.Start(session, () =>
{
    Console.WriteLine("Hello from " + session.Name);

    while (Console.ReadLine() is { } line)
    {
        Console.WriteLine("You typed: " + line);
    }
});
```

The thread's `Console` is the session. When the body returns, the session closes: a virtual console goes away, and a remote session hangs up. An exception that escapes the body is logged to the serial port and ends the session the same way.

Threads the body starts are not bound to the session: their `Console` is the primary console. Bind them with `SessionManager.Start` if they should write to the session.

## Switching sessions

Sessions are numbered from 1 to 12 (`SessionManager.MaxSessions`), and the number is also the function key that shows it:

| Keys | Shows |
|---|---|
| Alt+F1 … Alt+F12 | Session 1 … 12 |
| Alt+Left, Alt+Right | The previous or next open session |

The session on the display is `SessionManager.Active`. The local keyboard types into it, whether it is local or remote, so you can take over a Telnet session from the machine itself. From code:

```csharp
foreach (ConsoleSession session in SessionManager.GetSessions())
{
    Console.WriteLine(session.Id + "  " + session.Name + (session.IsActive ? "  (on the display)" : ""));
}

SessionManager.Switch(2);   // show tty2
```

Once a second session exists, the keyboard is routed: each key goes straight to the session on the display. From then on the read members of `KeyboardManager` (`KeyAvailable`, `TryReadKey`, `ReadKey`, `Peek`) read the calling thread's session too, like `Console`.

When QEMU runs in a window, the host desktop may catch Alt with a function key before the guest sees it; Alt+F4 in particular can close the window. Alt with the arrows, or `SessionManager.Switch` from a shell command, avoid that.

## Serving a shell over Telnet

The Telnet server is a package of its own, [Cosmos.Network.Telnet](https://github.com/CosmosOS/Cosmos.Network.Telnet). Add it to your kernel:

```xml
<ItemGroup>
    <PackageReference Include="Cosmos.Network.Telnet" Version="1.0.0" />
</ItemGroup>
```

`TelnetServer` listens on a TCP port and gives every client a session of its own, with a thread that runs the shell you pass in:

```csharp
using Cosmos.Network.Telnet;

TelnetServer server = new(session =>
{
    Console.WriteLine("Welcome, " + session.Name);

    while (Console.ReadLine() is { } line && line != "exit")
    {
        Console.WriteLine("You typed: " + line);
    }
});

server.Start();   // port 23 by default: new TelnetServer(shell, 2323) picks another
```

`Start()` returns at once; the server accepts connections on its own thread until `Stop()`, which leaves the sessions already open connected. Set `Log` to receive a line for every connection, disconnection and failure, for example `Log = message => Cosmos.Kernel.System.Diagnostics.Log.WriteString(message + "\n")`. The network must be configured, by DHCP or by hand ([Network](network.md#configure-ipv4)), for a client to reach it.

The server negotiates the way a terminal expects:

- It echoes what is typed and suppresses go-ahead, which puts the client in character-at-a-time mode, so the line editor works remotely: arrows, Home and End, Backspace and Delete.
- It asks for the window size (NAWS), so the session's `Console.WindowWidth` and `WindowHeight` are the client's, and follow it when the window is resized.
- Output is sent as VT100 sequences: colours, cursor moves and `Console.Clear()` show on the client as they do on the display.

Under QEMU user networking, forward a host port to the guest's port 23, then connect from the host:

```
$ cosmos run --nic e1000e --hostfwd tcp::2323-:23
$ telnet localhost 2323
```

With plain QEMU, the forward is an option of the user-mode NIC: `-nic user,model=e1000e,hostfwd=tcp::2323-:23`.

> [!WARNING]
> Telnet sends everything in the clear, and the server asks for no password: anyone who reaches the port gets a shell. Serve it on a network you trust, such as QEMU's private user network.

## In the DevKernel

The DevKernel opens `tty2` to `tty4` at boot, each with a shell of its own, and has these commands:

| Command | Does |
|---|---|
| `sessions` (`who`) | Lists the sessions: `*` marks the one on the display, `>` your own |
| `tty` | Shows which session the shell runs on |
| `chvt <n>` | Shows session `n` on the display |
| `openvt` | Opens another virtual console with a shell |
| `exit` (`logout`) | Closes the session the shell runs on; the primary console stays |

## Current limitations

- There is no login: a Telnet client gets a shell as soon as it connects.
- There is no SSH yet. Telnet is a first transport over the same sessions; SSH needs a key exchange, a cipher and a random source the kernel does not have.
- The TCP stack does not retransmit. On QEMU's user network nothing is lost, but on a real network a lost segment stalls the session.
- A session's screen lives on the display's canvas, so a kernel without a display has no sessions, remote ones included.
- Ctrl+C reaches the session as a key; nothing interrupts a running command.

## Remote sessions in a library

A server for another kind of remote terminal is a class derived from `ConsoleSession`, the way the Telnet server's is:

- The `ConsoleSession(int cols, int rows)` constructor gives the session its screen, hidden until it is shown. Override `Name`, the name it is listed under, and `IsRemote`, true.
- `EnqueueInput(KeyEvent)` queues a key the terminal sent and wakes the session's reader; it may be called from any thread. `Resize(cols, rows)` follows the terminal's window.
- The `On` hooks report every change to send to the terminal: `OnWritten` for each character, `OnCleared`, `OnCursorMoved`, `OnCursorVisibilityChanged`, `OnColorsChanged`, with `CursorLeft`, `CursorTop`, `ExplicitForeground` and `ExplicitBackground` to read the state from. They run inside the change, with interrupts masked: record what changed and return, without blocking or doing I/O. `OnFlush` and `OnClosed` run unmasked.
- `SessionManager.TryRegister(session)` lists the session, then `SessionManager.Start(session, body)` runs a thread on it.

The kernel's sockets are not safe to use from two threads at once, so the Telnet server reads and writes all its connections on one thread of its own: its hooks append to a buffer that thread sends.

## How it works

Every session keeps its screen as a `KernelConsole` grid on the display's canvas. Only the grid of the active session paints the canvas; the others keep their cells up to date and repaint the whole display when shown. A Telnet session also sends every change to its client as it happens: each character, and the escape sequence for each cursor move and colour change, taken from the grid. The two therefore agree on where every line wraps: the grid wraps as soon as a line is full, and the session ends the line explicitly on the client.

A session's reader blocks on an `InterruptEvent` that is signalled when a key is queued for it. Keys come either from the keyboard's decoder, which queues them on the active session, possibly in interrupt context, or from a remote session's server: the Telnet server's thread looks at its connections every 10 ms while they are idle, and queues the keys a client sent. A switch key wakes a thread that is otherwise blocked. The primary console's reader runs on the scheduler's idle thread, so it waits for the next interrupt instead of blocking, as the keyboard always did.
