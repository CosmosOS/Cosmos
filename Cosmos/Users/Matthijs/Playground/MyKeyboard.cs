using Cosmos.HAL;

namespace Playground
{
    public class MyKeyboard: Keyboard
    {
        public MyKeyboard()
        {
        }

        protected override void Initialize()
        {
            //
        }

        protected override void HandleScancode(byte aScancode, bool aReleased)
        {
            //
        }

        public override void UpdateLeds()
        {
            throw new System.NotImplementedException();
        }
    }
}
