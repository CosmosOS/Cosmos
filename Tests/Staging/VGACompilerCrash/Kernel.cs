using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL;
using Sys = Cosmos.System;

namespace VGACompilerCrash
{
    public class Kernel : Sys.Kernel
    {
      public static VGAScreen screen = new VGAScreen();

      public void startGraphics()
      {
        //screen.SetGraphicsMode(VGAScreen.ScreenSize.Size320x200,VGAScreen.ColorDepth.BitDepth8);
        //screen.Clear(0);
      }

      protected override void BeforeRun()
      {
        Console.WriteLine("Successfully Loaded.");
      }

      protected override void Run()
      {
        Console.WriteLine("Welcome. Please login with the administrative credentials provided to you.");
        //LOGIN
        string username = "";
        string password = "";
        bool loggedIn = false;
        int attempts = 0;
        while (!loggedIn)
        {
          Console.Write("Username:");
          username = Console.ReadLine();
          Console.Write("Password:");
          password = Console.ReadLine();
          if (username == "root" && password == "password")
            loggedIn = true;
          else { attempts++; Console.WriteLine("Error: password mismatch. Try again."); }
          if (attempts >= 3)
          {
            Console.WriteLine("Too many attempts. Please power down."); //add ACPI.Shutdown later
            while (!loggedIn) { }
          }
        }
        //END LOGIN
        while (loggedIn)
        {
          Console.Write(">: ");
          var input = Console.ReadLine();
          if (input == "startx")
          {
            startGraphics();
          }
        }
      }
    }
}
