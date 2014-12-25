

There is some basic hand coded assembly which is the first entry point of
Cosmos. From there the first C# entry point is Cosmos.System.Kernel.Start().

Cosmos.System.Kernel is an abstract class that each project creates one
descendant of to create the operating system.

