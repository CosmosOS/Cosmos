using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;


namespace Cosmos.Kernal.Test.Collections
{
    public class Kernel : Sys.Kernel
    {
        
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully.");
            
        }

        protected override void Run()
        {
            mDebugger.Send("Run");

            HashtableTest();

            DictionaryTest();

        }
        #region "Hashtable"
        private void HashtableTest()
        {
            mDebugger.Send("Making Hashtable");
            Hashtable a = new Hashtable();
            Hashtable aa = new Hashtable();
            #region "Core Tests"
            // add key test
            mDebugger.Send("Hashtable add key test");
            try
            {
                a.Add("test", "this is a test");
                mDebugger.Send("Hashtable add key 'test' with value of 'this is a test'");
            }
            catch
            {
                mDebugger.Send("Hashtable failded to add key 'test' with value of 'this is a test'");
            }

            // set key test
            mDebugger.Send("Hashtable set key test");
            try
            {
                a["test"] = "this is a test one";
                if (a["test"] == "this is a test one")
                {
                    mDebugger.Send("Hashtable set key 'test' with value of 'this is a test one'");
                }
                else
                {
                    mDebugger.Send("Hashtable failed to key 'test' with value of 'this is a test one'");
                }
            }
            catch
            {
                mDebugger.Send("Hashtable failed to key 'test' with value of 'this is a test one'");
            }
            // count test
            try
            {
                int ac = a.Count;
                if (ac == 1)
                {
                    mDebugger.Send("Hashtable Count Worked");
                }
                else
                {
                    mDebugger.Send("Hashtable failed did not return 1");
                }
            }
            catch
            {
                mDebugger.Send("Hashtable failed");
            }
            #endregion




        }

        #endregion

        #region "Dictionary"
        private void DictionaryTest()
        {
            mDebugger.Send("Making Dictionary");
             Dictionary<string, string> a = new Dictionary<string, string>();
            Dictionary<string, string> aa = new Dictionary<string, string>();

            // add key test
            mDebugger.Send("Dictionary add key test");
            try
            {
                a.Add("test", "this is a test");
                mDebugger.Send("Dictionary add key 'test' with value of 'this is a test'");
            }
            catch
            {
                mDebugger.Send("Dictionary failded to add key 'test' with value of 'this is a test'");
            }

            // set key test
            mDebugger.Send("Dictionary set key test");
            try
            {
                a["test"] = "this is a test one";
                if (a["test"] == "this is a test one")
                {
                    mDebugger.Send("Dictionary set key 'test' with value of 'this is a test one'");
                }
                else
                {
                    mDebugger.Send("Dictionary failed to key 'test' with value of 'this is a test one'");
                }
            }
            catch
            {
                mDebugger.Send("Dictionary failed to key 'test' with value of 'this is a test one'");
            }
            // count test
            try
            {
                int ac = a.Count;
                if (ac == 1)
                {
                    mDebugger.Send("Dictionary Count Worked");
                }
                else
                {
                    mDebugger.Send("Dictionary failed did not return 1");                    
                }
            }
            catch
            {
                mDebugger.Send("Dictionary failed");
            }




        }

        #endregion
    }
}
