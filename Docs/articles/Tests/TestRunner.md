# TestRunner

## Description
TestRunner is used to run Kernel Tests using the tests at a specified location and can run each kernel in multiple targets to ensure full compatibility and stability accross these targets. 

TestRunner generates a result file upon completion to detail the results of the tests.

## Get Started
To use the TestRunnner open the `Test.sln`, set the Target Project to `Cosmos.TestRunner` and run the solution.

## Configuration
Editing the configuration for TestRunner via [DefaultEngineConfiguration](https://github.com/CosmosOS/Cosmos/blob/master/Tests/Cosmos.TestRunner.Core/DefaultEngineConfiguration.cs#L6) in ..\Tests\TestRunner\TestRunner.Core
This allows one to choose which emulator to use and at what level the kernel is debugged.

To select which Test Kernels to run, edit `GetStableKernelTypes()` in `TestKernelSets.cs`.

### Adding a Kernel
To add a kernel to the TestRunner, open the TestKernelSets file and add a line to GetStableKernelTypes() like this line:

`yield return typeof(Cosmos.Kernel.Tests.Fat.Kernel);`

Note: This can be used to test your own kernel for debug purposes. Simply add a reference to your kernel csproj to Cosmos.TestRunner.Full and add it to TestKernelSets.

### Time Out
To set the period before the TestRunner registers an error, change the line: `engine.AllowedSecondsInKernel = 1800;` This means that, in this case, if the Kernel runs for more that 30 minutes then it will register a failure.

## Project Location
The TestRunner Projects are located in ..\Tests\TestRunner
