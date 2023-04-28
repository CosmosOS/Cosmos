# Plugs

Plugs are used to fill "holes" in .NET libraries and replace them with different 
 code. Holes exist for example when a method in .NET library uses a Windows API 
 call. That API call will not be available on Cosmos. Emulating the win32 API would be highly inefficient. Instead, 
 Cosmos replaces specific methods and property implementations that rely on 
 win32 API calls. Plugs can also be used to provide an alternate implementation 
 for a method, even if it does not rely on the Windows API.
 
 > **Important: All plugs must go in a seperate project, which is included in your original project using the `PlugReference` attribute in your kernels csproj.**

## Types of plugs

There are two types of plugs used and supported by Cosmos:

*   Code Plug - A standard C# (or any .NET language) method is used to provide the alternate implementation.
*   X#/Assembly Languge - In a few cases, it is difficult or impossible to write the code using C#/.Net since one needs exact control over the emitted assembly code. An assembly plug are designed for that case. Cosmos itself only uses this type of plug within the Cosmos.Core projects.

## How do plugs work

To explain  how plugs work, we first need to give an overview of how IL2CPU works. Roughly, IL2CPU compiles a kernel using the following steps: 
1. IL2CPU determines a list of all methods and types which are used by the kernel.
2. It then compiles each of these methods into assembly code. 
This is usually done by getting the list of IL instructions which make up the method and translating each of them into some corresponding assembly.
3. Together with a bit of boilerplate code, the emitted assembly for all the methods is compiled using yasm. 

Plugs change what happens in step 2. A normal code plug means that rather taking the IL instructions from the original method, the IL instructions from the plug are used and then converted into assembly. An assembly plug directly states what asm should be emitted. 

# Implementing a Plug

While one always plugs individual methods, plugs are defined class wise. Therefore the first step to plugging any method is to define a new static class to contain all the pluggged methods for some certain type. This class must be decorated with the `Plug` attribute. The plug attribute either takes the type it is plugging(`Plug(Type target)`) or a string with the target name(`Plug(string targetName)`). Using the string target name is required when plugging internal or private classes. An example for a plugged class is for the Math class. https://github.com/CosmosOS/Cosmos/blob/8a8393353f1957890c5154650e29847fd22bf893/source/Cosmos.System2_Plugs/System/MathImpl.cs#L8-L9

## Code Plug

Once you have created such a class, you can add methods to the class. If these methods share the signature with a method in the original class they will be used to plug the original methods. For example in the above mentioned Math plug class, the following method plugs the original `double Math.Abs(double)` implementation.

https://github.com/CosmosOS/Cosmos/blob/8a8393353f1957890c5154650e29847fd22bf893/source/Cosmos.System2_Plugs/System/MathImpl.cs#L52-L63

Note, when plugging a non-static method, the first argument will be correspond to "this" (the instance for which the method is being called). 

Sometimes it is impossible to define a method with exactly the same signature due to some of the arguments being from private or internal classes. In that case you can use the `PlugMethod` attribute. An example, is the following plug for `GC.AllocateNewArray` since `ALLOC_FLAGS` is a private enum. 
https://github.com/CosmosOS/Cosmos/blob/8a8393353f1957890c5154650e29847fd22bf893/source/Cosmos.Core_Plugs/System/GCImpl.cs#L17-L29

## Assembly Plug

Defining an assembly plug is slightly more complicated. The first step is the same and one needs to define a method with the same signature as the method one needs to plug. This acts as plug placeholder. The actual plug implemenation is a new class inheriting from `AssemblerMethod`. This class needs to override the `void AssembleNew(Assembler aAssembler, object aMethodInfo)` method. The `AssembleNew` method will be called when IL2CPU is executing and should emit the required asm. Examples of such classes can be found [here](https://github.com/CosmosOS/Cosmos/blob/master/source/Cosmos.Core_Plugs/MathImpl.cs) including the plug implementation for `double Math.Round(double)`,

https://github.com/CosmosOS/Cosmos/blob/8a8393353f1957890c5154650e29847fd22bf893/source/Cosmos.Core_Plugs/MathImpl.cs#L40-L49

The final step is to link the plug implementation to the plug placeholder by adding a `PlugMethod(Type Assembler)` to the plug placeholder, where the `Assembler` value is the class you created with the implementation. An example is the plug placeholder for the above mentioned `double Math.Round(double)`,

https://github.com/CosmosOS/Cosmos/blob/8a8393353f1957890c5154650e29847fd22bf893/source/Cosmos.Core_Plugs/MathImpl.cs#L15-L19

## Using plugs to write assembly in your kernel

While plugs are usually used to overwrite existing methods in the .Net runtime, they can also be used to include assembly methods in your kernel. 
This is for example done to implement the `void CPU.UpdateIDT(bool)` method in Cosmos. 
To do this for your own classes and methods is not more difficult than plugging any other method. Simply set target of the plug class to your own class and write the assembly plug as usual. As a reference you can look at [Cosmos.Core/CPU.cs](https://github.com/CosmosOS/Cosmos/blob/master/source/Cosmos.Core/CPU.cs), [Cosmos.Core_Asm/CPUImpl.cs](https://github.com/CosmosOS/Cosmos/blob/master/source/Cosmos.Core_Asm/CPUImpl.cs) and [CPUUpdateIDTAsm.cs]( https://github.com/CosmosOS/Cosmos/blob/master/source/Cosmos.Core_Asm/CPU/CPUUpdateIDTAsm.cs).
