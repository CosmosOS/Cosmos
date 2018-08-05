namespace Cosmos.HAL
{
    public abstract class KeyboardBase : Device
    {
        /// <summary>
        /// Initialize the device. Happens before the interrupt is registered, ie before the class is being used.
        /// </summary>
        public abstract void Initialize();

        public abstract void UpdateLeds(
            bool aScrollLock,
            bool aNumLock,
            bool aCapsLock);

        public delegate void KeyPressedEventHandler(byte ScanCode, bool Released);
        public KeyPressedEventHandler OnKeyPressed;

        public static void WaitForKey()
        {
            Core.Global.CPU.Halt();
        }
    }
}
