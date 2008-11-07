using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
    public static class InfraExtensions {
        public static string GetAsText(this IDefine aThis) {
            return "%define " + aThis.Symbol + " 1";
        }

        public static string GetAsText(this IIfDefined aThis) {
            return "%ifdef " + aThis.Symbol;
        }

        public static string GetAsText(this IEndIfDefined aThis) {
            return "%endif";
        }

        public static string GetAsText(this IIfNotDefined aThis) {
            return "%ifndef " + aThis.Symbol;
        }
    }
}