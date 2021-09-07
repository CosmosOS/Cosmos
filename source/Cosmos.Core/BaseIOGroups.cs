using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core {
    /// <summary>
    /// Base IO Groups. Used to easily access IO devices.
    /// </summary>
    public class BaseIOGroups {
        // These are common/fixed pieces of hardware. PCI, USB etc should be self discovering
        // and not hardcoded like this.
        // Further more some kind of security needs to be applied to these, but even now
        // at least we have isolation between the consumers that use these.
        /// <summary>
        /// PS/2 controller.
        /// </summary>
        public readonly IOGroup.PS2Controller PS2Controller = new IOGroup.PS2Controller();
        /// <summary>
        /// PC speaker.
        /// </summary>
        public static readonly IOGroup.PCSpeaker PCSpeaker = new IOGroup.PCSpeaker();
        /// <summary>
        /// PIT.
        /// </summary>
        public readonly IOGroup.PIT PIT = new IOGroup.PIT();
        /// <summary>
        /// Text screen.
        /// </summary>
        public readonly IOGroup.TextScreen TextScreen = new IOGroup.TextScreen();
        /// <summary>
        /// Primary ATA.
        /// </summary>
        public readonly IOGroup.ATA ATA1 = new IOGroup.ATA(false);
        /// <summary>
        /// Secondary ATA.
        /// </summary>
        public readonly IOGroup.ATA ATA2 = new IOGroup.ATA(true);
        /// <summary>
        /// Real time clock.
        /// </summary>
        public readonly IOGroup.RTC RTC = new IOGroup.RTC();
        /// <summary>
        /// VBE.
        /// </summary>
        public readonly IOGroup.VBEIOGroup VBE = new IOGroup.VBEIOGroup();
    }
}
