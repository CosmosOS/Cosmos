using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel {
    public class StringFuncs {
        public static string BoolString(bool aValue) {
            if (aValue) {
                return "true";
            } else {
                return "false";
            }
        }

        //TODO: convert to return string...
        public static string TimeString() {
            Console.Write("Time: ");
            Hardware.Storage.ATAOld.WriteNumber(Hardware.RTC.GetHours(), 8);
            Console.Write(":");
            Hardware.Storage.ATAOld.WriteNumber(Hardware.RTC.GetMinutes(), 8);
            Console.Write(":");
            Hardware.Storage.ATAOld.WriteNumber(Hardware.RTC.GetSeconds(), 8);
            Console.WriteLine("");
            return "";
        }

    }
}
