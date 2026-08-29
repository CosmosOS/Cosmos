// This code is licensed under the BSD 3-Clause license (see LICENSE for details)
// Ported from Cosmos.System2/Keyboard/ConsoleKeyEx.cs

namespace Cosmos.Kernel.System.Keyboard;

/// <summary>
/// Specifies the recognized virtual console keys, that are independent
/// from physical keyboard scan-codes.
/// </summary>
public enum ConsoleKeyEx
{
    /// <summary>
    /// An unknown, undefined, or otherwise unrecognized key.
    /// </summary>
    NoName,

    /// <summary>The Esc key.</summary>
    Escape,

    /// <summary>The F1 function key.</summary>
    F1,
    /// <summary>The F2 function key.</summary>
    F2,
    /// <summary>The F3 function key.</summary>
    F3,
    /// <summary>The F4 function key.</summary>
    F4,
    /// <summary>The F5 function key.</summary>
    F5,
    /// <summary>The F6 function key.</summary>
    F6,
    /// <summary>The F7 function key.</summary>
    F7,
    /// <summary>The F8 function key.</summary>
    F8,
    /// <summary>The F9 function key.</summary>
    F9,
    /// <summary>The F10 function key.</summary>
    F10,
    /// <summary>The F11 function key.</summary>
    F11,
    /// <summary>The F12 function key.</summary>
    F12,

    /// <summary>The Print Screen key.</summary>
    PrintScreen,
    /// <summary>The Scroll Lock key.</summary>
    ScrollLock,
    /// <summary>The Pause/Break key.</summary>
    PauseBreak,

    /// <summary>The backquote (`) key.</summary>
    Backquote,
    /// <summary>The 1 key on the top row.</summary>
    D1,
    /// <summary>The 2 key on the top row.</summary>
    D2,
    /// <summary>The 3 key on the top row.</summary>
    D3,
    /// <summary>The 4 key on the top row.</summary>
    D4,
    /// <summary>The 5 key on the top row.</summary>
    D5,
    /// <summary>The 6 key on the top row.</summary>
    D6,
    /// <summary>The 7 key on the top row.</summary>
    D7,
    /// <summary>The 8 key on the top row.</summary>
    D8,
    /// <summary>The 9 key on the top row.</summary>
    D9,
    /// <summary>The 0 key on the top row.</summary>
    D0,
    /// <summary>The minus (-) key.</summary>
    Minus,
    /// <summary>The equals (=) key.</summary>
    Equal,
    /// <summary>The backslash (\) key.</summary>
    Backslash,
    /// <summary>The Backspace key.</summary>
    Backspace,

    /// <summary>The Tab key.</summary>
    Tab,
    /// <summary>The Q key.</summary>
    Q,
    /// <summary>The W key.</summary>
    W,
    /// <summary>The E key.</summary>
    E,
    /// <summary>The R key.</summary>
    R,
    /// <summary>The T key.</summary>
    T,
    /// <summary>The Y key.</summary>
    Y,
    /// <summary>The U key.</summary>
    U,
    /// <summary>The I key.</summary>
    I,
    /// <summary>The O key.</summary>
    O,
    /// <summary>The P key.</summary>
    P,
    /// <summary>The left bracket ([) key.</summary>
    LBracket,
    /// <summary>The right bracket (]) key.</summary>
    RBracket,
    /// <summary>The Enter key.</summary>
    Enter,

    /// <summary>The Caps Lock key.</summary>
    CapsLock,
    /// <summary>The A key.</summary>
    A,
    /// <summary>The S key.</summary>
    S,
    /// <summary>The D key.</summary>
    D,
    /// <summary>The F key.</summary>
    F,
    /// <summary>The G key.</summary>
    G,
    /// <summary>The H key.</summary>
    H,
    /// <summary>The J key.</summary>
    J,
    /// <summary>The K key.</summary>
    K,
    /// <summary>The L key.</summary>
    L,
    /// <summary>The semicolon (;) key.</summary>
    Semicolon,
    /// <summary>The colon (:) key.</summary>
    Colon,
    /// <summary>The apostrophe (') key.</summary>
    Apostrophe,

