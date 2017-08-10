using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.TestRunner;
using Sys = Cosmos.System;

namespace VGACompilerCrash
{
    public class Kernel : Sys.Kernel
    {
      public static Sys.VGAScreen screen = new Sys.VGAScreen();

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
          Assert.IsTrue(true, "Fake assert");
          // the actual testing is done via Sys.VGAScreen
          TestController.Completed();
      }
    }
}
