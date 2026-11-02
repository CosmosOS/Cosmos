---
source_hash: 2cbf6ad4186d6d05f52343bb41f9eb69a7b3bdbe
---
<a id="keyboard"></a>

# 键盘

本文将介绍 Cosmos Gen3 上的键盘输入：如何通过标准的 `Console` API 读取文本，如何响应单个按键，以及如何切换键盘布局。

如果你从 Gen2 迁移过来，主要区别如下：

| | Gen2 | Gen3 |
|---|---|---|
| 管理器 API | `Cosmos.System.KeyboardManager` | 相同的 API，位于 `Cosmos.Kernel.System.Input`，其中 `SetKeyLayout`/`GetKeyLayout` 更名为 `SetLayout`/`GetLayout` |
| `Console.ReadLine` / `Console.ReadKey` | 通过 plug 实现，由管理器提供支持 | 通过 plug 实现，由管理器提供支持 |
| 按键事件 | `KeyEvent`（`KeyChar`、`Key`、`Modifiers`），按键类型为 `ConsoleKeyEx` | 相同，按键类型为 `Key`，成员也相同 |
| 布局 | 基于 `ScanMapBase` 的 US、FR、DE、ES、GB、TR、Dvorak 布局 | 同一组布局，基于 `KeyboardLayout`，位于 `Cosmos.Kernel.System.Input.Layouts` |
| 设备 | PS/2 键盘 | PS/2 键盘（x64）、virtio-keyboard 和 USB 键盘，全部基于驱动工具包（q35 和所有 PC 上的 8042；两种架构上均为 PCI 上的 virtio，ARM64 上还有 MMIO 上的 virtio；两种架构上均为 xHCI、EHCI 或 OHCI 控制器上的 USB） |

