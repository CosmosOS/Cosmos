using System;
using Cosmos.Compiler.Builder;
using Cosmos.Hardware;
using Cosmos.Playground.Xenni.TxUI;

namespace Cosmos.Playground.Xenni
{
    public class TxUIDemo
    {
        TxUIManager uiman = new TxUIManager(VGAScreen.TextSize.Size80x25);
        TxLabl lblCosmos = new TxLabl();
        TxLabl lbl1 = new TxLabl();
        TxBtn btn1 = new TxBtn();
        TxEditbox ed1 = new TxEditbox();

        public void btn1_OnActivate()
        {
            if (lbl1.Text.EndsWith("A "))
            {
                lbl1.Text = " Test Label - B ";
            }
            else
            {
                lbl1.Text = " Test Label - A ";
            }

            lbl1.Draw();
            uiman.Present();
        }

        public void Run()
        {
            TxUIManager.Instance = uiman;

            uiman.OveridePresent = true;
            lblCosmos.BackColor = ConsoleColor.DarkBlue;
            lblCosmos.ForeColor = ConsoleColor.White;
            lblCosmos.Text = "Cosmos OS - User Interface Test";
            lblCosmos.X = 0;
            lblCosmos.Y = 0;
            lblCosmos.Width = 80;
            lblCosmos.Height = 1;
            uiman.RegisterControl(lblCosmos);

            lbl1.BackColor = ConsoleColor.DarkGreen;
            lbl1.ForeColor = ConsoleColor.Cyan;
            lbl1.Text = " Test Label - A ";
            lbl1.X = 7;
            lbl1.Y = 2;
            lbl1.Width = 16;
            lbl1.Height = 1;
            uiman.RegisterControl(lbl1);

            btn1.FBackColor = ConsoleColor.DarkBlue;
            btn1.FForeColor = ConsoleColor.White;
            btn1.UBackColor = ConsoleColor.Gray;
            btn1.UForeColor = ConsoleColor.White;
            btn1.PBackColor = ConsoleColor.Black;
            btn1.PForeColor = ConsoleColor.White;
            btn1.Text = " Test Button ";
            btn1.X = 2;
            btn1.Y = 4;
            btn1.Width = 13;
            btn1.Height = 3;
            btn1.OnActivate = btn1_OnActivate;
            uiman.RegisterControl(btn1);

            ed1.FBackColor = ConsoleColor.DarkBlue;
            ed1.FForeColor = ConsoleColor.White;
            ed1.UBackColor = ConsoleColor.Gray;
            ed1.UForeColor = ConsoleColor.Black;
            ed1.X = 1;
            ed1.Y = 9;
            ed1.Width = 30;
            ed1.Height = 3;
            uiman.RegisterControl(ed1);

            uiman.OveridePresent = false;

            uiman.DrawAll();
            uiman.Present();

            uiman.Run();
        }
    }

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            BuildUI.Run();
        }

        // Main entry point of the kernel
        public static void Init()
        {
            new Cosmos.Sys.Boot().Execute();

            TxUIDemo demo = new TxUIDemo();
            demo.Run();
        }
    }
}