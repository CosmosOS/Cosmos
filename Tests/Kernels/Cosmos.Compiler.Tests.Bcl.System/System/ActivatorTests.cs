using System;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class ActivatorTests
    {
        public static unsafe void Execute()
        {
            // Till this moment (04/09/2024) the constructors, as any other method, are not included on the compilation result if never called,
            // so we need to call them before.
            Artesa dummy = new Artesa();
            WrappedLogger dummy2 = new WrappedLogger();
            ConsoleLogger dummy3 = new ConsoleLogger();


            // Generic method test
            ILogger logger = GetLogger<ConsoleLogger>();
            // Null checks are just for clarification, a ctor would throw if the object/struct was null.
            Assert.IsTrue(logger is not null, "Object incorrectly set.");
            Assert.IsTrue(typeof(ConsoleLogger).Equals(logger.GetType()), "Type Incorrectly set");
            logger.Log("Interface method Call works!");

            // Generic method test (with different type)
            logger = GetLogger<WrappedLogger>();
            Assert.IsTrue(logger is not null, "Object incorrectly set.");
            Assert.IsTrue(typeof(WrappedLogger).Equals(logger.GetType()), "Type Incorrectly set");
            logger.Log("Interface method call really really works!");
            ((WrappedLogger)logger).Logger.Log("property get works!");
            ((WrappedLogger)logger).Success("Type's specific methods work too.");

            // Struct Test
            var artesa = Activator.CreateInstance(typeof(Artesa));
            Assert.IsTrue(artesa is not null, "Struct incorrectly set.");
            Assert.IsTrue(typeof(Artesa).Equals(artesa.GetType()), "Type Incorrectly set");

            // Unboxing
            Artesa art = (Artesa)artesa;
            Assert.IsTrue(typeof(Artesa).Equals(art.GetType()), "Type Incorrectly set");

            // Check property
            Assert.IsTrue(art.Name is not null, "Property not set");

            // Test method overrides
            Assert.IsTrue(art.ToString() == $"{art.Name}-{art.LastName}", "Property not set");
        }

        public static T GetLogger<T>() where T : ILogger, new()
        {
            return new T();
        }
    }

    struct Artesa
    {
        public Artesa()
        {
            Console.WriteLine("LOL");
            Name = "Artesa";
            LastName = "Apple";
        }
        public string Name { get; }
        public string LastName { get; }

        public override string ToString()
        {
            return $"{Name}-{LastName}";
        }
    }

    class WrappedLogger : ILogger
    {
        private ILogger logger;

        public WrappedLogger() : this(ActivatorTests.GetLogger<ConsoleLogger>())
        {
            
        }

        public WrappedLogger(ILogger logger)
        {
            this.logger = logger;
        }

        internal ILogger Logger => logger;

        public void Log(string message) => logger.Log(message);
        public void Success(string message) => Assert.Succeed(message);
    }

    class ConsoleLogger : ILogger
    {
        public void Log(string message) => Console.WriteLine(message);
    }

    interface ILogger
    {
        void Log(string message);
    }
}
