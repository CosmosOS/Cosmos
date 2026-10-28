---
source_hash: 7ace168b5c49e9ad05d5761d8de79dc459f89135
---
<a id="mouse"></a>

# 鼠标

本文将介绍 Cosmos Gen3 上的鼠标输入：如何跟踪指针位置、响应按键按下和滚轮滚动，以及如何用图形 API 绘制光标。

如果你从 Gen2 迁移过来，主要区别如下：

| | Gen2 | Gen3 |
|---|---|---|
| 管理器 API | `Cosmos.System.MouseManager` | 相同的模型，位于 `Cosmos.Kernel.System.Input` |
| 按键状态 | `MouseState` 标志枚举 | `LeftButton`、`RightButton`、`MiddleButton` 布尔值 |
| 位置 | `X`、`Y` 被限制在屏幕尺寸内 | 相同 |
| 滚轮 | `ScrollDelta` + `ResetScrollDelta()` | 相同 |
| 光标 | 由你的代码绘制 | 由你的代码绘制 |
| 设备 | PS/2 鼠标 | 带滚轮的 PS/2 鼠标（x64）、virtio-mouse 和 USB 鼠标，全部基于驱动工具包（q35 和所有 PC 上的 8042；virtio 在两种架构上基于 PCI，在 ARM64 上还可基于 MMIO；USB 在两种架构上都基于 xHCI、EHCI 或 OHCI 控制器） |

