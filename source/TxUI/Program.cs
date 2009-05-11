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

        public void btn1_OnActivate()
        {
            if (lbl1.Text.EndsWith("A"))
            {
                lbl1.Text = "Test Label - B";
            }
            else
            {
                lbl1.Text = "Test Label - A";
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
            lblCosmos.Width = 31;
            lblCosmos.Height = 1;
            uiman.RegisterControl(lblCosmos);

            lbl1.BackColor = ConsoleColor.DarkGreen;
            lbl1.ForeColor = ConsoleColor.Cyan;
            lbl1.Text = "Test Label - A";
            lbl1.X = 5;
            lbl1.Y = 2;
            lbl1.Width = 14;
            lbl1.Height = 1;
            uiman.RegisterControl(lbl1);

            btn1.FBackColor = ConsoleColor.Gray;
            btn1.FForeColor = ConsoleColor.White;
            btn1.UBackColor = ConsoleColor.DarkGray;
            btn1.UForeColor = ConsoleColor.White;
            btn1.PBackColor = ConsoleColor.White;
            btn1.PForeColor = ConsoleColor.Black;
            btn1.Text = "Test Button";
            btn1.X = 2;
            btn1.Y = 4;
            btn1.Width = 11;
            btn1.Height = 1;
            btn1.OnActivate = btn1_OnActivate;
            uiman.RegisterControl(btn1);

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

            PIT.T0RateGen = true;
            PIT.T0DelyNS = 1000000;

            Console.WriteLine("Timer Test");
            PIT.Wait(500);
            Console.WriteLine("500");
            PIT.Wait(1000);
            Console.WriteLine("1000");
            Console.Read();

            TxUIDemo demo = new TxUIDemo();
            demo.Run();
        }
    }
}