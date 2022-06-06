# OnBoot
If you need to disable drivers because you are developing your own you may do so by adding the OnBoot method to your kernel, 
right now you can disable 3 drivers and disable one part of a driver, an example would be

```csharp
public override void OnBoot() {
Sys.Global.Init(GetTextScreen(),true,true,true,false);
}
```
in the above example we specify that the mousewheel is enabled, the ps2controller is loaded, network drivers are being loaded and the IDE controller is disabled.
this is helpful if you intend on developing your own IDE controller, the order of the booleans is as stated above:
Mousewheel,
PS2Controller,
Network Drivers,
IDE Controller