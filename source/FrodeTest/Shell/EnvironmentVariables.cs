using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FrodeTest.Shell
{
    public class EnvironmentVariables
    {
        private static EnvironmentVariables mCurrent = null;
        public static EnvironmentVariables GetCurrent()
        {
            if (mCurrent == null)
                mCurrent = new EnvironmentVariables();

            return mCurrent;
        }

        //Hidden constructor
        private EnvironmentVariables()
        {
        }

        private string mCurrentDirectory = "/0/";
        public string CurrentDirectory { get { return mCurrentDirectory; } set { mCurrentDirectory = value; } }
    }
}
