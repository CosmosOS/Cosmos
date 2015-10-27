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
        public static readonly IOGroup.Mouse Mouse = new IOGroup.Mouse();
        public static readonly IOGroup.PCSpeaker PCSpeaker = new IOGroup.PCSpeaker();
        public readonly IOGroup.PIT PIT = new IOGroup.PIT();
        public readonly IOGroup.TextScreen TextScreen = new IOGroup.TextScreen();
        public readonly IOGroup.ATA ATA1 = new IOGroup.ATA(false);
        public readonly IOGroup.ATA ATA2 = new IOGroup.ATA(true);
        public readonly IOGroup.RTC RTC = new IOGroup.RTC();
        public readonly IOGroup.VBE VBE = new IOGroup.VBE();
    }
}
