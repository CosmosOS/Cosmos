---
source_hash: ab8aa3258dbf81e84a5cfb2ad016a23e6e500cf4
---
<a id="console-sessions-and-telnet"></a>

# 控制台会话与 Telnet

本文将介绍 Cosmos Gen3 上的控制台会话：在本地显示设备上并排运行多个 shell，像 Linux 切换 tty 那样在它们之间切换，以及通过 Telnet 向远程终端提供 shell。

控制台会话是 `System.Console` 可以绑定的一个控制台：一块屏幕，以及在它上面敲下的按键。`Console` 跟随调用线程。在某个会话上启动的线程读取该会话的按键并写入它的屏幕，因此同样的 `Console.ReadLine()` 和 `Console.WriteLine()` 代码无需修改就能在每个会话上运行，无论是本地会话还是远程会话。

| 种类 | 按键来源 | 屏幕显示于 | 创建者 |
|---|---|---|---|
| 主控制台，`tty1` | 本地键盘 | 显示设备 | 内核，基于 `KernelConsole.Default` |
| 虚拟控制台，`tty2` 及之后 | 本地键盘 | 显示设备，当它是活动会话时 | `SessionManager.CreateVirtualConsole()` |
| Telnet 会话 | Telnet 客户端 | 客户端的终端；当它是活动会话时也显示在显示设备上 | [Cosmos.Network.Telnet](https://github.com/CosmosOS/Cosmos.Network.Telnet) 包中的 `TelnetServer`，每个连接一个 |

下面的代码片段依赖 `using Cosmos.Kernel.System.Sessions;`。

会话需要调度器（`CosmosEnableScheduler`），因为除主控制台之外的每个会话都运行在自己的线程上。它们还需要内核控制台，因为每块屏幕都是显示设备画布上的一个网格。Telnet 服务器还需要网络（`CosmosEnableNetwork`）。

<a id="the-primary-console"></a>

## 主控制台

`tty1` 是每个内核本来就有的控制台。内核的主循环（`Run()`）运行在它上面，所有没有在其他会话上启动的线程也是如此，而且它不能被关闭。从不打开会话的内核，行为与以前完全相同：`Console` 和 `KeyboardManager` 直接读取键盘。

<a id="virtual-consoles"></a>

## 虚拟控制台

`SessionManager.CreateVirtualConsole()` 打开一个与主控制台大小相同的本地控制台。它创建时处于隐藏状态，`SessionManager.Start` 会在它上面启动一个线程：

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

该线程的 `Console` 就是这个会话。函数体返回时，会话随之关闭：虚拟控制台消失，远程会话挂断。从函数体中逃逸的异常会被记录到串口，并以同样的方式结束会话。

函数体启动的线程不会绑定到该会话：它们的 `Console` 是主控制台。如果它们需要写入该会话，请用 `SessionManager.Start` 绑定它们。

<a id="switching-sessions"></a>

## 切换会话

会话编号为 1 到 12（`SessionManager.MaxSessions`），编号同时也对应显示该会话的功能键：

| 按键 | 显示 |
|---|---|
| Alt+F1 … Alt+F12 | 会话 1 … 12 |
| Alt+左方向键、Alt+右方向键 | 上一个或下一个已打开的会话 |

显示设备上的会话是 `SessionManager.Active`。本地键盘的输入会进入这个会话，无论它是本地会话还是远程会话，因此你可以在机器本身上接管一个 Telnet 会话。在代码中：

```csharp
foreach (ConsoleSession session in SessionManager.GetSessions())
{
    Console.WriteLine(session.Id + "  " + session.Name + (session.IsActive ? "  (on the display)" : ""));
}

SessionManager.Switch(2);   // show tty2
```

一旦存在第二个会话，键盘输入就会被路由：每个按键直接送往显示设备上的会话。从此以后，`KeyboardManager` 的读取成员（`KeyAvailable`、`TryReadKey`、`ReadKey`、`Peek`）也会像 `Console` 一样读取调用线程所在的会话。

QEMU 在窗口中运行时，宿主机桌面可能会在客户机收到之前截获 Alt 加功能键的组合；尤其是 Alt+F4 可能会关闭窗口。使用 Alt 加方向键，或在 shell 命令中调用 `SessionManager.Switch`，可以避免这个问题。

<a id="serving-a-shell-over-telnet"></a>

## 通过 Telnet 提供 shell

Telnet 服务器是一个独立的包：[Cosmos.Network.Telnet](https://github.com/CosmosOS/Cosmos.Network.Telnet)。将它添加到你的内核中：

```xml
<ItemGroup>
    <PackageReference Include="Cosmos.Network.Telnet" Version="1.0.0" />
</ItemGroup>
```

`TelnetServer` 监听一个 TCP 端口，为每个客户端提供一个独立的会话，以及一个运行你传入的 shell 的线程：

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

`Start()` 会立即返回；服务器在自己的线程上接受连接，直到 `Stop()` 为止，而停止时已经打开的会话会保持连接。设置 `Log` 即可在每次连接、断开和失败时收到一行消息，例如 `Log = message => Cosmos.Kernel.System.Diagnostics.Log.WriteString(message + "\n")`。网络必须已经配置好（通过 DHCP 或手动配置，见[网络](network.md#configure-ipv4)），客户端才能访问服务器。

服务器按照终端所期望的方式进行协商：

- 它回显输入的内容并抑制 go-ahead，从而让客户端进入逐字符模式，因此行编辑器在远程也能正常工作：方向键、Home 和 End、Backspace 和 Delete。
- 它请求窗口大小（NAWS），因此会话的 `Console.WindowWidth` 和 `WindowHeight` 就是客户端的窗口大小，并会在窗口调整大小时随之变化。
- 输出以 VT100 序列发送：颜色、光标移动和 `Console.Clear()` 在客户端上的显示效果与在显示设备上一致。

在 QEMU 用户模式网络下，将宿主机的一个端口转发到客户机的 23 端口，然后从宿主机连接：

```
$ cosmos run --nic e1000e --hostfwd tcp::2323-:23
$ telnet localhost 2323
```

直接使用 QEMU 时，端口转发是用户模式网卡的一个选项：`-nic user,model=e1000e,hostfwd=tcp::2323-:23`。

> [!WARNING]
> Telnet 以明文发送所有内容，而且服务器不要求密码：任何能访问该端口的人都能获得一个 shell。请只在你信任的网络上提供该服务，例如 QEMU 的私有用户模式网络。

<a id="current-limitations"></a>

## 当前限制

- 没有登录机制：Telnet 客户端一连接就能获得 shell。
- 目前还没有 SSH。Telnet 是基于同一套会话的第一种传输方式；SSH 需要密钥交换、加密算法和随机源，而内核目前还没有这些。
- TCP 协议栈不会重传。在 QEMU 用户模式网络上不会丢包，但在真实网络上，丢失一个报文段就会让会话停滞。
- 会话的屏幕位于显示设备的画布上，因此没有显示设备的内核就没有会话，远程会话也不例外。
- Ctrl+C 只是作为一个按键送达会话；没有任何机制能中断正在运行的命令。

<a id="remote-sessions-in-a-library"></a>

## 在库中实现远程会话

用于另一种远程终端的服务器，是一个派生自 `ConsoleSession` 的类，Telnet 服务器的会话类就是这样实现的：

- 构造函数 `ConsoleSession(int cols, int rows)` 为会话提供屏幕，在被显示之前处于隐藏状态。重写 `Name`（会话在列表中显示的名称）和 `IsRemote`（返回 true）。
- `EnqueueInput(KeyEvent)` 将终端发送的按键加入队列，并唤醒会话的读取者；可以从任何线程调用它。`Resize(cols, rows)` 跟随终端窗口的大小变化。
- `On` 钩子报告每一个需要发送给终端的变化：`OnWritten` 对应每个字符，还有 `OnCleared`、`OnCursorMoved`、`OnCursorVisibilityChanged`、`OnColorsChanged`，可通过 `CursorLeft`、`CursorTop`、`ExplicitForeground` 和 `ExplicitBackground` 读取状态。它们在变化过程中运行，此时中断被屏蔽：记录发生了什么变化后立即返回，不要阻塞，也不要进行 I/O。`OnFlush` 和 `OnClosed` 在中断未屏蔽的情况下运行。
- `SessionManager.TryRegister(session)` 将会话加入列表，然后 `SessionManager.Start(session, body)` 在它上面运行一个线程。

内核的套接字不能安全地被两个线程同时使用，因此 Telnet 服务器在自己独有的一个线程上读写所有连接：它的钩子把数据追加到一个缓冲区，由该线程负责发送。

<a id="how-it-works"></a>

## 工作原理

每个会话都把自己的屏幕保存为显示设备画布上的一个 `KernelConsole` 网格。只有活动会话的网格会绘制到画布上；其他会话保持各自单元格的最新状态，并在被显示时重绘整个显示设备。Telnet 会话还会在每次变化发生时就把它发送给客户端：每个字符，以及每次光标移动和颜色变化对应的转义序列，这些都取自网格。因此两者对每一行在何处换行的判断是一致的：网格在一行写满时立即换行，而会话会在客户端上显式结束该行。

会话的读取者阻塞在一个 `InterruptEvent` 上，当有按键为该会话入队时，这个事件就会被触发。按键要么来自键盘解码器（它把按键加入活动会话的队列，可能是在中断上下文中），要么来自远程会话的服务器：Telnet 服务器的线程在连接空闲时每 10 ms 检查一次这些连接，并把客户端发送的按键加入队列。切换键会唤醒一个原本处于阻塞状态的线程。主控制台的读取者运行在调度器的空闲线程上，因此它等待下一次中断而不是阻塞，键盘一直以来都是这样做的。
