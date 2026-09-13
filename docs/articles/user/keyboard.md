# Keyboard

In this article, we will discuss keyboard input on Cosmos Gen3: how to read text through the standard `Console` API, how to react to individual key presses, and how to switch keyboard layouts.

The main differences if you come from Gen2:

| | Gen2 | Gen3 |
|---|---|---|
| Manager API | `Cosmos.System.KeyboardManager` | Same API, in `Cosmos.Kernel.System.Keyboard` |
| `Console.ReadLine` / `Console.ReadKey` | Plugged, backed by the manager | Plugged, backed by the manager |
| Key events | `KeyEvent` (`KeyChar`, `Key`, `Modifiers`) | Same |
| Layouts | US, FR, DE, ES, GB, TR, Dvorak scan maps | Same set, in `Cosmos.Kernel.System.Keyboard.ScanMaps` |
| Devices | PS/2 keyboard | PS/2 keyboard (x64), virtio-keyboard (x64 PCI and ARM64 MMIO) |

If you find bugs or something abnormal, please [submit an issue](https://github.com/valentinbreiz/nativeaot-patcher/issues/new) on our repository.

## Enable the keyboard in your kernel

Keyboard support is behind a feature switch. Make sure your kernel's `.csproj` does not turn it off (it defaults to `true`):

```xml
<PropertyGroup>
  <CosmosEnableKeyboard>true</CosmosEnableKeyboard>
</PropertyGroup>
```

These are the `using`s the snippets below rely on:

```csharp
using System.Drawing;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Fonts;
using Cosmos.Kernel.System.Keyboard;
using Cosmos.Kernel.System.Keyboard.ScanMaps;
```

There is nothing to initialize by hand: at boot the kernel probes the PS/2 controller and the virtio bus, and registers every keyboard it finds with `KeyboardManager`.

## Reading a line

`Console.ReadLine()` works the way it does everywhere else in .NET, so most kernels never need anything more:

```csharp
Console.WriteLine("What is your name?");
Console.Write("> ");

string? name = Console.ReadLine();
Console.WriteLine("Hello, " + name + "!");
```

The line editor supports more than typing: Left and Right arrows move the cursor inside the input, Home and End jump to either end, Backspace and Delete remove characters, and typing in the middle of the line inserts rather than overwrites.

<!-- video: typing "Comsos Gen3", then fixing the typo with Home, arrows and Delete before pressing Enter -->
<video src="images/keyboard-readline.mp4" controls autoplay muted loop playsinline style="max-width:100%"></video>

## Reading single keys

For anything below the line level, `KeyboardManager.ReadKey()` blocks until a key is pressed and returns a `KeyEvent`:

```csharp
Console.WriteLine("Press keys to inspect them (Escape to leave):");

while (true)
{
    KeyEvent key = KeyboardManager.ReadKey();
    if (key.Key == ConsoleKeyEx.Escape)
    {
        break;
    }

    Console.WriteLine("Key: " + key.Key + "   Char: '" + key.KeyChar
        + "'   Modifiers: " + key.Modifiers);
}
```

A `KeyEvent` carries three things:

| Property | Type | Meaning |
|---|---|---|
| `Key` | `ConsoleKeyEx` | The physical key, independent of layout and modifiers (`A`, `D5`, `F1`, `LeftArrow`, ...) |
| `KeyChar` | `char` | The text character the key produces, `'\0'` if it produces none (function keys, arrows, Ctrl combinations) |
| `Modifiers` | `ConsoleModifiers` | The Shift, Alt and Control flags active at the time of the press |

<!-- video: the inspector loop printing Key/Char/Modifiers for letters, Shift+B, Shift+5, Ctrl+A, Alt+X, F1, F12, LeftArrow and Space -->
<video src="images/keyboard-readkey.mp4" controls autoplay muted loop playsinline style="max-width:100%"></video>

The standard `Console.ReadKey()` also works and returns a regular `ConsoleKeyInfo`; the `ReadKey(true)` overload suppresses the echo. It is a thin wrapper over `KeyboardManager.ReadKey()`, so use whichever fits your code.

## Polling without blocking

A render loop cannot afford to block on `ReadKey()`. `KeyboardManager.TryReadKey()` dequeues a pending key press and returns `false` when there is none, so the frame goes on. Here it drives a square over a canvas from the [Graphics](graphics.md) article:

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
            case ConsoleKeyEx.UpArrow: y -= Step; break;
            case ConsoleKeyEx.DownArrow: y += Step; break;
            case ConsoleKeyEx.LeftArrow: x -= Step; break;
            case ConsoleKeyEx.RightArrow: x += Step; break;
            case ConsoleKeyEx.Escape: running = false; break;
        }
    }

    canvas.Clear(Color.MidnightBlue);
    canvas.DrawString("Move the square with the arrow keys", font, Color.White, 40, 40);
    canvas.DrawFilledRectangle(Color.Gold, x, y, 60, 60);
    canvas.Display();

    Thread.Sleep(15);
}
```

<!-- video: the gold square moving around the canvas under the arrow keys -->
<video src="images/keyboard-square.mp4" controls autoplay muted loop playsinline style="max-width:100%"></video>

`KeyboardManager.KeyAvailable` (or the plugged `Console.KeyAvailable`) tells you whether a key press is waiting without consuming it, and `KeyboardManager.Peek()` returns the next `KeyEvent` while leaving it in the queue.

## Modifiers and lock keys

`KeyboardManager` tracks the modifier and lock state globally, outside of any key event:

| Property | Meaning |
|---|---|
| `ShiftPressed` | A Shift key is currently held |
| `ControlPressed` | A Control key is currently held |
| `AltPressed` | An Alt key is currently held |
| `CapsLock` | Caps Lock is toggled on |
| `NumLock` | Num Lock is toggled on |
| `ScrollLock` | Scroll Lock is toggled on |

The lock keys toggle their state on each press and update the keyboard LEDs. Held modifiers also arrive on every `KeyEvent` through its `Modifiers` flags, which is usually the more convenient form.

## Keyboard layouts

Key presses come out of the hardware as layout-neutral scan codes; a scan map turns them into characters. The default is US QWERTY, and `SetKeyLayout` switches at any time:

```csharp
KeyboardManager.SetKeyLayout(new FRStandardLayout());
```

Seven layouts ship in `Cosmos.Kernel.System.Keyboard.ScanMaps`:

| Class | Layout |
|---|---|
| `USStandardLayout` | US QWERTY (default) |
| `FRStandardLayout` | French AZERTY |
| `DEStandardLayout` | German QWERTZ |
| `ESStandardLayout` | Spanish QWERTY |
| `GBStandardLayout` | British QWERTY |
| `TRStandardLayout` | Turkish Q |
| `USDvorakLayout` | US Dvorak |

The switch is visible immediately: below, the same six physical keys are typed twice, first under the US layout, then under the French one.

<!-- video: typing the six keys right of Tab under the US layout ("qwerty"), switching to FRStandardLayout, typing them again ("azerty") -->
<video src="images/keyboard-layouts.mp4" controls autoplay muted loop playsinline style="max-width:100%"></video>

`KeyboardManager.GetKeyLayout()` returns the active scan map, and a custom layout is a class deriving from `ScanMapBase` that overrides `InitializeKeys()` to fill the `Keys` list with `KeyMapping` entries. The base constructor calls it, so the list is populated by the time `SetKeyLayout` sees the instance.

## Current limitations

- Key releases are not queued: `KeyEvent.Type` has a `Break` value, but only presses reach the buffer. Releases of Shift, Ctrl and Alt update the modifier state and are otherwise dropped.
- The bundled scan maps cover the base and shifted characters only: AltGr combinations (`@`, `#`, `{` on AZERTY) and dead keys are not mapped.
- Devices are detected once at boot; there is no keyboard hotplug.

## How it works

Every key press raises an interrupt (IRQ1 for the PS/2 keyboard on x64, a virtio-input event on ARM64). The handler feeds the raw scan code to `KeyboardManager`, which routes lock and modifier keys to the state properties and converts everything else through the active scan map into a `KeyEvent`, queued in the key buffer. `ReadKey()` halts the CPU until an interrupt delivers the next event; `TryReadKey()` just dequeues. `Console.ReadLine` and `Console.ReadKey` are plugs on top of the same queue, so console input and raw key events never conflict.

```
Console.ReadLine / Console.ReadKey        (plugs, Cosmos.Kernel.Plugs)
        │
KeyboardManager ── KeyEvent queue ◀── scan map (active layout)
        │                                    ▲
        │                              raw scan codes
        │                                    │
PS/2 keyboard, IRQ1 (x64)  /  virtio-keyboard (x64 PCI, ARM64 MMIO)
```
