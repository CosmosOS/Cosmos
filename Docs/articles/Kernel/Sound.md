# Network

In this article we will discuss about Sound on Cosmos, How to play sound, Detect sound cards and stop all playing sounds. For now, supported sound cards are **Sound blaster 16**. Note that Cosmos devkit must be installed for this article.

All sound cards here don't necessary support every feature described by their RFC and may have some bugs or architecture issues, if you find bugs or something abnormal please [submit an issue](http://https://github.com/CosmosOS/Cosmos/issues/new/choose "repository") on our repository. 

Cosmos Sound won't use Classes and Functions that are under .NET Core. Everything described here will be under:
```csharp
using Cosmos.HAL;
```
### Playing Sound
Right now, Cosmos only supports playing 8-Bit unsigned PCM at 16 KHz.
To play sound, you can use the ```AudioManager``` Class.
```csharp
byte[] MyAudioFile = { .... };
AudioManager.PlayAudio(MyAudioFile);
```

### Detecting which sound cards are installed
You can detect which sound cards are installed using again, the AudioManager class
```csharp
foreach(var xSoundCard in AudioManager.devices)
{
    Console.Writeline(xSoundCard.Name);
}
```

### Stop the playing sound
You can stop playing the currently playing sound by using the AudioManager.StopAllSound() method
```csharp
AudioManager.StopAllSound();
```
