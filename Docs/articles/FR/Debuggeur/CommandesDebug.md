# Commandes de debug

## DS en VS

```cs
Noop = 0;
TracePoint = 1;
Message = 2;
BreakPoint = 3;
Error = 4;
Pointer = 5;
// Ceci est envoyé une fois au démarrage. Le premier appel au DebugStub envoie ceci.
//L'Hôte peux ensuite répondre avec une série de point d'arrêts définis etc (ceux qui ont été définis avant d'être lancé).
Started = 6;
MethodContext = 7;
MemoryData = 8;
// Envoyé après les commandes pour accuser réception en mode batch.
CmdCompleted = 9;
Registers = 10;
Frame = 11;
Stack = 12;
Pong = 13;
BreakPointAsm = 14;
```

## VS en DS

```cs
Noop = 0;

TraceOff = 1; // Peut-être utilisé actuellement.
TraceOn = 2; // Peut-être utilisé actuellement.

Break = 3;
Continue = 4; // Après un point d'arrêt.
BreakOnAddress = 6;

BatchBegin = 7;
BatchEnd = 8;

StepInto = 5;
StepOver = 11;
StepOut = 12;

SendMethodContext = 9; // Envoies les données du stack, relatif au EBP (en x86).
SendMemory = 10;
SendRegisters = 13; // Envoies les données enregistrées au DC.
SendFrame = 14;
SendStack = 15;

    // Met un point d'arrêt d'assembly level.
    // Seulement un à la fois peut être actif. BreakOnAddress peut en avoir plusieurs.
    // L'utilisateur dois appeler continue après.
SetAsmBreak = 16;

Ping = 17;
    // Soyez sure que ç'est la dernière entrée. Utilisée par DebugStub pour vérifier les commandes.
Max = 18;
```

## Channels de debug

Nous supportons les channels, qui sont pré-fixé avec 192 ou plus. 192 est utilisé pour une vue de débug.

###### Traduction par Kiirox