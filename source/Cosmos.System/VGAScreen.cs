using System;
using HALVGAScreen =Cosmos.HAL.VGAScreen;

namespace Cosmos.System
{
    [Obsolete("This class has not been properly converted to the final cosmos architecture!")]
    public class VGAScreen
    {
        private HALVGAScreen mScreen = new HALVGAScreen();
        // todo: this class needs to wrap HALVGAScreen
    }
}
