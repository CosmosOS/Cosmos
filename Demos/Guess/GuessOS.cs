using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Cosmos.Debug.Kernel;
using Sys = Cosmos.System;

namespace GuessKernel
{
  public class GuessOS: Sys.Kernel
  {
    protected int mCount = 0;
    protected int mMagicNo = 22;

    public GuessOS()
    {
      // Didnt check if tickcount is working yet.. can change this later
      //var xRandom = new Random(234243534);
      //mMagicNo = xRandom.Next(1, 100);
    }

    protected override void BeforeRun()
    {
      //Cosmos.Core.HMI.Init();

      Console.WriteLine("Guess Demo");
      Console.WriteLine("Please guess a number from 1 to 100.");
    }

    private Debugger mDebugger = new Debugger("User", "Guess");

    private class KVPClass
    {
      public int Key;
      public int Value;
    }

    private struct KVPStruct
    {
      public int Key;
      public int Value;
    }

    protected override void Run()
    {
      mCount++;

      var xListClasses = new List<KVPClass>();
      var xListStructs = new List<KVPStruct>();

      xListClasses.Add(new KVPClass { Key = 1, Value = 2 });
      xListClasses.Add(new KVPClass { Key = 2, Value = 5 });

      var xListItem= xListClasses[0];
      Console.Write("Classes0. Key = ");
      Console.Write(xListItem.Key);
      Console.Write("Value = ");
      Console.WriteLine(xListItem.Value);
      xListItem = xListClasses[1];
      Console.Write("Classes1. Key = ");
      Console.Write(xListItem.Key);
      Console.Write("Value = ");
      Console.WriteLine(xListItem.Value);

      xListStructs.Add(new KVPStruct { Key = 1, Value = 2 });
      xListStructs.Add(new KVPStruct { Key = 2, Value = 5 });

      var xStructItem = xListStructs[0];
      Console.Write("Item0. Key = ");
      Console.Write(xStructItem.Key);
      Console.Write("Value = ");
      Console.WriteLine(xStructItem.Value);
      xStructItem = xListStructs[1];
      Console.Write("Item1. Key = ");
      Console.Write(xStructItem.Key);
      Console.Write("Value = ");
      Console.WriteLine(xStructItem.Value);


      Stop();
    }
  }
}