    /// <summary>The less-than (&lt;) key.</summary>
    LowerThan,
    /// <summary>The greater-than (&gt;) key.</summary>
    BiggerThan,

    /// <summary>The exclamation point (!) key.</summary>
    ExclamationPoint,

    /// <summary>The left Shift key.</summary>
    LShift,
    /// <summary>The right Shift key.</summary>
    RShift,
    /// <summary>
    /// The extra key next to the left Shift on ISO layouts (absent on a US
    /// keyboard); on the German layout it carries |, &lt; and &gt;.
    /// </summary>
    OEM102,
    /// <summary>
    /// The key registered as \ and | on a British keyboard, and as # and ~
    /// on a US one.
    /// </summary>
    OEM5,
    /// <summary>The Z key.</summary>
    Z,
    /// <summary>The X key.</summary>
    X,
    /// <summary>The C key.</summary>
    C,
    /// <summary>The V key.</summary>
    V,
    /// <summary>The B key.</summary>
    B,
    /// <summary>The N key.</summary>
    N,
    /// <summary>The M key.</summary>
    M,
    /// <summary>The comma (,) key.</summary>
    Comma,
    /// <summary>The period (.) key.</summary>
    Period,
    /// <summary>The slash (/) key.</summary>
    Slash,

    /// <summary>The left Ctrl key.</summary>
    LCtrl,
    /// <summary>The right Ctrl key.</summary>
    RCtrl,
    /// <summary>The left Windows key.</summary>
    LWin,
    /// <summary>The left Alt key.</summary>
    LAlt,
    /// <summary>The right Alt key.</summary>
    RAlt,
    /// <summary>The Spacebar key.</summary>
    Spacebar,
    /// <summary>The AltGr key.</summary>
    AltGr,
    /// <summary>The right Windows key.</summary>
    RWin,
    /// <summary>The Menu (context menu) key.</summary>
    Menu,

    /// <summary>The Insert key.</summary>
    Insert,
    /// <summary>The Home key.</summary>
    Home,
    /// <summary>The Page Up key.</summary>
    PageUp,
    /// <summary>The Delete key.</summary>
    Delete,
    /// <summary>The End key.</summary>
    End,
    /// <summary>The Page Down key.</summary>
    PageDown,

    /// <summary>The up arrow key.</summary>
    UpArrow,
    /// <summary>The down arrow key.</summary>
    DownArrow,
    /// <summary>The left arrow key.</summary>
    LeftArrow,
    /// <summary>The right arrow key.</summary>
    RightArrow,

    /// <summary>The Num Lock key.</summary>
    NumLock,
    /// <summary>The divide (/) key on the numeric keypad.</summary>
    NumDivide,
    /// <summary>The multiply (*) key on the numeric keypad.</summary>
    NumMultiply,
    /// <summary>The minus (-) key on the numeric keypad.</summary>
    NumMinus,
    /// <summary>The 7 key on the numeric keypad.</summary>
    Num7,
    /// <summary>The 8 key on the numeric keypad.</summary>
    Num8,
    /// <summary>The 9 key on the numeric keypad.</summary>
    Num9,
    /// <summary>The plus (+) key on the numeric keypad.</summary>
    NumPlus,
    /// <summary>The 4 key on the numeric keypad.</summary>
    Num4,
    /// <summary>The 5 key on the numeric keypad.</summary>
    Num5,
    /// <summary>The 6 key on the numeric keypad.</summary>
    Num6,
    /// <summary>The 1 key on the numeric keypad.</summary>
    Num1,
    /// <summary>The 2 key on the numeric keypad.</summary>
    Num2,
    /// <summary>The 3 key on the numeric keypad.</summary>
    Num3,
    /// <summary>The 0 key on the numeric keypad.</summary>
    Num0,
    /// <summary>The period (.) key on the numeric keypad.</summary>
    NumPeriod,
    /// <summary>The Enter key on the numeric keypad.</summary>
    NumEnter,

    /// <summary>The Power key.</summary>
    Power,
    /// <summary>The Sleep key.</summary>
    Sleep,
    /// <summary>The Wake key.</summary>
    Wake
}