如果你发现 bug 或任何异常，请在我们的仓库中[提交 issue](https://github.com/CosmosOS/Cosmos/issues/new/choose)。

<a id="enable-the-mouse-in-your-kernel"></a>

## 在内核中启用鼠标

鼠标支持由一个功能开关控制。请确保内核的 `.csproj` 没有将其关闭（默认值为 `true`）：

```xml
<PropertyGroup>
  <CosmosEnableMouse>true</CosmosEnableMouse>
</PropertyGroup>
```

下面的代码片段依赖以下 `using`；绘图类型来自[图形](graphics.md)一文：

```csharp
using System.Drawing;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Fonts;
using Cosmos.Kernel.System.Input;
```

与键盘一样，在驱动阶段，PS/2 鼠标（x64）由驱动工具包的 `Ps2MouseDriver` 绑定，virtio 鼠标由 `VirtioInputDriver` 绑定，USB 鼠标由 `UsbMouseDriver` 绑定，每个鼠标都会发布给管理器的消费者，由后者完成注册（[PS/2 设备](drivers.md#ps2-devices)、[virtio 设备](drivers.md#virtio-devices)、[USB 设备](drivers.md#usb-devices)）；你的内核一开始运行，`MouseManager` 就已就绪。

USB 鼠标还需要 USB 驱动程序，即 `CosmosEnableUsb`。你不必设置它：不设置时，只要 `CosmosEnableMouse` 开启，它就开启。将其设为 `false` 会保留 PS/2 和 virtio 鼠标，并连同主机控制器驱动程序一起去掉 USB 鼠标。

要在 QEMU 中试用 USB 鼠标，请向 `cosmos run` 传入 `--mouse usb-mouse`：启动器会添加一个 `qemu-xhci` 控制器（或复用 USB 磁盘或 `usb-kbd` 键盘已经带来的那个控制器），并把鼠标插到它上面，两种架构都是如此。`--usb-controller ehci`、`ohci` 或 `ehci-ohci` 则会改为放置一个 EHCI、一个 OHCI，或一个带 OHCI 伴随控制器的 EHCI，每个 USB 设备都固定连接到它的某个根端口。

<a id="position-and-buttons"></a>

## 位置与按键

`MouseManager` 是一组静态属性，由中断处理程序保持更新：`X` 和 `Y` 表示指针位置，`LeftButton`、`RightButton` 和 `MiddleButton` 表示按键。在你绘制光标之前，屏幕上不会有光标，因此鼠标代码就是一个每帧读取这些属性的渲染循环：

```csharp
Canvas canvas = Canvas.GetFullScreen();
Font font = PCScreenFont.DefaultFont;

/* Clamp the pointer to the actual screen */
MouseManager.SetScreenSize(canvas.Width, canvas.Height);

while (true)
{
    canvas.Clear(Color.MidnightBlue);

    canvas.DrawString("X: " + MouseManager.X + "   Y: " + MouseManager.Y,
        font, Color.White, 40, 40);
    canvas.DrawString("Buttons: "
        + (MouseManager.LeftButton ? "L" : "-")
        + (MouseManager.MiddleButton ? "M" : "-")
        + (MouseManager.RightButton ? "R" : "-"),
        font, Color.White, 40, 100);

    /* A minimal cursor: a white dot with a black outline */
    canvas.DrawFilledCircle(Color.White, MouseManager.X, MouseManager.Y, 6);
    canvas.DrawCircle(Color.Black, MouseManager.X, MouseManager.Y, 6);

    canvas.Display();
    Thread.Sleep(15);
}
```

<!-- video: 光标圆点跟随鼠标移动，同时位置读数不断更新，每按下一个按键，L、M、R 指示就会亮起 -->
<video src="images/mouse-cursor.mp4" controls autoplay muted loop playsinline style="max-width:100%"></video>

在启动时调用一次 `SetScreenSize`：管理器会把 `X` 和 `Y` 限制在该范围内（默认值为 1024x768，很少与帧缓冲区一致）。如果想要真正的箭头光标，请改为 blit 一张小位图，而不是画圆。

<a id="painting-with-the-mouse"></a>

## 用鼠标绘画

按键只是普通的布尔值，因此一个绘画程序只需几个 `if`。笔画累积在一个离屏画布上（参见[离屏画布](graphics.md#off-screen-canvases)），这样光标就可以绘制在其上方而不会擦除笔画：

```csharp
Canvas screen = Canvas.GetFullScreen();
MouseManager.SetScreenSize(screen.Width, screen.Height);

Canvas paint = new Canvas(screen.Width, screen.Height);
paint.Clear(Color.MidnightBlue);

while (true)
{
    if (MouseManager.LeftButton)
    {
        paint.DrawFilledCircle(Color.Gold, MouseManager.X, MouseManager.Y, 6);
    }

    if (MouseManager.RightButton)
    {
        paint.Clear(Color.MidnightBlue);
    }

    screen.DrawCanvas(paint, 0, 0);
    screen.DrawFilledCircle(Color.White, MouseManager.X, MouseManager.Y, 6);
    screen.DrawCircle(Color.Black, MouseManager.X, MouseManager.Y, 6);
    screen.Display();

    Thread.Sleep(15);
}
```

<!-- video: 按住左键画出金色笔画，右键单击清空画布，然后再次绘画 -->
<video src="images/mouse-paint.mp4" controls autoplay muted loop playsinline style="max-width:100%"></video>

<a id="the-scroll-wheel"></a>

## 滚轮

滚轮的移动累积在 `MouseManager.ScrollDelta` 中，每滚动一格计一个单位。驱动程序从不将其清零：读取它、应用它，然后调用 `ResetScrollDelta()`，以免同一次移动被应用两次。

```csharp
Canvas canvas = Canvas.GetFullScreen();
Font font = PCScreenFont.DefaultFont;

int radius = 60;

while (true)
{
    int delta = MouseManager.ScrollDelta;
    if (delta != 0)
    {
        radius = Math.Clamp(radius + delta * 10, 12, 320);
        MouseManager.ResetScrollDelta();
    }

    canvas.Clear(Color.MidnightBlue);
    canvas.DrawString("Scroll to resize the circle. Radius: " + radius,
        font, Color.White, 40, 40);
    canvas.DrawFilledCircle(Color.DeepSkyBlue,
        canvas.Width / 2, canvas.Height / 2, radius);
    canvas.Display();

    Thread.Sleep(15);
}
```

<!-- video: 随着滚轮向两个方向滚动，圆形不断变大和缩小 -->
<video src="images/mouse-scroll.mp4" controls autoplay muted loop playsinline style="max-width:100%"></video>

<a id="sensitivity-and-pointer-control"></a>

## 灵敏度与指针控制

`MouseManager` 还有两个可调项：

- `Sensitivity` 会乘以每个移动增量（默认值为 `1.0f`）；小于 1 的值让指针变慢，大于 1 的值让它变快。
- `SetPosition(x, y)` 以编程方式移动指针，并将其限制在屏幕范围内，例如在打开一个场景时把指针居中。

<a id="current-limitations"></a>

## 当前限制

- 仅支持相对定位设备。没有绝对（数位板）输入，因此在虚拟机窗口中，客户机指针不会与宿主机光标一一对应地移动。
- 滚轮的水平倾斜会被忽略；只有垂直滚轮的数据会进入 `ScrollDelta`。
- PS/2 鼠标只发布一次，即在驱动阶段，当鼠标驱动程序绑定了 8042 驱动程序的辅助端口节点且其复位得到应答时；启动时就存在的 virtio 鼠标也只发布一次，即在驱动阶段提供其设备时。USB 鼠标在其节点被绑定时发布，在节点被撤回时撤下；其 PCI 功能位于 PCI Express 热插拔插槽之后、由根端口驱动程序发布和撤回的 virtio 鼠标也是如此。因此，可以在内核运行时插入和拔出的正是这些鼠标。
- USB 鼠标以 HID 启动协议驱动，所有 PC 鼠标都支持该协议：三个按键、8 位的 X 和 Y 增量，以及在带滚轮的鼠标上，紧随其后那个字节中的滚轮数据。额外的按键、水平倾斜，以及某些鼠标在其自身报告协议中提供的更高精度，都不会被读取。

<a id="how-it-works"></a>

## 工作原理

在 x64 上，PS/2 鼠标会为移动数据包的每个字节触发 IRQ 12，8042 驱动程序读取该字节并将其交给辅助端口的 `Ps2Access`。鼠标驱动程序的中断处理程序负责组装由按键和 X/Y 增量构成的 3 个字节；一旦探测启用了 IntelliMouse 协议（采样率序列 200、100、80），再加上滚轮字节扩展为 4 个字节。它会丢弃不可能作为数据包开头的字节，并为每个数据包通过指针接收器向管理器的消费者交付一份报告。virtio 鼠标通过 PCI 或 MMIO，以其队列中事件记录的形式提供同样的信息：驱动工具包的 `VirtioInputDriver` 在工具包的工作线程上取出这些记录（由队列的中断唤醒，如果无法路由任何中断，则每 20 ms 唤醒一次），把轴事件和按键事件合并为每个同步事件一份报告，并通过指针接收器将其交给管理器的消费者。USB 鼠标的启动报告到达其中断 IN 管道，由主机控制器驱动程序的中断送达（在 xHCI 上是 MSI-X 消息，在 EHCI 和 OHCI 上是该功能的传统中断线），如果无法路由任何中断，则由其每 20 ms 一次的轮询送达；`UsbMouseDriver` 的处理程序把每份报告转换为一次相对移动（HID 与管理器一样让 Y 轴朝下，其滚轮值则取反，使正值表示向下滚动，与 PS/2 一致），并通过同一个接收器交出。无论哪种方式，增量最终都会到达 `MouseManager`，它会应用 `Sensitivity`，把增量加到 `X` 和 `Y` 上，将结果限制在屏幕尺寸内，并更新按键布尔值；滚轮增量累积在 `ScrollDelta` 中，直到轮询方将其取走。你的渲染循环只会读取状态，因此不需要加锁。

```
Render loop (reads X/Y, buttons, ScrollDelta)
        │
MouseManager ── Sensitivity, screen clamping, ScrollDelta accumulation
        │
PS/2 mouse over the driver kit (the 8042, IRQ 12, x64)  /  virtio-mouse over the driver kit (PCI, MMIO)  /  USB mouse over the driver kit (xHCI, EHCI, OHCI)
```
