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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cosmos.Build.Windows.Config {
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window {
        public Window1()
        {
            InitializeComponent();

        }

        void q_Status(object sender, Cosmos.Build.Windows.Config.Tasks.TaskStatusEventArgs e)
        {
            EventHandler<Cosmos.Build.Windows.Config.Tasks.TaskStatusEventArgs> del = new EventHandler<Cosmos.Build.Windows.Config.Tasks.TaskStatusEventArgs>(q_Status_Invoke);
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, del, sender, e);
        }

        void q_Status_Invoke(object sender, Cosmos.Build.Windows.Config.Tasks.TaskStatusEventArgs e)
        {
            taskLabel.Content = new Bold(new Run(e.TaskName));
            statusLabel.Content = new Run(e.Message);
            progressBar.Value = e.Percentage;
        }

        private void beginButton_Click(object sender, RoutedEventArgs e)
        {
            beginButton.Visibility = Visibility.Hidden;
            Tasks.TaskQueue q = new Cosmos.Build.Windows.Config.Tasks.TaskQueue();
            q.Add(new Tasks.CleanGacTask());
            q.Add(new Tasks.InstallGacTask());
			q.Add(new Tasks.InstallTemplateTask());
            q.Status += new EventHandler<Cosmos.Build.Windows.Config.Tasks.TaskStatusEventArgs>(q_Status);
            q.BeginExecute();
        }
    }
}
