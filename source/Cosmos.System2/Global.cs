using System;
using System.Diagnostics.CodeAnalysis;
using Cosmos.Debug.Kernel;
using Cosmos.HAL;

using Debugger = Cosmos.Debug.Kernel.Debugger;

namespace Cosmos.System
{
    /// <summary>
    /// Contains commonly used globals. Used to initialize the console, screen
    /// and debugger and get/set keyboard scan-maps.
    /// </summary>
    public static class Global
    {
        /// <summary>
        /// The global system ring debugger instance, with the tag "Global".
        /// </summary>
        public static readonly Debugger Debugger = DebuggerFactory.CreateDebugger("Global");

        /// <summary>
        /// The main global console instance.
        /// </summary>
        public static Console Console;

        /// <summary>
        /// Gets and sets the keyboards num-lock state.
        /// </summary>
        public static bool NumLock
        {
            get => KeyboardManager.NumLock;
            set => KeyboardManager.NumLock = value;
        }

        /// <summary>
        /// Gets and sets the keyboards caps-lock state.
        /// </summary>
        public static bool CapsLock
        {
            get => KeyboardManager.CapsLock;
            set => KeyboardManager.CapsLock = value;
        }

        /// <summary>
        /// Gets and sets the keyboards scroll-lock state.
        /// </summary>
        public static bool ScrollLock
        {
            get => KeyboardManager.ScrollLock;
            set => KeyboardManager.ScrollLock = value;
        }

        // TODO: continue adding exceptions to the list, as HAL and Core would be documented.
        /// <summary>
        /// Initializes the console, screen and keyboard.
        /// </summary>
        /// <param name="textScreen">A screen device.</param>
        public static void Init(TextScreenBase textScreen, bool initScrollWheel = true, bool initPS2 = true, bool initNetwork = true, bool ideInit = true)
        {

            // We must init Console before calling Inits.
            // This is part of the "minimal" boot to allow output.
            Debugger.Send("Creating the global console...");
            Console = new Console(textScreen);

            Debugger.Send("Initializing the Hardware Abstraction Layer (HAL)...");
            HAL.Global.Init(textScreen, initScrollWheel, initPS2, initNetwork, ideInit);

            // TODO: @ascpixi: The end-user should have an option to exclude parts of
            //       Cosmos, such as the network stack, when they are not needed. As of
            //       now, these modules will *always* be included, as they're referenced
            //       by the initialization code.
            Debugger.Send("Initializing the network stack...");
            Network.NetworkStack.Initialize();

            NumLock = false;
            CapsLock = false;
            ScrollLock = false;
        }
    }
}