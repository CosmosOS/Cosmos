using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleClassTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            Class1 xObj = new Class2();
            xObj.MyMethod();
        }
    }

    public class Class1
    {
        public virtual string MyMethod()
        {
            return "Base(1)";
        }
    }

    public class Class2: Class1
    {
        public override string MyMethod()
        {
            return "Class2";
        }
    }
}
