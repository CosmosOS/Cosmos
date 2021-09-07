using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.TestRunner;
using Sys = Cosmos.System;

namespace Cosmos.Compiler.Tests.Interfaces.Kernel
{
    public class ClassA : IInterfaceA
    {
        public virtual void PrintText()
        {
            this.PrintTextA();
        }

        public virtual void PrintTextA()
        {
            Console.WriteLine("PrintTextA : ClassA : InterfaceA");
        }
    }

    public class ClassB : ClassA, IInterfaceB
    {
        public override void PrintText()
        {
            base.PrintText();

            this.PrintTextB();
        }

        public virtual void PrintTextB()
        {
            Console.WriteLine("PrintTextB : ClassB : InterfaceB");
        }
    }

    public class ClassC : ClassB, IInterfaceB
    {
        public override void PrintText()
        {
            base.PrintText();

            base.PrintTextA();
            this.PrintTextA();

            base.PrintTextB();
            this.PrintTextB();
        }

        public override void PrintTextA()
        {
            Console.WriteLine("PrintTextA : ClassC : InterfaceA");
        }

        public override void PrintTextB()
        {
            Console.WriteLine("PrintTextB : ClassC : InterfaceB");
        }
    }

    public class ClassT<T> : ClassC, IInterfaceT<T>
    {
        public void PrintText(T aType)
        {
            this.PrintTextT(aType);
        }

        public void PrintTextT(T aType)
        {
            Console.WriteLine("PrintText<{0}> : ClassT : InterfaceT", typeof(T).ToString());
        }
    }

    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully.");
        }

        protected override void Run()
        {
            var xA = new ClassA();
            var xB = new ClassB();
            var xC = new ClassC();
            var xT = new ClassT<string>();

            Console.WriteLine("-- A --");
            xA.PrintText();

            Console.WriteLine("-- B --");
            xB.PrintText();

            Console.WriteLine("-- C --");
            xC.PrintText();

            Console.WriteLine("-- T --");
            xT.PrintText("Test");

            TestController.Completed();
        }
    }
}
