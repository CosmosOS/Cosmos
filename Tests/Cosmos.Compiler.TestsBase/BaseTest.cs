using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Core;
using ApprovalTests.Namers;
using ApprovalTests.Writers;

namespace Cosmos.Compiler.TestsBase
{
    public abstract class BaseTest
    {
        private class Namer : UnitTestFrameworkNamer
        {
            private string mName;

            public Namer(string name)
            {
                mName = name;
            }
            public override string Name
            {
                get
                {
                    return base.Name + "." + mName;
                }
            }
        }

        protected void Verify(string name, string value)
        {
            Approvals.Verify(WriterFactory.CreateTextWriter(value), new Namer(name), Approvals.GetReporter());
        }
    }
}