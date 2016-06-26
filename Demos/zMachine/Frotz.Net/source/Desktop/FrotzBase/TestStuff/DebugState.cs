using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Frotz.Generic;

namespace Frotz {
    public class DebugState {

        internal static bool IsActive { get; private set; }
        public static void StartState(String stateFileToLoad) {
            if (stateFileToLoad != null) {
                var good = new StreamReader(stateFileToLoad);
                String line;
                while ((line = good.ReadLine()) != null) {
                    if (!line.StartsWith("#")) {
                        _stateLines.Add(line);
                    }
                }
            }
            IsActive = true;
        }

        private static List<String> _stateLines = new List<string>();
        public static List<String> StateLines { get { return _stateLines; } }

        public static List<String> _outputLines = new List<string>();
        public static List<String> OutputLines { get { return _outputLines; } }

        static int currentState = 0;

        internal static String last_call_made = "";

        public static void Output(String s, params Object[] data) {
            Output(true, s, data);
        }

        public static void Output(bool log, String s, params Object[] data) {
            if (IsActive) {
                //String current = String.Format(s, data);

                //if (log && currentState < _stateLines.Count && !s.StartsWith("#")) {
                //    String expected = _stateLines[currentState++];

                //    if (!string.Equals(expected, current, StringComparison.OrdinalIgnoreCase)) {
                //        System.Diagnostics.Debug.WriteLine("mismatch! Expected:" + expected + ": Current:" + current + ":" + currentState);
                //        _stateLines.Clear();
                //    }

                //} else {
                //    OutputLines.Add(current);
                //    System.Diagnostics.Debug.WriteLine(current);
                //}

            }
        }

        //private static Debugger mDebugger = new Debugger("");

        public static void savezmachine(string fileToSaveTo) {
            if (IsActive)
            {
                FileStream fs = new FileStream(fileToSaveTo, FileMode.Create);
                fs.Write(FastMem.ZMData, 0, FastMem.ZMData.Length);
                fs.Close();
            }
        }

        static int _seed = 0;
        internal static int RandomSeed() {
            return _seed++;

        }
    }
}
