# Mouse

In this article, we will discuss mouse input on Cosmos Gen3: how to track the pointer position, react to button presses and the scroll wheel, and draw a cursor with the graphics API.

The main differences if you come from Gen2:

| | Gen2 | Gen3 |
|---|---|---|
| Manager API | `Cosmos.System.MouseManager` | Same model, in `Cosmos.Kernel.System.Mouse` |
| Button state | `MouseState` flags enum | `LeftButton`, `RightButton`, `MiddleButton` booleans |
| Position | `X`, `Y` clamped to the screen size | Same |
| Scroll wheel | `ScrollDelta` + `ResetScrollDelta()` | Same |
| Cursor | Drawn by your code | Drawn by your code |
| Devices | PS/2 mouse | PS/2 mouse with scroll wheel (x64), virtio-mouse (x64 PCI and ARM64 MMIO) |

If you find bugs or something abnormal, please [submit an issue](https://github.com/CosmosOS/Cosmos/issues/new/choose) on our repository.

## Enable the mouse in your kernel

Mouse support is behind a feature switch. Make sure your kernel's `.csproj` does not turn it off (it defaults to `true`):

```xml
<PropertyGroup>
  <CosmosEnableMouse>true</CosmosEnableMouse>
</PropertyGroup>
```

These are the `using`s the snippets below rely on; the drawing types come from the [Graphics](graphics.md) article:

```csharp
using System.Drawing;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Fonts;
using Cosmos.Kernel.System.Mouse;
```

Like the keyboard, the mouse is detected and registered at boot; `MouseManager` is ready as soon as your kernel runs.

## Position and buttons

`MouseManager` is a set of static properties that the interrupt handler keeps current: `X` and `Y` for the pointer position, `LeftButton`, `RightButton` and `MiddleButton` for the buttons. There is no cursor on screen until you draw one, so mouse code is a render loop that reads the properties each frame:

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

<!-- video: the cursor dot following the mouse while the position readout updates and the L, M and R indicators light up as each button is pressed -->
<video src="images/mouse-cursor.mp4" controls autoplay muted loop playsinline style="max-width:100%"></video>

Call `SetScreenSize` once at startup: the manager clamps `X` and `Y` to those bounds (the default is 1024x768, which rarely matches the framebuffer). For a real arrow cursor, blit a small bitmap instead of the circle; the DevKernel has a ready-made one in [MouseCursor.cs](https://github.com/CosmosOS/Cosmos/blob/gen3/examples/DevKernel/Graphics/MouseCursor.cs).

## Painting with the mouse

Buttons are plain booleans, so a paint program is a couple of `if`s. The strokes accumulate on an off-screen canvas (see [Off-screen canvases](graphics.md#off-screen-canvases)) so the cursor can be drawn on top without erasing them:

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

<!-- video: painting gold strokes while holding the left button, clearing the canvas with a right click, then painting again -->
<video src="images/mouse-paint.mp4" controls autoplay muted loop playsinline style="max-width:100%"></video>

## The scroll wheel

Wheel movement accumulates in `MouseManager.ScrollDelta`, one unit per click. The driver never clears it: read it, apply it, then call `ResetScrollDelta()` so the same movement is not applied twice.

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

<!-- video: the circle growing and shrinking as the wheel scrolls in both directions -->
<video src="images/mouse-scroll.mp4" controls autoplay muted loop playsinline style="max-width:100%"></video>

## Sensitivity and pointer control

Two more knobs on `MouseManager`:

- `Sensitivity` multiplies every movement delta (default `1.0f`); values below 1 slow the pointer down, values above speed it up.
- `SetPosition(x, y)` moves the pointer programmatically, clamped to the screen bounds, for example to center it when a scene opens.

## Current limitations

- Only relative pointing devices are supported. There is no absolute (tablet) input, so inside a VM window the guest pointer does not track the host cursor one to one.
- Horizontal wheel tilt is ignored; only the vertical wheel reaches `ScrollDelta`.
- Devices are detected once at boot; there is no mouse hotplug.

## How it works

On x64 the PS/2 mouse raises IRQ12 for every byte of a movement packet: 3 bytes of buttons and X/Y deltas, extended to 4 by the scroll wheel byte once the driver has enabled the IntelliMouse protocol (the magic sample-rate sequence 200, 100, 80 at boot). On ARM64 (and over PCI on x64) the virtio-input device delivers the same information as event records. Either way the driver hands the deltas to `MouseManager`, which applies `Sensitivity`, adds them to `X` and `Y`, clamps to the screen size and updates the button booleans; wheel deltas accumulate in `ScrollDelta` until a poller consumes them. Your render loop only ever reads state, which is why no locking is needed.

```
Render loop (reads X/Y, buttons, ScrollDelta)
        │
MouseManager ── Sensitivity, screen clamping, ScrollDelta accumulation
        │
PS/2 mouse, IRQ12 (x64)  /  virtio-mouse (x64 PCI, ARM64 MMIO)
```
