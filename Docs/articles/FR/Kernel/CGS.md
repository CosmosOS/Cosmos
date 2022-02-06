# Introduction
Le module graphique de Cosmos (Cosmos Graphic Subsystem ou CGS) est basé sur l'abstraction du Canvas est un espace vide où l'utilisateur peut afficher son contenu. CGS n'est pas un widget toolkit comme Winforms ou Gnome / GTK mais plus pour du bas niveau dans lequel les widget toolkit seront implémentés. CGS caches le pilote graphique (avec VGA, VBE et SVGAII) utilisé et il est pensé de manière universelle d'afficher sur l'écran dans Cosmos.

Bien que Canvas peut être overwritten (par exemple pour créer des sous-fenêtres), l'utilisateur du CGS n'a pas a s'en occuper directement mais il doit utiliser la classe statique `FullScreenCanvas`.

Regardons les méthodes de l'API.
# FullScreenCanvas

    public static Canvas GetFullScreenCanvas(Mode mode) obtient l'instance de Canvas représentant l'écran complet (en fait l'instance du pilote VGA en cours d'exécution) en utilisant le mode spécifié
    public static Canvas GetFullScreenCanvas() pareil mais en utilisant le mode préféré du pilote VGA

Pour vraiment afficher sur l'écran nous avons besoin d'utiliser la classe Canvas. Regardons un peu l'API:
# Canvas
## Liste des propriétés de la classe Canvas

    Mode: get / set (Prendre le mode de la carte video ou mettre le mode a celui choisi.) Il jette si le mode sélectionné n'est pas pris en charge par la carte vidéo
    DefaultGraphicMode: mode graphique par défaut, cela changera en fonction du matériel sous-jacent
    AvailableModes: liste des modes disponibles pris en charge, cela changera en fonction du matériel sous-jacent

## Liste des méthodes de la classe canvas

    Clear(Color color: black) effacer tout le Canvas en utilisant la couleur spécifiée comme arrière-plan
    void DrawPoint(Pen pen, int x, int y) dessine un point aux coordonnées spécifiées par x et y avec le pen spécifié
    void DrawLine(Pen pen, int x_start, int y_start, int x_end, int y_end) trace une ligne aux coordonnées spécifiées par x_start, y_start et x_end, y_end avec le pen spécifié
    void DrawRectangle(Pen pen, int x_start, int y_start, int width, int height) dessine un rectangle spécifié par une paire de coordonnées, une largeur et une hauteur avec le pen spécifié
    void DrawImage(Image image, int x, int y) dessine une image aux x et y spécifiés (Seulement les images en .bmp converties en base64 fonctionnent)
    void DrawString(String string, Font font, Brush brush, int x, int y) dessine une chaîne avec la police et le brush spécifiés aux coordonnées x et y spécifiées
    void Display() uniquement pour le double buffering, permute les 2 buffers puis affiche tout à l'écran

# Un exemple
```CSharp
using System;
using Sys = Cosmos.System;
using Cosmos.System.Graphics;

namespace GraphicTest
{
    public class Kernel : Sys.Kernel
    {
        Canvas canvas;

        private readonly Bitmap bitmap = new Bitmap(10, 10,
                new byte[] { 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0,
                    255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255,
                    0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255,
                    0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 23, 59, 88, 255,
                    23, 59, 88, 255, 0, 255, 243, 255, 0, 255, 243, 255, 23, 59, 88, 255, 23, 59, 88, 255, 0, 255, 243, 255, 0,
                    255, 243, 255, 0, 255, 243, 255, 23, 59, 88, 255, 153, 57, 12, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255,
                    243, 255, 0, 255, 243, 255, 153, 57, 12, 255, 23, 59, 88, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243,
                    255, 0, 255, 243, 255, 0, 255, 243, 255, 72, 72, 72, 255, 72, 72, 72, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0,
                    255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 72, 72,
                    72, 255, 72, 72, 72, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255,
                    10, 66, 148, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255,
                    243, 255, 10, 66, 148, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 10, 66, 148, 255, 10, 66, 148, 255,
                    10, 66, 148, 255, 10, 66, 148, 255, 10, 66, 148, 255, 10, 66, 148, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255,
                    243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 10, 66, 148, 255, 10, 66, 148, 255, 10, 66, 148, 255, 10, 66, 148,
                    255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255,
                    0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, }, ColorDepth.ColorDepth32);

        protected override void BeforeRun()
        {
            // Si tout fonctionne vous ne devrez pas le voir :D
            Console.WriteLine("Cosmos booted successfully. Let's go in Graphical Mode");

            // Vous n'avez pas besoin de spécifier le Mode, mais ici nous le faisons pour vous montrer qu'on le peut.
            canvas = FullScreenCanvas.GetFullScreenCanvas(new Mode(640, 480, ColorDepth.ColorDepth32));
            canvas.Clear(Color.Blue);
        }

        protected override void Run()
        {
            try
            {
                Pen pen = new Pen(Color.Red);

                // Un point rouge
                canvas.DrawPoint(pen, 69, 69);

                // Une ligne horizontale vert jaune
                pen.Color = Color.GreenYellow;
                canvas.DrawLine(pen, 250, 100, 400, 100);

                // An IndianRed vertical line
                // Une ligne verticale rouge indien
                pen.Color = Color.IndianRed;
                canvas.DrawLine(pen, 350, 150, 350, 250);

                // A MintCream diagonal line
                // une ligne menthe crème en diagonale
                pen.Color = Color.MintCream;
                canvas.DrawLine(pen, 250, 150, 400, 250);

                // Un rectangle rouge violet pâle
                pen.Color = Color.PaleVioletRed;
                canvas.DrawRectangle(pen, 350, 350, 80, 60);

                // Un rectangle vert citron
                pen.Color = Color.LimeGreen;
                canvas.DrawRectangle(pen, 450, 450, 80, 60);

                // Une bitmap
                canvas.DrawImage(bitmap, new Point(100, 150));

                Console.ReadKey();
                Sys.Power.Shutdown();
            }
            catch (Exception e)
            {
                mDebugger.Send("Exception occurred: " + e.Message);
                Sys.Power.Shutdown();
            }
        }
    }
}
```
# Limites de l'implémentation actuelle

1. Seule la profondeur de couleur de 32 bits est réellement prise en charge, l'API fournit des méthodes pour définir une résolution avec 24, 16, 8 et 4 bits mais le pilote Bochs de bas niveau ne les a pas encore implémentées.
   
2. De plus, d'autres choses intéressantes pourraient être implémentées:
    - Fonctions Plugging System.Drawing pour une manipulation plus facile des couleurs
    - Une stratégie de double tampon, pour rendre le dessin plus rapide (un est déjà implémenté dans le pilote VBE, mais pas VGA ou SVGA II)
  
3. CGS interagit mal avec la méthode Kernel.Stop : l'écran se fige sans afficher le moindre message d'erreur. Vous devez utiliser la fonction Sys.Power.Shutdown() pour éteindre correctement votre ordinateur.

###### Traduction par Kiirox