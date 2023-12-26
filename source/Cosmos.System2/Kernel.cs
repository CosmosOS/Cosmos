using System;
using Cosmos.Debug.Kernel;
using Cosmos.HAL;

namespace Cosmos.System;

// MtW: if the fullname (namespace + name) of this class changes, please also change IL2CPU msbuild task
/// <summary>
/// Provides a base kernel class for a Cosmos-based system
/// </summary>
public abstract class Kernel
{
    /// <summary>
    /// User ring debugger instance, with the tag "Kernel".
    /// </summary>
    public readonly Debugger mDebugger = new("Kernel");

    // Set after initial start. Can be started and stopped at same time
    protected bool mStarted;

    // Set to signal stopped
    protected bool mStopped;

    /// <summary>
    /// Gets the text screen device to initialize the system with. If not
    /// overriden, the default text screen device will be used.
    /// </summary>
    // If this method returns "null", that means that the default device should be used.
    protected virtual TextScreenBase GetTextScreen() => null;

    /// <summary>
    /// Start the system up using the properties for configuration.
    /// </summary>
    public virtual void Start()
    {
        try {
            Global.Debugger.Send("Starting the kernel...");
            if (mStarted)
            {
                Global.Debugger.Send("ERROR: The kernel has already been started.");
                throw new Exception("Kernel has already been started. A kernel cannot be started twice.");
            }

            mStarted = true;

            Global.Debugger.Send("Initializing hardware bootstrap...");
            Bootstrap.Init();
            OnBoot();

            BeforeRun();

            // now enable interrupts:
            HAL.Global.EnableInterrupts();

            while (!mStopped)
            {
                Run();
            }

            Global.Debugger.Send("The main kernel loop has stopped.");
            AfterRun();
        }
        catch (Exception e)
        {
            // todo: better ways to handle?
            Global.Debugger.Send($"Kernel Exception {e}");
            global::System.Console.ForegroundColor = ConsoleColor.Red;
            global::System.Console.WriteLine("A kernel exception has occured:");
            global::System.Console.ForegroundColor = ConsoleColor.White;
            global::System.Console.WriteLine(e.ToString());
        }
    }

    /// <summary>
    /// This method controls the driver initialisation process.
    /// </summary>
    // 1. Mousewheel, if you experience your mouse cursors being stuck in the lower left corner set this to "false", default: true
    // 2. PS2 Driver initialisation, true/false , default: true
    // 3. Network Driver initialisation, true/false, default: true
    // 4. IDE initialisation, true/false, default: true
    // If you need anything else to be initialised early on, place it here.
    protected virtual void OnBoot() => Global.Init(GetTextScreen());

    /// <summary>
    /// Called before the main kernel loop begins.
    /// </summary>
    protected virtual void BeforeRun()
    {
    }

    /// <summary>
    /// The main kernel loop method; this method is called on a infinite
    /// loop, until <see cref="Stop"/> is called.
    /// </summary>
    protected abstract void Run();

    /// <summary>
    /// Called after the main kernel loop method finishes. The main kernel
    /// loop can stop after e.g. a call to the <see cref="Stop"/> method.
    /// </summary>
    protected virtual void AfterRun()
    {
    }

    /// <summary>
    /// Stops the main kernel loop.
    /// </summary>
    public void Stop() => mStopped = true;

    /// <summary>
    /// The kernel object construtor. Overriding this constructor is not
    /// recommended and may result in undesirable behavior.
    /// </summary>
    public Kernel()
    {
        Global.Debugger.Send("Constructing a new Cosmos.System.Kernel instance.");
    }

    /// <summary>
    /// Prints a message to the debugger with the "Global" tag.
    /// </summary>
    /// <param name="message">The message to print.</param>
    public static void PrintDebug(string message) => Global.Debugger.Send(message);

    /// <summary>
    /// Whether system interrupts are currently enabled.
    /// </summary>
    public static bool InterruptsEnabled => HAL.Global.InterruptsEnabled;
}