using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core {
    public class BaseIOGroups {
        // These are common/fixed pieces of hardware. PCI, USB etc should be self discovering
        // and not hardcoded like this.
        // Further more some kind of security needs to be applied to these, but even now
        // at least we have isolation between the consumers that use these.
        public readonly IOGroup.Keyboard Keyboard = new IOGroup.Keyboard();
        public readonly IOGroup.PIC PIC = new IOGroup.PIC();
        public readonly IOGroup.PIT PIT = new IOGroup.PIT();
        public readonly IOGroup.TextScreen TextScreen = new IOGroup.TextScreen();
    }
}
