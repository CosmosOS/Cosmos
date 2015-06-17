using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Cosmos.Debug.Kernel;
using Sys = Cosmos.System;

namespace GuessKernel {
  public class GuessOS : Sys.Kernel {
    protected int mCount = 0;
    protected int mMagicNo = 22;

    public GuessOS() {
      // Didnt check if tickcount is working yet.. can change this later
      //var xRandom = new Random(234243534);
      //mMagicNo = xRandom.Next(1, 100);
    }

    protected override void BeforeRun() {
      //Cosmos.Core.HMI.Init();

      Console.WriteLine("Guess Demo");
      Console.WriteLine("Please guess a number from 1 to 100.");
    }

    private Debugger mDebugger = new Debugger("User", "Guess");

    [StructLayout(LayoutKind.Explicit)]
    private class MyStruct
    {
      public MyStruct()
      {

      }

      public MyStruct(short a, short b, short c, short d, short e)
      {
        A = a;
        B = b;
        C = c;
        D = d;
        E = e;
      }

      [FieldOffset(0)]
      public short A;
      [FieldOffset(2)]
      public short B;
      [FieldOffset(4)]
      public short C;
      [FieldOffset(6)]
      public short D;
      [FieldOffset(8)]
      public short E;
    }

    private static T GetValue<T>(T[] arr, int index)
    {
      return arr[index];
    }

    protected override void Run()
    {
      //var xQueue = new Queue<MyStruct>(8);
      //xQueue.Enqueue(new MyStruct(1, 2, 3, 4, 5));
      //xQueue.Enqueue(new MyStruct(6, 7, 8, 9, 10));

      //var xTest = 3 % 8;
      //Console.Write("Test: ");
      //Console.WriteLine(xTest.ToString());

      ////var xItem = xQueue.Dequeue();
      ////Console.Write("Char: ");
      ////Console.WriteLine(xResult.KeyChar);
      //var xItem = new MyStruct
      //            {
      //              A = 1,
      //              B = 2,
      //              C = 3,
      //              D = 4,
      //              E = 5
      //            };

      //var xArray = new MyStruct[1];
      //xArray[0] = xItem;
      ////xArray[0] = new MyStruct(1, 2, 3, 4, 5);

      //xItem = xArray[0];
      //Console.Write("A: ");
      //Console.WriteLine(xItem.A);
      //Console.Write("B: ");
      //Console.WriteLine(xItem.B);
      //Console.Write("C: ");
      //Console.WriteLine(xItem.C);
      //Console.Write("D: ");
      //Console.WriteLine(xItem.D);
      //Console.Write("E: ");
      //Console.WriteLine(xItem.E);

      ////xItem = new MyStruct(6, 7, 8, 9, 10);

      //Console.WriteLine("Next: ");
      ////xItem = xQueue.Dequeue();
      ////Console.Write("Char: ");
      ////Console.WriteLine(xResult.KeyChar);

      ////var xArray = new MyStruct[0];
      ////xArray[0] = new MyStruct(1, 2, 3, 4, 5);

      //xItem = GetValue(xArray, 0);
      //Console.Write("A: ");
      //Console.WriteLine(xItem.A);
      //Console.Write("B: ");
      //Console.WriteLine(xItem.B);
      //Console.Write("C: ");
      //Console.WriteLine(xItem.C);
      //Console.Write("D: ");
      //Console.WriteLine(xItem.D);
      //Console.Write("E: ");
      //Console.WriteLine(xItem.E);


      //while (true)
      //  ;

      //Stop();
      mCount++;
      mDebugger.SendMessage("Kernel", "New iteration");
      Console.WriteLine();
      Console.Write("\n");
      Console.WriteLine("Guess #" + mCount);
      Console.Write("Please enter a guess: ");
      string xInputStr = Console.ReadLine();
      Console.Write("Input length: ");
      Console.WriteLine(xInputStr.Length.ToString());
      int xGuess = int.Parse(xInputStr);
      Console.WriteLine("Your guess was " + xGuess);
      if (xGuess < mMagicNo)
      {
        Console.WriteLine("Too low.");
      }
      else if (xGuess > mMagicNo)
      {
        Console.WriteLine("Too high.");
      }
      else
      {
        Console.WriteLine("You guessed it!");
        Stop();
      }
    }
  }
}
