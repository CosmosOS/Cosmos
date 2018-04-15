namespace CosmosKernel

open System

type Kernel() =
   inherit Cosmos.System.Kernel()
   override u.BeforeRun() = (Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");)
   override u.Run() =
      Console.Write("Input: ");
      let input = Console.ReadLine();
      Console.Write("Text typed: ");
      Console.WriteLine(input);
