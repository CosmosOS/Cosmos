Plugs are used to fill "holes" in .NET libraries and replace them with different 
 code. Holes exist for example when a method in .NET library uses a Windows API 
 call. That API call will not be available on Cosmos. Emulating the win32 API would be highly inefficient. Instead, 
 Cosmos replaces specific methods and property implementations that rely on 
 win32 API calls. Plugs can also be used to provide an alternate implementation 
 for a method, even if it does not rely on the Windows API.

 Plugs can be implemented in the following manners:

*   Code Plug - Standard C# (or any .NET language) is used to provide the alternate  implementation.
*   Assembly Languge - In a few low level cases assembly language is used. This is  reserved for a few cases and only allowed in the Cosmos.Core ring.
*   X# - Not supported as of yet. Used in place of Assembly language.

 When a Cosmos project is compiled, IL2CPU creates a list of plugs. When it 
 encounters a method or property for which a plug exists, instead of using the IL 
 contained in the existing implementation, it substitutes it with the plug version 
 instead.

### 
 Plug Targets

*   Source Plugs - Used to target a class library without modifiable or available  source.  For example the Console class in the .NET Framework. The Console class uses the  Windows API, so plugs can be used to redefine the methods in the Console class  to use code which interacts with Cosmos instead. For these types of plug  targets, the attributes are specified on the plug implemenation since the source  cannot be modified of the target. If we coud modify the source, we wouldn&#39;t need  plugs. :)
*   Assembly Plug - Empty methods which create a sort of &quot;abstract&quot; class are used  to define an interface to an assembly language plug. This type of plug is rare  and only for the core ring. For these types of plugs, the attributes are  specified on the target, rather than the implementation.

 Non-source targets should never need to use assembly language, but if they do 
 the plugs must be cascaded since assembly language plugs can only be applied to 
 classes with modifiable source.

### 
 Implementing a Plug

 Plugs can be applied to a class, or to an individual method or property. Plug 
 implementations are defined by applying attributes to the class which provides 
 the plug implementation.

### 
 Plug Assembly Naming in Cosmos

 Plugs assemblies in Cosmos are named in the following manner.

 Cosmos.<ring>.Plugs.<Unique section of target namespace>

 ie Cosmos.System.Plugs.System

 Normally only one section will exist after plugs, but mutliple sections could 
 exist to further seperate a group of plugs into multiple assemblies. In the 
 previous example we can determine the following information:

*   **Cosmos**.System.Plugs.System - All Cosmos assemblies start with Cosmos.*.  User assemblies should not use the Cosmos. prefix.
*   Cosmos.**System**.Plugs.System - The plugs are part of the System ring.
*   Cosmos.System.**Plugs**.System - Plugs seperates plugs from normal code with  in the Cosmos.System.*
*   Cosmos.System.Plugs.**System** - The plugs in this assembly will apply to  System.*. It could also be System.Collections for example to apply to  System.Collections.*

 Plugs should be implemented in the highest ring possible and according to the 
 code in the plug, not according to the class to which the plug targets. Most 
 plugs should be in fact in the system ring. Except for a few special plugs 
 needed by the compiler, off hand I cannot think of any that should be 
 implemented in any ring lower than the system ring.

 **Plug Target**

 Please specify the PlugTarget using the fully qualified name, like: 
 global::System.Console. To avoid later name colisions.
