using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Indy.IL2CPU;
using System.Threading;

namespace Cosmos.Compiler.Builder
{
    public partial class MainWindow : Window, IMainWindow
    {
        // internal static UIEvents UIEvents = new UIEvents();  
        MainWindowController controller;

        public MainWindow()
        {
            InitializeComponent();
            controller = new MainWindowController(this);
            butnBuild.Click += new RoutedEventHandler(butnBuild_Click);
            butnCancel.Click += new RoutedEventHandler(butnCancel_Click);

            Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            controller.Init();
            this.Activate();
        }

        private void butnBuild_Click(object sender, RoutedEventArgs e)
        {
            //MoveFocus TRO CONTROLLER


            //MainWindow.UIEvents.ProceedButtonPressed();
            butnBuild.Visibility = Visibility.Collapsed;
            controller.StartBuild();
        }

        private void butnCancel_Click(object sender, RoutedEventArgs e)
        {
            // MainWindow.UIEvents.StopButtonPressed();
            controller.CancelBuild();
        }










        public void ShowOptions()
        {
            (OptionUC as UserControl).Visibility = Visibility.Visible;
            (BuildProgressUC as UserControl).Visibility = Visibility.Collapsed;

        }

        public void ShowBuildProgress()
        {
            (OptionUC as UserControl).Visibility = Visibility.Collapsed;
            (BuildProgressUC as UserControl).Visibility = Visibility.Visible;

        }

        delegate void AddLog(string param);

        public void AddToLog(string logMsg)
        {

            if (listInformation.Dispatcher.Thread != Thread.CurrentThread)
                listInformation.Dispatcher.Invoke(new AddLog(AddToLog), logMsg);
            else
                listInformation.Items.Add(logMsg);
        }

        public IOptionUC OptionUC
        {
            get { return optionsUC; }
        }

        public IBuildProgressUC BuildProgressUC
        {
            get { return buildUC; }
        }

        public void ThreadedClose()
        {
            Dispatcher.BeginInvoke(
                (Action)delegate()
                {
                    Close();
                });
        }

        private void logExpander_Expanded(object sender, RoutedEventArgs e)
        {

        }


    } //mainWIndow
}
