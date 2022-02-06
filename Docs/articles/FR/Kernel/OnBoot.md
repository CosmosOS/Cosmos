# OnBoot
Si vous avez besoin de désactiver des pilotes parce que vous développez les vôtres, vous pouvez le faire en ajoutant la méthode OnBoot à votre noyau,
en ce moment, vous pouvez désactiver 3 pilotes et désactiver une partie d'un pilote, un exemple serait


```csharp
public override void OnBoot() {
Sys.Global.Init(GetTextScreen(),true,true,true,false);
}
```
dans l'exemple ci-dessus, nous spécifions que la molette de la souris est activée, le ps2controller est chargé, les pilotes réseau sont en cours de chargement et le contrôleur IDE est désactivé.
ceci est utile si vous avez l'intention de développer votre propre contrôleur IDE, l'ordre des booléens est comme indiqué ci-dessus :
Mousewheel,
PS2Controller,
Network Drivers,
IDE Controller

###### Traduction par Kiirox