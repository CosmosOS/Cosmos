using System;
using Cosmos.Compiler.Builder;
using Cosmos.Sys;
using System.Collections.Generic;
using Cosmos.Hardware;

namespace BaseTest
{
    class Program
    {
        #region Cosmos Builder logic
        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        static void Main(string[] args)
        {
            BuildUI.Run();
        }
        #endregion

        public static void Eratosthene()
        {
            Console.WriteLine("Eratosthene check (A little benchmark on the memoryaccess/cpu of Cosmos)");
            Console.Write("Executing 1024000 element (int) array test : ");

            Int32 BeforeSeconds = RTC.GetSeconds();
            Int32 BeforeMinutes = RTC.GetMinutes();
            Int32 BeforeHours = RTC.GetHours();

            Boolean[] arr = new Boolean[1024000];
            for (int i = 2; (i * i) <= arr.Length; i++)
            {
                for (int j = (i * i); j <= arr.Length; j = j + i)
                {
                    arr[j] = true;
                }
            }

            Int32 AfterSeconds = RTC.GetSeconds();
            Int32 AfterMinutes = RTC.GetMinutes();
            Int32 AfterHours = RTC.GetHours();

            Int32 DiffsHours = (AfterHours - BeforeHours);
            Int32 DiffsMinutes = (AfterMinutes - BeforeMinutes);
            Int32 DiffsSeconds = (AfterSeconds - BeforeSeconds);

            Int32 FinalHours = DiffsHours;
            Int32 FinalMinutes = DiffsMinutes;
            Int32 FinalSeconds = DiffsSeconds;

            Int32 Completed = 0;
            while (Completed < 3)
            {
                Completed = 0;
                if (DiffsHours < 0)
                    FinalHours = 24 - DiffsHours;
                else
                    Completed++;
                if (DiffsMinutes < 0)
                {
                    FinalMinutes = 60 - DiffsMinutes;
                    FinalHours--;
                }
                else
                    Completed++;
                if (DiffsSeconds < 0)
                {
                    FinalSeconds = 60 - DiffsSeconds;
                    FinalMinutes--;
                }
                else
                    Completed++;
            }
            Console.WriteLine(FinalHours + " Hours" +
                              FinalMinutes + " Minutes" +
                              FinalSeconds + " Seconds");
        }

        public static void PrimeNumbers()
        {
            Console.WriteLine("Traditional prime number check (A little benchmark on the memoryaccess/cpu of Cosmos)");
            Console.Write("Executing 4096 element array test : ");

            Int32 BeforeSeconds = RTC.GetSeconds();
            Int32 BeforeMinutes = RTC.GetMinutes();
            Int32 BeforeHours = RTC.GetHours();

            Boolean[] arr = new Boolean[4096];
            for (int i = 2; i < arr.Length; i++)
            {
                for (int j = i + 1; j < arr.Length; j++)
                {
                    if (arr[j] == false)
                        if (j % i == 0)
                            arr[j] = true;
                }
            }

            Int32 AfterSeconds = RTC.GetSeconds();
            Int32 AfterMinutes = RTC.GetMinutes();
            Int32 AfterHours = RTC.GetHours();

            Int32 DiffsHours = (AfterHours - BeforeHours);
            Int32 DiffsMinutes = (AfterMinutes - BeforeMinutes);
            Int32 DiffsSeconds = (AfterSeconds - BeforeSeconds);

            Int32 FinalHours = DiffsHours;
            Int32 FinalMinutes = DiffsMinutes;
            Int32 FinalSeconds = DiffsSeconds;

            Int32 Completed = 0;
            while (Completed < 3)
            {
                Completed = 0;
                if (DiffsHours < 0)
                    FinalHours = 24 - DiffsHours;
                else
                    Completed++;
                if (DiffsMinutes < 0)
                {
                    FinalMinutes = 60 - DiffsMinutes;
                    FinalHours--;
                }
                else
                    Completed++;
                if (DiffsSeconds < 0)
                {
                    FinalSeconds = 60 - DiffsSeconds;
                    FinalMinutes--;
                }
                else
                    Completed++;
            }
            Console.WriteLine(FinalHours + " Hours" +
                              FinalMinutes + " Minutes" +
                              FinalSeconds + " Seconds");
        }

        // Main entry point of the kernel
        public static void Init()
        {
            Boot bt = new Boot();
            bt.Execute();

            Console.WriteLine("Welcome! This will be my playground to test out the various features i'll implement. :)");

            PrimeNumbers();
            Eratosthene();

            Deboot.ShutDown();
        }
    }
}