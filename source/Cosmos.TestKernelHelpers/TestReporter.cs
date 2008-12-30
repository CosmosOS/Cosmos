using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware;

namespace Cosmos.TestKernelHelpers {
    public static class TestReporter {
        public enum CommandEnum : uint {
            Unknown,
            String = 1,
            Initialized = 2,
            TestRunCompleted = 3,
            TestCompleted = 4
        }
        public static void Initialize() {
            Serial.InitSerial(1);
            WriteUIntToSerial((uint)CommandEnum.Initialized);
        }

        public static void TestsCompleted(){
            WriteUIntToSerial((uint)CommandEnum.TestRunCompleted);
        }

        private static void WriteStringToSerial(string aString) {
            WriteUIntToSerial((uint)aString.Length);
            for (int i = 0; i < aString.Length; i++) {
                Serial.WriteSerial(1, (byte)aString[i]); // Todo: unicode?
            }
        }

        public static void WriteUIntToSerial(uint aValue) {
            var xBytes = BitConverter.GetBytes(aValue);
            for (int i = 0; i < xBytes.Length; i++) {
                Serial.WriteSerial(1, xBytes[i]);
            }
        }

        public static void WriteString(string aString) {
            WriteUIntToSerial((uint)CommandEnum.String);
            WriteStringToSerial(aString);
        }

        public static void WriteTestResult(string aTest, string aDescription, bool aPassed){
            WriteUIntToSerial((uint)CommandEnum.TestCompleted);
            WriteStringToSerial(aTest);
            WriteStringToSerial(aDescription);
            Serial.WriteSerial(1, (byte)(aPassed ? 1 : 0));
        }
    }
}