### IL2CPU

C'est l'un des morceaux de code les plus importants de Cosmos. C'est un 
compilateur AOT (Ahead-Of-Time).

Lorsque vous compilez votre programme C# (ou n'importe quel langage .NET), il est compilé dans l'IL (intermediate language). L'IL est ensuite interprété et exécuté par une machine virtuelle lorsque
tu ouvres ton exe.

Cosmos est écrit en C# et Visual Studio le compile en IL comme toujours. Mais un PC n'est pas livré avec un interpréteur pour le code IL. Et écrire une machine virtuelle pour exécuter un système d'exploitation n'est pas toujours idéal.

C'est là qu'IL2CPU entre en jeu. IL2CPU prend le code IL et le traduit en opcodes de processeur. Actuellement, seuls les opcodes x86 sont disponibles pour le moment. Cependant, d'autres architectures sont prévues pour le futur (ARM, PowerPC, x86-64).

À ce stade, IL2CPU effectue un peu plus de magie avant de finalement convertir le fichier entier en un fichier binaire bootable, qui peut être chargé par un chargeur de démarrage sur n'importe quel système (Cosmos utilise Syslinux).

Comme vous pourriez le penser, IL2CPU est un élément fondamental du développement
de Cosmos. IL2CPU est responsable de la sortie finale, c'est pourquoi la plupart des optimisations ajoutées concernent IL2CPU.

###### Traduction par Kiirox