using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Lager_automation.Models;

namespace Lager_automation.Views
{
    /// <summary>
    /// Interaction logic for MainMenuView.xaml
    /// </summary>
    public partial class MainMenuView : UserControl
    {
        private readonly MainWindow _main;


        public MainMenuView(MainWindow main)
        {
            InitializeComponent();
            _main = main;
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            RunButton.IsEnabled = false;
            RunButton.Content = "Running...";

            try
            {
                var manager = new Manager();
                manager.BeginRackingProcess();

            }
            finally
            {
                Application.Current.Shutdown();
            }
        }
    }
}
