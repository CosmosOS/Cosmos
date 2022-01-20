# Répertoires

À première vue, la structure des répertoires de Cosmos est assez déroutante. Il y a des dossiers avec les mêmes noms et ainsi de suite. Je vais essayer de vous montrer comment les répertoires sont structurés, afin que vous puissiez trouver ce que vous cherchez.

### \Artwork

Ce répertoire contient les logos Cosmos et un logo de chaîne YouTube.

### \Build

Ce répertoire contient les fichiers utilisé pour créer des images ISO et des fichiers et images de machines virtuelles, il existe un dossier avec des fichiers de support pour BOCHS, pour Virtual PC, pour VMware, QEMU et pour le démarrage à partir d'un CD ISO, USB, Ethernet (PXE).

### \Demos

Ce répertoire contient les projets de démonstration Cosmos pour vous aider à programmer votre système d'exploitation.

#### \Demos\Guess

Ce répertoire contient le projet de démonstration Cosmos de base, c'est un simple jeu de devinettes.

### \Docs

Ce répertoire contient la documentation Cosmos.

### \Docs\FR

Ce répertoire contient la documentation Cosmos que vous lisez actuellement.

### \QA

Ce répertoire contient les anciens scripts de construction de Cosmos, il n'est plus utilisé pour le moment.

### \Resources

Ce répertoire contient les bibliothèques tierces utilisées par Cosmos.

### \Setup

Ce répertoire contient tous les scripts et langages pour créer le programme d'installation du kit utilisateur Cosmos. La création (et l'exécution) du programme d'installation est lancée à partir de '.\install-VS2019.bat'.

### \Source

Ce répertoire contient tout le code du projet Cosmos. Vous passerez la plupart de votre temps ici, y compris le compilateur, l'installateur, le débogueur et les principales fonctionnalités de Cosmos. Il contient du code inutilisé, le fichier de solution de Cosmos et certaines bibliothèques de support.

#### \Source\Cosmos.Assembler

Ce répertoire contient l'assembleur Cosmos, il qui écrit le code d'assemblage généré dans le fichier du noyau.

#### \Source\Cosmos.Build

Ce répertoire contient du code lié au processus de construction de Cosmos à l'exception de
IL2CPU qui est cependant exécuté à partir d'ici, il contient du code pour exécuter ld,
nasm, makeiso, MSBuild et autres.

#### \Source\Cosmos.Common

Ce répertoire contient divers assistants et fichiers pour Cosmos.

#### \Source\Cosmos.Core

Ce répertoire contient l'assembly de base de cosmos. Il contient du code pour gérer le CPU, les IO groups, les interruptions, etc.

##### \Source\Cosmos.Core.Plugs

Ce répertoire contient les plugs de bas niveau pour Cosmos.Core.

#### \Source\Cosmos.Debug

Ce répertoire contient le code du moteur de débogage et du connecteur et donne également la possibilité de
travailler avec GDB. Pour plus d'informations sur le débogage, consultez la page [Debugger](Debuggeur/CommandesDebug.md)

#### \Source\Cosmos.Deploy

Ce répertoire contient le code de déploiement.

#### \Source\Cosmos.HAL

Ce répertoire contient le Cosmos HAL (Hardware Abstraction Layer), c'est-à-dire les pilotes matériels pour les graphiques, la mise en réseau, le disque dur, etc.

#### \Source\Cosmos.IL2CPU

Ce répertoire contient le code du programme IL2CPU, le compilateur Cosmos AOT. Pour plus
informations sur les compilateurs AOT et IL2CPU, voir la documentation [IL2CPU](https://github.com/CosmosOS/Cosmos/blob/master/Docs/FR/Compiler/il2cpu.md).

#### \Source\Cosmos.System

Ce répertoire contient le code au niveau du système pour Cosmos. Il contient des wrappers pour la console, la mise en réseau, le système de fichiers et contient également la classe de base pour le noyau.

#### \Source\Cosmos.VS

Ce répertoire contient le code pour l'intégration avec Visual Studio. Ajoute la prise en charge du type de projet Cosmos Kernel et des étapes de génération personnalisées.

##### \Source\Cosmos.VS.Debug

Ce répertoire contient le code pour l'intégration du debug Visual Studio.

##### \Source\Cosmos.VS.ProjectSystem

Ce répertoire contient le code du package Visual Studio de Cosmos.

##### \Source\Cosmos.VS.Windows

Ce répertoire contient les fenêtres Cosmos dans Visual Studio, comme la fenêtre Registres.

##### \Source\Cosmos.VS.Windows.Test

Ce répertoire contient les tests pour Cosmos.VS.Windows.

##### \Source\Cosmos.VS.Wizards

Ce répertoire contient les projets d'assistants de Cosmos.

#### \Source\Unused

Ce répertoire contient des fonctionnalités en cours ou obsolètes, telles que FAT, VGA, mise en réseau, etc.

#### \Source\XSharp

Ce répertoire contient du code permettant d'écrire Assembly en C#. De cette façon, nous pouvons garder le tout dans un style OO.

#### \Users

Ce répertoire contient le terrain de jeu de code personnalisé. Vous pouvez mettre vos exemples, votre code aléatoire et vos idées
ici

###### Traduction par Kiirox