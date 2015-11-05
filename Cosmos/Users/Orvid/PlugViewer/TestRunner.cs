//#define DebugErrorLoading
//#define DebugWarningLoading

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using PlugViewer.Warnings;
using PlugViewer.Errors;
using PlugViewer.TreeViewNodes;

namespace PlugViewer
{
    public static class TestRunner
    {
        public static List<BaseError> FoundErrors = new List<BaseError>();
        public static List<BaseWarning> FoundWarnings = new List<BaseWarning>();

        private interface ITest
        {
            void Test();
        }
        private class CTest : ITest
        {
            public void Test() { }
        }

        private static void Test()
        {
            ITest i = new CTest();
            i.Test();
        }

        static TestRunner()
        {
            Assembly a = Assembly.GetAssembly(typeof(TestRunner));
            Type[] tps = a.GetTypes();
            foreach (Type t in tps)
            {
                if (t.BaseType == typeof(BaseError))
                {
#if DebugErrorLoading
                    Log.WriteLine("Loaded Error: '" + t.Name + "'");
#endif
                    FoundErrors.Add((BaseError)Activator.CreateInstance(t));
                }
                else if (t.BaseType == typeof(BaseWarning))
                {
#if DebugWarningLoading
                    Log.WriteLine("Loaded Warning: '" + t.Name + "'");
#endif
                    FoundWarnings.Add((BaseWarning)Activator.CreateInstance(t));
                }
            }
            tps = null;
            System.GC.Collect();
        }

        public static void RunTests()
        {
            for (byte b = 0; b < 8; b++)
            {
                foreach (OTreeNode n in OTreeNode.TreeNodes[b])
                {
                    n.Errors = new List<BaseError>();
                    n.Warnings = new List<BaseWarning>();
                }
            }

            foreach (BaseError e in FoundErrors)
            {
                foreach (OTreeNode n in OTreeNode.TreeNodes[(byte)e.AppliesTo])
                {
                    e.EvaluateNode(n);
                }
            }
            foreach (BaseWarning w in FoundWarnings)
            {
                foreach (OTreeNode n in OTreeNode.TreeNodes[(byte)w.AppliesTo])
                {
                    w.EvaluateNode(n);
                }
            }
        }

    }
}