如果你发现 bug 或任何异常，请在我们的仓库中[提交 issue](https://github.com/CosmosOS/Cosmos/issues/new/choose)。

<a id="enable-the-keyboard-in-your-kernel"></a>

## 在内核中启用键盘

键盘支持由一个功能开关控制。请确保内核的 `.csproj` 没有关闭它（默认值为 `true`）：

```xml
<PropertyGroup>
  <CosmosEnableKeyboard>true</CosmosEnableKeyboard>
</PropertyGroup>
```

下面的代码片段依赖以下 `using` 指令：

```csharp
using System.Drawing;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Fonts;
using Cosmos.Kernel.System.Input;
using Cosmos.Kernel.System.Input.Layouts;
```

无需手动初始化任何东西：PS/2 键盘（x64）、virtio 键盘或 USB 键盘会在驱动阶段由驱动工具包的 `Ps2KeyboardDriver`、`VirtioInputDriver` 或 `UsbKeyboardDriver` 绑定，并发布给管理器的消费者，由后者完成注册（[PS/2 设备](drivers.md#ps2-devices)、[virtio 设备](drivers.md#virtio-devices)、[USB 设备](drivers.md#usb-devices)）。

USB 键盘还需要 USB 驱动程序，即 `CosmosEnableUsb`。你不必设置它：不设置时，只要 `CosmosEnableKeyboard` 开启，它就会开启。把它设为 `false` 会保留 PS/2 和 virtio 键盘，并连同主机控制器驱动程序一起去掉 USB 键盘。

<a id="reading-a-line"></a>

## 读取一行

`Console.ReadLine()` 的行为与在 .NET 中其他任何地方一样，因此大多数内核都不需要更多功能：

```csharp
Console.WriteLine("What is your name?");
Console.Write("> ");

string? name = Console.ReadLine();
Console.WriteLine("Hello, " + name + "!");
```

行编辑器支持的不只是输入：左右方向键在输入内容中移动光标，Home 和 End 跳到行首或行尾，Backspace 和 Delete 删除字符，在行中间输入会插入字符而不是覆盖。

<!-- video: 输入“Comsos Gen3”，然后用 Home、方向键和 Delete 修正拼写错误，再按下 Enter -->
<video src="images/keyboard-readline.mp4" controls autoplay muted loop playsinline style="max-width:100%"></video>

<a id="reading-single-keys"></a>

## 读取单个按键

对于比整行更底层的操作，`KeyboardManager.ReadKey()` 会一直阻塞到有键按下，并返回一个 `KeyEvent`：

```csharp
Console.WriteLine("Press keys to inspect them (Escape to leave):");

while (true)
{
    KeyEvent key = KeyboardManager.ReadKey();
    if (key.Key == Key.Escape)
    {
        break;
    }

    Console.WriteLine("Key: " + key.Key + "   Char: '" + key.KeyChar
        + "'   Modifiers: " + key.Modifiers);
}
```

一个 `KeyEvent` 包含三项内容：

| 属性 | 类型 | 含义 |
|---|---|---|
| `Key` | `Key` | 物理按键，与布局和修饰键无关（`A`、`D5`、`F1`、`LeftArrow` 等） |
| `KeyChar` | `char` | 按键产生的文本字符；如果按键不产生字符（功能键、方向键、Ctrl 组合键），则为 `'\0'` |
| `Modifiers` | `ConsoleModifiers` | 按下按键时处于激活状态的 Shift、Alt 和 Control 标志 |

<!-- video: 检查循环为字母、Shift+B、Shift+5、Ctrl+A、Alt+X、F1、F12、LeftArrow 和 Space 打印 Key/Char/Modifiers -->
<video src="images/keyboard-readkey.mp4" controls autoplay muted loop playsinline style="max-width:100%"></video>

标准的 `Console.ReadKey()` 同样可用，并返回普通的 `ConsoleKeyInfo`；`ReadKey(true)` 重载会禁止回显。它只是 `KeyboardManager.ReadKey()` 的一层薄封装，因此哪个更适合你的代码就用哪个。

一旦内核打开第二个控制台会话，键盘就会向显示设备上的会话输入，而 `Console` 和 `KeyboardManager` 都会读取调用线程所在会话的按键（[控制台会话与 Telnet](sessions.md)）。

<a id="polling-without-blocking"></a>

## 非阻塞轮询

渲染循环不能阻塞在 `ReadKey()` 上。`KeyboardManager.TryReadKey()` 会从队列中取出一个待处理的按键，没有按键时返回 `false`，这样这一帧可以照常继续。下面用它在[图形](graphics.md)一文的画布上移动一个方块：

```csharp
Canvas canvas = Canvas.GetFullScreen();
Font font = PCScreenFont.DefaultFont;

int x = (canvas.Width - 60) / 2;
int y = (canvas.Height - 60) / 2;
const int Step = 20;

bool running = true;
while (running)
{
    /* Drain every key pressed since the last frame */
    while (KeyboardManager.TryReadKey(out KeyEvent? key))
    {
        switch (key.Key)
        {
            case Key.UpArrow: y -= Step; break;
            case Key.DownArrow: y += Step; break;
            case Key.LeftArrow: x -= Step; break;
            case Key.RightArrow: x += Step; break;
            case Key.Escape: running = false; break;
        }
    }

    canvas.Clear(Color.MidnightBlue);
    canvas.DrawString("Move the square with the arrow keys", font, Color.White, 40, 40);
    canvas.DrawFilledRectangle(Color.Gold, x, y, 60, 60);
    canvas.Display();

    Thread.Sleep(15);
}
```

<!-- video: 金色方块在方向键的控制下在画布上移动 -->
<video src="images/keyboard-square.mp4" controls autoplay muted loop playsinline style="max-width:100%"></video>

`KeyboardManager.KeyAvailable`（或通过 plug 实现的 `Console.KeyAvailable`）告诉你是否有按键在等待，但不会消费它；`KeyboardManager.Peek()` 返回下一个 `KeyEvent`，同时把它留在队列中。

<a id="modifiers-and-lock-keys"></a>

## 修饰键与锁定键

`KeyboardManager` 在全局范围内跟踪修饰键和锁定键的状态，独立于任何按键事件：

| 属性 | 含义 |
|---|---|
| `ShiftPressed` | 当前按住了一个 Shift 键 |
| `ControlPressed` | 当前按住了一个 Control 键 |
| `AltPressed` | 当前按住了一个 Alt 键 |
| `CapsLock` | Caps Lock 已开启 |
| `NumLock` | Num Lock 已开启 |
| `ScrollLock` | Scroll Lock 已开启 |

锁定键每按一次就切换一次状态，并更新键盘指示灯（virtio 键盘的指示灯保持不变：驱动程序不驱动设备的状态队列）。按住的修饰键也会通过每个 `KeyEvent` 的 `Modifiers` 标志传递，这通常是更方便的形式。

AltGr 是右 Alt 键，用于那些为按键提供第三层字符的布局（德语、西班牙语、土耳其语）。按住它时，按键会通过布局的 Control+Alt 列进行转换，`KeyEvent` 的 `Modifiers` 中会同时带有 `Control` 和 `Alt`，这与 Windows 报告同一按键的方式一致。`ControlPressed` 和 `AltPressed` 仍然反映物理 Control 和 Alt 键的状态。在其他布局上，右 Alt 只是第二个 Alt 键。

<a id="keyboard-layouts"></a>

## 键盘布局

按键以与布局无关的扫描码形式从硬件发出；布局把它们转换为字符。默认布局是 US QWERTY，`SetLayout` 可以随时切换布局：

```csharp
KeyboardManager.SetLayout(new FRStandardLayout());
```

`Cosmos.Kernel.System.Input.Layouts` 中提供了七种布局：

| 类 | 布局 |
|---|---|
| `USStandardLayout` | 美式 QWERTY（默认） |
| `FRStandardLayout` | 法语 AZERTY |
| `DEStandardLayout` | 德语 QWERTZ |
| `ESStandardLayout` | 西班牙语 QWERTY |
| `GBStandardLayout` | 英式 QWERTY |
| `TRStandardLayout` | 土耳其语 Q |
| `USDvorakLayout` | 美式 Dvorak |

切换会立即生效：下面把同样的六个物理按键输入两次，第一次使用美式布局，第二次使用法语布局。

<!-- video: 在美式布局下输入 Tab 右侧的六个键（“qwerty”），切换到 FRStandardLayout，再输入一次（“azerty”） -->
<video src="images/keyboard-layouts.mp4" controls autoplay muted loop playsinline style="max-width:100%"></video>

`KeyboardManager.GetLayout()` 返回当前布局；自定义布局是一个派生自 `KeyboardLayout` 的类，它重写 `InitializeKeys()`，用 `KeyMapping` 项填充 `Keys` 列表。该方法只运行一次，即在扫描码第一次到达该布局时。

扫描码是第 1 套扫描码中的按下码（make code），并去掉了扩展键的 `E0` 前缀，内置布局正是这样列出 Windows 键和导航键区的。唯一的例外是右 Alt，它去掉前缀后的代码与左 Alt 相同：驱动程序把它报告为 `0x60`，这是第 1 套中未使用的代码。布局把这个代码映射到 `Key.AltGr`，可以让该键成为它的第三层修饰键；映射到 `Key.RAlt`，则让它保持为普通的 Alt。两者都不映射的布局会失去这个键。按键的第三层是 `KeyMapping` 的 `ctrlAlt` 参数，`ctrlAltShift` 则是第四层：

```csharp
/* Right Alt selects the third level on this layout */
Keys.Add(new KeyMapping(0x60, Key.AltGr));
/* Q: q, Q, and @ under AltGr */
Keys.Add(new KeyMapping(0x10, 'q', 'Q', 'q', 'Q', 'q', 'Q', '@', Key.Q));
```

<a id="current-limitations"></a>

## 当前限制

- 按键释放不会进入队列：`KeyEvent.Type` 有一个 `Break` 值，但只有按下事件会到达缓冲区。Shift、Ctrl 和 Alt 的释放会更新修饰键状态，除此之外都会被丢弃。
- 德语、西班牙语和土耳其语布局带有各自的 AltGr 字符；法语和英式布局还没有（AZERTY 上的 `@`、`#`、`{`），因此它们的右 Alt 仍是普通的 Alt。死键不会组合：土耳其语的 `¨`、`~`、`´` 和 `` ` `` 会按原样输出。
- PS/2 键盘只在驱动阶段发布一次，条件是键盘驱动程序已绑定 8042 驱动程序的键盘端口节点，并且其复位得到了应答；启动时就存在的 virtio 键盘也只发布一次，即在驱动阶段提供其设备时。USB 键盘在其节点被绑定时发布，在节点被撤回时撤下；PCI 功能位于 PCI Express 热插拔插槽后面的 virtio 键盘也是如此，这类插槽由根端口驱动程序发布和撤回。因此，可以在内核运行时插入和拔出的正是这些键盘；在被拔出的键盘上按住的修饰键会一直保持按下状态，直到在另一块键盘上按下它。

<a id="how-it-works"></a>

## 工作原理

在 x64 上，PS/2 键盘的每次按键都会触发 IRQ 1；8042 驱动程序的中断处理程序从控制器的数据端口读取字节，并把它交给键盘端口的 `Ps2Access`，后者的中断源会唤醒键盘驱动程序的处理程序：该处理程序把 0xE0 前缀和释放位合并为第 1 套扫描码（右 Alt 为 0x60），并通过键盘接收器报告给管理器的消费者。virtio 键盘则是在其队列上投递一条事件记录：队列的中断（PCI 上为 MSI-X 消息，MMIO 上为插槽的 GIC 中断线）会在驱动工具包的工作线程上调度驱动程序的排空操作，如果无法路由任何中断，排空操作就每 20 ms 运行一次；排空操作把 Linux 键码转换为同样的第 1 套扫描码，并通过键盘接收器报告给管理器的消费者，后者像 PS/2 键盘的消费者一样为 `KeyboardManager` 提供输入。USB 键盘的 8 字节引导协议报告到达其中断 IN 管道，由主机控制器驱动程序的中断投递（xHCI 上为 MSI-X 消息，EHCI 和 OHCI 上为该功能的传统中断线），在无法路由任何中断时则由其 20 ms 轮询投递；`UsbKeyboardDriver` 的处理程序把每份报告与上一份进行比较，得出同样的第 1 套扫描码，并通过同一个接收器报告；锁定键则通过 `SetLeds` 点亮它的指示灯，即在工具包工作线程上执行一次 SET_REPORT 控制传输。锁定键同样会通过 `SetLeds`（0xED 加指示灯字节）在工具包工作线程上点亮 PS/2 键盘的指示灯。管理器把锁定键和修饰键交给状态属性，其余按键则通过当前布局转换为 `KeyEvent`，放入按键缓冲区。`ReadKey()` 会让 CPU 暂停（halt），直到中断投递下一个事件；`TryReadKey()` 只是从队列中取出。`Console.ReadLine` 和 `Console.ReadKey` 是建立在同一队列之上的 plug，因此控制台输入和原始按键事件永远不会冲突。

```
Console.ReadLine / Console.ReadKey        (plugs, Cosmos.Kernel.Plugs)
        │
KeyboardManager ── KeyEvent queue ◀── active layout
        │                                    ▲
        │                              raw scan codes
        │                                    │
PS/2 keyboard over the driver kit (the 8042, IRQ 1, x64)  /  virtio-keyboard over the driver kit (PCI, MMIO)  /  USB keyboard over the driver kit (xHCI, EHCI, OHCI)
```

<a id="summary"></a>

## 总结

| 任务 | 调用 |
|---|---|
| 读取一行 | `Console.ReadLine()` |
| 等待按键 | `KeyboardManager.ReadKey()` |
| 查看按下的键及其产生的字符 | `KeyEvent.Key`、`KeyEvent.KeyChar`、`KeyEvent.Modifiers` |
| 通过控制台读取按键，带或不带回显 | `Console.ReadKey()`、`Console.ReadKey(true)` |
| 非阻塞轮询按键 | `KeyboardManager.TryReadKey(out key)` |
| 检查是否有按键在等待 | `KeyboardManager.KeyAvailable`、`Console.KeyAvailable` |
| 查看下一个按键但不取出 | `KeyboardManager.Peek()` |
| 检查是否按住了 Shift、Control 或 Alt | `KeyboardManager.ShiftPressed`、`ControlPressed`、`AltPressed` |
| 读取锁定键状态 | `KeyboardManager.CapsLock`、`NumLock`、`ScrollLock` |
| 切换布局 | `KeyboardManager.SetLayout(layout)` |
| 读取当前布局 | `KeyboardManager.GetLayout()` |
| 编写自定义布局 | `KeyboardLayout`、`InitializeKeys()`、`KeyMapping` |
| 让右 Alt 成为 AltGr | `Keys.Add(new KeyMapping(0x60, Key.AltGr))` |
