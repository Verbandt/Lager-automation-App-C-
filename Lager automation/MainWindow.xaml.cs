using Lager_automation.Models;
using Lager_automation.Views;
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

namespace Lager_automation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Start with the main menu
            MainContent.Content = new MainMenuView(this);
        }

        public void ShowMainMenu()
        {
            MainContent.Content = new MainMenuView(this);
        }

        
    }
}