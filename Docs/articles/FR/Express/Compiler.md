## Compiler Cosmos (OBSELETE)

Pour créer son propre Cosmos en Express 2013, tu auras besoin du suivant:
- Visual Studio Express pour C# ou VB.NET
- Cosmos userkit ou devkit installé
- Visual Studio 2013 Shell (Shell isolé)

Dans Visual Studio Express tu crée un projet de librairie qui contient le code de ton OS et un projet pour le compiler.

Lances Visual Studio Express 2013 et crée un projet appelé "Cosmos C# Library". C'est la partie de ton OS pour ajouter du code. Maintenant compile le projet pour créer le DLL pour l'utiliser après, après ça sauvegarde le.
Copie maintenant la librairie créé avec le nom de ton projet dans le dossier du build du projet dans bin/Debug/ c'est nécessaire sinon la référence ajoutée dans la prochaine étape ne seras pas trouvée. Ajoute maintenant la référence au build du projet et essaye de build. Quand tu reçois un warning qui dit que l'assembly ajoutée n'est pas trouvée, tu reçois "No Kernel Found!".

###### Traduction par Kiirox