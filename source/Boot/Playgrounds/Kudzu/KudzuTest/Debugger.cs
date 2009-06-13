using System;
using System.Collections.Generic;
using System.Text;
//using Dbg = Cosmos.Debug;

namespace Cosmos.Playground.Kudzu {
  public class Debugger {
    public static void Main() {
      Random xRandom = new Random((int)(Cosmos.Hardware.Global.TickCount
          + Cosmos.Hardware.RTC.GetSeconds()));
      // Divide by 100, get remainder
      //Dbg.Debugger.Break();
      int xMagicNo = xRandom.Next() % 100;
      //Dbg.Debugger.Send("The magic number is " + xMagicNo);
      Console.WriteLine("Hello world");
      //Dbg.Debugger.TraceOn();
      int xDummy = 4;
      //Dbg.Debugger.TraceOff();
      Console.ReadLine();
    }
  }
}
