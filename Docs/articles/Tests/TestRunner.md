# TestRunner

## Description
TestRunner is used to run Kernel Tests using the tests at a specified location and can run each kernel in multiple targets to ensure full compatibility and stability accross these targets. 

TestRunner generates a result file upon completion to detail the results of the tests.

## Get Started
To use the TestRunnner open the `Test.sln` and set the Target Project to `Cosmos.TestRunner`.

## Configuration
Editing the configuration for TestRunner can be done by editing [DefaultEngineConfiguration](https://github.com/CosmosOS/Cosmos/blob/master/Tests/Cosmos.TestRunner.Core/DefaultEngineConfiguration.cs#L6) in ..\Tests\TestRunner\TestRunner.Core
This allows one to choose between running it using VMWare or Bochs.

To select which Test Kernels to run, edit `GetStableKernelTypes()` in `TestKernelSets.cs`.

## Project Location
The TestRunner Projects are located in ..\Tests\TestRunner
