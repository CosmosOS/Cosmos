For creating your own Cosmos in Express 2013 you need the following:

 * Visual Studio Express for C# or VB.NET
 * installed Cosmos Kit(User Kit or compiled from checkout (Dev Kit))
 * Visual Studio 2013 Shell (Isolated Shell)

In Visual Studio Express you create a library project, that contain your OS
code, and a project for compiling your OS.

 1. Start Visual Studio Express 2013 and create a project named "Cosmos C# Library". This is the part of your OS for adding Code. Compile this project now for creating the DLL for later use. After that save it.
 2. (UNTESTED) Start your Microsoft Visual Studio 2013 Shell and create a Project "Cosmos Project (Empty)". It contains a custom Project that use MSBuild with many tasks to create your OS. Save the project.
 3. Copy now the created library with name of your project in the folder of the build project under bin/Debug/ Thats needed else would the added reference in next step not found.
 4. Add now the reference to the build project and try to build. When you get a warning that the added assembly is not found, you get "No Kernel Found!".
