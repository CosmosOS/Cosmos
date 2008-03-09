using System;
using Cosmos.Build.Windows;

namespace FrodeTest
{
    class Program
    {
        #region Cosmos Builder logic
        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        static void Main(string[] args)
        {
            var xBuilder = new Builder();
            xBuilder.Build();
        }
        #endregion

        // Main entry point of the kernel
        //This is the playground for Frode "Scalpel" Lillerud.
        public static void Init()
        {
            Cosmos.Kernel.Boot.Default();

            Cosmos.Kernel.Staging.DefaultStageQueue stages = new Cosmos.Kernel.Staging.DefaultStageQueue();
            stages.Enqueue(new Cosmos.Kernel.Staging.Stages.KernelStage());
            stages.Run();
            //Stages
            /*
             * 1. Initialize hardware
             * 2. Communicate with filesystem
             * 2. Set up basic security
             * 3. Initialize Shell, and allow user to log in
             */
            
            Security.User currentUser = Security.User.Authenticate("frode", "secret");
            Shell.Session currentSession =  Shell.Session.CreateSession(currentUser);
            currentSession.Run();

            //TESTING
            Test.Dummy dummy = new FrodeTest.Test.Dummy();
            dummy.Execute();
             

            Console.WriteLine("Shutting down computer");
            while (true)
                ;
        }
    }
}